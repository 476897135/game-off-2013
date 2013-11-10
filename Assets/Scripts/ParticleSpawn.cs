using UnityEngine;
using System.Collections;

/// <summary>
/// Use this with PoolController
/// </summary>
public class ParticleSpawn : MonoBehaviour {
    void OnSpawned() {
        particleSystem.Play();
    }

    void OnDespawned() {
        particleSystem.Clear();
    }

    // Update is called once per frame
    void LateUpdate() {
        if(!particleSystem.IsAlive())
            PoolController.ReleaseAuto(transform);
    }
}
