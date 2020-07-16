using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public Material backgroundFarMaterial;
    //public Material backgroundMaterial;
    public Transform backgT;

    Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        MoveBackground(Time.time);
        backgT.position = new Vector3(
            backgT.position.x,
            -mainCamera.transform.position.y / 1.5f,
            backgT.position.z
        );
    }

    private void MoveBackground(float value)
    {
        backgroundFarMaterial.SetTextureOffset("_MainTex", new Vector2(value, 0));
        //backgroundMaterial.SetTextureOffset("_MainTex", new Vector2(value / 2000f, 0));
    }
}