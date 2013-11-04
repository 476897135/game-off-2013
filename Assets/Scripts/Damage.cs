using UnityEngine;
using System.Collections;

public class Damage : MonoBehaviour {
    public const string DamageMessage = "OnDamage";

    public enum Type {
        Energy,
        Fire,
        Lightning,
        Explosion,
        Wind
    }

    public float amount;
    public Type type = Type.Energy;

    public void CallDamageTo(GameObject target) {
        target.SendMessage(DamageMessage, this, SendMessageOptions.DontRequireReceiver);
    }
}
