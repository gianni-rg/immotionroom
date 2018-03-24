using UnityEngine;
using System.Collections;
using ImmotionAR.ImmotionRoom.LittleBoots.VR.PlayerController;

public class moveplayer : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.O))
            FindObjectOfType<IroomPlayerController>().CharController.transform.position += new Vector3(5, 5, 0);
	}
}
