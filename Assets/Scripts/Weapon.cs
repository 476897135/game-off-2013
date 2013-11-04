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
    }

    public tk2dSpriteAnimator anim;

    public string projGroup = "proj";
    public string projType;
    public int projMax = 4;

    public ChargeInfo[] charges;

    public Transform spawnPoint;
        
    private tk2dSpriteAnimationClip[] mClips;

    private bool mFireActive = false;
    private int mCurChargeLevel = -1;
    private int mCurProjCount = 0;
    private bool mStarted = false;

    public bool canFire {
        get { return mCurProjCount < projMax; }
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

    void OnEnable() {
        if(mStarted) {
            if(anim)
                anim.gameObject.SetActive(true);

            anim.Play(mClips[(int)AnimState.normal]);
        }
    }

    void OnDisable() {
        if(anim)
            anim.gameObject.SetActive(false);

        if(mCurChargeLevel >= 0) {
            if(charges[mCurChargeLevel].go)
                charges[mCurChargeLevel].go.SetActive(false);
        }

        mFireActive = false;
        mCurChargeLevel = -1;
    }

    void OnDestroy() {
        if(anim)
            anim.AnimationCompleted -= OnAnimationClipEnd;
    }

    void Awake() {
        anim.AnimationCompleted += OnAnimationClipEnd;

        mClips = M8.tk2dUtil.GetSpriteClips(anim, typeof(AnimState));

        foreach(ChargeInfo inf in charges) {
            if(inf.go)
                inf.go.SetActive(false);
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

        //fire projectile

        //do charging
        mCurChargeLevel = -1;

        if(charges.Length > 0) {
            mFireActive = true;

            float mCurTime = 0.0f;
            WaitForFixedUpdate wait = new WaitForFixedUpdate();

            while(mFireActive) {
                //check if ready for next charge level
                int nextLevel = mCurChargeLevel + 1;
                if(nextLevel < charges.Length) {
                    mCurTime += Time.fixedDeltaTime;
                    if(mCurTime >= charges[nextLevel].delay) {
                        //hide previous charge gameobject and activate/set new one
                        if(mCurChargeLevel >= 0 && charges[mCurChargeLevel].go)
                            charges[mCurChargeLevel].go.SetActive(false);

                        charges[nextLevel].go.SetActive(true);

                        mCurChargeLevel = nextLevel;

                        //beginning first charge
                        if(mCurChargeLevel == 0)
                            anim.Play(mClips[(int)AnimState.charge]);
                    }
                }

                yield return wait;
            }
        }

        //release charge?
        if(mCurChargeLevel >= 0) {
            anim.Play(mClips[(int)AnimState.attack]);

            //spawn charged projectile
            string projChargeType = charges[mCurChargeLevel].projType;
                        
            //reset charge
            if(charges[mCurChargeLevel].go)
                charges[mCurChargeLevel].go.SetActive(false);

            mCurChargeLevel = -1;
        }
    }

    //> AnimationCompleted
    void OnAnimationClipEnd(tk2dSpriteAnimator aAnim, tk2dSpriteAnimationClip aClip) {
        if(aAnim == anim && aClip == mClips[(int)AnimState.attack]) {
            anim.Play(mClips[(int)AnimState.normal]);    
        }
    }
}
