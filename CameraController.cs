using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ZoomOptions { Focus, Normal, Far }

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;
    public Transform playerT;
    Vector3 target;
    public float velocity = 1f;
    Camera mainCamera;
    int cameraSize = 27;
    float verticalExtra = 0;
    public Material backgroundMaterial;
    public Material backgroundFarMaterial;

    private void Awake()
    {
        Instance = Instance ?? this;
        mainCamera = Camera.main;
    }

    void FollowTarget()
    {
        target = playerT.position;
        transform.position = new Vector3(
            Mathf.Clamp(Mathf.Lerp(transform.position.x, target.x, Time.deltaTime * velocity), 0, 54f),
            transform.position.y,
            transform.position.z
        );
        Vector2 backgroundOffset = backgroundMaterial.mainTextureOffset;
        Vector2 backgroundFarOffset = backgroundFarMaterial.mainTextureOffset;
        backgroundMaterial.SetTextureOffset("_MainTex", new Vector2(transform.position.x / 200f, 0));
        backgroundFarMaterial.SetTextureOffset("_MainTex", new Vector2(transform.position.x / 600f, 0));
    }

    private void Update()
    {
        FollowTarget();
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