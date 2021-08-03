using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Enums;

public class ShopScrollMenu : MonoBehaviour, IEndDragHandler, IBeginDragHandler
{
    [SerializeField] private SkinType _type;
    public SkinType Type
    {
        get { return _type; }
    }
    private ShopItem[] _items;
    public ShopItem[] Items
    {
        get { return _items; }
    }
    private int _selectedItemIndex = -1;
    public int SelectedItemIndex
    { 
        get { return _selectedItemIndex; }
    }

    [Header("Model Spawn Properties")]
    [SerializeField] private Vector3 _spawnPos = new Vector3(100f, 100f, 100f);
    [SerializeField] private float _spawnInterval = 3f;

    [Header("References")]
    [SerializeField] private Shop _shop = null;
    [SerializeField] private GameObject _cellPrefab = null;
    [SerializeField] private GameObject _modelPrefab = null;
    [SerializeField] private ScrollRect _scrollRect = null;
    [SerializeField] private RectTransform _scrollTransform = null;
    [SerializeField] private RectTransform _layoutElementTransform = null;
    [SerializeField] private GridLayoutGroup _gridLayoutGroup = null;
    [Space]
    [SerializeField] private Button _selectButton = null;
    [SerializeField] private Button _purchaseButton = null;

    private List<RectTransform> _gridChilds = new List<RectTransform>();
    private Coroutine _moveCoroutine;
    
    //public UnityEvent<int> purchaseEvent = new UnityEvent<int>();
    //public UnityEvent<int> selectEvent = new UnityEvent<int>();

    // Start is called before the first frame update
    private void Start()
    {
        switch (_type)
        {
            case SkinType.Hero:
                _items = ItemStorage.current.characterSkins;
                break;
            case SkinType.Shield:
                _items = ItemStorage.current.shieldSkins;
                break;
        }

        GameObject modelHandler = new GameObject("Model Handler");
        int index = 0;

        foreach (ShopItem item in _items)
        {
            GameObject cell = Instantiate(_cellPrefab);
            cell.transform.SetParent(_gridLayoutGroup.transform, false);
            _gridChilds.Add(cell.GetComponent<RectTransform>());

            GameObject prefab = Instantiate(_modelPrefab, _spawnPos, _modelPrefab.transform.rotation, modelHandler.transform);
            GameObject skinModel = null;

            if (item.model != null)
            {
                skinModel = Instantiate(item.model, _spawnPos, Quaternion.identity, prefab.transform);
                skinModel.transform.localRotation = Quaternion.identity;
            }

            if (item.material != null)
                prefab.GetComponentInChildren<Renderer>().material = item.material;

            cell.GetComponentInChildren<ShopCell>().SetProperties(index, prefab, item, this, _scrollTransform, _shop);

            _spawnPos += Vector3.right * _spawnInterval;
            index++;
        }

        float width = _gridLayoutGroup.padding.left + _gridLayoutGroup.padding.right +
            _gridChilds.Count * _gridLayoutGroup.cellSize.x +
            (_gridChilds.Count - 1) * _gridLayoutGroup.spacing.x;
        _layoutElementTransform.sizeDelta = new Vector2(width, _layoutElementTransform.sizeDelta.y);

        _purchaseButton.gameObject.SetActive(false);
        _selectButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        StartCoroutine(SetStartScrollPos());
    }

    private IEnumerator MoveToNearestCell(Vector3 scrollPos, bool waitForScrollStop = false)
    {
        while (_scrollRect.normalizedPosition.x > 0.01f && _scrollRect.normalizedPosition.x < .99f && Mathf.Abs(_scrollRect.velocity.x) > 500f)
            yield return new WaitForEndOfFrame();

        if (waitForScrollStop == true)
        {
            scrollPos = _scrollTransform.anchoredPosition;
            scrollPos.x *= -1f;
        }

        float lastDistance = float.MaxValue;
        float pos = scrollPos.x;

        for(int i = 0; i < _gridChilds.Count; i++)
        {
            float distance = Mathf.Abs(scrollPos.x - _gridChilds[i].localPosition.x);

            if (distance < lastDistance)
            {
                pos = _gridChilds[i].localPosition.x;
                lastDistance = distance;
                _selectedItemIndex = i;
            }
        }

        if (_items[_selectedItemIndex].purchased == true)
        {
            _purchaseButton.gameObject.SetActive(false);
            _selectButton.gameObject.SetActive(true);

            if (_items[_selectedItemIndex].equipped == true)
                _selectButton.interactable = false;
            else
                _selectButton.interactable = true;
        }
        else
        {
            _selectButton.gameObject.SetActive(false);
            _purchaseButton.gameObject.SetActive(true);
            _purchaseButton.interactable = _items[_selectedItemIndex].cost > GameManager.MoneyAmount ? false : true;
        }

        Vector3 targetPos = new Vector3(-pos, scrollPos.y, scrollPos.z);
        float lerpTime = 0f;

        while (lerpTime < 1f)
        {
            _scrollRect.velocity = Vector2.zero;
            _scrollTransform.anchoredPosition = Vector3.Lerp(_scrollTransform.anchoredPosition, targetPos, lerpTime);
            yield return new WaitForEndOfFrame();
            lerpTime += Time.deltaTime;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);

        _purchaseButton.interactable = false;
        _selectButton.interactable = false;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        MoveToCell(_scrollTransform.anchoredPosition, true);
    }

    public void MoveToCell(Vector3 scrollPos, bool waitForScrollStop = false)
    {
        if (_moveCoroutine != null)
            StopCoroutine(_moveCoroutine);

        _purchaseButton.interactable = false;
        _selectButton.interactable = false;
        _moveCoroutine = StartCoroutine(MoveToNearestCell(scrollPos, waitForScrollStop));
    }

    private IEnumerator SetStartScrollPos()
    {
        yield return new WaitForEndOfFrame();

        for(int i = 0; i < _items.Length; i++)
        {
            if (_items[i].equipped == true)
            {
                MoveToCell(_gridChilds[i].localPosition);
                break;
            }
        }
    }
}
