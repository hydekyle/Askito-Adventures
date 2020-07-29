using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class EnemiesManager : MonoBehaviour
{
    public static EnemiesManager Instance;

    GameManager GM;
    Player player;

    public List<Enemy> waitingForAction = new List<Enemy>();

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
        StartCoroutine(WaiterBoss());
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Space)) SendAllEnemiesToAttack();
    }

    IEnumerator WaiterBoss()
    {
        while (true)
        {
            if (waitingForAction.Count > 0)
            {
                Enemy enemy = waitingForAction[0];
                waitingForAction.RemoveAt(0);
                if (enemy.status == Status.Alive) StartCoroutine(ApproachToPlayerAndAttack(enemy, player.transform));
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.3f, 0.55f)); //Tiempo para ir dando nuevas acciones
        }
    }

    public void ImWaitingForNextAction(Enemy enemy)
    {
        if (enemy.status == Status.Alive && !waitingForAction.Contains(enemy)) waitingForAction.Add(enemy);
    }

    public void SendAllEnemiesToAttack()
    {
        for (var x = 0; x < GM.enemies.Length; x++)
        {
            if (GM.enemies[x].status == Status.Alive)
            {
                float distanceToPlayer = Vector2.Distance(player.transform.position, GM.enemies[x].transform.position);
                if (distanceToPlayer > 1.4f)
                {
                    if (rutinas[x] != null) StopCoroutine(rutinas[x]);
                    rutinas[x] = ApproachToPlayerAndAttack(GM.enemies[x], player.transform);
                    StartCoroutine(rutinas[x]);
                }
            }
        }
    }

    public IEnumerator[] rutinas;

    IEnumerator ApproachToPlayerAndAttack(Enemy enemy, Transform target)
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
        while (distanceToPlayer > 1.4f && enemy.status == Status.Alive);

        enemy.Idle();
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.22f));

        if (initialLife == enemy.stats.life && player.isActive)
        {
            enemy.MoveToDirection((target.position - enemy.transform.position).normalized);
            enemy.PlayAnim("Attack");
            StartCoroutine(Waiter(UnityEngine.Random.Range(0.5f, 0.75f), () =>
            {
                if (enemy.status == Status.Alive) ImWaitingForNextAction(enemy);
            }));
        }
    }

    IEnumerator Waiter(float time, Action onEnded)
    {
        yield return new WaitForSeconds(time);
        onEnded.Invoke();
    }

    public void StopEnemyRoutine(int ID)
    {
        if (rutinas[ID] != null)
        {
            StopCoroutine(rutinas[ID]);
        }
    }
}
