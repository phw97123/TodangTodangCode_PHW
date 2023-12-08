using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_MarketItemSlot : UI_Base
{
    [SerializeField] private Image _icon;
    [SerializeField] private Image _typeImage;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _currentQuantityText;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private Button _minusBtn;
    [SerializeField] private Button _plusBtn;
    [SerializeField] private TextMeshProUGUI _quantityText;
    [SerializeField] private TextMeshProUGUI _expirationText;

    public int CurrentQuantity { get; private set; }

    public int ItemIdx {get;set; }

    public event Action OnQuantityChange;

    private void Awake()
    {
        InitBind();

        _quantityText.text = "0"; 
        CurrentQuantity = int.Parse(_quantityText.text);
    }

    private void InitBind()
    {
        _minusBtn.onClick.AddListener(() => UpdateQuantity(-1));
        _plusBtn.onClick.AddListener(() => UpdateQuantity(1));
    }

    public void Initialize(int itemIdx, string name, string expiration,int currentQuantity, Sprite typeImage,int price, Sprite icon) 
    {
        _icon.sprite = icon; 
        ItemIdx = itemIdx;
        _nameText.text = name;
        _expirationText.text = $"유통기한 {expiration}일";
        _typeImage.sprite = typeImage; 
        _priceText.text =price.ToString();
        _currentQuantityText.text = $"보유 {(currentQuantity > 100 ? "100+" : currentQuantity.ToString())}";
    }

    public void UpdateQuantity(int value)
    {
        SoundManager.Instance.Play(Strings.Sounds.UI_BUTTON);

        CurrentQuantity = Mathf.Clamp(CurrentQuantity + value, 0,99);
        _quantityText.text = $"{CurrentQuantity}";

        OnQuantityChange.Invoke();
    }
}
