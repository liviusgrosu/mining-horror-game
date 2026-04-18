# Iteration 3 Design Document

## Core Identity

**"Read the environment, improvise with what you have."**

The player is a miner whose ingenuity with limited tools determines whether they survive. The mine is the puzzle. Gems are how you interact with it. Enemies are hazards within it.

The player's skill is **observation and improvisation** — not reflexes or combat mastery. A skilled player reads the environment, plans with their current tools, and finds creative solutions. The game is a dialogue: the player asks "can I do this?" and the systems respond "yes."

**Design philosophy:**
- Immersive sim principles: systems over scripts, environment communicates, player feels clever
- "Inch wide, miles deep" — tight environments with deep interactivity
- Scarcity creates tension — gems and charges are precious
- Enemies are environmental obstacles, not combat targets

---

## The Pickaxe

The pickaxe is the player's multi-tool. Everything they do to the environment goes through it. It has two upgradeable components:

### Pick (Head)
- Determines **mining power**
- Linear progression — upgrades let you mine harder ores and break tougher blockages
- Mines **charge-crystals** to refuel gem abilities
- Simple and clear: better pick = access more of the mine

### Handle
- Determines **gem enhancement**
- Different handles buff different gem categories (stealth, distraction, acrobatics)
- Craftable and swappable — soft playstyle identity, not a permanent commitment
- A stealth handle makes stealth gems last longer / cost fewer charges
- A distraction handle makes distraction gems more effective

### Diegetic Feedback (always on, no gem needed)
The pickaxe IS the player's HUD. No UI overlays for stealth state.

- **Sound level:** pickaxe subtly vibrates/hums proportional to noise the player is making. Louder = more visible vibration. Standing still = nothing.
- **Visibility exposure:** pickaxe metal tint shifts. Cool/dim = hidden in shadow. Warm/bright = exposed in light.
- **Long-term goal:** full diegetic UI on the pickaxe (Dead Space health spine style). Remove all HUD overlays.

---

## Gems (Abilities)

Gems are active abilities the player collects and uses. They are the primary expression of player agency.

### Rules
- **No slot limit** — if you find it, you can use it
- **All gems are active abilities with charges** — no passive gem category
- Charges prevent spamming; refueled by mining charge-crystals (ore)
- Crafting/item-based recharging planned for later iterations
- Selected via **radial menu** (hold button, flick to gem, release to activate)
- Enhanced by matching handle type (stealth handle + stealth gems = better effect)

### Iteration 3 Gems

| Gem | Category | Effect | Charges |
|-----|----------|--------|---------|
| **Dampening** | Stealth | Suppresses player noise for X seconds. Footsteps silent, mining quieter. | Consumed on activation |
| **Decoy** | Distraction | Creates a noise event at a target location. Enemy hears it and investigates. | Consumed per use |

Additional gems for later iterations: invisibility, shadow (extinguish lights), distraction flash, super jump, dash, etc.

### What Gems Are NOT
- Gems are not passive buffs you slot and forget
- Gems don't provide information the player needs for core systems (sound/light levels are built into the pickaxe for free)
- Gems don't replace observation — they augment it

---

## Stealth System

Comprehensive Thief-inspired stealth built on two channels: **sound** and **light**. The player can read both channels and manipulate them with movement choices, environmental awareness, and gem abilities.

### Channel 1: Sound

**Surface Noise System**
- Every walkable surface is tagged with a noise value:
  - **Dirt/earth** — quiet (low noise)
  - **Stone** — moderate
  - **Gravel** — loud
  - **Metal grate** — very loud
  - **Wood** — moderate-loud (creaky)
  - **Water** — splash (loud, directional)
- Player footstep volume = surface noise value x movement speed multiplier:
  - Crouch-walk: 0.2x
  - Walk: 0.5x
  - Run: 1.0x
  - Sprint: 1.5x
- Mining generates noise scaled by ore hardness
- All noise is broadcast as world-space events with a position and radius

**What the player can manipulate:**
- Choose which surfaces to walk on (route planning)
- Control movement speed (speed vs. stealth tradeoff)
- Use Dampening gem to suppress noise temporarily
- Use Decoy gem to create noise elsewhere
- Time mining for when the enemy is far away

### Channel 2: Light and Visibility

**Light Zone System**
- Areas tagged with light level: **lit**, **dim**, **dark**
- Player visibility value based on current zone
- Player's light gem (if active) adds to their visibility — real tradeoff: see but be seen
- Light-sensitive elements in the mine (glowing fungi, lanterns, crystals) create natural light variation

**What the player can manipulate:**
- Stay in shadows (route planning around light sources)
- Toggle light gem off to hide (but lose visibility)
- Use environment for cover (pillars, equipment, rock formations)
- Future: extinguish light sources, shadow gem

