using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

[CreateAssetMenu(fileName = "Level Area", menuName = "Menu/Level Area")]
public class LevelArea : ScriptableObject
{
    public GameObject levelButtonPrefab;
    public Sprite backgroundImage;
    public int areaIndex;
    public int levelsCount;
    public GameMode gameMode;
}
