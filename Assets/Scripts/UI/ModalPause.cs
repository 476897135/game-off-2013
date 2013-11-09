using UnityEngine;
using System.Collections;

public class ModalPause : UIController {
    public Transform weaponsHolder;

    public UIEnergyBar hpBar;

    public UILabel lifeLabel;

    public GameObject energySubTankBar1;
    public UISprite energySubTankBar1Fill;

    public GameObject energySubTankBar2;
    public UISprite energySubTankBar2Fill;

    public GameObject weaponSubTankBar1;
    public UISprite weaponSubTankBar1Fill;

    public GameObject weaponSubTankBar2;
    public UISprite weaponSubTankBar2Fill;

    public UIEventListener energySubTank;
    public UIEventListener weaponSubTank;

    public UIEventListener exit;
    public UIEventListener options;

    private UIEnergyBar[] mWeapons;

    private int mInputLockCounter;
    private int mNumEnergyTank;
    private int mNumWeaponTank;

    private int mEnergySubTankBar1FillW;
    private int mEnergySubTankBar2FillW;
    private int mWeaponSubTankBar1FillW;
    private int mWeaponSubTankBar2FillW;

    protected override void OnActive(bool active) {
        if(active) {
            InitHP();
            InitSubTanks();
            InitWeapons();

            //life
            lifeLabel.text = "x" + PlayerStats.curLife;

            exit.onClick = OnExitClick;
            options.onClick = OnOptionsClick;
        }
        else {
            for(int i = 0, max = mWeapons.Length; i < max; i++) {
                UIEventListener eventListener = mWeapons[i].GetComponent<UIEventListener>();
                eventListener.onClick = null;
            }

            energySubTank.onClick = null;
            weaponSubTank.onClick = null;

            exit.onClick = null;
            options.onClick = null;

            mInputLockCounter = 0;
        }
    }

    protected override void OnOpen() {

    }

    protected override void OnClose() {

    }

    void Awake() {
        mWeapons = weaponsHolder.GetComponentsInChildren<UIEnergyBar>(true);
        System.Array.Sort(mWeapons,
            delegate(UIEnergyBar bar1, UIEnergyBar bar2) {
                return bar1.name.CompareTo(bar2.name);
            });

        mEnergySubTankBar1FillW = energySubTankBar1Fill.width;
        mEnergySubTankBar2FillW = energySubTankBar2Fill.width;
        mWeaponSubTankBar1FillW = weaponSubTankBar1Fill.width;
        mWeaponSubTankBar2FillW = weaponSubTankBar2Fill.width;
    }

    void InitSubTanks() {
        UIButtonKeys energyBtnKeys = energySubTank.GetComponent<UIButtonKeys>();
        UIButtonKeys weaponBtnKeys = weaponSubTank.GetComponent<UIButtonKeys>();
        UIButtonKeys exitBtnKeys = exit.GetComponent<UIButtonKeys>();
        UIButtonKeys optionsBtnKeys = options.GetComponent<UIButtonKeys>();

        mNumEnergyTank = 0;
        if(PlayerStats.isSubTankEnergy1Acquired) mNumEnergyTank++;
        if(PlayerStats.isSubTankEnergy2Acquired) mNumEnergyTank++;

        if(mNumEnergyTank > 0) {
            energySubTank.onClick = OnEnergySubTankClick;
            energySubTankBar1.SetActive(mNumEnergyTank >= 1);
            energySubTankBar2.SetActive(mNumEnergyTank > 1);
        }
        else {
            energySubTankBar1.SetActive(false);
            energySubTankBar2.SetActive(false);
        }

        mNumWeaponTank = 0;
        if(PlayerStats.isSubTankWeapon1Acquired) mNumWeaponTank++;
        if(PlayerStats.isSubTankWeapon2Acquired) mNumWeaponTank++;

        if(mNumWeaponTank > 0) {
            weaponSubTank.onClick = OnWeaponSubTankClick;
            weaponSubTankBar1.SetActive(mNumWeaponTank >= 1);
            weaponSubTankBar2.SetActive(mNumWeaponTank > 1);
        }
        else {
            weaponSubTankBar1.SetActive(false);
            weaponSubTankBar2.SetActive(false);
        }

        energyBtnKeys.selectOnDown = mNumWeaponTank > 0 ? weaponBtnKeys : exitBtnKeys;

        weaponBtnKeys.selectOnUp = mNumEnergyTank > 0 ? energyBtnKeys : optionsBtnKeys;

        exitBtnKeys.selectOnUp =
            mNumWeaponTank > 0 ? weaponBtnKeys :
                mNumEnergyTank > 0 ? energyBtnKeys :
                    optionsBtnKeys;

        optionsBtnKeys.selectOnDown =
            mNumEnergyTank > 0 ? energyBtnKeys :
                mNumWeaponTank > 0 ? weaponBtnKeys :
                    exitBtnKeys;

        RefreshEnergyTank();
        RefreshWeaponTank();
    }

