﻿using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviourPun, IPunObservable
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

    private InputAction m_SuperAction{
        get {
            return m_PlayerInput.actions["super"];
        }
    }

    public Action OnUIShouldUpdate;

    // Attributes
    public float health = 100.0f;
    [SerializeField] public float super = 0.0f;
    public bool invulnerable = false;
    [Header("Movement")]
    [SerializeField] private float jumpSpeed = 18.0F;
    [SerializeField] private float moveSpeed = 8.0F;
    [SerializeField] private float gravity = 40.0F;
    [SerializeField] private Animator animator = null;
    private Vector3 _moveDirection = Vector3.zero;
    [Header("Projectiles")]
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileOrigin = null;

    [Header("Effects")]
    public ParticleSystem system;
    public AudioClip damageClip;

    private ProjectileController _currentShot = null;
    private CharacterController _charaterController = null;

    private bool isLocal = true;
    private bool initialized = false;

    public void ChargeSuper(float v)
    {
        super += v;
        OnUIShouldUpdate?.Invoke();
    }

    public bool instantShoot = false;

    private HazardSpawner spawner;

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

    public void Initialize(PlayerID playerID, bool isLocal, HazardSpawner spawner){ //TODO: Sync with alex on owner?
        this.playerID = playerID;
        this.isLocal = isLocal;
        this.spawner = spawner;

        GetComponent<PlayerInput>().enabled = isLocal;

        SetColor(DataUtility.GetColorFor(playerID));
        _charaterController = GetComponent<CharacterController>();
        initialized = true;
        _moveDirection = Vector3.zero;
    }

    private void SetColor(Color c){
        Renderer[] renderes = GetComponentsInChildren<Renderer>();

        foreach(var r in renderes){
            r.material.color = c;
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
            animator.SetTrigger("jump");
        }
        _moveDirection.y -= gravity * Time.deltaTime;
        _moveDirection.x = moveSpeed * move.x;
        _charaterController.Move(_moveDirection * Time.deltaTime);
        
        if(Mathf.Abs(_moveDirection.x) > 1){
            animator.SetBool("forward",(m_otherPlayer.transform.position.x - transform.position.x) * _moveDirection.x > 0 ? true : false);
            animator.SetBool("backwards",(m_otherPlayer.transform.position.x - transform.position.x) * _moveDirection.x > 0 ? false : true);
        }
        else{
            animator.SetBool("forward",false);
            animator.SetBool("backwards",false);
        }

        // Fire
        if(m_FireAction.triggered && throwHazard != null){
            throwHazard.Throw( transform.forward.x > 0 ? false : true, playerID);
        }
        else if (m_FireAction.triggered && _charaterController.isGrounded) {
            if (_currentShot == null || !_currentShot.alive){
                animator.SetTrigger("shoot");
                ShootProjectile();
            }
            else if(instantShoot){
                animator.SetTrigger("shoot");
                ShootProjectile();
            }
        }

        if(m_SuperAction.triggered && super >= 100f){
            
            super = 0f;
            OnUIShouldUpdate?.Invoke();

            if(DataUtility.gameData.isNetworkedGame){
                PunTools.PhotonRpcMine(photonView, "RPC_Super", RpcTarget.MasterClient);
            }
            else{
                DoSuperInteral();
            }
        }

        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        if(m_otherPlayer != null)
        {
            transform.forward = new Vector3((m_otherPlayer.transform.position.x - transform.position.x) > 0 ? 1 : -1, 0 ,0);
        }
    }

    private void DoSuperInteral()
    {
        for(int i = 0; i < 3; i ++){
            spawner.CreateThrowHazard(1, transform.position + new Vector3(0, 3f*i, 0), playerID, transform.forward.x > 0 ? false : true);
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
        
        if( (photonView != null && photonView.IsMine) || !DataUtility.gameData.isNetworkedGame){
            _currentShot.owner = this;   
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
                SetColor(DataUtility.gameData.PlayerInvColor);
                system.emissionRate = 0;
                Invoke("SwitchOfInv", 2.0f);
                SoundManager.Instance.PlaySound(damageClip);
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
        system.emissionRate = 1;
        SetColor(DataUtility.GetColorFor(playerID));
    }

    #region PUN methods   
    [PunRPC]
    protected void RPC_ShootProjectile()
    {        
        InternalShootProjectile();
    }

    [PunRPC]
    protected void RPC_Super()
    {        
        DoSuperInteral();
    }
    
    [PunRPC]
    protected void RPC_Damage(float amount)
    {        
        health -= amount;
        OnUIShouldUpdate?.Invoke();
        invulnerable = true;
        SetColor(DataUtility.gameData.PlayerInvColor);
        system.emissionRate = 0;
        Invoke("SwitchOfInv", 2.0f);
        SoundManager.Instance.PlaySound(damageClip);
    }

    void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(super);
        }
        else
        {
            float ls = (float)stream.ReceiveNext();

            if(ls != super){
                super = ls;
                OnUIShouldUpdate?.Invoke();
            }
        }
    }
    #endregion
}

public enum PlayerID{
    NP = 0,
    Player1 = 1,
    Player2 = 2
}