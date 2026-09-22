---
name: unity-stack-scaffold
description: Scaffold or refactor Unity C# classes (managers, systems, services) using this project's established stack — VContainer for DI, UniTask for async, MessagePipe for pub/sub events, R3 for reactive state, ZLogger.Unity for logging, ZString for string building, DOTween for tweening. Use this whenever the user asks to create a new manager/system/service class, wants to "이 스택으로" build or refactor something, mentions VContainer/UniTask/MessagePipe/R3/ZLogger/ZString/DOTween by name, or asks to convert a coroutine/event/singleton pattern to the project's DI+async style — even if they just say "매니저 하나 만들어줘" without naming the libraries. The concrete conventions here come from this project's own Packages/com.huliacdev.template code (RootLifetimeScope, GameManagerBase, SoundManager), not generic library docs, so prefer this skill over general Unity/C# knowledge for this stack.
---

# Unity 스택 스캐폴딩 (VContainer / UniTask / MessagePipe / R3 / ZLogger / ZString)

이 스킬은 라이브러리 사용법을 처음부터 설계하는 게 아니라, 이 프로젝트의 `Packages/com.huliacdev.template` (RootLifetimeScope.cs, GameManagerBase.cs, SoundManager.cs)에 이미 정착된 실제 패턴을 재현하기 위한 것이다. 왜 이 방식인지 궁금하면 해당 파일들을 직접 열어 대조해도 된다.

이 스킬은 "어떤 라이브러리를 어떤 순서/형태로 조합하는가"만 다룬다. `GetComponent` 대신 `TryGetComponent`, null 비교 시 암시적 bool, `var` 금지 같은 규칙은 프로젝트 CLAUDE.md가 이미 항상 적용하고 있으므로 여기서 반복하지 않는다.

## 1. HuliacDev 템플릿 우선 재사용

새 기능을 처음부터 작성하기 전에, `com.huliacdev.template` 패키지(`HuliacDev.App` / `HuliacDev.Core` / `HuliacDev.UI` / `HuliacDev.Utils` / `HuliacDev.Data` / `HuliacDev.Hardware` / `HuliacDev.Network` 네임스페이스)에 이미 그 역할을 하는 베이스 클래스나 유틸리티가 있는지 먼저 확인한다. 없는 걸 새로 만드는 것보다 있는 걸 최대한 활용하는 게 우선이다.

- 씬/전역 매니저는 `MonoBehaviour`를 직접 상속하지 않고 `HuliacDev.Core.GameManagerBase`를 상속한다 (`GameManagerBase`는 제네릭이 아닌 일반 추상 클래스임). `SingletonGuard<GameManagerBase>`를 통한 싱글톤 보존, InputAction(디버그/인스펙터/마우스 토글) 연결, Reporter/RuntimeInspector 디버그 UI 연동, `AppSettingsProvider`를 통한 Settings 비동기 로드 및 `OnSettingsLoaded(Settings)` 가상 메서드 초기화가 이미 구현되어 있으므로 중복 구현하지 않는다.
- 사운드/UI/페이드/비디오 관련 요청이면 새로 만들기 전에 `HuliacDev.UI.SoundManager` / `UIManager` / `FadeManager` / `VideoManager`가 이미 그 역할을 하는지 먼저 확인하고, 있으면 확장하거나 그대로 호출한다.
- 설정/데이터 파일 로딩은 직접 파일 I/O나 JSON 파싱을 짜지 않고 `HuliacDev.Utils.JsonLoader.Load<T>` / `LoadAsync<T>` (`where T : new()`, `.json` 확장자 자동 처리, 실패 시 `new T()` 반환) 또는 VContainer에 등록된 `AppSettingsProvider`를 재사용한다.
- 전역 DI/로깅/MessagePipe 브로커 등록이 필요하면 완전히 새로운 LifetimeScope를 만들기보다 `HuliacDev.App.RootLifetimeScope`를 상속해 필요한 구성 메서드(`ConfigureLogging`, `ConfigureLogRetention`, `ConfigureMessagePipe`, `ConfigureSettings`, `ConfigureNetwork`, `ConfigureCoreComponents`, `ConfigureOptionalComponents`)만 override하는 걸 우선 고려한다.
- 템플릿에 대응되는 게 없는 완전히 새로운 기능일 때만 아래 2~8번 패턴을 따라 처음부터 작성한다.

## 2. VContainer — DI 등록

- 전역 등록은 `LifetimeScope.Configure(IContainerBuilder builder)`에서 하되, 관심사별로 `ConfigureLogging(builder)`, `ConfigureMessagePipe(builder)`처럼 **private/protected 메서드로 쪼갠다.** 하나의 Configure가 모든 걸 다 하지 않도록 하는 이유는, 나중에 로깅만 바꾸거나 파생 LifetimeScope에서 특정 부분만 override하기 쉽게 하기 위함이다.
- 개별 컴포넌트는 생성자 주입이 아니라 **메서드 주입**을 쓴다:
  ```csharp
  [Inject]
  public void Construct(IPublisher<SomeEvent> publisher, ILogger<MyManager> logger)
  {
      _publisher = publisher;
      _logger = logger;
  }
  ```
  MonoBehaviour는 생성자를 직접 호출할 수 없어서 이 패턴이 필요하다.

