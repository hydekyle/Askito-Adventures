using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
    public static SoundsManager Instance;
    public AudioTable audios;
    AudioSource mainSource;

    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
        mainSource = Camera.main.GetComponent<AudioSource>();
    }

    public void PlayHeadCut()
    {
        mainSource.PlayOneShot(audios.head_cut);
    }
}
