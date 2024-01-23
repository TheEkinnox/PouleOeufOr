using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    public float rotationSpeed = 180.0f;
    public float acceleration = 2.0f;
    public float carryingSpeedMultiplier = .75f;
    public float chargeSpeed = 2.0f;
    public float maxDashDistance = 10.0f;
    public float distancePerTap = 1.0f;
    public float stunDuration = 2.0f;
    public float dropDistance = -1.0f;
    public float dashCollisionRadius = 1.0f;
    public int raycastHitsCount = 5;

    private Rigidbody2D _rb;
    private Transform _transform;
    private float _currentSpeed;
    private float _currentCharge;
    private Vector2 _rotationInput;
    private float _distanceTraveled;
    private GameObject _carriedObject;
    private bool _isStunned;
    private bool _isCharging;
    private float _stunEndTime;
    private RaycastHit2D[] _hits;
    private float _dashCooldown = -1.0f;


    private bool CanMove
    {
        get => !_isStunned && !_isCharging && Mathf.Approximately(_currentCharge, 0f);
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _transform = transform;
        _hits = new RaycastHit2D[raycastHitsCount];
    }

    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        // Utilisez rotationInput pour effectuer la rotation du personnage ici
        _rotationInput = context.canceled ? Vector2.zero : context.ReadValue<Vector2>();
    }

    private void OnAcceleratePerformed(InputAction.CallbackContext context)
    {
        if (context.canceled || !CanMove || _distanceTraveled >= distancePerTap)
            return;

        _currentSpeed = acceleration;

        if (_carriedObject)
            _currentSpeed *= carryingSpeedMultiplier;
    }

    private void OnDashPerformed(InputAction.CallbackContext context)
    {
        _isCharging = !context.canceled;
    }

    private void Update()
    {
        if (_dashCooldown > 0)
        {
            _dashCooldown -= Time.deltaTime;

            if (_dashCooldown <= 0)
            {
                _dashCooldown = -1.0f;
            }
        }

        if (_isCharging)
            _currentCharge += chargeSpeed * Time.deltaTime;

        if (!_isStunned)
        {
            // Appliquez la rotation en fonction de rotationInput
            _transform.Rotate(new Vector3(0, 0, _rotationInput.x * rotationSpeed * Time.deltaTime));

            TryDash();
            TryMove();
        }
        else if (Time.time >= _stunEndTime)
        {
            _isStunned = false;
        }
    }

    private void TryDash()
    {
        if (_isStunned || _isCharging || Mathf.Approximately(_currentCharge, 0) || _dashCooldown > 0)
            return;

        _currentCharge = Mathf.Min(_currentCharge, maxDashDistance);
        Vector2 startPos = _rb.position;
        Vector2 dir = _transform.up;
        Vector2 translation = dir * _currentCharge;
        _rb.MovePosition(startPos + translation);

        int hitCount = Physics2D.CircleCastNonAlloc(startPos, dashCollisionRadius, dir, _hits, _currentCharge);

        for (int i = 0; i < hitCount; i++)
        {
            RaycastHit2D hit = _hits[i];

            if (hit.transform == _transform)
                continue;

            PlayerMovement player = hit.transform.GetComponent<PlayerMovement>();

            if (player)
                player.Stun();
        }

        _currentCharge = 0;
        _dashCooldown = 5.0f; // Temps de recharge en secondes
    }

    private void TryMove()
    {
        if (_currentSpeed <= 0)
            return;

        float distanceThisFrame = _currentSpeed * Time.deltaTime;
        _distanceTraveled += distanceThisFrame;

        if (_distanceTraveled >= distancePerTap)
        {
            _currentSpeed = 0f;
            _distanceTraveled = 0f;
        }

        // Appliquez la vitesse au mouvement du personnage
        Vector2 movement = _transform.up * distanceThisFrame;
        _rb.MovePosition(_rb.position + movement);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isStunned && other.gameObject.CompareTag("Collectible"))
        {
            _carriedObject = other.gameObject;
            _carriedObject.SetActive(false);
        }
        else if (_carriedObject != null && other.gameObject.CompareTag("Player"))
        {
            Stun();
        }
    }

    private void Stun()
    {
        _isStunned = true;
        _stunEndTime = Time.time + stunDuration;
        _isCharging = false;
        _currentCharge = 0;

        DropObject();
    }

    private void DropObject()
    {
        if (!_carriedObject)
            return;

        _carriedObject.SetActive(true);
        _carriedObject.transform.position = _transform.position + _transform.up * dropDistance;
        _carriedObject = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Color color = Gizmos.color;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashCollisionRadius);
        Gizmos.color = color;
    }
#endif
}
