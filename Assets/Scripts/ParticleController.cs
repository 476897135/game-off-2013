using UnityEngine;
using System.Collections;

public class ParticleController : MonoBehaviour {
    public bool playOnEnable;
    public bool stopOnDisable;
    public bool clearOnStop;

    private bool mStarted;

    public void Play(bool withChildren) {
        particleSystem.Play(withChildren);
    }

    public void Stop() {
        particleSystem.Stop();
    }

    public void Pause() {
        particleSystem.Pause();
    }

    public void SetLoop(bool loop) {
        particleSystem.loop = loop;
    }

    void OnEnable() {
        if(mStarted && playOnEnable && !particleSystem.isPlaying)
            particleSystem.Play();
    }

    void OnDisable() {
        if(mStarted && stopOnDisable) {
            particleSystem.Stop();

            if(clearOnStop)
                particleSystem.Clear();
        }
    }

    void Start() {
        mStarted = true;
        if(playOnEnable)
            particleSystem.Play();
    }
}
