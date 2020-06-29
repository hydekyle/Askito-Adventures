using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance;

    private int workCounter = 0;
    private float distanceMap;

    Transform playerT;

    public void Awake()
    {
        if (Instance != null) Destroy(this);
        else Instance = this;
    }

    private void Start()
    {
        playerT = GameManager.Instance.playerTransform;
        Transform map1 = transform.GetChild(0);
        Transform map2 = transform.GetChild(1);
        distanceMap = Vector3.Distance(map1.position, map2.position);
        Destroy(map2.gameObject);
    }

    private void CheckForExpanseMap()
    {
        if (playerT.position.x > distanceMap * workCounter) ExpanseMap();
    }

    void ExpanseMap()
    {
        Transform basedMap = transform.GetChild(0);
        var go = GameObject.Instantiate(basedMap.gameObject, new Vector3(distanceMap * ++workCounter, 0, 0), transform.rotation, transform);
        go.name = workCounter.ToString();
    }

    Transform lastPieceInvisible;
    public void PieceBecameInvisible(Transform piece)
    {
        if (piece.name == (workCounter - 1).ToString()) return; // Evita remover la pieza de la derecha

        if (lastPieceInvisible == null)
        {
            lastPieceInvisible = piece;
        }
        else if (lastPieceInvisible != piece)
        {
            Destroy(lastPieceInvisible.gameObject);
            lastPieceInvisible = piece;
        }
    }

    public void PieceBecameVisible(Transform piece)
    {
        if (int.Parse(piece.name) == workCounter)
        {
            ExpanseMap();
        }
    }
}
