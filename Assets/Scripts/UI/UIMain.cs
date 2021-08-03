using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Enums;

public class UIMain : MonoBehaviour
{
    [Header("Level Start")]
    [SerializeField] private TextMeshProUGUI _levelNumberText = null;
    [SerializeField] private GameObject _levelNumberCanvas = null;
    private int _levelIndex;

    [Header("Level Completed")]
    [SerializeField] private CanvasGroup _levelCompletedCanvas = null;
    [SerializeField] private Image[] _winPanelStarsImages = new Image[3];
    [SerializeField] private GameObject _restartButton = null;
    [SerializeField] private TextMeshProUGUI _awardText = null;
    [SerializeField] private CanvasGroup _awardCanvas = null;
    [Space]
    //[SerializeField] private GameObject _starParticlePrefab = null;
    [SerializeField] private GameObject _confettiParticlePrefab = null;

    [Header("Top Bar")]
    [SerializeField] private RectTransform _throwsLeftParent = null;
    [SerializeField] private GameObject _throwImagePrafab = null;
    [SerializeField] private GameObject _extraThrowImagePrefab = null;
    private List<GameObject> _throwsImages = new List<GameObject>();

    [Header("Rate Us")]
    [SerializeField] private GameObject _rateUsPrefab = null;

    [Header("Ads")]
    [SerializeField] private GameObject _rewardAd = null;
    [SerializeField] private Button _rewardButton = null;
    [SerializeField] private GameObject _rewardBtnActive = null;
    [SerializeField] private GameObject _rewardBtnLoad = null;
    [SerializeField] private TextMeshProUGUI _rewardAdText = null;
    private int _extraRewardAmount;
    private bool _rewardAdViewed = true;
    private bool _rewardAdWatchEventSended;

    private static UIMain current;
    private static Canvas canvas;

    private Coroutine _awardCoroutine;

    private void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        DontDestroyOnLoad(gameObject);

