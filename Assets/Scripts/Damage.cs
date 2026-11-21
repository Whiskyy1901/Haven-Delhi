using System;
using System.Collections.Generic;
using UnityEngine;

public class Damage : MonoBehaviour
{
    [SerializeField] private LayerMask _toDamage;
    
    [SerializeField] private bool _destroyOnNonTargets = true;

    // SFX
    [Header("Sounds")]
    [SerializeField] AudioClip _impactAudio;
    [SerializeField] ParticleSystem _impactParticle;

    private AudioSource _source;

    private int _damage;
    private float _speed;
    private int _pierceLeft;
    private bool _finished;

    private readonly HashSet<Health> _alreadyHit = new HashSet<Health>();

    private void Start()
    {
        _source = GetComponent<AudioSource>();
    }

    public void Initialization(int damage, int pierceCount = 0, float bulletSpeed = 0f)
    {
        _damage = damage;
        _pierceLeft = Mathf.Max(0, pierceCount);
        _speed = bulletSpeed;
    }
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_finished) return;
        bool isTarget = (_toDamage.value & (1 << other.gameObject.layer)) != 0;

        if (!isTarget && !_destroyOnNonTargets) return;

        bool damagedSomething = false;

        if (isTarget)
        {
            Health health = other.GetComponentInParent<Health>();
            if (health != null)
            {
                if (_alreadyHit.Contains(health)) return;
                _alreadyHit.Add(health);

                health.Damage(_damage);
                damagedSomething = true;

                if (_pierceLeft > 0)
                {
                    _pierceLeft--;
                    return;
                }
            }
        }

        _finished = true;

        if (!damagedSomething)
            PlayParticlesAndAudio(_source, _impactAudio, _impactParticle);

        Destroy(gameObject);
    }
    
    private void PlayParticlesAndAudio(AudioSource source, AudioClip audioClip, ParticleSystem particles)
    {
        if(audioClip == null && particles == null) return;
      
        source.PlayOneShot(audioClip);
        particles.Play();
    }
}