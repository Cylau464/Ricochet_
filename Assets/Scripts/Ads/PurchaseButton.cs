using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class PurchaseButton : MonoBehaviour
{
    private string _removeAds = "puzzle.flying.shield.removeads";

    private Product _product;
    private bool _purchaseCompleted;
    private Coroutine _waitCoroutine;

    private void Start()
    {
        if (AdManager.noAdsPurchased == true)
            gameObject.SetActive(false);
    }

    public void OnPurchaseComplete(Product product)
    {
        if(product.definition.id == _removeAds)
        {
            AdManager.RemoveAds();
            _product = product;
            _purchaseCompleted = true;

            StartCoroutine(PurchaseThrowFix());
        }
    }

    public void OnPurchaseFailder(Product product, PurchaseFailureReason reason)
    {
        if (_waitCoroutine != null)
        {
            StopCoroutine(_waitCoroutine);
            _waitCoroutine = null;
        }
    }

    private IEnumerator PurchaseThrowFix()
    {
        yield return new WaitForEndOfFrame();
        gameObject.SetActive(false);
    }

    public void SendPurchaseEvent()
    {
        _waitCoroutine = StartCoroutine(WaitPurchaseComplete());
    }

    private IEnumerator WaitPurchaseComplete()
    {
        while (_purchaseCompleted == false)
            yield return new WaitForEndOfFrame();

        if (_product != null)
        {
            AppsFlyerObjectScript.current.PurchaseEvent(_product);
            AppMetricaEvents.Instance.PaymentSucceed(_product, "no_ads");
        }
        else
        {
            Debug.LogError("Product is null!");
        }

        _waitCoroutine = null;
    }
}
