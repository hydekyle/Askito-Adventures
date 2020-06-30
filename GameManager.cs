using System.Net.NetworkInformation;
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

    public List<Entity> enemies = new List<Entity>();
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
        foreach (Entity entity in enemies) entity.Update();
    }

    public void SpawnEnemyRandom()
    {
        string enemyName = "Enemy" + ++enemyCounter;
        GameObject go = Instantiate(entityPrefab, Vector3.zero, Quaternion.Euler(0, 180, 0));
        Enemy enemy = new Enemy(
            go.transform,
            new Stats() { life = 1, strength = 1, velocity = 1 },
            enemyName
        );
        go.name = enemyName;
        enemy.Start();
        enemies.Add(enemy);
    }

    private void FixMapSpriteOrders()
    {
        foreach (Transform breakablesT in mapBreakables)
        {
            SpriteRenderer renderer = breakablesT.GetComponent<SpriteRenderer>();
            renderer.sortingOrder = renderer.sortingOrder - (int)(breakablesT.position.y * 100);
        }
    }

    private void GenerateMapEnemies()
    {
        foreach (Transform enemyT in mapEnemies)
        {
            string enemyName = enemyT.name;
            Enemy enemy = new Enemy(
                enemyT,
                new Stats() { life = 1, strength = 1, velocity = 1 },
                enemyName
            );
            enemy.Start();
            enemies.Add(enemy);
            enemyCounter++;
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
        if (breakableT.name == "Barrel")
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

    public static void RemoveEntity(Entity entity)
    {
        if (entity.GetType() == typeof(Enemy)) GameManager.Instance.enemies.Remove(entity);
        else Debug.Log("Se muere el player?");
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

    public Entity GetEnemyByName(string name)
    {
        return enemies.Find(e => e.name == name);
    }

    public void PadVibration(float vForce)
    {
        GamePad.SetVibration(PlayerIndex.One, vForce, vForce);
    }

}


