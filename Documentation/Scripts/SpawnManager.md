# Spawning System – Technical Overview

---

## Table of Contents

1. [Purpose & High‑Level Picture](#purpose)
2. [Key Components](#components)

   * 2.1 [CustomerSpawnedChannelSO](#channel)
   * 2.2 [SpawnRequestSO](#spawnrequest)
   * 2.3 [SimplePool & PoolRegistry](#pooling)
   * 2.4 [SpawnManager](#spawnmanager)
   * 2.5 [EmployeeAnimationController](#employeecontroller)
   * 2.6 [Quest‑related Stubs](#quest)
3. [Runtime Data & Control Flow](#flow)
4. [Extending the System](#extend)
5. [Glossary](#glossary)
6. [TODO / Future Work](#todo)

---

## 1  Purpose & High‑Level Picture

A **customer spawning framework** that is:

* **Decoupled** – no scene‑level singletons; instead it uses ScriptableObject event channels.
* **Designer‑Friendly** – spawn recipes are authored as assets (SpawnRequestSO) in the Project window.
* **Performant** – every prefab instance comes from an object pool (SimplePool) to avoid runtime allocations.
* **Extensible** – new spawn triggers, customer modifiers, and spawn points plug in with minimal code changes.


```
 ┌────────────┐   QueueSpawn()    ┌────────────────────┐
 │ QuestMgr   │ ───────────────▶ │ SpawnManager       │
 └────────────┘                  │  ↖ Rent()          │
        ▲                        │  ↓                 │
        │  OnCustomerSpawned     │  SimplePool        │
        └───────────────┐        └────────────────────┘
                        │               ↓
                ┌──────────────┐   GameObjects
                │ EmployeeAnim │   activated &
                └──────────────┘   positioned
```

---

<a name="components"></a>

## 2  Key Components

<a name="channel"></a>

### 2.1 CustomerSpawnedChannelSO *(Event Channel)*

| Field                 | Type                       | Description                                                   |
| --------------------- | -------------------------- | ------------------------------------------------------------- |
| **OnCustomerSpawned** | `event Action<GameObject>` | Multicast event raised after a customer is live in the scene. |

*Lives in* ➜ `Assets/_Game/Scripts/Core`
*Menu item* ➜ **Events / Channels / Customer Spawned**


### 2.2 SpawnRequestSO *(Spawn Recipe)*

| Field            | Type                | Notes                              |
| ---------------- | ------------------- | ---------------------------------- |
| **prefab**       | `GameObject`        | Customer to spawn.                 |
| **customerType** | `CustomerType` enum | `Regular`, `Vip`, `Quest`          |
| **questData**    | `QuestData`         | Required only for Quest customers. |

*Validation*: `OnValidate()` throws editor errors if `prefab == null` or quest data is missing.

<a name="pooling"></a>

### 2.3 SimplePool & PoolRegistry *(Object Pooling)*

* **SimplePool** – Keeps a `Stack<GameObject>`; pre‑warms `initialSize` clones, `Rent()`/`Return()` are allocation‑free. Each rented object carries a `PoolReturnHook` which returns itself to the pool when disabled.
* **PoolRegistry** – Guarantees *one* pool per prefab. `GetOrCreatePool(prefab)` lazily creates & registers a pool.

Inspector knobs:

| Parameter        | Purpose                                      |
| ---------------- | -------------------------------------------- |
| **initialSize**  | How many instances to pre‑instantiate.       |
| **expandIfFull** | Allow the pool to grow beyond `initialSize`. |

<a name="spawnmanager"></a>

### 2.4 SpawnManager *(Coordinator)*

**Responsibilities**

1. Listens to the **Spawn** Input Action (`PlayerControls.Gameplay.Spawn`).
2. Throttles spawns via `spawnCooldown` (no flag race conditions).
3. Dequeues pending `SpawnRequestSO` objects (or uses the default).
4. Rents an object from the appropriate pool, positions it, adds any quest/VIP components, **then raises** `CustomerSpawnedChannelSO`.

**Public API**

```csharp
void QueueSpawn(SpawnRequestSO request);
```

Allows QuestManager, wave systems, etc. to schedule special customers.

<a name="employeecontroller"></a>

### 2.5 EmployeeAnimationController *(Listener Example)*

* Subscribes in `OnEnable`, unsubscribes in `OnDisable` (safe for pooled employees).
* Plays `Wave` animation on every spawn; triggers `SpecialGreeting` if the spawned object has `QuestTrigger`.

<a name="quest"></a>

### 2.6 Quest‑related Stubs

| Script           | Purpose                                                                    |
| ---------------- | -------------------------------------------------------------------------- |
| **QuestData**    | Minimal ScriptableObject holding `questId`, `description`, future rewards. |
| **QuestTrigger** | Component injected at spawn; stores a reference to its `QuestData`.        |
| **VipCustomer**  | Empty tag component – future behaviours attach here.                       |

---

<a name="flow"></a>

## 3  Runtime Data & Control Flow

1. **Player presses F** ➜ *Input System* fires *Spawn* action.
2. **SpawnManager** checks `cooldown`, queries **PoolRegistry** for a pool, `Rent()`s an object.
3. The object is **configured** (quest/VIP) and **activated** at the `spawnPoint`.
4. **SpawnManager** raises the **CustomerSpawnedChannelSO** event.
5. **Any listeners** (audio, animation, analytics) respond.
6. When the customer completes its lifecycle it calls `SetActive(false)` ➜ `PoolReturnHook` returns it to the pool.

---

<a name="extend"></a>

## 4  Extending the System

### Adding a new customer prefab

1. Create the prefab under `Prefabs/Customers/`.
2. Create a **Spawn Request** asset and assign the prefab.
3. Queue it at runtime via `SpawnManager.QueueSpawn(requestAsset)` or set it as `defaultRequest`.

### Multiple / Weighted spawn points

*Create a `SpawnPointGroupSO` (array of Transforms + weights). Provide the asset to SpawnManager, replace the current single‑point pick with a weighted random draw.*

### New spawn conditions

Implement `ISpawnCondition : IInputActionCallbackReceiver` (for input, timers, AI). SpawnManager consumes a list of conditions and spawns when *any* of them fire.

### Pool tuning tips

* Pre‑warm during a loading screen.
* Track `ActiveCount / Size` in a debug HUD to spot pool exhaustion.
* Disable `expandIfFull` in release builds to keep memory predictable.

---

<a name="glossary"></a>

## 5  Glossary

| Term               | Meaning                                                                  |
| ------------------ | ------------------------------------------------------------------------ |
| **Event Channel**  | ScriptableObject that owns and dispatches a C# event.                    |
| **Prefab Variant** | A prefab that inherits from another prefab, overriding only differences. |
| **Rent / Return**  | Terms used by object pools for *get* and *release*.                      |
| **Warm‑up**        | Pre‑instantiating pool objects during loading to avoid runtime spikes.   |

---

<a name="todo"></a>

## 6  TODO / Future Work

* Replace `switch` in `ConfigureCustomer()` with an **ICustomerModifier** strategy or prefab variants.
* Implement **ISpawnCondition** interface & plug in wave timers.
* Flesh out **QuestManager** (serve‑order counts, rewards, UI).
* Create real customer art + behaviour scripts (movement, ordering, leaving).
* Add collision / navmesh if customers move.
* Build **Rush\_UI** additive overlay scene & integrate.
* Implement save‑game hooks for outstanding quests and customers in pool.
