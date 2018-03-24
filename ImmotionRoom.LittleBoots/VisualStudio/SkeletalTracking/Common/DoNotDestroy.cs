namespace ImmotionAR.ImmotionRoom.LittleBoots.SkeletalTracking.Common
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// Mark the game object as Don't destroy on load, so that it survives across scenes
    /// </summary>
    public class DoNotDestroy : MonoBehaviour
    {
        /// <summary>
        /// True to use object name for identification; false to use tag
        /// </summary>
        public bool UseName = true;

        /// <summary>
        /// Allocations map to avoid duplicates for objects
        /// </summary>
        protected static Dictionary<string, int> AllocationsMap = new Dictionary<string, int>();

        /// <summary>
        /// Executed at script start
        /// </summary>
        protected virtual void Awake()
        {
            //avoid duplicates, destroying current object if an object of the same type is already present
            string key = UseName ? gameObject.name : gameObject.tag;

            if (AllocationsMap.ContainsKey(key))
            {
                //Debug.Log("Destroy");
                Destroy(transform.gameObject);
            }
            else
            {
                //set object as to persist through scenes
                DontDestroyOnLoad(transform.gameObject);

                //save current gameobject instance ID into the map
                AllocationsMap[key] = gameObject.GetInstanceID();
            }
            //DontDestroyOnLoad(this);
            //transform.Find("")
        }

        void OnDestroy()
        {
            string key = UseName ? gameObject.name : gameObject.tag;

            if (AllocationsMap.ContainsKey(key) && AllocationsMap[key] == gameObject.GetInstanceID())
            {
                AllocationsMap.Remove(key);
            }
        }

    }

}
