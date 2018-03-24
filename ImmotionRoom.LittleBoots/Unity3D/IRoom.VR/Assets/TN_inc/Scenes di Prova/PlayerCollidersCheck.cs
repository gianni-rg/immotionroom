using UnityEngine;
using System.Collections;
using ImmotionAR.ImmotionRoom.LittleBoots.Avateering.Collisions;

public class PlayerCollidersCheck : MonoBehaviour
{

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        if (!GameObject.Find("IRoomPlayerController"))
            return;

        Collider col = AvatarCollidersProps.GetAvatarLeftFootCollider(GameObject.Find("IRoomPlayerController").transform);

        if (col != null)
            Debug.Log("Left foot: " + col.gameObject.name);

        col = AvatarCollidersProps.GetAvatarRightFootCollider(GameObject.Find("IRoomPlayerController").transform);

        if (col != null)
            Debug.Log("Right foot: " + col.gameObject.name);

        col = AvatarCollidersProps.GetAvatarLeftHandCollider(GameObject.Find("IRoomPlayerController").transform);

        if (col != null)
            Debug.Log("Left hand: " + col.gameObject.name);

        col = AvatarCollidersProps.GetAvatarRightHandCollider(GameObject.Find("IRoomPlayerController").transform);

        if (col != null)
            Debug.Log("Right hand: " + col.gameObject.name);
	}

    void OnCollisionEnter(Collision collision)
    {
        string dbgString = "";

        if(AvatarCollidersProps.IsAvatarCollider(collision.collider))
        {
            dbgString += "AVATAR_";

            if(AvatarCollidersProps.IsAvatarHandCollider(collision.collider))
            {
                dbgString += "HAND_";

                if(AvatarCollidersProps.IsAvatarLeftHandCollider(collision.collider))
                {
                    dbgString += "LEFT";
                }
                else if (AvatarCollidersProps.IsAvatarRightHandCollider(collision.collider))
                {
                    dbgString += "RIGHT";
                }
            }
            else if (AvatarCollidersProps.IsAvatarFootCollider(collision.collider))
            {
                dbgString += "FOOT_";

                if (AvatarCollidersProps.IsAvatarLeftFootCollider(collision.collider))
                {
                    dbgString += "LEFT";
                }
                else if (AvatarCollidersProps.IsAvatarRightFootCollider(collision.collider))
                {
                    dbgString += "RIGHT";
                }
            }
        }

        transform.parent.GetChild(1).GetComponent<TextMesh>().text = dbgString;
    }

    void OnTriggerEnter(Collider collider)
    {
        string dbgString = "";

        if (AvatarCollidersProps.IsAvatarCollider(collider))
        {
            dbgString += "AVATAR_";

            if (AvatarCollidersProps.IsAvatarHandCollider(collider))
            {
                dbgString += "HAND_";

                if (AvatarCollidersProps.IsAvatarLeftHandCollider(collider))
                {
                    dbgString += "LEFT";
                }
                else if (AvatarCollidersProps.IsAvatarRightHandCollider(collider))
                {
                    dbgString += "RIGHT";
                }
            }
            else if (AvatarCollidersProps.IsAvatarFootCollider(collider))
            {
                dbgString += "FOOT_";

                if (AvatarCollidersProps.IsAvatarLeftFootCollider(collider))
                {
                    dbgString += "LEFT";
                }
                else if (AvatarCollidersProps.IsAvatarRightFootCollider(collider))
                {
                    dbgString += "RIGHT";
                }
            }
        }

        transform.parent.GetChild(1).GetComponent<TextMesh>().text = dbgString;
    }
}
