# 0_Startup — Unity 프로젝트 템플릿

전시·키오스크형 Unity 앱을 빠르게 시작하기 위한 템플릿입니다.
자주 쓰는 패키지와 설정, 공통 매니저 연결이 미리 들어 있습니다.

## 사용 방법

**준비물:** Unity 2022.3.62f3, Git
(일부 패키지를 Git 주소로 받으므로 Git이 설치되어 있어야 합니다.)

템플릿은 **이 저장소만 추적하는 폴더를 하나 받아 두고, 새 프로젝트는 그 폴더에서 필요한 파일만 복사해 시작**하는 방식을 권장합니다.
새 프로젝트는 템플릿의 git 기록 없이 깨끗하게 시작하고, 템플릿 폴더는 `git pull` 로 언제든 최신으로 맞출 수 있습니다.

### 1. 템플릿 폴더 준비 (처음 한 번)

```bash
git clone https://github.com/wonjeong97/0_Startup.git
```

이 폴더에서는 작업하지 않습니다. 새 프로젝트를 시작하기 전에 `git pull` 로 최신 상태로 맞춥니다.

### 2. 새 프로젝트 시작

1. 템플릿 폴더에서 아래 7개만 새 프로젝트 폴더로 복사합니다. `Library`, `.git` 등 나머지는 복사하지 않습니다.

   | 복사할 것 | 내용 |
   |-----------|------|
   | `Assets/` | 스크립트, 프리팹, 씬, 런타임 설정 파일 |
   | `Packages/` | 패키지 목록과 고정 버전 |
   | `ProjectSettings/` | 프로젝트 설정 |
   | `.gitignore` | git에서 뺄 파일 |
   | `.gitattributes` | 큰 이미지·영상은 Git LFS로, DLL·서드파티 파일은 일반 파일로 저장하는 규칙 |
   | `CLAUDE.md` | 프로젝트 설명 틀(Claude Code용) |
   | `README.project.md` | 프로젝트 README 틀(프로젝트 이름, 한 줄 개요, Unity 버전). 새 프로젝트에서는 이름을 `README.md` 로 바꿉니다 |

2. Unity Hub에서 **Add > Add project from disk** 로 새 폴더를 엽니다. 처음 한 번은 패키지를 받느라 몇 분 걸립니다.
3. **Edit > Project Settings > Player** 에서 **Product Name** 을 새 프로젝트 이름으로 바꿉니다.
   앱 로그 폴더와 PlayerPrefs 위치가 Company Name·Product Name으로 정해지므로, 그대로 두면 같은 PC에 설치한 다른 앱과 섞입니다.
4. `README.md`(복사한 `README.project.md`)에 프로젝트 이름과 한 줄 개요를 적습니다. 나중에 Unity 버전을 올리면 README의 Unity 버전도 함께 고칩니다.

> 이미 시작한 프로젝트에는 템플릿 변경이 자동으로 들어가지 않습니다.
> 공통 코드(HuliacDev Template)와 스택 패키지는 `Tools > Update Stack Packages` 로 올리고, 그 밖의 변경은 필요한 파일만 골라 옮깁니다.

GitHub의 **Code > Download ZIP** 으로 받은 폴더를 템플릿 폴더로 써도 되지만, 그때는 `git pull` 대신 다시 받아야 최신이 됩니다.

## 들어 있는 것

