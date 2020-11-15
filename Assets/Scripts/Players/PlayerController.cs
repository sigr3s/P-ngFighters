using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
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

    public PlayerID playerID = PlayerID.Player1;

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

    public Action OnUIShouldUpdate;

    // Attributes
    public float health = 100.0f;
    public float super = 0.0f;
    public bool invulnerable = false;
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

    private bool isLocal = true;
    private bool initialized = false;

    PhotonView photonView;
    public bool instantShoot = false;

    private PlayerController _other = null;

    private PlayerController m_otherPlayer {
        get{
            if(_other == null){
                var pcs = FindObjectsOfType<PlayerController>();

                foreach(var pc in pcs){
                    if(pc != this){
                        _other = pc;
                    }
                }
            }

            return _other;
        }
    }

    public void Initialize(PlayerID playerID, bool isLocal){ //TODO: Sync with alex on owner?
        this.playerID = playerID;
        this.isLocal = isLocal;

        GetComponent<PlayerInput>().enabled = isLocal;

        _charaterController = GetComponent<CharacterController>();
        GetComponentInChildren<Renderer>().material.color =  DataUtility.GetColorFor(playerID);
        initialized = true;
        _moveDirection = Vector3.zero;

        if(DataUtility.gameData.isNetworkedGame)
        {
            if(photonView == null){ photonView = GetComponentInChildren<PhotonView>(); }
        }
    }

    void Update()
    {
        if(!isLocal || !initialized){
            return;
        }

        if(!m_MoveAction.enabled){
            return;
        }

        // Movement
        var move = m_MoveAction.ReadValue<Vector2>();
        if (_charaterController.isGrounded && move.y > 0.4f) {
            _moveDirection.y = jumpSpeed;
        }
        _moveDirection.y -= gravity * Time.deltaTime;
        _moveDirection.x = moveSpeed * move.x;
        _charaterController.Move(_moveDirection * Time.deltaTime);
        // Fire
        if(m_FireAction.triggered && throwHazard != null){
            throwHazard.Throw( transform.forward.x > 0 ? false : true, playerID);
        }
        else if (m_FireAction.triggered && _charaterController.isGrounded) {
            if (_currentShot == null || !_currentShot.alive){
                ShootProjectile();
            }
            else if(instantShoot){
                ShootProjectile();
            }
        }

        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        if(m_otherPlayer != null)
        {
            transform.forward = new Vector3((m_otherPlayer.transform.position.x - transform.position.x) > 0 ? 1 : -1, 0 ,0);
        }
    }

    private void ShootProjectile()
    {
        if(DataUtility.gameData.isNetworkedGame)
        {
            PunTools.PhotonRpcMine(photonView, "RPC_ShootProjectile", RpcTarget.AllBuffered);
        }
        else
        {
            InternalShootProjectile(); 
        }
    }
    
    private void InternalShootProjectile()
    {
        if (_currentShot != null) {
            Destroy(_currentShot.gameObject);
        }
        _currentShot = GameObject.Instantiate(projectilePrefab, projectileOrigin.position, Quaternion.identity).GetComponent<ProjectileController>();
        _currentShot.shooter = playerID;
        if(photonView == null){ photonView = GetComponentInChildren<PhotonView>(); }
        
        if(photonView){
            _currentShot.owner = photonView.IsMine ? this : null;   
        }
        else{
            _currentShot.owner = null;
        }
    }


    public void Damage(float amount)
    {
        if (!invulnerable)
        {
            if(DataUtility.gameData.isNetworkedGame){
                PunTools.PhotonRpcMine(photonView, "RPC_Damage", RpcTarget.AllBuffered, amount);
            }
            else
            {
                health -= amount;
                OnUIShouldUpdate?.Invoke();
                invulnerable = true;
                Invoke("SwitchOfInv", 2.0f);
            }
        }
    }

    private Hazard throwHazard;

    private void OnTriggerEnter(Collider other) {
        //Damage, Powe UP
        if(!DataUtility.gameData.isNetworkedGame || photonView.IsMine) {
            Hazard hazard = other.gameObject.GetComponent<Hazard>();

            if (hazard != null && hazard.hazardOwner != playerID && hazard.hazardOwner != PlayerID.NP){
                
                if(hazard.thrown){
                    float pix = m_MoveAction.ReadValue<Vector2>().x;

                    if( Mathf.Sign(hazard.throwSpeed.x) == Mathf.Sign(pix) && Mathf.Abs(pix) > 0.15f ){
                        hazard.Throw( transform.forward.x > 0 ? false : true, playerID);
                    }
                    else{
                        Damage(20.0f);
                        hazard.DestroyIfThrown();
                    }
                }
                else{
                    Damage(20.0f);
                    hazard.DestroyIfThrown();
                }
            }
            else if(hazard != null && hazard.hazardOwner != PlayerID.NP){
                throwHazard = hazard;
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        Hazard hazard = other.gameObject.GetComponent<Hazard>();

        if(hazard != null && throwHazard == hazard){
            StartCoroutine(ReserShootingHazard(hazard));
        }
    }

    private IEnumerator ReserShootingHazard(Hazard h)
    {
        yield return new WaitForSeconds(0.25f);

        if(throwHazard == h){
            throwHazard = null;
        }
    }

    /*private void OnTriggerStay(Collider other) {
        //Damage, Powe UP
        if(!DataUtility.gameData.isNetworkedGame || photonView.IsMine) {
            Hazard hazard = other.gameObject.GetComponent<Hazard>();
            if (hazard != null && m_FireAction.triggered && hazard.hazardOwner == playerID){
                hazard.Throw( transform.forward.x > 0 ? false : true, playerID);
            }
        }
    }*/

    private void SwitchOfInv()
    {
        invulnerable = false;
    }

    #region PUN methods   
    [PunRPC]
    protected void RPC_ShootProjectile()
    {        
        InternalShootProjectile();
    }
    
    [PunRPC]
    protected void RPC_Damage(float amount)
    {        
        health -= amount;
        OnUIShouldUpdate?.Invoke();
        invulnerable = true;
        Invoke("SwitchOfInv", 2.0f);
    }
    #endregion
}

public enum PlayerID{
    NP = 0,
    Player1 = 1,
    Player2 = 2
}