using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D _rb;
    
    [Header("Movement Variables")]
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _moveSpeedMultiplier = 1f;
    private float _currentSpeed;
    
    [Header("Dash")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.6f;
    
    private Vector2 _lastDirection = Vector2.down;
    private bool _dashQueued;
    private bool _isDashing;
    private float _dashTimer;
    private float _cooldownTimer;
    private Vector2 _dashDirection;

    public bool IsDashing => _isDashing;
    
    //Input Variables
    private bool _isTakingInput = true;
    private InputAction _moveAction;
    private Vector2 _moveDir;
    private InputAction _dashAction;

    private void Awake()
    {
        _moveAction = InputSystem.actions.FindAction("Move");
        _dashAction = InputSystem.actions.FindAction("Dash");

        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0;
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        if (_isTakingInput)
            _moveDir = _moveAction.ReadValue<Vector2>();

        _currentSpeed = _moveSpeed * _moveSpeedMultiplier;

        if (_isTakingInput && _dashAction.WasPressedThisFrame())
            _dashQueued = true;

        _cooldownTimer -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (_isDashing)
        {
            HandleDash();
            return;
        }   
        
        if (_dashQueued && _cooldownTimer <= 0f)
            StartDash(_moveDir != Vector2.zero ? _moveDir.normalized : _lastDirection);
        _dashQueued = false;
        
        _rb.linearVelocity = new Vector2(_moveDir.normalized.x * _moveSpeed, _moveDir.normalized.y * _currentSpeed);
    }
    
    private void StartDash(Vector2 direction)
    {
        _isDashing = true;
        _dashTimer = dashDuration;
        _dashDirection = direction;
        _cooldownTimer = dashCooldown;
    }
    
    private void HandleDash()
    {
        _rb.linearVelocity = _dashDirection * dashSpeed;
        _dashTimer -= Time.fixedDeltaTime;

        if (_dashTimer <= 0f)
            _isDashing = false;
    }

}
