using UnityEngine;
using System.Collections;

public class Stats : MonoBehaviour {
    public delegate void ChangeCallback(Stats stat, float delta);

    [System.Serializable]
    public class DamageMod {
        public Damage.Type type;
        public float val;
    }

    public float maxHP;

    public float damageReduction = 0.0f;

    public DamageMod[] damageTypeAmp;
    public DamageMod[] damageTypeReduction;

    public bool stunImmune;

    public LayerMask deathMask;

    public event ChangeCallback changeHPCallback;

    private float mCurHP;
    private bool mIsInvul;

    private Damage mLastDamage;
    private Vector3 mLastDamagePos;
    private Vector3 mLastDamageNorm;

    public float curHP {
        get { return mCurHP; }

        set {
            float v = Mathf.Clamp(value, 0, maxHP);
            if(mCurHP != v) {
                float prev = mCurHP;
                mCurHP = v;

                if(changeHPCallback != null)
                    changeHPCallback(this, mCurHP - prev);
            }
        }
    }

    public bool isInvul { get { return mIsInvul; } set { mIsInvul = value; } }

    public Damage lastDamageSource { get { return mLastDamage; } }

    /// <summary>
    /// This is the latest damage hit position when hp was reduced, set during ApplyDamage
    /// </summary>
    public Vector3 lastDamagePosition { get { return mLastDamagePos; } }

    /// <summary>
    /// This is the latest damage hit normal when hp was reduced, set during ApplyDamage
    /// </summary>
    public Vector3 lastDamageNormal { get { return mLastDamageNorm; } }

    public DamageMod GetDamageMod(DamageMod[] dat, Damage.Type type) {
        for(int i = 0, max = dat.Length; i < max; i++) {
            if(dat[i].type == type) {
                return dat[i];
            }
        }
        return null;
    }

    public bool ApplyDamage(Damage damage, Vector3 hitPos, Vector3 hitNorm) {
        mLastDamage = damage;
        mLastDamagePos = hitPos;
        mLastDamageNorm = hitNorm;

        if(!mIsInvul) {
            float amt = damage.amount;

            if(damageReduction > 0.0f) {
                amt -= amt * damageReduction;
            }

            DamageMod damageAmpByType = GetDamageMod(damageTypeReduction, damage.type);
            if(damageAmpByType != null) {
                amt += amt * damageAmpByType.val;
            }
            else {
                DamageMod damageReduceByType = GetDamageMod(damageTypeReduction, damage.type);
                if(damageReduceByType != null)
                    amt -= amt * damageReduceByType.val;
            }

            if(amt > 0.0f) {
                curHP -= amt;
                return true;
            }
        }

        return false;
    }

    public virtual void Reset() {
        curHP = maxHP;
        mIsInvul = false;
        mLastDamage = null;
    }

    protected virtual void OnDestroy() {
        changeHPCallback = null;
    }

    protected virtual void Awake() {
        mCurHP = maxHP;
    }

    void OnCollisionEnter(Collision col) {
        if(((1 << col.gameObject.layer) & deathMask) != 0) {
            curHP = 0;
        }
    }

    void OnTriggerEnter(Collider col) {
        if(((1 << col.gameObject.layer) & deathMask) != 0) {
            curHP = 0;
        }
    }
}
