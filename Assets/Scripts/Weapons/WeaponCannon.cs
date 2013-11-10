using UnityEngine;
using System.Collections;

public class WeaponCannon : Weapon {
    public const string fireTake = "fire";

    public AnimatorData cannonAnimDat;

    public float rotateSpeed;
    public float rotateMin = -45;
    public float rotateMax = 45;

    private float mCurRot = 0.0f;

    public override bool canFire {
        get { return base.canFire && !cannonAnimDat.isPlaying; }
    }

    protected override Projectile CreateProjectile(int chargeInd, Transform seek) {
        Projectile ret = base.CreateProjectile(chargeInd, seek);

        if(ret) {
            cannonAnimDat.Play(fireTake);
        }

        return ret;
    }

    void Update() {
        if(Player.instance.inputEnabled) {
            Player.instance.controllerSprite.anim.Sprite.FlipX = false;

            float axisY = Main.instance.input.GetAxis(0, InputAction.MoveY);

            mCurRot += axisY * rotateSpeed * Time.deltaTime;
            if(mCurRot < rotateMin) mCurRot = rotateMin;
            else if(mCurRot > rotateMax) mCurRot = rotateMax;

            activeGO.transform.localRotation = Quaternion.AngleAxis(mCurRot, Vector3.forward);

            if(Player.instance.controllerSprite.isLeft)
                Player.instance.controllerSprite.anim.Sprite.FlipX = true;
        }
    }
}
