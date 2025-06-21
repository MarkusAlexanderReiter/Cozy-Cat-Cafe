# GameManager

## Purpose
Centralized singleton that manages the overall **café phase** gameplay loop. It drives transitions between the four defined `CafePhase` states (Morning, Rush, Siesta, EveningGlow) and ensures that the correct UI panel and systems are active for each phase.  
By living across scene loads (`DontDestroyOnLoad`) it provides a single source of truth for the game state that other systems can query or subscribe to.

## Context / Usage
* Added once to a root `GameObject` in the initial bootstrap scene (recommended name: `GameManager`).  
* Marked as persistent so it survives scene changes and remains the authoritative state holder.  
* Other scripts should **read** `GameManager.Instance.CurrentPhase` or subscribe to `OnPhaseChanged` when they need to react to phase switches.

## Key Methods & Responsibilities
| Method | Responsibility |
| ------ | -------------- |
| `Awake()` | Enforces the persistent‐singleton pattern, caches itself and initialises UI references. |
| `Start()` | Sets the initial phase to **Morning** after the scene loads. |
| `Update()` | For development only – polls keyboard shortcuts (1-4 / numpad 1-4) via the Input System to change phases. |
| `HandleDebugInput()` | Reads debug keys and calls `SetPhase()` accordingly. |
| `SetPhase(CafePhase)` | Safely changes `CurrentPhase`, raises `OnPhaseChanged` and triggers an autosave. |
| `AdvancePhase()` | Convenience helper that cycles **Morning → Rush → Siesta → EveningGlow → Morning**. |
| `InitializePhaseUI()` | Resolves serialized UI references or finds them via fallback tags, then disables all panels. |
| `UpdatePhaseUI()` | Enables the UI panel relevant to the current phase and disables the rest. |
| `TriggerAutosave()` | Placeholder hook for the upcoming save-system; currently logs to the console. |

## Events / Delegates
| Event | Description |
| ----- | ----------- |
| `Action<CafePhase> OnPhaseChanged` | Fired every time `CurrentPhase` changes. Subscribe to react when the game enters a new café phase. |

## Serialized Fields / Dependencies
| Field | Type | Notes |
| ----- | ---- | ----- |
| `morningUI`, `rushUI`, `siestaUI`, `eveningGlowUI` | `GameObject` | Direct references to phase-specific UI canvases/panels. |
| `morningUITag`, `rushUITag`, `siestaUITag`, `eveningGlowUITag` | `string` | Fallback Unity tags to locate the UI objects at runtime if the direct reference is left empty. |

**Dependencies:** The script uses the **Unity Input System** (`UnityEngine.InputSystem`) to read debug keys.

## ScriptableObject References
None – the manager currently stores all state in memory and does not reference any ScriptableObjects.

## API / Networking
Not applicable – this class does not perform any API calls.

## Scene or Prefab Binding
* Add the script to a dedicated `GameObject` in your bootstrap scene.  
* Make the object a **prefab** if you want to reuse it across multiple scenes; ensure **`DontDestroyOnLoad`** remains in `Awake()`.  
* Drag the four phase UI canvases into their respective serialized fields, **or** assign matching tags so `InitializePhaseUI()` can locate them at runtime.

---
*Last updated: 2025-06-21*
