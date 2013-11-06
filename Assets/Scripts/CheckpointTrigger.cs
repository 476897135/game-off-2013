using UnityEngine;
using System.Collections;

public class CheckpointTrigger : MonoBehaviour {
    void OnTriggerEnter(Collider col) {
        LevelController.CheckpointSet(transform.position);
    }

    void OnDrawGizmos() {
        Color clr = Color.blue;

        Gizmos.color = clr;

        BoxCollider bc = collider != null ? collider as BoxCollider : null;
        if(bc != null) {
            Gizmos.DrawWireCube(transform.position + bc.center, new Vector3(bc.size.x * transform.localScale.x, bc.size.y * transform.localScale.y, bc.size.z * transform.localScale.z) * 0.5f);
        }

        clr.a = 0.5f;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}
