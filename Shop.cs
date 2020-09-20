using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SellableWeapon
{
    public int price;
    public string weaponName;
}

public class Shop : MonoBehaviour
{
    public List<SellableWeapon> weapons = new List<SellableWeapon>();
}
