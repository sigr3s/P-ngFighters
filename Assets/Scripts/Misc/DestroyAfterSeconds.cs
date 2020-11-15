using System.Collections;
using UnityEngine;

public class DestroyAfterSeconds : MonoBehaviour {
    public float seconds = 1f;

    private void Awake() {
        StartCoroutine(DestroyAfter(seconds));
    }

    IEnumerator DestroyAfter(float seconds){
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);
    }
}