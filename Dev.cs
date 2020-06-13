using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dev : MonoBehaviour
{
    Player player;

    private void Start()
    {
        player = GameManager.Instance.player;
    }

    public void Attack()
    {
        player.CastRay();
    }
}