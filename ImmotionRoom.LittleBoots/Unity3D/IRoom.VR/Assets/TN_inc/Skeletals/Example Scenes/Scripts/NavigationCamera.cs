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
namespace ImmotionAR.ImmotionRoom.LittleBoots.IRoom.SkeletalTracking
{
    using UnityEngine;
    using System.Collections;

    /// <summary>
    /// Script to make the camera it is attached to, movable using mouse and keyboard to explore the scene
    /// </summary>
    public class NavigationCamera : MonoBehaviour
    {
        /// <summary>
        /// Rotational speed of the camera when the mouse is moved, in degrees / seconds
        /// </summary>
        [Tooltip("Rotational speed of the camera when the mouse is moved, in degrees / seconds")]
        public float RotationSpeed = 5.0f;

        /// <summary>
        /// Translational speed of the camera when the arrow/WASD keys are pressed, in units / seconds
        /// </summary>
        [Tooltip("Translational speed of the camera when the arrow/WASD keys are pressed, in units / seconds")]
        public float TranslationSpeed = 5.0f;               
        
        // Update is called once per frame
        void Update()
        {
            //if mouse is moved, increment x and y rotational components of the camera
            Vector3 eulerAngles = transform.localRotation.eulerAngles;

            eulerAngles.x += -RotationSpeed * Time.deltaTime * Input.GetAxis("Mouse Y");
            eulerAngles.y += RotationSpeed * Time.deltaTime * Input.GetAxis("Mouse X");
            transform.localRotation = Quaternion.Euler(eulerAngles);
          
            //if keys are pressed, make a step in the required direction
            Vector3 movementVector = Vector3.zero;

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
                movementVector.z += 1;

            if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
                movementVector.z -= 1;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
                movementVector.x -= 1;

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
                movementVector.x += 1;

            if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.RightControl))
                movementVector.y -= 1;

            if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.Minus))
                movementVector.y += 1;

            movementVector.Normalize(); //in case more than one key gets pressed simultaneously

            transform.localPosition += (new Vector3(0, movementVector.y, 0) + transform.localRotation * new Vector3(movementVector.x, 0, movementVector.z)) * TranslationSpeed * Time.deltaTime;
        }
    }
}