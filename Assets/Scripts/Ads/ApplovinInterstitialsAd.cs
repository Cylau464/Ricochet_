using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplovinInterstitialsAd: MonoBehaviour
{
    [SerializeField] private float _adCooldown = 25f;
    public static bool isEnabled = true;
    public static string adUnitId = "562b2f1604ffacdf";
    private int _retryAttempt;

    public static ApplovinInterstitialsAd current;

    private void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        DontDestroyOnLoad(gameObject);
    }

    public void InitializeInterstitialAds()
    {
        // Attach callback
        MaxSdkCallbacks.OnInterstitialLoadedEvent += OnInterstitialLoadedEvent;
        MaxSdkCallbacks.OnInterstitialLoadFailedEvent += OnInterstitialFailedEvent;
        MaxSdkCallbacks.OnInterstitialAdFailedToDisplayEvent += InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.OnInterstitialHiddenEvent += OnInterstitialDismissedEvent;

        // Load the first interstitial
        LoadInterstitial();
    }

    private void LoadInterstitial()
    {
        MaxSdk.LoadInterstitial(adUnitId);
    }

    private void OnInterstitialLoadedEvent(string adUnitId)
    {
        // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(adUnitId) will now return 'true'

        // Reset retry attempt
        _retryAttempt = 0;
    }

    private void OnInterstitialFailedEvent(string adUnitId, int errorCode)
    {
        // Interstitial ad failed to load 
        // We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds)

        _retryAttempt++;
        double retryDelay = Mathf.Pow(2, Mathf.Min(6, _retryAttempt));

        Invoke("LoadInterstitial", (float)retryDelay);
    }

    private void InterstitialFailedToDisplayEvent(string adUnitId, int errorCode)
    {
        // Interstitial ad failed to display. We recommend loading the next ad
        LoadInterstitial();
    }

    private void OnInterstitialDismissedEvent(string adUnitId)
    {
        // Interstitial ad is hidden. Pre-load the next ad
        LoadInterstitial();
        Invoke(nameof(PauseEnd), _adCooldown);
        isEnabled = false;
    }

    private void PauseEnd()
    {
        isEnabled = true;
    }
}
