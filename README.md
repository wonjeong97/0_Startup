# 0_Startup — Unity 프로젝트 템플릿

전시·키오스크형 Unity 앱을 빠르게 시작하기 위한 템플릿입니다.  
자주 사용하는 패키지와 설정, 공통 매니저 연결이 미리 구성되어 있습니다.

## Unity 버전

- **Unity 2022.3.62f3 LTS**
- **Render Pipeline:** Universal RP (URP) 14.0.12

## 패키지 스택

### 핵심 아키텍처

| 패키지 | 용도 |
|--------|------|
| [VContainer](https://github.com/hadashiA/VContainer) | DI (의존성 주입) 컨테이너 |
| [UniTask](https://github.com/Cysharp/UniTask) | 비동기 처리 (`async/await`) |
| [MessagePipe](https://github.com/Cysharp/MessagePipe) | 인앱 메시지 버스 (Pub/Sub) |
| [R3](https://github.com/Cysharp/R3) | 리액티브 프로그래밍 |
| [ZLogger.Unity](https://github.com/Cysharp/ZLogger) | 고성능 구조화 로깅 |
| [ZString](https://github.com/Cysharp/ZString) | 제로 할당 문자열 빌더 |
| [DOTween](https://dotween.demigiant.com/) | 트윈 애니메이션 (`Assets/Plugins/Demigiant`) |

ZLogger와 R3가 쓰는 NuGet 의존성은 NuGetForUnity로 `Assets/Packages` 에 설치되어 있습니다.

### Unity 패키지

| 패키지 | 용도 |
|--------|------|
| Input System | 새 입력 시스템 |
| Addressables | 에셋 번들 관리 |
| TextMeshPro | 텍스트 렌더링 |
| Timeline | 컷씬 / 시퀀스 |
| Visual Scripting | 비주얼 스크립팅 |
| Test Framework | 유닛 테스트 |
| Memory Profiler | 메모리 스냅샷 분석 |
| Profile Analyzer | 프로파일러 프레임 통계 비교 |

### 개발 도구

| 패키지 | 용도 |
|--------|------|
| [HuliacDev Template](https://github.com/wonjeong97/Template) (`com.huliacdev.template`) | 공통 매니저/시스템 베이스 코드 |
| [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) | Unity에서 NuGet 패키지 사용 |
| [MCP for Unity](https://github.com/CoplayDev/unity-mcp) | Claude Code와 Unity Editor 연동 |

HuliacDev Template은 GameManagerBase, InactivityTimer(무입력 복귀), ShutdownScheduler(자동 종료),
UI·Fade·Sound·Video 매니저, ApiManagerBase, ArduinoManager, 로그 보관 등을 제공합니다.

## 시작하기

1. 이 저장소를 클론합니다.
2. Unity Hub에서 **Unity 2022.3.62f3**로 프로젝트를 엽니다.
3. 패키지가 자동으로 다운로드됩니다 (최초 1회, 수 분 소요).

## 기본 구성

- **DI 루트:** `Assets/Prefabs/GameLifetimeScope.prefab` 이 `VContainerSettings` 의 Root Lifetime Scope로 지정되어 있어
  씬에 배치하지 않아도 자동 생성됩니다. 프리팹에는 GameManager, APIManager, InactivityTimer, ShutdownScheduler,
  Reporter(런타임 로그 뷰어)가 들어 있습니다.
- **런타임 설정:** `Assets/StreamingAssets/Settings.json` (무입력 타임아웃, 프레임레이트, API 주소, 숨은 종료 버튼)과
  `ShutdownSettings.json` (요일별 자동 종료)을 빌드 후에도 수정할 수 있습니다.
- **렌더링:** UI 위주 성능 설정이 기본입니다. Quality·Graphics 기본값은 `URP-Performant` 이고 카메라 후처리는 꺼져 있습니다.
  3D를 쓰게 되면 High Fidelity와 카메라 후처리를 켭니다.
- **Roslyn (MCP 전용):** `Assets/Plugins/Roslyn` 의 DLL은 MCP for Unity의 스크립트 검증·코드 실행에만 쓰이므로
  Editor 전용으로 설정되어 있습니다. 다시 설치해도 `.meta` 가 남아 있으면 설정이 유지되지만, 폴더를 지운 뒤
  설치하면 모든 플랫폼으로 들어오므로 Inspector에서 Editor만 체크해 플레이어 빌드에 들어가지 않게 합니다.

## 패키지 업데이트

Unity 에디터에서 `Tools > Update Stack Packages` 메뉴를 실행하면  
위 스택 패키지(VContainer, UniTask, MessagePipe, R3, ZLogger, ZString, NuGetForUnity, MCP for Unity, HuliacDev Template)만 일괄 업데이트합니다.
Addressables, Input System, URP처럼 버전을 고정해 둔 패키지는 건드리지 않습니다.

- **레지스트리 패키지:** 현재 에디터와 호환되는 최신 버전으로 업데이트
- **Git 패키지:** 최신 커밋으로 갱신

## 프로젝트 구조

```
Assets/
├── Input/                  # 프로젝트 입력 액션(ProjectInputActions)
├── Packages/               # NuGetForUnity로 설치한 NuGet 패키지
├── Plugins/
│   ├── Demigiant/DOTween/
│   └── Roslyn/             # MCP for Unity용 (Editor 전용)
├── Prefabs/
│   └── GameLifetimeScope.prefab
├── Scenes/
│   └── 0_Idle.unity        # 빌드 첫 씬
├── Scripts/
│   ├── App/GameLifetimeScope.cs   # 루트 DI 스코프 (템플릿 RootLifetimeScope 상속)
│   ├── Core/GameManager.cs        # 무입력 타임아웃 시 첫 화면 복귀
│   └── Network/APIManager.cs      # ApiManagerBase 상속
├── Settings/               # URP 에셋 (Performant / Balanced / HighFidelity)
├── StreamingAssets/        # Settings.json, ShutdownSettings.json
└── UI/
Packages/
├── manifest.json           # 패키지 의존성 정의
└── packages-lock.json      # 패키지 버전 고정
```
