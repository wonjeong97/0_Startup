# HANDOFF

Claude가 Antigravity(agy)와 리뷰 서브에이전트에 맡긴 검증 기록. 새 항목을 맨 위에 추가한다. 항목 형식은 공통 스킬 `handoff` 를 따른다.

---

### [2026-11-03 12:35] Claude → Antigravity + 서브에이전트 · 회사 기술 문서 10장 기준 맞추기 (chore/bible-folder-structure)
- 요청: 폴더 구조를 Assets/_Project 기준으로 옮기고, 새 프로젝트용 README 틀을 두고, 자동 재시작·Build 메뉴가 들어간 Template으로 올리기.
- 변경: Scenes·Scripts·Prefabs·Input·Settings와 루트 설정 에셋 2개를 Assets/_Project 아래로(Unity 안에서 이동), 빈 UI 폴더 삭제, Art·Audio·Video에 .gitkeep. `README.project.md` 추가와 README·CLAUDE.md 틀 갱신. Template 26.11.3-2와 NuGet 의존성 업데이트, Settings.json `autoRestart` 켬, Version 26.11.3.
- Claude 확인: 이동 뒤 GUID 유지(이름 바꾸기 R100), VContainer Root Lifetime Scope·Preloaded Assets·Graphics·Quality·URP Global Settings·전역 입력 액션·빌드 씬이 새 경로를 가리킴, 콘솔 오류 0, Windows 빌드 성공(이동 직후, 패키지 업데이트 뒤 각 1번, 빌드 부산물은 되돌림). UPM ZLogger.Unity 2.6.0 + NuGet ZLogger 2.5.10 조합은 로거 생성은 됐지만 README의 지원 조합에 맞춰 둘 다 2.6.0으로 올림.
- 교차 검증 (agy gemini-4-argon-high + 서브에이전트, 1묶음):
  - 폴더 이동(GUID·설정 참조·고아 .meta·그대로 둘 폴더·생성 코드 변경 범위) → 둘 다 통과
- 서브에이전트 (1묶음):
  - 문서·설정 → 지적(낮음): 재시작 간격이 최대 10분까지 늘어난다는 설명 없음, 강제 종료 뒤 다시 켜지는 점을 Breaking에 적지 않음, UPM·NuGet ZLogger 짝이 어긋남 → 모두 고침
- agy (gemini-4-argon-high, 1묶음): 문서·설정 최종본 → 지적(낮음): README의 Build 메뉴 설명에 복사 폴더를 비우고 최신만 둔다는 말이 없음 → 고침. 설정 JSON·패키지 버전 짝·개인 경로 → 통과
- 남은 것: 사용자가 NuGet 창의 Update All로 올린 결과는 `etc: 사용자 변경` 커밋으로 따로 남김(의존성 3개가 내려간 상태였고, 이어서 메뉴로 다시 올림).

### [2026-10-16 10:59] Claude → Antigravity · HuliacDev Template 26.10.16-1 업데이트 (main)
- 요청: Template을 최신으로 올리고, 바뀐 내용과 이 프로젝트에 걸리는 것이 있는지 확인.
- 변경: packages-lock.json 고정 커밋 1c5e127(26.10.12-1) → a036657(26.10.16-1, 항상 위 옵션 `alwaysOnTop`). Settings.json에 `"alwaysOnTop": false` 추가, bundleVersion 26.10.16, CHANGELOG에 10-12·10-16 운영자 영향 항목 추가.
- Claude 확인: 배치 모드로 a03665700c 패키지 받기·스크립트 컴파일, error CS 0, return code 0. 프로젝트 코드는 `ConfigureWindowFocus`·`ConfigureProjectMessageBrokers`·`GetTextWithRetryAsync` 를 override·호출하지 않음.
- agy (gemini-3.8-flash-high, 2묶음):
  - 프로젝트 코드와 바뀐 Template API 충돌, Settings.json 키 이름·기본 동작 → 통과
  - CHANGELOG 사실 일치, 배치 모드 로그, lock 해시·bundleVersion·README → 통과(로그 근거 줄 440·866 직접 확인)