## 3. UniTask — 비동기 처리

- Fire-and-forget 초기화/연출은 `async UniTaskVoid` + 호출부에서 `.Forget()`:
  ```csharp
  protected virtual async UniTaskVoid InitializeAsync()
  {
      await DoSomethingAsync(this.GetCancellationTokenOnDestroy());
  }
  // 호출부: InitializeAsync().Forget();
  ```
  (참고: `GameManagerBase` 파생 클래스에서 세팅 완료 후 초기화가 필요한 경우, 직접 로드하기보다 `protected override void OnSettingsLoaded(Settings loadedSettings)`를 override하여 처리한다.)
- 취소 토큰은 기본적으로 `this.GetCancellationTokenOnDestroy()`를 쓴다. 오브젝트가 파괴되면 자동으로 취소되어 별도 정리 코드가 필요 없다.
- 사용자가 도중에 취소할 수 있는 흐름(페이드, 연출 등)은 별도 `CancellationTokenSource`를 만들어 관리한다:
  ```csharp
  _fadeCts = CancellationTokenSource.CreateLinkedTokenSource(this.GetCancellationTokenOnDestroy());
  FadeOutAsync(duration, _fadeCts).Forget();
  ```
  이렇게 링크하면 오브젝트 파괴 시 자동 취소와 수동 취소(`_fadeCts.Cancel()`) 둘 다 동작한다. `try { ... } catch (OperationCanceledException) { ... } finally { _fadeCts?.Dispose(); _fadeCts = null; }` 형태로 정리한다.
- **다중 대기 태스크 캐싱은 `UniTask` 대신 `Task`를 사용한다**: `UniTask`는 구조체 기반이라 완료 후 여러 소비자가 동시에 `await`하면 continuation이 중복 등록되어 `InvalidOperationException("Already continuation registered")`가 발생한다. 따라서 같은 리소스를 여러 곳에서 동시에 요청할 수 있는 로더/다운로더는 다중 awaiter를 지원하는 `Dictionary<string, Task<T>>`로 캐싱한다 (`SoundManager` 및 `AppSettingsProvider`의 `_activeDownloads` 패턴 참고).

## 4. MessagePipe — 이벤트

- 전역 브로커는 LifetimeScope에서 `builder.RegisterMessageBroker<TEvent>(options)`로 등록한다.
- 발행은 `IPublisher<TEvent>`, 구독은 `ISubscriber<TEvent>`를 주입받아 사용한다.
- 구독은 반드시 `IDisposable`을 받아 `OnDestroy` 등에서 해제한다. 구독 해제를 빼먹으면 파괴된 오브젝트가 이벤트를 계속 받으려다 예외가 난다.

## 5. R3 — 반응형 상태

- 외부에서 관찰해야 하는 상태(체력, 스코어, UI 바인딩 대상 등)는 일반 필드 대신 `ReactiveProperty<T>`로 노출한다.
- 구독은 `.Subscribe(...)`가 반환하는 `IDisposable`을 모아서(`DisposableBag` 등) 오브젝트 파괴 시 한 번에 해제한다. MessagePipe 구독과 동일하게, 해제를 빼먹지 않는 게 핵심이다.

## 6. ZLogger.Unity — 로깅

- 필드로 `ILogger<T> _logger`를 주입받는다.
- 로그를 남길 때는 항상 이 형태를 따른다:
  ```csharp
  if (_logger != null) _logger.ZLogWarning($"[MyManager] 대상이 null이라 처리를 건너뜀.");
  ```
  - `[클래스명]` 태그를 메시지 맨 앞에 붙인다 — 여러 매니저의 로그가 한 콘솔에 섞여도 어디서 난 건지 바로 구분하기 위함.
  - null이 발생할 수 있는 지점(Fallback이 필요한 지점)마다 `_logger != null` 체크 후 경고/에러 로그를 남긴다. 로그 없이 조용히 return하지 않는다.
- **if로 조건/참조를 검사하는 모든 곳은 실패 분기(else)에 로그를 남긴다.** `if (someRef != null) { ... }`처럼 성공 분기만 작성하고 else를 생략하면, 조건이 실패했을 때 아무 흔적 없이 조용히 아무 일도 안 일어난다 — 원인 파악이 콘솔 로그가 아니라 코드 리딩으로만 가능해진다.
  ```csharp
  if (flowController != null)
  {
      flowController.ShowCompletePanel();
  }
  else if (_logger != null)
  {
      _logger.ZLogWarning($"[ResultVideoPanel] flowController is null. CompletePanel will not fade in.");
  }
  ```
  실제로 `ResultVideoPanel`에서 `flowController`가 씬에 연결되지 않았는데 else 로그가 없어서, 영상 재생이 끝나도 CompletePanel이 페이드인되지 않는 원인을 로그 없이 코드까지 뒤져서 찾아야 했던 사례가 있었다. 조건이 맞지 않는 경우가 "정상적으로 자주 발생하는 no-op"이 아니라면 반드시 로그를 남긴다.
