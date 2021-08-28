using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalOnionAmountAchievement : Achievement {
    public override bool Unlock(AchievementTriggerData triggerData) {
        if (unlocked) return false;

        if (triggerData.totalOnions >= this.triggerData.totalOnions) {
            UnlockInternal();
            return true;
        }
        return false;
    }
}
