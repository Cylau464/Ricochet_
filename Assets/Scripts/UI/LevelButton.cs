using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    //[SerializeField] private Image _starsFillImage = null;
    [SerializeField] private Image _levelImage = null;
    [SerializeField] private TextMeshProUGUI _numberText = null;
    [SerializeField] private GameObject[] _stars = null;
    private Level _level;

    [SerializeField] private Sprite _openedLevelSprite = null;
    [SerializeField] private Sprite _finishedLevelSprite = null;


    private void Start()
    {
        Button button = GetComponent<Button>();
        //_starsFillImage.fillAmount = _level.result / 3f;    // Set stars
        button.onClick.AddListener(() => ChooseLevel(_level.levelName));

        if (_level.locked == true)
            button.interactable = false;
    }

    public void SetLevel(Level level)
    {
        _level = level;
        _numberText.text = level.levelNumber.ToString();

        if (level.completed == true)
        {
            _numberText.color = Color.white;
            _levelImage.sprite = _finishedLevelSprite;

            if(_level.result > 0)
                _stars[_level.result - 1].SetActive(true);
        }
        else if (level.locked == false)
        {
            _levelImage.sprite = _openedLevelSprite;
        }
    }

    public void ChooseLevel(string sceneName)
    {
        LevelManager.currentLevel = LevelManager.nextLevel = _level;
        SceneManager.LoadScene(sceneName);
    }
}
