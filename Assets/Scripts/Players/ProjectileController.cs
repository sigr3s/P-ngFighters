using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private GameObject trailGO = null;
    private Vector3 _moveDirection = Vector3.zero;
    public bool alive = true;
    public PlayerID shooter = PlayerID.NP;
    public PlayerController owner = null;

    void Start()
    {
        _moveDirection.y = 10.0f;
        Renderer[] renderParts = GetComponentsInChildren<Renderer>();

        foreach(Renderer r in renderParts){
            r.material.color = DataUtility.GetColorFor(shooter);
        }
    }

    void Update()
    {
        transform.Translate(_moveDirection * Time.deltaTime, Space.World);
        trailGO.transform.localScale = new Vector3(trailGO.transform.localScale.x, Mathf.Abs(transform.position.y) - 0.5f, trailGO.transform.localScale.z);
        trailGO.transform.localPosition = new Vector3(trailGO.transform.localPosition.x, -0.5f - (trailGO.transform.localScale.y / 2.0f), trailGO.transform.localPosition.z);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Ceiling")
        {
            alive = false;
            gameObject.SetActive(false);
        }
        else{
            // FIXME: Handle player here
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(other.gameObject.TryGetComponent<Hazard>(out Hazard h)){
            if(owner != null){
                int hazardLevel = h.TryDestroyHazard(shooter);
                if(hazardLevel > 0){
                    Disable(hazardLevel);
                }
            }
        }
    }

    public void Disable(int hazardLevel){
        alive = false;
        gameObject.SetActive(false);
        owner?.ChargeSuper( Mathf.Pow(1.5f, (5.0f- hazardLevel)));
    }

    private void OnDisable() {
        alive = false;
    }
}
