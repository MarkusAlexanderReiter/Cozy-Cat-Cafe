using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;

/// <summary>
/// Enum representing the different phases of the cafe gameplay loop
/// </summary>
public enum CafePhase 
{ 
    Morning,    // Morning Prep - Choose menu, dispatch ingredient scouts
    Rush,       // Brunch Rush - Seat cats, play dish mini-games, earn coins/hearts
    Siesta,     // Siesta - Decorate, shop, chat, story events
    EveningGlow // Evening Glow - Night party with special recipes & photos (Periodic)
}

/// <summary>
/// Central game state manager implementing a persistent Singleton pattern.
/// Controls game phase transitions and associated UI panels.
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Singleton
    
    private static GameManager _instance;
    
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("GameManager instance is null! Make sure GameManager is present in the scene.");
            }
            return _instance;
        }
    }
    
    public static bool HasInstance => _instance != null;
    
    #endregion
    
    #region Events
    
    // Event that triggers when the cafe phase changes
    public event Action<CafePhase> OnPhaseChanged;
    
    #endregion
    
    #region Properties
    
    // Cached enum count for performance
    private static readonly int PhaseCount = Enum.GetValues(typeof(CafePhase)).Length;
    
    // Current cafe phase
    private CafePhase _currentPhase = CafePhase.Morning;
    public CafePhase CurrentPhase
    {
        get { return _currentPhase; }
        private set
        {
            if (_currentPhase != value)
            {
                _currentPhase = value;
                OnPhaseChanged?.Invoke(_currentPhase);
                TriggerAutosave();
            }
        }
    }
    
    #endregion
    
    #region UI References
    
    // References to phase-specific UI panels
    [SerializeField] private GameObject morningUI;
    [SerializeField] private GameObject rushUI;
    [SerializeField] private GameObject siestaUI;
    [SerializeField] private GameObject eveningGlowUI;
    
    // UI reference tags for fallback finding
    [Header("UI Fallback Tags")]
    [SerializeField] private string morningUITag = "MorningUI";
    [SerializeField] private string rushUITag = "RushUI";
    [SerializeField] private string siestaUITag = "SiestaUI";
    [SerializeField] private string eveningGlowUITag = "EveningGlowUI";
    
    #endregion
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Ensure singleton instance
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple GameManager instances detected. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize
        InitializePhaseUI();
    }
    
    private void Start()
    {
        // Set initial phase
        SetPhase(CafePhase.Morning);
    }
    
    private void Update()
    {
        // Temporary keyboard controls for phase advancement
        HandleDebugInput();
    }
    
    private void OnDestroy()
    {
        // Clean up singleton reference
        if (_instance == this)
        {
            _instance = null;
        }
    }
    
    #endregion
    
    #region Input Handling
    
    /// <summary>
    /// Handles debug keyboard input for phase switching
    /// </summary>
    private void HandleDebugInput()
    {
        // Use the new Input System for debug key checks
        if ((Keyboard.current.digit1Key != null && Keyboard.current.digit1Key.wasPressedThisFrame) ||
            (Keyboard.current.numpad1Key != null && Keyboard.current.numpad1Key.wasPressedThisFrame))
        {
            SetPhase(CafePhase.Morning);
        }
        else if ((Keyboard.current.digit2Key != null && Keyboard.current.digit2Key.wasPressedThisFrame) ||
                 (Keyboard.current.numpad2Key != null && Keyboard.current.numpad2Key.wasPressedThisFrame))
        {
            SetPhase(CafePhase.Rush);
        }
        else if ((Keyboard.current.digit3Key != null && Keyboard.current.digit3Key.wasPressedThisFrame) ||
                 (Keyboard.current.numpad3Key != null && Keyboard.current.numpad3Key.wasPressedThisFrame))
        {
            SetPhase(CafePhase.Siesta);
        }
        else if ((Keyboard.current.digit4Key != null && Keyboard.current.digit4Key.wasPressedThisFrame) ||
                 (Keyboard.current.numpad4Key != null && Keyboard.current.numpad4Key.wasPressedThisFrame))
        {
            SetPhase(CafePhase.EveningGlow);
        }
    }
    
    #endregion
    
    #region Phase Management
    
    /// <summary>
    /// Changes the current cafe phase and updates UI accordingly
    /// </summary>
    public void SetPhase(CafePhase newPhase)
    {
        // Validate phase
        if (!Enum.IsDefined(typeof(CafePhase), newPhase))
        {
            Debug.LogError($"Invalid phase: {newPhase}");
            return;
        }
        
        Debug.Log($"Changing phase from {CurrentPhase} to {newPhase}");
        CurrentPhase = newPhase;
        UpdatePhaseUI();
    }
    
    /// <summary>
    /// Advances to the next phase in sequence
    /// </summary>
    public void AdvancePhase()
    {
        CafePhase nextPhase = (CafePhase)(((int)CurrentPhase + 1) % PhaseCount);
        SetPhase(nextPhase);
    }
    
    #endregion
    
    #region UI Management
    
    /// <summary>
    /// Initial setup for UI panels - uses tags as fallback for missing references
    /// </summary>
    private void InitializePhaseUI()
    {
        // Use tags as fallback only if references are not assigned in inspector
        if (morningUI == null && !string.IsNullOrEmpty(morningUITag))
        {
            GameObject found = GameObject.FindWithTag(morningUITag);
            if (found != null)
            {
                morningUI = found;
                Debug.Log($"Found MorningUI via tag: {morningUITag}");
            }
            else
            {
                Debug.LogWarning($"MorningUI not found with tag: {morningUITag}");
            }
        }
        
        if (rushUI == null && !string.IsNullOrEmpty(rushUITag))
        {
            GameObject found = GameObject.FindWithTag(rushUITag);
            if (found != null)
            {
                rushUI = found;
                Debug.Log($"Found RushUI via tag: {rushUITag}");
            }
            else
            {
                Debug.LogWarning($"RushUI not found with tag: {rushUITag}");
            }
        }
        
        if (siestaUI == null && !string.IsNullOrEmpty(siestaUITag))
        {
            GameObject found = GameObject.FindWithTag(siestaUITag);
            if (found != null)
            {
                siestaUI = found;
                Debug.Log($"Found SiestaUI via tag: {siestaUITag}");
            }
            else
            {
                Debug.LogWarning($"SiestaUI not found with tag: {siestaUITag}");
            }
        }
        
        if (eveningGlowUI == null && !string.IsNullOrEmpty(eveningGlowUITag))
        {
            GameObject found = GameObject.FindWithTag(eveningGlowUITag);
            if (found != null)
            {
                eveningGlowUI = found;
                Debug.Log($"Found EveningGlowUI via tag: {eveningGlowUITag}");
            }
            else
            {
                Debug.LogWarning($"EveningGlowUI not found with tag: {eveningGlowUITag}");
            }
        }
        
        // Make sure all UIs are initially disabled
        DisableAllUI();
    }
    
    /// <summary>
    /// Disables all UI panels
    /// </summary>
    private void DisableAllUI()
    {
        morningUI?.SetActive(false);
        rushUI?.SetActive(false);
        siestaUI?.SetActive(false);
        eveningGlowUI?.SetActive(false);
    }
    
    /// <summary>
    /// Updates the active UI panel based on current phase
    /// </summary>
    private void UpdatePhaseUI()
    {
        // Disable all UI panels first
        DisableAllUI();
        
        // Enable appropriate UI panel for current phase
        switch (CurrentPhase)
        {
            case CafePhase.Morning:
                if (morningUI != null)
                {
                    morningUI.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("MorningUI reference is null!");
                }
                break;
                
            case CafePhase.Rush:
                if (rushUI != null)
                {
                    rushUI.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("RushUI reference is null!");
                }
                break;
                
            case CafePhase.Siesta:
                if (siestaUI != null)
                {
                    siestaUI.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("SiestaUI reference is null!");
                }
                break;
                
            case CafePhase.EveningGlow:
                if (eveningGlowUI != null)
                {
                    eveningGlowUI.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("EveningGlowUI reference is null!");
                }
                break;
                
            default:
                Debug.LogError($"Unhandled phase in UpdatePhaseUI: {CurrentPhase}");
                break;
        }
    }
    
    #endregion
    
    #region Save System
    
    /// <summary>
    /// Triggers autosave functionality
    /// </summary>
    private void TriggerAutosave()
    {
        // TODO: Implement saving game state when save system is created
        Debug.Log($"Autosave triggered after phase change to {CurrentPhase}");
    }
    
    #endregion
}
