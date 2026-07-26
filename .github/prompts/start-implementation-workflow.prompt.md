---
description: "Start the staged implementation workflow: planning, TDD, implementation, review, unit test expansion, and QA."
name: "Start Implementation Workflow"
argument-hint: "Goal, scope, constraints, and done criteria"
agent: "Implementation Orchestrator"
model: ["Claude Sonnet 5 (copilot)", "GPT-5.3-Codex (copilot)"]
tools: [read, search, agent, todo]
---
Run the implementation workflow for the request below.

Required input format:
- Goal:
- In scope:
- Out of scope:
- Constraints:
- Done criteria:
- Risk level (low/medium/high):

Execution policy:
- Follow stage order exactly.
- Pause for approval after Planning and after Review.
- Enforce build/test/release gates.
- Use TDD when feasible, otherwise require characterization tests before implementation.
