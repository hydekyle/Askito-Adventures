using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour
{
    public GameObject explosionPrefab;
    SpriteRenderer myRenderer;

    private void OnEnable()
    {
        Invoke("Explode", 0.25f);
        myRenderer = myRenderer ?? GetComponent<SpriteRenderer>();
        myRenderer.enabled = true;
    }

    public void Explode()
    {
        if (GameManager.Instance.bombEffectPool.TryGetNextObject(transform.position, transform.rotation, out GameObject bombAnim))
        {
            float radius = 6.66f;
            var hits = Physics2D.CircleCastAll(transform.position, radius, Vector2.zero);

            GameManager.Instance.ResolveExplosion(
                transform,
                hits
            );
            StartCoroutine(CleanAll(bombAnim));
        }

    }

    IEnumerator<WaitForSeconds> CleanAll(GameObject bombAnim)
    {
        myRenderer.enabled = false;
        yield return new WaitForSeconds(3f);
        bombAnim.SetActive(false);
        gameObject.SetActive(false);
    }
}
