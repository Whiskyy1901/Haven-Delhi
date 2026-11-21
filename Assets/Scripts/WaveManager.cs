using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class EnemyEntry
{
    public GameObject obj;
    [Min(1)] public int value = 1;
}

public class WaveManager : MonoBehaviour
{
    [SerializeField] private GameObject _player;

    [SerializeField] private List<EnemyEntry> _enemies = new List<EnemyEntry>();

    [Header("Wave budget")]
    [SerializeField] private int _baseWaveValue = 25;
    [SerializeField] private int _waveValueIncrease = 100;
    [SerializeField] private float _waveCooldown = 5f;

    [Header("Spawning over time")]
    [SerializeField] private float _waveSpawnDuration = 12f;
    [SerializeField] private float _maxSpawnGap = 1.5f;
    [SerializeField] private float _minSpawnGap = 0.35f;
    [SerializeField] private int _maxAliveEnemies = 40;

    [Header("Spawn ring around the player")]
    [SerializeField] private float _minSpawnDistance = 8f;
    [SerializeField] private float _maxSpawnDistance = 12f;
    
    [Header("Spawn placement (predicting the player's path)")]
    [SerializeField, Range(0f, 1f)] private float _predictedChance = 0.8f;
    [SerializeField, Range(0f, 180f)] private float _coneAngle = 35f;
    [SerializeField] private float _headingSmoothing = 6f;
    [SerializeField] private float _minHeadingSpeed = 0.5f;

    [Header("Spawn pop")]
    [SerializeField] private float _popDuration = 0.3f;

    // SFX
    [Header("Sounds")]
    [SerializeField] private AudioClip _waveStartSound;
    [SerializeField] private AudioClip _waveClearedSound;
    private AudioSource _source;

    private readonly HashSet<GameObject> _waveSpawns = new HashSet<GameObject>();
    private readonly List<EnemyEntry> _affordable = new List<EnemyEntry>();
    private readonly List<GameObject> _spawnQueue = new List<GameObject>();

    private float _currentCooldown;
    private int _waveNumber;
    private bool _isSpawning;

    private Vector2 _lastPlayerPos;
    private Vector2 _smoothedVelocity;
    private Vector2 _heading;
    private bool _hasHeading;

    public int WaveNumber => _waveNumber;

    public event Action<GameObject> OnEnemyDefeated;
    public event Action<int> OnWaveCleared;
    public event Action<GameObject> OnBossDefeated;

    private GameObject _currentBoss;

    private void Start()
    {
        if (_player != null)
            _lastPlayerPos = _player.transform.position;
        _source = GetComponent<AudioSource>();
    }

    private void Update()
    {
        TrackPlayerHeading();

        if (_isSpawning || _waveSpawns.Count > 0) return;

        _currentCooldown += Time.deltaTime;
        if (_currentCooldown < _waveCooldown) return;

        _currentCooldown = 0f;
        _waveNumber++;
        PlayParticlesAndAudio(_source, _waveStartSound);

        StartCoroutine(SpawnWaveRoutine(_baseWaveValue + (_waveNumber - 1) * _waveValueIncrease));
    }

    private void TrackPlayerHeading()
    {
        if (_player == null || Time.deltaTime <= 0f) return;

        Vector2 position = _player.transform.position;
        Vector2 velocity = (position - _lastPlayerPos) / Time.deltaTime;
        _lastPlayerPos = position;

        float blend = 1f - Mathf.Exp(-_headingSmoothing * Time.deltaTime);
        _smoothedVelocity = Vector2.Lerp(_smoothedVelocity, velocity, blend);

        if (_smoothedVelocity.magnitude > _minHeadingSpeed)
        {
            _heading = _smoothedVelocity.normalized;
            _hasHeading = true;
        }
    }

    private IEnumerator SpawnWaveRoutine(int waveValue)
    {
        _isSpawning = true;
        BuildWaveQueue(waveValue);

        int count = _spawnQueue.Count;
        float averageGap = count > 1
            ? Mathf.Clamp(_waveSpawnDuration / count, _minSpawnGap, Mathf.Max(_minSpawnGap, _maxSpawnGap))
            : 0f;

        for (int i = 0; i < count; i++)
        {
            while (_maxAliveEnemies > 0 && _waveSpawns.Count >= _maxAliveEnemies && _player != null)
                yield return null;

            if (_player == null) break;

            if (TryGetSpawnPosition(out Vector2 spawnPosition))
                Spawn(_spawnQueue[i], spawnPosition);

            if (i < count - 1)
                yield return new WaitForSeconds(averageGap * Random.Range(0.5f, 1.5f));
        }

        _spawnQueue.Clear();
        _isSpawning = false;
    }

    private void BuildWaveQueue(int waveValue)
    {
        _spawnQueue.Clear();
        int budget = waveValue;

        while (true)
        {
            _affordable.Clear();
            foreach (var entry in _enemies)
            {
                if (entry.obj != null && entry.value > 0 && entry.value <= budget)
                    _affordable.Add(entry);
            }

            if (_affordable.Count == 0) break;

            EnemyEntry pick = _affordable[Random.Range(0, _affordable.Count)];
            budget -= pick.value;
            _spawnQueue.Add(pick.obj);
        }
    }

    public GameObject Spawn(GameObject enemy, Vector2 position)
    {
        GameObject obj = Instantiate(enemy, position, Quaternion.identity);

        if (obj.TryGetComponent(out Enemy enemyScript))
            enemyScript.Initialization(_player);

        if (!obj.TryGetComponent(out DestroyTracker tracker))
            tracker = obj.AddComponent<DestroyTracker>();
        tracker.manager = this;

        if (_popDuration > 0f)
        {
            if (!obj.TryGetComponent(out SpawnPop pop))
                pop = obj.AddComponent<SpawnPop>();
            pop.Play(_popDuration);
        }

        _waveSpawns.Add(obj);
        return obj;
    }

    public void NotifyDestroyed(GameObject obj)
    {
        bool removed = _waveSpawns.Remove(obj);
        OnEnemyDefeated?.Invoke(obj);

        if (_currentBoss != null && obj == _currentBoss)
        {
            _currentBoss = null;
            OnBossDefeated?.Invoke(obj);
        }

        if (removed && _waveSpawns.Count == 0 && !_isSpawning && gameObject.scene.isLoaded)
        {
            PlayParticlesAndAudio(_source, _waveClearedSound);
            OnWaveCleared?.Invoke(_waveNumber);
        }
    }

    public Vector2 RandomPosition() => RandomPosition(_minSpawnDistance, _maxSpawnDistance);

    private Vector2 RandomPosition(float minDistance, float maxDistance)
    {
        Vector2 center = _player != null ? (Vector2)_player.transform.position : (Vector2)transform.position;
        Vector2 direction;

        if (_hasHeading && Random.value <= _predictedChance)
        {
            float angle = Random.Range(-_coneAngle, _coneAngle);
            direction = (Vector2)(Quaternion.Euler(0f, 0f, angle) * (Vector3)_heading);
        }
        else
        {
            direction = Random.insideUnitCircle.normalized;
            if (direction == Vector2.zero) direction = Vector2.right;
        }

        float distance = Random.Range(minDistance, maxDistance);
        return center + direction * distance;
    }
    
    private bool TryGetSpawnPosition(out Vector2 position)
    {
        position = RandomPosition();
        return true;
    }
    
    private void PlayParticlesAndAudio(AudioSource source, AudioClip audioClip, ParticleSystem particles = null)
    {
        if(audioClip == null && particles == null) return;
      
        source.PlayOneShot(audioClip);
        particles.Play();
    }
}