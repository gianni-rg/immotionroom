using ImmotionAR.ImmotionRoom.LittleBoots.VR.Girello;
using ImmotionAR.ImmotionRoom.LittleBoots.VR.PlayerController;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GirelloCoso : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        var playerController = FindObjectOfType<IroomPlayerController>();

        if (playerController.IsVrReady)
        {
            GirelloData gd = playerController.GirelloData;
            transform.GetChild(0).position = gd.Center;
            transform.GetChild(0).rotation = gd.Rotation;
            transform.GetChild(0).localScale = gd.Size;

            transform.GetChild(1).position = gd.Center + gd.Rotation * new Vector3(gd.Size.x / 2, 0, gd.Size.z / 2);
        }
	}
}
