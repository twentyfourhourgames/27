using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamRotator : MonoBehaviour {
    
    float SmoothStep(float t) {
        return t * t * (3 - 2 * t);
    }

    public void Rotate(bool positiveRot) {
        float start = transform.eulerAngles.y;
        float end = positiveRot ? start + 90 : start - 90;
        StartCoroutine(DoRotate(start, end));
    }

    IEnumerator DoRotate(float start, float end) {
        float t = 0;
        while (t < 1) {
            float curr = Mathf.Lerp(start, end, SmoothStep(t));
            transform.eulerAngles = new Vector3(35, curr, 0);
            t += 5 * Time.deltaTime;
            yield return null;
        }
        transform.eulerAngles = new Vector3(35f, end, 0);
    }

    public void VertRotate(bool lookUp) {
        StartCoroutine(DoVertRotate(lookUp));
    }

    IEnumerator DoVertRotate(bool lookUp) {
        float currY = transform.eulerAngles.y;
        float t = 0;
        while (t < 1) {
            float currX = Mathf.Lerp(-35, 35, SmoothStep(t));
            if (lookUp)
                currX *= -1;
            transform.eulerAngles = new Vector3(currX, currY, 0);
            t += 5 * Time.deltaTime;
            yield return null;
        }
        transform.eulerAngles = new Vector3(lookUp ? -35 : 35, currY, 0);
    }
}
