using System.Collections;
using UnityEngine;
using Enums;

[System.Serializable]
public class Level
{
    public int ID;
    public string levelName;
    public int levelNumber;
    public GameMode gameMode;
    public int result;
    public bool locked;
    public bool completed;

    public Level(int id, string levelName, int levelNumber, GameMode gameMode, bool locked)
    {
        ID = id;
        this.levelName = levelName;
        this.levelNumber = levelNumber;
        this.gameMode = gameMode;
        this.locked = locked;
        result = 0;
        completed = false;
    }

    public void Unlock()
    {
        locked = false;
    }

    public void Complete(int result)
    {
        completed = true;
        this.result = result > this.result ? result : this.result;
    }
}