### Enemy Perception

**Hearing**
- Enemies listen for noise events within a hearing radius
- Noise volume attenuated by distance
- Threshold determines reaction:
  - Below threshold: ignore
  - Above threshold: suspicious
  - Well above: search/chase
- Mining, footsteps, and gem activations all generate noise through the same system — no special cases

**Vision**
- Cone-based FOV detection (existing system, refined)
- Detection range modulated by player's visibility value:
  - Lit player: full detection range
  - Dim: half range
  - Dark: close range only
- Raycast line-of-sight verification (existing)

### Enemy State Machine

```
Idle ──→ Suspicious ──→ Searching ──→ Chasing
 ↑            │              │             │
 └────────────┴──── (timer expires, lost player) ────┘
```

| State | Trigger | Behavior | Audio Cue | Visual Cue |
|-------|---------|----------|-----------|------------|
| **Idle/Patrol** | Default | Follows patrol route, predictable | Calm breathing | Relaxed posture, steady pace |
| **Suspicious** | Noise above threshold OR brief visual contact | Pauses, looks toward stimulus, short timer | Alert grunt/growl | Head scanning, stops moving |
| **Searching** | Sustained noise OR suspicious timer expires without resolution | Moves to stimulus location, investigates area | Heavy footsteps, agitated breathing | Purposeful movement, checking corners |
| **Chasing** | Direct visual confirmation of player | Sprint pursuit, terrifying | Roar/scream, sprinting sounds | Full sprint, direct line to player |

**De-escalation:**
- Chasing → Searching: player breaks line of sight for X seconds
- Searching → Suspicious: investigation timer expires, nothing found
- Suspicious → Idle: short timer expires, returns to patrol
- Each de-escalation takes time — the player must stay hidden and wait

**Enemy proximity communication:**
- Spatial audio only — footsteps, breathing, growling
- No UI indicators — the player reads the enemy like a real creature
- Audio cues change per state so the player can identify enemy state by listening

---

## Block Placing (Scripted)

Not systemic — specific predetermined locations in the level.

**Two uses:**
1. **Block enemy patrol paths** — mine a support beam or collapse a passage to redirect or stop an enemy
2. **Open new passages** — break through a weak wall (requires sufficient pickaxe power)

**Design intent:** tactical tool that gives the player environmental control at key decision points. Not a building system — pre-placed interactive points in the level.

---

## Enemies

Enemies are **environmental obstacles with readable behavior**, not combat targets.

**Core principles:**
- Few enemies, well-placed. Each encounter is a problem to solve, not a fight to win.
- Readable through observation — the player learns their patterns by watching and listening
- React to the same systems as everything else — noise attracts them, light exposes the player to them
- Killing is **emergent, not designed** — the player manipulates the environment and sometimes the result is lethal (collapse, hazard, pit). Not a direct combat verb.
- Threatening enough that avoidance is the default

---

## System Interaction Map

```
┌─────────────┐     generates      ┌──────────────┐
│   PLAYER    │───────────────────→│ NOISE EVENTS │
│  MOVEMENT   │                    │  (position,  │
│(surface+speed)                   │   radius)    │
└─────────────┘                    └──────┬───────┘
                                          │
┌─────────────┐     generates             │ heard by
│   MINING    │──────────────────→────────┤
│ (ore + pick │                           │
│   power)    │                    ┌──────▼───────┐
└─────────────┘                    │    ENEMY     │
                                   │  PERCEPTION  │
┌─────────────┐     determines     │ (hearing +   │
│ LIGHT ZONES │───────────────────→│   vision)    │
│(lit/dim/dark)│                   └──────┬───────┘
└──────┬──────┘                           │
       │                                  │ drives
       │ affects          ┌───────────────▼────────┐
       │                  │    ENEMY STATE MACHINE  │
┌──────▼──────┐           │ Idle→Suspicious→Search  │
│   PLAYER    │           │      →Chasing           │
│ VISIBILITY  │           └────────────────────────┘
└──────┬──────┘
       │                  ┌────────────────────────┐
       │ displayed on     │         GEMS           │
       │                  │ Dampening → suppresses  │
┌──────▼──────┐           │   noise events         │
│  PICKAXE    │           │ Decoy → creates noise  │
│  DIEGETIC   │           │   events               │
│  FEEDBACK   │           │ (both feed into same    │
│(tint + vibe)│           │  noise system)          │
└─────────────┘           └────────────────────────┘
```

**Key principle:** gems don't need special enemy code. They create or suppress noise/light events. The enemy already reacts to those events. Systems talk to systems.

---

## Build Phases

### Phase 1: Stealth Foundation
*No dependencies. Build first. Can be built in parallel.*

