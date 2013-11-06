using UnityEngine;
using System.Collections;

public class Stats : MonoBehaviour {
    public delegate void ChangeCallback(Stats stat, float prevVal);

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

    public float curHP {
        get { return mCurHP; }

        set {
            float v = Mathf.Clamp(value, 0, maxHP);
            if(mCurHP != v) {
                float prev = mCurHP;
                mCurHP = v;

                if(changeHPCallback != null)
                    changeHPCallback(this, prev);
            }
        }
    }

    public bool isInvul { get { return mIsInvul; } set { mIsInvul = value; } }

    public void ApplyDamage(Damage damage) {
        if(!mIsInvul) {
            float amt = damage.amount;

            if(damageReduction > 0.0f) {
                amt -= amt * damageReduction;
            }

            for(int i = 0, max = damageTypeReduction.Length; i < max; i++) {
                DamageReduceData dat = damageTypeReduction[i];
                if(dat.type == damage.type) {
                    amt -= amt * dat.reduction;
                    break;
                }
            }

            if(amt > 0.0f)
                curHP -= amt;
        }
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
