using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Moves the character in all directions using the Unity Input System and drives idle / walk animations.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class cat : MonoBehaviour
{
    #region Fields

    [Header("Movement")]
    [SerializeField, Min(0f)]
    private float _moveSpeed = 5f;

    [Header("Input")]
    [Tooltip("Input Action Reference with a 2D Vector composite (e.g. WASD / left stick).")]
    [SerializeField]
    private InputActionReference _moveAction;

    [Header("Animation")]
    [Tooltip("Animator with Idle and Walk states driven by the given float parameter.")]
    [SerializeField]
    private Animator _animator;

    [SerializeField]
    [Tooltip("Name of the bool parameter that marks running state.")]
    private string _runParam = "isrunning";

    [Header("Plane")]
    [Tooltip("If true, move on XY plane (top-down 2D). Otherwise XZ (3D).")]
    [SerializeField]
    private bool _useXYPlane = false;

    [Header("Sprite")]
    [SerializeField]
    private SpriteRenderer _spriteRenderer;

    private CharacterController _controller;
    private int _runHash;

    #endregion

    #region Unity Life-cycle

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _runHash = Animator.StringToHash(_runParam);

        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        Debug.Assert(_moveAction != null,
                     $"{nameof(cat)} on {gameObject.name} is missing an {nameof(InputActionReference)}.",
                     this);
        Debug.Assert(_animator != null,
                     $"{nameof(cat)} on {gameObject.name} is missing an {nameof(Animator)}.",
                     this);
    }

    private void OnEnable()
    {
        _moveAction.action.Enable();
    }

    private void OnDisable()
    {
        _moveAction.action.Disable();
    }

    private void Update()
    {
        Vector2 input = _moveAction.action.ReadValue<Vector2>();

        // Convert 2D input (x = horizontal, y = vertical) to 3D world direction (XZ plane).
        Vector3 move = _useXYPlane
            ? new Vector3(input.x, input.y, 0f)  // XY plane (top-down 2D)
            : new Vector3(input.x, 0f, input.y); // XZ plane (default 3D)

        // Clamp diagonal speed.
        if (move.sqrMagnitude > 1f) move.Normalize();

        _controller.Move(move * _moveSpeed * Time.deltaTime);

        // Flip sprite horizontally based on input.x
        if (_spriteRenderer != null)
        {
            if (input.x > 0.01f)
                _spriteRenderer.flipX = false;
            else if (input.x < -0.01f)
                _spriteRenderer.flipX = true;
        }

        // Animation — set bool parameter based on movement.
        if (_animator != null)
        {
            bool isRunning = move.sqrMagnitude > 0.0001f;
            _animator.SetBool(_runHash, isRunning);
        }
    }

    #endregion
}
