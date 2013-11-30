using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour {
    public UIEnergyBar barHP;
    public UIEnergyBar barEnergy;
    public UIEnergyBar barBoss;
    public UILabel lifeCountLabel;

    private static HUD mInstance;

    public static HUD instance { get { return mInstance; } }

    public void RefreshLifeCount() {
        lifeCountLabel.text = PlayerStats.curLife.ToString();
    }

    void OnDestroy() {
        if(mInstance == this) {
            mInstance = null;
        }
    }

    void Awake() {
        if(mInstance == null) {
            mInstance = this;
        }
    }

    // Use this for initialization
    void Start() {

    }
}
