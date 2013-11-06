using UnityEngine;
using System.Collections;

/// <summary>
/// Player specific stats
/// </summary>
public class PlayerStats : Stats {
    public const string hpModFlagsKey = "playerHPMod";
    public const int hpModCount = 8;
    public const float hpMod = 2;
    public const int defaultNumLives = 3;

    public const string lifeCountKey = "playerLife";

    public const string weaponFlagsKey = "playerWeapons";
        
    public event ChangeCallback changeMaxHPCallback;

    private float mDefaultMaxHP;
        
    public static int curLife {
        get {
            return SceneState.instance.GetGlobalValue(lifeCountKey);
        }

        set {
            SceneState.instance.SetGlobalValue(lifeCountKey, Mathf.Clamp(value, 0, 99), false);
        }
    }

    public bool IsWeaponAvailable(int index) {
        return SceneState.instance.CheckGlobalFlag(weaponFlagsKey, index);
    }
        
    protected override void OnDestroy() {
        if(SceneState.instance) {
            SceneState.instance.onValueChange -= OnSceneStateValue;
        }

        changeMaxHPCallback = null;

        base.OnDestroy();
    }

    protected override void Awake() {
        mDefaultMaxHP = maxHP;

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

        float newMaxHP = mDefaultMaxHP + numMod * hpMod;

        if(maxHP != newMaxHP) {
            float prevMaxHP = maxHP;

            maxHP = newMaxHP;

            if(changeMaxHPCallback != null) {
                changeMaxHPCallback(this, maxHP - prevMaxHP);
            }
        }
    }

    void OnSceneStateValue(bool isGlobal, string name, SceneState.StateValue val) {
        if(isGlobal && name == hpModFlagsKey) {
            ApplyHPMod();
        }
    }
}
