using System;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using UnityEngine;

public class HazardSpawner : MonoBehaviour {
    [SerializeField] private Hazard hazard = null;
    [SerializeField] private List<Transform> hazardSpawnPoints = new List<Transform>();
    [SerializeField] private int initialPoolSize = 30;    
    [SerializeField] private int hazardLevel = 4;

    
    [Header("Network")]
    string pathRelativeToResources = "PhotonPrefabs";
    string prefabName => "PunHazard";

    private List<Hazard> hazards = new List<Hazard>();
    private Queue<Hazard> hazardPool = new Queue<Hazard>();


    private int lvl1Destroyed = 0;


    // Out of start
    private void Start() {
        if(!DataUtility.gameData.isNetworkedGame){
            GrowHazardPool(initialPoolSize);
        }
    }

    public void StartRound(){
        if(DataUtility.gameData.isNetworkedGame && !PhotonNetwork.IsMasterClient) return;
           
        for(int i = 0; i < hazardSpawnPoints.Count; i ++){
            CreateHazard(hazardLevel, hazardSpawnPoints[i].position, PlayerID.NP);
        }
    }

    private Hazard GetPooledHazard(){

        if(hazardPool.Count > 0){
            return hazardPool.Dequeue();
        }
        else{
            GrowHazardPool(initialPoolSize);
        }

        return hazardPool.Dequeue();
    }

    private void GrowHazardPool(int poolSize)
    {
         for(int i = 0; i < poolSize; i++){
            Hazard h = Instantiate(hazard, transform);
            h.gameObject.SetActive(false);
            h.transform.localPosition = Vector3.zero;
            h.transform.rotation = Quaternion.identity;
            h.transform.localScale = Vector3.one;
            hazardPool.Enqueue(h);
        }
    }

    public void HazardDestroyed(int hl, Vector3 pos, PlayerID player, Hazard hazard){

        hazards.Remove(hazard);

        if(hl == 1){
            lvl1Destroyed++;
        }
        else{
            float dir = 1;
            for(int i = 0; i < 2; i ++){
                CreateHazard(hl - 1 , pos, player, dir);
                dir *= -1;
            }
        }

        if(lvl1Destroyed == Math.Pow(2, hazardLevel -1)){
            int p = UnityEngine.Random.Range(0, hazardSpawnPoints.Count);
            CreateHazard(hazardLevel, hazardSpawnPoints[p].position, PlayerID.NP);
            lvl1Destroyed = 0;
        } 
    }

    private Hazard CreateHazard(int level, Vector3 pos, PlayerID owner = PlayerID.NP, float dir = 1){
        Hazard hazard = null;
        if(DataUtility.gameData.isNetworkedGame){
            var hp = PhotonNetwork.Instantiate(Path.Combine(pathRelativeToResources, prefabName), pos, Quaternion.identity);
            hazard = hp.GetComponent<Hazard>();
        }
        else{
            hazard = GetPooledHazard();
        }

        hazard.Initialize(this, level, pos, owner, dir);
        hazards.Add(hazard);

        return hazard;
    }

    public void Return(Hazard hazard)
    {
        hazardPool.Enqueue(hazard);
    }

    public void CleanAll(){
        for(int i = hazards.Count -1; i >= 0; i--){
            if(hazards[i] != null){
                hazards[i].DestroyHazard();
            }
        }

        hazards = new List<Hazard>();

        lvl1Destroyed = 0;
    }

    public void CreateThrowHazard(int level, Vector3 pos, PlayerID owner, bool left){
        Hazard h = CreateHazard(level, pos, owner);
        h.Throw(left, owner);
    }
}