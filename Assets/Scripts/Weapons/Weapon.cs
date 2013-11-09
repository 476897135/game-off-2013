﻿using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
    public enum EnergyType {
        Unlimited = -1,

        LightningFork,
        ConflictResolver,
        TimeWarp,
        Whip,
        HoolaHoop,
        Clone,

        NumTypes
    }

    public enum AnimState {
        normal,
        attack,
        charge
    }

    [System.Serializable]
    public class ChargeInfo {
        public float energyCost;

        public GameObject go;
        public float delay;
        public string projType;
        public ParticleSystem particles;

        public void Enable(bool enable) {
            if(particles) {
                if(enable) {
                    particles.loop = true;
                    particles.Play();
                }
                else {
                    particles.loop = false;
                }
            }

            if(go)
                go.SetActive(enable);
        }
    }

    public delegate void ChangeValueCallback(Weapon weapon, float delta);

    public const string weaponEnergyPrefix = "wpnE";
    public const float weaponEnergyDefaultMax = 32.0f;

    [SerializeField]
    string _iconSpriteRef;

    [SerializeField]
    string _labelTextRef;

    public EnergyType energyType = EnergyType.Unlimited;

    public Color color = Color.white;

    public tk2dSpriteAnimator anim;

    public string projGroup = "proj";
    public int projMax = 4;

    //first charge is the regular fire
    public ChargeInfo[] charges;

    public event ChangeValueCallback energyChangeCallback;

    [SerializeField]
    Transform _spawnPoint;

    private tk2dSpriteAnimationClip[] mClips;

    private bool mFireActive = false;
    private int mCurChargeLevel = 0;
    protected int mCurProjCount = 0;
    private bool mStarted = false;
    private bool mFireCancel = false;
    private float mCurTime;

    private float mCurEnergy;

    public static string GetWeaponEnergyKey(EnergyType type) {
        if(type == EnergyType.Unlimited || type == EnergyType.NumTypes)
            return null;

        return weaponEnergyPrefix + ((int)type);
    }

    public string iconSpriteRef { get { return _iconSpriteRef; } }
    public string labelText { get { return GameLocalize.GetText(_labelTextRef); } }

    public string energyTypeKey {
        get { return GetWeaponEnergyKey(energyType); }
    }

    public float currentEnergy {
        get { return mCurEnergy; }
        set {
            if(energyType != EnergyType.Unlimited) {
                float newVal = Mathf.Clamp(value, 0.0f, weaponEnergyDefaultMax);
                if(mCurEnergy != newVal) {
                    float prevVal = mCurEnergy;
                    mCurEnergy = newVal;

                    if(energyChangeCallback != null)
                        energyChangeCallback(this, mCurEnergy - prevVal);
                }
            }
        }
    }

    public bool isMaxEnergy {
        get { return mCurEnergy >= weaponEnergyDefaultMax; }
    }

    public bool canFire {
        get { return mCurProjCount < projMax && (energyType == EnergyType.Unlimited || mCurEnergy >= charges[mCurChargeLevel].energyCost); }
    }

    public bool isFireActive {
        get { return mFireActive; }
    }

    public Vector3 spawnPoint {
        get {
            Vector3 pt = _spawnPoint ? _spawnPoint.position : transform.position;
            pt.z = 0.0f;
            return pt;
        }
    }

    public Vector3 dir {
        get {
            if(_spawnPoint) {
                return new Vector3(Mathf.Sign(_spawnPoint.lossyScale.x), 0.0f, 0.0f);
            }

            return new Vector3(Mathf.Sign(transform.lossyScale.x), 0.0f, 0.0f);
        }
    }

    /// <summary>
    /// Call this to preserve energy when going to a new scene, usu. when you die
    /// </summary>
    public void SaveEnergySpent() {
        string key = energyTypeKey;
        if(!string.IsNullOrEmpty(key)) {
            SceneState.instance.SetGlobalValueFloat(key, mCurEnergy, false);
        }
    }

    public void ResetEnergySpent() {
        string key = energyTypeKey;
        if(!string.IsNullOrEmpty(key)) {
            SceneState.instance.SetGlobalValueFloat(key, weaponEnergyDefaultMax, false);
        }

        mCurEnergy = weaponEnergyDefaultMax;
    }

    public void FireStart() {
        if(canFire) {
            StopAllCoroutines();
            StartCoroutine(DoFire());
        }
    }

    public void FireStop() {
        mFireActive = false;
    }

    public void FireCancel() {
        mFireActive = false;
        mFireCancel = true;
    }

    public void ResetCharge() {
        if(mCurChargeLevel > 0)
            charges[mCurChargeLevel].Enable(false);

        mCurChargeLevel = 0;
        mCurTime = 0;
    }

    protected virtual Projectile CreateProjectile(int chargeInd, Transform seek) {
        Projectile ret = null;

        string type = charges[chargeInd].projType;
        if(!string.IsNullOrEmpty(type)) {
            ret = Projectile.Create(projGroup, type, spawnPoint, dir, seek);
            if(ret) {
                mCurProjCount++;
                ret.releaseCallback += OnProjRelease;

                //spend energy
                currentEnergy -= charges[chargeInd].energyCost;
            }
        }

        return ret;
    }

    protected virtual void OnEnable() {
        if(mStarted) {
            if(anim)
                anim.gameObject.SetActive(true);

            anim.Play(mClips[(int)AnimState.normal]);
        }
    }

    protected virtual void OnDisable() {
        if(anim)
            anim.gameObject.SetActive(false);

        if(mCurChargeLevel > 0) {
            charges[mCurChargeLevel].Enable(false);
        }

        mFireActive = false;
        mCurChargeLevel = 0;
        mFireCancel = false;
    }

    void OnDestroy() {
        if(anim)
            anim.AnimationCompleted -= OnAnimationClipEnd;

        energyChangeCallback = null;
    }

    protected virtual void Awake() {
        anim.AnimationCompleted += OnAnimationClipEnd;

        mClips = M8.tk2dUtil.GetSpriteClips(anim, typeof(AnimState));

        foreach(ChargeInfo inf in charges) {
            inf.Enable(false);
        }

        //get saved energy spent
        string key = energyTypeKey;
        if(!string.IsNullOrEmpty(key))
            mCurEnergy = SceneState.instance.GetGlobalValueFloat(key, weaponEnergyDefaultMax);
    }

    // Use this for initialization
    void Start() {
        mStarted = true;
        OnEnable();
    }

    IEnumerator DoFire() {
        anim.Stop();
        anim.Play(mClips[(int)AnimState.attack]);

        mCurChargeLevel = 0;

        //fire projectile
        if(canFire)
            CreateProjectile(mCurChargeLevel, null);

        //do charging

        if(charges.Length > 1) {
            mFireActive = true;

            mCurTime = 0.0f;
            WaitForFixedUpdate wait = new WaitForFixedUpdate();

            while(mFireActive) {
                //check if ready for next charge level
                int nextLevel = mCurChargeLevel + 1;
                if(nextLevel < charges.Length) {
                    //check if we can fire this charge
                    if(currentEnergy >= charges[nextLevel].energyCost) {
                        mCurTime += Time.fixedDeltaTime;
                        if(mCurTime >= charges[nextLevel].delay) {
                            //hide previous charge gameobject and activate/set new one
                            if(mCurChargeLevel > 0)
                                charges[mCurChargeLevel].Enable(false);

                            charges[nextLevel].Enable(true);

                            mCurChargeLevel = nextLevel;

                            //beginning first charge
                            if(mCurChargeLevel == 1)
                                anim.Play(mClips[(int)AnimState.charge]);
                        }
                    }
                    else {
                        //if we are only in level 0, then just stop
                        if(mCurChargeLevel == 0)
                            break;
                    }
                }

                yield return wait;
            }
        }

        //release charge?
        if(mCurChargeLevel > 0) {
            if(mFireCancel) {
                mFireCancel = false;
            }
            else {
                anim.Play(mClips[(int)AnimState.attack]);

                //spawn charged projectile
                CreateProjectile(mCurChargeLevel, null);
            }

            //reset charge
            charges[mCurChargeLevel].Enable(false);

            mCurChargeLevel = 0;
        }
    }

    //> AnimationCompleted
    void OnAnimationClipEnd(tk2dSpriteAnimator aAnim, tk2dSpriteAnimationClip aClip) {
        if(aAnim == anim && aClip == mClips[(int)AnimState.attack]) {
            anim.Play(mClips[(int)AnimState.normal]);
        }
    }

    protected virtual void OnProjRelease(EntityBase ent) {
        mCurProjCount = Mathf.Clamp(mCurProjCount - 1, 0, projMax);
        ent.releaseCallback -= OnProjRelease;
    }
}
