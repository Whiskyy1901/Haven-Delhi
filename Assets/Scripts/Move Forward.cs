using UnityEngine;

public class MoveForward : MonoBehaviour
{
    [SerializeField] private LayerMask _bounceLayers;
    [SerializeField] private float _radius = 0.25f;
    [SerializeField] private float _bounceCooldown = 0.1f;

    private float _speed;
    private float _nextBounceTime;

    public void Initialization(float speed)
    {
        _speed = speed;
    }

    private void FixedUpdate()
    {
        Vector2 direction = transform.right;
        float distance = _speed * Time.fixedDeltaTime;

        if (Time.time < _nextBounceTime)
        {
            transform.position += (Vector3)(direction * distance);
            return;
        }

        RaycastHit2D hit = Physics2D.CircleCast(
            transform.position, _radius, direction, distance, _bounceLayers);

        if (hit.collider == null || Vector2.Dot(direction, hit.normal) >= 0f)
        {
            transform.position += (Vector3)(direction * distance);
            return;
        }

        transform.position = hit.centroid + hit.normal * 0.01f;
        transform.right = Vector2.Reflect(direction, hit.normal);

        _nextBounceTime = Time.time + _bounceCooldown;
    }
}