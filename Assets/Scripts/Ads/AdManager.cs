using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdManager : MonoBehaviour
{
    public static bool noAdsPurchased;

    public static AdManager current;

    private void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        DontDestroyOnLoad(gameObject);

        MaxSdkCallbacks.OnSdkInitializedEvent += (MaxSdkBase.SdkConfiguration sdkConfiguration) => {
            // AppLovin SDK is initialized, start loading ads
        };

        MaxSdk.SetSdkKey("6AQkyPv9b4u7yTtMH9PT40gXg00uJOTsmBOf7hDxa_-FnNZvt_qTLnJAiKeb5-2_T8GsI_dGQKKKrtwZTlCzAR");
        MaxSdk.InitializeSdk();
    }

    private void Start()
    {
        if (noAdsPurchased == false)
        {
            ApplovinBannerAd.InitializeBannerAds();
            ApplovinInterstitialsAd.current.InitializeInterstitialAds();
            MaxSdkCallbacks.OnBannerAdLoadedEvent += ShowBanner;

            MaxSdkCallbacks.OnInterstitialHiddenEvent += BannerCanBeViewed;
            MaxSdkCallbacks.OnRewardedAdHiddenEvent += BannerCanBeViewed;
        }
        else
        {
            ApplovinInterstitialsAd.isEnabled = false;
            BannerCannotBeViewed();
        }

        ApplovinRewardAd.current.InitializeRewardedAds();
    }

    public static void RemoveAds()
    {
        ApplovinInterstitialsAd.current.CancelInvoke();
        ApplovinInterstitialsAd.isEnabled = false;
        MaxSdkCallbacks.OnBannerAdLoadedEvent -= current.ShowBanner;
        MaxSdk.HideBanner(ApplovinBannerAd.bannerAdUnitId);
        noAdsPurchased = true;
        SaveSystem.SaveData();
    }

    private void ShowBanner(string adUnitId)
    {
        MaxSdk.ShowBanner(ApplovinBannerAd.bannerAdUnitId);
        MaxSdkCallbacks.OnBannerAdLoadedEvent -= ShowBanner;
    }

    public static void BannerCannotBeViewed()
    {
        ApplovinBannerAd.bannerWatchAvailable = false;
    }

    private static void BannerCanBeViewed(string adUnitId)
    {
        ApplovinBannerAd.bannerWatchAvailable = true;
    }
}
