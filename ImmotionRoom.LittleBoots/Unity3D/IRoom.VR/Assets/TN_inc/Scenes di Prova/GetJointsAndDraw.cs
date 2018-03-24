using UnityEngine;
using System.Collections;
using System;
using ImmotionAR.ImmotionRoom.LittleBoots.Avateering;
using ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model;

public class GetJointsAndDraw : MonoBehaviour 
{

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        BodyAvatarer[] avatarers = FindObjectsOfType<BodyAvatarer>();

        foreach(BodyAvatarer avatarer in avatarers)
        {
            foreach(TrackingServiceBodyJointTypes joint in Enum.GetValues(typeof(TrackingServiceBodyJointTypes)))
            {
                Transform goTransf = transform.FindChild(avatarer.transform.parent.parent.gameObject.name + avatarer.gameObject.name + joint.ToString());
                GameObject go;

                if (goTransf == null)
                {
                    go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.name = avatarer.transform.parent.parent.gameObject.name + avatarer.gameObject.name + joint.ToString();
                    go.transform.SetParent(transform, false);
                    go.GetComponent<Collider>().enabled = false;
                    go.GetComponent<Renderer>().material.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value);
                    go.transform.localScale = 0.15f * Vector3.one;
                }
                else
                    go = goTransf.gameObject;

                if (avatarer.GetJointTransform(joint) != null)
                    go.transform.position = avatarer.GetJointTransform(joint).position;
            }
        }
	}
}
