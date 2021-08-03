using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using TMPro;
using Enums;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _classicStars = null;
    [SerializeField] private GameObject _selectLevelMenu = null;
    [SerializeField] private GameObject _shopMenu = null;

    private void Start()
    {
        GetStars();
    }

    private void GetStars()
    {
        int classicStars = 0;
        LevelManager.Levels.Where(level => level.gameMode == GameMode.Classic).Sum(x => classicStars += x.result);
        _classicStars.text = classicStars.ToString();
    }

    public void Play()
    {
        SceneManager.LoadScene(LevelManager.currentLevel.levelName);
    }

    public void SelectLevelMenu()
    {
        _selectLevelMenu.SetActive(true);
    }

    public void SelectShopMenu()
    {
        _shopMenu.SetActive(true);
    }
}
