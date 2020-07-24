using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    public static EnemiesManager Instance;

    GameManager GM;
    Player player;

    private void Awake()
    {
        if (Instance != null) Destroy(this.gameObject);
        Instance = this;
    }

    void Start()
    {
        rutinas = new IEnumerator[GameManager.Instance.maxEnemies];
        GM = GameManager.Instance;
        player = GM.player;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) CheckForActions();
    }

    public void CheckForActions()
    {
        for (var x = 0; x < GM.enemies.Length; x++)
        {
            if (GM.enemies[x].status == Status.Alive)
            {
                float distanceToPlayer = Vector2.Distance(player.transform.position, GM.enemies[x].transform.position);
                if (distanceToPlayer > 1.5f)
                {
                    if (rutinas[x] != null) StopCoroutine(rutinas[x]);
                    rutinas[x] = ApproachToPlayer(GM.enemies[x], player.transform);
                    StartCoroutine(rutinas[x]);
                }
            }
        }
    }

    public IEnumerator[] rutinas;

    IEnumerator ApproachToPlayer(Enemy enemy, Transform target)
    {
        float distanceToPlayer;
        int initialLife = enemy.stats.life;
        do
        {
            Vector2 dir = (target.position - enemy.transform.position).normalized;
            distanceToPlayer = Vector2.Distance(target.position, enemy.transform.position);
            enemy.MoveToDirection(dir);
            yield return new WaitForFixedUpdate();
        }
        while (distanceToPlayer > 1.5f && enemy.status == Status.Alive);
        enemy.Idle();
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.7f));
        if (player.isActive)
        {
            enemy.MoveToDirection((target.position - enemy.transform.position).normalized);
            if (initialLife == enemy.stats.life) enemy.PlayAnim("Attack");
        }
    }

    public void StopEnemyRoutine(int ID)
    {
        if (rutinas[ID] != null)
        {
            StopCoroutine(rutinas[ID]);
        }
    }
}
