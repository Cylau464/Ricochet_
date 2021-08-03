using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Enums;
using UnityEngine.Purchasing;

public class AppMetricaEvents : MonoBehaviour
{
    private const string LEVEL_START_EVENT = "level_start";
    private const string LEVEL_FINISH_EVENT = "level_finish";
    private const string SHIELD_STUCK = "shield_stuck";
    private const string RATE_US = "rate_us";
    private const string AD_AVAILABLE = "video_ads_available";
    private const string AD_STARTED = "video_ads_started";
    private const string AD_WATCH = "video_ads_watch";
    private const string PAYMENT_SUCCEED = "payment_succeed";
    private const string SKIN_UNLOCK = "skin_unlock";

    private Dictionary<string, object> _eventParameters = new Dictionary<string, object>();

    public static AppMetricaEvents Instance;

    private enum FinishResults { Win, Lose, Leave, Restart }
    private bool _levelFinised;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        //GameManager.levelStart.AddListener(LevelStart);
        GameManager.levelCompletedEvent.AddListener(() => LevelFinish(FinishResults.Win));
        GameManager.gameOverEvent.AddListener(() => LevelFinish(FinishResults.Lose));
        GameManager.levelLeave.AddListener(() => LevelFinish(FinishResults.Leave));
        GameManager.levelRestart.AddListener(() => LevelFinish(FinishResults.Restart));
    }

    private void SendEvent(string eventName)
    {
        AppMetrica.Instance.ReportEvent(eventName, _eventParameters);
        AppMetrica.Instance.SendEventsBuffer();
        _eventParameters.Clear();
    }

    public void LevelStart()
    {
        _levelFinised = false;

        _eventParameters = new Dictionary<string, object>()
        {
            { "level_number", LevelManager.currentLevel.levelNumber },
            { "level_name", LevelManager.currentLevel.levelName.ToLower() },
            { "level_count", LevelManager.levelCount },
            { "level_loop", LevelManager.levelLoop }
        };
        SendEvent(LEVEL_START_EVENT);
    }

    private void LevelFinish(FinishResults result)
    {
        float progress = result == FinishResults.Win ? 100f : 0f;

        if (_levelFinised == true && result == FinishResults.Leave)
            return;

        _eventParameters = new Dictionary<string, object>()
        {
            { "level_number",  LevelManager.currentLevel.levelNumber },
            { "level_name", LevelManager.currentLevel.levelName.ToLower() },
            { "level_count", LevelManager.levelCount },
            { "level_loop", LevelManager.levelLoop },
            { "result", result.ToString().ToLower() },
            { "time", Mathf.RoundToInt(ScoreHandler.timeSpentOnLevel) },
            { "progress", progress } //Mathf.FloorToInt((float)Mathf.Clamp(ScoreHandler.curNumberOfThrows, 0, 3) / ScoreHandler.Throws * 100f) }
            
        };
        SendEvent(LEVEL_FINISH_EVENT);

        _levelFinised = true;
    }

    public void ShieldStuck(int count)
    {
        _eventParameters = new Dictionary<string, object>()
        {
            { "count",  count.ToString() }
        };
        SendEvent(SHIELD_STUCK);
    }

    public void RateUs(string reason, int rateResult = 0)
    {
        _eventParameters = new Dictionary<string, object>()
        {
            { "show_reason", reason },
            { "rate_result", rateResult }
        };
        SendEvent(RATE_US);
    }

    public void AdAvailable(AdType type, string placement, string result)
    {
        _eventParameters = new Dictionary<string, object>()
        {
            { "ad_type", type.ToString().ToLower() },
            { "placement", placement },
            { "result", result },
            { "connection", (!(Application.internetReachability == NetworkReachability.NotReachable)).ToString().ToLower() }
        };
        SendEvent(AD_AVAILABLE);
    }

    public void AdStarted(AdType type, string placement, string result)
    {
        _eventParameters = new Dictionary<string, object>()
        {
            { "ad_type", type.ToString().ToLower() },
            { "placement", placement },
            { "result", result },
            { "connection", (!(Application.internetReachability == NetworkReachability.NotReachable)).ToString().ToLower() }
        };
        SendEvent(AD_STARTED);
    }

    public void AdWatch(AdType type, string placement, string result)
    {
        _eventParameters = new Dictionary<string, object>()
        {
            { "ad_type", type.ToString().ToLower() },
            { "placement", placement },
            { "result", result },
            { "connection", (!(Application.internetReachability == NetworkReachability.NotReachable)).ToString().ToLower() }
        };
        SendEvent(AD_WATCH);
    }

    public void PaymentSucceed(Product product, string inappType)
    {
        _eventParameters = new Dictionary<string, object>()
        {
            { "inapp_id", product.definition.id.ToLower() },
            { "currency", product.metadata.isoCurrencyCode.ToLower() },
            { "price", product.metadata.localizedPrice.ToString() },
            { "inapp_type", inappType }
        };
        SendEvent(PAYMENT_SUCCEED);
    }

    public void SkinUnlock(ShopItem item, string unlockType)
    {
        _eventParameters = new Dictionary<string, object>()
        {
            { "skin_type", item.type.ToString().ToLower() },
            { "skin_name", item.name.ToLower() },
            { "skin_rarity", item.rarity.ToString().ToLower() },
            { "unlock_type", unlockType.ToLower() }
        };
        SendEvent(SKIN_UNLOCK);
    }
}