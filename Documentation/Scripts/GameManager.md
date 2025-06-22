# GameManager

## Purpose
Centralised singleton that controls the **café phase gameplay loop** and handles additive **overlay scene** loading.  
The manager keeps the main Café scene resident at all times and, when the phase changes, it:
1. Fades to black via `UITransitions` (UniTask-based helper).  
2. Unloads any previously loaded overlay scene.  
3. Loads the new overlay scene for the selected phase (additive, async).  
4. Falls back to an in-scene panel if no overlay scene is configured.  
5. Fades back in.

This produces a cleaner hierarchy, lower memory usage, and makes it easy to add new phase-specific UIs or 3-D props.

---

## Context / Usage
* Add the script once to a root `GameObject` in the Café scene and let it persist (`DontDestroyOnLoad`).  
* Other systems should query `GameManager.Instance.CurrentPhase` or subscribe to `OnPhaseChanged`.

---

## Key Methods & Responsibilities
| Method | Responsibility |
| ------ | -------------- |
| `Awake()` | Enforces the singleton pattern and ensures fallback panels start inactive. |
| `Start()` | Sets initial phase to **Morning**. |
| `Update()` | Dev-only keyboard shortcuts (1–4 / numpad 1–4) to change phase. |
| `HandleDebugInput()` | Reads debug keys and calls `SetPhase()`. |
| `SetPhase(CafePhase)` | Changes `CurrentPhase`, fires `OnPhaseChanged`, triggers autosave, and starts overlay loader. |
| `AdvancePhase()` | Cycles **Morning → Rush → Siesta → EveningGlow → Morning**. |
| `UpdatePhaseOverlayAsync()` | Async routine that fades, unloads old overlay, loads new overlay, and fades back. |
| `TriggerAutosave()` | Placeholder hook for future save system. |

---

## Events
| Event | Description |
| ----- | ----------- |
| `Action<CafePhase> OnPhaseChanged` | Fired every time the café phase changes. |

---

## Serialized Fields / Dependencies
| Field | Type | Notes |
| ----- | ---- | ----- |
| `phaseOverlays` | `List<PhaseOverlay>` | Inspector-configurable mapping from phase → overlay scene name → optional fallback panel. |

**Dependencies**
* [UniTask](https://github.com/Cysharp/UniTask) for async/await in Unity.  
* `UITransitions` static helper for fade-in/out.  
* `UnityEngine.SceneManagement.SceneManager` for additive scene loading.

---

## PhaseOverlay Structure
| Field | Type | Meaning |
| ----- | ---- | ------- |
| `phase` | `CafePhase` | The phase this entry represents. |
| `overlaySceneName` | `string` | Name of the scene asset to load additively. Leave empty to skip loading. |
| `fallbackPanel` | `GameObject` | (Optional) In-scene panel to show instantly while the overlay scene loads. |

---

## Scene / Prefab Binding
1. Create overlay scenes (e.g. `Morning_UI`, `Rush_UI`, …) containing only UI/props for that phase and add them to **Build Settings**.  
2. In the inspector on the `GameManager` object:  
   * Expand **Phase Overlays** → set *Size* to the number of phases.  
   * For each element choose the `phase`, type the `overlaySceneName`, and optionally assign a `fallbackPanel`.  
3. Ensure overlay scenes do **not** contain an extra `AudioListener` to avoid Unity warnings.

---
*Last updated: 2025-06-21*


<!-- LEGACY SECTION: Deprecated pre-refactor description. TODO: Remove this section once issue #<TBD> is resolved. -->
## Legacy Panel-Based Implementation (deprecated)
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
Not applicable – this class does not perform any web requests or external API calls.

## Scene or Prefab Binding
* Add the script to a dedicated `GameObject` in your bootstrap scene.  
* Make the object a **prefab** if you want to reuse it across multiple scenes; ensure **`DontDestroyOnLoad`** remains in `Awake()`.  
* Drag the four phase UI canvases into their respective serialized fields, **or** assign matching tags so `InitializePhaseUI()` can locate them at runtime.

---
*Last updated: 2025-06-21*
