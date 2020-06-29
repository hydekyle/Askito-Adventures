using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CanvasMenu : MonoBehaviour
{
    public void LoadFirstScene()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }
}