- 로그 레벨: 복구 가능한 이상 상황은 `ZLogWarning`, 기능이 실패한 경우는 `ZLogError`, 정상 흐름 기록은 `ZLogInformation`.

## 7. ZString — 문자열 조합

- 런타임에 반복적으로 실행되는 문자열 연결(로그 메시지의 보간 문자열 자체는 예외)이나 경로/URI 조합은 `+`나 `string.Format` 대신 `ZString.Concat(...)` / `ZString.Format(...)`을 쓴다. GC 할당을 줄이기 위함이다.
  ```csharp
  string uri = ZString.Concat("file://", path);
  ```

## 8. DOTween — 트윈/연출

- 템플릿도 DOTween을 표준으로 쓴다. `HuliacDev.UI.FadeManager`와 `SoundManager`의 페이드가 이미 `DOFade(...).SetUpdate(true)` + UniTask 연동으로 구현되어 있으므로, 새 연출도 같은 패턴으로 작성한다. 화면/볼륨 페이드 자체는 여전히 새로 만들지 않고 해당 매니저를 호출한다(1번 재사용 원칙).
- 새로 만드는 연출에서 `Update()` 안의 수동 `Lerp` 누적이나 `WaitForSeconds` 코루틴으로 위치/색/알파를 직접 보간하지 않는다. 그런 코드는 DOTween 한 줄로 대체된다:
  ```csharp
  await _panel.DOAnchorPosY(0f, 0.3f).SetEase(Ease.OutCubic);
  ```
- **UniTask와 연동해서 await 한다** (3번의 취소 토큰 규칙을 그대로 따른다). `UNITASK_DOTWEEN_SUPPORT` define이 켜져 있어 `DOTweenAsyncExtensions`를 쓸 수 있다. 대기는 `ToUniTask(cancellationToken: ...)`로 통일한다 — 템플릿 표준 패턴이고, `WithCancellation`과 달리 취소 시 동작(`TweenCancelBehaviour`)을 연출별로 지정할 수 있다.
- **`SetUpdate(true)`는 연출 성격에 따라 구분한다.** `Time.timeScale`을 무시하는 옵션이므로, 페이드/일시정지 메뉴/로딩처럼 timeScale이 0이어도 돌아야 하는 UI·시스템 연출에만 붙이고, 일시정지·슬로모션을 따라야 하는 게임플레이 연출에는 붙이지 않는다:
  ```csharp
  // UI/시스템 연출 (timeScale 0에서도 동작해야 함)
  await _canvasGroup.DOFade(1f, 0.3f).SetUpdate(true)
      .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

  // 게임플레이 연출 (일시정지/슬로모션을 따라야 함) — SetUpdate 없이
  await transform.DOMove(target, 1f)
      .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());
  ```
  `yield return tween.WaitForCompletion()` 같은 코루틴 대기는 쓰지 않는다.
- **생성한 트윈은 반드시 수명을 묶는다.** 4번(MessagePipe 구독 해제), 5번(R3 구독 해제)과 같은 이유로, 해제를 빼먹으면 파괴된 오브젝트를 트윈이 계속 건드리다 예외가 난다.
  ```csharp
  transform.DOMove(target, 1f).SetLink(gameObject);
  ```
  `SetLink`로 오브젝트 파괴 시 자동 Kill 되게 하는 게 기본이다. 무한 반복(`SetLoops(-1)`) 트윈처럼 도중에 직접 멈춰야 하는 건 `Tween` 참조를 필드로 들고 있다가 `_tween?.Kill()`로 정리한다.
- 트윈 대상이 없는 순수 수치 보간이나 지연 실행은 빈 GameObject를 만들지 말고 `DOVirtual.Float(...)` / `DOVirtual.DelayedCall(...)`을 쓴다.
- **무료판이라 TextMeshPro 숏컷이 없다.** `DOText`, TMP `DOColor`/`DOFade`는 Pro 전용이라 이 프로젝트에서 컴파일되지 않는다. TMP 텍스트 연출이 필요하면 `DOVirtual.Float`로 값을 보간해 콜백에서 직접 대입한다.

## 9. 리플렉션 최소화

런타임 코드에서 리플렉션(`System.Reflection`, `Type.GetType`, `GetMethod`/`GetField`/`Invoke`, `Activator.CreateInstance`, `dynamic`)을 쓰지 않는다. 이유는 세 가지다.

