using UnityEngine;
using System.Collections;

/// <summary>
/// Assumes vertical, just rotate the base for horizontal.
/// </summary>
public class UIEnergyBar : MonoBehaviour {
    public UILabel label;
    public UISprite icon;

    public UISprite bar; //make sure anchor is bottom and it is in tile mode
    public int barHeight = 3; //height of each bar

    public UISprite panelTop; //make sure anchor is bottom
    public float panelTopYOfs;
    public UISprite panelBase; //make sure anchor is bottom

    private int mCurMaxBar = 1;
    private int mCurNumBar;

    public int max {
        get { return mCurMaxBar; }
        set {
            if(mCurMaxBar != value) {
                mCurMaxBar = value;
                RefreshHeight();
            }
        }
    }

    public int current {
        get { return mCurNumBar; }
        set {
            if(mCurNumBar != value) {
                mCurNumBar = value;
                RefreshBars();
            }
        }
    }

    public void SetIconSprite(string atlasRef) {
        if(icon) {
            icon.spriteName = atlasRef;
            icon.MakePixelPerfect();
        }
    }

    public void SetBarSprite(string atlasRef) {
        bar.spriteName = atlasRef;
    }

    public void SetBarColor(Color clr) {
        bar.color = clr;
    }

    void RefreshHeight() {
        int h = barHeight * mCurMaxBar;

        if(panelBase) {
            panelBase.height = h;
        }

        if(panelTop) {
            Vector3 topPos = new Vector3(0, h + panelTopYOfs, 0);
            panelTop.transform.localPosition = topPos;
        }
    }

    void RefreshBars() {
        if(mCurNumBar == 0)
            bar.gameObject.SetActive(false);
        else {
            bar.gameObject.SetActive(true);
            bar.height = mCurNumBar*barHeight;
        }
    }
}
