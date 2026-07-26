---
description: "Use when implementing features or fixes with an orchestrated workflow across planning, TDD tests, implementation, review, additional unit testing, and QA."
---
# Implementation Workflow Instructions

Use this stage order for implementation work:
1. Planning
2. TDD Tests
3. Implementation
4. Review
5. Additional Unit Testing
6. QA

## Approval Checkpoints
- Require explicit user approval after Planning.
- Require explicit user approval after Review.

## Quality Gates
- Build gate: `dotnet build`
- Unit test gate: `dotnet test src/D2NG.Core.Tests/D2NG.Core.Tests.csproj`
- Release readiness gate: `dotnet build -c Release`

If a required gate fails, stop and report the failing command and summary before continuing.

## TDD Policy
- Prefer red-green-refactor.
- If red phase is not feasible, create characterization/regression tests before implementation and state why.

## Domain Safety
- Respect existing domain instruction files for Pickit, Muling, and Bot Types.
- If gameplay behavior intent is unclear (thresholds, precedence, lifecycle sequencing), ask the user before changing logic.

## Stage Output Format
For each stage, report:
- Stage
- Inputs
- Work performed
- Evidence
- Gate status
- Next action
