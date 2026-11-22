using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Volume))]
public class DamageVignette : MonoBehaviour
{
    public static DamageVignette Instance { get; private set; }

    [SerializeField] private Color _hitColor = new Color(0.8f, 0f, 0f, 1f);
    [SerializeField] private float _maxPulse = 0.6f;
    [SerializeField] private float _decayPerSecond = 1.5f;

    private Vignette _vignette;
    private Color _baseColor;
    private float _baseIntensity;
    private float _pulse;

    private void Awake()
    {
        Instance = this;

        var volume = GetComponent<Volume>();

        if (!volume.profile.TryGet(out _vignette))
        {
            return;
        }

        _baseColor = _vignette.color.value;
        _baseIntensity = _vignette.intensity.value;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void Pulse(float strength)
    {
        Instance._pulse = Mathf.Min(Instance._maxPulse, Instance._pulse + strength);
    }

    private void Update()
    {
        if (_pulse <= 0f) return;

        _pulse = Mathf.MoveTowards(_pulse, 0f, _decayPerSecond * Time.deltaTime);

        float t = _pulse / _maxPulse;
        _vignette.intensity.Override(_baseIntensity + _pulse);
        _vignette.color.Override(Color.Lerp(_baseColor, _hitColor, t));
    }
}