using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ZoomOptions { Focus, Normal, Far }

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    Transform playerT;
    public Material backgroundMaterial;
    public Material backgroundFarMaterial;
    public float velocity = 1.3f;
    public float backDistanceMax = 10f;

    public float maxDistanceLeft;
    public float maxDistanceRight;

    Camera mainCamera;
    float targetX;
    float cameraSize = 4.44f;
    float minPosX;
    public bool followActive = true;

    public bool battleMode = false;
    bool isPacificMap;
    public float minCameraLimitX = -999f, maxCameraLimitX = 999f;
    float finalValue = 7;
    float extraX = 6;

    private void Awake()
    {
        if (Instance != null) Destroy(Instance.gameObject);
        Instance = this;
        mainCamera = Camera.main;
    }

    private void Start()
    {
        playerT = GameManager.Instance.playerTransform;
        minPosX = playerT.position.x;
        isPacificMap = GameManager.Instance.isPacificLevel;
    }

    private void Update()
    {
        UpdateCameraClampValue();
        if (followActive) FollowTarget();
        if (!isPacificMap) UpdateExtraValue();
    }

    public void SetBattleMode(bool battleON)
    {
        battleMode = battleON;
        if (battleON)
        {
            finalValue = 0f;
            minCameraLimitX = transform.position.x - backDistanceMax;
            maxCameraLimitX = transform.position.x + backDistanceMax;
            GetMapPosValues();
        }
        else finalValue = 9f;
    }

    public void GetMapPosValues()
    {

    }

    public Vector3 GetRandomValidPosition()
    {
        float xValue = UnityEngine.Random.Range(0.2f, 0.8f);
        float yValue = UnityEngine.Random.Range(GameManager.Instance.minY, GameManager.Instance.maxY);
        Vector3 randomPos = mainCamera.ViewportToWorldPoint(new Vector3(xValue, 0f, 0f));
        randomPos = new Vector3(randomPos.x, yValue, 0f);
        return randomPos;
    }

    void UpdateExtraValue()
    {
        if (extraX != finalValue)
        {
            extraX = Mathf.MoveTowards(extraX, finalValue, Time.deltaTime * velocity);
        }
    }

    private float TargetCamPosX()
    {
        targetX = playerT.position.x;
        if (isPacificMap) return Mathf.Clamp(targetX, minCameraLimitX, maxCameraLimitX);
        return battleMode ?
                    Mathf.Clamp(targetX + extraX, minCameraLimitX, maxCameraLimitX) :
                    Mathf.Clamp(targetX + extraX, Mathf.Clamp(minPosX - backDistanceMax, 5, Mathf.Infinity), Mathf.Infinity);
    }

    private void FollowTarget()
    {
        transform.position = new Vector3(
            Mathf.Lerp(
                transform.position.x,
                TargetCamPosX(),
                Time.deltaTime * velocity
                ),
            transform.position.y,
            transform.position.z
        );
        MoveBackground();
    }

    private void UpdateCameraClampValue()
    {
        if (minPosX < playerT.position.x) minPosX = playerT.position.x;
        maxDistanceLeft = mainCamera.ViewportToWorldPoint(new Vector3(mainCamera.rect.xMin, 0, 0)).x;
        maxDistanceRight = mainCamera.ViewportToWorldPoint(new Vector3(mainCamera.rect.xMax, 0, 0)).x;
    }

    private void MoveBackground()
    {
        backgroundMaterial.SetTextureOffset("_MainTex", new Vector2(transform.position.x / 350f, 0));
        backgroundFarMaterial.SetTextureOffset("_MainTex", new Vector2(transform.position.x / 700f, 0));
    }

    public void ZoomOption(ZoomOptions zoomMode)
    {
        switch (zoomMode)
        {
            case ZoomOptions.Focus:
                cameraSize = 3f;
                break;
            case ZoomOptions.Far:
                cameraSize = 5f;
                break;
            default:
                cameraSize = 4.44f;
                break;
        }
        mainCamera.orthographicSize = cameraSize;
    }

}