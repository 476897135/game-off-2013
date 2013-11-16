using UnityEngine;
using System.Collections;

/// <summary>
/// Note: make sure to use rigidbody and with gravity
/// </summary>
public class ProjectileArc : Projectile {
    public float farVelocity;
    public float farDistance;

    public float nearVelocity;
    public float nearDistance;

    protected override void StateChanged() {
        base.StateChanged();

        switch((State)state) {
            case State.Seek:
                Vector3 pos = collider.bounds.center;
                Vector3 target = mSeek.collider ? mSeek.collider.bounds.center : mSeek.position;
                if(target.x != pos.x) {
                    float vel = seekVelocity;
                                       
                    float x = target.x - pos.x;
                    float y = target.y - pos.y;

                    float distSqr = y < 0.0f ? x*x : (target - pos).sqrMagnitude;
                    if(distSqr > farDistance * farDistance)
                        vel = farVelocity;
                    else if(distSqr < nearDistance * nearDistance)
                        vel = nearVelocity;

                    //determine angle
                    GravityController gctrl = GetComponent<GravityController>();
                    float grav = Mathf.Abs(gctrl != null ? gctrl.gravity : Physics.gravity.magnitude);
                    float vSqr = vel * vel;

                    float theta = Mathf.Atan((vSqr + Mathf.Sqrt(vSqr * vSqr + grav * (grav * x * x + 2 * y * vSqr))) / (grav * x));
                    if(float.IsNaN(theta)) {
                        theta = Mathf.PI * 0.5f;
                    }

                    mDir.Set(Mathf.Sign(x), 0, 0);
                    mDir = Quaternion.AngleAxis(Mathf.Rad2Deg * theta, Vector3.forward) * mDir;

                    rigidbody.velocity = mDir * vel;// .AddForce(mDir * vel, ForceMode.VelocityChange);
                    //Debug.Log("theta: " + (Mathf.Rad2Deg * theta));
                }
                else {
                    rigidbody.AddForce(new Vector3(0.0f, Mathf.Sign(target.y - pos.y), 0.0f) * seekVelocity, ForceMode.VelocityChange);
                }

                seek = null;
                mActiveForce = Vector3.zero;
                break;
        }
    }
}
