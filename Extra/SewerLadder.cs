using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SewerLadder : MonoBehaviour
{
    public void OnPlayerInteraction()
    {
        SceneManager.LoadScene("MainLevel");
    }
}
