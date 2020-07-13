using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ScriptableWeapons : ScriptableObject
{
    public List<Weapon> common;
    public List<Weapon> rare;
    public List<Weapon> epic;
    public List<Weapon> legendary;

}