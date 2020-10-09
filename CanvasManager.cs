using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using System;

public class CanvasManager : MonoBehaviour
{
    public static CanvasManager Instance;
    public TextMeshProUGUI messageText;
    public GameObject androidControls;
    public GameObject menuOptions;
    public Slider sliderMusic, sliderSound;
    public AudioMixer audioMixer;
    public GameObject retryGO;
    Button retryButton;
    public LightPuzzle ligthPuzzle;
    public Healthbar healthbar;

    public Image avatarTweeter;

    public Image fadeImage;
    float fadeVelocity;

    public Transform shop;

    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }

    private void Start()
    {
        retryButton = retryGO.GetComponent<Button>();
        SetPreviousConfig();
        if (!Application.isMobilePlatform) androidControls.SetActive(false);
    }

    public void HealthbarTakeDamage(int value)
    {
        healthbar.SetHealth(value);
    }

    public float fadeSpeed = 1.5f;
    public IEnumerator FadeIn(Action onEnded)
    {
        float startedTime = Time.time;
        while (fadeImage.color.a < 1f)
        {
            fadeImage.color = new Color(
                0,
                0,
                0,
                Mathf.MoveTowards(fadeImage.color.a, 1f, Time.deltaTime * fadeSpeed)
            );
            yield return new WaitForEndOfFrame();
        }
        onEnded();
    }

    public IEnumerator FadeOut(Action onEnded)
    {
        float startedTime = Time.time;
        while (fadeImage.color.a > 0f)
        {
            fadeImage.color = new Color(
                0,
                0,
                0,
                Mathf.MoveTowards(fadeImage.color.a, 0f, Time.deltaTime * fadeSpeed)
            );
            yield return new WaitForEndOfFrame();
        }
        onEnded();
    }

    public IEnumerator FadeOut()
    {
        fadeImage.color = new Color(0, 0, 0, 1);
        float startedTime = Time.time;
        while (fadeImage.color.a > 0f)
        {
            fadeImage.color = new Color(
                0,
                0,
                0,
                Mathf.MoveTowards(fadeImage.color.a, 0f, Time.deltaTime * fadeSpeed)
            );
            yield return new WaitForEndOfFrame();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) MenuOptionsSetActive(!menuOptions.activeSelf);
    }

    private void SetPreviousConfig()
    {
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume"));
            SetSoundVolume(PlayerPrefs.GetFloat("SoundVolume"));
            sliderMusic.value = PlayerPrefs.GetFloat("MusicVolume");
            sliderSound.value = PlayerPrefs.GetFloat("SoundVolume");
        }
    }

    public void ShowMessage(string msg)
    {
        messageText.text = msg;
    }

    public void MobileControlsSetActive(bool active)
    {
        if (!Application.isMobilePlatform) return;
        androidControls.SetActive(active);
    }

    public void MenuOptionsSetActive(bool active)
    {
        if (!active)
        {
            PlayerPrefs.SetFloat("MusicVolume", sliderMusic.value);
            PlayerPrefs.SetFloat("SoundVolume", sliderSound.value);
        }

        menuOptions.SetActive(active);
        MobileControlsSetActive(!active);
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
        audioMixer.SetFloat("MusicVolume", Mathf.Log(value) * 20);
    }

    private void SetSoundVolume(float value)
    {
        audioMixer.SetFloat("SoundVolume", Mathf.Log(value) * 20);
    }

    public float delayRetryButton = 1f;
    public void ShowRetry()
    {
        Invoke("EnableRetryButton", delayRetryButton);
        retryGO.GetComponent<Animator>().Play("RetryUp", 0);
    }

    public void EnableRetryButton()
    {
        retryButton.interactable = true;
    }

    public void BackToHouse()
    {
        GameManager.Instance.db.AddSouls(ScoreUI.Instance.GetScore());
        GameManager.Instance.LoadNewScene("House");
    }

    public void StartLightPuzzle()
    {
        if (GameManager.Instance.db.CheckKey("LightPuzzle")) return;
        LightPuzzleSetActive(true);
    }

    public void LightPuzzleSetActive(bool active)
    {
        GameManager.Instance.player.isActive = !active;
        if (active) ligthPuzzle.StartPuzzle();
        else ligthPuzzle.ExitPuzzle();

    }

    public void OpenShop()
    {
        GameManager.Instance.player.isActive = false;
        shop.gameObject.SetActive(true);
    }

    public void CloseShop()
    {
        GameManager.Instance.player.isActive = true;
        shop.gameObject.SetActive(false);
    }

}