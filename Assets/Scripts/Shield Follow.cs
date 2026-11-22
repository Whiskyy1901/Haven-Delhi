using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ShieldFollow : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private bool _orbitPlayer = true;

    [Header("Orbit Settings")]
    [SerializeField] private Transform _orbitCenter;
    [SerializeField] private float _orbitRadius = 0.8f;
    [SerializeField] private bool _flipWhenAimingLeft = true;

    [Header("Shield")]
    [SerializeField] private float _maxStamina = 3f;
    [SerializeField] private float _regenRate = 1f;
    [SerializeField] private BoxCollider2D _collider2D;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("UI")]
    [SerializeField] private Image[] _staminaSegments;
    [SerializeField] private Color _normalColor = Color.white;
    [SerializeField] private Color _exhaustedColor = Color.red;

    private float _currentStamina;
    private bool _exhausted;
    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
        if (_orbitCenter == null && transform.parent != null)
            _orbitCenter = transform.parent;

        _currentStamina = _maxStamina;
        UpdateStaminaBar();
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        bool wantsAttack = Mouse.current.leftButton.isPressed && !_exhausted;

        UpdateStamina(wantsAttack);
        _collider2D.enabled = wantsAttack;
        _spriteRenderer.enabled = wantsAttack;

        Aim();
    }

    private void UpdateStamina(bool wantsAttack)
    {
        if (wantsAttack)
        {
            _currentStamina -= Time.deltaTime;
            if (_currentStamina <= 0f)
            {
                _currentStamina = 0f;
                _exhausted = true;
            }
        }
        else
        {
            _currentStamina = Mathf.Min(_currentStamina + Time.deltaTime * _regenRate, _maxStamina);
            if (_exhausted && _currentStamina >= _maxStamina)
                _exhausted = false;
        }

        UpdateStaminaBar();
    }

    private void UpdateStaminaBar()
    {
        if (_staminaSegments == null || _staminaSegments.Length == 0) return;

        float t = _maxStamina > 0f ? _currentStamina / _maxStamina : 0f;
        int active = Mathf.Clamp(Mathf.CeilToInt(t * _staminaSegments.Length), 0, _staminaSegments.Length);
        Color color = _exhausted ? _exhaustedColor : _normalColor;

        for (int i = 0; i < _staminaSegments.Length; i++)
        {
            if (_staminaSegments[i] == null) continue;
            _staminaSegments[i].enabled = i < active;
            _staminaSegments[i].color = color;
        }
    }

    private void Aim()
    {
        Vector2 mouseWorldPos = _cam.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        bool orbiting = _orbitPlayer && _orbitCenter != null;
        Vector2 origin = orbiting ? _orbitCenter.position : transform.position;
        Vector2 direction = mouseWorldPos - origin;

        if (direction.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (orbiting)
            transform.position = origin + direction.normalized * _orbitRadius;

        if (_flipWhenAimingLeft)
        {
            Vector3 scale = transform.localScale;
            scale.y = Mathf.Abs(scale.y) * (direction.x < 0f ? -1f : 1f);
            transform.localScale = scale;
        }
    }
}