using System.Runtime.CompilerServices;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class EnemiesManager : MonoBehaviour
{
    public static EnemiesManager Instance;

    public IEnumerator[] rutinas;

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
                var random = UnityEngine.Random.Range(0, 10);
                Enemy enemy = waitingForAction[0];
                waitingForAction.RemoveAt(0);
                if (enemy.status == Status.Alive)
                {
                    if (random % 2 == 0) SetEnemyRoutine(enemy, ApproachToPlayerAndAttack(enemy, player.transform));
                    else SetEnemyRoutine(enemy, AproachToWorldPoint(enemy, CameraController.Instance.GetRandomValidPosition()));
                }
            }
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.33f)); //Tiempo para ir dando nuevas acciones
        }
    }

    public void StopAllEnemies()
    {
        waitingForAction = new List<Enemy>();
        foreach (var enemy in GameManager.Instance.enemies) enemy.Idle();
        foreach (var rutina in rutinas) StopCoroutine(rutina);
    }

    public void ImWaitingForNextAction(Enemy enemy, float waitTime)
    {
        if (enemy.status == Status.Alive && !waitingForAction.Contains(enemy))
        {
            StartCoroutine(Waiter(waitTime, () => { waitingForAction.Add(enemy); }));
        }
    }

    private void SetEnemyRoutine(Enemy enemy, IEnumerator rutina)
    {
        if (rutinas[enemy.ID] != null) StopCoroutine(rutinas[enemy.ID]);
        rutinas[enemy.ID] = rutina;
        StartCoroutine(rutinas[enemy.ID]);
    }

    public void SetActionForAllEnemies()
    {
        for (var x = 0; x < GM.enemies.Length; x++)
        {
            Enemy enemy = GM.enemies[x];
            if (enemy.status == Status.Alive)
            {

                var random = UnityEngine.Random.Range(0, 10);
                float distanceToPlayer = Vector2.Distance(player.transform.position, enemy.transform.position);
                if (distanceToPlayer > 1.4f)
                {
                    if (random % 2 == 0) SetEnemyRoutine(enemy, ApproachToPlayerAndAttack(enemy, player.transform));
                    else SetEnemyRoutine(enemy, AproachToWorldPoint(enemy, CameraController.Instance.GetRandomValidPosition()));
                }
            }
        }
    }

    IEnumerator AproachToWorldPoint(Enemy enemy, Vector3 targetPosition)
    {
        float distanceToPlayer;
        float distanceToTarget;
        float nextTime = Time.time + 1.2f;
        float lastDistanceToTarget = 999f;

        do
        {
            Vector2 dir = (targetPosition - enemy.transform.position).normalized;
            distanceToPlayer = Vector2.Distance(enemy.transform.position, player.transform.position);
            distanceToTarget = Vector2.Distance(enemy.transform.position, targetPosition);
            enemy.MoveToDirection(dir);

            if (nextTime < Time.time)
            {
                if (lastDistanceToTarget - distanceToTarget < 1)
                {
                    //Llevo un tiempo atascado
                    break;
                }
                lastDistanceToTarget = distanceToTarget;
                nextTime = Time.time + 1.2f;
            }
            yield return new WaitForFixedUpdate();
        }
        while (distanceToTarget > 2f && distanceToPlayer > 2.5f);
        if (UnityEngine.Random.Range(0, 10) < 3)
        {
            enemy.MoveToDirection(-enemy.transform.right);
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.15f, 0.25f));
        }
        enemy.Idle();
        ImWaitingForNextAction(enemy, UnityEngine.Random.Range(0.1f, 0.3f));
    }

    IEnumerator ApproachToPlayerAndAttack(Enemy enemy, Transform target)
    {
        float distanceToPlayer;
        int initialLife = enemy.stats.life;
        float delayCheck = UnityEngine.Random.Range(0.55f, 1.55f);
        float nextTimeCheckDistance = Time.time + delayCheck;
        float lastDistanceToTarget = 999f;

        do
        {
            Vector2 dir = (target.position - enemy.transform.position).normalized;
            distanceToPlayer = Vector2.Distance(target.position, enemy.transform.position);
            enemy.MoveToDirection(dir);

            if (nextTimeCheckDistance < Time.time)
            {
                if (lastDistanceToTarget - distanceToPlayer < 1)
                {
                    //Llevo un tiempo atascado 
                    break;
                }
                lastDistanceToTarget = distanceToPlayer;
                nextTimeCheckDistance = Time.time + delayCheck;
            }

            yield return new WaitForFixedUpdate();
        }
        while (distanceToPlayer > 1.5f && enemy.status == Status.Alive);

        enemy.Idle();
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.066f, 0.122f));

        if (initialLife == enemy.stats.life && player.isActive)
        {
            enemy.MoveToDirection((target.position - enemy.transform.position).normalized);
            enemy.PlayAnim("Attack");
            StartCoroutine(Waiter(UnityEngine.Random.Range(0.5f, 0.75f), () =>
            {
                if (enemy.status == Status.Alive) ImWaitingForNextAction(enemy, UnityEngine.Random.Range(0.6f, 0.8f));
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
