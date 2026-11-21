using System;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class Health : MonoBehaviour
{
   private AudioSource _source;
   
   [Header("Health Components")]
   [SerializeField] private float _maxHealth = 10f;
   private float _currentHealth;
   [SerializeField] private ParticleSystem _damageParticles;
   [SerializeField] private AudioClip _damageSound;
   [SerializeField] private ParticleSystem _healParticles;
   [SerializeField] private AudioClip _healSound;
   
   [Header("Death Sequence")]
   [SerializeField] private ParticleSystem _deathParticles;
   [SerializeField] private AudioClip _deathSound;

   private void Start()
   {
      _currentHealth = _maxHealth;
      _source = GetComponent<AudioSource>();
   }

   public void Heal(int HealAmount)
   {
      if (_isDead) return;

      PlayParticlesAndAudio(_source, _healSound, _healParticles);
      
      if (_currentHealth + HealAmount >= _maxHealth)
         _currentHealth = _maxHealth;
      else
         _currentHealth += HealAmount;
   }

   private bool _isDead;

   public void Damage(int Damage)
   {
      if (_isDead) return;

      if (_currentHealth - Damage <= 0)
         Death();
      else
      {
         _currentHealth -= Damage;
         PlayParticlesAndAudio(_source, _damageSound, _damageParticles);
      }
   }

   private void Death()
   {
      _isDead = true;
      PlayParticlesAndAudio(_source, _deathSound, _deathParticles);
   }

   private void Update()
   {
      if (_isDead && !_source.isPlaying && !_deathParticles.IsAlive(true))
         Destroy(gameObject);
   }

   private void PlayParticlesAndAudio(AudioSource source, AudioClip audioClip, ParticleSystem particles)
   {
      if(audioClip == null && particles == null) return;
      
      source.PlayOneShot(audioClip);
      particles.Play();
   }
}
