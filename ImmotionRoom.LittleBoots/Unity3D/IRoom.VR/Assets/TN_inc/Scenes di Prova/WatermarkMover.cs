using UnityEngine;
using System.Collections;
using ImmotionAR.ImmotionRoom.LittleBoots.VR.Watermarking;

public class WatermarkMover : MonoBehaviour {

	// Update is called once per frame
	void LateUpdate () {
        GameWatermark wam = FindObjectOfType<GameWatermark>();

        if (wam != null)
            wam.transform.position = 1000 * Vector3.one;
	
	}
}
