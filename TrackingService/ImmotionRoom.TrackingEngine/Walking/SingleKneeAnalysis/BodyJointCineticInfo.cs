namespace ImmotionAR.ImmotionRoom.TrackingEngine.Walking
{
    using System;
    using Model;

    /// <summary>
    ///     Info about a body joint and physical measurements about its movement
    /// </summary>
    internal struct BodyJointCineticInfo
    {
        /// <summary>
        ///     Type of joint
        /// </summary>
        public BodyJointTypes JointType { get; set; }

        /// <summary>
        ///     Type of reference joint of current joint.
        ///     This is the joint "father" of current joint, in the human body hierarchy:
        ///     e.g. the joint knee is the reference for the joint ankle, because the former provides frame of reference for the
        ///     latter
        /// </summary>
        public BodyJointTypes ReferenceJointType { get; set; }

        /// <summary>
        ///     Time of current measurement, since the program start
        /// </summary>
        public TimeSpan Time { get; set; }

        /// <summary>
        ///     Position of joint, in world coordinates
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        ///     Instant velocity of joint
        /// </summary>
        public Vector3 InstantSpeed { get; set; }

        /// <summary>
        ///     Instant acceleration of joint
        /// </summary>
        public Vector3 InstantAcceleration { get; set; }

        /// <summary>
        ///     Polar angles of current joint, wrt the reference joint.
        ///     Considering a reference system aligned with the world one and centered into the reference joint,
        ///     the x component holds the angle in the XZ plane, while the y one the vertical angle along the y direction
        /// </summary>
        public Vector2 Angle { get; set; }

        /// <summary>
        ///     Angular speed of current joint, wrt the reference joint.
        ///     Angular reference system is the one described for <see cref="Angle" /> property
        /// </summary>
        public Vector2 InstantAngleSpeed { get; set; }

        #region Constructors

        /// <summary>
        ///     Constructor with full initialization
        /// </summary>
        /// <param name="jointType">Joint type</param>
        /// <param name="referenceJointType">Reference joint of current joint</param>
        /// <param name="time">Timestamp of the informations</param>
        /// <param name="position">Position of the joint</param>
        /// <param name="instantSpeed">Velocity of the joint</param>
        /// <param name="instantAcceleration">Acceleration of the joint</param>
        /// <param name="angle">Polar angles of current joint, wrt the reference joint</param>
        /// <param name="instantAngleSpeed">Angular speed of current joint, wrt the reference joint</param>
        public BodyJointCineticInfo(BodyJointTypes jointType, BodyJointTypes referenceJointType, TimeSpan time, Vector3 position,
            Vector3 instantSpeed, Vector3 instantAcceleration, Vector2 angle, Vector2 instantAngleSpeed) : this()
        {
            JointType = jointType;
            ReferenceJointType = referenceJointType;
            Time = time;
            Position = position;
            InstantSpeed = instantSpeed;
            InstantAcceleration = instantAcceleration;
            Angle = angle;
            InstantAngleSpeed = instantAngleSpeed;
        }

        /// <summary>
        ///     Constructs a cinetic informations object, calculating all the necessary data from an actual body.
        ///     Notice that since we have not previous measurements, all speed and acceleartions will be zero
        /// </summary>
        /// <param name="timestamp">Timestamp of joint measurements</param>
        /// <param name="body">Body joints</param>
        /// <param name="jointType">Type of joint to be taken in count</param>
        /// <param name="referenceJointType">Reference joint to be taken in count</param>
        public BodyJointCineticInfo(TimeSpan timestamp, BodyData body, BodyJointTypes jointType, BodyJointTypes referenceJointType)
            : this()
        {
            JointType = jointType;
            ReferenceJointType = referenceJointType;
            Time = timestamp;

            //compute position
            Position = body.Joints[jointType].ToVector3();

            //we don't have speed and acceleration
            InstantSpeed = Vector3.Zero;
            InstantAcceleration = Vector3.Zero;

            //compute angles.
            //Remember that angle measurements for small displacements are unreliable
            Vector3 referencePosition = body.Joints[referenceJointType].ToVector3();
            Vector3 positionInReference = Position - referencePosition; //joint position wrt the reference joint frame

            if (positionInReference.Magnitude > 0.025f)
                Angle = new Vector2(MathUtilities.ClampedAtan2(positionInReference.Z, positionInReference.X), MathUtilities.AdjustOrientation(Math.Asin(positionInReference.Y/positionInReference.Magnitude), 0));
            else
                Angle = Vector2.Zero;

            //we don't have angular speed
            InstantAngleSpeed = Vector2.Zero;
        }

        /// <summary>
        ///     Constructs a cinetic informations object, calculating all the necessary data from an actual body and a previous
        ///     one.
        ///     Joint types are taken from previous data
        /// </summary>
        /// <param name="timestamp">Timestamp of joint measurements</param>
        /// <param name="body">Body joints</param>
        /// <param name="previousMeasurements">Measurement of body joints at past keyframe</param>
        public BodyJointCineticInfo(TimeSpan timestamp, BodyData body, BodyJointCineticInfo previousMeasurements)
            : this()
        {
            JointType = previousMeasurements.JointType;
            ReferenceJointType = previousMeasurements.ReferenceJointType;
            Time = timestamp;

            //compute position
            Position = body.Joints[JointType].ToVector3();

            //compute speed and acceleration
            var deltaTime = (float) ((timestamp - previousMeasurements.Time).TotalMilliseconds*0.001);
            InstantSpeed = (Position - previousMeasurements.Position)/deltaTime;
            InstantAcceleration = (InstantSpeed - previousMeasurements.InstantSpeed)/deltaTime;

            //compute angles.
            //Remember that angle measurements for small displacements are unreliable
            Vector3 referencePosition = body.Joints[ReferenceJointType].ToVector3();
            Vector3 positionInReference = Position - referencePosition; //joint position wrt the reference joint frame

            if (positionInReference.Magnitude > 0.025f)
                Angle = new Vector2(MathUtilities.ClampedAtan2(positionInReference.Z, positionInReference.X), MathUtilities.AdjustOrientation(Math.Asin(positionInReference.Y/positionInReference.Magnitude), 0));
            else
                Angle = Vector2.Zero;

            //compute angular speed
            //beware to make the angles the most similar as possible, before making the difference
            //(e.g. 0 and 360 degrees difference is 0 and not 360)
            InstantAngleSpeed = (Angle - new Vector2(MathUtilities.AdjustOrientation(previousMeasurements.Angle.X, Angle.X),
                MathUtilities.AdjustOrientation(previousMeasurements.Angle.Y, Angle.Y)))/deltaTime;
        }

        /// <summary>
        ///     Constructs a cinetic informations object, calculating all the necessary data from an actual body and a previous
        ///     one.
        ///     Joint types are taken from previous data.
        ///     This method performs a forward direction check on the angle of the joint. Provided a forward direction, if the
        ///     Angle on the
        ///     XZ plane results opposite to the one of the forward direction, the vertical angle gets put at a certain
        ///     nonForwardYDefault angle (Angle.y = nonForwardYDefault).
        ///     This is useful when the joint to be analyzed is a knee, to not consider innatural movement of leg going behind the
        ///     hip
        ///     as a step
        /// </summary>
        /// <param name="timestamp">Timestamp of joint measurements</param>
        /// <param name="body">Body joints</param>
        /// <param name="previousMeasurements">Measurement of body joints at past keyframe</param>
        /// <param name="forwardDirectionAngle">Forward direction angle of the body, in the XZ plane</param>
        /// <param name="nonForwardYDefault">Value the Y component of angle has to assume if the joint fails the forward check</param>
        /// <param name="toleranceAngle">Tolerance angle, in radians, for forward direction check</param>
        public BodyJointCineticInfo(TimeSpan timestamp, BodyData body, BodyJointCineticInfo previousMeasurements,
            float forwardDirectionAngle, float nonForwardYDefault = (float)-Math.PI/2, float toleranceAngle = 108.3f * MathConstants.Deg2Rad)
            : this()
        {
            JointType = previousMeasurements.JointType;
            ReferenceJointType = previousMeasurements.ReferenceJointType;
            Time = timestamp;

            //compute position
            Position = body.Joints[JointType].ToVector3();

            //compute speed and acceleration
            var deltaTime = (float) ((timestamp - previousMeasurements.Time).TotalMilliseconds*0.001);
            InstantSpeed = (Position - previousMeasurements.Position)/deltaTime;
            InstantAcceleration = (InstantSpeed - previousMeasurements.InstantSpeed)/deltaTime;

            //compute angles.
            //Remember that angle measurements for small displacements are unreliable
            Vector3 referencePosition = body.Joints[ReferenceJointType].ToVector3();
            Vector3 positionInReference = Position - referencePosition; //joint position wrt the reference joint frame

            if (positionInReference.Magnitude > 0.025f)
                Angle = new Vector2(MathUtilities.ClampedAtan2(positionInReference.Z, positionInReference.X),
                    MathUtilities.AdjustOrientation(Math.Asin(positionInReference.Y/positionInReference.Magnitude), 0));
            else
                Angle = Vector2.Zero;

            //peform forward direction check
            if (MathUtilities.AdjustedAnglesAbsDifference(forwardDirectionAngle, Angle.X) >= toleranceAngle)
                Angle = new Vector2(Angle.X, nonForwardYDefault);

            //compute angular speed
            //beware to make the angles the most similar as possible, before making the difference
            //(e.g. 0 and 360 degrees difference is 0 and not 360)
            InstantAngleSpeed = (Angle - new Vector2(MathUtilities.AdjustOrientation(previousMeasurements.Angle.X, Angle.X),
                MathUtilities.AdjustOrientation(previousMeasurements.Angle.Y, Angle.Y)))/deltaTime;
        }

        #endregion

        #region Object Methods

        /// <summary>
        ///     Converts object to string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Body Joint Cinetic Info\nTimestamp: {2}\nJoint: {0}\nReference joint: {1}\nPosition: {3}\nLinear Speed: {4}\n" +
                                 "LinearAcceleration: {5}\nAngle (deg): {6}\nAngular speed (deg): {7}",
                JointType, ReferenceJointType, Time, Position.ToString("+00.00;_00.00;+00.00"), InstantSpeed.ToString("+000.00;_000.00;+000.00"), InstantAcceleration.ToString("+0000.00;_0000.00;+0000.00"), (MathConstants.Rad2Deg*Angle).ToString("+000;_000;+000"),
                (MathConstants.Rad2Deg*InstantAngleSpeed).ToString("+000;_000;+000"));
        }

        #endregion
    }
}
