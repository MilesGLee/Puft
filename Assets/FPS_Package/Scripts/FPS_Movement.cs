using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FPS_Movement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _playerCamera;
    [SerializeField] private Transform _playerOrientation;
    private Rigidbody _rigidbody;
    [Header("Movement Variables")]
    [SerializeField] private float _moveSpeed = 4500;
    [SerializeField] private float _maxSpeed = 20;
    private bool _grounded;
    [SerializeField] private LayerMask _ground;
    [SerializeField] private float _counterMovement = 0.175f;
    private float _threshold = 0.01f;
    [SerializeField] private float _maxSlopeAngle = 35f;
    [Header("Rotation Variables")]
    private float _xRotation;
    private float _sensitivity = 50f;
    private float _sensitivityMultiplier = 1f;
    [Header("Crouching Variables")]
    //[SerializeField] private float _slideForce = 400;
    [SerializeField] private float _slideCounterMovement = 0.2f;
    private Vector3 _crouchScale = new Vector3(1, 0.5f, 1);
    private Vector3 _playerScale;
    private Vector3 _normalVector = Vector3.up;
    private Vector3 _wallNormalVector;
    [Header("Jumping Variables")]
    [SerializeField] private float _jumpForce = 550f;
    private bool _readyToJump = true;
    private float _jumpCooldown = 0.25f;
    [Header("Input Variables")]
    private float _xInput, _yInput;
    private float _desiredX;
    private bool _jumping, _sprinting, _crouching;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        _playerScale = transform.localScale;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void FixedUpdate()
    {
        Movement();
    }

    void Update()
    {
        MyInput();
        Look();
    }

    /// <summary>
    /// Put this in its own class eventually
    /// </summary>
    private void MyInput()
    {
        _xInput = Input.GetAxisRaw("Horizontal");
        _yInput = Input.GetAxisRaw("Vertical");
        _jumping = Input.GetButton("Jump");
        _crouching = Input.GetKey(KeyCode.LeftControl);

        //Crouching
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            StartCrouch();
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            StopCrouch();
        }
    }

    private void StartCrouch()
    {
        transform.localScale = _crouchScale;
        transform.position = new Vector3(transform.position.x, transform.position.y - 0.5f, transform.position.z);
    }

    private void StopCrouch()
    {
        transform.localScale = _playerScale;
        transform.position = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
    }

    private void Movement()
    {
        //Extra Gravity
        _rigidbody.AddForce(Vector3.down * Time.deltaTime * 10);

        //Find actual velocity relative to where player is looking
        Vector2 mag = FindVelocityRelativeToLook();
        float xMag = mag.x, yMag = mag.y;

        //Counteract sliding and sloppy movement
        CounterMovement(_xInput, _yInput, mag);

        //If holding jump && ready to jump, then jump
        if (_readyToJump && _jumping) Jump();

        //Set max speed
        float maxSpeed = this._maxSpeed;

        //If sliding down a ramp, add force down so player stays grounded and also builds speed
        if (_crouching && _grounded && _readyToJump)
        {
            _rigidbody.AddForce(Vector3.down * Time.deltaTime * 3000);
            return;
        }

        //If speed is larger than maxspeed, cancel out the input so you don't go over max speed
        if (_xInput > 0 && xMag > maxSpeed) _xInput = 0;
        if (_xInput < 0 && xMag < -maxSpeed) _xInput = 0;
        if (_yInput > 0 && yMag > maxSpeed) _yInput = 0;
        if (_yInput < 0 && yMag < -maxSpeed) _yInput = 0;

        //Some multipliers
        float multiplier = 1f, multiplierV = 1f;

        // Movement in air
        if (!_grounded)
        {
            multiplier = 0.5f;
            multiplierV = 0.5f;
        }

        // Movement while sliding
        if (_grounded && _crouching) multiplierV = 0f;

        //Apply forces to move player
        _rigidbody.AddForce(_playerOrientation.transform.forward * _yInput * _moveSpeed * Time.deltaTime * multiplier * multiplierV);
        _rigidbody.AddForce(_playerOrientation.transform.right * _xInput * _moveSpeed * Time.deltaTime * multiplier);
    }

    private void Jump()
    {
        if (_grounded && _readyToJump)
        {
            _readyToJump = false;

            //Add jump force
            _rigidbody.AddForce(Vector2.up * _jumpForce * 1.5f);
            _rigidbody.AddForce(_normalVector * _jumpForce * 0.5f);

            //If jumping while falling, reset y velocity
            Vector3 vel = _rigidbody.velocity;
            if (_rigidbody.velocity.y < 0.5f)
                _rigidbody.velocity = new Vector3(vel.x, 0, vel.z);
            else if (_rigidbody.velocity.y > 0)
                _rigidbody.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

            Invoke(nameof(ResetJump), _jumpCooldown);
        }
    }

    private void ResetJump()
    {
        _readyToJump = true;
    }

    private void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * _sensitivity * Time.fixedDeltaTime * _sensitivityMultiplier;
        float mouseY = Input.GetAxis("Mouse Y") * _sensitivity * Time.fixedDeltaTime * _sensitivityMultiplier;

        //Find current look rotation
        Vector3 rot = _playerCamera.transform.localRotation.eulerAngles;
        _desiredX = rot.y + mouseX;

        //Rotate, and also make sure we dont over- or under-rotate.
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        //Perform the rotations
        _playerCamera.transform.localRotation = Quaternion.Euler(_xRotation, _desiredX, 0);
        _playerOrientation.transform.localRotation = Quaternion.Euler(0, _desiredX, 0);
    }

    private void CounterMovement(float x, float y, Vector2 mag)
    {
        if (!_grounded || _jumping) return;

        //Slow down sliding
        if (_crouching)
        {
            _rigidbody.AddForce(_moveSpeed * Time.deltaTime * -_rigidbody.velocity.normalized * _slideCounterMovement);
            return;
        }

        //Counter movement
        if (Mathf.Abs(mag.x) > _threshold && Mathf.Abs(x) < 0.05f || (mag.x < -_threshold && x > 0) || (mag.x > _threshold && x < 0))
        {
            _rigidbody.AddForce(_moveSpeed * _playerOrientation.transform.right * Time.deltaTime * -mag.x * _counterMovement);
        }
        if (Mathf.Abs(mag.y) > _threshold && Mathf.Abs(y) < 0.05f || (mag.y < -_threshold && y > 0) || (mag.y > _threshold && y < 0))
        {
            _rigidbody.AddForce(_moveSpeed * _playerOrientation.transform.forward * Time.deltaTime * -mag.y * _counterMovement);
        }

        //Limit diagonal running. This will also cause a full stop if sliding fast and un-crouching, so not optimal.
        if (Mathf.Sqrt((Mathf.Pow(_rigidbody.velocity.x, 2) + Mathf.Pow(_rigidbody.velocity.z, 2))) > _maxSpeed)
        {
            float fallspeed = _rigidbody.velocity.y;
            Vector3 n = _rigidbody.velocity.normalized * _maxSpeed;
            _rigidbody.velocity = new Vector3(n.x, fallspeed, n.z);
        }
    }

    public Vector2 FindVelocityRelativeToLook()
    {
        float lookAngle = _playerOrientation.transform.eulerAngles.y;
        float moveAngle = Mathf.Atan2(_rigidbody.velocity.x, _rigidbody.velocity.z) * Mathf.Rad2Deg;

        float u = Mathf.DeltaAngle(lookAngle, moveAngle);
        float v = 90 - u;

        float magnitue = _rigidbody.velocity.magnitude;
        float yMag = magnitue * Mathf.Cos(u * Mathf.Deg2Rad);
        float xMag = magnitue * Mathf.Cos(v * Mathf.Deg2Rad);

        return new Vector2(xMag, yMag);
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < _maxSlopeAngle;
    }

    private bool cancellingGrounded;

    private void OnCollisionStay(Collision other)
    {
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (_ground != (_ground | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                _grounded = true;
                cancellingGrounded = false;
                _normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }

        //Invoke ground/wall cancel, since we can't check normals with CollisionExit
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        _grounded = false;
    }
}