- IL2CPP 빌드에서 코드 스트리핑에 걸려 에디터에서는 되는데 빌드에서만 깨진다. 재현이 어려운 종류의 버그다.
- 컴파일 타임 검증이 사라진다. 문자열로 참조한 멤버는 이름을 바꿔도 컴파일러가 잡아주지 않는다.
- 매 호출마다 할당과 조회 비용이 든다. ZString(7번)으로 GC를 줄이는 것과 정반대 방향이다.

이 스택에는 리플렉션이 필요할 만한 자리에 이미 대안이 있다.

- 타입으로 구현체를 찾아 생성 → VContainer 주입(2번)
- 다른 시스템의 메서드를 이름으로 호출 → MessagePipe 이벤트(4번) 또는 인터페이스
- 상태 변화 감지 → R3 `ReactiveProperty`(5번)
- JSON 역직렬화 → `HuliacDev.Utils.JsonLoader` 재사용(1번)
- 인스펙터 노출 → `[SerializeField]`

`SendMessage`, `Invoke("MethodName", ...)`, `StartCoroutine("MethodName")`처럼 문자열로 메서드를 호출하는 Unity API도 같은 이유로 쓰지 않는다.

예외는 `Editor/` 폴더의 에디터 전용 도구와 일회성 디버그 코드다. 빌드에 포함되지 않으므로 스트리핑 문제가 없다. 그래도 공개 API가 있으면 그쪽을 먼저 쓴다.

기존 코드에서 리플렉션을 발견하면 조용히 남겨두지 말고, 위 대안 중 무엇으로 바꿀 수 있는지 사용자에게 알린다. 다만 CLAUDE.md의 "수술적 변경" 원칙에 따라 요청받지 않은 리팩터링을 임의로 수행하지는 않는다.

## 10. 메서드 분리 기준

한 메서드가 여러 책임(캐시 확인 → 다운로드 → 적용 같은)을 한 번에 처리하지 않도록, 단계별로 private 메서드를 쪼갠다. SoundManager의 `LoadAndPlayAsync → DownloadAndCacheClipAsync → ExecuteDownloadAsync` 체인이 예시다. 기준은 "이 메서드 이름만 보고 무슨 일을 하는지 한 문장으로 설명할 수 있는가" — 안 되면 쪼갠다.

## 11. 주석 규칙 (이 스택 코드에 한함)

프로젝트 전역 규칙은 "주석은 최소화"지만, 이 프로젝트의 매니저/서비스 클래스는 이미 모든 메서드에 한국어 summary 주석이 달려 있다 (실측 컨벤션). 이 스킬이 적용되는 코드에서는 그 기존 컨벤션을 따른다:

- public/protected/private 구분 없이 **모든 메서드**에 `/// <summary>...</summary>` 작성. `<param>`, `<returns>` 태그는 쓰지 않는다.
- Summary는 이 메서드가 **어떤 역할을 하는지**를 한 문장으로 작성한다 (예: "프레임 단위 보간을 통해 볼륨을 줄이는 페이드아웃 핵심 로직").
- 이모티콘, 특수문자 없이 평서형으로 끝맺는다.

## 12. 상수 중앙 관리 (씬 이름 등)

여러 파일에서 참조하는 문자열 상수 — 특히 **씬 이름**, StreamingAssets 파일 이름처럼 "식별자" 성격의 값 — 은 각 클래스에 `[SerializeField] private string xxxSceneName = "..."` 기본값이나 리터럴로 흩어놓지 않고, `DGAIZone.App.Constants` 같은 **static 클래스 한 곳에 const로 모아** 참조한다.

- 씬을 리네임하면 `Constants.Scenes` 한 줄만 바꾸면 모든 참조가 따라온다.
- 씬 이름을 SerializeField로 두면 코드 기본값과 씬에 직렬화된 값이 이원화되어, 씬 파일에 낡은 값이 남은 채로 조용히 잘못된 씬을 로드하는 사고가 난다. 실제로 이 프로젝트에서 씬 번호를 다시 매긴 뒤 `resultSceneName = "3_Result"`, `outroSceneName = "4_Outro"`, `gameSceneName = "2_Game"`이 갱신되지 않아 존재하지 않는 씬을 로드하려던 버그가 있었다. 그래서 씬 이름은 SerializeField를 제거하고 코드에서 `Constants.Scenes.Xxx`를 직접 참조한다.
- 관심사별 중첩 static 클래스로 묶는다:
  ```csharp
  public static class Constants
  {
      public static class Scenes
      {
          public const string Title = "0_Title";
          public const string Game = "3_Game";
          // ...
      }

      public static class Files { public const string RfidMappings = "RfidMappings.json"; }
  }
  ```
- 반대로 씬별로 조정 가능한 튜닝 값(페이드 시간, 타이핑 속도 등)은 그대로 SerializeField로 둔다. 중앙화 대상은 "정체성(식별자)"이지 "인스턴스별 조정 파라미터"가 아니다.

