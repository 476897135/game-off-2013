using UnityEngine;
using System.Collections;

public class ModalPause : UIController {
    public Transform weaponsHolder;

    public UIEventListener energySubTank;
    public UIEventListener weaponSubTank;

    public UIEventListener exit;
    public UIEventListener options;

    private UIEnergyBar[] mWeapons;

    protected override void OnActive(bool active) {
        if(active) {
            InitSubTanks();
            InitWeapons();
                        
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
        }
    }

    protected override void OnOpen() {

    }

    protected override void OnClose() {

    }

    void InitSubTanks() {
        UIButtonKeys energyBtnKeys = energySubTank.GetComponent<UIButtonKeys>();
        UIButtonKeys weaponBtnKeys = weaponSubTank.GetComponent<UIButtonKeys>();
        UIButtonKeys exitBtnKeys = exit.GetComponent<UIButtonKeys>();
        UIButtonKeys optionsBtnKeys = options.GetComponent<UIButtonKeys>();

        if(PlayerStats.isEnergySubTankAvailable) {
            energySubTank.gameObject.SetActive(true);

            energySubTank.onClick = OnEnergySubTankClick;
        }
        else {
            energySubTank.gameObject.SetActive(false);
        }

        if(PlayerStats.isWeaponSubTankAvailable) {
            weaponSubTank.gameObject.SetActive(true);

            weaponSubTank.onClick = OnWeaponSubTankClick;
        }
        else {
            energySubTank.gameObject.SetActive(false);
        }

        energyBtnKeys.selectOnDown = PlayerStats.isWeaponSubTankAvailable ? weaponBtnKeys : exitBtnKeys;

        weaponBtnKeys.selectOnUp = PlayerStats.isEnergySubTankAvailable ? energyBtnKeys : optionsBtnKeys;

        exitBtnKeys.selectOnUp =
            PlayerStats.isWeaponSubTankAvailable ? weaponBtnKeys :
                PlayerStats.isEnergySubTankAvailable ? energyBtnKeys :
                    optionsBtnKeys;

        optionsBtnKeys.selectOnDown =
            PlayerStats.isEnergySubTankAvailable ? energyBtnKeys :
                PlayerStats.isWeaponSubTankAvailable ? weaponBtnKeys :
                    exitBtnKeys;
    }

    void InitWeapons() {
        if(mWeapons == null) {
            mWeapons = weaponsHolder.GetComponentsInChildren<UIEnergyBar>(true);
            System.Array.Sort(mWeapons,
                delegate(UIEnergyBar bar1, UIEnergyBar bar2) {
                    return bar1.name.CompareTo(bar2.name);
                });
        }

        Player player = Player.instance;

        UIButtonKeys firstWeaponButtonKeys = null;
        UIButtonKeys lastWeaponButtonKeys = null;
        UIButtonKeys rightButtonKeys = null;

        if(PlayerStats.isEnergySubTankAvailable)
            rightButtonKeys = energySubTank.GetComponent<UIButtonKeys>();
        else if(PlayerStats.isWeaponSubTankAvailable)
            rightButtonKeys = weaponSubTank.GetComponent<UIButtonKeys>();
        else
            rightButtonKeys = exit.GetComponent<UIButtonKeys>();

        for(int i = 0, max = mWeapons.Length; i < max; i++) {
            Weapon wpn = i < player.weapons.Length ? player.weapons[i] : null;

            UIEventListener eventListener = wpn.GetComponent<UIEventListener>();

            if(PlayerStats.IsWeaponAvailable(i) && wpn) {
                mWeapons[i].gameObject.SetActive(true);
                mWeapons[i].label.text = wpn.labelText;
                mWeapons[i].SetIconSprite(wpn.iconSpriteRef);

                mWeapons[i].max = Mathf.CeilToInt(Weapon.weaponEnergyDefaultMax);
                mWeapons[i].current = Mathf.CeilToInt(wpn.currentEnergy);

                eventListener.onClick = OnWeaponClick;

                UIButtonKeys buttonKeys = GetComponent<UIButtonKeys>();

                buttonKeys.selectOnUp = lastWeaponButtonKeys;
                buttonKeys.selectOnRight = rightButtonKeys;

                if(firstWeaponButtonKeys == null)
                    firstWeaponButtonKeys = buttonKeys;

                if(lastWeaponButtonKeys)
                    lastWeaponButtonKeys.selectOnDown = buttonKeys;

                lastWeaponButtonKeys = buttonKeys;
            }
            else {
                mWeapons[i].gameObject.SetActive(false);

                eventListener.onClick = null;

                UIButtonKeys buttonKeys = GetComponent<UIButtonKeys>();
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

    void OnWeaponClick(GameObject go) {
        for(int i = 0, max = mWeapons.Length; i < max; i++) {
            if(mWeapons[i].gameObject == go) {
                //unpause?
                Player.instance.currentWeaponIndex = i;
                break;
            }
        }
    }

    void OnEnergySubTankClick(GameObject go) {
    }

    void OnWeaponSubTankClick(GameObject go) {
    }

    void OnExitClick(GameObject go) {
    }

    void OnOptionsClick(GameObject go) {
    }
}
