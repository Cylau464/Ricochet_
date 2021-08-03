using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;
using UnityEngine.Purchasing;

// This class is intended to be used the the AppsFlyerObject.prefab

public class AppsFlyerObjectScript : MonoBehaviour, IAppsFlyerConversionData
{

    // These fields are set from the editor so do not modify!
    //******************************//
    public string devKey;
    public string appID;
    public bool isDebug;
    public bool getConversionData;
    //******************************//

    public static AppsFlyerObjectScript current;

    void Awake()
    {
        if (current != null && current != this)
        {
            Destroy(gameObject);
            return;
        }

        current = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        // These fields are set from the editor so do not modify!
        //******************************//
        AppsFlyer.setIsDebug(isDebug);
        AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
        //******************************//

        AppsFlyer.startSDK();
    }

    // Mark AppsFlyer CallBacks
    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("didReceiveConversionData", conversionData);
        Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
        // add deferred deeplink logic here
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
        Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
        // add direct deeplink logic here
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }

    public void PurchaseEvent(Product product)
    {
        Dictionary<string, string> purchaseEvent = new Dictionary<string, string>();
        purchaseEvent.Add(AFInAppEvents.CURRENCY, product.metadata.isoCurrencyCode);
        purchaseEvent.Add(AFInAppEvents.REVENUE, product.metadata.localizedPrice.ToString());
        purchaseEvent.Add(AFInAppEvents.QUANTITY, "1");
        purchaseEvent.Add(AFInAppEvents.CONTENT_TYPE, product.metadata.localizedTitle);
        AppsFlyer.sendEvent("af_purchase", purchaseEvent);
    }
}