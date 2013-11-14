using UnityEngine;
using System.Collections;

public class EnemyCatRoller : Enemy {
    public const string rockProjType = "rockBombRoller";

    public float rockYOfs;

    public float defaultMoveSide = -1.0f;

    private EntitySensor mSensor;
    private Projectile mRock;
    private PlatformerController mRockCtrl;
    private PlatformerSpriteController mSpriteCtrl;

    protected override void StateChanged() {
        base.StateChanged();

        switch((EntityState)state) {
            case EntityState.Normal:
                if(!mRock) {
                    Vector3 rockPos = transform.position;
                    rockPos.y += rockYOfs;
                    mRock = Projectile.Create(projGroup, rockProjType, rockPos, Vector3.zero, null);

                    mRockCtrl = mRock.GetComponent<PlatformerController>();

                    mSpriteCtrl.controller = mRockCtrl;
                }

                mRockCtrl.dirHolder = transform;
                mRockCtrl.moveSideLock = true;
                mRockCtrl.moveSide = defaultMoveSide;

                if(mSensor) {
                    mSensor.Activate(true);
                }
                break;

            case EntityState.Stun:
                mRockCtrl.moveSide = 0.0f;
                break;

            case EntityState.Dead:
                if(mRock && mRock.isAlive) {
                    if(mRock.stats)
                        mRock.stats.curHP = 0;
                    else
                        mRock.state = (int)Projectile.State.Dying;

                    mRock = null;
                }

                if(mSensor) {
                    mSensor.Activate(false);
                }

                mSpriteCtrl.controller = null;
                break;

            case EntityState.RespawnWait:
                if(mRock && !mRock.isReleased) {
                    mRock.Release();
                    mRock = null;
                }

                if(mSensor) {
                    mSensor.Activate(false);
                }

                mSpriteCtrl.controller = null;

                RevertTransform();
                break;
        }
    }

    protected override void Awake() {
        base.Awake();

        mSensor = GetComponent<EntitySensor>();
        mSensor.updateCallback += OnSensorUpdate;

        mSpriteCtrl = GetComponent<PlatformerSpriteController>();
    }

    void FixedUpdate() {
        bool updatePos = false;

        switch((EntityState)state) {
            case EntityState.Hurt:
            case EntityState.Normal:
                if(mSensor)
                    mSensor.hFlip = mSpriteCtrl.isLeft;

                if(mRock.state == (int)Projectile.State.Dying) {
                    mRock = null;
                    state = (int)EntityState.Dead;
                }
                else {
                    if(mRockCtrl.isGrounded) {
                        if(mRockCtrl.moveSide == 0.0f)
                            mRockCtrl.moveSide = defaultMoveSide;
                    }
                }
                updatePos = true;
                break;

            case EntityState.Stun:
                updatePos = true;
                break;
        }

        if(updatePos && mRock) {
            Vector3 rockPos = mRock.transform.position;
            rockPos.y -= rockYOfs;
            rigidbody.MovePosition(rockPos);
        }
    }

    void OnSensorUpdate(EntitySensor sensor) {
        switch((EntityState)state) {
            case EntityState.Normal:
            case EntityState.Hurt:
                if(sensor.isHit) {
                    if(Vector3.Angle(mRockCtrl.moveDir, sensor.hit.normal) >= 170.0f) {
                        //mRockCtrl.rigidbody.velocity = Vector3.zero;
                        mRockCtrl.moveSide *= -1.0f;
                        //Debug.Log("move side: " + mRockCtrl.moveSide);
                    }
                }
                else {
                    if(mRockCtrl.isGrounded) {
                        mRockCtrl.rigidbody.velocity = Vector3.zero;
                        mRockCtrl.moveSide *= -1.0f;
                        //Debug.Log("move side: " + mRockCtrl.moveSide);
                    }
                }
                break;
        }
    }

    void OnDrawGizmosSelected() {
        Vector3 pos = transform.position;
        pos.y += rockYOfs;

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(pos, 0.1f);
    }
}
