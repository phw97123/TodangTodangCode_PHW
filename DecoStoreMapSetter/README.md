## 확장성을 고려한 DecoStore GameObject 배치


## 개요

- Deco 상점에서 구매한 꾸미기 아이템을 Scene에 보여준다
- 새로운 아이템이 추가될 가능성을 열어두어 확장성을 고려한다
  

## 주요 코드

```csharp
private Dictionary<int, GameObject> _decoObjectDic;

 private void Awake()
 {
     if (_gameManager == null) _gameManager = GameManager.Instance;
     if (_decoStoreData == null) _decoStoreData = _gameManager.GetDecoStore();

     Debug.Assert(_gameManager != null, "Null Exception : GameManager");
     Debug.Assert(_decoStoreData != null, "Null Exception : DecoStoreData");

     _decoObjectDic = new Dictionary<int, GameObject>();

     Transform[] childTransforms = transform.GetComponentsInChildren<Transform>();
     Debug.Assert(childTransforms != null, "Null Exception : childTransforms");

     ...
 }

private void Start()
{
     if (_decoStoreData != null)
     {
         _decoStoreData.OnDecorationBought += HandleDecorationBought;
     }

   ...
}

private void HandleDecorationBought(int id)
 {
     StoreDecorationInfoData data = _decoStoreData.GetStoreDecoData(id);

     Debug.Assert(data != null, "Null Exception : StoreDecorationInfoData");

     if (_decoObjectDic.TryGetValue(id, out GameObject obj))
     {
         obj.gameObject.SetActive(data.IsSoldOut);
     }
 }

private void OnDestroy()
{
    _decoObjectDic.Clear();
}
```

- Dictionary로 데이터를 관리하여 검색 속도를 높인다
- 하위에 있는 오브젝트들을 가져와서 Dictionary에 ID를 키로 저장하고 아이템을 구매했다는 이벤트가 발생하면 활성화한다
- OnDestroy 메서드를 사용하여 Dictionary를 정리하여 메모리 누수를 방지한다

---

## 트러블 슈팅

### 시도

- 오브젝트 생성 부분
    - 아이템을 구매할 때마다 각 ID에 맞는 프리팹을 생성
    
    → 많은 양의 아이템이 동적으로 생성되면 메모리 사용량이 높아질 수 있다
    
    → 생성하는 동안에는 일시적인 지연이 발생할 수도 있다
    
    → 동적으로 생성된 오브젝트가 삭제되면 메모리에 일부 공간이 할당되어 메모리 단편화가 발생할 수도 있다 
    

- ID 매핑 부분
    - 조건문을 이용하여 각 ID와 오브젝트 연결
    
    → 새로운 아이템이 추가되면 조건문이 추가되어 확장성이 떨어짐 
    
    - List를 이용하여 Inspector창에 각 ID 순서대로 할당
    
    → 새로운 아이템이 추가되어도 코드를 수정하지 않아도 되지만 List의 순서가 변경되는 상황이 발생할 수 있다 
    
    → 연속된 ID 만을 사용하지 않는다면 코드를 수정해야한다 
    

### 해결

- 오브젝트 생성 부분
    - 맵에 미리 배치해두고 아이템을 구매하면 활성화, 구매하지 않았다면 비활성화
    
    → 메모리에 로드되어 있으므로 초기 로딩 시간은 길어질 수 있으나 게임 중에는 필요한 오브젝트만 활성화하여 특정 순간의 자원 사용은 낮아진다 
    
    → 오브젝트를 동적으로 생성하는 것보다 더 빠르다 
    
- ID 매핑 부분
    - 오브젝트의 ID를 이름으로 설정하여 저장하고, 각 ID에 맞는 오브젝트를 Dictionary에 저장하여 데이터의 ID에 맞는 오브젝트를 검색
    
    → 오브젝트의 이름만 ID에 맞춰서 설정하여 배치만 해주면 별도의 코드 수정 없이도 새로운 아이템을 간편하게 추가할 수 있다
    

## 활용

![스크린샷 2023-12-10 032214.png](https://github.com/phw97123/TodangTodangCode_PHW/assets/132995834/42401187-f7bc-4936-acf0-1a83467022c5)

```csharp
foreach (Transform child in childTransforms)
     {
         string childName = child.name;

         int num;
         if (int.TryParse(childName.Split('_')[0], out num))
         {
             _decoObjectDic.Add(num, child.gameObject);
         }
     }
```

오브젝트를 미리 맵에 배치한 후 맨 앞 글자를 데이터의 ID로 설정해두고 가져올 때 ID만 따로 가져와서 저장
