using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    // A player can:
    //  Move wasd arrows etc x
    //  Attack something ~
    //  Block stun
    //  A player always faces the other player bc you know fgc

    private PlayerInput _playerInput;
    private PlayerInput m_PlayerInput {
        get{
            if(_playerInput == null) {
                _playerInput = GetComponent<PlayerInput>();
            }
            return _playerInput;
        }
    }
    private InputAction m_MoveAction {
        get {
           return m_PlayerInput.actions["move"];
        }
    }

    private InputAction m_JumpAction {
        get {
           return m_PlayerInput.actions["jump"];
        }
    }

    private InputAction m_FireAction{
        get {
            return m_PlayerInput.actions["fire"];
        }
    }

    [Header("Movement")]
    [SerializeField] private float jumpSpeed = 18.0F;
    [SerializeField] private float moveSpeed = 8.0F;
    [SerializeField] private float gravity = 40.0F;
    private Vector3 _moveDirection = Vector3.zero;
    [Header("Projectiles")]
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileOrigin = null;
    private ProjectileController _currentShot = null;

    private CharacterController _charaterController = null;
    
    void Awake()
    {
        _charaterController = GetComponent<CharacterController>();
    }

    void Update()
    {
        // Movement
        var move = m_MoveAction.ReadValue<Vector2>();
        if (_charaterController.isGrounded && move.y > 0.4f) {
            _moveDirection.y = jumpSpeed;
        }
        _moveDirection.y -= gravity * Time.deltaTime;
        _moveDirection.x = moveSpeed * move.x;
        _charaterController.Move(_moveDirection * Time.deltaTime);
        // Fire
        if (m_FireAction.triggered) {
            if (_currentShot == null || !_currentShot.alive) ShootProjectile();
        }
    }

    private void ShootProjectile()
    {
        if (_currentShot != null) {
            Destroy(_currentShot.gameObject);
        }
        _currentShot = GameObject.Instantiate(projectilePrefab, projectileOrigin.position, Quaternion.identity).GetComponent<ProjectileController>();
    }
}
