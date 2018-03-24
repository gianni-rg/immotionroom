using UnityEngine;
using System.Collections;
using ImmotionAR.ImmotionRoom.LittleBoots.VR.HeadsetManagement;

public class PrintOculusRotation : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        Debug.Log("Rotation: " + GetComponent<OculusHmdManager>().OrientationInGame.eulerAngles);
	}
}
