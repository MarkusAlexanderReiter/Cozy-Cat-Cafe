# Customer Seating System

## Table of Contents
1. [Purpose & High-Level Picture](#purpose)
2. [Key Components](#components)
3. [Runtime Data & Control Flow](#flow)
4. [Extending the System](#extend)
5. [Glossary](#glossary)
6. [TODO / Future Work](#todo)

---

## 1  Purpose & High-Level Picture <a name="purpose"></a>
* Manage café customers from spawn to despawn with minimal per-frame overhead.
* Central `SeatManager` tracks available seats and notifies waiting customers.
* `CustomerLifecycle` drives each customer’s state machine (waiting → seated → leaving).
* Object pooling (`SimplePool`) avoids costly Instantiate/Destroy cycles.
* System designed for easy swap-in of real path-finding and further behaviours.

---

## 2  Key Components <a name="components"></a>
### 2.1 SeatManager
| Field | Type | Description |
|-------|------|-------------|
| `freeColor` | `Color` | Debug colour for free seat sprites |
| `occColor`  | `Color` | Debug colour for occupied seat sprites |

* Singleton. Registers/unregisters `Seat` objects.
* Grants seats (`RequestSeat`) or queues customers when none free.
* Frees seats and wakes the next waiter.
* Provides helper `RemoveFromQueue` for pooled despawns.

### 2.2 CustomerLifecycle
| Field | Type | Description |
|-------|------|-------------|
| `_assignedSeat` | `Transform` | Currently claimed seat (null if none) |
| `_waitCoroutine` | `Coroutine` | Patience timer while waiting |
| `_seatRoutine` | `Coroutine` | Sequence after getting seat |
| `_spawnPos` | `Vector3` | Original spawn location for return teleport |

Responsibilities:
* On enable → claim seat or join wait queue.
* Waits 10-15 s; despawns if still seat-less.
* After seat granted: 3 s delay → sit 10 s → leave seat → return to spawn → despawn.
* Cleans up coroutines/seats on disable or destroy.

### 2.3 SimplePool / PoolRegistry
* Generic stack-based pool for GameObjects.
* Pre-warms a configurable count; can auto-expand.
* Returns objects to pool via internal hook on disable.

---

## 3  Runtime Data & Control Flow <a name="flow"></a>
1. `SpawnManager` rents a customer prefab from `SimplePool` and enables it.
2. `CustomerLifecycle.OnEnable` records spawn position and calls `TryClaimSeat`.
3. `SeatManager.RequestSeat` returns a free seat or enqueues the customer.
4. Waiting customers start `WaitAndDespawnRoutine` (10-15 s patience).
5. When any customer frees a seat, `SeatManager` dequeues the next waiter and sends `OnSeatAvailable` → they retry.
6. Once a seat is assigned:
   ```seq
   Customer→SeatLifecycleRoutine: wait 3s
   Customer→SeatLifecycleRoutine: teleport to seat
   Customer→SeatLifecycleRoutine: sit 10s
   Customer→SeatManager: FreeSeat
   Customer→SeatLifecycleRoutine: teleport to spawn
   Customer→Pool: SetActive(false)
   ```

---

## 4  Extending the System <a name="extend"></a>
* **Path-finding** – replace the teleport sections with a `NavMeshAgent` or custom movement, keeping the same coroutine timings.
* **Variable patience/durations** – expose the wait and sit times as serialized fields or ScriptableObjects.
* **Customer behaviours** – subscribe to "sat down" and "leaving" moments for ordering, animations, audio.
* **Seat Types** – extend `Seat` with capacity or reservation logic; `SeatManager` already centralises access.
* **Editor validation** – add play-mode tests ensuring no duplicate seats and pooled objects maintain hooks.

---

## 5  Glossary <a name="glossary"></a>
| Term | Meaning |
|------|---------|
| Seat | Placeable object a customer can occupy |
| Wait Queue | FIFO queue inside `SeatManager` storing customers without seats |
| Pool | Reusable cache of GameObjects managed by `SimplePool` |
| Patience Timer | 10-15 s window a customer waits for a seat before leaving |

---

## 6  TODO / Future Work <a name="todo"></a>
* Integrate real movement/path-finding.
* Add seated-duration variance per customer type (e.g. VIPs linger longer).
* Visual feedback: animations for walking, sitting, leaving.
* Performance: convert coroutines to UniTask async if targeting IL2CPP AOT for consoles.
* Unit tests for edge-cases (queue removal, pool reuse).
