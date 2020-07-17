using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance;
    public TextMeshProUGUI messageText;

    public Image avatarTweeter;

    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }

    public void ShowMessage(string msg)
    {
        messageText.text = msg;
    }

}
