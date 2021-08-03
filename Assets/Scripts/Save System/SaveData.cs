using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    //Level properties
    public Level[] levels;
    public Level lastLevel;
    public int levelCount;
    public int levelCompleted;
    public int levelLoop;
    public int moneyAmount;

    // Items properties
    public SaveShopItem[] characterSkins;
    public SaveShopItem[] shieldSkins;
    public SaveShopItem selectedCharacterSkin;
    public SaveShopItem selectedShieldSkin;

    // Rate Us properties
    public bool rateUsDisabled;
    public System.DateTime rateUsNextShowDatetime;

    // Purchases properties
    public bool noAdsPurchased;

    public SaveData()
    {
        levels = LevelManager.Levels;
        lastLevel = LevelManager.nextLevel;
        levelCount = LevelManager.levelCount;
        levelCompleted = LevelManager.levelCompleted;
        levelLoop = LevelManager.levelLoop;
        moneyAmount = GameManager.MoneyAmount;

        selectedCharacterSkin = new SaveShopItem(ItemStorage.selectedCharacterSkin);
        selectedShieldSkin = new SaveShopItem(ItemStorage.selectedShieldSkin);

        characterSkins = new SaveShopItem[ItemStorage.current.characterSkins.Length];

        for(int i = 0; i < ItemStorage.current.characterSkins.Length; i++)
        {
            characterSkins[i] = new SaveShopItem(ItemStorage.current.characterSkins[i]);
        }

        shieldSkins = new SaveShopItem[ItemStorage.current.shieldSkins.Length];

        for (int i = 0; i < ItemStorage.current.shieldSkins.Length; i++)
        {
            shieldSkins[i] = new SaveShopItem(ItemStorage.current.shieldSkins[i]);
        }

        rateUsDisabled = RateUs.isDisabled;
        rateUsNextShowDatetime = RateUs.nextShowDatetime;

        noAdsPurchased = AdManager.noAdsPurchased;
    }
}
