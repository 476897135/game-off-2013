using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
    public enum Mode {
        Lock, //stop camera motion by controller
        Free, //camera follows attach, restricted by bounds
        HorizontalLock, //camera X is forced at center of bounds, Y still follows attach
        VerticalLock, //camera Y is forced at center of bounds, X still follows attach
    }

    public Mode mode = Mode.Lock;
    public float eyePositionDelay = 0.1f; //reposition delay

    private static CameraController mInstance;

    private Transform mAttach;
    private Vector3 mCurVel;
    private Bounds mBounds;

    public static CameraController instance { get { return mInstance; } }

    public Transform attach {
        get { return mAttach; }
        set {
            if(mAttach != value) {
                mAttach = value;

                mCurVel = Vector3.zero;
            }
        }
    }

    public Bounds bounds {
        get { return mBounds; }
        set {
            mBounds = value;
        }
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            //init stuff
        }
        else {
            DestroyImmediate(gameObject);
        }
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }
}
