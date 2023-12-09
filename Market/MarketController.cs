using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class MarketController : MonoBehaviour
{
    [SerializeField] private UI_Market _uiMarket;

    private List<IngredientInfoSO> _ingredientInfoSO;
    private List<IngredientInfoData> _ingredientInfoDatas;

    private GameManager _gameManager;
    private MarketData _marketData;
    private PlayerData _playerData;

    private void Awake()
    {
        if (_gameManager == null) _gameManager = GameManager.Instance;
        if (_playerData == null) _playerData = _gameManager.GetPlayerData();
        if (_marketData == null) _marketData = _marketData = _gameManager.GetMarketData();

        Debug.Assert(_gameManager != null, "Null Exception : GameManager");
        Debug.Assert(_playerData != null, "Null Exception : PlayerData");
        Debug.Assert(_marketData != null, "Null Exception : MarketData");

        InitializeIngredientInfoDatas();

        // �� �̺�Ʈ ����
        _uiMarket.OnOrderSubmitted += OrderSubmitted;

        foreach (var slot in _uiMarket.GetMarketItemSlots())
        {
            slot.OnQuantityChange += TotalOrderAmount;
        }

        _uiMarket.OnChangeTab += ChangeTabType;
        _uiMarket.OnSort += ChangeSort;

        // �ʱ� ���� ����
        ChangeSort();
    }

    // ��� ������ �ʱ�ȭ
    private void InitializeIngredientInfoDatas()
    {
        _ingredientInfoDatas = new List<IngredientInfoData>();

        UpdateCurrentIngredientsFromRecipes();

        _ingredientInfoSO = _gameManager.GetMarketData().GetCurrentIngredients();

        List<IngredientInfoData> playerIngredientInfoData = _gameManager.GetPlayerData().GetInventory<IngredientInfoData>();

        foreach (var ingredientData in _ingredientInfoSO)
        {
            int quantity = 0;
            foreach (var ingredientInfoData in playerIngredientInfoData)
            {
                if (ingredientData == ingredientInfoData.DefaultData)
                    quantity += ingredientInfoData.Quantity;
            }

            IngredientInfoData data = new IngredientInfoData(ingredientData, quantity);
            data.PriceAtBuy = _marketData.GetCurrentIngredientPrice(data.DefaultData.name);
            _ingredientInfoDatas.Add(data);
        }

        _uiMarket.InitIngredientSlots(_ingredientInfoDatas);
    }

    // ���� ������ �ִ� ������ ��Ͽ� ���� �� �� �ִ� ��� ��� ������Ʈ
    private void UpdateCurrentIngredientsFromRecipes()
    {
        List<RecipeInfoData> recipeInfoList = _gameManager.GetPlayerData().GetInventory<RecipeInfoData>();
        foreach (RecipeInfoData recipe in recipeInfoList)
        {
            if (recipe.Level >= 1)
            {
                foreach (IngredientInfoSO ingredientRecipe in recipe.DefaultData.IngredientInfoSO)
                {
                    _marketData.UpdateCurrentIngredients(ingredientRecipe.name);
                }
            }
        }
    }

    // �� �ֹ� �ݾ� ���
    private void TotalOrderAmount()
    {
        int totalAmount = _uiMarket.GetMarketItemSlots()
            .Sum(marketitemSlot => marketitemSlot.CurrentQuantity * _ingredientInfoDatas[marketitemSlot.ItemIdx].PriceAtBuy);

        _uiMarket.UpdateTotalOrderAmount(totalAmount);
    }

    // ���õ� �ǿ� ���� ���͸�
    private void ChangeTabType(Enums.MarketTabType tabType)
    {
        foreach (var slot in _uiMarket.GetMarketItemSlots())
        {
            bool shouldSetActive = tabType == Enums.MarketTabType.Total || _ingredientInfoSO[slot.ItemIdx].Type + 1 == (Enums.FoodType)tabType;
            slot.gameObject.SetActive(shouldSetActive);
        }
    }

    // ���ݿ� ���� slot�� ���� ���� ���� 
    private void ChangeSort()
    {
        List<UI_MarketItemSlot> sortedSlots = _uiMarket.GetMarketItemSlots().ToList();

        sortedSlots.Sort((slot1, slot2) =>
        {
            int index1 = slot1.ItemIdx;
            int index2 = slot2.ItemIdx;
            return _ingredientInfoDatas[index1].PriceAtBuy.CompareTo(_ingredientInfoDatas[index2].PriceAtBuy);
        });

        if (!_uiMarket.IsAscendingOrder)
        {
            sortedSlots.Reverse();
        }

        foreach (var slot in sortedSlots)
        {
            slot.transform.SetAsLastSibling();
        }
    }

    // �ֹ��� �Ϸ�Ǿ��� �� ������ ó�� 
    private void OrderSubmitted()
    {
        List<UI_CartSlot> orderedItems = _uiMarket.GetCartSlots().ToList();

        foreach (UI_CartSlot cartSlot in orderedItems)
        {
            int itemIndex = cartSlot.ItemIdx;
            IngredientInfoData orderedItem = _ingredientInfoDatas[itemIndex];

            int quantity = cartSlot.Quantity;
            int price = _marketData.GetCurrentIngredientPrice(orderedItem.DefaultData.name);

            orderedItem.Quantity = quantity;

            _playerData.AddIngredient(orderedItem, price);
        }
    }
}
