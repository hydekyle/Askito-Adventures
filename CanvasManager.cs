using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Audio;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance;
    public TextMeshProUGUI messageText;
    public GameObject androidControls;
    public GameObject menuOptions;
    public Slider sliderMusic, sliderSound;
    public AudioMixer audioMixer;

    public Image avatarTweeter;

    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }

    private void Start()
    {
        SetPreviousConfig();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !menuOptions.activeSelf) MenuOptionsSetActive(true);
    }

    private void SetPreviousConfig()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume"));
            SetSoundVolume(PlayerPrefs.GetFloat("SoundVolume"));
        }
    }

    public void ShowMessage(string msg)
    {
        messageText.text = msg;
    }

    public void AndroidControlsSetActive(bool active)
    {
        androidControls.SetActive(active);
    }

    public void MenuOptionsSetActive(bool active)
    {
        if (active && PlayerPrefs.HasKey("MusicVolume"))
        {
            sliderMusic.value = PlayerPrefs.GetFloat("MusicVolume");
            sliderSound.value = PlayerPrefs.GetFloat("SoundVolume");
        }
        else
        {
            // Guardar ajustes al cerrar el menú
            PlayerPrefs.SetFloat("MusicVolume", sliderMusic.value);
            PlayerPrefs.SetFloat("SoundVolume", sliderSound.value);
        }

        menuOptions.SetActive(active);
        AndroidControlsSetActive(!active);
    }

    public void OnMusicValueChanged()
    {
        SetMusicVolume(sliderMusic.value);
    }

    public void OnSoundValueChanged()
    {
        SetSoundVolume(sliderSound.value);
    }

    private void SetMusicVolume(float value)
    {
        var musicValue = Mathf.Clamp(-80f + value, -80f, 0f);
        audioMixer.SetFloat("MusicVolume", musicValue);
    }

    private void SetSoundVolume(float value)
    {
        var soundValue = Mathf.Clamp(-80f + value, -80f, 0f);
        audioMixer.SetFloat("SoundVolume", soundValue);
    }

}
