using UnityEngine;
using System.Collections;

public class Damage : MonoBehaviour {
    public const string DamageMessage = "OnDamage";

    public enum Type {
        Energy,
        Fire,
        Lightning,
        Explosion,
        Wind,

        NumType
    }

    public float amount;
    public Type type = Type.Energy;

    public void CallDamageTo(GameObject target, Vector3 hitPos, Vector3 hitNorm) {
        //target.SendMessage(DamageMessage, this, SendMessageOptions.DontRequireReceiver);
        Stats stat = target.GetComponent<Stats>();
        if(stat)
            stat.ApplyDamage(this, hitPos, hitNorm);
    }
}
