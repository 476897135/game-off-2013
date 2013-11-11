using UnityEngine;
using System.Collections;

public class TimeWarpField : MonoBehaviour {
    public const int maxCount = 8;

    public class Data {
        public Collider col;
        public Projectile proj; //if not null, then use its speed limit
        public RigidBodyController bodyCtrl; //if not null, then use its speed limit
        public TransAnimSpinner spinner;
        public AnimatorData animDat; //if not null, then use its time scale

        public void Init(Collider aCol, float scale) {
            col = aCol;

            //global rigidbody modify velocity to scale
            if(col.rigidbody != null) {
                Vector3 v = col.rigidbody.velocity;
                float mag = v.magnitude;
                if(mag > 0.0f) {
                    col.rigidbody.velocity = (v / mag) * (mag * 0.3f);
                }
            }

            proj = aCol.GetComponent<Projectile>();
            if(proj) {
                proj.moveScale = scale;
                return;
            }

            bodyCtrl = aCol.GetComponent<RigidBodyController>();
            if(bodyCtrl) {
                return;
            }

            spinner = aCol.GetComponent<TransAnimSpinner>();
            if(spinner) {
                spinner.speedScale = scale;
                return;
            }
        }

        public void Restore() {
            if(col != null && col.gameObject.activeInHierarchy) {
                if(proj) {
                    proj.moveScale = 1.0f;
                    proj = null;
                }
                else if(spinner) {
                    spinner.speedScale = 1.0f;
                    spinner = null;
                }
            }

            col = null;
        }
    }

    public float radius = 1.0f;
    public LayerMask masks;
    public float scale = 0.3f;

    private bool mStarted;
    private bool mUpdateActive;

    private int mCount;
    private Data[] mItems;
        
    void OnEnable() {
        if(mStarted && !mUpdateActive) {
            StartCoroutine(DoUpdate());
        }
    }

    void OnDisable() {
        mUpdateActive = false;

        if(mStarted) {
            for(int i = 0; i < mCount; i++) {
                mItems[i].Restore();
            }
            mCount = 0;
        }
    }

    void Awake() {
        mItems = new Data[maxCount];
        for(int i = 0; i < maxCount; i++) {
            mItems[i] = new Data();
        }
    }

    void Start() {
        mStarted = true;
        OnEnable();
    }

    IEnumerator DoUpdate() {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while(mUpdateActive) {
            yield return wait;

            //
            Collider[] cols = Physics.OverlapSphere(transform.position, radius, masks);
            int colCount = cols.Length;

            //check to see if colliders are already in data
            //if a collider in our data is not in the list, remove it
            for(int i = 0; i < mCount; i++) {
                Data item = mItems[i];

                bool doRemove = true;

                if(item.col != null && item.col.gameObject.activeInHierarchy) {
                    //check if it's in collisions
                    for(int j = 0; j < colCount; j++) {
                        if(item.col == cols[j]) {
                            //remove from cols, already in our items
                            if(colCount > 1) {
                                cols[j] = cols[colCount - 1];
                                cols[colCount - 1] = null;
                                colCount--;
                            }
                            
                            doRemove = false;
                            break;
                        }
                    }
                }

                if(doRemove) {
                    item.Restore();

                    if(mCount > 1) {
                        mItems[i] = mItems[mCount - 1];
                        mItems[mCount - 1] = item;
                    }

                    mCount--;
                    i--;
                }
            }

            //add new items
            for(int i = 0; i < colCount && mCount < mItems.Length; i++) {
                mItems[mCount].Init(cols[i], scale);
                mCount++;
            }
        }
    }

    void OnDrawGizmos() {
        if(radius > 0.0f) {
            Gizmos.color = Color.magenta * 0.5f;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