## 13. 테스트 작성 기준 (UTF)

이 스킬로 매니저/시스템 클래스를 스캐폴딩하거나 수정할 때, 아래 조건에 해당하면 `Tests/Runtime`에 대응하는 `[UnityTest]`를 같이 작성한다. 조건에 해당하지 않으면 테스트를 만들지 않는다 — 모든 클래스에 테스트를 강제하지 않는다.

**테스트가 필요한 경우:**
- `Time.timeScale`, `SetUpdate(true)`처럼 타이밍/일시정지에 영향을 받는 로직 (예: 페이드, 연출)
- 상태 플래그(`_isTransitioning` 등)로 중복 호출을 막는 로직 — 플래그가 해제되지 않으면 이후 호출이 전부 무시되는 소프트락 위험이 있는 코드
- 여러 곳에서 동시에 호출될 수 있는 캐싱/중복 방지 로직 (`SoundManager`의 `_activeDownloads` 패턴 등)
- 과거에 버그가 발생했던 지점을 수정하는 경우 (회귀 방지)

**테스트가 필요 없는 경우:**
- 단순 getter/setter, DI 등록 코드, 데이터 클래스
- Inspector 값 대입만 하는 초기화 코드

**작성 패턴** (`FadeManagerTests.cs` 표준을 따른다):
```csharp
[UnityTest]
public IEnumerator 설명은_한글_평서형으로() => UniTask.ToCoroutine(async () =>
{
    await AwaitWithRealtimeTimeout(_target.SomeAsync());
    Assert.IsFalse(GetIsTransitioning(), "플래그가 해제되지 않아 이후 호출이 무시됨");
});
```
- 비동기 완료 대기는 반드시 `UniTask.WhenAny(task, UniTask.Delay(..., DelayType.UnscaledDeltaTime))`로 실시간 타임아웃을 건다. 가드가 없으면 결함이 있는 구현에서 테스트가 "실패"가 아니라 "무한 대기"로 멈춰 테스트 러너 전체를 막는다.
- `[TearDown]`에서 `Time.timeScale` 등 건드린 전역 상태를 반드시 원복한다.
- private 필드 검증에 새 리플렉션 코드를 추가하지 않는다(9번 규칙과 동일한 이유). 이미 `NonPublic` 리플렉션이 쓰인 기존 헬퍼가 있으면 재사용하되, 새로 만들 경우 가능하면 공개 상태/이벤트로 노출하는 걸 우선 고려한다.

## 14. 구조체 메모리 정렬 및 패딩 (Data Alignment & Padding)

C#의 참조 타입(`class`)은 CLR이 `[StructLayout(LayoutKind.Auto)]`를 통해 런타임에 멤버를 재배치해 패딩을 줄여주지만, 값 타입(`struct`)은 기본값이 `[StructLayout(LayoutKind.Sequential)]`이므로 프로그래머가 작성한 순서 그대로 메모리에 배치된다.

- **원인**: CPU는 메모리 버스 단위(64비트는 8바이트)로 데이터를 읽으며, 각 기본 타입은 자신의 크기 배수 주소에 놓여야 한다(Unaligned Access 방지). 작은 타입 뒤에 큰 타입이 오면 그 사이에 패딩 바이트(공백)가 강제 삽입된다.
- **규칙**: 상태 저장용 또는 대량 인스턴스가 생성되는 `struct`를 정의할 때는 멤버 변수를 **바이트 크기 내림차순(8B → 4B → 2B → 1B)**으로 선언한다:
  ```csharp
  // 비권장 (패딩 9바이트 낭비 -> 총 24바이트)
  public struct BadData
  {
      public byte id;     // 1B + 패딩 3B
      public int hp;      // 4B
      public byte level;  // 1B + 패딩 7B
      public double exp;  // 8B
  }

  // 권장 (패딩 2바이트 최소화 -> 총 16바이트)
  public struct GoodData
  {
      public double exp;  // 8B
      public int hp;      // 4B
      public byte id;     // 1B
      public byte level;  // 1B + 끝 패딩 2B (8의 배수 정렬)
  }
  ```
- 특히 C++ 네이티브 플러그인, Compute Shader(HLSL 버퍼), Job System/DOTS, 네트워크 패킷으로 전달되는 구조체는 필드 순서가 어긋나면 메모리 깨짐이 발생하므로 반드시 이 규칙을 준수한다.

## 15. 비동기 스레드 안전성과 메인 스레드 전환 (UniTask)

Unity 엔진의 네이티브 객체(`Transform`, `GameObject`, `Component`, UI 등)는 락(Lock) 없이 설계되어 멀티스레드 동시 접근 시 데이터 레이스 및 크래시가 발생하므로, 메인 스레드에서만 조작해야 한다.