        canvas = GetComponent<Canvas>();
        canvas.worldCamera = GameObject.FindGameObjectWithTag("GUI Camera").GetComponent<Camera>();
    }

    private void Start()
    {
        GameManager.levelCompletedEvent.AddListener(ActivateLevelCompletedMenu);
        LevelManager.switchLevelEvent.AddListener(DeactivateLevelCompletedMenu);
        GameManager.gameOverEvent.AddListener(ActivateRestartButton);

        _rewardBtnActive.SetActive(false);
        _rewardBtnLoad.SetActive(true);
        _rewardButton.interactable = false;
        MaxSdkCallbacks.OnRewardedAdLoadedEvent += RewardAdLoaded;
    }

    private void ActivateLevelCompletedMenu()
    {
        _levelCompletedCanvas.alpha = 1f;
        _levelCompletedCanvas.interactable = true;
        _levelCompletedCanvas.blocksRaycasts = true;

        _awardCoroutine = StartCoroutine(AwardsDisplay());
    }

    private IEnumerator AwardsDisplay()
    {
        int awardsAmount = 0;
        int throwsLeft = Mathf.Clamp(ScoreHandler.curNumberOfThrows, 0, 3);

        if (LevelManager.currentLevel.result < throwsLeft)
            awardsAmount = (throwsLeft - LevelManager.currentLevel.result) * ScoreHandler.current.AwardForThrows;

        GameManager.MoneyAmount += awardsAmount;

        if (awardsAmount > 0)
        {
            if (_rewardAdViewed == false)
                ApplovinRewardAd.rewardMultiplier = Mathf.Clamp(ApplovinRewardAd.rewardSkipIncreaser + ApplovinRewardAd.rewardMultiplier, 3, 7);
            else
                _rewardAdViewed = false;

            _rewardAd.SetActive(true);
            int extraReward = awardsAmount * ApplovinRewardAd.rewardMultiplier;
            _rewardAdText.text = "+" + extraReward.ToString();
            _extraRewardAmount = extraReward - awardsAmount;
        }
        else
        {
            _rewardAd.SetActive(false);
        }

        for (int i = 0; i < throwsLeft; i++)
        {
            _winPanelStarsImages[i].enabled = true;
            Color color = _winPanelStarsImages[i].color;

            while (color.a < 1f)
            {
                color.a += Time.deltaTime * 4f;
                _winPanelStarsImages[i].color = color;
                yield return new WaitForEndOfFrame();
            }
            //Spawn particle
        }

        Instantiate(_confettiParticlePrefab, _winPanelStarsImages[1].transform.position, Quaternion.identity, transform);

        if (awardsAmount > 0)
        {
            _awardText.text = "";
            float awardsAccrued = 0;

            while (_awardCanvas.alpha < 1f)
            {
                _awardCanvas.alpha += Time.deltaTime * 4f;
                yield return new WaitForEndOfFrame();
            }

            while (awardsAccrued < awardsAmount)
            {
                awardsAccrued = Mathf.Clamp(Time.deltaTime * awardsAmount + awardsAccrued, 0f, awardsAmount);
                _awardText.text = "+" + Mathf.FloorToInt(awardsAccrued);
                yield return new WaitForEndOfFrame();
            }
        }
    }

    private void DeactivateLevelCompletedMenu()
    {
        if(_awardCoroutine != null)
            StopCoroutine(_awardCoroutine);

        _awardCanvas.alpha = 0f;
        _levelCompletedCanvas.alpha = 0f;
        _levelCompletedCanvas.interactable = false;
        _levelCompletedCanvas.blocksRaycasts = false;

        for (int i = 0; i < _winPanelStarsImages.Length; i++)
        {
            Color color = _winPanelStarsImages[i].color;
            color.a = 0f;
            _winPanelStarsImages[i].color = color;
            _winPanelStarsImages[i].enabled = false;
        }
    }

    private void ActivateRestartButton()
    {
        _restartButton.SetActive(true);
    }

    private void OnLevelWasLoaded(int level)
    {
        if (current != null && current != this) return;

        DeactivateLevelCompletedMenu();
        _restartButton.SetActive(false);
        CreateThrowsImages();

        if (_levelIndex != level /*LevelManager.currentLevel.ID*/)
            ShowLevelNumber();

        if (level <= 1)
        {
            CancelInvoke(nameof(HideLevelNumber));
            HideLevelNumber();
            canvas.enabled = false;
        }
        else if (canvas.enabled == false)
        {
            canvas.enabled = true;
        }

        _levelIndex = level;
        //_levelIndex = LevelManager.currentLevel.ID;

        if (canvas.worldCamera == null)
            canvas.worldCamera = GameObject.FindGameObjectWithTag("GUI Camera")?.GetComponent<Camera>();
    }

    private void CreateThrowsImages()
    {
        if(_throwsImages.Count > 0)
        {
            foreach (GameObject image in _throwsImages)
                Destroy(image);
        }

        _throwsImages = new List<GameObject>();
        int throws = ScoreHandler.Throws;
        int extraThrows = ScoreHandler.ExtraThrows;
        GameObject prefab;

        for (int i = 0; i < throws + extraThrows; i++)
        {
            if (i < throws)
                prefab = _throwImagePrafab;
            else
                prefab = _extraThrowImagePrefab;

            GameObject image = Instantiate(prefab, Vector3.zero, Quaternion.identity, _throwsLeftParent);
            _throwsImages.Add(image);
        }

        CalculateImagePosition();
    }

    public static void DestroyThrowImage()
    {
        if (current._throwsImages.Count > 0)
        {
            Destroy(current._throwsImages[current._throwsImages.Count - 1]);
            current._throwsImages.RemoveAt(current._throwsImages.Count - 1);

            current.CalculateImagePosition();
        }
    }

    private void CalculateImagePosition()
    {
        float interval = 20f;
        float spriteWidth = _throwImagePrafab.GetComponent<RectTransform>().rect.width;
        Vector2 spawnPos = new Vector2((-spriteWidth / 2f * (_throwsImages.Count - 1)) - (interval * (_throwsImages.Count - 1) / 2f), 0f);

        for (int i = 0; i < _throwsImages.Count; i++)
        {
            _throwsImages[i].transform.localPosition = spawnPos;
            spawnPos.x += spriteWidth + interval;
        }
    }

    private void ShowLevelNumber()
    {
        _levelNumberText.text = "LEVEL " + LevelManager.currentLevel.levelNumber;
        _levelNumberCanvas.SetActive(true);
        Invoke(nameof(HideLevelNumber), 1f);
    }

    private void HideLevelNumber()
    {
        _levelNumberCanvas.SetActive(false);
    }

    public void Restart()
    {
        LevelManager.Restart();
    }

    public void ToMainMenu()
    {
        LevelManager.ToMainMenu();
    }

    public void NextLevel()
    {
        LevelManager.ActivateNextLevel();
    }

    public static void ShowRateUsPopup()
    {
        GameObject inst = Instantiate(current._rateUsPrefab);
        inst.transform.SetParent(canvas.transform, false);
    }

    public void ShowRewardAd()
    {
        if (MaxSdk.IsRewardedAdReady(ApplovinRewardAd.adUnitId))
        {
            MaxSdkCallbacks.OnRewardedAdReceivedRewardEvent += GetExtraReward;
            MaxSdkCallbacks.OnRewardedAdHiddenEvent += OnRewardedAdHiddenEvent;
            MaxSdkCallbacks.OnRewardedAdDisplayedEvent += OnRewardedAdDisplayedEvent;
            MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent += OnRewardedAdFailedToDisplayEvent;
            MaxSdkCallbacks.OnRewardedAdClickedEvent += OnRewardedAdClickedEvent;

            _rewardAdWatchEventSended = false;
            AppMetricaEvents.Instance.AdAvailable(AdType.Rewarded, "get_extra_coins", "success");
            AdManager.BannerCannotBeViewed();
            MaxSdk.ShowRewardedAd(ApplovinRewardAd.adUnitId);
            _rewardBtnActive.SetActive(false);
            _rewardBtnLoad.SetActive(true);
            _rewardButton.interactable = false;
        }
        else
        {
            AppMetricaEvents.Instance.AdAvailable(Enums.AdType.Rewarded, "get_extra_coins", "not_available");
        }
    }

    private void RewardAdLoaded(string adUnitId)
    {
        _rewardBtnLoad.SetActive(false);
        _rewardBtnActive.SetActive(true);
        _rewardButton.interactable = true;
    }

    private void GetExtraReward(string adUnitId, MaxSdk.Reward reward)
    {
        _rewardAdViewed = true;
        GameManager.MoneyAmount += _extraRewardAmount;
        _extraRewardAmount = 0;
        _rewardAd.SetActive(false);
        _awardText.text = _rewardAdText.text;

        if (_rewardAdWatchEventSended == false)
        {
            AppMetricaEvents.Instance.AdWatch(AdType.Rewarded, "get_extra_coins", "watched");
            _rewardAdWatchEventSended = true;
        }

        MaxSdkCallbacks.OnRewardedAdReceivedRewardEvent -= GetExtraReward;
    }

    private void OnRewardedAdHiddenEvent(string adUnitId)
    {
        if (_rewardAdWatchEventSended == false)
        {
            AppMetricaEvents.Instance.AdWatch(AdType.Rewarded, "get_extra_coins", "canceled");
            _rewardAdWatchEventSended = true;
        }

        MaxSdkCallbacks.OnRewardedAdReceivedRewardEvent -= GetExtraReward;
        MaxSdkCallbacks.OnRewardedAdHiddenEvent -= OnRewardedAdHiddenEvent;
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId)
    {
        AppMetricaEvents.Instance.AdStarted(AdType.Rewarded, "get_extra_coins", "start");

        MaxSdkCallbacks.OnRewardedAdDisplayedEvent -= OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent -= OnRewardedAdFailedToDisplayEvent;
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, int errorCode)
    {
        AppMetricaEvents.Instance.AdStarted(AdType.Rewarded, "get_extra_coins", "not_available");

        MaxSdkCallbacks.OnRewardedAdDisplayedEvent -= OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.OnRewardedAdFailedToDisplayEvent -= OnRewardedAdFailedToDisplayEvent;
    }

    private void OnRewardedAdClickedEvent(string adUnitId)
    {
        if (_rewardAdWatchEventSended == false)
        {
            AppMetricaEvents.Instance.AdWatch(AdType.Rewarded, "get_extra_coins", "clicked");
            _rewardAdWatchEventSended = true;
        }

        MaxSdkCallbacks.OnRewardedAdClickedEvent -= OnRewardedAdClickedEvent;
    }
}
