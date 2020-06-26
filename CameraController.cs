using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ZoomOptions { Focus, Normal, Far }

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;

    public Transform playerT;
    public Material backgroundMaterial;
    public Material backgroundFarMaterial;
    public float velocity = 1f;

    Camera mainCamera;
    Vector3 target;
    int cameraSize = 27;
    float minPosX;

    private void Awake()
    {
        Instance = Instance ?? this;
        mainCamera = Camera.main;
    }

    private void Start()
    {
        minPosX = playerT.position.x;
    }

    private void Update()
    {
        UpdateClampValue();
        FollowTarget();
    }

    private void FollowTarget()
    {
        target = playerT.position;
        transform.position = new Vector3(
            Mathf.Lerp(
                Mathf.Clamp(transform.position.x, minPosX - 3.3f, Mathf.Infinity),
                target.x,
                Time.deltaTime * velocity
                ),
            transform.position.y,
            transform.position.z
        );
        MoveBackground();
    }

    private void UpdateClampValue()
    {
        if (minPosX < playerT.position.x) minPosX = playerT.position.x;
    }

    private void MoveBackground()
    {
        Vector2 backgroundOffset = backgroundMaterial.mainTextureOffset;
        Vector2 backgroundFarOffset = backgroundFarMaterial.mainTextureOffset;
        backgroundMaterial.SetTextureOffset("_MainTex", new Vector2(transform.position.x / 350f, 0));
        backgroundFarMaterial.SetTextureOffset("_MainTex", new Vector2(transform.position.x / 700f, 0));
    }

    public void ZoomOption(ZoomOptions zoomMode)
    {
        switch (zoomMode)
        {
            case ZoomOptions.Focus:
                cameraSize = 18;
                break;
            case ZoomOptions.Far:
                cameraSize = 30;
                break;
            default:
                cameraSize = 27;
                break;
        }
    }

}