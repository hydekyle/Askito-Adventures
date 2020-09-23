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
    Transform[] maps = new Transform[4];

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
        if (playerT.position.x + 5 > distanceMap * workCounter) ExpanseMap(2);
    }

    void ExpanseMap(int newPieces)
    {
        byte newPiecesCounter = 0;

        while (newPiecesCounter < newPieces)
        {
            index = index + 1 >= maps.Length ? 0 : index + 1;
            CleanBreakables(maps[index]);
            maps[index].transform.position = new Vector3(distanceMap * ++workCounter, 0, 0);
            AddRandomStuff(maps[index]);
            newPiecesCounter++;
        }
    }

    void AddRandomStuff(Transform mapPiece)
    {
        int barrelsNumber = Random.Range(1, 4);
        for (var x = 0; x < barrelsNumber; x++)
        {
            if (GameManager.Instance.barrelsPool.TryGetNextObject(mapPiece.transform.localPosition, Quaternion.identity, out GameObject newBarrel))
            {
                newBarrel.transform.parent = mapPiece.Find("Breakables");
                newBarrel.transform.position = mapPiece.position + new Vector3(Random.Range(-9f, 9f), Random.Range(0f, -2f), 0) + Vector3.right * barrelsNumber;
                GameManager.Instance.SetSpriteOrder(newBarrel.GetComponent<SpriteRenderer>(), 15);
                newBarrel.SetActive(true);
            }
        }
    }

    void CleanBreakables(Transform mapPiece)
    {
        foreach (Transform t in mapPiece.Find("Breakables")) t.gameObject.SetActive(false);
    }
}
