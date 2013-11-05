using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
    public enum AnimState {
        normal,
        attack,
        charge
    }

    [System.Serializable]
    public class ChargeInfo {
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

    public tk2dSpriteAnimator anim;

    public string projGroup = "proj";
    public int projMax = 4;

    //first charge is the regular fire
    public ChargeInfo[] charges;

    [SerializeField]
    Transform _spawnPoint;
        
    private tk2dSpriteAnimationClip[] mClips;

    private bool mFireActive = false;
    private int mCurChargeLevel = 0;
    protected int mCurProjCount = 0;
    private bool mStarted = false;
    private bool mFireCancel = false;
    private float mCurTime;

    public bool canFire {
        get { return mCurProjCount < projMax; }
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
    }

    protected virtual void Awake() {
        anim.AnimationCompleted += OnAnimationClipEnd;

        mClips = M8.tk2dUtil.GetSpriteClips(anim, typeof(AnimState));

        foreach(ChargeInfo inf in charges) {
            inf.Enable(false);
        }
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
