# Wizbang: Comprehensive Weapon Combination System Design

**Last Updated**: 2025-12-22
**Status**: MVP Complete, Decisions Finalized, Ready for Phase 2
**Goal**: Create a unique bullet heaven game differentiated by weapon fusion and combo synergies
**Marketing Hook**: "Fuse weapons to create powerful combos"

---

## Table of Contents

1. [Design Decisions (Locked In)](#design-decisions-locked-in)
2. [Tag System](#tag-system)
3. [Weapons Library](#weapons-library)
4. [Weapon Combination System](#weapon-combination-system)
5. [Progression & Run Structure](#progression--run-structure)
6. [Implementation Roadmap](#implementation-roadmap)
7. [Balance Framework](#balance-framework)

---

## Design Decisions (Locked In)

All major design questions have been answered. These are the confirmed decisions for v1.0.

### Core Balance

| Decision | Choice | Notes |
|----------|--------|-------|
| Combined weapon damage | **1.5x** base | Balanced, doesn't obsolete basics |
| Synergy bonus | **25%** per matching weapon | TBD, will iterate |
| Combination limit | **No limit** | Combine as many as you want |
| Can uncombine? | **No** | Permanent decisions, high stakes |
| Weapon slots | **6** | May reduce + unlock later |
| Freed slot on combine | **Filled at next level-up** | Slot carries forward |

### Weapon System

| Decision | Choice | Notes |
|----------|--------|-------|
| Tag count - Basic | 0-2 tags | Some weapons untagged |
| Tag count - Tier 1 | 3-4 tags | Inherits from both parents |
| Tag count - Tier 2 | 5+ tags | Inherits from both Tier 1 parents |
| Combined weapon stats | **Take better of both** | Higher values win |
| Tier 2 in v1.0? | **Yes, 2 combos** | Powerful endgame options |
| Total at launch | **13 basic + 15 Tier 1 + 2 Tier 2** | 30 total weapons |

### Combination Discovery

| Decision | Choice | Notes |
|----------|--------|-------|
| Discovery method | **Offered when you have components** | Clean UX, natural discovery |
| When offered? | **Alongside new weapon options** | Part of level-up flow |
| Future iteration | Compendium hints | "You're close to X!" when 1 of 2 components |

### Items & Drops

| Decision | Choice | Notes |
|----------|--------|-------|
| Item permanence | **Mix** | Permanent + consumable |
| Curse items? | **Yes** | High risk, high reward |
| Drop method | **RNG + chests** | Enemy kills and chest spawns |

### Progression & Unlocks

| Decision | Choice | Notes |
|----------|--------|-------|
| Unlock pace | **~1 weapon per run** | Medium pace |
| Unlock type | **Mix** | Character-specific + generic |
| Discovery persistence | **Permanent** | First discovery unlocks forever |

### Run Structure

| Decision | Choice | Notes |
|----------|--------|-------|
| Win condition | **Defeat final boss** | Boss TBD |
| Difficulty curve | **Continuous escalation** | Requires combining to survive |
| Run modifiers | **None for v1.0** | Keep simple |
| Level-up frequency | **~3 per minute** | ~90 levels in 30 min run |

### Enemies

| Decision | Choice | Notes |
|----------|--------|-------|
| Enemy variety | **Sprites/HP/damage differ** | Same movement patterns |
| Elites | **3% spawn, 4x HP** | Larger sprite variants |
| Target count | **200+** on screen | Performance goal |

### Characters

| Decision | Choice | Notes |
|----------|--------|-------|
| Starting weapons | **Unique per character** | Differentiated starts |
| Draft pool | **Shared** | All characters can get any weapon |
| Stats | **Slightly different** | Minor variance |
| Passives/affinities | **Yes** | Character identity |

### Meta

| Decision | Choice | Notes |
|----------|--------|-------|
| Polish level | **Moderate** | Gameplay first, good-enough VFX |
| Story | **None** | Pure gameplay |
| Multiplayer | **Post-launch** | Not for v1.0 |

---

## Tag System

### Overview

**8 tags total** across 2 categories. Simplified from original 14-tag system.

Synergy bonus: **+25% damage per matching weapon** (subject to balance iteration)

### Elements (5)

| Tag | Effect Theme | Example Weapons |
|-----|--------------|-----------------|
| `Fire` | Burn damage, area denial | Circle of Fire, Piercing Laser |
| `Ice` | Slow/freeze effects | Frost Shard, Glacial Aura |
| `Lightning` | Chain effects, high burst | Chain Lightning, Lightning Storm |
| `Poison` | DoT, area control | Poison Cloud, Acid Spray |
| `Arcane` | Magical, seeking | Magic Missile |

### Types (3)

| Tag | Effect Theme | Example Weapons |
|-----|--------------|-----------------|
| `Gun` | Ranged projectile weapons | Rapid Fire, Frost Shard |
| `Melee` | Close-range attacks | Orbiting Blades, War Hammer |
| `Area` | AoE effects | Circle of Fire, Poison Cloud |

### Removed Tags

The following tags have been **removed** from the game:
- ~~Projectile~~ → Too generic, replaced by `Gun` for ranged weapons
- ~~Beam~~ → Not needed
- ~~Rapid~~ → Tempo removed
- ~~Heavy~~ → Tempo removed
- ~~Sustained~~ → Tempo removed
- ~~Burst~~ → Tempo removed
- ~~Summon~~ → Deferred to post-v1.0

### Synergy Examples

With 25% bonus per matching weapon:

| Matching Weapons | Bonus per Weapon |
|------------------|------------------|
| 2 | +25% |
| 3 | +50% |
| 4 | +75% |
| 5 | +100% |
| 6 | +125% |

**Example Build:**
- 3 Fire weapons = each Fire weapon gets +50% damage
- If one is a Tier 1 combo with [Fire, Melee, Area], it benefits from all matching tags

---

## Weapons Library

### Current Basic Weapons (7)

| Weapon | Tags | Base Damage | Fire Rate | Notes |
|--------|------|-------------|-----------|-------|
| Magic Missile | Arcane | 10 | 1.0 | Auto-aim seeking |
| Circle of Fire | Area, Fire | 8 | 1.0 | Tick damage in radius |
| Orbiting Blades | Melee | 15 | 0 | Always active, orbits player |
| Chain Lightning | Lightning | 15 | 0.8 | Chains to 3 enemies |
| Poison Cloud | Poison, Area | 4 | 1.5 | DoT cloud |
| Rapid Fire | Gun | 5 | 4.0 | High fire rate |
| Piercing Laser | Fire | 12 | 2.0 | Infinite pierce |

**Removed:** Boomerang (had no tags after restructure)

### New Basic Weapons (6 to add for 13 total)

#### Ice Weapons (2)

**1. Frost Shard** [Ice, Gun]
- Fires ice shards that slow enemies by 30%
- Medium projectile count, low damage per hit
- Slow stacks with multiple hits (up to 60%)
- Base Damage: 6 | Fire Rate: 2.5

**2. Glacial Aura** [Ice, Area]
- Freezing aura around player (medium radius)
- Enemies in range take damage and are slowed
- Freeze enemies solid at 3 stacks (1 second)
- Base Damage: 3 | Tick Rate: 0.5s

#### Melee Weapons (2)

**3. Blade Dash** [Melee]
- Quick melee slash in movement direction
- Deals damage in a cone ahead of player
- Very short cooldown, encourages aggressive movement
- Base Damage: 12 | Cooldown: 0.3s

**4. War Hammer** [Melee]
- Slow, powerful melee smash
- Knocks back enemies in radius
- Small AoE on impact
- Base Damage: 25 | Cooldown: 1.5s

#### Elemental Combos (2)

**5. Lightning Storm** [Lightning, Area]
- Periodic lightning strikes in random locations around player
- High damage per strike, medium radius
- 3-5 strikes per activation
- Base Damage: 18 per strike | Cooldown: 2.0s

**6. Acid Spray** [Poison, Gun]
- Rapid-fire poison projectiles in spread pattern
- Each hit applies poison DoT stack
- Projectiles leave small poison puddles on ground
- Base Damage: 4 | Fire Rate: 3.0 | DoT: 2/sec for 3s

### Tag Coverage Summary

| Tag | Weapons |
|-----|---------|
| Fire | Circle of Fire, Piercing Laser |
| Ice | Frost Shard, Glacial Aura |
| Lightning | Chain Lightning, Lightning Storm |
| Poison | Poison Cloud, Acid Spray |
| Arcane | Magic Missile |
| Gun | Rapid Fire, Frost Shard, Acid Spray |
| Melee | Orbiting Blades, Blade Dash, War Hammer |
| Area | Circle of Fire, Poison Cloud, Glacial Aura, Lightning Storm |

**Design Note:** Arcane only has 1 weapon. Consider adding **Shadow Bolt** [Arcane, Gun] in future to balance.

---

## Weapon Combination System

### How Combinations Work

**Tier Structure:**
- **Basic (Level 1)**: Single uncombined weapon
- **Tier 1 (Level 2)**: Basic + Basic = Combined Weapon
- **Tier 2 (Level 3)**: Tier 1 + Tier 1 = Super Combined Weapon

**Key Rules:**
1. Combining consumes both parent weapons
2. Combined weapon inherits ALL tags from both parents
3. Combined weapon stats = better of both parents × 1.5
4. Combined weapon has unique behavior/visual
5. Combinations are **permanent** for that run
6. Freed slot is filled at next level-up
7. Combos only offered when you have compatible weapons in inventory

### Tag Inheritance Example

```
Circle of Fire [Area, Fire]
  +
Poison Cloud [Poison, Area]
  =
Poisoned Flames [Area, Fire, Poison]
```

Poisoned Flames benefits from: Area synergy, Fire synergy, AND Poison synergy!

### Tier 1 Combinations (15 planned for v1.0)

#### Group 1: Fire Combos

**1. Poisoned Flames** = Circle of Fire + Poison Cloud
- Tags: [Area, Fire, Poison]
- Behavior: Fire ring that also applies poison DoT
- Visual: Green-tinged flames with toxic particles

**2. Flaming Blades** = Orbiting Blades + Circle of Fire
- Tags: [Melee, Area, Fire]
- Behavior: Orbiting fire blades with larger radius
- Visual: Blazing sword aura

**3. Inferno Beam** = Piercing Laser + Circle of Fire
- Tags: [Fire, Area]
- Behavior: Laser that ignites enemies, causing AoE explosions
- Visual: Orange beam with fire particles

#### Group 2: Ice Combos

**4. Frost Barrage** = Rapid Fire + Frost Shard
- Tags: [Gun, Ice]
- Behavior: Rapid-fire seeking ice projectiles
- Visual: Blue bullet stream with frost trails

**5. Frozen Orbiters** = Orbiting Blades + Frost Shard
- Tags: [Melee, Ice, Gun]
- Behavior: Orbiting ice blades that shoot shards
- Visual: Crystalline spinning blades

**6. Arctic Storm** = Lightning Storm + Glacial Aura
- Tags: [Lightning, Area, Ice]
- Behavior: Lightning strikes that freeze in radius
- Visual: Blue lightning with ice crystals

#### Group 3: Lightning Combos

**7. Storm Shield** = Orbiting Blades + Chain Lightning
- Tags: [Melee, Lightning]
   - Behavior: Orbiting blades that chain lightning to nearby enemies
- Visual: Electric arcing blades

**8. Lightning Barrage** = Rapid Fire + Chain Lightning
- Tags: [Gun, Lightning]
- Behavior: Bullets that chain to nearby enemies
- Visual: Electric bullets with arc effects

**9. Thunder Hammer** = War Hammer + Chain Lightning
- Tags: [Melee, Lightning]
- Behavior: Hammer strike chains lightning to all hit enemies
- Visual: Electric hammer with arc on impact

#### Group 4: Poison Combos

**10. Toxic Barrage** = Rapid Fire + Poison Cloud
- Tags: [Gun, Poison, Area]
- Behavior: Rapid-fire projectiles that leave poison trails
- Visual: Green bullets with toxic wake

**11. Venom Dash** = Blade Dash + Poison Cloud
- Tags: [Melee, Poison, Area]
- Behavior: Dash leaves toxic trail behind
- Visual: Green slash with lingering cloud

#### Group 5: Arcane Combos

**12. Arcane Gatling** = Rapid Fire + Magic Missile
- Tags: [Gun, Arcane]
- Behavior: Rapid-fire seeking projectiles
- Visual: Purple rapid-fire with homing trails

**13. Meteor Shower** = Magic Missile + Circle of Fire
- Tags: [Arcane, Area, Fire]
- Behavior: Seeking projectiles that explode in fire rings
- Visual: Flaming magic missiles

#### Group 6: Area Combos

**14. Poison Storm** = Lightning Storm + Poison Cloud
- Tags: [Lightning, Area, Poison]
- Behavior: Lightning strikes leave poison pools
- Visual: Green lightning with toxic splashes

**15. Acid Blizzard** = Frost Shard + Acid Spray
- Tags: [Ice, Gun, Poison]
- Behavior: Frozen acid projectiles that slow and poison
- Visual: Icy green projectiles

### Tier 2 Combinations (2 for v1.0)

Tier 2 = Tier 1 + Tier 1 (requires 5 combinations to achieve: 2 for first Tier 1, 2 for second Tier 1, 1 to combine them)

**1. Elemental Maelstrom** = Arctic Storm + Poisoned Flames
- Parent Tags: [Lightning, Area, Ice] + [Area, Fire, Poison]
- Final Tags: [Lightning, Area, Ice, Fire, Poison]
- Behavior: Swirling vortex of all elements around player
- Visual: Multi-colored elemental storm
- Power: Massive sustained AoE, slows, burns, poisons, and shocks

**2. Apocalypse Cannon** = Lightning Barrage + Arcane Gatling
- Parent Tags: [Gun, Lightning] + [Gun, Arcane]
- Final Tags: [Gun, Lightning, Arcane]
- Behavior: Rapid-fire homing bullets that chain lightning
- Visual: Purple-electric bullet storm
- Power: Ultimate single-target and chain damage

### Combination Matrix

Quick reference for what combines with what:

| Weapon | Best Combos With |
|--------|------------------|
| Magic Missile | Rapid Fire, Circle of Fire |
| Circle of Fire | Poison Cloud, Orbiting Blades, Piercing Laser |
| Orbiting Blades | Chain Lightning, Circle of Fire, Frost Shard |
| Chain Lightning | Orbiting Blades, Rapid Fire, War Hammer |
| Poison Cloud | Circle of Fire, Rapid Fire, Blade Dash, Lightning Storm |
| Rapid Fire | Magic Missile, Poison Cloud, Chain Lightning, Frost Shard |
| Piercing Laser | Circle of Fire |
| Frost Shard | Rapid Fire, Orbiting Blades, Acid Spray |
| Glacial Aura | Lightning Storm |
| Blade Dash | Poison Cloud |
| War Hammer | Chain Lightning |
| Lightning Storm | Glacial Aura, Poison Cloud |
| Acid Spray | Frost Shard |

---

## Progression & Run Structure

### Target Loadout by Time

| Time | Level 1 (Basic) | Level 2 (Tier 1) | Level 3 (Tier 2) | Total Combos Done |
|------|-----------------|------------------|------------------|-------------------|
| 0 min | 1 (starting) | 0 | 0 | 0 |
| 5 min | 4-5 | 0-1 | 0 | 0-1 |
| 10 min | 3-4 | 1-2 | 0 | 1-2 |
| 15 min | 3 | 2 | 1 | 5 |
| 20 min | 2 | 2-3 | 1 | 6-7 |
| 25 min | 1-2 | 2 | 2 | 8-10 |
| 30 min (boss) | 0-1 | 2 | 2-3 | 10+ |

### Combination Math Breakdown

To reach "3 Basic + 2 Tier 1 + 1 Tier 2" at 15 minutes:

| Step | Action | Inventory |
|------|--------|-----------|
| 1-6 | Pick up 6 basics | 6 Basic |
| 7 | Combine 2 → Tier 1 | 4 Basic, 1 T1, 1 empty |
| 8 | Fill slot | 5 Basic, 1 T1 |
| 9 | Combine 2 → Tier 1 | 3 Basic, 2 T1, 1 empty |
| 10 | Fill slot | 4 Basic, 2 T1 |
| 11 | Combine T1 + T1 → T2 | 4 Basic, 1 T2, 1 empty |
| 12 | Fill slot | 5 Basic, 1 T2 |
| 13 | Combine 2 → Tier 1 | 3 Basic, 1 T1, 1 T2, 1 empty |
| 14 | Fill slot | 4 Basic, 1 T1, 1 T2 |
| 15 | Combine 2 → Tier 1 | 2 Basic, 2 T1, 1 T2, 1 empty |
| 16 | Fill slot | 3 Basic, 2 T1, 1 T2 ✓ |

**Total combinations: 5** (as expected)

### Difficulty Curve

| Time | Enemy HP | Enemy Damage | Spawn Rate | Notes |
|------|----------|--------------|------------|-------|
| 0-5 min | 50-100 | Low | Low | Tutorial phase |
| 5-10 min | 100-200 | Medium | Medium | First combo recommended |
| 10-15 min | 200-400 | Medium-High | High | Tier 1 combos needed |
| 15-20 min | 400-800 | High | Very High | Tier 2 helpful |
| 20-25 min | 800-1600 | Very High | Max | Tier 2 required |
| 25-30 min | 1600+ | Extreme | Max | Boss prep |

### Elite Enemies

- **Spawn Rate**: 3% of regular spawns
- **HP**: 4× regular enemy
- **Damage**: 2× regular enemy
- **Size**: 1.5× regular sprite
- **Drop Rate**: Guaranteed item drop on kill

---

## Implementation Roadmap

### Phase 1: MVP (✅ COMPLETE)

- [x] Tag system (now 9 tags)
- [x] Synergy calculations
- [x] UI for synergies and weapons
- [x] 7 basic weapons working

---

### Phase 2: Core Combination System (Weeks 1-2)

**Week 1: Infrastructure**
- [ ] Create `WeaponCombination` ScriptableObject
- [ ] Create `CombinationDatabase` with all 20 Tier 1 recipes
- [ ] Implement `WeaponInventory.CombineWeapons()` method
- [ ] Add combination validation (do you have both components?)
- [ ] Implement stat inheritance (take better of both × 1.5)
- [ ] Implement tag inheritance (union of both tag sets)

**Week 1: Level-Up UI**
- [ ] Detect when player has combinable weapons
- [ ] Show combination option alongside new weapon choices
- [ ] Design combo preview card (show parents → result)
- [ ] Show inherited tags and estimated power
- [ ] Handle freed slot tracking

**Week 2: First 5 Tier 1 Combinations**
- [ ] Poisoned Flames (Circle of Fire + Poison Cloud)
- [ ] Flaming Blades (Orbiting Blades + Circle of Fire)
- [ ] Storm Shield (Orbiting Blades + Chain Lightning)
- [ ] Toxic Barrage (Rapid Fire + Poison Cloud)
- [ ] Arcane Gatling (Rapid Fire + Magic Missile)

**Week 2: Testing**
- [ ] Verify combinations work in gameplay
- [ ] Test synergy stacking with combined weapons
- [ ] Balance pass on damage multipliers
- [ ] Test level-up flow

---

### Phase 3: Weapon Expansion (Weeks 3-4)

**Week 3: New Basic Weapons**
- [ ] Frost Shard [Ice, Gun]
- [ ] Glacial Aura [Ice, Area]
- [ ] Blade Dash [Melee]
- [ ] War Hammer [Melee]
- [ ] Lightning Storm [Lightning, Area]
- [ ] Acid Spray [Poison, Gun]

**Week 4: Remaining Tier 1 Combinations**
- [ ] Inferno Beam (Piercing Laser + Circle of Fire)
- [ ] Meteor Shower (Magic Missile + Circle of Fire)
- [ ] Lightning Barrage (Rapid Fire + Chain Lightning)
- [ ] Frost Barrage (Rapid Fire + Frost Shard)
- [ ] Frozen Orbiters (Orbiting Blades + Frost Shard)
- [ ] Arctic Storm (Lightning Storm + Glacial Aura)
- [ ] Thunder Hammer (War Hammer + Chain Lightning)
- [ ] Venom Dash (Blade Dash + Poison Cloud)
- [ ] Poison Storm (Lightning Storm + Poison Cloud)
- [ ] Acid Blizzard (Frost Shard + Acid Spray)

---

### Phase 4: Tier 2 Combinations (Week 5)

**Week 5: Tier 2 System**
- [ ] Implement Tier 1 + Tier 1 → Tier 2 combination
- [ ] Update UI to show Tier 2 options
- [ ] Add visual distinction for Tier 2 weapons

**Week 5: Tier 2 Weapons**
- [ ] Elemental Maelstrom (Arctic Storm + Poisoned Flames)
- [ ] Apocalypse Cannon (Lightning Barrage + Arcane Gatling)

**Week 5: Balance**
- [ ] Test Tier 2 weapons at 15+ minute gameplay
- [ ] Adjust power levels
- [ ] Ensure Tier 2 feels rewarding but not required before 15 min

---

### Phase 5: Items & Modifiers (Weeks 6-7)

**Week 6: Item System**
- [ ] Create `Item` ScriptableObject base class
- [ ] Implement item drops from enemies
- [ ] Implement chest spawns
- [ ] Create item pickup UI
- [ ] Implement item inventory (separate from weapons)

**Week 6: Items (15 items)**
- [ ] 5 Common stat boosters
- [ ] 5 Uncommon tag amplifiers
- [ ] 3 Rare modifiers
- [ ] 2 Curse items (negative + positive)

**Week 7: Legendary/Supreme System**
- [ ] Legendary items can combine with Tier 1 weapons
- [ ] Supreme items can combine with Tier 2 weapons
- [ ] Design 3 Legendary items
- [ ] Design 2 Supreme items

---

### Phase 6: Metaprogression (Weeks 8-9)

**Week 8: Unlock System**
- [ ] Create `UnlockManager` with save/load
- [ ] Define unlock conditions
- [ ] Start with 7 weapons, unlock remaining 8
- [ ] First combination discovery = permanent unlock

**Week 9: Compendium**
- [ ] Weapons tab
- [ ] Combinations tab
- [ ] Items tab
- [ ] Characters tab
- [ ] Show unlock progress

---

### Phase 7: Characters (Week 10)

**Week 10: Character System**
- [ ] Define 3 starting characters
- [ ] Unique starting weapon per character
- [ ] Slight stat variance
- [ ] Passive abilities / tag affinities
- [ ] Character unlock conditions

---

### Phase 8: Polish & Boss (Weeks 11-12)

**Week 11: Boss Design**
- [ ] Design final boss for 30-minute mark
- [ ] Boss phases and patterns
- [ ] Victory screen / run complete flow

**Week 12: Polish**
- [ ] VFX pass on all weapons
- [ ] Sound effects
- [ ] UI polish
- [ ] Performance optimization (200+ enemies)
- [ ] Bug fixing

---

## Balance Framework

### Damage Calculation

```
Final Damage = Base Damage × Synergy Multiplier × Tier Multiplier × Upgrades

Where:
- Synergy Multiplier = 1 + (0.25 × matching weapons count)
- Tier Multiplier = 1.0 (Basic), 1.5 (Tier 1), 2.25 (Tier 2)
- Upgrades = cumulative from level-ups
```

### Example Calculation

**Poisoned Flames** [Area, Fire, Poison]
- Base Damage: 10 (better of Circle of Fire 8, Poison Cloud 4, × 1.5 rounded)
- Other weapons: Piercing Laser [Fire], Rapid Fire [Gun], Magic Missile [Arcane]
- Matching: 1 Fire weapon = +25%
- Synergy Multiplier: 1.25
- Tier Multiplier: 1.5
- Final: 10 × 1.25 × 1.5 = 18.75 damage

### Napkin DPS Estimates

| Time | Target DPS | How to Achieve |
|------|------------|----------------|
| 5 min | 50 | 4 basics firing |
| 10 min | 120 | 1-2 Tier 1 combos + synergies |
| 15 min | 300 | 2 Tier 1 + 1 Tier 2 + synergies |
| 20 min | 600 | 2-3 Tier 1 + 1-2 Tier 2 + items |
| 25 min | 1000+ | Full build, items, Tier 2s |

**Note:** These are rough targets. Iterate based on playtesting.

### Synergy Sweet Spots

| # Matching | Bonus | Feel |
|------------|-------|------|
| 2 | +25% | Noticeable but not required |
| 3 | +50% | Strong, worth building around |
| 4 | +75% | Very powerful, focused build |
| 5 | +100% | Reward for commitment |
| 6 | +125% | All-in, but limits combo options |

---

## Future Considerations (Post v1.0)

### Planned Additions
- More Tier 2 combinations (4-6 total)
- Additional basic weapons (20 total)
- Compendium hints ("You're close to discovering X!")
- Ascension / difficulty modifiers
- More characters (5 total)
- Multiplayer co-op

### Design Debt to Address
- Only 1 Arcane weapon (add Shadow Bolt)
- Summon weapons need more combo options
- Item + weapon fusion (Legendary/Supreme) needs full design
- Elite enemy variety

### Open Questions for Future
- Should some combos be "secret" (not shown until discovered)?
- Character-specific exclusive combos?
- Daily/weekly challenge runs with modifiers?
- Endless mode after boss defeat?

---

**Document Status:** Ready for Phase 2 implementation. All core decisions locked in.
