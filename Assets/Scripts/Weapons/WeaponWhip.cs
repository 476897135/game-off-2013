using UnityEngine;
using System.Collections;

public class WeaponWhip : Weapon {
    public override bool canFire {
        get {
            return base.canFire && anim.CurrentClip == mClips[(int)AnimState.normal];
        }
    }

    public Transform whipStart;
    public Transform whipEnd;

    public LayerMask hitMask;

    private Damage mDmg;

    protected override void OnDestroy() {
        if(anim)
            anim.AnimationEventTriggered -= OnAnimEvent;

        base.OnDestroy();
    }

    protected override void Awake() {
        base.Awake();

        if(anim)
            anim.AnimationEventTriggered += OnAnimEvent;

        mDmg = GetComponent<Damage>();
    }

    void OnAnimEvent(tk2dSpriteAnimator aAnim, tk2dSpriteAnimationClip clip, int frame) {
        if(anim == aAnim && clip == mClips[(int)AnimState.attack]) {
            tk2dSpriteAnimationFrame frameDat = clip.GetFrame(frame);
            if(frameDat.eventInfo == "act") {
            }
        }
    }
}
