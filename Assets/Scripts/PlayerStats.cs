using UnityEngine;
using System.Collections;

/// <summary>
/// Player specific stats
/// </summary>
public class PlayerStats : Stats {
    public const string hpModFlagsKey = "playerHPMod";
    public const int hpModCount = 8;
    public const int hpMod = 2;

    public const string lifeCountKey = "playerLife";
        
    public event ChangeCallback changeMaxHPCallback;

    public int curLife {
        get {
            return SceneState.instance.GetGlobalValue(lifeCountKey);
        }

        set {
            SceneState.instance.SetGlobalValue(lifeCountKey, Mathf.Clamp(value, 0, 99), false);
        }
    }
        
    protected override void OnDestroy() {
        if(SceneState.instance) {
            SceneState.instance.onValueChange -= OnSceneStateValue;
        }

        changeMaxHPCallback = null;

        base.OnDestroy();
    }

    protected override void Awake() {
        SceneState.instance.onValueChange += OnSceneStateValue;

        ApplyHPMod();

        base.Awake();
    }

    void ApplyHPMod() {
        //change max hp for any upgrade
        int numMod = 0;

        //get hp mod flags
        int hpModFlags = SceneState.instance.GetGlobalValue(hpModFlagsKey);
        for(int i = 0, check = 1; i < hpModCount; i++, check <<= 1) {
            if((hpModFlags & check) != 0)
                numMod++;
        }

        int newMaxHP = numMod * hpMod;

        if(maxHP != newMaxHP) {
            int prevMaxHP = maxHP;

            maxHP = newMaxHP;

            if(changeMaxHPCallback != null) {
                changeMaxHPCallback(this, prevMaxHP);
            }
        }
    }

    void OnSceneStateValue(bool isGlobal, string name, SceneState.StateValue val) {
        if(isGlobal && name == hpModFlagsKey) {
            ApplyHPMod();
        }
    }
}
