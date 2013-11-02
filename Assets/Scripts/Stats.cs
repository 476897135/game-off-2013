using UnityEngine;
using System.Collections;

public class Stats : MonoBehaviour {
    public delegate void ChangeCallback(Stats stat, float prevVal);

    public int maxHP;

    public event ChangeCallback changeHPCallback;

    private int mCurHP;

    public int curHP {
        get { return mCurHP; }

        set {
            int v = Mathf.Clamp(value, 0, maxHP);
            if(mCurHP != v) {
                int prev = mCurHP;
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
