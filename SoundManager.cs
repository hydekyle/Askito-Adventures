using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioSource audioSource;
    public ScriptableSounds sounds;

    private void Awake()
    {
        if (Instance != null) Destroy(this.gameObject);
        Instance = this;
    }

    public void PlayComboSuccess()
    {
        audioSource.PlayOneShot(sounds.comboAttackSuccess);
    }

    public void PlayAttack()
    {
        audioSource.PlayOneShot(sounds.attack);
    }

    public void PlayGameOver()
    {
        audioSource.PlayOneShot(sounds.gameOver);
    }

    public void PlayComboFailure()
    {
        audioSource.PlayOneShot(sounds.comboFailure);
    }


}
