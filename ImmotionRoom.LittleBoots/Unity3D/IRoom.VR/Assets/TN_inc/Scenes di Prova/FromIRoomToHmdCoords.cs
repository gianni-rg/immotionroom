namespace ImmotionAR.ImmotionRoom.LittleBoots.IRoom.VR
{
    using UnityEngine;
    using System.Collections;
    using ImmotionAR.ImmotionRoom.LittleBoots.VR.Calibration;
    using ImmotionAR.ImmotionRoom.Tools.Unity3d.Tools;

    public class FromIRoomToHmdCoords : MonoBehaviour
    {

        // Use this for initialization
        void Start()
        {
            StartCoroutine(GetCalibrationMatrix());
        }

        // Update is called once per frame
        void OnDestroy()
        {
            StopAllCoroutines();
        }

        /// <summary>
        /// Gets calibration matrix and apply it to current object
        /// </summary>
        /// <returns></returns>
        IEnumerator GetCalibrationMatrix()
        {
            //wait for the calibrator
            IroomPlayerCalibrator calibrator = null;
            while (calibrator == null)
            {
                calibrator = FindObjectOfType<IroomPlayerCalibrator>();
                yield return new WaitForEndOfFrame();
            }

            //wait for done calibration
            while (!calibrator.CalibrationDone)
            {
                yield return new WaitForSeconds(0.5f);
            }

            //transform current object using the calibration data
            transform.position = calibrator.CalibrationData.CalibrationMatrix.MultiplyPoint3x4(transform.position);
            transform.rotation = calibrator.CalibrationData.CalibrationMatrix.ToQuaternion() * transform.rotation;

            yield break;
        }
    }

}