using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApplovinBannerAd : MonoBehaviour
{
    public static string bannerAdUnitId = "f347136aa2dcbba0"; // Retrieve the id from your account

    public static bool bannerWatchAvailable = true;

    public static void InitializeBannerAds()
    {
        // Banners are automatically sized to 320x50 on phones and 728x90 on tablets
        // You may use the utility method `MaxSdkUtils.isTablet()` to help with view sizing adjustments
        MaxSdk.CreateBanner(bannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);

        // Set background or background color for banners to be fully functional
        MaxSdk.SetBannerBackgroundColor(bannerAdUnitId, new Color(1f, 1f, 1f, 0f));

        MaxSdkCallbacks.OnBannerAdLoadedEvent += OnBannerAdLoadedEvent;
    }

    private static void OnBannerAdLoadedEvent(string adUnitId)
    {
        if(bannerWatchAvailable == true)
            AppMetricaEvents.Instance.AdWatch(Enums.AdType.Banner, "continue_play", "watched");
    }
}
