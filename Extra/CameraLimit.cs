using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLimit : MonoBehaviour
{
    public bool isRightLimit;

    private void OnBecameVisible()
    {
        if (isRightLimit) CameraController.Instance.maxCameraLimitX = CameraController.Instance.transform.position.x;
        else CameraController.Instance.minCameraLimitX = CameraController.Instance.transform.position.x;
    }

}
