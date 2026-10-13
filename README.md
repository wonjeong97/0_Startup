# 0_Startup — Unity 프로젝트 템플릿

전시·키오스크형 Unity 앱을 빠르게 시작하기 위한 템플릿입니다.
자주 쓰는 패키지와 설정, 공통 매니저 연결이 미리 들어 있습니다.

## 사용 방법

**준비물:** Unity 2022.3.62f3, Git
(일부 패키지를 Git 주소로 받으므로, 어떤 방법으로 받든 Git이 설치되어 있어야 합니다.)

### 1. Clone해서 새 프로젝트 만들기 (추천)

비공개 저장소로 만들 수 있고, 템플릿 하나로 프로젝트를 여러 개 만들 수 있습니다.

1. GitHub에서 빈 저장소를 만듭니다(README 등 초기 파일 없이).
2. 템플릿을 받아 그 저장소에 올립니다. `<내 계정>/MyProject` 는 1에서 만든 저장소로 바꿉니다.

   ```bash
   git clone https://github.com/wonjeong97/0_Startup.git MyProject
   cd MyProject
   git remote rename origin upstream
   git remote add origin https://github.com/<내 계정>/MyProject.git
   git config lfs.allowincompletepush true   # 예전 커밋의 LFS 파일은 올리지 않음(현재 파일에는 영향 없음)
   git push -u origin main
   ```

3. Unity Hub에서 폴더를 엽니다. 처음 한 번은 패키지를 받느라 몇 분 걸립니다.

템플릿이 업데이트되면 아래 명령으로 받습니다.

```bash
git pull upstream main
```

### 2. Fork해서 쓰기

GitHub에서 Fork한 뒤 내 fork를 clone합니다. 템플릿 업데이트는 내 fork 페이지의 **Sync fork** 버튼으로 받고 `git pull` 합니다.
공개 저장소의 fork는 비공개로 바꿀 수 없고 한 계정에 하나만 만들 수 있으므로, 공개해도 되는 프로젝트 하나에만 알맞습니다.

### 3. ZIP으로 받기

GitHub의 **Code > Download ZIP** 은 둘러보기용입니다. git 기록이 없어서 템플릿 업데이트를 받을 수 없습니다.

> 업데이트를 받을 때 내가 고친 씬·프리팹·`ProjectSettings`·`Packages/manifest.json` 과 겹치면 충돌이 날 수 있습니다.
> 프로젝트를 시작한 뒤에는 필요할 때만 받습니다.

## 들어 있는 것

| 구분 | 패키지 |
|------|--------|
| 구조 | [VContainer](https://github.com/hadashiA/VContainer)(DI), [UniTask](https://github.com/Cysharp/UniTask)(async), [MessagePipe](https://github.com/Cysharp/MessagePipe)(Pub/Sub), [R3](https://github.com/Cysharp/R3)(리액티브) |
| 유틸 | [ZLogger](https://github.com/Cysharp/ZLogger)(로깅), [ZString](https://github.com/Cysharp/ZString)(문자열), [DOTween](https://dotween.demigiant.com/)(트윈) |
| 공통 코드 | [HuliacDev Template](https://github.com/wonjeong97/Template) — GameManagerBase, 무입력 복귀, 자동 종료, UI·Fade·Sound·Video 매니저, API·Arduino 매니저, 로그 보관 |
| Unity | URP 14, Input System, Addressables, TextMeshPro, Timeline, Visual Scripting, Test Framework, Memory Profiler, Profile Analyzer |
| 개발 도구 | [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity), [MCP for Unity](https://github.com/CoplayDev/unity-mcp)(Claude Code 연동) |

## 기본 구성

- **DI 루트:** `Assets/Prefabs/GameLifetimeScope.prefab` 이 VContainerSettings의 Root Lifetime Scope라서 씬에 두지 않아도 자동으로 생깁니다.
  GameManager, APIManager, InactivityTimer, ShutdownScheduler, Reporter(로그 뷰어)가 들어 있습니다.
- **런타임 설정:** `Assets/StreamingAssets/Settings.json`(무입력 복귀, 프레임레이트, 창 포커스 복구, API 주소, 숨은 종료 버튼)과
  `ShutdownSettings.json`(요일별 자동 종료)은 빌드한 뒤에도 고칠 수 있습니다.
- **자동 종료 설정 프로그램:** `ShutdownScheduleEditor.exe` 는 용량이 커서(약 150MB) 저장소에 없습니다.
  필요하면 `Tools > Build Shutdown Scheduler Exe` 로 만듭니다(.NET 8 SDK 필요).
- **렌더링:** UI 위주 성능 설정(URP-Performant, 카메라 후처리 꺼짐)이 기본입니다. 3D를 쓰면 High Fidelity와 후처리를 켭니다.
- **Roslyn:** `Assets/Plugins/Roslyn` 은 MCP for Unity 전용이라 Editor 전용으로 설정되어 있습니다.
  폴더를 지우고 다시 설치하면 Inspector에서 Editor만 다시 체크합니다.

## 패키지 업데이트

`Tools > Update Stack Packages` 를 실행하면 VContainer, UniTask, MessagePipe, R3, ZLogger, ZString, HuliacDev Template, NuGetForUnity, MCP for Unity만 최신으로 올립니다.
URP·Input System·Addressables처럼 버전을 고정한 패키지와 `Assets/Plugins` 에 직접 넣은 DOTween은 건드리지 않습니다.

## 폴더 구조

```
Assets/
├── Input/            # 입력 액션
├── Packages/         # NuGet 패키지 (NuGetForUnity)
├── Plugins/          # DOTween, Roslyn(Editor 전용)
├── Prefabs/          # GameLifetimeScope.prefab
├── Scenes/           # 0_Idle.unity (첫 씬)
├── Scripts/          # GameLifetimeScope, GameManager, APIManager
├── Settings/         # URP 에셋
├── StreamingAssets/  # Settings.json, ShutdownSettings.json
└── UI/
Packages/
├── manifest.json       # 패키지 목록
└── packages-lock.json  # 패키지 버전 고정
```
