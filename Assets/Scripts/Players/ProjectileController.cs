using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    private Vector3 _moveDirection = Vector3.zero;
    public bool alive = true;

    void Start()
    {
        _moveDirection.y = 15.0f;
    }

    void Update()
    {
        transform.Translate(_moveDirection * Time.deltaTime, Space.World);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.tag == "Ceiling")
        {
            alive = false;
            gameObject.SetActive(false);
        }
    }
}
