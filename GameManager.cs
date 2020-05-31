using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public Transform playerTransform;
    public List<Entity> entities = new List<Entity>();
    Player player;
    public Stats cheatStats;
    //public GameObject entityPrefab;

    private void Awake()
    {
        Instance = Instance ?? this;
    }

    private void Start()
    {
        AddDefaultPlayer("Askito");
    }

    private void Update()
    {
        foreach (Entity entity in entities) entity.Update();

        if (Input.GetKeyDown(KeyCode.Mouse0)) player.Attack();
        else if (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.0f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.0f)
            player?.MoveToDirection(new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")));
        else player.Idle();

        if (Input.GetKeyDown(KeyCode.F1)) SetCheatStats();

        if (Input.GetKey(KeyCode.Mouse1)) player?.SetVelocity(2);
        else player?.SetVelocity(1);

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
        entities.Add(player);
    }
}
