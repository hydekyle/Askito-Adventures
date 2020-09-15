using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class Helpers : MonoBehaviour
{
    public static async void Waiter(float seconds, Action onEnded)
    {
        await Task.Delay((int)(seconds * 1000));
        onEnded();
    }
}
