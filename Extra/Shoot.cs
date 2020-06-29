using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    Vector2 initialPosition;

    private void OnEnable()
    {
        GetComponent<Rigidbody2D>().simulated = true;
        Invoke("CleanMyself", 0.4f);
        initialPosition = transform.position;
    }

    private void OnDisable()
    {
        GetComponent<Rigidbody2D>().simulated = false;
    }

    private void CleanMyself()
    {
        if (transform.gameObject.activeSelf) transform.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        LayerMask hitLayer = other.gameObject.layer;
        Vector2 hitDir = other.transform.position - transform.position;

        if (Vector2.Distance(other.transform.position, initialPosition) > 10f) return;

        if (hitLayer == LayerMask.NameToLayer("Enemy"))
        {
            Entity hitEnemy = GameManager.Instance.GetEnemyByName(other.transform.name);
            if (hitEnemy != null)
            {
                hitEnemy.Burst(hitDir);
            }
        }
        else if (hitLayer == LayerMask.NameToLayer("Breakable"))
        {
            GameManager.BreakBreakable(other.transform, hitDir);
        }
    }
}
