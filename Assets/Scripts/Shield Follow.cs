using UnityEngine;
using UnityEngine.InputSystem;

public class ShieldFollow : MonoBehaviour
{
    [Header("Mode")]
    [SerializeField] private bool _orbitPlayer = true;

    [Header("Orbit Settings")]
    [SerializeField] private Transform _orbitCenter;
    [SerializeField] private float _orbitRadius = 0.8f;
    [SerializeField] private bool _flipWhenAimingLeft = true;

    private Camera _cam;

    private void Awake()
    {
        _cam = Camera.main;
        if (_orbitCenter == null && transform.parent != null)
            _orbitCenter = transform.parent;
    }

    private void Update()
    {

        if (Mouse.current == null) return;

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
