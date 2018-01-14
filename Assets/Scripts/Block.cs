using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {

    public int value;
	
    public void Spawn(Vector3 pos) {
        transform.position = pos;
        value = Random.Range(0, 10) < 9 ? 1 : 2;
        StartCoroutine(DoSpawn());
    }

    IEnumerator DoSpawn() {
        float t = 0;
        while(t< 0.8f) {
            transform.localScale = new Vector3(t, t, t);
            t += 5 * Time.deltaTime;
            yield return null;
        }
        transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
    }

    public void Move(Vector3 toPos, bool merge) {
        StartCoroutine(DoMove(toPos, merge));
    }

    IEnumerator DoMove(Vector3 toPos, bool merge) {
        Vector3 fromPos = transform.position;
        float t = 0;
        while(t < 1) {
            transform.position = (1 - t) * fromPos + t * toPos;
            t += 5 * Time.deltaTime;
            yield return null;
        }
        transform.position = merge ? new Vector3(1000,1000,1000) : toPos ;
    }

}
