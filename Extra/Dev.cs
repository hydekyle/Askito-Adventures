using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dev : MonoBehaviour
{
    Entity myself;

    private void Start()
    {
        var myName = transform.parent.name;
        if (myName == "Player") myself = GameManager.Instance.player;
        else myself = GameManager.Instance.enemies.Find(enemy => enemy.name == myName);
    }

    public void Attack()
    {
        myself.CastAttack(myself.attackDirection);
    }

}