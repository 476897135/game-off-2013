using UnityEngine;
using System.Collections;

public class Stats : MonoBehaviour {
    public delegate void ChangeCallback(Stats stat, float delta);

    [System.Serializable]
    public class DamageReduceData {
        public Damage.Type type;
        public float reduction;
    }

    public float maxHP;

    public float damageReduction = 0.0f;

    public DamageReduceData[] damageTypeReduction;

    public event ChangeCallback changeHPCallback;

    private float mCurHP;
    private bool mIsInvul;

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

    /// <summary>
    /// This is the latest damage hit position when hp was reduced, set during ApplyDamage
    /// </summary>
    public Vector3 lastDamagePosition { get { return mLastDamagePos; } }

    /// <summary>
    /// This is the latest damage hit normal when hp was reduced, set during ApplyDamage
    /// </summary>
    public Vector3 lastDamageNormal { get { return mLastDamageNorm; } }

    public DamageReduceData GetDamageReduceData(Damage.Type type) {
        for(int i = 0, max = damageTypeReduction.Length; i < max; i++) {
            if(damageTypeReduction[i].type == type) {
                return damageTypeReduction[i];
            }
        }
        return null;
    }

    public bool ApplyDamage(Damage damage, Vector3 hitPos, Vector3 hitNorm) {
        mLastDamagePos = hitPos;
        mLastDamageNorm = hitNorm;

        if(!mIsInvul) {
            float amt = damage.amount;

            if(damageReduction > 0.0f) {
                amt -= amt * damageReduction;
            }

            DamageReduceData damageReduceByType = GetDamageReduceData(damage.type);
            if(damageReduceByType != null)
                amt -= amt * damageReduceByType.reduction;

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
    }

    protected virtual void OnDestroy() {
        changeHPCallback = null;
    }

    protected virtual void Awake() {
        mCurHP = maxHP;
    }


}