    void InitWeapons() {
        Player player = Player.instance;

        UIButtonKeys firstWeaponButtonKeys = null;
        UIButtonKeys lastWeaponButtonKeys = null;
        UIButtonKeys rightButtonKeys = null;

        if(PlayerStats.isSubTankEnergy1Acquired || PlayerStats.isSubTankEnergy2Acquired)
            rightButtonKeys = energySubTank.GetComponent<UIButtonKeys>();
        else if(PlayerStats.isSubTankWeapon1Acquired || PlayerStats.isSubTankWeapon2Acquired)
            rightButtonKeys = weaponSubTank.GetComponent<UIButtonKeys>();
        else
            rightButtonKeys = exit.GetComponent<UIButtonKeys>();

        for(int i = 0, max = mWeapons.Length; i < max; i++) {
            UIEnergyBar wpnUI = mWeapons[i];
            Weapon wpn = i < player.weapons.Length ? player.weapons[i] : null;

            UIEventListener eventListener = wpnUI.GetComponent<UIEventListener>();

            if(PlayerStats.IsWeaponAvailable(i) && wpn) {
                wpnUI.gameObject.SetActive(true);
                wpnUI.label.text = wpn.labelText;
                wpnUI.SetIconSprite(wpn.iconSpriteRef);

                wpnUI.max = Mathf.CeilToInt(Weapon.weaponEnergyDefaultMax);
                wpnUI.current = Mathf.CeilToInt(wpn.currentEnergy);

                eventListener.onClick = OnWeaponClick;

                UIButtonKeys buttonKeys = wpnUI.GetComponent<UIButtonKeys>();

                buttonKeys.selectOnUp = lastWeaponButtonKeys;
                buttonKeys.selectOnRight = rightButtonKeys;

                if(firstWeaponButtonKeys == null)
                    firstWeaponButtonKeys = buttonKeys;

                if(lastWeaponButtonKeys)
                    lastWeaponButtonKeys.selectOnDown = buttonKeys;

                lastWeaponButtonKeys = buttonKeys;
            }
            else {
                wpnUI.gameObject.SetActive(false);

                eventListener.onClick = null;

                UIButtonKeys buttonKeys = wpnUI.GetComponent<UIButtonKeys>();
                buttonKeys.selectOnUp = null;
                buttonKeys.selectOnDown = null;
            }
        }

        if(firstWeaponButtonKeys) {
            firstWeaponButtonKeys.selectOnUp = lastWeaponButtonKeys;
        }

        if(lastWeaponButtonKeys) {
            lastWeaponButtonKeys.selectOnDown = firstWeaponButtonKeys;
        }
    }

    void InitHP() {
        hpBar.max = Mathf.CeilToInt(Player.instance.stats.maxHP);
        hpBar.current = Mathf.CeilToInt(Player.instance.stats.curHP);
    }

    void RefreshEnergyTank() {
    }

    void RefreshWeaponTank() {
    }

    void OnWeaponClick(GameObject go) {
        if(mInputLockCounter > 0)
            return;

        for(int i = 0, max = mWeapons.Length; i < max; i++) {
            if(mWeapons[i].gameObject == go) {
                //unpause?
                Player.instance.currentWeaponIndex = i;
                break;
            }
        }
    }

    void OnEnergySubTankClick(GameObject go) {
        if(mInputLockCounter > 0)
            return;


    }

    void OnWeaponSubTankClick(GameObject go) {
        if(mInputLockCounter > 0)
            return;
    }

    void OnExitClick(GameObject go) {
        if(mInputLockCounter > 0)
            return;
    }

    void OnOptionsClick(GameObject go) {
        if(mInputLockCounter > 0)
            return;
    }
}