**1a. Surface Noise System**
- Tag system for ground surfaces (gravel, stone, dirt, metal, wood)
- Player footstep volume = surface noise x movement speed multiplier
- Mining noise generation (scaled by ore hardness)
- Noise events broadcast as world-space events with position and radius
- [ ] Surface tags on terrain/objects
- [ ] Noise event system (broadcast + attenuation)
- [ ] Player noise generation (footsteps + mining)

**1b. Light Zone System**
- Areas tagged with light level (lit, dim, dark)
- Player visibility value based on current zone
- Existing light gem affects player visibility when active
- [ ] Light zone tagging
- [ ] Player visibility calculation
- [ ] Light gem integration

### Phase 2: Enemy Perception
*Depends on Phase 1.*

**2a. Enemy Hearing**
- Listen for noise events within hearing radius
- Distance attenuation on noise volume
- Threshold-based reaction (ignore / suspicious / search / chase)
- [ ] Noise listener on enemy
- [ ] Distance attenuation
- [ ] Threshold reactions feeding state machine

**2b. Enemy Vision**
- Cone-based FOV (refine existing)
- Range modulated by player visibility from light zones
- Raycast LOS verification (existing)
- [ ] Vision range modulation by light level
- [ ] Integration with light zone system

**2c. Enemy State Machine Rework**
- Refactor ShadeBehaviour into four states: Idle, Suspicious, Searching, Chasing
- Distinct audio cue per state
- Distinct visual behavior per state
- De-escalation timers
- [ ] State machine refactor
- [ ] Audio cues per state
- [ ] Visual/animation behavior per state
- [ ] De-escalation logic

### Phase 3: Diegetic Feedback
*Depends on Phase 1. Can be built alongside Phase 2.*

**3. Pickaxe Feedback**
- Vibration/particle effect scales with player noise output
- Material tint shift based on player visibility value
- [ ] Noise visualization on pickaxe model
- [ ] Visibility tint on pickaxe material
- [ ] Tuning and readability testing

### Phase 4: Gems
*Depends on Phase 1 + Phase 2.*

**4. Two Starter Gems + Radial Menu**
- Dampening gem: suppress player noise for X seconds
- Decoy gem: create noise event at target location
- Radial menu for gem selection
- Charge system (use charges, refuel by mining)
- [ ] Gem base system (charges, activation, radial UI)
- [ ] Dampening gem implementation
- [ ] Decoy gem implementation
- [ ] Charge-crystal mining for refueling

### Phase 5: Test Level
*Depends on all above. Integration test.*

**5. Focused Test Area**
- Mine section with mixed surfaces (gravel corridor, dirt side path, metal grate shortcut)
- Lit main passage, dark side tunnels
- One enemy with patrol route through the area
- Objective on the other side of the patrol
- Dampening gem and decoy gem in discoverable off-path locations
- At least one scripted block-placing point
- [ ] Level blockout with surface variety
- [ ] Light zone placement
- [ ] Enemy patrol route
- [ ] Gem placement
- [ ] Block-place point
- [ ] Playtesting: do players read the environment? experiment with gems? find unplanned solutions? feel scared?

### Dependencies

```
Phase 1a (Surface Noise) ──┐
                            ├──→ Phase 2a (Enemy Hearing) ──┐
Phase 1b (Light Zones) ────┤                                ├──→ Phase 4 (Gems)
                            ├──→ Phase 2b (Enemy Vision)    │
                            │                                ├──→ Phase 5 (Test Level)
                            ├──→ Phase 2c (State Machine) ──┘
                            │
                            └──→ Phase 3 (Pickaxe Feedback)
```

---

## Playtest Questions (Phase 5)

These are the questions the test level must answer:

1. **Do players read the environment before acting?** If they rush in, the space isn't communicating.
2. **Do players experiment with gems on the environment?** If they only try to use gems directly on the enemy, the environment isn't reactive enough.
3. **Do players find solutions we didn't plan?** If yes, the systems are working.
4. **Is it scary?** If the enemy doesn't create tension, it needs to be more threatening.
5. **Can players tell their stealth state from the pickaxe?** If they keep getting caught without understanding why, the diegetic feedback isn't clear enough.
6. **Do the two gems feel meaningfully different?** Dampening = "I become quieter." Decoy = "I make noise elsewhere." If players don't see distinct use cases, the gems are too similar.
7. **Is the charge economy right?** Too many charges = no tension. Too few = frustration. Players should feel every use matters.

---

## Out of Scope (Later Iterations)

- Acrobatics playstyle and associated gems
- Handle crafting and swapping (iteration 3 uses a single default handle)
- Crafting system for gems
- Item-based charge refueling (beyond mining ore)
- Additional gems beyond dampening and decoy
- Multiple enemies in one area
- Full diegetic UI (no HUD at all)
- Prospector/divining gem for finding secrets
