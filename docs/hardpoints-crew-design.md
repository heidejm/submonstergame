# Hardpoints, Crew & Mutations Design

## Overview

Submarines have customizable loadouts through **hardpoint slots** that accept equipment. **Crew members** are required to operate these systems - unmanned equipment is non-functional. Monster attacks can kill crew, degrading the submarine's capabilities over time.

Monsters have **body parts** that define their abilities, which can be modified by **random mutations** at encounter start.

---

## Submarine Hardpoints

### Hardpoint Types

| Type | Purpose | Crew Required |
|------|---------|---------------|
| **Weapon** | Offensive capabilities | 1-2 per weapon |
| **Engine** | Movement speed/range | 1-2 |
| **Navigation** | Required for movement | 1 |
| **Shield** | Damage reduction/absorption | 0-1 |
| **Utility** | Special abilities (sonar, flares, etc.) | 0-1 |

### Submarine Classes (Examples)

```
Scout Sub
├── Hardpoints: 2 Weapon, 1 Engine, 1 Nav, 1 Utility
├── Crew Capacity: 4
└── Base Stats: Fast, fragile

Assault Sub
├── Hardpoints: 4 Weapon, 1 Engine, 1 Nav, 1 Shield
├── Crew Capacity: 8
└── Base Stats: Slow, heavily armed

Support Sub
├── Hardpoints: 1 Weapon, 2 Engine, 1 Nav, 2 Utility
├── Crew Capacity: 5
└── Base Stats: Medium speed, utility-focused
```

### Equipment Examples

**Weapons:**
| Name | Damage | Range | Crew | Notes |
|------|--------|-------|------|-------|
| Torpedo Launcher | 15 | 4 | 2 | High damage, slow reload |
| Harpoon Gun | 8 | 2 | 1 | Fast, short range |
| Depth Charges | 12 | 1 | 1 | AOE potential |
| Electric Discharge | 6 | 1 | 1 | Stun effect |

**Engines:**
| Name | Speed Bonus | Crew | Notes |
|------|-------------|------|-------|
| Standard Engine | +0 | 1 | Base movement |
| Turbine Engine | +2 | 2 | High speed, more crew |
| Silent Running | +0 | 1 | Reduces monster detection |

**Utility:**
| Name | Effect | Crew | Notes |
|------|--------|------|-------|
| Sonar Ping | Reveal monster stats | 1 | Intel gathering |
| Emergency Repair | Restore shield/HP | 1 | Limited uses |
| Flare | Distract monster | 0 | Single use |

---

## Crew System

### Core Mechanics

- Each submarine has a **crew capacity** (max crew)
- Crew are assigned to **stations** (hardpoint equipment)
- **Unmanned stations cannot be used**
- Crew can be **reassigned** during the player's turn (costs action?)

### Station Requirements

```
[Submarine]
├── Navigation (1 crew) ─── REQUIRED for any movement
├── Engine (1-2 crew) ──── Determines movement range
├── Weapon 1 (2 crew) ──── Torpedo Launcher
├── Weapon 2 (1 crew) ──── Harpoon Gun
└── Utility (1 crew) ───── Sonar

Total: 6-7 crew needed for full operation
```

### Crew Death

When the submarine takes damage:
1. Calculate crew casualties (based on damage amount?)
2. Randomly select which crew die
3. Their stations become unmanned
4. Player must reassign remaining crew or lose access to systems

**Damage → Crew Loss Formula Options:**
- Fixed: 1 crew per X damage
- Percentage: 10% of damage = crew lost
- Threshold: Damage > 10 = 1 crew, > 20 = 2 crew, etc.
- Random: Roll for each crew member's survival

### Crew Reassignment

During player's turn:
- Can move crew between stations
- Cost: Free? Or uses an action/movement?
- Constraint: Can only reassign from unmanned or overstaffed stations

---

## Monster Parts & Mutations

### Monster Anatomy

Monsters have **body parts** that determine capabilities:

```
[Sea Beast]
├── Head ────── Attack type, range
├── Tentacles ─ Number of attacks, grab ability
├── Body ────── Health, armor
├── Fins ────── Movement speed
└── Tail ────── Special attack, knockback
```

