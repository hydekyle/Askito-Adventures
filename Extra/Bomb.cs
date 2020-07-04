using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject explosionPrefab;

    private void OnEnable()
    {
        Invoke("Explode", 0.25f);
    }

    public void Explode()
    {
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        float radius = 6.66f;
        var hits = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero);

        GameManager.Instance.Explosion(
            transform,
            hits
        );
    }
}
