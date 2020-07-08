using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public Material backgroundFarMaterial;
    public Material backgroundMaterial;
    public Transform backgT;

    Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void Update()
    {
        MoveBackground(Time.time * 10);
        backgT.position = new Vector3(
            backgT.position.x,
            -mainCamera.transform.position.y / 7f,
            backgT.position.z
        );
        //backT.position = backT.position - Vector3.up * mainCamera.transform.position.y / 10;
    }

    private void MoveBackground(float value)
    {
        backgroundMaterial.SetTextureOffset("_MainTex", new Vector2(value / 350f, 0));
        backgroundFarMaterial.SetTextureOffset("_MainTex", new Vector2(value / 700f, 0));
    }
}
