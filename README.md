# 0_Startup — Unity 프로젝트 템플릿

새 Unity 프로젝트를 빠르게 시작하기 위한 템플릿입니다.  
자주 사용하는 패키지와 설정이 미리 구성되어 있습니다.

## Unity 버전

- **Unity 2022.x LTS**
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

### Unity 패키지

| 패키지 | 용도 |
|--------|------|
| Input System | 새 입력 시스템 |
| Addressables | 에셋 번들 관리 |
| TextMeshPro | 텍스트 렌더링 |
| Timeline | 컷씬 / 시퀀스 |
| Visual Scripting | 비주얼 스크립팅 |
| Test Framework | 유닛 테스트 |

### 개발 도구

| 패키지 | 용도 |
|--------|------|
| [Wonjeong Template](https://github.com/wonjeong97/Template) | 공통 매니저/시스템 베이스 코드 |
| [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity) | Unity에서 NuGet 패키지 사용 |
| [MCP for Unity](https://github.com/CoplayDev/unity-mcp) | Claude Code와 Unity Editor 연동 |

## 시작하기

1. 이 저장소를 클론합니다.
2. Unity Hub에서 **Unity 2022.x LTS**로 프로젝트를 엽니다.
3. 패키지가 자동으로 다운로드됩니다 (최초 1회, 수 분 소요).

## 패키지 업데이트

Unity 에디터에서 `Tools > Update All Packages` 메뉴를 실행하면  
설치된 모든 패키지를 일괄 업데이트합니다.

- **레지스트리 패키지:** 최신 버전으로 자동 업데이트
- **Git 패키지:** 최신 커밋으로 자동 갱신

## 프로젝트 구조

```
Assets/
├── Editor/
│   └── PackageUpdater.cs   # 패키지 일괄 업데이트 에디터 도구
└── Settings/
    ├── URP-Balanced.asset
    └── URP-Performant.asset
Packages/
├── manifest.json           # 패키지 의존성 정의
└── packages-lock.json      # 패키지 버전 고정
```
