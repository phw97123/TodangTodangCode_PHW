using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_CartSlot : UI_Base
{
    [SerializeField] private Image _icon;
    [SerializeField] private Button _deleteBtn;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _totalPriceText;
    [SerializeField] private TextMeshProUGUI _quantityText;

    private int _basePrice; 

    public event Action<int> OnDeleteBtn;
    
    public int ItemIdx { get; set; }

    public int Quantity { get; private set; }

    private void Awake()
    {
        _deleteBtn.onClick.AddListener(OnDeleteButtonClick); 
    }

    public void Initialize(int itemIdx, string name, int quantity, int price, Sprite icon)
    {
        gameObject.SetActive(false);

        _icon.sprite = icon; 
        ItemIdx = itemIdx;
        _nameText.text = name;
        _quantityText.text = quantity.ToString();
        _basePrice = price; 
        _totalPriceText.text = _basePrice.ToString();
    }

    private void OnDeleteButtonClick()
    {
        SoundManager.Instance.Play(Strings.Sounds.UI_BUTTON);
        OnDeleteBtn.Invoke(ItemIdx); 
    }

    public void UpdateQuantity(int newQuantity)
    {
        Quantity = newQuantity;
        int totalPrice = newQuantity * _basePrice;

        _quantityText.text = Quantity.ToString();
        _totalPriceText.text = totalPrice.ToString();

        if (!gameObject.activeSelf)
        {
            transform.SetAsLastSibling();
        }

        gameObject.SetActive(Quantity > 0);
    }
}