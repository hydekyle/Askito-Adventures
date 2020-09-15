using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class ScriptableIcons : ScriptableObject
{
    public Sprite souls;
    [Space(5)]
    [Header("Stats")]
    public Sprite attr_attack;
    public Sprite attr_distance;
    public Sprite attr_velocity;
}
