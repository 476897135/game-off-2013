﻿using UnityEngine;
using System.Collections;

public class Enemy : EntityBase {
    public const string projGroup = "projEnemy";
    public const float stunDelay = 1.0f;

    public bool respawnOnSleep = true; //for regular enemies, this will cause a restart on deactivate

    public bool toRespawnAuto = true; //if true, wait for a delay during death to wait for respawn
    public float toRespawnDelay = 0.0f;

    public AnimatorData animator; //if this enemy is controlled by an animator (usu. as parent)
    public GameObject visibleGO; //the game object to deactivate while dead/respawning
    public GameObject stunGO;

    public string deathSpawnGroup; //upon death, spawn this
    public string deathSpawnType;

    private Stats mStats;
    private bool mRespawnReady;

    private Vector3 mSpawnPos;
    private Quaternion mSpawnRot;
    private bool mSpawnRigidBodyKinematic;

    private GravityController mGravCtrl;
    private RigidBodyController mBodyCtrl;
    private float mDefaultDeactiveDelay;

    private Damage[] mDamageTriggers;

    public Stats stats { get { return mStats; } }

    /// <summary>
    /// Call this for manual respawn wait during death sequence
    /// </summary>
    public void ToRespawnWait() {
        state = (int)EntityState.RespawnWait;
    }

    protected override void OnDespawned() {
        //reset stuff here
        state = (int)EntityState.Invalid;

        if(animator)
            animator.Stop();

        Restart();

        mRespawnReady = false;

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    protected override void StateChanged() {
        switch((EntityState)prevState) {
            case EntityState.Stun:
                if(stunGO)
                    stunGO.SetActive(false);

                SetPhysicsActive(true, true);

                CancelInvoke("DoStun");
                break;

            case EntityState.RespawnWait:
                if(activator)
                    activator.deactivateDelay = mDefaultDeactiveDelay;
                break;
        }

        switch((EntityState)state) {
            case EntityState.Dead:
                SetPhysicsActive(false, false);

                Blink(0.0f);
                mStats.isInvul = true;

                if(visibleGO)
                    visibleGO.SetActive(false);

                if(!string.IsNullOrEmpty(deathSpawnGroup) && !string.IsNullOrEmpty(deathSpawnType)) {
                    PoolController.Spawn(deathSpawnGroup, deathSpawnType, deathSpawnType, null, transform.position, Quaternion.identity);
                }

                if(toRespawnAuto) {
                    StartCoroutine(DoRespawnWaitDelayKey);
                }
                break;

            case EntityState.Stun:
                if(stunGO)
                    stunGO.SetActive(true);

                SetPhysicsActive(false, true);

                Invoke("DoStun", stunDelay);
                break;

            case EntityState.RespawnWait:
                //Debug.Log("respawn wait");
                RevertTransform();

                if(activator) {
                    activator.deactivateDelay = 0.0f;
                    activator.ForceActivate();
                }
                break;
        }
    }

    protected override void ActivatorWakeUp() {
        base.ActivatorWakeUp();

        if(mRespawnReady) {
            //Debug.Log("respawned");

            mRespawnReady = false;
            state = (int)EntityState.Normal;
        }
    }

    protected override void ActivatorSleep() {
        base.ActivatorSleep();

        switch((EntityState)state) {
            case EntityState.RespawnWait:
                Restart();
                mRespawnReady = true;
                break;

            case EntityState.Normal:
            case EntityState.Hurt:
            case EntityState.Stun:
                if(respawnOnSleep) {
                    SetPhysicsActive(false, false);

                    if(visibleGO)
                        visibleGO.SetActive(false);

                    ToRespawnWait();
                }
                break;

            case EntityState.Dead:
                if(toRespawnAuto) {
                    StopCoroutine(DoRespawnWaitDelayKey);
                    ToRespawnWait();
                }
                break;

            case EntityState.Invalid:
                break;
        }
    }

    public override void SpawnFinish() {
        //start ai, player control, etc
        state = (int)EntityState.Normal;
    }

    protected override void SpawnStart() {
        //initialize some things
        mSpawnPos = transform.position;
        mSpawnRot = transform.rotation;

        if(rigidbody)
            mSpawnRigidBodyKinematic = rigidbody.isKinematic;
    }

    protected override void Awake() {
        base.Awake();

        mStats = GetComponent<Stats>();
        mStats.changeHPCallback += OnStatsHPChange;

        mBodyCtrl = GetComponent<RigidBodyController>();
        mGravCtrl = GetComponent<GravityController>();

        mDamageTriggers = GetComponentsInChildren<Damage>(true);

        if(!FSM)
            autoSpawnFinish = true;

        if(stunGO)
            stunGO.SetActive(false);

        if(activator)
            mDefaultDeactiveDelay = activator.deactivateDelay;

        //initialize variables
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    void SetPhysicsActive(bool aActive, bool excludeCollision) {
        if(rigidbody) {
            if(!mSpawnRigidBodyKinematic) {
                rigidbody.isKinematic = !aActive;
            }

            if(aActive || !excludeCollision)
                rigidbody.detectCollisions = aActive;
        }

        if(collider && (aActive || !excludeCollision)) {
            collider.enabled = aActive;
        }

        if(mGravCtrl) {
            mGravCtrl.enabled = aActive;
        }

        if(animator) {
            if(aActive) {
                if(animator.isPaused)
                    animator.Resume();
            }
            else {
                if(animator.isPlaying)
                    animator.Pause();
            }
        }

        if(mBodyCtrl)
            mBodyCtrl.enabled = aActive;

        for(int i = 0, max = mDamageTriggers.Length; i < max; i++)
            mDamageTriggers[i].gameObject.SetActive(aActive);
    }

    /// <summary>
    /// This is called after death or when set to sleep,
    /// Use this to reset states.  The base will reset stats and telemetry
    /// </summary>
    protected virtual void Restart() {
        //reset physics
        SetPhysicsActive(true, false);

        if(visibleGO)
            visibleGO.SetActive(true);

        //reset blink
        Blink(0.0f);

        mStats.Reset();

        if(FSM)
            FSM.Reset();

        if(mBodyCtrl)
            mBodyCtrl.enabled = true;

        if(stunGO)
            stunGO.SetActive(false);

        StopCoroutine(DoRespawnWaitDelayKey);
    }

    protected void RevertTransform() {
        transform.position = mSpawnPos;
        transform.rotation = mSpawnRot;
    }

    void OnStatsHPChange(Stats stat, float delta) {
        if(stat.curHP <= 0.0f) {
            state = (int)EntityState.Dead;
        }
        else if(delta < 0.0f) {
            if(stat.lastDamageSource != null && stat.lastDamageSource.stun)
                state = (int)EntityState.Stun;
        }
    }

    private const string DoRespawnWaitDelayKey = "DoRespawnWaitDelay";
    IEnumerator DoRespawnWaitDelay() {
        if(toRespawnDelay > 0.0f) {
            yield return new WaitForSeconds(toRespawnDelay);
        }
        else {
            yield return new WaitForFixedUpdate();
        }

        ToRespawnWait();
    }

    void DoStun() {
        if(state == (int)EntityState.Stun)
            state = (int)EntityState.Normal;
    }
}
