using System;
using System.Collections.Generic;
using UnityEngine;

public class HazardSpawner : MonoBehaviour {
    [SerializeField] private Hazard hazard = null;
    [SerializeField] private List<Transform> hazardSpawnPoints = new List<Transform>();
    [SerializeField] private int initialPoolSize = 30;    
    [SerializeField] private int hazardLevel = 4;

    private List<Hazard> hazards = new List<Hazard>();
    private Queue<Hazard> hazardPool = new Queue<Hazard>();


    private int lvl1Destroyed = 0;


    private void Start() {
        GrowHazardPool(initialPoolSize);

        for(int i = 0; i < hazardSpawnPoints.Count; i ++){
            var h = GetPooledHazard();
            h.Initialize(this, hazardLevel, hazardSpawnPoints[i]);
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

    public void HazardDestroyed(int hl, Transform t){

        if(hl == 1){
            lvl1Destroyed++;
        }
        else{
            float dir = 1;
            for(int i = 0; i < 2; i ++){
                var h = GetPooledHazard();
                h.Initialize(this, hl - 1 , t, dir);
                dir *= -1;
            }
        }

        if(lvl1Destroyed == Math.Pow(2, hazardLevel -1)){
            var h = GetPooledHazard();
            h.Initialize(this, hazardLevel, hazardSpawnPoints[UnityEngine.Random.Range(0, hazardSpawnPoints.Count)]);
            lvl1Destroyed = 0;
        }   
    }

    public void Return(Hazard hazard)
    {
        hazardPool.Enqueue(hazard);
    }
}