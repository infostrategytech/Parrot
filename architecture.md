# Architecture

> Context document for the agent-design platform. Read this first in every
> session before touching code. It defines the system's shape, vocabulary, and
> the invariants that must not be violated. When code and this document
> disagree, fix one of them deliberately — do not let them drift silently.

## What this product is

A low-code SaaS where non-technical end customers build agentic systems
without knowing they are doing so. A customer answers a dynamically generated
questionnaire in plain language and dynamic UI  ("I want something that answers billing
questions and escalates angry customers"), and the system produces a sound
supervisor / sub-agent architecture behind the scenes. Users see friendly
concepts ("give it a teammate who handles refunds"); the system records
architectural primitives ("spawn a sub-agent under the supervisor with the
refund tool, classed ACTION-HIGH").

The named, non-negotiable constraint is **reliability**: a customer-authored
agent must not break in production. Most of the architecture exists to enforce
that constraint by construction rather than by hope.

## The core problem

The system bridges two models that never meet directly:
-
- The user's **mental model**: goals, in plain words, with no notion of
  topology, tool risk classes, or chain depth.
- The machine's **execution model**: a supervisor, sub-agents, tools, edges,
  policies, and budgets that actually run.

The questionnaire captures the first. The compiler produces the second. The
gap between them is where all the product value lives.

## The three planes

The system is divided into three planes with hard boundaries. Keep them
separate. A change in one plane must not reach into another except through the
defined artifact that passes between them (the partial spec, then the
validated spec, then the trace).

### 1. Intent plane

Captures plain-language goals through a dynamically generated questionnaire.

- Questions are generated from the current **partial spec**, not from a fixed
  form. After the user states a goal, the system knows what primitive is
  missing and asks the question that fills it — phrased in friendly terms.
- The user is unknowingly placing nodes and setting policies. Friendly phrase
  in, architectural primitive out.
- Outputs a structured **intent object** (goals, data sources, escalation
  preferences, risk appetite) — never topology directly.

This plane holds no architectural truth. It only gathers what the compiler
needs.

### 2. Compiler plane

Translates intent into a validated, declarative agent spec. **This is the
moat.** Most engineering judgment and iteration belongs here.

Three stages:

1. **Map to graph** — intent object → candidate topology (supervisor,
   sub-agents, tools, edges). These mapping rules are *learned from real
   customer usage*, not specified up front. Implement a rule once it is known;
   do not invent the ruleset blind.
2. **Validate** — run the candidate spec through the validator rule pipeline
   (see below). Failures become repair questions sent back to the intent
   plane.
3. **Emit spec** — produce a versioned, declarative spec document.

### 3. Runtime plane

Executes a validated spec.

- **Executor** runs the supervisor and sub-agents. Built on an existing
  orchestration framework (LangGraph or an agent SDK) — do not hand-roll the
  agent loop.
- **Gateway** sits in front of every tool call and enforces RBAC,
  verification, and audit emission. No tool is reachable except through it.
- **Tenant isolation** — separate credential stores, data separation, and
  per-tenant rate/cost limits. Every run emits a **trace** scoped to one
  tenant and tied to the spec version.

## The spec is data, not code

The agent spec is a declarative document (JSON/YAML), never generated source
code. This is load-bearing:

- It is safe to run thousands of customer-authored agents in one runtime
  because the runtime *interprets a constrained schema*, it does not execute
  arbitrary code.
- Versioning, diffing, rollback, and eval all depend on the spec being plain
  data.
- The validator operates on this document. The executor compiles it down to
  the orchestration framework's graph format at run time.

## Tool risk classes

Every tool carries exactly one class. The class drives gateway behavior and
validator rules.

- `READ` — retrieves information, no side effects.
- `ACTION-LOW` — reversible or low-stakes side effects.
- `ACTION-HIGH` — irreversible or high-stakes (moves money, deletes data,
  contacts customers). Defaults to requiring human approval.

## Validator rules

The validator is a pipeline of pure functions over the candidate spec. Each
function takes the spec and returns either pass or a structured failure:

```
{ rule_id, severity, repair_question, affected_node }
```

The compiler collects all failures, sorts by severity, and the highest-severity
failure becomes the next questionnaire screen. **A failed rule is never a raw
error — it is the next question the user gets asked.**

### Severity behavior

- **hard** — blocks compilation entirely. Cannot ship. (action safety,
  grounding)
- **soft** — compiles, but a conservative default is applied automatically and
  the user is told in plain language what was done. (chain length, routing
  fallback)
- **advisory** — surfaces as a note only. (cost, simplicity)

### Defaults fill the gaps

Most users will not answer most questions. The safe choice must be the
default, so a user who clicks through quickly still gets something that will
not break:

- Unanswered action class → `ACTION-HIGH` (most restrictive)
- Unspecified loop bound → low iteration cap
- No fallback defined → escalate to human

### Rule groups

**Chain length** — fights compounding error.
Cap sequential model steps per task (≤7 before a checkpoint). Reject unbounded
auto-planning. Force long tasks to split into sub-agents or staged steps.

**Action safety** (hard) — prevents irreversible mistakes.
Every tool classed READ / ACTION-LOW / ACTION-HIGH. ACTION-HIGH defaults to
human approval. No write action without a verify step after it. No action
reachable without passing the gateway.

**Grounding** (hard) — stops hallucinated answers.
Every answering capability needs a real data source. No tool declared without
a backing system. Knowledge questions require retrieval, not free recall.
Empty-result paths must have a fallback.

**Loops and cost** — caps runaway runs.
No cycles without a max-iteration bound and exit condition. Hard per-run token
and tool-call budget. Sub-agents cannot recurse into each other endlessly.
Timeout on every external call.

**Clarity of routing** — removes ambiguous decisions.
Supervisor must have non-overlapping, exhaustive routes to sub-agents. No two
sub-agents with the same trigger. A catch-all "none of the above" path is
mandatory. Each sub-agent has one clear job.

## Why these rules exist: compounding error

Per-step reliability multiplies across a chain. At 95% per step, a 20-step task
succeeds only ~36% of the time; at 90% per step it is below a coin flip. A
reliability that is fine for a single chatbot reply collapses when run in a long
sequence. Every validator rule, the gateway, the verify steps, and human
approval for ACTION-HIGH exist to interrupt that multiplication — either by
catching errors before they propagate or by keeping chains short.

## The eval loop closes the system

Every run emits a trace tied to the spec version. The rule set is also the eval
target:

- Each rule maps to a failure detectable in production traces (hit the step
  cap, bypassed approval, empty query with no fallback).
- A new failure mode seen in real traces becomes a new validator rule.
- Re-running historical specs against the new rule finds every existing agent
  with the same latent flaw.
- Re-running a customer's historical traces against a new compiler version
  proves no regression before rollout.

This is how "reliability" stays true as the platform scales across many
customers, rather than decaying.

## Build order

Each item is its own session with its own tests. Do not collapse them.

1. **Spec schema + validator** (no runtime, no LLM, no UI — pure logic + tests).
   At least one passing spec and one failing fixture per rule.
2. **Stub compiler** — intent object → candidate graph, hardcoded, no LLM yet.
3. **Questionnaire engine** — partial spec → next question. Expect the most
   iteration here.
4. **Executor** — wire the validated spec to the orchestration framework.
5. **Multi-tenancy + gateway** — isolation, RBAC, audit. Foundational, painful
   to retrofit; treat as a requirement, not a feature.
6. **Eval loop** — trace capture, replay, regression checks.

## Invariants (do not violate)

- The spec is declarative data. Never generate executable code as the spec.
- No tool call reaches a real system except through the gateway.
- Every ACTION-HIGH path defaults to human approval unless the user explicitly
  loosens it.
- Every answering capability has a backing data source.
- A failed validator rule produces a repair question, never a raw error to the
  end user.
- Conservative defaults apply to every unanswered question.
- The three planes communicate only via their defined artifacts (partial spec,
  validated spec, trace).