using System.Collections;
using UnityEngine;
using Enums;

[System.Serializable]
public class SaveShopItem
{
    public int index;
    public bool purchased;
    public bool equipped;

    public SaveShopItem() { }
    public SaveShopItem(ShopItem item)
    {
        index = item.index;
        purchased = item.purchased;
        equipped = item.equipped;
    }
}