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

    private void Awake()
    {
        Instance = Instance ?? this;
        mainCamera = Camera.main;
    }

    void FollowTarget()
    {
        float yAxis = Input.GetAxis("Vertical");
        if (Mathf.Abs(yAxis) > 0.3f)
        {
            verticalExtra = Mathf.Clamp(Mathf.MoveTowards(verticalExtra, verticalExtra + yAxis * 4, Time.deltaTime * velocity * 9), -7f, 7f);
        }
        else
        {
            verticalExtra = Mathf.MoveTowards(verticalExtra, 0, Time.deltaTime * velocity * 3);
        }
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, cameraSize, Time.deltaTime * velocity);
        target = playerT.position;
        transform.position = new Vector3(
            Mathf.Lerp(transform.position.x, target.x, Time.deltaTime * velocity),
            Mathf.Lerp(transform.position.y, target.y + verticalExtra * 2, Time.deltaTime * velocity * 2),
            transform.position.z
        );
    }

    private void Update()
    {
        FollowTarget();
    }

    public void ZoomOption(ZoomOptions zoomMode)
    {
        switch (zoomMode)
        {
            case ZoomOptions.Focus: cameraSize = 18; break;
            case ZoomOptions.Far: cameraSize = 30; break;
            default: cameraSize = 27; break;
        }
    }

}
