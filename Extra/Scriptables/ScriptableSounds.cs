using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ScriptableSounds : ScriptableObject
{
    [Header("Combat")]
    public AudioClip attack;
    public AudioClip comboAttackSuccess;
    public AudioClip comboFailure;
    public AudioClip hitSlash;
    public AudioClip hitPlayer;

    [Header("Others")]
    public AudioClip gameOver;
}