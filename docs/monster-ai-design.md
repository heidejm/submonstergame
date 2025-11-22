# Monster AI System Design

## Overview

The Monster AI system is inspired by Kingdom Death: Monster's AI deck mechanic. Rather than deterministic AI that always makes "optimal" decisions, monsters draw from a deck of behavior cards that introduce controlled randomness and variety to encounters.

## Core Concepts

### AI Deck

Each monster type has an **AI Deck** - a collection of **Behavior Cards** that define possible actions. When it's a monster's turn:

1. A card is "drawn" (randomly selected) from the deck
2. The card's logic is evaluated against the current game state
3. The monster executes the resulting action(s)

### Behavior Cards

Each card is essentially a **decision tree** with conditional logic:

```
[Card Name]
├── IF (condition A) THEN
│   └── Action A
├── ELSE IF (condition B) THEN
│   └── Action B
└── ELSE
    └── Fallback Action
```

### Example Card Structure

```
"Lunge Attack"
├── IF (target in attack range) THEN
│   └── Attack nearest target
├── ELSE IF (can move toward target) THEN
│   └── Move toward nearest target
│   └── IF (now in attack range) THEN
│       └── Attack
└── ELSE
    └── Idle / Roar / Special behavior
```

## Card Components

### Conditions

Conditions evaluate the game state to determine which branch to execute:

| Condition | Description |
|-----------|-------------|
| `TargetInAttackRange` | Any valid target within attack range |
| `TargetInMoveRange` | Can reach a target this turn |
| `HealthBelow(%)` | Monster health below threshold |
| `TargetCount(n)` | Number of valid targets meets criteria |
| `HasLineOfSight` | Clear path to target exists |
| `IsWounded` | Monster has taken damage |
| `TargetIsWounded` | Potential target is below max health |

### Target Selection

Cards specify how to select targets:

| Selector | Description |
|----------|-------------|
| `Nearest` | Closest valid target (by path distance) |
| `Furthest` | Most distant valid target |
| `Weakest` | Target with lowest current health |
| `Strongest` | Target with highest current health |
| `Random` | Random valid target |
| `LastAttacker` | Entity that last damaged this monster |
| `MostWounded` | Target with lowest health percentage |

### Actions

Actions the monster can perform:

| Action | Description |
|--------|-------------|
| `Attack(target)` | Attack the specified target |
| `MoveToward(target)` | Move as close as possible to target |
| `MoveAway(target)` | Move away from target (flee) |
| `MoveRandom` | Move to random valid position |
| `Idle` | Do nothing (pass turn) |
| `Roar` | Flavor action (future: could buff/debuff) |
| `SpecialAbility(name)` | Execute a named special ability |

## Deck Composition

Different monster types have different deck compositions:

### Aggressive Monster (Example)
| Card | Count | Behavior |
|------|-------|----------|
| Lunge Attack | 3 | Move + Attack nearest |
| Frenzy | 2 | Attack twice if in range |
| Chase | 2 | Move toward furthest target |
| Rage | 1 | Attack weakest target |

### Defensive Monster (Example)
| Card | Count | Behavior |
|------|-------|----------|
| Cautious Strike | 3 | Attack if in range, else idle |
| Retreat | 2 | Move away if wounded |
| Ambush | 2 | Only attack if target adjacent |
| Territorial | 1 | Attack last attacker |

## Card Weighting (Optional Enhancement)

Cards can have **weights** affecting draw probability:

- Base weight: 1.0
- Wounded modifier: Some cards more likely when hurt
- Rage modifier: Some cards more likely after taking damage
- Proximity modifier: Some cards more likely when targets nearby

## Data Structure Proposal

```csharp
// Represents a single condition check
public interface IAICondition
{
    bool Evaluate(IGameState state, IEntity monster);
}

// Represents an action to execute
public interface IAIAction
{
    void Execute(IGameState state, IEntity monster);
}

// A single behavior card
public class BehaviorCard
{
    public string Name { get; }
    public List<ConditionalBranch> Branches { get; }
    public IAIAction FallbackAction { get; }
}

// A condition-action pair
public class ConditionalBranch
{
    public IAICondition Condition { get; }
    public List<IAIAction> Actions { get; }
}

// The deck of cards for a monster type
public class AIDeck
{
    public List<BehaviorCard> Cards { get; }
    public BehaviorCard DrawCard(System.Random rng);
}

// Executes AI turns
public class MonsterAIController
{
    public void ExecuteTurn(IEntity monster, IGameState state);
}
```

## Turn Execution Flow

```
MonsterAIController.ExecuteTurn(monster, state)
│
├── 1. Get monster's AI Deck
│
├── 2. Draw a random card from deck
│
├── 3. Evaluate card branches in order:
│   ├── For each branch:
│   │   ├── Evaluate condition
│   │   ├── If TRUE: Execute actions, STOP
│   │   └── If FALSE: Continue to next branch
│   │
│   └── If no branch matched: Execute fallback action
│
└── 4. End monster's turn
```

## Future Considerations

### Deck Manipulation
- Cards that add/remove cards from deck mid-fight
- "Shuffle" triggers that reset the deck
- "Discard" pile that prevents repeats until reshuffle

### Card Modifiers
- Environmental effects that modify card behavior
- Monster "states" (enraged, wounded, etc.) that unlock different cards

### Multi-Action Cards
- Cards that allow multiple actions (move AND attack)
- Cards with sequential actions (attack, then move away)

### Telegraphing
- Show the drawn card to player before execution
- Adds strategic element: "Monster is preparing to Lunge!"

## Implementation Phases

### Phase 1: Basic Framework
- [ ] IAICondition and IAIAction interfaces
- [ ] BehaviorCard class with conditional branches
- [ ] AIDeck with random draw
- [ ] MonsterAIController integration with turn system

### Phase 2: Core Conditions & Actions
- [ ] TargetInAttackRange condition
- [ ] CanReachTarget condition
- [ ] Attack action
- [ ] MoveToward action
- [ ] Target selectors (Nearest, Random)

### Phase 3: Sample Deck
- [ ] Create 4-6 basic behavior cards
- [ ] Test AI behavior in gameplay
- [ ] Tune weights and probabilities

### Phase 4: Data-Driven Cards
- [ ] ScriptableObject for BehaviorCard
- [ ] ScriptableObject for AIDeck
- [ ] Editor tooling for creating cards

---

*Design inspired by Kingdom Death: Monster's AI deck system*
