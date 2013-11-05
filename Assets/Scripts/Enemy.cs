using UnityEngine;
using System.Collections;

public class Enemy : EntityBase {
    protected override void OnDespawned() {
        //reset stuff here

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    public override void SpawnFinish() {
        //start ai, player control, etc
    }

    protected override void SpawnStart() {
        //initialize some things
    }

    protected override void Awake() {
        base.Awake();

        //initialize variables
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }
}
