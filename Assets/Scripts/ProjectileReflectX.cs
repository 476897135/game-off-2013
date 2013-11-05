using UnityEngine;
using System.Collections;

public class ProjectileReflectX : Projectile {
    public float angleCriteria = 170.0f; //the check for current force direction to normal

    protected override void ApplyContact(GameObject go, Vector3 normal) {
        base.ApplyContact(go, normal);

        if(Vector3.Angle(mActiveForce, normal) >= angleCriteria) {
            mActiveForce.x *= -1.0f;
        }
    }
}