| 구분 | 패키지 |
|------|--------|
| 구조 | [VContainer](https://github.com/hadashiA/VContainer)(DI), [UniTask](https://github.com/Cysharp/UniTask)(async), [MessagePipe](https://github.com/Cysharp/MessagePipe)(Pub/Sub), [R3](https://github.com/Cysharp/R3)(리액티브) |
| 유틸 | [ZLogger](https://github.com/Cysharp/ZLogger)(로깅), [ZString](https://github.com/Cysharp/ZString)(문자열), [DOTween](https://dotween.demigiant.com/)(트윈) |
| 공통 코드 | [HuliacDev Template](https://github.com/wonjeong97/Template) — GameManagerBase, 무입력 복귀, 자동 종료, UI·Fade·Sound·Video 매니저, API·Arduino 매니저, 로그 보관 |
| Unity | URP 14, Input System, Addressables, TextMeshPro, Timeline, Visual Scripting, Test Framework, Memory Profiler, Profile Analyzer |
| 개발 도구 | [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity), [MCP for Unity](https://github.com/CoplayDev/unity-mcp)(Claude Code 연동) |

## 기본 구성

- **DI 루트:** `Assets/_Project/Prefabs/GameLifetimeScope.prefab` 이 VContainerSettings의 Root Lifetime Scope라서 씬에 두지 않아도 자동으로 생깁니다.
  GameManager, APIManager, InactivityTimer, ShutdownScheduler, Reporter(로그 뷰어)가 들어 있습니다.
- **런타임 설정:** `Assets/StreamingAssets/Settings.json`(무입력 복귀, 프레임레이트, 창 포커스 복구·항상 위, 자동 재시작, API 주소, 숨은 종료 버튼)과
  `ShutdownSettings.json`(요일별 자동 종료)은 빌드한 뒤에도 고칠 수 있습니다.
- **자동 재시작:** Windows 빌드가 크래시하거나 강제로 꺼지거나(`taskkill /F` 등) 60초 넘게 '응답 없음'이면 5초 뒤 다시 켭니다.
  숨은 종료 버튼, 예약 종료, Alt+F4·창 닫기로 끈 경우는 다시 켜지 않습니다(응답하는 앱을 작업 관리자 '작업 끝내기'로 끈 경우도 창 닫기와 같아 다시 켜지 않음). 현장 PC에 따로 설치할 것은 없고, `Settings.json` 의 `autoRestart` 로 끄거나 시간을 바꿉니다.
  켜자마자 다시 꺼지는 일이 이어지면 재시작 간격을 두 배씩 늘립니다(최대 10분). 설정을 바꾸면 앱을 정상 종료(Alt+F4 등)한 뒤 다시 켜야 반영됩니다.
  재시작 기록은 앱 로그 폴더(`%USERPROFILE%\AppData\LocalLow\<Company Name>\<Product Name>\Logs`)의 `AutoRestart.log` 에 남습니다.
- **빌드:** `Build > Windows 빌드 후 보관` 으로 Windows 빌드를 `Build/` 에 만들고, 버전 관리용 zip을 빌드 보관 폴더의 `<프로젝트 폴더 이름>/<YYMMDD>-<N>/` 에 보관하고, 현장 PC로 옮길 빌드를 복사 폴더의 `<프로젝트 폴더 이름>/` 에 복사합니다(보관은 빌드마다 새 폴더로 쌓이고, 복사 폴더는 비운 뒤 최신 빌드만 둠).
  두 폴더는 팀원·PC마다 다르므로 저장소가 아니라 각자 `Edit > Preferences > HuliacDev Build`(또는 `Build > 설정...`)에서 정합니다. 처음 빌드할 때 정해져 있지 않으면 설정을 열지 묻습니다.
- **자동 종료 설정 프로그램:** `ShutdownScheduleEditor.exe` 는 용량이 커서(약 150MB) 저장소에 없습니다.
  필요하면 `Tools > Build Shutdown Scheduler Exe` 로 만듭니다(.NET 8 SDK 필요).
- **렌더링:** UI 위주 성능 설정(URP-Performant, 카메라 후처리 꺼짐)이 기본입니다. 3D를 쓰면 High Fidelity와 후처리를 켭니다.
- **Roslyn:** `Assets/Plugins/Roslyn` 은 MCP for Unity 전용이라 Editor 전용으로 설정되어 있습니다.
  폴더를 지우고 다시 설치하면 Inspector에서 Editor만 다시 체크합니다.

## 패키지 업데이트

`Tools > Update Stack Packages` 를 실행하면 VContainer, UniTask, MessagePipe, R3, ZLogger, ZString, HuliacDev Template, NuGetForUnity, MCP for Unity를 최신으로 올리고, 이어서 NuGetForUnity의 Updates 탭에 뜨는 NuGet 패키지(ZLogger·R3 구현 DLL 등)도 올립니다.
URP·Input System·Addressables처럼 버전을 고정한 패키지와 `Assets/Plugins` 에 직접 넣은 DOTween은 건드리지 않습니다. NuGet 창의 Update All만 쓰면 일부 의존성이 오히려 내려갈 수 있으므로 이 메뉴를 씁니다.

## 폴더 구조

프로젝트에서 만드는 에셋은 `Assets/_Project/` 아래에 종류별로 둡니다. 외부에서 받은 것과 도구가 위치를 정해 둔 폴더는 `Assets/` 바로 아래에 그대로 둡니다.

```
Assets/
├── _Project/
│   ├── Scenes/           # 씬 (0_Idle.unity가 첫 씬)
│   ├── Scripts/          # 스크립트, 기능별 하위 폴더 (App, Core, Network …)
│   ├── Prefabs/          # 프리팹 (GameLifetimeScope.prefab, UI 프리팹)
│   ├── Art/              # 이미지, 3D 모델, 머티리얼 (UI 이미지는 Art/UI)
│   ├── Audio/            # 사운드
│   ├── Video/            # 영상 (교체할 영상은 StreamingAssets)
│   ├── Input/            # 프로젝트 입력 액션과 생성된 C# 클래스
│   └── Settings/         # URP 에셋, URP Global Settings, VContainerSettings
├── Plugins/              # 외부 플러그인 (DOTween, Roslyn — Editor 전용)
├── StreamingAssets/      # 설정 파일, 교체 가능한 영상·데이터 (Settings.json, ShutdownSettings.json)
├── Packages/             # NuGet 패키지 (NuGetForUnity가 이 위치를 씀)
├── Resources/            # DOTweenSettings (DOTween이 이 위치를 씀)
├── TextMesh Pro/         # TMP 기본 리소스 (TMP가 이 위치를 씀)
├── csc.rsp               # 컴파일러 옵션 (Assets 바로 아래에 있어야 함)
└── NuGet.config, packages.config  # NuGetForUnity 설정 (Assets 바로 아래에 있어야 함)
Packages/
├── manifest.json       # 패키지 목록
└── packages-lock.json  # 패키지 버전 고정
```

- 빈 폴더(`Art`, `Audio`, `Video`)에는 `.gitkeep` 이 들어 있습니다. git은 빈 폴더를 저장하지 않아, 없으면 clone한 쪽에서 Unity가 폴더의 `.meta` 를 지웁니다. Unity는 `.` 으로 시작하는 파일을 가져오지 않으므로 에셋으로 보이지 않습니다. 폴더에 파일을 넣은 뒤에는 지워도 됩니다.
- 에셋은 Unity Editor 안에서 옮깁니다. 탐색기에서 옮기면 `.meta` 가 따라가지 않아 씬·프리팹의 참조가 끊길 수 있습니다.
