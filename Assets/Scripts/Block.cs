using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour {

    public static Color[] colors = {
        Color.HSVToRGB(0,.5f,1), Color.HSVToRGB(1f/12, .5f, 1), Color.HSVToRGB(2f/12, .5f, 1), Color.HSVToRGB(3f/12,.5f,1),
        Color.HSVToRGB(4f/12,.5f,1), Color.HSVToRGB(5f/12, .5f, 1), Color.HSVToRGB(6f/12, .5f, 1), Color.HSVToRGB(7f/12,.5f,1),
        Color.HSVToRGB(8f/12,.5f,1), Color.HSVToRGB(9f/12, .5f, 1), Color.HSVToRGB(10f/12, .5f, 1), Color.HSVToRGB(11f/12,.5f,1),
        Color.HSVToRGB(0,1,1), Color.HSVToRGB(1f/12, 1, 1), Color.HSVToRGB(2f/12, 1, 1), Color.HSVToRGB(3f/12,1,1),
        Color.HSVToRGB(4f/12,1,1), Color.HSVToRGB(5f/12, 1, 1), Color.HSVToRGB(6f/12, 1, 1), Color.HSVToRGB(7f/12,1,1),
        Color.HSVToRGB(8f/12,1,1), Color.HSVToRGB(9f/12, 1, 1), Color.HSVToRGB(10f/12, 1, 1), Color.HSVToRGB(11f/12,1,1),
        new Color(.2f,.2f,.2f), new Color(.6f,.6f,.6f), new Color(1, 1, 1)
    };

    public int value;
    int colorIndex;
    MaterialPropertyBlock props;
    Renderer renderer;

    void Awake() {
        renderer = GetComponent<Renderer>();
        props = new MaterialPropertyBlock();
    }

    public void Spawn(Vector3 pos, int val) {
        transform.position = pos;
        value = val;
        colorIndex = value - 1;
        props.SetColor("_Color", colors[colorIndex]);
        renderer.SetPropertyBlock(props);
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

    public void Merge() {
        value *= 2;
        colorIndex++;
        StartCoroutine(DoMerge());
    }

    IEnumerator DoMerge() {
        yield return new WaitForSeconds(0.2f);
        props.SetColor("_Color", colors[colorIndex]);
        renderer.SetPropertyBlock(props);
        float t = 0;
        while (t < 1) {
            float s = 0.8f + 0.5f * Mathf.Sin(t * Mathf.PI);
            transform.localScale = new Vector3(s, s, s);
            t +=  5 * Time.deltaTime;
            yield return null; 
        }
        transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
    }
}
