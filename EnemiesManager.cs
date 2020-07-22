using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    GameManager GM;
    Player player;

    void Start()
    {
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
                if (distanceToPlayer > 1.5f) StartCoroutine(ApproachToPlayer(GM.enemies[x]));
            }
        }
    }

    IEnumerator ApproachToPlayer(Enemy enemy)
    {
        float distanceToPlayer;
        do
        {
            Vector2 dir = (player.transform.position - enemy.transform.position).normalized;
            distanceToPlayer = Vector2.Distance(player.transform.position, enemy.transform.position);
            enemy.MoveToDirection(dir);
            yield return new WaitForFixedUpdate();
        }
        while (distanceToPlayer > 1.5f && enemy.status == Status.Alive);
        enemy.PlayAnim("Attack");
        print("Se acabçó");
    }
}
