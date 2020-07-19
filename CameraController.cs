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
    public float backDistanceMax = 20f;

    public float maxPlayerDistanceLeft;
    //public float maxPlayerDistanceRight;

    Camera mainCamera;
    Vector3 target;
    float cameraSize = 4.44f;
    float minPosX;
    public bool followActive = true;

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
    }

    private void Update()
    {
        if (followActive) FollowTarget();
        UpdateCameraClampValue();
    }

    private void FollowTarget()
    {
        target = playerT.position;
        transform.position = new Vector3(
            Mathf.Lerp(
                transform.position.x,
                Mathf.Clamp(target.x + 1, minPosX - backDistanceMax, Mathf.Infinity),
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
        maxPlayerDistanceLeft = mainCamera.ViewportToWorldPoint(new Vector3(mainCamera.rect.xMin, 0, 0)).x;
        //maxPlayerDistanceRight = mainCamera.ViewportToWorldPoint(new Vector3(mainCamera.rect.xMax, 0, 0)).x;
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