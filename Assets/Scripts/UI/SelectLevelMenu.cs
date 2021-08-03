using System.Collections;
using UnityEngine;
using TMPro;
using System.Linq;
using Enums;

public class SelectLevelMenu : MonoBehaviour
{
    [SerializeField] private GameMode _gameMode;
    [SerializeField] private TextMeshProUGUI _starsProgressText = null;
    [SerializeField] private TextMeshProUGUI _levelProgressText = null;
    [SerializeField] private TextMeshProUGUI _moneyText = null;

    [SerializeField] private Animator _animator;
    private int _activateParamID;
    private int _deactivateParamID;


    private void Awake()
    {
        _activateParamID = Animator.StringToHash("activate");
        _deactivateParamID = Animator.StringToHash("deactivate");

        var levels = LevelManager.Levels.Where(level => level.gameMode == _gameMode);
        int currentStars = 0;
        levels.Sum(x => currentStars += x.result);
        _starsProgressText.text = currentStars.ToString();
        int completedLevels = 0;
        levels.Sum(x => completedLevels += x.completed ? 1 : 0);
        _levelProgressText.text = completedLevels.ToString() + "/" + levels.Count();

        _moneyText.text = GameManager.MoneyAmount.ToString();
    }

    private void OnEnable()
    {
        _animator.SetTrigger(_activateParamID);
    }

    public void CloseWindow()
    {
        _animator.SetTrigger(_deactivateParamID);
    }

    public void DisableWindow()
    {
        gameObject.SetActive(false);
    }
}