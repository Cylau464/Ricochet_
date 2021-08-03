using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

[CreateAssetMenu(fileName = "Shop Item", menuName = "Shop/Item")]
public class ShopItem : ScriptableObject
{
    public int index;
    public SkinType type;
    public SkinRarity rarity;
    public string itemName;
    public GameObject model;
    public Material material;
    public float spawnHeight;
    public int cost;
    public bool purchased;
    public bool equipped;
}
