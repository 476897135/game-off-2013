using UnityEngine;
using System.Collections;

public class Stats : MonoBehaviour {
    public delegate void ChangeCallback(Stats stat, float prevVal);

    public float maxHP;

    public event ChangeCallback changeHPCallback;

    private float mCurHP;

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

    public virtual void Reset() {
        curHP = maxHP;
    }

    protected virtual void OnDestroy() {
        changeHPCallback = null;
    }

    protected virtual void Awake() {
        mCurHP = maxHP;
    }


}
