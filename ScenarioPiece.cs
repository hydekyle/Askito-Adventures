using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioPiece : MonoBehaviour
{
    private void OnBecameInvisible()
    {
        ScenarioManager.Instance.PieceBecameInvisible(transform);
    }

    private void OnBecameVisible()
    {
        ScenarioManager.Instance.PieceBecameVisible(transform);
    }
}
