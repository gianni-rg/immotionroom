/************************************************************************************************************
 * 
 * Copyright (C) 2014-2016 ImmotionAR, a division of Beps Engineering. All rights reserved.
 * 
 * Licensed under the ImmotionAR ImmotionRoom SDK License (the "License");
 * you may not use the ImmotionAR ImmotionRoom SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 * 
 * You may obtain a copy of the License at
 * 
 * http://www.immotionar.com/legal/ImmotionRoomSDKLicense.PDF
 * 
 ************************************************************************************************************/
namespace ImmotionAR.ImmotionRoom.LittleBoots.IRoom.VR
{
    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// Behaviour of the arrows of the girello
    /// </summary>
    public class GirelloArrow : MonoBehaviour
    {
        /// <summary>
        /// Speed of variation of scale of the arrow model
        /// </summary>
        public float ScaleSpeed = 1.0f;

        /// <summary>
        /// Initial scale of the arrow model
        /// </summary>
        private float m_initialScale;

        // Use this for initialization
        void Start()
        {
            m_initialScale = transform.localScale.y;
            transform.localScale = new Vector3(transform.localScale.x, 0, transform.localScale.z);
        }

        // Update is called once per frame
        void Update()
        {
            //perform a growing animation from scale 0 to initial scale of the object... and loop forever
            float newScale = transform.localScale.y + ScaleSpeed * Time.deltaTime;

            if(newScale > m_initialScale)
                newScale = 0;

            transform.localScale = new Vector3(transform.localScale.x, newScale, transform.localScale.z);
        }
    }

}
