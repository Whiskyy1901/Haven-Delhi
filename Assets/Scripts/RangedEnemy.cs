using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class RangedEnemy : Enemy
{
    [Header("Ranged")]
    [SerializeField] private float _shootRange = 7f;
    [SerializeField] private float _fireInterval = 1f;
    [SerializeField] private Transform _firePos;

    [Header("Positioning")]
    [SerializeField] private float _preferredDistance = 5.5f;
    [SerializeField] private float _distanceTolerance = 2f;
    [SerializeField, Range(0f, 1f)] private float _strafeAmount = 0.8f;
    [SerializeField] private Vector2 _strafeSwitchTime = new Vector2(1f, 2.5f);
    [SerializeField] private float _acceleration = 20f;

    [Header("Projectile")]
    [SerializeField] private GameObject _bullet;
    [SerializeField] private float _bulletSpeed = 8f;
    [SerializeField] private float _spread = 3f;

    [Header("Burst")]
    [SerializeField] private int _burstCount = 1;
    [SerializeField] private float _burstInterval = 0.1f;

    [Header("Sound")]
    private AudioSource _audioSource;
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _shootSound;

    [SerializeField] private ParticleSystem _shootParticle;

    private float _fireTimer;
    private bool _isBursting;
    private float _strafeDirection;
    private float _strafeTimer;

    private void OnValidate()
    {
        _preferredDistance = Mathf.Min(_preferredDistance, _shootRange);
    }

    protected override void Awake()
    {
        base.Awake();

        _strafeDirection = Random.value < 0.5f ? -1f : 1f;
        _strafeTimer = Random.Range(_strafeSwitchTime.x, _strafeSwitchTime.y);
    }

    protected override void Behave()
    {
        float distance = DistanceToPlayer;
        Vector2 toPlayer = DirectionToPlayer;

        Move(distance, toPlayer);

        _fireTimer += Time.fixedDeltaTime;
        if (distance <= _shootRange && _fireTimer >= _fireInterval && !_isBursting)
        {
            _fireTimer = 0f;
            StartCoroutine(FireBurst());
        }
    }

    private void Move(float distance, Vector2 toPlayer)
    {
        float tolerance = Mathf.Max(0.01f, _distanceTolerance);
        float offset = distance - _preferredDistance;

        float radial = Mathf.Clamp(offset / tolerance, -1f, 1f);
        float strafeWeight = Mathf.Clamp01(1f - offset / tolerance);

        _strafeTimer -= Time.fixedDeltaTime;
        if (_strafeTimer <= 0f)
        {
            _strafeDirection = -_strafeDirection;
            _strafeTimer = Random.Range(_strafeSwitchTime.x, _strafeSwitchTime.y);
        }

        Vector2 sideways = new Vector2(-toPlayer.y, toPlayer.x) * (_strafeDirection * _strafeAmount * strafeWeight);

        Vector2 move = toPlayer * radial + sideways;
        if (move.sqrMagnitude > 1f) move.Normalize();

        _rb.linearVelocity = Vector2.MoveTowards(_rb.linearVelocity, move * _speed, _acceleration * Time.fixedDeltaTime);
    }

    private IEnumerator FireBurst()
    {
        _isBursting = true;
        for (int i = 0; i < _burstCount; i++)
        {
            if (_player == null) break;
            Shoot();
            yield return new WaitForSeconds(_burstInterval);
        }

        _isBursting = false;
    }

    private void Shoot()
    {
        Vector2 origin = _firePos != null ? (Vector2)_firePos.position : (Vector2)transform.position;
        Vector2 dir = (Vector2)_player.transform.position - origin;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg
                      + Random.Range(-_spread * 0.5f, _spread * 0.5f);

        GameObject bullet = Instantiate(_bullet, origin, Quaternion.Euler(0f, 0f, angle));
        CameraShake.Shake((float) _damage/10);
        PlayParticlesAndAudio(_audioSource, _shootSound, _shootParticle);
        if (bullet.TryGetComponent<Damage>(out var damage))
            damage.Initialization(_damage);

        if (bullet.TryGetComponent<MoveForward>(out var move))
            move.Initialization(_bulletSpeed);
    }
    
    private void PlayParticlesAndAudio(AudioSource source, AudioClip audioClip, ParticleSystem particles)
    {
        if (audioClip == null && particles == null) return;

        source.PlayOneShot(audioClip);
        particles.Play();
    }
}