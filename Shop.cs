using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shop : MonoBehaviour
{
    public List<SellableWeapon> weapons = new List<SellableWeapon>();


    public void OnPlayerInteraction()
    {
        print("Hello there");
    }

}