- 무거운 연산(수학 계산, 파일 I/O, 대용량 JSON 파싱 등)을 `UniTask.RunOnThreadPool`이나 백그라운드 태스크로 처리하는 것은 권장된다.
- **규칙**: 백그라운드 스레드에서 작업이 끝난 후 Unity 엔진 객체를 참조/수정해야 할 때는 반드시 `await UniTask.SwitchToMainThread()`를 통해 메인 스레드로 명시적 복귀한 뒤 조작한다:
  ```csharp
  // 백그라운드 워커 스레드에서 무거운 데이터 처리
  await UniTask.RunOnThreadPool(() =>
  {
      ProcessHeavyData();
  });

  // Unity API 호출 전 메인 스레드로 안전하게 복귀
  await UniTask.SwitchToMainThread(cancellationToken: this.GetCancellationTokenOnDestroy());
  transform.position = newPosition;
  ```
- **멀티스레드 공유 플래그와 원자적 연산**:
  - 백그라운드 스레드와 메인 스레드가 함께 참조하는 실행 플래그는 CPU 레지스터 캐싱 및 컴파일러의 명령어 재배치(Out-of-Order Execution)를 방지하기 위해 `private volatile bool _isRunning;` 형태로 선언한다.
  - 스레드 간 동시 카운트 증감이나 상태 플래그 교체는 락(Lock) 없이 고속으로 안전하게 처리하기 위해 `Interlocked.Increment(ref _counter)` 또는 `Interlocked.Exchange(...)` 등 원자적(Atomic) 연산을 사용한다.

## 16. 캐시 지역성(Cache Locality)과 자료구조 선택

현대 하드웨어는 CPU 연산 속도와 메인 RAM 접근 속도의 격차(메모리 벽)를 극복하기 위해 64바이트 캐시 라인(Cache Line) 단위로 데이터를 CPU 캐시(L1/L2/L3)로 퍼온다.

- **LinkedList 지양**: `LinkedList<T>`는 이론상 중간 삽입/삭제가 $O(1)$이지만, 노드들이 힙(Heap) 곳곳에 분산 할당되어 순회 시 포인터를 타고 갈 때마다 100% 캐시 미스(Cache Miss)가 발생하고 노드당 추가 힙 할당(GC)이 유발된다. 실무에서는 특별한 이유가 없는 한 사용하지 않는다.
- **연속 메모리 우선**: 데이터가 메모리에 일렬로 연속 배치되는 `T[]` (배열) 또는 `List<T>`를 기본으로 사용한다. 인덱스 0번에 접근할 때 인접 요소들이 64바이트 캐시 라인에 함께 적재(공간 지역성, Spatial Locality)되므로 순회 속도가 압도적이다.
- **Swap-Back 삭제 패턴**: 빈번한 중간 요소 삭제가 발생하고 요소의 순서 유지가 중요하지 않은 컬렉션은, $O(N)$ 메모리 이동 복사를 피하기 위해 맨 뒤 요소를 삭제 위치로 덮어쓰고 마지막을 제거하는 패턴을 사용한다:
  ```csharp
  public static void RemoveAtSwapBack<T>(List<T> list, int index)
  {
      int lastIndex = list.Count - 1;
      list[index] = list[lastIndex];
      list.RemoveAt(lastIndex);
  }
  ```
- **공간 분할(Spatial Hashing) 활용 ($O(N^2)$ 전수 검사 금지)**: 다수의 오브젝트(관람객 인터랙션, 센서 마커, 파티클 등) 간의 근접 검사나 충돌 탐색 시 모든 대상 간의 거리를 이중 루프로 전수 검사하지 않는다. 2D/3D 공간을 그리드 셀로 분할하여 인접한 셀의 대상들만 국소적으로 검사하도록 작성한다.

## 17. 핫패스(Hot Path) 성능 및 가상 함수/인라인 최적화

C#의 `virtual` 메서드나 인터페이스 호출은 런타임에 객체의 메서드 테이블(vtable/메타데이터)을 조회하는 간접 참조 비용이 들며, 컴파일러의 가장 강력한 최적화인 **인라인화(Inlining)**를 차단한다.

- 매니저, 서비스, 생명주기 이벤트 등 일반적인 시스템 구조에서는 DI와 유지보수를 위해 VContainer 인터페이스 및 가상 메서드를 표준으로 사용한다 (2번 원칙).
- **규칙**: 매 프레임 수백~수천 번 이상 호출되는 핫패스(Hot Path, 예: `Update()` 내의 물리 연산 루프, 대량 파티클/엔티티 수학 계산)에서는 불필요한 단일 구현 인터페이스나 깊은 가상 메서드 체인을 피하고 구체 클래스/구조체 직접 호출을 유지하여 JIT/IL2CPP 인라인 최적화를 보존한다.

## 18. Zero-GC 및 박싱 방지 (GC Spike 방지 규칙)

