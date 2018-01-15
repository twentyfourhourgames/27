using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResManager : MonoBehaviour {

#if UNITY_STANDALONE
    void Awake () {
		int h = Screen.currentResolution.height - 100;
        int w = h * 9 / 16;
        Screen.SetResolution(w, h, false);
	}
#endif
}
