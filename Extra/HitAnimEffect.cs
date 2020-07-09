using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitAnimEffect : MonoBehaviour
{
    ParticleSystem particles;

    private void Awake() 
    {
        particles = particles ?? GetComponent<ParticleSystem>();
    }

    private void OnEnable() 
    {
        particles.Play(true);
        Invoke("CleanMyself", 2f);
    }

    private void CleanMyself()
    {
        gameObject.SetActive(false);
    }

}
