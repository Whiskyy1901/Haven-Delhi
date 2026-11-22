using System;
using UnityEngine;
using UnityEngine.SceneManagement;

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

   [SerializeField] private float _hitPulse = 2f;
   [SerializeField] private HealthBar _bar;
   
   [Header("Death Sequence")]
   [SerializeField] private ParticleSystem _deathParticles;
   [SerializeField] private AudioClip _deathSound;

   private void Start()
   {
      _currentHealth = _maxHealth;
      _source = GetComponent<AudioSource>();
      if(gameObject.CompareTag("Player"))
         _bar.SetMaxHealth(_maxHealth);
   }

   private bool _isDead;

   public void Damage(int Damage)
   {
      if (_isDead) return;

      if (_currentHealth - Damage <= 0)
      {
         Death();
         CameraShake.Shake(50);
      }
      else
      {
         _currentHealth -= Damage;
         //PlayParticlesAndAudio(_source, _damageSound, _damageParticles);
         CameraShake.Shake(10);
      }

      if (gameObject.CompareTag("Player"))
      {
         DamageVignette.Pulse(_hitPulse);
         _bar.SetHealth(_currentHealth);
      }
   }

   private void Death()
   {
      _isDead = true;
      //PlayParticlesAndAudio(_source, _deathSound, _deathParticles);
      Destroy(this.gameObject);
      if (this.gameObject.CompareTag("Player"))
         SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); 
   }

   private void Update()
   {
      //if (_isDead && !_source.isPlaying && !_deathParticles.IsAlive(true))
      //Destroy(gameObject);  
   }

   private void PlayParticlesAndAudio(AudioSource source, AudioClip audioClip, ParticleSystem particles)
   {
      if(audioClip == null && particles == null) return;
      
      source.PlayOneShot(audioClip);
      particles.Play();
   }
}
