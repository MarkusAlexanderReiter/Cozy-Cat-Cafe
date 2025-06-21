# Cozy Cat Café – Game Design Document (GDD)
*Version 0.1 • June 19 2025*

---

## Table of Contents
1. [High-Level Concept](#high-level-concept)  
2. [Design Pillars](#design-pillars)  
3. [Target Audience & Platforms](#target-audience--platforms)  
4. [Core Gameplay Loop](#core-gameplay-loop)  
5. [Systems & Features](#systems--features)  
   - 5.1 [Morning Prep](#51-morning-prep)  
   - 5.2 [Brunch Rush](#52-brunch-rush)  
   - 5.3 [Siesta](#53-siesta)  
   - 5.4 [Evening Glow (Periodic)](#54-evening-glow-periodic)  
   - 5.5 [Currencies & Progression](#55-currencies--progression)  
   - 5.6 [Mini-Game Framework](#56-mini-game-framework)  
6. [Scoping & MVP Cut](#scoping--mvp-cut)  
7. [Art, Audio & Feel](#art-audio--feel)  
8. [Technical Design (Unity 2D)](#technical-design-unity-2d)  
9. [Production Roadmap](#production-roadmap)  
10. [Risks & Mitigations](#risks--mitigations)  
11. [Glossary](#glossary)  

---

## High-Level Concept
A **low-stress cat-themed cooking & café-management game**.  
Players run a rooftop café, prepare comfort food through quick tactile mini-games, decorate their space, and befriend a cast of feline patrons—all at a cozy, lo-fi pace.

---

## Design Pillars
| Pillar | Implication |
|--------|-------------|
| **Cozy, Never Stressful** | Soft failure (no game-over); autosave on every phase. |
| **Tactile Mini-Bursts** | 3-to-6 sec mini-games with satisfying haptics & sound. |
| **Visible Progress** | Café décor changes daily; new cats & recipes appear quickly. |
| **Cat Personality** | Scouts, patrons, and staff have simple quirky traits. |

---

## Target Audience & Platforms
- **Players:** Fans of _Animal Crossing_, _Dave the Diver_, lo-fi games, cat lovers (age 12 +).  
- **Primary Platform:** Nintendo Switch, Steam Deck, PC (mouse + KB / controller).  
- **Secondary:** iOS/Android tablets (stretch goal—touch & gyro support).  

---

## Core Gameplay Loop
1. **Morning Prep (1-2 min)** – Choose menu, dispatch ingredient scouts.  
2. **Brunch Rush (4-5 min)** – Seat cats, play dish mini-games, earn coins/hearts.  
3. **Siesta (2 min)** – Decorate, shop, chat, story events.  
4. **[Periodic] Evening Glow (3-4 min)** – Night party with special recipes & photos.  

Total real-time “day”: ~8-10 min. Player can quit anytime—autosave triggers as the chef curls up for a nap.

---

## Systems & Features

### 5.1 Morning Prep
- **Menu Selection** – Pick 3 dishes from owned recipe list.  
- **Scout Dispatch**  
  - Each scout-cat has _Cost_, _Forage Range (RNG)_ and _Reliability_% stats.  
  - One-tap “Send Scout” → returns after fade-out with 3-6 ingredients (MVP).  
- **Popularity Forecast** – Optional “whisker-twitch” meter hints at which dish may trend (+10 % demand).

### 5.2 Brunch Rush
- **Customer Flow** – Static seats (MVP); speech-bubble order appears.  
- **Order Logic** – Checks player inventory. If missing ingredients, order greys out.  
- **Mini-Game Trigger** – One combo mini-game per dish (slice → froth → garnish).  
- **Ratings & Tips** – Score (★-★★★) converts to Paw-Coins and Hearts.  
- **Zen Mode** – Toggle halves spawn rate for accessibility.

### 5.3 Siesta
- **Café Décor Mode** – Grid placement of furniture.  
  - _Comfiness ★_ increases seat cap & slows rush speed.  
- **Shop UI** – Spend coins on recipes, furniture, equipment.  
- **Story Beats** – Short dialogues or postcard cut-ins unlock at café Lv 1/2/3.  

### 5.4 Evening Glow (Periodic)
- Triggers every 3 in-game “days” (configurable).  
- Party lighting, upbeat lo-fi remix, photo mode.  
- **Night-Guests** request _Special Recipes_ (unlock on perfect serve).

### 5.5 Currencies & Progression
| Currency | Earned From | Spent On |
|----------|-------------|----------|
| **Paw-Coins** | Tips, party bonuses | Furniture, equipment upgrades |
| **Hearts** | Serving favorite dishes, story events | Unlock new recipes |
| **Comfiness ★** | Decor rating | Passive stat (visitor cap, rush pacing) |

### 5.6 Mini-Game Framework
- Base **`MiniGameBase`** class (`StartGame()`, `EndGame(score)` callbacks).  
- **Input “Atoms”**:  
  1. _Swipe_ (knead)  
  2. _Drag-Follow Path_ (slice)  
  3. _Tilt-Balance_ (froth temp)  
  4. _Flick_ (garnish)  
- Combine 2-3 atoms into a single 6 sec sequence per recipe.  
- Difficulty modifiers: ingredient rarity, equipment tier, special request.

---

## Scoping & MVP Cut
| Feature | MVP | Stretch |
|---------|-----|---------|
| Scouts | Single button, 1 cat | Multiple cats, personality events |
| Recipes | 3 dishes, shared mini-game | 15 +dishes, unique games |
| Customers | Static seats | Pathfinding, emotes |
| Décor | 6 items | 100 + items, color sets |
| Story | 3 text events | Monthly voiced chapters |
| Evening Glow | Disabled | Full party event |

---

## Art, Audio & Feel
- **Visuals** – Soft pastels (apricot, mint, cream). 2D hand-drawn sprites or chunky low-poly if 3D.  
- **Animation** – Slow tail sways, loaf “bread” poses, steam curls.  
- **UI** – Cardboard-box inventory slots, rounded corners, large touch targets.  
- **Audio** – Lo-fi beats; layered purr ambience that intensifies with café occupancy.  
- **Haptics** – Light rumble on kneads & perfect cuts.

---

## Technical Design (Unity 2D)
- **Language / Pattern** – C# • ScriptableObjects for data (RecipeSO, FurnitureSO).  
- **Scene Flow** – Single persistent scene; cameras switch panels per phase.  
- **Game State Machine**  
  ```csharp
  enum CafePhase { Morning, Rush, Siesta, EveningGlow }
  CafePhase current;
  event OnPhaseChanged;


* **Inventory** – `Dictionary<IngredientSO,int>`.
* **Save/Load** – JSON via `JsonUtility` or asset Easy Save 3; autosave on phase change.
* **Input** – Unity Input System; actions map to mouse, gamepad, touch + gyro.

---

## Production Roadmap

| Week | Milestone                                        |
| ---- | ------------------------------------------------ |
| 1-2  | Data setup + GameManager skeleton                |
| 3-4  | Single mini-game prototype (slice-froth-garnish) |
| 5    | Static customer queue, tip payout                |
| 6    | Shop + décor grid (6 items)                      |
| 7    | Scout button, ingredient inventory               |
| 8    | Save/Load, art/audio polish                      |
| 9-10 | User testing, bug-fix, Steam Next Fest demo      |

---

## Risks & Mitigations

| Risk                      | Impact                         | Mitigation                                        |
| ------------------------- | ------------------------------ | ------------------------------------------------- |
| **Scope Creep**           | Never-ending feature additions | MVP gate; monthly scope review                    |
| **Too Many Mini-Games**   | Art/anim overload              | Re-use input atoms; add reskins not new mechanics |
| **Performance on Mobile** | Stutters                       | Lightweight 2D, object pooling                    |

---

## Glossary

* **Paw-Coins** – Primary currency from tips.
* **Hearts** – Friendship points; unlock recipes.
* **Comfiness ★** – Décor rating stat.
* **Mini-Game Atom** – Reusable input micro-interaction (swipe, drag, tilt, flick).
* **Evening Glow** – Optional night party event with special orders.

---

*End of Document*


