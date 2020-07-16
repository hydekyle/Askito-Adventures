using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Serialization;

[CreateAssetMenu]
public class ScriptableWeapons : ScriptableObject
{
    //[FormerlySerializedAs("hp")]
    public List<Weapon> common;
    public List<Weapon> rare;
    public List<Weapon> epic;
    public List<Weapon> legendary;

}