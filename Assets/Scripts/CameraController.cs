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
    public float delay = 0.1f; //reposition delay
    public float transitionDelay = 0.5f;
    public float transitionExpire = 1.0f;

    private static CameraController mInstance;

    private tk2dCamera mCam;
    private Transform mAttach;
    private Vector3 mCurVel;
    private Bounds mBounds;
    private bool mDoTrans;
    private float mLastTransTime;
    private float mCurDelay;

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
            mCurVel = Vector3.zero;
        }
    }

    public void SetTransition(bool transition) {
        mDoTrans = transition;
        mLastTransTime = Time.time;
        mCurVel = Vector3.zero;
        mCurDelay = transitionDelay;
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;

            //init stuff
            mCam = GetComponentInChildren<tk2dCamera>();

            mCurDelay = delay;
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
        if(mode == Mode.Lock)
            return;

        if(mDoTrans) {
            float curT = Time.time - mLastTransTime;
            if(curT >= transitionExpire) {
                mDoTrans = false;
                mCurDelay = delay;
            }
            else {
                float t = Mathf.Clamp(curT/transitionExpire, 0.0f, 1.0f);
                mCurDelay = Mathf.Lerp(transitionDelay, delay, t);
            }
        }

        Vector3 curPos = transform.position;
        Vector3 dest = mAttach ? mAttach.position : curPos;

        //apply bounds
        switch(mode) {
            case Mode.HorizontalLock:
                ApplyBounds(ref dest);
                dest.x = bounds.center.x;
                break;

            case Mode.VerticalLock:
                ApplyBounds(ref dest);
                dest.y = bounds.center.y;
                break;

            default:
                ApplyBounds(ref dest);
                break;
        }

        if(curPos != dest) {
            transform.position = Vector3.SmoothDamp(curPos, dest, ref mCurVel, mCurDelay, Mathf.Infinity, Time.deltaTime);
        }
    }

    void ApplyBounds(ref Vector3 pos) {
        if(bounds.size.x > 0.0f && bounds.size.y > 0.0f) {
            Rect screen = mCam.ScreenExtents;

            if(pos.x - screen.width * 0.5f < bounds.min.x)
                pos.x = bounds.min.x + screen.width * 0.5f;
            else if(pos.x + screen.width * 0.5f > bounds.max.x)
                pos.x = bounds.max.x - screen.width * 0.5f;

            if(pos.y - screen.height * 0.5f < bounds.min.y)
                pos.y = bounds.min.y + screen.height * 0.5f;
            else if(pos.y + screen.height * 0.5f > bounds.max.y)
                pos.y = bounds.max.y - screen.height * 0.5f;
        }
    }
}
