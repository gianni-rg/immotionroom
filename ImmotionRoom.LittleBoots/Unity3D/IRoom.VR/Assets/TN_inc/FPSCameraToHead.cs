namespace ImmotionAR.ImmotionRoom.LittleBoots.IRoom.VR
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.PlayerController;
    using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;

    public class FPSCameraToHead : MonoBehaviour
    {
        float oldAngle;

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            IroomPlayerController playerController = FindObjectOfType<IroomPlayerController>();

            if (playerController.IsVrReady)
            {
                Vector3 shoulderLeftPos = playerController.MainAvatar.GetJointTransform(TrackingServiceBodyJointTypes.ShoulderLeft).position;
                Vector3 shoulderRightPos = playerController.MainAvatar.GetJointTransform(TrackingServiceBodyJointTypes.ShoulderRight).position;
                Vector3 headPos = playerController.MainAvatar.GetJointTransform(TrackingServiceBodyJointTypes.Head).position;
                Vector3 neckPos = playerController.MainAvatar.GetJointTransform(TrackingServiceBodyJointTypes.Neck).position;

                transform.rotation = Quaternion.Euler(0, //Mathf.Rad2Deg * Mathf.Atan2(-headPos.z + neckPos.z, -headPos.x + neckPos.x),
                                                      Mathf.Rad2Deg * Mathf.Atan2(shoulderLeftPos.z - shoulderRightPos.z, shoulderLeftPos.x - shoulderRightPos.x),
                                                      0);
            }
        }
    }

}