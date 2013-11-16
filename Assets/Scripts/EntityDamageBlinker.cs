using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Use to make entity blink when damaged
/// </summary>
public class EntityDamageBlinker : MonoBehaviour {
    public float blinkDelay = 0.2f;
    public float blinkInterval = 0.06f;
    public string modProperty = "_Mod";
    public bool invulOnBlink = false;

    private Renderer[] mRenderers;
    private Material[] mBlinkMats;
    private bool mBlinkOn;
    private bool mBlinkIntervalActive;

    private EntityBase mEnt;
    private Stats mStats;

    private bool mStarted;

    void OnDestroy() {
        if(mBlinkMats != null) {
            for(int i = 0, max = mBlinkMats.Length; i < max; i++) {
                DestroyImmediate(mBlinkMats[i]);
            }
            mBlinkMats = null;
        }
    }

    void OnEnable() {
        if(mStarted) {
            if(mEnt && mEnt.isBlinking)
                OnEntityBlink(mEnt, true);
        }
    }

    void OnDisable() {
        if(mStarted) {
            if(mEnt)
                OnEntityBlink(mEnt, false);
        }
    }

    void Awake() {
        Renderer[] renders = GetComponentsInChildren<Renderer>(true);
        if(renders.Length > 0) {
            List<Renderer> validRenders = new List<Renderer>(renders.Length);
            foreach(Renderer r in renders) {
                if(r.sharedMaterial.HasProperty(modProperty)) {
                    validRenders.Add(r);
                }
            }

            mRenderers = new Renderer[validRenders.Count];
            mBlinkMats = new Material[validRenders.Count];

            for(int i = 0, max = mBlinkMats.Length; i < max; i++) {
                mRenderers[i] = validRenders[i];

                validRenders[i].sharedMaterial = mBlinkMats[i] = new Material(validRenders[i].sharedMaterial);
                mBlinkMats[i].SetFloat(modProperty, 0.0f);
            }
        }

        mEnt = GetComponent<EntityBase>();
        mEnt.setBlinkCallback += OnEntityBlink;

        mStats = GetComponent<Stats>();
        if(mStats)
            mStats.changeHPCallback += OnStatsHPChange;

        tk2dBaseSprite[] sprites = GetComponentsInChildren<tk2dBaseSprite>(true);
        foreach(tk2dBaseSprite spr in sprites) {
            spr.SpriteChanged += OnSpriteChanged;
        }
    }

    void Start() {
        mStarted = true;
    }

    void OnStatsHPChange(Stats stat, float delta) {
        if(mEnt.gameObject.activeInHierarchy && stat.curHP > 0.0f && delta < 0.0f) {
            mEnt.Blink(blinkDelay);
        }
    }

    void OnEntityBlink(EntityBase ent, bool b) {
        if(b) {
            if(!mBlinkIntervalActive) {
                InvokeRepeating("DoBlinkInterval", 0.0f, blinkInterval);
                mBlinkIntervalActive = true;
            }
        }
        else {
            if(mBlinkIntervalActive) {
                CancelInvoke("DoBlinkInterval");
                mBlinkIntervalActive = false;
            }

            if(mBlinkOn)
                DoBlinkInterval();
        }

        if(invulOnBlink && mStats) {
            mStats.isInvul = b;
        }
    }

    void DoBlinkInterval() {
        if(mBlinkOn) {
            for(int i = 0, max = mBlinkMats.Length; i < max; i++) {
                mBlinkMats[i].SetFloat(modProperty, 0.0f);
            }
        }
        else {
            for(int i = 0, max = mBlinkMats.Length; i < max; i++) {
                mBlinkMats[i].SetFloat(modProperty, 1.0f);
            }
        }

        mBlinkOn = !mBlinkOn;
    }

    void OnSpriteChanged(tk2dBaseSprite spr) {
        for(int i = 0, max = mRenderers.Length; i < max; i++) {
            if(spr.renderer == mRenderers[i]) {
                mRenderers[i].sharedMaterial = mBlinkMats[i];
                break;
            }
        }
    }
}
