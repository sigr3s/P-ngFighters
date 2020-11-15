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
        if(DataUtility.gameData.isNetworkedGame){
           if(!PhotonNetwork.IsMasterClient) return;
           
            for(int i = 0; i < hazardSpawnPoints.Count; i ++){
                var hp = PhotonNetwork.Instantiate(Path.Combine(pathRelativeToResources, prefabName), hazardSpawnPoints[i].position, Quaternion.identity);
                hp.GetComponent<Hazard>().Initialize(this, hazardLevel, hazardSpawnPoints[i]);
                hazards.Add(hp.GetComponent<Hazard>());
            }
        }
        else{
            for(int i = 0; i < hazardSpawnPoints.Count; i ++){
                var h = GetPooledHazard();
                h.Initialize(this, hazardLevel, hazardSpawnPoints[i]);
                hazards.Add(h);
            }
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

    public Hazard GetHazard(){
        return GetPooledHazard();
    }

    public void HazardDestroyed(int hl, Transform t, PlayerID player, Hazard hazard){

        hazards.Remove(hazard);

        if(DataUtility.gameData.isNetworkedGame){
            if(hl == 1){
                lvl1Destroyed++;
            }
            else{
                float dir = 1;
                for(int i = 0; i < 2; i ++){
                    var hp = PhotonNetwork.Instantiate(Path.Combine(pathRelativeToResources, prefabName), t.position, Quaternion.identity);
                    hp.GetComponent<Hazard>().Initialize(this, hl - 1 , t, player, dir);
                    dir *= -1;
                    hazards.Add(hp.GetComponent<Hazard>());
                }
            }

            if(lvl1Destroyed == Math.Pow(2, hazardLevel -1)){
                int pos = UnityEngine.Random.Range(0, hazardSpawnPoints.Count);

                var hp = PhotonNetwork.Instantiate(Path.Combine(pathRelativeToResources, prefabName), hazardSpawnPoints[pos].position, Quaternion.identity);
                hp.GetComponent<Hazard>().Initialize(this, hazardLevel, hazardSpawnPoints[pos]);
                lvl1Destroyed = 0;
                hazards.Add(hp.GetComponent<Hazard>());
            } 
        }
        else{
            if(hl == 1){
                lvl1Destroyed++;
            }
            else{
                float dir = 1;
                for(int i = 0; i < 2; i ++){
                    var h = GetPooledHazard();
                    h.Initialize(this, hl - 1 , t, player, dir);
                    dir *= -1;
                    hazards.Add(h);
                }
            }
            
            if(lvl1Destroyed == Math.Pow(2, hazardLevel -1)){
                var h = GetPooledHazard();
                h.Initialize(this, hazardLevel, hazardSpawnPoints[UnityEngine.Random.Range(0, hazardSpawnPoints.Count)]);
                lvl1Destroyed = 0;
                hazards.Add(h);
            }   
        }
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
}