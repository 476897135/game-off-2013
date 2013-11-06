using UnityEngine;
using System.Collections;

public class Enemy : EntityBase {

    public bool respawnOnSleep = true; //for regular enemies, this will cause a restart on deactivate

    public bool toRespawnAuto = true; //if true, wait for a delay during death to wait for respawn
    public float toRespawnDelay = 0.0f;

    public GameObject visibleGO; //the game object to deactivate while dead/respawning

    private Stats mStats;
    private bool mRespawnReady;

    private Vector3 mSpawnPos;
    private Quaternion mSpawnRot;
    private bool mSpawnRigidBodyKinematic;

    private GravityController mGravCtrl;

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

        Restart();

        mRespawnReady = false;

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    protected override void StateChanged() {
        switch((EntityState)state) {
            case EntityState.Dead:
                SetPhysicsActive(false);

                Blink(0.0f);
                mStats.isInvul = true;

                if(visibleGO)
                    visibleGO.SetActive(false);

                if(toRespawnAuto) {
                    StartCoroutine(DoRespawnWaitDelayKey);
                }
                break;

            case EntityState.RespawnWait:
                Debug.Log("respawn wait");
                RevertTransform();
                activator.ForceActivate();
                break;
        }
    }

    protected override void ActivatorWakeUp() {
        base.ActivatorWakeUp();

        if(state != (int)EntityState.Invalid) {
            if(mRespawnReady) {
                Debug.Log("respawned");

                mRespawnReady = false;
                state = (int)EntityState.Normal;
            }
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
                if(respawnOnSleep) {
                    SetPhysicsActive(false);

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

        mGravCtrl = GetComponent<GravityController>();

        if(!FSM)
            autoSpawnFinish = true;

        //initialize variables
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    void SetPhysicsActive(bool active) {
        if(rigidbody) {
            if(!mSpawnRigidBodyKinematic) {
                rigidbody.isKinematic = !active;
            }

            rigidbody.detectCollisions = active;
        }

        if(collider) {
            collider.enabled = active;
        }

        if(mGravCtrl) {
            mGravCtrl.enabled = active;
        }
    }

    /// <summary>
    /// This is called after death or when set to sleep,
    /// Use this to reset states.  The base will reset stats and telemetry
    /// </summary>
    protected virtual void Restart() {
        //reset physics
        SetPhysicsActive(true);

        if(visibleGO)
            visibleGO.SetActive(true);

        //reset blink
        Blink(0.0f);

        mStats.Reset();

        if(FSM)
            FSM.Reset();

        StopCoroutine(DoRespawnWaitDelayKey);
    }

    void RevertTransform() {
        transform.position = mSpawnPos;
        transform.rotation = mSpawnRot;
    }

    void OnStatsHPChange(Stats stat, float prevVal) {
        if(stat.curHP <= 0.0f) {
            state = (int)EntityState.Dead;
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
}
