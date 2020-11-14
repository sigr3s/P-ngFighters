using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hazard : MonoBehaviour
{
    public enum HazardTarget{
        None,
        Player1,
        Player2
    }
    
    public HazardTarget hazardTarget;
    public int EnemyLevel = 4;
    public float gravity = 20.0F;
    public float speed = 20.0F;
    public float bounceFactor = 1.9f;


    private float xSpeed = 0f;
    private float ySpeed = 0f;

    public LayerMask mask;
    public LayerMask Horizontal;
    public LayerMask Vertical;

    private RaycastHit[] hits = new RaycastHit[10];
    int hitCount = 0;

    private void Start() {
        xSpeed = speed;
        ySpeed = gravity;
    }

    private void Update() {
        transform.position += new Vector3(xSpeed * Time.deltaTime, ySpeed * Time.deltaTime, 0f);

        hitCount = Physics.SphereCastNonAlloc(transform.position, transform.localScale.x, transform.forward, hits, 0f, mask, QueryTriggerInteraction.UseGlobal);

        for(int i = 0; i < hitCount; i++){
            var h = hits[i];

            if(h.collider != null && h.collider.gameObject != gameObject){
                

                if(Horizontal == (Horizontal | (1 << h.collider.gameObject.layer))){
                    if(ySpeed > 0){
                        ySpeed = gravity * bounceFactor * Mathf.Sign(h.transform.forward.y);
                    }
                    else{
                        ySpeed = -gravity * bounceFactor * Mathf.Sign(h.transform.forward.y);
                    }
                    transform.position += new Vector3(0, ySpeed * Time.deltaTime, 0f);
                }
                else if( Vertical == (Vertical | (1 << h.collider.gameObject.layer))){
                    xSpeed *= -1;
                    transform.position += new Vector3(2* xSpeed * Time.deltaTime, 0f, 0f);
                }
                else{
                        //TODO: Hit player?

                        continue;
                }

                transform.position += 1.25f * (transform.forward * speed) * Time.deltaTime; 
            }
        }


        ySpeed = Mathf.Clamp(ySpeed + gravity*Time.deltaTime, gravity, -gravity * 3f);
    }
}