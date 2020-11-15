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

    [SerializeField] private List<PowerUP> powerUps = new List<PowerUP>();
    [SerializeField] private GameObject destroyEffect = null;
    [SerializeField] private GameObject throwEffect = null;

    private RaycastHit[] hits = new RaycastHit[10];
    private int hitCount = 0;
    private HazardSpawner spawner;
    private bool alive = false;
    public bool thrown = false;
    public Vector3 throwSpeed = Vector3.zero;

    public static float HazardSimulationRate = 1f;

    PhotonView _view;
    PhotonView view{
        get{
            if(_view == null){
                _view = GetComponent<PhotonView>();
            }

            return _view;
        }
    }
    
    public void Initialize(HazardSpawner hazardSpawner, int level, Vector3 pos, PlayerID owner = PlayerID.NP, float dir = 1)
    {
        spawner = hazardSpawner;

        if(DataUtility.gameData.isNetworkedGame){
            PunTools.PhotonRpcMine(view, "RPC_Initialize", RpcTarget.AllBuffered, level, pos, owner, dir);
        }
        else{
            gameObject.SetActive(true);
            transform.localScale = level*scaleFactor*Vector3.one;
            transform.position = pos;
            xSpeed = speed * dir;
            ySpeed = gravity;
            HazardLevel = level;
            alive = true;
            hazardOwner = owner;

            thrown = false;
            throwSpeed = Vector3.zero;

            GetComponent<Renderer>().material.color = DataUtility.GetColorFor(owner);
        }
        
    }

    private Vector3 prevPosition = Vector3.zero;

    private void FixedUpdate() 
    {   
        UpdatePosition();
    }

    private void UpdatePosition(){
        if(!alive) return;

        if (thrown) {
            transform.position += throwSpeed * Time.deltaTime;
            hitCount = Physics.SphereCastNonAlloc(transform.position, transform.localScale.x * 0.5f, (prevPosition- transform.position).normalized, hits, 0f, mask, QueryTriggerInteraction.UseGlobal);

            for(int i = 0; i < hitCount; i++){
                var h = hits[i];
                
                if( Vertical == (Vertical | (1 << h.collider.gameObject.layer))){
                    TryDestroyHazard(PlayerID.NP);
                }
            }
            return;
        }

        transform.position += new Vector3(xSpeed * Time.fixedDeltaTime * HazardSimulationRate, ySpeed * Time.fixedDeltaTime * HazardSimulationRate, 0f);

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


                    transform.position += new Vector3(0, ySpeed * Time.fixedDeltaTime * HazardSimulationRate, 0f);
                }
                else if( Vertical == (Vertical | (1 << h.collider.gameObject.layer))){
                    xSpeed *= -1;
                    transform.position += new Vector3(2* xSpeed * Time.fixedDeltaTime * HazardSimulationRate, 0f, 0f);
                }
            }
        }


        ySpeed = Mathf.Clamp(ySpeed + gravity*Time.fixedDeltaTime, gravity, -gravity * 3f);

        prevPosition = transform.position;
    }

    public int TryDestroyHazard(PlayerID player){
        if(player == hazardOwner){ 
            return -1;
        }

        bool generatePowerUp = UnityEngine.Random.Range(0f, 1f) > 0.95f;
        int powerUp = UnityEngine.Random.Range(0, powerUps.Count);

        if(DataUtility.gameData.isNetworkedGame){
            PunTools.PhotonRPC(view, "RPC_TryDestroyHazard", RpcTarget.AllBuffered, player, generatePowerUp , powerUp);
            return HazardLevel;
        }   
        else{
            if(generatePowerUp){
                Instantiate(powerUps[powerUp], transform.position, Quaternion.identity);
            }
            gameObject.SetActive(false);
            spawner.HazardDestroyed(HazardLevel, transform.position, player, this);
            spawner.Return(this);
            return HazardLevel;
        }
    }

    public void DestroyHazard()
    {
        if(DataUtility.gameData.isNetworkedGame){
            PunTools.PhotonRPC(view, "RPC_DestroyHazard", RpcTarget.AllBuffered);
        }   
        else{
            Destroy(gameObject);
        }
    }

    public void Throw(bool left, PlayerID owner)
    {
        if(DataUtility.gameData.isNetworkedGame){
            PunTools.PhotonRPC(view, "RPC_ThrowHazard", RpcTarget.AllBuffered, left, owner);
        }
        else{
            ThrowInternal(left, owner);
        }
    }

    private void ThrowInternal(bool left, PlayerID owner){
        this.hazardOwner = owner;
        GetComponent<Renderer>().material.color = DataUtility.GetColorFor(owner);

        thrown = true;
        if (left)
        {
            throwSpeed = new Vector3(-10.0f, 0.0f, 0.0f);
        }
        else
        {
            throwSpeed = new Vector3(10.0f, 0.0f, 0.0f);
        }

        var effect = Instantiate(throwEffect, transform.position, Quaternion.identity);
        effect.transform.localScale *= HazardLevel/2f;
    }

    public void DestroyIfThrown()
    {
        if (thrown) {
            TryDestroyHazard(PlayerID.NP);
        }
    }
    
    private void OnDisable() {
       var effect = Instantiate(destroyEffect, transform.position, Quaternion.identity);
       effect.transform.localScale *= HazardLevel/3f;
    }

    #region PUN methods   
    [PunRPC]
    protected void RPC_TryDestroyHazard(PlayerID player, bool generatePowerUp, int powerUp)
    {        
        spawner?.HazardDestroyed(HazardLevel, transform.position, player, this);

        if(generatePowerUp){
            Instantiate(powerUps[powerUp], transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

   [PunRPC]
    protected void RPC_DestroyHazard()
    {    
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

    [PunRPC]
    protected void RPC_ThrowHazard(bool left, PlayerID owner){
        ThrowInternal(left, owner);
    }
    #endregion
}