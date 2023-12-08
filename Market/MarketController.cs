using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MarketController : MonoBehaviour
{
    [SerializeField] private UI_Market _uiMarket;

    private List<IngredientInfoSO> _ingredientInfoSO;
    private List<IngredientInfoData> _ingredientInfoDatas;

    private GameManager _gameManager;
    private DataManager _dataManager;
    private MarketData _marketSystem;
    private PlayerData _playerData;

    private void Start()
    {
        if (_dataManager == null) _dataManager = DataManager.Instance;
        if (_gameManager == null) _gameManager = GameManager.Instance;
        if (_playerData == null) _playerData = _gameManager.GetPlayerData();

        Debug.Assert(_dataManager != null, "Null Exception : DataManager");
        Debug.Assert(_gameManager != null, "Null Exception : GameManager");
        Debug.Assert(_playerData != null, "Null Exception : PlayerData");

        _marketSystem = _gameManager.GetMarketSystem();

        _ingredientInfoDatas = new List<IngredientInfoData>();

        List<RecipeInfoData> _recipeInfoList = _gameManager.GetPlayerData().GetInventory<RecipeInfoData>();
        foreach (RecipeInfoData recipe in _recipeInfoList)
        {
            foreach(IngredientInfoSO ingredientRecipe in recipe.DefaultData.IngredientInfoSO) 
            {
                if(recipe.Level >= 1)
                    _marketSystem.UpdateCurrentIngredients(ingredientRecipe.name); 
            }
        }

        _ingredientInfoSO = _gameManager.GetMarketSystem().GetCurrentIngredients();

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
            data.PriceAtBuy = _marketSystem.GetCurrentIngredientPrice(data.DefaultData.name); 
            _ingredientInfoDatas.Add(data);
        }

        _uiMarket.InitIngredientSlot(_ingredientInfoDatas);

        _uiMarket.OrderSubmitted += OnOrderSubmitted;
    }

    public IngredientInfoData GetIngredientData(int itemIdx)
    {
        return _ingredientInfoDatas[itemIdx];
    }

    public IngredientInfoSO GetIngredientInfoSO(int itemIdx)
    {
        return _ingredientInfoSO[itemIdx];
    }

    private void OnOrderSubmitted()
    {
        List<UI_CartSlot> orderedItems = _uiMarket.GetCartSlots().ToList();

        foreach (UI_CartSlot cartSlot in orderedItems)
        {
            int itemIndex = cartSlot.ItemIdx;
            IngredientInfoData orderedItem = _ingredientInfoDatas[itemIndex];

            int quantity = cartSlot.Quantity;
            int price = _marketSystem.GetCurrentIngredientPrice(orderedItem.DefaultData.name); 

            orderedItem.Quantity = quantity;

            _playerData.AddIngredient(orderedItem, price);

            _gameManager.ChangeState(Enums.PlayerDayCycleState.DayEnd); 
            _dataManager.SaveAllData();
        }
    }
}
