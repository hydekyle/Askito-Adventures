using System;
using UnityEngine;

public class CullingManager
{
    CullingGroup group;
    BoundingSphere[] spheres;
    int maxEnemies;

    public CullingManager()
    {
        Initialize();
    }

    void Initialize()
    {
        maxEnemies = GameManager.Instance.maxEnemies;
        spheres = new BoundingSphere[maxEnemies];

        for (var x = 0; x < spheres.Length; x++)
        {
            spheres[x].position = Vector3.up * 9999;
            spheres[x].radius = 2f;
        }
        group = new CullingGroup();
        group.targetCamera = Camera.main;
    }

    public void SetSphere(int ID, Vector2 spherePosition)
    {
        spheres[ID].position = spherePosition;
        group.SetBoundingSpheres(spheres);
        group.SetBoundingSphereCount(maxEnemies);
        group.onStateChanged = StateChangedMethod;
    }

    public bool IsVisible(int ID)
    {
        return group.IsVisible(ID);
    }

    private void StateChangedMethod(CullingGroupEvent evt)
    {
        if (evt.hasBecomeVisible)
            Debug.LogFormat("Sphere {0} has become visible!", evt.index);
        if (evt.hasBecomeInvisible)
            Debug.LogFormat("Sphere {0} has become invisible!", evt.index);
    }

    public void Dispose()
    {
        group.Dispose();
    }

}