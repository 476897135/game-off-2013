using UnityEngine;
using System.Collections;

public class ItemPickup : EntityBase {
    public const float destroyDelay = 3.0f;
    public const float destroyStartBlinkDelay = destroyDelay * 0.7f;

    public ItemType type;
    public int bit; //which bit in the flag, used by certain types
    public float value; //used by certain types
    public string sound;

    public LayerMask dropLayerMask; //which layers the drop will stop when hit
    public float dropSpeed;

    private bool mDropActive;
    private float mRadius;
    private bool mSpawned;

    private SpriteColorBlink[] mBlinkers;

    void OnTriggerStay(Collider col) {
        Player player = col.GetComponent<Player>();
        if(player && player.state != (int)EntityState.Dead && player.state != (int)EntityState.Invalid) {
            switch(type) {
                case ItemType.Health:
                    if(player.stats.curHP < player.stats.maxHP)
                        player.stats.curHP += value;
                    else {
                        //TODO: life tank
                    }
                    break;

                case ItemType.Energy:
                    Weapon wpn = null;
                    if(player.currentWeaponIndex == 0) {
                        wpn = player.lowestEnergyWeapon;
                    }
                    else
                        wpn = player.currentWeapon;

                    if(wpn) {
                        wpn.currentEnergy += value;
                    }
                    else {
                        //TODO: energy tank
                    }
                    break;

                case ItemType.Life:
                    PlayerStats.curLife++;
                    break;

                case ItemType.HealthUpgrade:
                    PlayerStats.AddHPMod(bit);
                    break;

                case ItemType.HealthTank:
                    break;

                case ItemType.EnergyTank:
                    break;

                case ItemType.Armor:
                    break;
            }

            if(!string.IsNullOrEmpty(sound)) {
                SoundPlayerGlobal.instance.Play(sound);
            }

            if(collider)
                collider.enabled = false;

            //TODO: dialog?

            Release();
        }
    }

    protected override void ActivatorSleep() {
        base.ActivatorSleep();

        if(mSpawned) {
            Release();
        }
    }

    protected override void OnDespawned() {
        //reset stuff here
        if(collider)
            collider.enabled = true;

        mSpawned = false;
        mDropActive = false;
        
        foreach(SpriteColorBlink blinker in mBlinkers) {
            blinker.enabled = false;
        }

        CancelInvoke();

        base.OnDespawned();
    }

    protected override void OnDestroy() {
        //dealloc here

        base.OnDestroy();
    }

    public override void SpawnFinish() {
        //start ai, player control, etc
        mSpawned = true;

        StartCoroutine(DoDrop());
        Invoke("Release", destroyDelay);
        Invoke("DoBlinkers", destroyStartBlinkDelay);
    }

    protected override void SpawnStart() {
        //initialize some things
    }

    protected override void Awake() {
        base.Awake();

        //initialize variables
        if(collider) {
            SphereCollider sphr = collider as SphereCollider;
            if(sphr) {
                mRadius = sphr.radius;
            }
            else {
                mRadius = collider.bounds.extents.y;
            }
        }

        mBlinkers = GetComponentsInChildren<SpriteColorBlink>(true);
        foreach(SpriteColorBlink blinker in mBlinkers) {
            blinker.enabled = false;
        }

        autoSpawnFinish = true;
    }

    // Use this for initialization
    protected override void Start() {
        base.Start();

        //initialize variables from other sources (for communicating with managers, etc.)
    }

    IEnumerator DoDrop() {
        mDropActive = true;

        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        while(mDropActive) {
            yield return wait;

            float moveY = dropSpeed * Time.fixedDeltaTime;
            Vector3 pos = transform.position;

            RaycastHit hit;
            if(Physics.SphereCast(collider.bounds.center, mRadius, Vector3.down, out hit, moveY, dropLayerMask)) {
                pos = hit.point + hit.normal * mRadius;
                mDropActive = false;
            }
            else {
                pos.y -= moveY;
            }

            transform.position = pos;
        }
    }

    void DoBlinkers() {
        foreach(SpriteColorBlink blinker in mBlinkers) {
            blinker.enabled = true;
        }
    }
}
