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

    public GameObject entityPrefab;
    public GameObject breakingBarrelPrefab;

    public Transform mapEnemies;
    public Transform mapBreakables;
    public GameObject bombPrefab;
    public GameObject shootPrefab;
    EZObjectPool shootPool;

    GamePadState state;
    GamePadState prevState;

    int enemyCounter = 0;

    private void Awake()
    {
        if (Instance != null) Destroy(Instance.gameObject);
        Instance = this;
        enemies = new Enemy[200];
        Inicialize();
        FixMapSpriteOrders();
        AddDefaultPlayer("Player");
        GenerateMapEnemies();
    }

    private void Inicialize()
    {
        shootPool = EZObjectPool.CreateObjectPool(shootPrefab, "Shoot1", 20, true, true, true);
    }

    private void Update()
    {
        Controls();
        player.Update();
        foreach (var valuePair in enemiesRef) valuePair.Value.Update();
        //for (var x = 0; x < enemies.Length; x++) enemies[x]?.Update();
    }

    private int GetNextEnemyID()
    {
        if (enemyCounter + 1 >= enemies.Length) enemyCounter = 0;
        else enemyCounter++;
        return enemyCounter;
    }

    public void SpawnEnemyRandom()
    {
        int enemyID = GetNextEnemyID();
        string enemyName = "Enemy " + enemyID;
        GameObject go = Instantiate(entityPrefab, Vector3.zero, Quaternion.Euler(0, 180, 0));
        Enemy enemy = new Enemy(
            go.transform,
            new Stats() { life = 1, strength = 1, velocity = 1 },
            enemyName,
            enemyID
        );
        go.name = enemyName;
        enemy.Start();
        AddEnemy(enemyID, enemy);
    }

    private void GenerateMapEnemies()
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
            AddEnemy(enemyID, enemy);
            enemy.Start();
        }
    }

    public void AddEnemy(int enemyID, Enemy enemy)
    {
        enemiesRef.Add(enemyID, enemy);
        enemies[enemyID] = enemy;
    }

    public void DeleteEnemy(int enemyID)
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
        //enemies[UnityEngine.Random.Range(0, enemies.Count)].Attack();
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

    private void RestartScene()
    {
        Scene thisScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(thisScene.name);
    }

    public void ShootWeapon(Vector2 sPosition, Vector2 sDirection)
    {
        GameObject go;
        if (shootPool.TryGetNextObject(sPosition, transform.rotation, out go))
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

    public void RemoveEntity(Entity entity)
    {
        if (entity.GetType() == typeof(Enemy))
        {
            DeleteEnemy(entity.ID);
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

}


