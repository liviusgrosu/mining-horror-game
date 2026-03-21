# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

First-person mining horror game built in Unity 2022.3 (URP). The player descends into a mine, collects minerals (Copper/Silver/Gold), upgrades their pickaxe at anvils, and must evade enemy creatures to escape. No direct combat — survival through evasion and sprint management.

## Build & Development

- **Unity Version**: 2022.3 with Universal Render Pipeline (URP)
- **Scenes in build**: Demo1.unity (main level), Demo2.unity (alternate)
- Open in Unity Hub, then build via File > Build Settings
- No external build scripts, CI, or test framework configured

## Key Packages

- URP 17.3.0, AI Navigation 2.0.10 (NavMesh), TextMesh Pro, ProBuilder, Timeline

## Architecture

### Singleton Managers

Global state is managed through singletons accessed via static `.Instance` properties:

- **GameManager** — game state (pause, death, win), mineral inventory, UI overlays, enemy spawn triggers
- **PickaxeHand** — weapon system (3 tiers: Bronze/Silver/Gold), raycasting for hits, VFX spawning
- **ScreenShakeEffect** — camera shake coroutine, temporarily disables player movement
- **MusicManager** — switches between ambient and chase music tracks
- **OtherSFXManager** — plays one-shot environmental sounds (earthquake)

### Game Progression Flow

1. Player mines deposits → collects minerals → upgrades pickaxe at anvil
2. **First Gold pickup** triggers: first enemy spawns in chase mode, screen shake, earthquake SFX, blockage wall appears
3. **Silver Pickaxe upgrade** triggers: second enemy spawns, first enemy chase ends via EndChaseCollider
4. Player reaches exit trigger → GameWinTrigger fades to white → win screen

### Enemy AI (ShadeBehaviour)

Finite state machine: Idle → Patrol → Engage → Check → Return. Uses NavMeshAgent for pathfinding, FOV-based detection with raycast line-of-sight verification. Chase can be triggered by game events (gold pickup) or player detection.

### Mining System (MineralDeposit)

Ore deposits have 100 HP. First hit spawns a weak point at a random surface position (2.5x damage multiplier). Pickaxe power level gates breakable walls. Destruction spawns collectible mineral prefabs.

### Player Interaction (Pickup + PickaxeHand)

Two raycast systems: Pickup (3m range, E-key) handles collecting minerals and interacting with objects. PickaxeHand (5m range, LMB) handles mining and breaking walls.

### Tags Used for Gameplay Logic

Player, Enemy, Mineral, Anvil, Entrance Door, Mineral Deposit, Blockage Rock, Breakable, Gravel, Stone

### Interface

`IInteractable` — used by Mineral for outline toggle on hover.

## Code Conventions

- C# scripts in `Assets/Scripts/` (flat structure, no subdirectories for gameplay scripts)
- Editor scripts in `Assets/Scripts/Editor/` and `Assets/TutorialInfo/Scripts/`
- Coroutines used extensively for fade effects, camera shake, audio transitions
- Surface detection via raycast + tag comparison (Gravel/Stone) for footstep audio
