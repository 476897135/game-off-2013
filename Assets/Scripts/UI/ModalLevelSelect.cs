using UnityEngine;
using System.Collections;

public class ModalLevelSelect : UIController {
    public UILevelSelectItem gitgirl;
    public UILevelSelectItem finalLevel;

    private UILevelSelectItem[] mLevelItems;

    protected override void OnActive(bool active) {
        if(active) {
            UICamera.selectedObject = finalLevel.isFinalUnlock ? finalLevel.gameObject : gitgirl.gameObject;

            foreach(UILevelSelectItem item in mLevelItems) {
                item.listener.onClick = OnLevelClick;
                item.listener.onSelect = OnLevelSelect;
            }
        }
        else {
            foreach(UILevelSelectItem item in mLevelItems) {
                item.listener.onClick = null;
                item.listener.onSelect = null;
            }
        }
    }

    protected override void OnOpen() {
    }

    protected override void OnClose() {
    }

    void Awake() {
        mLevelItems = GetComponentsInChildren<UILevelSelectItem>(true);

        //init items
        foreach(UILevelSelectItem item in mLevelItems) {
            item.Init();
        }

        finalLevel.InitFinalLevel(mLevelItems);
    }

    void OnLevelSelect(GameObject go, bool s) {
        if(s) {
            for(int i = 0, max = mLevelItems.Length; i < max; i++) {
                if(mLevelItems[i].gameObject == go) {
                    gitgirl.portrait.spriteName = mLevelItems[i].gitGirlPortraitRef;
                    break;
                }
            }
        }
    }

    void OnLevelClick(GameObject go) {
        for(int i = 0, max = mLevelItems.Length; i < max; i++) {
            if(mLevelItems[i].gameObject == go && mLevelItems[i] != gitgirl) {
                mLevelItems[i].Click();
                break;
            }
        }
    }
}
