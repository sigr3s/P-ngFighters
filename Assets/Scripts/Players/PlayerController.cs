using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    private PlayerInput _playerInput;
    private PlayerInput m_PlayerInput {
        get{
            if(_playerInput){
                _playerInput = GetComponent<PlayerInput>();
            }

            return _playerInput;
        }
    }
    private InputAction m_MoveAction {
        get{
           return m_PlayerInput.actions["move"];
        }
    }

    private InputAction m_JumpAction {
        get{
           return m_PlayerInput.actions["jump"];
        }
    }

    private InputAction m_FireAction{
        get{
            return m_PlayerInput.actions["fire"];
        }
    }

    // A player can:
    //  Move wasd arrows etc
    //  Attack something
    //  Block stun
    //  A player always faces the other player bc you know fgc


    void Start()
    {
        
    }

    void Update()
    {
        if (m_FireAction.triggered){
             
        }

        var move = m_MoveAction.ReadValue<Vector2>();
    }
}
