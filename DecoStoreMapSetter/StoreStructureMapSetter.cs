using System.Collections.Generic;
using UnityEngine;

public class StoreStructureMapSetter : MonoBehaviour
{
    [SerializeField] private GameObject _tv;
    [SerializeField] private GameObject _plant;
    [SerializeField] private GameObject _floorTile;
    [SerializeField] private GameObject _defaultTile;
    [SerializeField] private GameObject _tableAndChair;

    private Dictionary<int, IDecoObject> _decoObjectMap = new Dictionary<int, IDecoObject>();
    private DecoStoreData _decoStoreData;

    private void Start()
    {
        _decoStoreData = GameManager.Instance.GetDecoStore();

        if (_decoStoreData != null)
        {
            _decoStoreData.OnDecorationBought += HandleDecorationBought;
        }

        InitObejectMap();
        InitActivateObject();
    }

    private void InitObejectMap()
    {
        if (_tableAndChair != null)
            _decoObjectMap.Add(0, new ActivateDecoObject(_tableAndChair));
        if (_plant != null)
            _decoObjectMap.Add(1, new ActivateDecoObject(_plant));
        if (_tv != null)
            _decoObjectMap.Add(2, new ActivateDecoObject(_tv));
        if (_tableAndChair != null && _defaultTile != null)
            _decoObjectMap.Add(3, new ToggleActivateDecoObject(_floorTile, _defaultTile));

        foreach (var kvp in _decoObjectMap)
        {
            if (kvp.Value == null)
            {
                Debug.LogError($"_decoObjectMap의 키 {kvp.Key} 가 null 입니다");
            }
        }
    }

    private void InitActivateObject()
    {
        List<StoreDecorationInfoData> storeDecoDatas = _decoStoreData.GetAllStoreDecoData();

        if (storeDecoDatas == null)
        {
            Debug.LogWarning("StoreDecorationInfoData 가 null 입니다.");
        }

        foreach (var decoData in storeDecoDatas)
        {
            int id = decoData.DefaultData.ID;

            IDecoObject decoObject;

            if (!_decoObjectMap.ContainsKey(id))
            {
                if(id != 3)
                    Debug.LogError($"{id}, {decoData.DefaultData.name}");
            }

            if (_decoObjectMap.TryGetValue(id, out decoObject) && decoObject != null)
            {

                if (decoData.IsSoldOut)
                {
                    decoObject.SetActivate(true);
                }
                else
                {
                    decoObject.SetActivate(false);
                }
            }
        }
    }

    private void HandleDecorationBought(int id)
    {
        IDecoObject decoObject;

        if (_decoObjectMap.TryGetValue(id, out decoObject))
        {
            decoObject.SetActivate(true);
        }
    }
}
