using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance;

    void Awake()
    {
        Instance = Instance ?? this;
    }

    public string GetTextDialog(Dialog dialog)
    {
        string textDialog = "";
        switch (dialog)
        {
            case Dialog.Test: textDialog = "De locos tests"; break;
        }
        return textDialog;
    }

    public enum Dialog { Test, Test1 };
}