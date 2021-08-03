using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using TMPro;

public class ShopCell : MonoBehaviour
{
    [SerializeField] private float _increaseScaleMultiplier = 1.5f;
    [SerializeField] private float _scaleChangeOffset = 100f;
    [SerializeField] private float _scaleChangeSpeed = 20f;

    //[SerializeField] private Color _enoughCoinsColor = Color.green;
    //[SerializeField] private Color _notEnoughCoinsColor = Color.red;

    [Header("References")]
    [SerializeField] private RawImage _image = null;
    [SerializeField] private TextMeshProUGUI _titleText = null;
    [SerializeField] private TextMeshProUGUI _costText = null;
    [SerializeField] private TextMeshProUGUI _selectedText = null;
    [SerializeField] private Image _costIcon = null;

    [SerializeField] private Texture _holderTexture = null;

    private int _cellIndex;
    private GameObject _model;
    private ShopScrollMenu _scrollMenu;
    private Shop _shop;
    private int _cameraIndex = -1;
    private ShopItem _item;

    private RectTransform _scrollTransform;
    private RectTransform _rectTransform;
    private RectTransform _imageRectTransform;
    private Camera _camera;

    private Coroutine _checkCoroutine;

    private void OnDisable()
    {
        if (_checkCoroutine != null)
        {
            StopCoroutine(_checkCoroutine);
            _checkCoroutine = null;
        }

        if(_cameraIndex >= 0)
        {
            ResetCamera();
        }

        _shop.selectEvent.RemoveListener(Selected);
        _shop.purchaseEvent.RemoveListener(Purchased);
    }

    private void OnEnable()
    {
        if (_item == null) return;

        _shop.selectEvent.AddListener(Selected);

        if (_item.equipped == true)
        {
            Selected(_cellIndex);
        }
        else
        {
            if (_item.purchased == true)
                Purchased(_cellIndex, false);
            else
                _shop.purchaseEvent.AddListener(Purchased);
        }
    }

    private void Start()
    {
        _rectTransform = GetComponent<RectTransform>();
        _imageRectTransform = _image.GetComponent<RectTransform>();
        _camera = null; //Use Camera.main if used Screen Space Camera
        GameManager.moneyChangeEvent.AddListener(UpdateMoneyAmount);
        _shop.selectEvent.AddListener(Selected);

        if (_item.equipped == true)
        {
            Selected(_cellIndex);
        }
        else
        {
            if (_item.purchased == true)
                Purchased(_cellIndex, false);
            else
                _shop.purchaseEvent.AddListener(Purchased);
        }
    }

    private void Update()
    {
        if (_checkCoroutine == null)
        {
            if(_rectTransform.IsVisibleFrom(_camera) == true)
                _checkCoroutine = StartCoroutine(CheckFreeCamera());
        }
        else if(_rectTransform.IsVisibleFrom(_camera) == false)
        {
            if (_checkCoroutine != null)
            {
                StopCoroutine(_checkCoroutine);
                _checkCoroutine = null;
            }

            if(_cameraIndex >= 0)
                ResetCamera();
        }

        
        float distance = Mathf.Abs(-_scrollTransform.anchoredPosition.x - _rectTransform.localPosition.x);
        float scaleMultiplier = Mathf.Clamp(1 / (distance / _scaleChangeOffset) * _increaseScaleMultiplier, 1f, _increaseScaleMultiplier);
        float scale = Mathf.SmoothStep(_imageRectTransform.localScale.x, scaleMultiplier, Time.deltaTime * _scaleChangeSpeed);
        _imageRectTransform.localScale = Vector3.one * scale;
    }

    private void ResetCamera()
    {
        _image.texture = _holderTexture;
        ShopCameras.current.virtualCameras[_cameraIndex].Follow = null;
        ShopCameras.current.virtualCameras[_cameraIndex].LookAt = null;
        ShopCameras.current.busyCameras[_cameraIndex] = false;
        _cameraIndex = -1;
    }

    public void SetProperties(int index, GameObject model, ShopItem item, ShopScrollMenu scrollMenu, RectTransform scrollTransform, Shop shop)
    {
        _cellIndex = index;
        _model = model;
        _scrollMenu = scrollMenu;
        _titleText.text = item.itemName;
        _item = item;
        _costText.text = _item.cost.ToString();
        //_costText.color = _item.cost > GameManager.MoneyAmount ? _notEnoughCoinsColor : _enoughCoinsColor;
        _scrollTransform = scrollTransform;
        _shop = shop;
    }

    private IEnumerator CheckFreeCamera()
    {
        bool isVisible = false;
        
        while (isVisible == false)
        {
            for (int i = 0; i < ShopCameras.current.busyCameras.Length; i++)
            {
                if (ShopCameras.current.busyCameras[i] == false)
                {
                    ShopCameras.SetCameraTarget(i, _model.transform);
                    _image.texture = ShopCameras.current.cameras[i].targetTexture;
                    isVisible = true;
                    _cameraIndex = i;
                    break;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void ChooseCell()
    {
        _scrollMenu.MoveToCell(_rectTransform.localPosition);
    }

    private void UpdateMoneyAmount()
    {
        //if(_item.purchased == false)
        //    _costText.color = _item.cost > GameManager.MoneyAmount ? _notEnoughCoinsColor : _enoughCoinsColor;
    }

    private void Purchased(int index, bool firstUnlock)
    {
        if(index == _cellIndex)
        {
            _costIcon.enabled = false;
            _costText.enabled = false;
            _selectedText.enabled = true;
            _selectedText.text = "Unlocked";

            if(firstUnlock == true)
                AppMetricaEvents.Instance.SkinUnlock(_item, "purchase_" + _item.cost);
        }
    }

    private void Selected(int index)
    {
        if (index == _cellIndex)
        {
            _costIcon.enabled = false;
            _costText.enabled = false;
            _selectedText.enabled = true;
            _selectedText.text = "Selected";
        }
        else if(_selectedText.text == "Selected")
        {
            _selectedText.text = "Unlocked";
        }
    }
}
