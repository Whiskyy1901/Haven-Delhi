using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public float _speed = 5f;
    public int _damage = 10;

    public bool contactDamage = true;
    public float timeBetweenAttacks = 1f;

    [Header("Sound")]
    private AudioSource _source;
    [SerializeField] private AudioClip hitSFX;
    
    protected GameObject _player;
    protected Rigidbody2D _rb;
    private SpriteRenderer _sprite;
    private Color _basecolor = Color.red;
    private float _attackTimer;

    public virtual void Initialization(GameObject player)
    {
        _player = player;
    }

    protected virtual void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sprite = GetComponentInChildren<SpriteRenderer>();
        if(_sprite != null) _basecolor = _sprite.color;
        _attackTimer = timeBetweenAttacks; //to hit immediately on first contact

    } 
    
    void Start()
    {
        _source = GetComponent<AudioSource>();
        if(_player == null )
        {
            _player = GameObject.FindGameObjectWithTag("Player");
            if(_player == null)
                Debug.Log("No player tagged in scene");
        }
    }

    void Update()
    {
        _attackTimer += Time.deltaTime;
    }
    
    void FixedUpdate()
    {
        if(_player == null )
        {
            _rb.linearVelocity = Vector2.zero;
        }

        Behave();
    }
    protected abstract void Behave();
    
    protected virtual void OnContactPlayer() {}

    protected Vector2 DirectionToPlayer => 
            ((Vector2)_player.transform.position - (Vector2)transform.position);

    protected float DistanceToPlayer =>
            Vector2.Distance(_player.transform.position, transform.position);

    //protected void DamagePlayer()
    
    protected void Tint(Color color)
    {
        if(_sprite != null) _sprite.color = color;
    }

    protected void ResetTint()
    {
        if(_sprite != null) _sprite.color = _basecolor;
    }

    protected void Stop() => _rb.linearVelocity = Vector2.zero;

    private void OnTriggerStay2D(Collider2D other)
    {
        if(!contactDamage || _attackTimer < timeBetweenAttacks) return;

        if(!other.CompareTag("Player")) return;

        if(other.transform.root.TryGetComponent<Health>(out Health health))
        {
            health.Damage(_damage);
            _attackTimer = 0;
            OnContactPlayer();
        }
    }
}
