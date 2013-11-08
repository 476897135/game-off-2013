using UnityEngine;
using System.Collections;

public class Player : EntityBase {
    public const string clipHurt = "hurt";

    public float hurtForce = 15.0f;
    public float hurtInvulDelay = 0.5f;

    public float deathFinishDelay = 2.0f;

    public float slideForce;
    public float slideSpeedMax;
    public float slideDelay;
    public float slideHeight = 0.79f;

    public GameObject deathGOActivate;

    public LayerMask solidMask; //use for standing up, etc.

    public Weapon[] weapons;

    private PlayerStats mStats;

    private PlatformerController mCtrl;
    private PlatformerSpriteController mCtrlSpr;

    private SpriteColorBlink[] mBlinks;

    private float mDefaultCtrlMoveForce;
    private float mDefaultCtrlMoveMaxSpeed;
    private Vector3 mDefaultColliderCenter;
    private float mDefaultColliderHeight;

    private CapsuleCollider mCapsuleColl;

    private bool mInputEnabled;
    private bool mSliding;
    private float mSlidingLastTime;

    private bool mHurtActive;

    private int mCurWeaponInd = -1;

    public int currentWeaponIndex {
        get { return mCurWeaponInd; }
        set {
            if(mCurWeaponInd != value && PlayerStats.IsWeaponAvailable(value) && weapons[value] != null) {
                int prevWeaponInd = mCurWeaponInd;
                mCurWeaponInd = value;

                //disable previous
                if(prevWeaponInd >= 0 && prevWeaponInd < weapons.Length && weapons[prevWeaponInd])
                    weapons[prevWeaponInd].gameObject.SetActive(false);

                //enable new one
                weapons[mCurWeaponInd].gameObject.SetActive(true);
            }
        }
    }

    public Weapon currentWeapon {
        get {
            if(mCurWeaponInd >= 0)
                return weapons[mCurWeaponInd];
            return null;
        }
    }

    /// <summary>
    /// Returns the weapon with the current lowest energy
    /// </summary>
    public Weapon lowestEnergyWeapon {
        get {
            Weapon lowestWpn = null;
            for(int i = 0, max = weapons.Length; i < max; i++) {
                Weapon wpn = weapons[i];
                if(wpn && wpn.energyType != Weapon.EnergyType.Unlimited && !wpn.isMaxEnergy) {
                    if(lowestWpn) {
                        if(wpn.currentEnergy < lowestWpn.currentEnergy)
                            lowestWpn = wpn;
                    }
                    else
                        lowestWpn = wpn;
                }
            }
            return lowestWpn;
        }
    }

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

    public Stats stats { get { return mStats; } }

    protected override void StateChanged() {
        switch((EntityState)prevState) {
            case EntityState.Hurt:
                mHurtActive = false;
                break;
        }

        switch((EntityState)state) {
            case EntityState.Normal:
                inputEnabled = true;
                break;

            case EntityState.Hurt:
                inputEnabled = false;

                mCtrlSpr.PlayOverrideClip(clipHurt);

                Blink(hurtInvulDelay);

                StartCoroutine(DoHurtForce(mStats.lastDamageNormal));
                break;

            case EntityState.Dead:
                mCtrl.enabled = false;
                rigidbody.isKinematic = true;
                rigidbody.detectCollisions = false;
                collider.enabled = false;

                //disable all input
                inputEnabled = false;

                InputManager input = Main.instance != null ? Main.instance.input : null;
                if(input) {
                    input.RemoveButtonCall(0, InputAction.MenuEscape, OnInputPause);
                }
                //

                mCtrlSpr.anim.gameObject.SetActive(false);

                if(deathGOActivate)
                    deathGOActivate.SetActive(true);

                PlayerStats.curLife--;

                StartCoroutine(DoDeathFinishDelay());
                break;

            case EntityState.Invalid:
                inputEnabled = false;
                break;
        }
    }

