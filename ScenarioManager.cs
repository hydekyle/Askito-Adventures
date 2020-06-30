using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    public static ScenarioManager Instance;

    private int workCounter = 2; // El mapa empieza con 2 piezas
    private int index = -1;
    private float distanceMap;

    Transform playerT;
    Transform[] maps = new Transform[5];

    public void Awake()
    {
        if (Instance != null) Destroy(Instance.gameObject);
        Instance = this;
    }

    private void Start()
    {
        for (var x = 0; x < maps.Length; x++) maps[x] = transform.GetChild(x); // HardCoded map pieces

        playerT = GameManager.Instance.playerTransform;
        distanceMap = Vector3.Distance(maps[0].localPosition, maps[1].localPosition);
    }

    private void Update()
    {
        CheckForExpanseMap();
    }

    private void CheckForExpanseMap()
    {
        if (playerT.position.x + (Screen.width / 70) > distanceMap * workCounter) ExpanseMap(2);
    }

    void ExpanseMap(int newPieces)
    {
        byte newPiecesCounter = 0;

        while (newPiecesCounter < newPieces)
        {
            index = index + 1 >= maps.Length ? 0 : index + 1;
            maps[index].transform.position = new Vector3(distanceMap * ++workCounter, 0, 0);
            newPiecesCounter++;
        }

    }
}
