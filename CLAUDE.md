# CLAUDE.md

Behavioral guidelines to reduce common LLM coding mistakes. Merge with project-specific instructions as needed.

**Tradeoff:** These guidelines bias toward caution over speed. For trivial tasks, use judgment.

## 1. Think Before Coding

**Don't assume. Don't hide confusion. Surface tradeoffs.**

Before implementing:
- State your assumptions explicitly. If uncertain, ask.
- If multiple interpretations exist, present them - don't pick silently.
- If a simpler approach exists, say so. Push back when warranted.
- If something is unclear, stop. Name what's confusing. Ask.

## 2. Simplicity First

**Minimum code that solves the problem. Nothing speculative.**

- No features beyond what was asked.
- No abstractions for single-use code.
- No "flexibility" or "configurability" that wasn't requested.
- No error handling for impossible scenarios.
- If you write 200 lines and it could be 50, rewrite it.

Ask yourself: "Would a senior engineer say this is overcomplicated?" If yes, simplify.

## 3. Surgical Changes

**Touch only what you must. Clean up only your own mess.**

When editing existing code:
- Don't "improve" adjacent code, comments, or formatting.
- Don't refactor things that aren't broken.
- Match existing style, even if you'd do it differently.
- If you notice unrelated dead code, mention it - don't delete it.

When your changes create orphans:
- Remove imports/variables/functions that YOUR changes made unused.
- Don't remove pre-existing dead code unless asked.

The test: Every changed line should trace directly to the user's request.

## 4. Goal-Driven Execution

**Define success criteria. Loop until verified.**

Transform tasks into verifiable goals:
- "Add validation" → "Write tests for invalid inputs, then make them pass"
- "Fix the bug" → "Write a test that reproduces it, then make it pass"
- "Refactor X" → "Ensure tests pass before and after"

For multi-step tasks, state a brief plan:
```
1. [Step] → verify: [check]
2. [Step] → verify: [check]
3. [Step] → verify: [check]
```

Strong success criteria let you loop independently. Weak criteria ("make it work") require constant clarification.

---

**These guidelines are working if:** fewer unnecessary changes in diffs, fewer rewrites due to overcomplication, and clarifying questions come before implementation rather than after mistakes.

---

## Project Convention: CHANGELOG.md

관람객·운영자 경험에 영향을 주는 변경이 머지될 때마다 저장소 루트의 `CHANGELOG.md`를 갱신할 것.

- [Keep a Changelog](https://keepachangelog.com/ko/1.1.0/) 형식(`Added`/`Changed`/`Fixed`/`Removed`)을 따름.
- 내부 리팩터링, 오타 수정, 에디터 전용 작업 상태 등 사용자에게 영향 없는 변경은 기록하지 않음.
- 호환성이 깨지는 변경이 있으면 해당 섹션 최상단에 별도로 강조함.
- 각 항목은 한 문장으로 간결하게 작성함.
- 아직 main에 머지되지 않은 변경은 `[Unreleased]` 섹션에 기록하고, 머지되면 날짜 섹션(`## [YYYY-MM-DD]`)으로 옮김.
- 상세 절차는 `.claude/skills/changelog/SKILL.md` 참고.
