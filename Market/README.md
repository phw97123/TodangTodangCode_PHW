# Market (재료주문)


## 개요

- MVC 패턴을 활용하여 UI를 구성하는 View와 데이터 가공 및 처리를 담당하는 Controller를 명확하게 분리함으로써 코드 가독성을 높인다


## 기술 도입 배경

데이터를 표시하기 위해서는 다양한 연산과 로직이 필요했는데, 이러한 복잡한 과정을 하나의 스크립트 안에서 처리하면 


코드 가독성이 떨어지고 유지 보수가 어려웠다 


이를 해결하기 위해 MVC 패턴을 도입하여 모델, 뷰, 컨트롤러로 역할을 명확히 나누어 코드를 구성함으로써, 각 부분이 독립적으로 동작할 수 있게 해주었다 


## 이점

- 가독성 향상: 코드의 구조가 명확하게 나누어져, 각 부분이 독립적으로 동작하도록 설계되어 View는 UI 디자인에 , Controller는 데이터 가공 및 입력 처리에 집중하여 가독성을 향상시킬 수 있었다
- 유지 보수성 : View와 Controller가 각자의 역할에 집중하게 되어, 변경이나 업데이트되었을 때 대응하기 용이해지고 하나의 부분을 수정해도 다른 부분에 미치는 영향이 최소화되었다

## 주요코드

```csharp
MarketController.cs

private void Awake()
{
     if (_gameManager == null) _gameManager = GameManager.Instance;
     if (_playerData == null) _playerData = _gameManager.GetPlayerData();
     if (_marketData == null) _marketData = _marketData = _gameManager.GetMarketData();

     Debug.Assert(_gameManager != null, "Null Exception : GameManager");
     Debug.Assert(_playerData != null, "Null Exception : PlayerData");
     Debug.Assert(_marketData != null, "Null Exception : MarketData");

     // 데이터 가공
     InitializeIngredientInfoDatas();

     // View 초기화
     _uiMarket.InitIngredientSlots(_ingredientInfoDatas);

     // 각 이벤트 구독
     _uiMarket.OnOrderSubmitted += OrderSubmitted;

     foreach (var slot in _uiMarket.GetMarketItemSlots())
     {
         slot.OnQuantityChange += TotalOrderAmount;
     }

     _uiMarket.OnChangeTab += ChangeTabType;
     _uiMarket.OnSort += ChangeSort;

    ...

 }
```

---

## 트러블 슈팅

### 문제

- 전체 View에서 다른 View(Slot)들을 생성하는 과정에서 상호 참조 문제 발생
- 전체 View는 Controller의 역할을 수행하게 되어, 생성된 Slot들에게 데이터를 주입하기 위해 Controller를 알아야 하는 의존성이 발생

### 생각

- Controller가 Slot들을 생성하게 한다?
    
    → 데이터 가공을 하거나 입력 처리를 통해 생성되는 게 아니기 때문에 역할이 모호해질 수 있다 
    
- View의 독립성을 높이기 위해 Controller만이 View를 알고, View는 Controller의 존재를 모르게 하자.

### 해결

- Controller와 전체 UI의 상호 참조를 지우고 Controller만이 View를 알게 하여 전체 View와 Slot들을 Controller가 데이터를 넣어주어 초기화를 진행시켜준다.
- Controller는 View의 이벤트에 구독하여 입력 처리가 발생하면 View가 생성한 Slot들의 리스트를 가져와 처리해준다
