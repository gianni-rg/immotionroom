namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Model;

    /// <summary>
    ///     Defines a base class for objects that can detects walking movement of the player
    /// </summary>
    internal abstract class PlayerWalkingDetector : IPlayerWalkingDetector
    {
        /// <summary>
        ///     Last walking detection results
        /// </summary>
        protected PlayerWalkingDetection m_currentDetection;

        /// <summary>
        ///     Array of possible joint types
        /// </summary>
        protected static readonly IEnumerable<BodyJointTypes> JointTypesArray = Enum.GetValues(typeof (BodyJointTypes)).Cast<BodyJointTypes>();

        #region Constructor

        #endregion

        #region IPlayerWalkingDetector Methods

        /// <summary>
        ///     Get last analysis result of this object about user walking gesture
        /// </summary>
        public PlayerWalkingDetection CurrentDetection
        {
            get { return m_currentDetection; }
        }

        /// <summary>
        ///     Perform new detection of walking movement, because new joint data is arrived.
        ///     It is advised to call this function at a very regular interval
        /// </summary>
        /// <param name="timestamp">Time since a common reference event, like the start of the program</param>
        /// <param name="body">New body joint data</param>
        public abstract void UpdateDetection(TimeSpan timestamp, BodyData body);


        /// <summary>
        ///     Load detection algorithm parameters
        /// </summary>
        /// <param name="runtimeParameters">Key-value pairs of detector parameters</param>
        public abstract void LoadSettings(Dictionary<string, string> runtimeParameters);

        /// <summary>
        ///     Serialize object info into a dictionary, for debugging purposes
        /// </summary>
        /// <returns>Object serialization into a dictionary of dictionaries (infos are subdivided into groups)</returns>
        public abstract Dictionary<string, Dictionary<string, string>> DictionarizeInfo();

        #endregion

        #region Helper functions

        /// <summary>
        ///     Check if two bodies positions are completely equal
        /// </summary>
        /// <param name="body1">First body</param>
        /// <param name="body2">Second body</param>
        /// <returns>True if the body have EXACTLY the same positions in all joints; false otherwise</returns>
        public static bool CompareBodies(BodyData body1, BodyData body2)
        {
            //cycle through all joints and see if their positions are equal in both skeletons
            var equality = true;

            if (body1 != null && body2 != null)
            {
                foreach (BodyJointTypes jt in JointTypesArray)
                    if (body1.Joints[jt].ToVector3() != body2.Joints[jt].ToVector3())
                    {
                        equality = false;
                        break;
                    }
            }

            return equality;
        }

        /// <summary>
        ///     Check if two bodies positions are completely equal, plus converts the first body into a dictionary of joints.
        ///     The first is a body arrived from the kinect, the second is the body of the previous frame, already converted to a
        ///     dictionary of joint positions.
        ///     The method checks if the two bodies represents the same identical positions and in the meanwhile, it converts
        ///     currentBody to a dictionary
        ///     of joint positions and store this representation in previousBody
        /// </summary>
        /// <param name="currentBody">Current frame body, as arrived from Kinect</param>
        /// <param name="previousBody">Previous frame body: after the function call, it will hold current frame body data</param>
        /// <returns>True if the body have EXACTLY the same positions in all joints; false otherwise</returns>
        public static bool CompareBodiesAndUpdate(BodyData currentBody, ref Dictionary<BodyJointTypes, Vector3> previousBody)
        {
            //cycle through all joints and see if their positions are equal in both skeletons
            var equality = true;

            if (currentBody != null && previousBody != null)
            {
                //for each  joint
                foreach (BodyJointTypes jt in JointTypesArray)
                {
                    //get current position
                    Vector3 body1JointPos = currentBody.Joints[jt].ToVector3();

                    //compare with previous frame
                    if (previousBody.ContainsKey(jt) && currentBody.Joints[jt].ToVector3() != previousBody[jt])
                    {
                        equality = false;
                    }

                    //save new value in the dictionary
                    previousBody[jt] = body1JointPos;
                }
            }

            return equality;
        }
        
        #endregion
    }
}
