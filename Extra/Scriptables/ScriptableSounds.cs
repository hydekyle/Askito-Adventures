using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ScriptableSounds : ScriptableObject
{
    public AudioClip attack;
    public AudioClip comboAttackSuccess;
    public AudioClip comboFailure;
    public AudioClip gameOver;
}