    protected override void SetBlink(bool blink) {
        foreach(SpriteColorBlink blinker in mBlinks) {
            blinker.enabled = blink;
        }

        mStats.isInvul = blink;
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
        currentWeaponIndex = 0;

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

        mCtrlSpr.oneTimeClipFinishCallback += OnSpriteCtrlOneTimeClipEnd;

        mCapsuleColl = collider as CapsuleCollider;
        mDefaultColliderCenter = mCapsuleColl.center;
        mDefaultColliderHeight = mCapsuleColl.height;

        mStats = GetComponent<PlayerStats>();

        mStats.changeHPCallback += OnStatsHPChange;
        mStats.changeMaxHPCallback += OnStatsHPMaxChange;

        foreach(Weapon weapon in weapons) {
            if(weapon) {
                weapon.energyChangeCallback += OnWeaponEnergyCallback;
                weapon.gameObject.SetActive(false);
            }
        }

        mBlinks = GetComponentsInChildren<SpriteColorBlink>(true);
        foreach(SpriteColorBlink blinker in mBlinks) {
            blinker.enabled = false;
        }

        if(deathGOActivate)
            deathGOActivate.SetActive(false);
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
        LevelController.CheckpointApplyTo(transform);
        LevelController.CheckpointApplyTo(CameraController.instance.transform);

        //initialize hp stuff
        HUD.instance.barHP.max = Mathf.CeilToInt(mStats.maxHP);
        HUD.instance.barHP.current = Mathf.CeilToInt(mStats.curHP);
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

    //stats/weapons

    void OnStatsHPChange(Stats stat, float delta) {
        if(delta < 0.0f) {
            if(stat.curHP <= 0.0f) {
                state = (int)EntityState.Dead;
            }
            else {
                state = (int)EntityState.Hurt;
            }

            HUD.instance.barHP.current = Mathf.CeilToInt(stat.curHP);
        }
        else {
            //healed
            //TODO: pause and fill hp one by one
            HUD.instance.barHP.current = Mathf.CeilToInt(stat.curHP);
        }
    }

    void OnStatsHPMaxChange(Stats stat, float delta) {
        HUD.instance.barHP.max = Mathf.CeilToInt(stat.maxHP);
    }

    void OnWeaponEnergyCallback(Weapon weapon, float delta) {
    }

    IEnumerator DoHurtForce(Vector3 normal) {
        mHurtActive = true;

        mCtrl.enabled = false;
        rigidbody.velocity = Vector3.zero;
        rigidbody.drag = 0.0f;

        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        normal.x = Mathf.Sign(normal.x);
        normal.y = 0.0f;
        normal.z = 0.0f;

        while(mHurtActive) {
            yield return wait;

            rigidbody.AddForce(normal * hurtForce);
        }

        mCtrl.enabled = true;
        mCtrl.ResetCollision();

        mHurtActive = false;
    }

    //anim

    void OnSpriteCtrlOneTimeClipEnd(PlatformerSpriteController ctrl, tk2dSpriteAnimationClip clip) {
        if(clip.name == clipHurt) {
            if(state == (int)EntityState.Hurt)
                state = (int)EntityState.Normal;
        }
    }

    //input

    void OnInputFire(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(!mSliding) {
                if(currentWeapon) {
                    currentWeapon.FireStart();
                }
            }
        }
        else if(dat.state == InputManager.State.Released) {
            if(currentWeapon) {
                currentWeapon.FireStop();
            }
        }
    }

    void OnInputPowerNext(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            for(int i = 0, max = weapons.Length, toWeaponInd = currentWeaponIndex + 1; i < max; i++) {
                if(weapons[toWeaponInd] && PlayerStats.IsWeaponAvailable(toWeaponInd)) {
                    currentWeaponIndex = toWeaponInd;
                    break;
                }
                else {
                    toWeaponInd++;
                    if(toWeaponInd >= weapons.Length)
                        toWeaponInd = 0;
                }
            }
        }
    }

    void OnInputPowerPrev(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            for(int i = 0, max = weapons.Length, toWeaponInd = currentWeaponIndex - 1; i < max; i++) {
                if(weapons[toWeaponInd] && PlayerStats.IsWeaponAvailable(toWeaponInd)) {
                    currentWeaponIndex = toWeaponInd;
                    break;
                }
                else {
                    toWeaponInd--;
                    if(toWeaponInd < 0)
                        toWeaponInd = weapons.Length - 1;
                }
            }
        }
    }

    void OnInputJump(InputManager.Info dat) {
        if(dat.state == InputManager.State.Pressed) {
            if(!mSliding) {
                InputManager input = Main.instance.input;

                if(input.GetAxis(0, InputAction.MoveY) < -0.1f && mCtrl.isGrounded) {
                    SetSlide(true);

                    if(currentWeapon) {
                        currentWeapon.FireStop();
                    }
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

    //misc

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

    IEnumerator DoDeathFinishDelay() {
        yield return new WaitForSeconds(deathFinishDelay);

        if(PlayerStats.curLife > 0) {
            //apply changes
            foreach(Weapon weapon in weapons) {
                if(weapon)
                    weapon.SaveEnergySpent();
            }

            Main.instance.sceneManager.Reload();
        }
        else {
            //gameover
            LevelController.ResetData(true);

            Debug.Log("gameover");
        }
    }
}
