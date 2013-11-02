using UnityEngine;
using System.Collections;

public class Player : EntityBase {
    public float slideForce;
    public float slideSpeedMax;
    public float slideDelay;
    public float slideHeight = 0.79f;

    public LayerMask solidMask; //use for standing up, etc.

    private PlayerStats mStats;

    private PlatformerController mCtrl;
    private PlatformerSpriteController mCtrlSpr;

    private float mDefaultCtrlMoveForce;
    private float mDefaultCtrlMoveMaxSpeed;
    private Vector3 mDefaultColliderCenter;
    private float mDefaultColliderHeight;

    private CapsuleCollider mCapsuleColl;

    private bool mInputEnabled;
    private bool mSliding;
    private float mSlidingLastTime;

    public bool inputEnabled {
        get { return mInputEnabled; }
        set {
            if(mInputEnabled != value) {
                mInputEnabled = value;

                InputManager input = Main.instance != null ? Main.instance.input : null;

                if(input) {
                    if(mInputEnabled) {
                        input.AddButtonCall(0, InputAction.Fire, OnInputFire);
                        input.AddButtonCall(0, InputAction.PowerNext, OnInputPowerNext);
                        input.AddButtonCall(0, InputAction.PowerPrev, OnInputPowerPrev);
                        input.AddButtonCall(0, InputAction.Jump, OnInputJump);
                    }
                    else {
                        input.RemoveButtonCall(0, InputAction.Fire, OnInputFire);
                        input.RemoveButtonCall(0, InputAction.PowerNext, OnInputPowerNext);
                        input.RemoveButtonCall(0, InputAction.PowerPrev, OnInputPowerPrev);
                        input.RemoveButtonCall(0, InputAction.Jump, OnInputJump);
                    }
                }

                mCtrl.inputEnabled = mInputEnabled;
            }
        }
    }

    public PlatformerController controller { get { return mCtrl; } }
    public PlatformerSpriteController controllerSprite { get { return mCtrlSpr; } }

    protected override void StateChanged() {
        switch((EntityState)state) {
            case EntityState.Normal:
                inputEnabled = true;
                break;

            case EntityState.Hurt:
                inputEnabled = false;
                break;

            case EntityState.Dead:
                inputEnabled = false;
                break;

            case EntityState.Invalid:
                inputEnabled = false;
                break;
        }
    }

    protected override void OnDespawned() {
        //reset stuff here

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here
        inputEnabled = false;

        InputManager input = Main.instance != null ? Main.instance.input : null;
        if(input) {
            input.RemoveButtonCall(0, InputAction.MenuEscape, OnInputPause);
        }

        base.OnDestroy();
    }

    public override void SpawnFinish() {
        //start ai, player control, etc
        state = (int)EntityState.Normal;
    }

    protected override void SpawnStart() {
        //initialize some things
    }

    protected override void Awake() {
        base.Awake();

        //initialize variables
        Main.instance.input.AddButtonCall(0, InputAction.MenuEscape, OnInputPause);

        mCtrl = GetComponent<PlatformerController>();
        mCtrl.moveInputX = InputAction.MoveX;
        mCtrl.moveInputY = InputAction.MoveY;

        mDefaultCtrlMoveMaxSpeed = mCtrl.moveMaxSpeed;
        mDefaultCtrlMoveForce = mCtrl.moveForce;

        mCtrlSpr = GetComponent<PlatformerSpriteController>();

        mCapsuleColl = collider as CapsuleCollider;
        mDefaultColliderCenter = mCapsuleColl.center;
        mDefaultColliderHeight = mCapsuleColl.height;
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    void Update() {
        if(mSliding) {
            InputManager input = Main.instance.input;

            float inpX = input.GetAxis(0, InputAction.MoveX);
            if(inpX < -0.1f)
                mCtrl.moveSide = -1.0f;
            else if(inpX > 0.1f)
                mCtrl.moveSide = 1.0f;

            if(Time.time - mSlidingLastTime >= slideDelay) {
                SetSlide(false);
            }
        }
    }

    //input

    void OnInputFire(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
        }
        else if(dat.state == InputManager.State.Released) {
        }
    }

    void OnInputPowerNext(InputManager.Info dat) {
    }

    void OnInputPowerPrev(InputManager.Info dat) {
    }

    void OnInputJump(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(!mSliding) {
                InputManager input = Main.instance.input;

                if(input.GetAxis(0, InputAction.MoveY) < -0.1f && mCtrl.isGrounded) {
                    SetSlide(true);
                }
                else {
                    mCtrl.Jump(true);
                }
            }
        }
        else if(dat.state == InputManager.State.Released) {
            mCtrl.Jump(false);
        }
    }

    void OnInputPause(InputManager.Info dat) {
    }

    void SetSlide(bool slide) {
        if(mSliding != slide) {
            mSliding = slide;

            if(mSliding) {
                mSlidingLastTime = Time.time;

                mCapsuleColl.height = slideHeight;
                mCapsuleColl.center = new Vector3(mDefaultColliderCenter.x, mDefaultColliderCenter.y - (mDefaultColliderHeight - slideHeight) * 0.5f, mDefaultColliderCenter.z);

                mCtrl.moveMaxSpeed = slideSpeedMax;
                mCtrl.moveForce = slideForce;
                mCtrl.moveSideLock = true;
                mCtrl.moveSide = mCtrlSpr.isLeft ? -1.0f : 1.0f;

                mCtrlSpr.state = PlatformerSpriteController.State.Slide;
            }
            else {
                //cannot set to false if we can't stand
                if(CanStand()) {
                    //revert
                    mCapsuleColl.height = mDefaultColliderHeight;
                    mCapsuleColl.center = mDefaultColliderCenter;

                    mCtrl.moveMaxSpeed = mDefaultCtrlMoveMaxSpeed;
                    mCtrl.moveSideLock = false;
                    mCtrl.moveForce = mDefaultCtrlMoveForce;
                    mCtrl.moveSide = 0.0f;
                    rigidbody.velocity = Vector3.zero;

                    mCtrlSpr.state = PlatformerSpriteController.State.None;

                    Vector3 pos = transform.position;
                    pos.y += (mDefaultColliderHeight - slideHeight) * 0.5f - 0.1f;
                    transform.position = pos;
                }
                else {
                    mSliding = true;
                }
            }
        }
    }

    bool CanStand() {
        const float ofs = 0.2f;

        float r = mCapsuleColl.radius - 0.05f;

        Vector3 c = transform.position + mDefaultColliderCenter;
        Vector3 u = new Vector3(c.x, c.y + (mDefaultColliderHeight * 0.5f - mCapsuleColl.radius) + ofs, c.z);
        Vector3 d = new Vector3(c.x, (c.y - (mDefaultColliderHeight * 0.5f - mCapsuleColl.radius)) + ofs, c.z);

        return !Physics.CheckCapsule(u, d, r, solidMask);
    }
}
