using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScoreUI : MonoBehaviour
{
    public static ScoreUI Instance;

    int score = 0;

    public Text scoreText;

    public Color startColor;
    public Color finalColor;
    public float velocity;

    private void Awake()
    {
        if (Instance != null) Destroy(this);
        Instance = this;
    }

    private void Update()
    {
        scoreText.color = Color.Lerp(scoreText.color, startColor, Time.deltaTime * velocity);
    }

    public void AddScore(int value)
    {
        SoundManager.Instance.PlayComboSuccess();
        score += value;
        scoreText.text = score.ToString();
        scoreText.color = finalColor;
    }

}
