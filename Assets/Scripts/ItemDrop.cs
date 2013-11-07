using UnityEngine;
using System.Collections;

public class ItemDrop : MonoBehaviour {
    [System.Serializable]
    public class Data {
        public string itemSpawnType;
        public int weight = 1;
    }

    public string itemSpawnGroup = "item";

    public Data[] drops;
    public int range = 256;

    public Vector3 ofs;

    private EntityBase mEnt;
    private bool mDropActive;
    private int mDropRange;

    void Awake() {
        mEnt = GetComponent<EntityBase>();
        mEnt.setStateCallback += OnEntityState;

        mDropRange = 0;
        foreach(Data drop in drops)
            mDropRange += drop.weight;
    }

    void DoDrop() {
        if(string.IsNullOrEmpty(itemSpawnGroup))
            return;

        int r = Random.Range(0, range) + 1;

        if(r <= mDropRange) {
            string spawnType = null;

            for(int i = 0, max = drops.Length, w = 0; i < max; i++) {
                Data drop = drops[i];
                w += drop.weight;

                if(r <= w) {
                    spawnType = drop.itemSpawnType;
                    break;
                }
            }

            if(!string.IsNullOrEmpty(spawnType)) {
                Transform t = PoolController.Spawn(itemSpawnGroup, spawnType, spawnType, null, transform.position + ofs, Quaternion.identity);
                EntityBase ent = t.GetComponent<EntityBase>();
                ent.activator.deactivateOnStart = false;
            }
        }
    }

    void OnEntityState(EntityBase ent) {
        switch((EntityState)ent.state) {
            case EntityState.Normal:
                mDropActive = true;
                break;

            case EntityState.Dead:
                if(mDropActive) {
                    DoDrop();
                    mDropActive = false;
                }
                break;
        }
    }

    void OnDrawGizmosSelected() {
        Color clr = Color.red;
        clr.a = 0.5f;
        Gizmos.color = clr;
        Gizmos.DrawSphere(transform.position + ofs, 0.15f);
    }
}
