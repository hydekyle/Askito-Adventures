using System;
using System.Collections.Generic;
using UnityEngine;
using Assets.FantasyHeroes.Scripts;
using EZObjectPools;
using XInputDotNetPure;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Dictionary<int, Entity> enemiesRef = new Dictionary<int, Entity>();
    public Enemy[] enemies;

    public Player player;

    public GameObject playerGO;
    [HideInInspector]
    public Transform playerTransform;

    public static LayerMask enemyLayerMask;
    public static LayerMask ghostLayerMask;
    public static LayerMask breakableLayerMask;

    public Stats cheatStats;

    public GameObject enemyPrefab;

    public GameObject breakingBarrelPrefab;

    public Transform mapEnemies;
    public Transform mapBreakables;
    public GameObject bombPrefab;
    public GameObject shootPrefab;

    EZObjectPool bulletPool;
    EZObjectPool enemyPool;

    GamePadState state;
    GamePadState prevState;

    int maxEnemies = 10;
    int enemyCounterID = 0;

    private void Awake()
    {
        if (Instance != null) Destroy(Instance.gameObject);
        Instance = this;
        Inicialize();
    }

    private void Inicialize()
    {
        enemies = new Enemy[maxEnemies];

        bulletPool = EZObjectPool.CreateObjectPool(shootPrefab, "Shoot1", 20, true, true, true);
        enemyPool = EZObjectPool.CreateObjectPool(enemyPrefab, "Enemies", 2, true, true, true);

        FixMapSpriteOrders();
        AddDefaultPlayer("Player");
        GenerateMapEnemiesRefs();
    }

    private void Update()
    {
        Controls();
        player.Update();
        foreach (Entity enemy in enemiesRef.Values) enemy.Update();
    }

    private int GetNextEnemyID()
    {
        if (enemyCounterID + 1 >= enemies.Length) enemyCounterID = 0;
        else enemyCounterID++;

        if (enemiesRef.TryGetValue(enemyCounterID, out Entity storedEntity))
        {
            Debug.LogWarningFormat("La entidad {0} aún existe", storedEntity.name);
            storedEntity.Burst(Vector2.zero);
        }

        return enemyCounterID;
    }

    public void SpawnEnemyRandom()
    {
        Vector3 randomPos = new Vector3(UnityEngine.Random.Range(-2f, 2f), UnityEngine.Random.Range(-2f, 2f), 0);
        if (enemyPool.TryGetNextObject(player.transform.position + randomPos, Quaternion.identity, out GameObject go))
        {
            int enemyID = GetNextEnemyID();
            string enemyName = "Enemy " + enemyID;

            Enemy enemy = new Enemy(
                go.transform,
                new Stats() { life = 1, strength = 1, velocity = 1 },
                enemyName,
                enemyID
            );

            go.name = enemyName;
            enemy.Start();
            AddEnemyRefs(enemyID, enemy);
        }
    }

    private void GenerateMapEnemiesRefs()
    {
        foreach (Transform enemyT in mapEnemies)
        {
            int enemyID = GetNextEnemyID();
            string enemyName = enemyT.name.Split(' ')[0] + " " + enemyID;
            Enemy enemy = new Enemy(
                enemyT,
                new Stats() { life = 1, strength = 1, velocity = 1 },
                enemyName,
                enemyID
            );
            enemyT.name = enemyName;
            AddEnemyRefs(enemyID, enemy);
            enemy.Start();
        }
    }

    public void AddEnemyRefs(int enemyID, Enemy enemy)
    {
        enemiesRef.Add(enemyID, enemy);
        enemies[enemyID] = enemy;
    }

    public void DeleteEnemyRefs(int enemyID)
    {
        enemiesRef.Remove(enemyID);
        enemies[enemyID] = null;
    }

    private void FixMapSpriteOrders()
    {
        foreach (Transform breakablesT in mapBreakables)
        {
            SpriteRenderer renderer = breakablesT.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = renderer.sortingOrder - (int)(breakablesT.position.y * 100);
        }
    }

    private void SetCheatStats()
    {
        player.stats = cheatStats;
    }

    private void AddDefaultPlayer(string playerName)
    {
        playerTransform = Instantiate(playerGO, Vector3.zero, transform.rotation).transform;

        Player newPlayer = new Player(
            playerTransform,
            new Stats() { life = 1, strength = 1, velocity = 2 },
            playerName
        );
        player = newPlayer;
        playerTransform.name = playerName;
        //entities.Add(player);
    }

    private void RandomEnemyAttack()
    {
        bool pinga = UnityEngine.Random.Range(0, 2) > 0;
        foreach (var entity in enemiesRef.Values)
        {
            entity.PlayAnim(pinga ? "Attack" : "Die");
        }
    }

    private void RestartScene()
    {
        Scene thisScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(thisScene.name);
    }

    public void ShootWeapon(Vector2 sPosition, Vector2 sDirection)
    {
        if (bulletPool.TryGetNextObject(sPosition, transform.rotation, out GameObject go))
        {
            go.SetActive(true);
            go.GetComponent<Rigidbody2D>()?.AddForce(sDirection.normalized * Time.deltaTime / 2, ForceMode2D.Impulse);
        }
    }

    public static void BreakBreakable(Transform breakableT, Vector2 hitDir)
    {
        if (breakableT.name.Split(' ')[0] == "Barrel")
        {
            int breakableSortingOrder = breakableT.GetComponent<SpriteRenderer>().sortingOrder;
            var go = GameObject.Instantiate(GameManager.Instance.breakingBarrelPrefab);
            go.transform.position = breakableT.position;
            GameObject.Destroy(breakableT.gameObject);
            go.gameObject.SetActive(true);
            Transform pieceTop = go.transform.GetChild(0);
            Transform pieceBot = go.transform.GetChild(1);
            pieceTop.GetComponent<SpriteRenderer>().sortingOrder = pieceBot.GetComponent<SpriteRenderer>().sortingOrder = breakableSortingOrder;
            pieceTop.GetComponent<Rigidbody2D>().AddForce((hitDir * 8 + Vector2.up * 6) * 40, ForceMode2D.Force);
            pieceBot.GetComponent<Rigidbody2D>().AddForce((hitDir * 5 + Vector2.up) * 30, ForceMode2D.Force);
            GameObject.Destroy(go, 3f);
        }
        else
        {
            Destroy(breakableT.gameObject);
        }
    }

    IEnumerator<WaitForSeconds> ReciclateEntity(Entity entity)
    {
        DeleteEnemyRefs(entity.ID);
        yield return new WaitForSeconds(2.5f);
        entity.transform.position -= Vector3.right * 100; // Evitando bug de rotación en la animación de forma poco elegante
        entity.PlayAnim("Attack");
        yield return new WaitForSeconds(0.1f);
        entity.headT.gameObject.SetActive(true);
        entity.armRightT.gameObject.SetActive(true);
        entity.armLeftT.gameObject.SetActive(true);
        entity.legRightT.gameObject.SetActive(true);
        entity.legLeftT.gameObject.SetActive(true);
        entity.transform.gameObject.SetActive(false);

        entity.transform.gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    public void RemoveEntity(Entity entity)
    {
        if (entity.GetType() == typeof(Enemy))
        {
            StartCoroutine(ReciclateEntity(entity));
        }
        else Debug.Log("¡Se muere el player!");
    }

    public static string GetAnimationName(string clipName, WeaponType weaponType)
    {
        switch (clipName)
        {
            case "Alert":
                switch (weaponType)
                {
                    case WeaponType.Melee1H:
                    case WeaponType.MeleeTween:
                    case WeaponType.Bow:
                        return "Alert1H";
                    case WeaponType.Melee2H:
                        return "Alert2H";
                    default:
                        throw new NotImplementedException();
                }
            case "Attack":
                switch (weaponType)
                {
                    case WeaponType.Melee1H:
                        return "Attack1H";
                    case WeaponType.Melee2H:
                        return "Attack2H";
                    case WeaponType.MeleeTween:
                        return "AttackTween";
                    case WeaponType.Bow:
                        return "Shot";
                    default:
                        throw new NotImplementedException();
                }
            case "AttackLunge":
                switch (weaponType)
                {
                    case WeaponType.Melee1H:
                    case WeaponType.Melee2H:
                    case WeaponType.MeleeTween:
                    case WeaponType.Bow:
                        return "AttackLunge1H";
                    default:
                        throw new NotImplementedException();
                }
            case "Cast":
                switch (weaponType)
                {
                    case WeaponType.Melee1H:
                    case WeaponType.Melee2H:
                    case WeaponType.MeleeTween:
                    case WeaponType.Bow:
                        return "Cast1H";
                    default:
                        throw new NotImplementedException();
                }
            default:
                return clipName;
        }
    }

    public Entity GetEnemyByName(string enemyName)
    {
        int ID = int.Parse(enemyName.Split(' ')[1]);
        return enemies[ID];
    }

    public void PadVibration(float vForce)
    {
        GamePad.SetVibration(PlayerIndex.One, vForce, vForce);
    }

    private void Controls()
    {
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Attack"))
        {
            player.Attack(new Vector2(xAxis, yAxis).normalized);
        }
        else if (Mathf.Abs(xAxis) > 0.0f || Mathf.Abs(yAxis) > 0.0f)
        {
            player?.MoveToDirection(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
        }
        else player.Idle();

        if (Input.GetButtonDown("Fire2")) player.ShootWeapon();
        if (Input.GetButtonDown("Fire3")) player.ThrowBomb();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.F1)) SetCheatStats();
        if (Input.GetKeyDown(KeyCode.F2)) SpawnEnemyRandom();
        if (Input.GetKeyDown(KeyCode.F3)) RandomEnemyAttack();
        if (Input.GetKeyDown(KeyCode.F12)) RestartScene();
#endif
    }

}


