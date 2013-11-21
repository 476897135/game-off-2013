using UnityEngine;
using System.Collections;

public class EnemyCatchy : Enemy {
    public enum Axis {
        X,
        Y
    }

    public Axis axis = Axis.X;

    public GameObject[] buddies;

    public GameObject projectile;
    public float projectileSpeed;
    public float projectileRestDelay;

    public Transform[] projWP;

    public Transform dest;
    public float speed;
    public float speedChase; //when player is in the way

    private Stats[] mBuddyStats;
    private tk2dSpriteAnimator[] mBuddyAnims;
    private TimeWarp[] mBuddyTimeWarps;

    private float mCurDest;
    private float mDest;
    private float mStart;
    private int mCurProjDestInd = 0;
    private float mLastProjTime = 0;
    private int mNumDead = 0;

    private string mDeathSpawnType;

    protected override void StateChanged() {
        base.StateChanged();

        switch((EntityState)state) {
            case EntityState.Stun:
                projectile.SetActive(false);
                Vector3 projPos = projWP[0].position; projPos.z = 0;
                projectile.transform.position = projPos;
                mCurProjDestInd = 1;
                break;

            case EntityState.Normal:
                mLastProjTime = Time.fixedTime;
                projectile.SetActive(mNumDead == 0);
                break;
        }
    }

    protected override void Restart() {
        base.Restart();

        for(int i = 0; i < buddies.Length; i++) {
            buddies[i].SetActive(true);
            mBuddyAnims[i].Play("normal");
            mBuddyStats[i].Reset();
        }

        mNumDead = 0;

        Vector3 projPos = projWP[0].position; projPos.z = 0;
        projectile.transform.position = projPos;

        mCurDest = mDest;
        mCurProjDestInd = 1;
    }

    public override void SpawnFinish() {
        base.SpawnFinish();

        for(int i = 0; i < mBuddyAnims.Length; i++) {
            mBuddyAnims[i].Play("normal");
        }
    }

    protected override void Awake() {
        base.Awake();

        switch(axis) {
            case Axis.X:
                mDest = dest.position.x;
                mStart = transform.position.x;
                break;

            case Axis.Y:
                mDest = dest.position.y;
                mStart = transform.position.y;
                break;
        }

        mBuddyStats = new Stats[buddies.Length];
        mBuddyAnims = new tk2dSpriteAnimator[buddies.Length];
        mBuddyTimeWarps = new TimeWarp[buddies.Length];

        for(int i = 0; i < buddies.Length; i++) {
            mBuddyStats[i] = buddies[i].GetComponent<Stats>();
            mBuddyStats[i].changeHPCallback += OnBuddyHPChange;
            mBuddyTimeWarps[i] = buddies[i].GetComponent<TimeWarp>();
            mBuddyAnims[i] = buddies[i].GetComponentInChildren<tk2dSpriteAnimator>();
        }

        mDeathSpawnType = deathSpawnType;
        deathSpawnType = "";

        Vector3 projPos = projWP[0].position; projPos.z = 0;
        projectile.transform.position = projPos;

        mCurDest = mDest;
        mCurProjDestInd = 1;
    }

    void OnBuddyHPChange(Stats aStat, float delta) {
        int deadInd = -1;
        int stunInd = -1;

        for(int i = 0; i < mBuddyStats.Length; i++) {

            if(mBuddyStats[i] == aStat) {
                if(aStat.lastDamageSource && aStat.lastDamageSource.stun)
                    stunInd = i;

                if(aStat.curHP <= 0) {
                    deadInd = i;
                    mNumDead++;
                }
            }
        }

        if(deadInd != -1) {
            buddies[deadInd].SetActive(false);
            Vector3 pt = buddies[deadInd].collider.bounds.center; pt.z = 0.0f;
            PoolController.Spawn(deathSpawnGroup, mDeathSpawnType, mDeathSpawnType, null, pt, Quaternion.identity);
        }

        projectile.SetActive(mNumDead == 0);

        if(mNumDead == mBuddyStats.Length) {
            state = (int)EntityState.Dead;
        }
        else {
            if(deadInd != -1) {
                for(int i = 0; i < mBuddyAnims.Length; i++) {
                    if(i != deadInd) {
                        mBuddyAnims[i].Play("sad");
                    }
                }
            }
            else if(stunInd != -1) {
                state = (int)EntityState.Stun;
            }
        }
    }

    void FixedUpdate() {
        switch((EntityState)state) {
            case EntityState.Normal:
                float timeScale = 1.0f;
                for(int i = 0; i < mBuddyTimeWarps.Length; i++) {
                    if(mBuddyTimeWarps[i].scale < timeScale)
                        timeScale = mBuddyTimeWarps[i].scale;
                }

                Bounds playerBounds = Player.instance.collider.bounds;

                float curVal = 0;
                float curProjVal = 0, projDest = 0;

                float curSpeed = speed;

                switch(axis) {
                    case Axis.X:
                        curVal = transform.position.x;
                        curProjVal = projectile.transform.position.y;
                        projDest = projWP[mCurProjDestInd].position.y;

                        //check player range
                        for(int i = 0; i < buddies.Length; i++) {
                            Bounds b = buddies[i].collider.bounds;
                            if(!(playerBounds.max.y < b.min.y || playerBounds.min.y > b.max.y)) {
                                curSpeed = speedChase;
                                break;
                            }
                        }
                        break;

                    case Axis.Y:
                        curVal = transform.position.y;
                        curProjVal = projectile.transform.position.x;
                        projDest = projWP[mCurProjDestInd].position.x;

                        //check player range
                        for(int i = 0; i < buddies.Length; i++) {
                            Bounds b = buddies[i].collider.bounds;
                            if(!(playerBounds.max.x < b.min.x || playerBounds.min.x > b.max.x)) {
                                curSpeed = speedChase;
                                break;
                            }
                        }
                        break;
                }

                //move
                float dval = mCurDest - curVal;
                float dirVal = Mathf.Sign(dval);

                float nval = curVal + (dirVal * curSpeed * Time.fixedDeltaTime * timeScale);

                //capped? then set to new dest for later
                if((dirVal < 0.0f && nval < mCurDest) || (dirVal > 0.0f && nval > mCurDest)) {
                    nval = mCurDest;

                    if(mCurDest == mDest) mCurDest = mStart;
                    else mCurDest = mDest;
                }

                Vector3 pos = transform.position;
                switch(axis) {
                    case Axis.X:
                        pos.x = nval;
                        break;
                    case Axis.Y:
                        pos.y = nval;
                        break;
                }
                transform.position = pos;

                /////////////////////////////
                //projectile move
                if(mNumDead == 0 && (Time.fixedTime - mLastProjTime) * timeScale > projectileRestDelay) {
                    dval = projDest - curProjVal;
                    dirVal = Mathf.Sign(dval);

                    nval = curProjVal + (dirVal * projectileSpeed * Time.fixedDeltaTime * timeScale);

                    //capped? then set to new dest for later
                    if((dirVal < 0.0f && nval < projDest) || (dirVal > 0.0f && nval > projDest)) {
                        nval = projDest;

                        mCurProjDestInd++; if(mCurProjDestInd == projWP.Length) mCurProjDestInd = 0;
                        mLastProjTime = Time.fixedTime;
                    }

                    pos = projectile.transform.position;
                    switch(axis) {
                        case Axis.X:
                            pos.y = nval;
                            break;
                        case Axis.Y:
                            pos.x = nval;
                            break;
                    }
                    projectile.transform.position = pos;
                }
                break;
        }
    }
}
