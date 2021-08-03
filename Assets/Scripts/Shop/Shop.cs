using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Enums;
using TMPro;

public class Shop : MonoBehaviour
{
    [SerializeField] private ShopScrollMenu _characterSkinScroll = null;
    [SerializeField] private ShopScrollMenu _shieldSkinScroll = null;
    private SkinType _type;

    [Header("Animation Properties")]
    private int _activateParamID;
    private int _deactivateParamID;
    [SerializeField] private float _selectorSlideDuration = .2f;

    [Header("References")]
    [SerializeField] private Animator _animator = null;
    [SerializeField] private Button _selectButton = null;
    [SerializeField] private Button _purchaseButton = null;
    [SerializeField] private TextMeshProUGUI _moneyText = null;
    [SerializeField] private Slider _selectorSlider = null;
    //[SerializeField] private GameObject _characterSkinButton = null;
    //[SerializeField] private GameObject _shieldSkinButton = null;

    public UnityEvent<int> selectEvent;
    public UnityEvent<int, bool> purchaseEvent;
    private Coroutine _sliderCoroutine;
    private float _sliderValue;
    private float _targetSliderValue;

    private void Awake()
    {
        _moneyText.text = GameManager.MoneyAmount.ToString();
        selectEvent = new UnityEvent<int>();
        purchaseEvent = new UnityEvent<int, bool>();
        _activateParamID = Animator.StringToHash("activate");
        _deactivateParamID = Animator.StringToHash("deactivate");
        SwitchTab(0);
    }

    private void OnEnable()
    {
        _animator.SetTrigger(_activateParamID);
        GameManager.moneyChangeEvent.AddListener(MoneyChange);
    }

    private void OnDisable()
    {
        GameManager.moneyChangeEvent.RemoveListener(MoneyChange);
    }

    private void LateUpdate()
    {
        _sliderValue = _selectorSlider.value;
    }

    public void CloseWindow()
    {
        _animator.SetTrigger(_deactivateParamID);
    }

    public void DisableWindow()
    {
        gameObject.SetActive(false);
    }

    public void SwitchTab(int index)
    {
        switch(index)
        {
            case 0:
                _shieldSkinScroll.gameObject.SetActive(false);
                _characterSkinScroll.gameObject.SetActive(true);
                _type = _characterSkinScroll.Type;

                if(_sliderCoroutine != null)
                    StopCoroutine(_sliderCoroutine);

                _sliderCoroutine = StartCoroutine(MoveSlider(0f));

                break;
            case 1:
                _characterSkinScroll.gameObject.SetActive(false);
                _shieldSkinScroll.gameObject.SetActive(true);
                _type = _shieldSkinScroll.Type;

                if (_sliderCoroutine != null)
                    StopCoroutine(_sliderCoroutine);

                _sliderCoroutine = StartCoroutine(MoveSlider(1f));

                break;
        }

    }

    private IEnumerator MoveSlider(float value)
    {
        while(Mathf.Approximately(_selectorSlider.value, value) == false)
        {
            _selectorSlider.value = Mathf.Lerp(_selectorSlider.value, value, Time.deltaTime / _selectorSlideDuration);
            yield return new WaitForEndOfFrame();
        }

        _sliderCoroutine = null;
    }

    public void OnBeginDrag()
    {
        _targetSliderValue = _selectorSlider.value;
        _selectorSlider.value = _sliderValue;
    }
    
    public void Drag()
    {
        if (_sliderCoroutine != null)
            StopCoroutine(_sliderCoroutine);

        _targetSliderValue = _selectorSlider.value;
    }

    public void OnEndDrag()
    {
        SwitchTab(Mathf.RoundToInt(_targetSliderValue));
    }

    public void SelectItem()
    {
        switch (_type)
        {
            case SkinType.Hero:
                ItemStorage.selectedCharacterSkin.equipped = false;
                ItemStorage.selectedCharacterSkin = _characterSkinScroll.Items[_characterSkinScroll.SelectedItemIndex];
                ItemStorage.selectedCharacterSkin.equipped = true;
                selectEvent.Invoke(_characterSkinScroll.SelectedItemIndex);
                break;
            case SkinType.Shield:
                ItemStorage.selectedShieldSkin.equipped = false;
                ItemStorage.selectedShieldSkin = _shieldSkinScroll.Items[_shieldSkinScroll.SelectedItemIndex];
                ItemStorage.selectedShieldSkin.equipped = true;
                selectEvent.Invoke(_shieldSkinScroll.SelectedItemIndex);
                break;
        }

        _selectButton.interactable = false;
        SaveSystem.SaveData();
    }

    public void PurchaseItem()
    {
        switch(_type)
        {
            case SkinType.Hero:
                GameManager.MoneyAmount -= _characterSkinScroll.Items[_characterSkinScroll.SelectedItemIndex].cost;
                _characterSkinScroll.Items[_characterSkinScroll.SelectedItemIndex].purchased = true;
                purchaseEvent.Invoke(_characterSkinScroll.SelectedItemIndex, true);
                break;
            case SkinType.Shield:
                GameManager.MoneyAmount -= _shieldSkinScroll.Items[_shieldSkinScroll.SelectedItemIndex].cost;
                _shieldSkinScroll.Items[_shieldSkinScroll.SelectedItemIndex].purchased = true;
                purchaseEvent.Invoke(_shieldSkinScroll.SelectedItemIndex, true);
                break;
        }

        _purchaseButton.gameObject.SetActive(false);
        _selectButton.gameObject.SetActive(true);
        _selectButton.interactable = true;
        SelectItem();
    }

    private void MoneyChange()
    {
        _moneyText.text = GameManager.MoneyAmount.ToString();
    }
}
