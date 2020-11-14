using System.Collections.Generic;
using UnityEngine;

public class HazardSpawner : MonoBehaviour {
    [SerializeField] private Hazard hazard;
    [SerializeField] private int screenHazardNum;
    [SerializeField] private int initialPoolSize;

    private List<Hazard> hazards;
    private Queue<Hazard> hazardPool = new Queue<Hazard>();


    private void Start() {
        for(int i = 0; i < initialPoolSize; i++){
            Hazard h = Instantiate(hazard, transform);
            h.gameObject.SetActive(false);
            h.transform.localPosition = Vector3.zero;
            h.transform.rotation = Quaternion.identity;
            h.transform.localScale = Vector3.one;
            hazardPool.Enqueue(h);
        }

        for(int i = 0; i < screenHazardNum; i ++){
            var h = hazardPool.Dequeue();
            h.gameObject.SetActive(true);
        }
    }
}