매 프레임 호출되는 게임플레이 루프에서 발생하는 미세한 힙 할당은 0세대(Gen 0) 가비지로 누적되어 예고 없는 순간적인 프레임 드랍(GC 스파이크)을 유발한다.

- **LINQ 지양 (런타임 루프)**: `.Where()`, `.Select()`, `.ToList()`, `.OrderBy()` 등 `System.Linq` 메서드는 호출할 때마다 내부적으로 이터레이터 객체와 대리자(Delegate)를 힙에 새로 할당한다. 초기화/설정 로드가 아닌 런타임 반복 호출부(`Update`, 빈번한 이벤트 핸들러)에서는 LINQ 대신 단순 `for` 루프나 캐싱된 리스트를 사용한다.
- **박싱(Boxing) 방지**:
  - 값 타입(`int`, `float`, `struct`, `enum`)을 `object`나 제약 없는 인터페이스 타입으로 전달하면 힙에 포장 객체가 생성된다.
  - `enum`을 `Dictionary<TKey, TValue>`의 키로 사용할 때 기본 해시 연산에서 박싱이 일어날 수 있으므로 주의한다.
- **람다 클로저(Closure) 주의**:
  - 람다식이나 이벤트 리스너 내부에서 바깥 스코프의 로컬 변수를 참조(캡처)하면, 컴파일러가 해당 변수를 담기 위한 임시 클래스 인스턴스를 힙에 매번 할당한다.
  - 반복 호출되는 콜백에는 외부 변수를 캡처하지 않는 정적 람다(`static (x) => ...`)를 쓰거나 상태를 매개변수로 명시적 전달한다.
- **오브젝트 풀링**: 빈번하게 생성/파괴되는 투사체, 대미지 텍스트, 파티클, UI 목록 아이템은 `Instantiate`/`Destroy` 대신 풀링을 적용하여 힙 단편화와 GC 부하를 억제한다.
- **임시 버퍼 풀링 (`ArrayPool<T>` / `NativeArray Temp`)**: 반복적으로 실행되는 비동기 I/O 패킷 파싱이나 대량 수학 연산에서 `new byte[4096]`처럼 임시 배열을 힙에 반복 생성하지 않는다. `System.Buffers.ArrayPool<T>.Shared.Rent(size)`로 대여 후 `Return`하거나, 유니티의 스택 기반 네이티브 얼로케이터인 `NativeArray<T>(count, Allocator.Temp)`를 활용하여 GC 발생을 원천 차단한다.

## 19. UI 렌더링 및 캔버스 최적화 (Overdraw & Rebuild 방지)

UI(UGUI)는 CPU의 메시 재생성(Rebuild)과 GPU의 픽셀 덮어쓰기(Overdraw) 양쪽에서 병목의 주원인이 된다.

- **Raycast Target 비활성화**: 클릭이나 터치 입력을 받지 않는 모든 `Image`, `TextMeshProUGUI`는 반드시 `Raycast Target` 체크를 끈다. 켜져 있으면 사용자가 화면을 터치할 때마다 `GraphicRaycaster`가 해당 컴포넌트들의 경계를 불필요하게 전부 순회 검사하여 CPU 스파이크를 일으킨다.
- **캔버스 분리 (Dynamic vs Static)**:
  - 캔버스 내의 UI 요소가 단 하나라도 이동/크기변경/텍스트수정되면, 해당 캔버스에 속한 모든 UI 요소의 버텍스 메시가 처음부터 전부 재생성(Canvas Rebuild)된다.
  - 매 프레임 또는 자주 갱신되는 UI(체력 바, 타이머, 미니맵 아이콘 등)는 별도의 하위 `Canvas` 컴포넌트를 붙여 정적인 배경/프레임 UI와 메시 재생성 영역을 분리한다.
- **투명 패널 오버드로우 방지**:
  - 단순 레이아웃 정렬이나 클릭 차단용으로 투명한 패널을 만들 때, 알파가 0인 `Image` 컴포넌트를 화면 전체에 깔아두지 않는다. 화면에 보이지 않아도 GPU는 해당 영역의 픽셀 셰이더를 전부 실행(오버드로우)한다.
  - 레이아웃 정렬에는 컴포넌트 없는 빈 `RectTransform`을 사용하고, 광선 차단이 목적이면 `CanvasRenderer`를 사용하지 않는 커스텀 빈 그래픽 컴포넌트를 활용한다.
- **UI 스프라이트 Mipmap 비활성화**:
  - UGUI/HUD에 사용되는 모든 2D 스프라이트 및 UI 텍스처는 인스펙터 Import Settings에서 `Generate Mip Maps`를 반드시 끈다.
  - UI는 카메라와의 거리가 일정하여 밉맵 축소본을 참조할 일이 없으므로, 켜둘 경우 33%의 불필요한 VRAM 낭비 및 특정 해상도에서 UI 텍스트나 아이콘이 뿌옇게 흐려지는(Blur) 현상이 발생한다.

