using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public Player player;
    [SerializeField]
    public List<Enemy> enemies = new List<Enemy>();

    public Transform playerTransform;

    public LayerMask entityLayerMask;
    public LayerMask ghostLayerMask;

    public Stats cheatStats;

    public GameObject entityPrefab;
    public Transform mapEnemies;

    private void Awake()
    {
        Instance = Instance ?? this;
        AddDefaultPlayer("Hyde");
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

    private void Update()
    {
        Controls();
        foreach (Entity entity in enemies) entity.Update();
    }

    int enemyCounter = 0;

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

    private void SetCheatStats()
    {
        player.stats = cheatStats;
    }

    private void AddDefaultPlayer(string playerName)
    {
        Player newPlayer = new Player(
            playerTransform,
            new Stats() { life = 1, strength = 1, velocity = 1 },
            playerName
        );
        player = newPlayer;
        playerTransform.name = playerName;
        //entities.Add(player);
    }

    private void Controls()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) player.Attack();
        else if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.0f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.0f)
            player?.MoveToDirection(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
        else player.Idle();

        if (Input.GetKeyDown(KeyCode.F1)) SetCheatStats();
        if (Input.GetKeyDown(KeyCode.F2)) SpawnEnemyRandom();

        // if (Input.GetKey(KeyCode.Mouse1)) player?.SetVelocity(2);
        // else player?.SetVelocity(1);
    }
}
