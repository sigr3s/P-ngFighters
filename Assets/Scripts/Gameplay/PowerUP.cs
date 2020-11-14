using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUP : MonoBehaviour
{
    public float duration = 3f;
    public float Gravity = -5f;
    public LayerMask Mask;
    public LayerMask Horizontal;
    public LayerMask Player;
    public GameObject Model;
   
    private Vector3 prevPosition = Vector3.zero;
    private RaycastHit[] hits = new RaycastHit[10];
    private int hitCount = 0;
    private float ySpeed = -5;
    private bool interactable = true;
    private float powerUPTimeOn = 0f;
    private PlayerController playerController;

    public virtual void StartPowerUP(PlayerController player){

    }

    public virtual void FinishPowerUP(PlayerController player){

    }

    public virtual void PowerUPUpdate(){

    }

    private void OnEnable() {
        ySpeed = Gravity;
        interactable = true;
        powerUPTimeOn = 0f;
        Model.SetActive(true);
    }

    protected void Update() {
        if(interactable){   
            transform.position += new Vector3(0, ySpeed * Time.deltaTime, 0f);

            hitCount = Physics.SphereCastNonAlloc(transform.position, transform.localScale.x * 0.5f, (prevPosition- transform.position).normalized, hits, 0f, Mask, QueryTriggerInteraction.UseGlobal);

            for(int i = 0; i < hitCount; i++){    
                var h = hits[i];

                if(h.collider != null && h.collider.gameObject != gameObject){
                    if(Horizontal == (Horizontal | (1 << h.collider.gameObject.layer))){
                        ySpeed = 0;
                    }
                    else if(Player.value == (Player.value | (1 << h.collider.gameObject.layer))){
                        if(h.collider.gameObject.TryGetComponent<PlayerController>(out playerController)){   
                            interactable = false;
                            Model.SetActive(false);
                            StartPowerUP(playerController);
                        }
                        continue;   
                    }
                }
            }

            if(hitCount == 0 && ySpeed == 0){
                ySpeed = Gravity;
            }
        }
        else if(powerUPTimeOn >= -0.5f){
            powerUPTimeOn += Time.deltaTime;

            if(powerUPTimeOn >= duration){
                powerUPTimeOn = -1;
                FinishPowerUP(playerController);
            }

        }

        PowerUPUpdate();
    }
}
