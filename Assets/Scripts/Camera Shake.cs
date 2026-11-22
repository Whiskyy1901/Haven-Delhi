using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Follow")]
    [SerializeField] private Transform _target;
    [SerializeField] private Vector2 _followOffset = Vector2.zero;
    [SerializeField] private float _followSpeed = 6f;
    [SerializeField] private bool _snapToTargetOnStart = true;

    [Header("Shake")]
    [SerializeField] private float _maxShake = 0.6f;
    [SerializeField] private float _shakeDecay = 8f;
    [SerializeField] private float _shakeFrequency = 35f;

    [Header("Zoom Pop")]
    [SerializeField] private float _maxZoomKick = 0.5f;
    [SerializeField] private float _zoomAttack = 25f;
    [SerializeField] private float _zoomRecover = 6f;

    private Camera _camera;
    private float _baseSize;
    private float _zoomTarget;
    private float _zoomCurrent;

    private Vector3 _followPosition;
    private float _currentShake;
    private float _noiseSeedX;
    private float _noiseSeedY;

    private void Awake()
    {
        Instance = this;
        _followPosition = transform.position;
        _noiseSeedX = Random.Range(0f, 100f);
        _noiseSeedY = Random.Range(100f, 200f);
        if (_camera == null) _camera = GetComponent<Camera>();
        _baseSize = _camera.orthographicSize;
    }

    private void Start()
    {
        if (_snapToTargetOnStart && _target != null)
            _followPosition = GetDesiredPosition();
    }

    public static void Shake(float amount)
    {
        Instance._currentShake = Mathf.Min(Instance._maxShake, Instance._currentShake + amount);
    }

    public static void ZoomPop(float amount)
    {
        Instance._zoomTarget = Mathf.Clamp(Instance._zoomTarget + amount, -Instance._maxZoomKick, Instance._maxZoomKick);
    }

    private Vector3 GetDesiredPosition()
    {
        return new Vector3(
            _target.position.x + _followOffset.x,
            _target.position.y + _followOffset.y,
            _followPosition.z);
    }

    private void UpdateFollow()
    {
        _followPosition = Vector3.Lerp(_followPosition, GetDesiredPosition(), _followSpeed * Time.deltaTime);
    }

    private void UpdateZoom()
    {
        float dt = Time.deltaTime;
        _zoomCurrent = Mathf.Lerp(_zoomCurrent, _zoomTarget, 1f - Mathf.Exp(-_zoomAttack * dt));
        _zoomTarget *= Mathf.Exp(-_zoomRecover * dt);
        _camera.orthographicSize = _baseSize * (1f + _zoomCurrent);
    }

    private void LateUpdate()
    {
        UpdateFollow();
        UpdateZoom();

        if (_currentShake <= 0.0001f)
        {
            _currentShake = 0f;
            transform.position = _followPosition;
            return;
        }

        float t = Time.time * _shakeFrequency;
        float x = Mathf.PerlinNoise(t, _noiseSeedX) * 2f - 1f;
        float y = Mathf.PerlinNoise(t, _noiseSeedY) * 2f - 1f;
        transform.position = _followPosition + new Vector3(x, y, 0f) * _currentShake;

        _currentShake *= Mathf.Exp(-_shakeDecay * Time.deltaTime);
    }
}