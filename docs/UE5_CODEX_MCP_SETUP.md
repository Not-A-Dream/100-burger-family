# UE5 Codex MCP Setup

## Goal

Connect Codex to an Unreal Engine 5 project with the smallest useful bridge first.

The first target is not full automatic game creation. The first target is:

- Codex can manage project files, C++ source, config, docs, and Git.
- Unreal Editor can expose project state through MCP or an equivalent bridge.
- Blueprint-heavy work stays human-verifiable inside the Editor.

## Recommended Engine Strategy

```text
Preferred: UE5.8 if the MCP plugin is available and stable
Fallback: UE5.6 / UE5.7 if templates support those versions better
Rule: template supported engine version wins over experimental MCP features
```

## Project Layout

```text
100-burger-family/
  unreal-game/
    100BurgerFamily.uproject
    Config/
    Content/
    Source/
  docs/
    UE5_CODEX_MCP_SETUP.md
```

## Phase 1: File-Level Codex Integration

This works before any Editor MCP bridge exists.

Codex can safely work on:

- `README.md`
- `README_KO.md`
- `PROJECT_CONTEXT.md`
- `TODO.md`
- `docs/`
- `unreal-game/*.uproject`
- `unreal-game/Config/`
- `unreal-game/Source/`
- CSV/JSON/DataTable source files

Avoid committing generated Unreal folders:

- `Binaries/`
- `Build/`
- `DerivedDataCache/`
- `Intermediate/`
- `Saved/`

## Phase 2: Unreal Editor MCP Bridge

When UE5.8 or a compatible plugin is available:

1. Create or open the UE5 project in `unreal-game/`.
2. Enable the Unreal MCP plugin in the Editor.
3. Restart Unreal Editor.
4. Confirm the MCP server/endpoint shown by the plugin.
5. Register that MCP server with Codex.
6. Verify Codex can read basic Editor/project state.

Expected first useful operations:

- list loaded project information
- inspect selected Actor
- inspect asset metadata
- trigger simple Editor commands
- run scripted validation tasks

Do not rely on MCP for these until proven:

- fully editing Blueprint graphs
- retargeting MaAM animations end-to-end
- rewriting marketplace templates automatically
- generating final level design without human review

## Phase 3: Game Framework Integration

Once the bridge is verified, build the shared location-management framework:

```text
Visitor
  enter -> wait -> interact -> satisfy -> exit

Staff
  idle -> move -> serve/support -> return

Zone
  entrance / queue / service / table / exhibit / shelf / exit

Service
  burger order / counseling / delivery / museum viewing / bookstore browsing
```

## Validation Checklist

- UE project opens from `unreal-game/*.uproject`.
- Codex can edit docs/config/source files.
- Git status stays clean except intentional changes.
- Unreal generated folders are ignored.
- MCP plugin is enabled in Unreal Editor.
- Codex can call at least one safe MCP read command.
- A human can verify all Blueprint edits in the Editor.

## Current Status

```text
UE project: not created yet
Demo build: external-demos/DTT_Demo_1.3 is execution-only
MCP bridge: pending UE5 version/plugin verification
```
