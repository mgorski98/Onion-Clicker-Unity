using System.Collections.Generic;
using UnityEngine;

public class UpgradesManager : MonoBehaviour
{
    public static UpgradesManager instance;

    public List<GameUpgrade> allUpgrades;
    public List<GameUpgrade> unlockedUpgrades;
    public List<GameUpgrade> boughtUpgrades;

    [SerializeField]
    private PlayerDetails playerDetails;

    public List<GeneratorDetails> generatorDetails;

    private void Awake() {
        instance = this;
    }

    public bool Unlock(GameUpgrade upgrade, GlobalUpgradeTriggerData? data = null) {
        if (IsUnlocked(upgrade) || IsBought(upgrade)) return false;
        if (data != null) { //global upgrade
            var actualData = data.Value;
            switch (upgrade.upgradeType) {//types not included in here are generator specific and will be handled in gen details
                case GameUpgrade.UpgradeType.Global:
                    if (!(actualData.totalOnions >= upgrade.unlockedAtTotalOnions && upgrade.unlockedAtTotalOnions >= 0)) {
                        return false;
                    }
                    break;
                case GameUpgrade.UpgradeType.Click:
                    if (!(actualData.totalClicks >= upgrade.unlockedAtTotalClicks && upgrade.unlockedAtTotalClicks != GameUpgrade.NOT_VALID)) {
                        return false;
                    }
                    break;
                case GameUpgrade.UpgradeType.GlobalIncreasePerNGenerators:
                    if (!(actualData.totalGeneratorsAmount >= upgrade.unlockedAtTotalGenerators && upgrade.unlockedAtTotalGenerators != GameUpgrade.NOT_VALID)) {
                        return false;
                    }
                    break;
            }
        }//else its a generator upgrade (its checked for unlocks from outside this function)
        unlockedUpgrades.Add(upgrade);
        UIActions.instance.SpawnGeneratorUpgradeDetailsWindow(upgrade);
        return true;
    }

    public void Buy(GameUpgrade upgrade) {
        if (IsBought(upgrade)) return;
        boughtUpgrades.Add(upgrade);

        switch (upgrade.upgradeType) {
            case GameUpgrade.UpgradeType.Click:
                playerDetails.IncreaseClickIncome(upgrade.multiplier);
                break;
            case GameUpgrade.UpgradeType.Generator:
                var details = generatorDetails.Find(d => d.generator.ID == upgrade.targetGeneratorID);
                details.incomeGeneratorMultiplier *= upgrade.multiplier;
                break;
            case GameUpgrade.UpgradeType.IncreasePerNGenerators:
                GeneratorDetails genToIncreaseDetails = generatorDetails.Find(d => d.generator.ID == upgrade.targetGeneratorID);
                GeneratorDetails genToIncreaseByNOfDetails = generatorDetails.Find(d => d.generator.ID == upgrade.increasePerNOfGeneratorID); //N of this generator - % of multiplier to the one above
                //just use closure in lambda to capture generator references, we're doing it the FUNCTIONAL WAY BOIS (yeah i know its technically not functional way but ssshhhhhh)
                //and subscribe to the OnGeneratorAmountChanged event, with that same lambda
                //god i hope this doesn't leak memory
                genToIncreaseByNOfDetails.OnGeneratorAmountChanged += (oldAmount, currentAmount) => {
                    //calculate increase like this: calculate total multiplier for current and for old amount and add the difference
                    var previousMultiplier = (oldAmount / upgrade.perN) * upgrade.multiplier;
                    var currentMultiplier = (currentAmount / upgrade.perN) * upgrade.multiplier;
                    genToIncreaseDetails.increaseFromOtherGeneratorsMultiplier += currentMultiplier - previousMultiplier;
                };
                break;
            case GameUpgrade.UpgradeType.GlobalIncreasePerNGenerators:
                GeneratorDetails genToIncreaseByNOfDetails_Global = generatorDetails.Find(d => d.generator.ID == upgrade.increasePerNOfGeneratorID);
                genToIncreaseByNOfDetails_Global.OnGeneratorAmountChanged += (oldAmount, currentAmount) => {
                    var previousMultiplier = (oldAmount / upgrade.perN) * upgrade.multiplier;
                    var currentMultiplier = (currentAmount / upgrade.perN) * upgrade.multiplier;
                    playerDetails.IncreaseMultiplier(currentMultiplier - previousMultiplier);
                };
                break;
            case GameUpgrade.UpgradeType.Global:
                playerDetails.IncreaseMultiplier(upgrade.multiplier);
                break;
        }
    }

    public bool IsUnlocked(GameUpgrade upgrade) => unlockedUpgrades.Contains(upgrade);

    public bool IsBought(GameUpgrade upgrade) => boughtUpgrades.Contains(upgrade);
}