### Base Monster Types

| Type | Size | HP | Speed | Attack | Special |
|------|------|-----|-------|--------|---------|
| Leviathan | 3x3x3 | 100 | 2 | 15 | Swallow (insta-kill) |
| Kraken | 2x2x2 | 60 | 3 | 10 | Multi-attack |
| Serpent | 1x1x3 | 40 | 5 | 8 | Coil (immobilize) |
| Angler | 2x2x2 | 50 | 2 | 12 | Lure (pull subs closer) |

### Mutation System

At encounter start, roll for mutations:
- **Mutation Chance**: 10% base (rare but very dangerous)
- **Mutation Count**: 1-3 mutations possible
- **Reveal**: Hidden until observed (sonar) or mutation triggers

### Mutation Examples

| Mutation | Effect | Visual Tell |
|----------|--------|-------------|
| Armored Scales | +25% damage reduction | Darker coloration |
| Berserk | +50% damage when wounded | Red glow when hurt |
| Regeneration | Heal 5 HP per turn | Pulsing effect |
| Extra Limb | +1 attack per turn | Additional tentacle |
| Venomous | Attacks apply DOT | Green particles |
| Swift | +2 movement range | Elongated fins |
| Thick Hide | Immune to < 5 damage | Bulkier model |
| Ambush | First attack deals 2x | Camouflage pattern |

### Mutation Discovery

Players can discover mutations via:
1. **Observation** - See mutation trigger in combat
2. **Sonar Ping** - Reveals 1 random mutation
3. **Intel** - Pre-mission briefing hints at mutation

---

## Data Structures

### Submarine

```csharp
public class SubmarineConfig
{
    public string Name;
    public int CrewCapacity;
    public List<HardpointSlot> Hardpoints;
    public int BaseHealth;
    public int BaseMovement;
}

public class HardpointSlot
{
    public HardpointType Type;
    public Equipment InstalledEquipment;
    public int AssignedCrew;
    public bool IsOperational => AssignedCrew >= RequiredCrew;
}
```

### Equipment

```csharp
public abstract class Equipment
{
    public string Name;
    public HardpointType SlotType;
    public int CrewRequired;
    public abstract void Activate(IGameState state, IEntity user);
}

public class WeaponEquipment : Equipment
{
    public int Damage;
    public int Range;
    public int Cooldown;
}
```

### Mutations

```csharp
public class Mutation
{
    public string Name;
    public string Description;
    public MutationTrigger Trigger; // Passive, OnDamaged, OnAttack, etc.
    public Action<IEntity, IGameState> Effect;
}

public class MutatedMonster
{
    public MonsterConfig BaseType;
    public List<Mutation> ActiveMutations;
    public bool[] MutationsRevealed;
}
```

---

## Implementation Phases

### Phase 1: Hardpoint Framework
- [ ] HardpointSlot and HardpointType
- [ ] Equipment base class
- [ ] SubmarineConfig with hardpoint definitions
- [ ] Basic weapon equipment

### Phase 2: Crew System
- [ ] Crew count and assignment tracking
- [ ] Station operational checks
- [ ] Crew death on damage
- [ ] Crew reassignment action

### Phase 3: Equipment Variety
- [ ] Multiple weapon types
- [ ] Engine equipment (affects movement)
- [ ] Shield equipment
- [ ] Utility equipment

### Phase 4: Monster Mutations
- [ ] Mutation definitions
- [ ] Random mutation assignment at encounter start
- [ ] Mutation triggers and effects
- [ ] Mutation discovery mechanics

---

## Design Decisions

1. **Crew Reassignment Cost** - Free for now (designed to be adjustable later)
2. **Crew Death Formula** - Survival rolls per crew member; bigger hits = worse odds
3. **Equipment Switching** - Base only, unless sub has special equipment allowing field swaps
4. **Multiple Subs** - 4 submarines per mission
5. **Mutation Rate** - 10% chance (rare but very dangerous when encountered)
6. **Part Targeting** - Yes, players can target specific monster parts to disable abilities

---

*This system creates tactical depth: players must manage crew as a resource, prioritize which systems to keep operational, and adapt to unexpected monster mutations.*
