using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RateUs : MonoBehaviour
{
    public static bool isDisabled = false;
    public static bool newVersion = false;
    public static int firstShowLevelNumber = 4;
    public static double nextShowDelaySeconds = 86400;//259200;
    public static DateTime nextShowDatetime = default;

    //[Header("Stars Properties")]
    //[SerializeField] private Image[] _starImages = null;

    [Header("Buttons Properties")]
    [SerializeField] private string _gameUrl = "";
    [Space]
    [SerializeField] private Animator _animator = null;
    private int _activateParamID;
    private int _deactivateParamID;
    private int _starsCount;

    private void Start()
    {
        _activateParamID = Animator.StringToHash("activate");
        _deactivateParamID = Animator.StringToHash("deactivate");
        _animator.SetTrigger(_activateParamID);
    }

    public void ClosePopup()
    {
        Destroy(gameObject);
    }

    public void RateUp()
    {
        isDisabled = true;
        Application.OpenURL(_gameUrl);
        SaveSystem.SaveData();
        string reason = newVersion == true ? "new_version" : "new_player";
        _starsCount = 5;
        Destroy(gameObject);
    }

    //public void SetStars(int count)
    //{
    //    _starsCount = count;
    //    Color color = _starImages[0].color;

    //    for(int i = 0; i < _starImages.Length; i++)
    //    {
    //        if (i < count)
    //        {
    //            color.a = 1f;
    //            _starImages[i].color = color;
    //        }
    //        else
    //        {
    //            color.a = 0f;
    //            _starImages[i].color = color;
    //        }
    //    }
    //}

    public void Deactivate(int starsCount = 0)
    {
        _starsCount = starsCount;
        _animator.SetTrigger(_deactivateParamID);
    }

    private void OnDestroy()
    {
        // Save new time to show popup
        nextShowDatetime = DateTime.Now;
        nextShowDatetime = nextShowDatetime.AddSeconds(nextShowDelaySeconds);
        SaveSystem.SaveData();
        string reason = newVersion == true ? "new_version" : "new_player";
        AppMetricaEvents.Instance.RateUs(reason, _starsCount);
    }
}
