# Agentic Implementation Workflow

This repository now includes a staged workflow for implementation tasks using custom Copilot agents.

## Model Allocation
- High-effort reasoning stages use Sonnet 5 first, then GPT-5.3 Codex as fallback:
  - Orchestration
  - Planning
  - Review
  - QA
- Code-heavy stages use GPT-5.3 Codex first, then Sonnet 5 as fallback:
  - TDD Tests
  - Implementation
  - Unit Test Expansion

## Stage Order
1. Planning
2. TDD Tests
3. Implementation
4. Review
5. Unit Test Expansion
6. QA

## Approval Checkpoints
- Required after Planning.
- Required after Review.

## Required Quality Gates
- `dotnet build`
- `dotnet test src/D2NG.Core.Tests/D2NG.Core.Tests.csproj`
- `dotnet build -c Release`

## Entry Points
- Prompt: `.github/prompts/start-implementation-workflow.prompt.md`
- Orchestrator agent: `.github/agents/implementation-orchestrator.agent.md`

## Stage Agent Files
- `.github/agents/planning-stage.agent.md`
- `.github/agents/tdd-stage.agent.md`
- `.github/agents/implementation-stage.agent.md`
- `.github/agents/review-stage.agent.md`
- `.github/agents/unit-test-expansion-stage.agent.md`
- `.github/agents/qa-stage.agent.md`

## Safety and Gating Hook
- Hook config: `.github/hooks/implementation-workflow.json`
- Hook script: `.github/hooks/scripts/pretool-policy.ps1`

Current hook behavior:
- Denies obviously destructive shell command patterns.
- Requires confirmation for `dotnet publish` as a QA-stage action.

## Repository Domain Guardrails
For code in sensitive areas, existing instruction files remain authoritative:
- Pickit logic: `.github/instructions/pickit.instructions.md`
- Muling logic: `.github/instructions/muling.instructions.md`
- Bot architecture: `.github/instructions/bot-types.instructions.md`

If gameplay behavior intent is ambiguous (thresholds, precedence, lifecycle sequencing), stop and ask before changing logic.
