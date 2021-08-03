using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ItemStorage : MonoBehaviour
{
    public static ItemStorage current;

    public static ShopItem selectedCharacterSkin;
    public static ShopItem selectedShieldSkin;

    public ShopItem[] characterSkins;
    public ShopItem[] shieldSkins;

    private void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        DontDestroyOnLoad(gameObject);

        // Sorting array
        characterSkins = characterSkins.OrderBy(x => x.rarity).ThenBy(x => x.cost).ToArray();
        shieldSkins = shieldSkins.OrderBy(x => x.rarity).ThenBy(x => x.cost).ToArray();

        SaveData saveData = SaveSystem.LoadData();

        if (saveData != null)
        {
            // Load skins
            foreach (SaveShopItem item in saveData.characterSkins)
            {
                characterSkins[item.index].purchased = item.purchased;
                characterSkins[item.index].equipped = item.equipped;
            }

            // Set index to new skins
            for (int i = saveData.characterSkins.Length; i < characterSkins.Length; i++)
            {
                characterSkins[i].index = i;
            }

            // Load skins
            foreach (SaveShopItem item in saveData.shieldSkins)
            {
                shieldSkins[item.index].purchased = item.purchased;
                shieldSkins[item.index].equipped = item.equipped;
            }

            // Set index to new skins
            for (int i = saveData.shieldSkins.Length; i < shieldSkins.Length; i++)
            {
                shieldSkins[i].index = i;
            }

            selectedCharacterSkin = characterSkins[saveData.selectedCharacterSkin.index];
            selectedShieldSkin = shieldSkins[saveData.selectedShieldSkin.index];
        }
        else
        {
            for(int i = 0; i < characterSkins.Length; i++)
            {
                characterSkins[i].index = i;

                if (i > 0)
                {
                    characterSkins[i].purchased = false;
                    characterSkins[i].equipped = false;
                }
                else
                {
                    characterSkins[i].purchased = true;
                    characterSkins[i].equipped = true;
                }

                if (characterSkins[i].equipped == true)
                    selectedCharacterSkin = characterSkins[i];
            }

            for (int i = 0; i < shieldSkins.Length; i++)
            {
                shieldSkins[i].index = i;

                if (i > 0)
                {
                    shieldSkins[i].purchased = false;
                    shieldSkins[i].equipped = false;
                }
                else
                {
                    shieldSkins[i].purchased = true;
                    shieldSkins[i].equipped = true;
                }

                if (shieldSkins[i].equipped == true)
                {
                    selectedShieldSkin = shieldSkins[i];
                }
            }

        }
    }
}
