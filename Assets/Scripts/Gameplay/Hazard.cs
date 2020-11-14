using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Hazard : MonoBehaviour
{   
    public PlayerID hazardOwner;
    public float gravity = 20.0F;
    public float speed = 20.0F;
    public float bounceFactor = 1.9f;
    public float scaleFactor = 5f;


    private int HazardLevel = 4;
    private float xSpeed = 0f;
    private float ySpeed = 0f;

    public LayerMask mask;
    public LayerMask Horizontal;
    public LayerMask Vertical;
    public LayerMask Weapon;
    public LayerMask Player;
    private RaycastHit[] hits = new RaycastHit[10];
    private int hitCount = 0;
    private HazardSpawner spawner;
    private bool alive = false;

    public static float HazardSimulationRate = 1f;

    PhotonView view;
    
    public void Initialize(HazardSpawner hazardSpawner, int level, Transform t, PlayerID owner = PlayerID.NP, float dir = 1)
    {
        spawner = hazardSpawner;

        if(DataUtility.gameData.isNetworkedGame){
            PunTools.PhotonRpcMine(view, "RPC_Initialize", RpcTarget.AllBuffered, level, t.position, owner, dir);
        }
        else{
            gameObject.SetActive(true);
            transform.localScale = level*scaleFactor*Vector3.one;
            transform.position = t.position;
            xSpeed = speed * dir;
            ySpeed = gravity;
            HazardLevel = level;
            alive = true;
            hazardOwner = owner;

            GetComponent<Renderer>().material.color = DataUtility.GetColorFor(owner);
        }
        
    }

    private Vector3 prevPosition = Vector3.zero;

    private void Update() 
    {
        if(DataUtility.gameData.isNetworkedGame){
            if(view == null){
                view = GetComponent<PhotonView>();
            }
        }
        
        UpdatePosition();
    }

    private void UpdatePosition(){
        if(!alive) return;

        transform.position += new Vector3(xSpeed * Time.deltaTime * HazardSimulationRate, ySpeed * Time.deltaTime * HazardSimulationRate, 0f);

        hitCount = Physics.SphereCastNonAlloc(transform.position, transform.localScale.x * 0.5f, (prevPosition- transform.position).normalized, hits, 0f, mask, QueryTriggerInteraction.UseGlobal);

        for(int i = 0; i < hitCount; i++){
            var h = hits[i];

            if(h.collider != null && h.collider.gameObject != gameObject){
                

                if(Horizontal == (Horizontal | (1 << h.collider.gameObject.layer))){
                    if(h.collider.gameObject.tag == "Ceiling"){
                        ySpeed = gravity * Mathf.Pow(bounceFactor, HazardLevel) * Mathf.Sign(h.transform.forward.y);
                    }
                    else{
                        ySpeed = -gravity *  Mathf.Pow(bounceFactor, HazardLevel) * Mathf.Sign(h.transform.forward.y);
                    }


                    transform.position += new Vector3(0, ySpeed * Time.deltaTime * HazardSimulationRate, 0f);
                }
                else if( Vertical == (Vertical | (1 << h.collider.gameObject.layer))){
                    xSpeed *= -1;
                    transform.position += new Vector3(2* xSpeed * Time.deltaTime * HazardSimulationRate, 0f, 0f);
                }
            }
        }


        ySpeed = Mathf.Clamp(ySpeed + gravity*Time.deltaTime, gravity, -gravity * 3f);

        prevPosition = transform.position;
    }

    public bool TryDestroyHazard(PlayerID player){
        if(player == hazardOwner) return false;

        if(DataUtility.gameData.isNetworkedGame){
            if(PhotonNetwork.IsMasterClient){
                spawner.HazardDestroyed(HazardLevel, transform, player);
                Destroy(gameObject);
            }
            else{
                PunTools.PhotonRpcMine(view, "RPC_DestroyHazard", RpcTarget.MasterClient, player);
            }

            return true;
        }   
        else{
            gameObject.SetActive(false);
            spawner.HazardDestroyed(HazardLevel, transform, player);
            spawner.Return(this);
            return true;
        }
    }

    public void Throw()
    {
        
    }

    #region PUN methods   
    [PunRPC]
    protected void RPC_DestroyHazard(PlayerID player)
    {        
        spawner.HazardDestroyed(HazardLevel, transform, player);
        Destroy(gameObject);
    }

    [PunRPC]
    protected void RPC_Initialize(int level, Vector3 pos, PlayerID owner, float dir = 1)
    {        
        gameObject.SetActive(true);
        transform.localScale = level*scaleFactor*Vector3.one;
        transform.position = pos;
        xSpeed = speed * dir;
        ySpeed = gravity;
        HazardLevel = level;
        alive = true;
        hazardOwner = owner;

        GetComponent<Renderer>().material.color = DataUtility.GetColorFor(owner);
    }
    #endregion
}