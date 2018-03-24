using UnityEngine;
using System.Collections;
using ImmotionAR.ImmotionRoom.LittleBoots.Avateering;

public class GiveHMDOrientation : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
        BodyAvatarer[] avatarers = FindObjectsOfType<BodyAvatarer>();

        foreach(BodyAvatarer avatarer in avatarers)
            avatarer.InjectedJointPoses[ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model.TrackingServiceBodyJointTypes.Neck] =
                FindObjectOfType<OVRManager>().transform.GetChild(0).FindChild("CenterEyeAnchor").rotation;
	
	}
}