## 20. 부동소수점(float) 연산 및 비교 규칙 (IEEE 754)

컴퓨터는 소수를 2진수 비트로 근사 표현하므로(`0.1f + 0.2f != 0.3f`), 부동소수점 연산에는 항상 미세한 오차가 존재한다.

- **직접 일치 비교(`==`, `!=`) 금지**: `float` 변수를 `0f`나 특정 목표값과 직접 `==`로 비교하면 영원히 참이 되지 않아 타이머 멈춤, 무한 루프, 이동 상태 전이 실패 등의 버그가 발생한다.
- **권장 비교 방식**:
  - 단순 일치 비교: `Mathf.Approximately(a, b)` 사용.
  - 타이머/게이지 카운트다운: 등호 대신 부등호 사용 (`currentTimer <= 0f`).
  - 좌표/거리 도달 검사: `Vector3.Distance(current, target) < 0.01f` 또는 `Mathf.Abs(a - b) < 0.001f`처럼 명시적 오차 허용치(Epsilon)를 둔다.

## 21. 셰이더 및 GPU 연산 최적화 (HLSL / ShaderGraph)

GPU는 32개(또는 64개)의 스레드가 한 묶음으로 동일한 명령어를 실행하는 SIMT(Single Instruction, Multiple Threads / Warp) 구조로 작동한다.

- **분기 다이버전스(Branch Divergence) 방지**:
  - 픽셀 셰이더(Fragment Shader) 내부에서 `if/else` 분기문을 사용하면, 워프 내에서 분기 결과가 갈릴 때 참인 스레드와 거짓인 스레드가 서로의 연산이 끝날 때까지 번갈아 유휴 대기(Idle)하므로 GPU 처리 속도가 급격히 저하된다.
  - 셰이더 연산에서는 `if/else` 분기를 지양하고, `step()`, `lerp()`, `smoothstep()`, `saturate()` 등 GPU 하드웨어에 최적화된 내장 수학 함수를 활용하여 단일 수식으로 분기 없이 계산한다:
    ```hlsl
    // 비권장 (분기 다이버전스 유발)
    float3 color;
    if (val > threshold) color = colorA;
    else color = colorB;

    // 권장 (분기 없는 Branchless 연산)
    float t = step(threshold, val);
    float3 color = lerp(colorB, colorA, t);
    ```

## 22. 게임플레이 아키텍처: 상태 패턴(FSM) 및 명령 패턴(Command)

캐릭터 제어, 몬스터/NPC AI, 또는 전시 체험 시퀀스(대기 → 인식 → 체험 → 결과)를 구현할 때 거대한 `if-else`/`switch` 플래그 스파게티를 지양하고 전용 패턴을 채택한다.

- **상태 패턴 (State Pattern / FSM)**:
  - 복합적인 행동과 상태 전이가 필요한 객체는 `IState` 인터페이스(`Enter()`, `Update()`, `Exit()`)를 따르는 독립된 클래스로 쪼개어 관리한다.
  - `Player.cs` 등의 주체는 직접 행동을 판별하지 않고, 현재 상태 객체의 생명주기 메서드만 위임 호출한다.
  - **Zero-GC 준수**: 상태를 바꿀 때마다 `new JumpState()`처럼 힙 할당을 발생시키지 않는다. 객체 초기화(`Awake`/`Start`) 시 사용될 상태 인스턴스들을 미리 생성해두고, 전환 시에는 캐싱된 인스턴스의 참조만 교체한다:
    ```csharp
    public interface IState
    {
        void Enter();
        void Update();
        void Exit();
    }

    public class StateMachine
    {
        public IState CurrentState { get; private set; }

        public void ChangeState(IState newState)
        {
            CurrentState?.Exit();
            CurrentState = newState;
            CurrentState?.Enter();
        }

        public void Update() => CurrentState?.Update();
    }
    ```

- **명령 패턴 (Command Pattern)**:
  - 사용자 입력(키보드, 마우스, 터치, 센서)이나 시스템 요청을 처리할 때 실행할 행동을 `ICommand`(`Execute()`, 필요시 `Undo()`) 객체로 캡슐화한다.
  - **적용 대상**:
    - **입력 버퍼링 (선입력)**: 모션/딜레이 중 입력된 커맨드를 `Queue<ICommand>`에 보관했다가 행동 가능 시점에 즉시 실행하여 씹힘 없는 조작감 구현.
    - **실행 취소 (Undo / Redo)**: 퍼즐, 에디터 도구, 턴제 시스템에서 행동 히스토리를 `Stack<ICommand>`에 저장하여 되돌리기 지원.
    - **네트워크 동기화 & 리플레이**: 전체 좌표 전송 대신 발생한 커맨드 목록만 직렬화하여 전송함으로써 대역폭 최소화 및 리플레이 재현.



