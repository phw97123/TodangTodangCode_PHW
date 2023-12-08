using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class UI_Market : UI_Base
{
    [SerializeField] private GameObject _marKetItemContent;

    [SerializeField] private Button[] _tabBtn;
    [SerializeField] private Sprite[] _defaultTabSprite;
    [SerializeField] private Sprite[] _selectedTabSprite;
    [SerializeField] private Button _sortBtn;
    [SerializeField] private TextMeshProUGUI _sortBtnText;
    private UI_MarketItemSlot[] _marketItemSlots;

    [SerializeField] private GameObject _cartContent;
    [SerializeField] private TextMeshProUGUI _totalOrderAmountText;
    [SerializeField] private TextMeshProUGUI _playerMoneyText;
    [SerializeField] private Button _orderBtn;
    [SerializeField] private Button _exitBtn;
    private UI_CartSlot[] _cartSlots;

    [SerializeField] private MarketController _controller;

    private SoundManager _soundManager;

    private Enums.MarketTabType _selectedTab = Enums.MarketTabType.Total;
    private bool _isAscendingOrder = true;

    private int _playerMoney;
    private int _totalOrderAmount;

    public event Action OrderSubmitted;
    public event Action<bool> OnTutorialClosed;


    private void Start()
    {
        _playerMoney = GameManager.Instance.GetPlayerData().Money;
        _soundManager = SoundManager.Instance;

        _totalOrderAmount = 0;

        InitBind();
        SetSelectedTab(Enums.MarketTabType.Total);
    }

    public override void OpenUI(bool isSound = true, bool isAnimated = true)
    {
        base.OpenUI(isSound, isAnimated);
    }

    public override void CloseUI(bool isSound = true, bool isAnimated = true)
    {

        UIManager.Instance.ShowPopup<UI_DefaultPopup>(
            new PopupParameter(
                content: Strings.Market.EXITCONFIRM
                , confirmCallback: Exit
                , cancelCallback: null
                )
            );  ;
    }

    private void InitBind()
    {
        for (int i = 0; i < _tabBtn.Length; i++)
        {
            int index = i;
            _tabBtn[i].onClick.AddListener(() => OnTabButtonClick((Enums.MarketTabType)index));
        }

        _sortBtn.onClick.AddListener(OnSortButtonClick);
        _orderBtn.onClick.AddListener(OnOrderButtonClick);
        _exitBtn.onClick.AddListener(()=>CloseUI(true));

        _playerMoneyText.text = _playerMoney.ToString();
        _totalOrderAmountText.text = _totalOrderAmount.ToString();

        _orderBtn.interactable = false;
    }

    public void InitIngredientSlot(List<IngredientInfoData> data)
    {
        _marketItemSlots = new UI_MarketItemSlot[data.Count()];
        _cartSlots = new UI_CartSlot[data.Count()];

        for (int i = 0; i < data.Count(); i++)
        {
            InitializeMarketItemSlot(data[i], i);
            InitializeCartSlot(data[i], i);
        }

        SortMarketItemSlots();
    }

    private void InitializeMarketItemSlot(IngredientInfoData data, int index)
    {
        GameObject marketItemSlot = Instantiate(Resources.Load<UI_MarketItemSlot>(Strings.Prefabs.UI_MARKETITEMSLOT), _marKetItemContent.transform).gameObject;
        _marketItemSlots[index] = marketItemSlot.GetComponent<UI_MarketItemSlot>();

        Sprite foodType = GetFoodTypeIcon(data.DefaultData.Type);

        int currentQuantity = data.Quantity;

        _marketItemSlots[index].Initialize(index, data.DefaultData.Name, data.DefaultData.BaseExpirationDate.ToString(), currentQuantity, foodType, data.PriceAtBuy, data.DefaultData.IconSprite);
    }

    private void InitializeCartSlot(IngredientInfoData data, int index)
    {
        GameObject cartSlot = Instantiate(Resources.Load<UI_CartSlot>(Strings.Prefabs.UI_CARTSLOT), _cartContent.transform).gameObject;

        _cartSlots[index] = cartSlot.GetComponent<UI_CartSlot>();
        _cartSlots[index].Initialize(index, data.DefaultData.Name, 0, data.PriceAtBuy, data.DefaultData.IconSprite);

        _cartSlots[index].OnDeleteBtn += HandleCartSlotDelete;

        _marketItemSlots[index].OnQuantityChange += () => _cartSlots[index].UpdateQuantity(_marketItemSlots[index].CurrentQuantity);
        _marketItemSlots[index].OnQuantityChange += UpdateTotalOrderAmount;
    }

    private Sprite GetFoodTypeIcon(Enums.FoodType type)
    {
        string iconPath = (type == Enums.FoodType.Ricecake) ? Strings.Sprites.MARKET_RICECAKE_TYPE_ICON : Strings.Sprites.MARKET_TEA_TYPE_ICON;
        return ResourceManager.Instance.LoadSprite(iconPath);
    }

    private void HandleCartSlotDelete(int index)
    {
        _marketItemSlots[index].UpdateQuantity(-_marketItemSlots[index].CurrentQuantity);
    }

    private void UpdateTotalOrderAmount()
    {
        _totalOrderAmount = 0;

        foreach (var item in _marketItemSlots)
        {
            int quantity = item.CurrentQuantity;
            int price = _controller.GetIngredientData(item.ItemIdx).PriceAtBuy;

            int subtotal = price * quantity;
            _totalOrderAmount += subtotal;
        }

        _totalOrderAmountText.text = _totalOrderAmount.ToString();

        _orderBtn.interactable = !SetOrderButtonActivation();
    }

    private bool SetOrderButtonActivation()
    {
        return _cartSlots.All(slot => !slot.gameObject.activeSelf) || _playerMoney < _totalOrderAmount;
    }

    private void OnTabButtonClick(Enums.MarketTabType tabType)
    {
        _soundManager.Play(Strings.Sounds.UI_BUTTON); 

        SetSelectedTab(tabType);

        foreach (var slot in _marketItemSlots)
        {
            bool shouldSetActive = tabType == Enums.MarketTabType.Total || _controller.GetIngredientInfoSO((slot.ItemIdx)).Type + 1 == (Enums.FoodType)tabType;
            slot.gameObject.SetActive(shouldSetActive);
        }
    }

    private void SetSelectedTab(Enums.MarketTabType tabType)
    {
        ChangeTabImage(_selectedTab, _defaultTabSprite);
        _selectedTab = tabType;
        ChangeTabImage(tabType, _selectedTabSprite);
    }

    private void ChangeTabImage(Enums.MarketTabType tabType, Sprite[] newImage)
    {
        _tabBtn[(int)tabType].GetComponent<Image>().sprite = newImage[(int)tabType];
    }

    private void OnSortButtonClick()
    {
        _soundManager.Play(Strings.Sounds.UI_BUTTON);
        _isAscendingOrder = !_isAscendingOrder;
        SortMarketItemSlots();
    }

    private void SortMarketItemSlots()
    {
        List<UI_MarketItemSlot> sortedSlots = _marketItemSlots.OrderBy(slot => _controller.GetIngredientData(GetItemIndex(slot)).PriceAtBuy).ToList();

        if (!_isAscendingOrder)
        {
            sortedSlots.Reverse();
        }

        foreach (var slot in sortedSlots)
        {
            slot.transform.SetAsLastSibling();
        }

        _sortBtnText.text = _isAscendingOrder == true ? "가격순 ▲" : "가격순 ▼";
    }

    private int GetItemIndex(UI_MarketItemSlot slot)
    {
        return Array.IndexOf(_marketItemSlots, slot);
    }

    private void OnOrderButtonClick()
    {
        if (_playerMoney >= _totalOrderAmount)
        { 
            UIManager.Instance.ShowPopup<UI_DefaultPopup>(
                new PopupParameter(
                    content: Strings.Market.ORDERCONFIRM
                    , confirmCallback: Order
                    , cancelCallback: null
                    )
                );
        }
    }

    private void Order()
    {
        _soundManager.Play(Strings.Sounds.UI_BUYANDSELL); 
        OrderSubmitted?.Invoke();
        Exit(); 
    }

    private void Exit()
    {
        OnTutorialClosed?.Invoke(true);

        GameManager.Instance.ChangeState(Enums.PlayerDayCycleState.DayEnd);
        DataManager.Instance.SaveAllData();
        base.CloseUI();
    }

    public List<UI_CartSlot> GetCartSlots()
    {
        List<UI_CartSlot> cartSlot = new List<UI_CartSlot>();
        foreach (var slot in _cartSlots)
        {
            if (slot.gameObject.activeSelf)
            {
                cartSlot.Add(slot);
            }
        }

        return cartSlot;
    }
}