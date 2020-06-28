using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject explosionPrefab;

    Vector3 startPosition;
    Vector3 destinyPosition;
    float startTime;

    private void OnEnable()
    {
        Invoke("Explode", 0.25f);
        startTime = Time.time;
        startPosition = transform.position;
        destinyPosition = transform.position - Vector3.up * 3;
    }

    public void Explode()
    {
        Instantiate(explosionPrefab, transform.position, transform.rotation);
        float radius = 6.66f;
        var hits = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero);
        foreach (var hit in hits)
        {
            float distance = Vector2.Distance(hit.transform.position, transform.position);
            Vector2 hitDir = (hit.transform.position - transform.position).normalized;
            var hitMask = hit.transform.gameObject.layer;
            if (hitMask == LayerMask.NameToLayer("Enemy"))
            {
                GameManager.Instance.enemies.Find(enemy => enemy.name == hit.transform.gameObject.name)?.Burst(hitDir);
            }
            else if (hitMask == LayerMask.NameToLayer("Breakable"))
            {
                GameManager.BreakBreakable(hit.transform, hitDir);
            }
            else if (hitMask == LayerMask.NameToLayer("Player"))
            {
                hit.transform.GetComponent<Rigidbody2D>().AddForce(hitDir * 2, ForceMode2D.Impulse);
            }
            else if (hitMask == LayerMask.NameToLayer("Movible"))
            {
                hit.transform.GetComponent<Rigidbody2D>().AddForce(hitDir * 999 / Mathf.Pow(distance, 2), ForceMode2D.Impulse);
            }
        }
        Destroy(gameObject);
    }
}
