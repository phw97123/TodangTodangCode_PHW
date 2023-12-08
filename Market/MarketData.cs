using System.Collections.Generic;

public class MarketData
{
    // 기본 데이터
    private IngredientInfoSO[] _ingredientInfoList;

    // 재료 현재 가격
    private Dictionary<string, int> _ingredientPrices;

    // 판매 가능한 재료 
    private Dictionary<string, bool> _isSellable;

    public MarketData(IngredientInfoSO[] ingredientInfoList, MarketSaveData data=null)
    {
        _ingredientInfoList = ingredientInfoList;

        _ingredientPrices = new Dictionary<string, int>();
        _isSellable = new Dictionary<string, bool>();

        if (data != null)
        {
            for (int i = 0; i < data.IngredientPriceStrs.Count; ++i)
            {
                _ingredientPrices.Add(data.IngredientPriceStrs[i],data.IngredientPriceValues[i]);
                _isSellable.Add(data.IsSellableStrs[i],data.IsSellableValues[i]);
            }
        }
    }

    public IngredientInfoSO[] GetIngredientInfoList()
    {
        return _ingredientInfoList;
    }

    public Dictionary<string, int> GetIngredientPrices()
    {
        return _ingredientPrices;
    }

    public Dictionary<string, bool> GetIsSellableDatas()
    {
        return _isSellable;
    }

    public int GetCurrentIngredientPrice(string name)
    {
        return _ingredientPrices[name];
    }

    public void UpdateIngredientPrice(string name, int price)
    {
        if (_ingredientPrices.ContainsKey(name))
        {
            _ingredientPrices[name] = price;
        }
    }

    public List<IngredientInfoSO> GetCurrentIngredients()
    {
        List<IngredientInfoSO> currentIngredients = new List<IngredientInfoSO>();

        foreach (var ingredient in _ingredientInfoList)
        {
            if (_isSellable[ingredient.name] == true)
            {
                currentIngredients.Add(ingredient);
            }
        }

        return currentIngredients;
    }

    public void UpdateCurrentIngredients(string name)
    {
        _isSellable[name] = true;
    }
}
