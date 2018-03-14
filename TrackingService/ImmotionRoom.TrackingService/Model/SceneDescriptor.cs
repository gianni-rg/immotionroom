namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    using TrackingEngine.Model;

    public class SceneDescriptor
    {
        /// <summary>
        ///     Returns the floor plane, as detected by the master Data Source.
        ///     X,Y,Z represents the normal to the plane, while W is the height of the sensor in meters.
        ///     The plane is valid only when a Tracking Session is running. Otherwise, it will be all zeros.
        /// </summary>
        public Vector4 FloorClipPlane { get; set; }

        /// <summary>
        ///     The room center and size in meters.
        /// </summary>
        public Boundaries StageArea { get; set; }

        /// <summary>
        ///     The game area center and size in meters.
        /// </summary>
        public Boundaries GameArea { get; set; }

        /// <summary>
        ///     The game area inner limits (to start showing warning arrows)
        /// </summary>
        public Vector3 GameAreaInnerLimits { get; set; }

        public override bool Equals(object obj)
        {
            SceneDescriptor sd = obj as SceneDescriptor;

            if (sd == null)
            {
                return false;
            }

            return Equals(sd);
        }

        public bool Equals(SceneDescriptor sd)
        {
            if (sd == null)
            {
                return false;
            }

            if (sd.FloorClipPlane != FloorClipPlane)
            {
                return false;
            }

            if (sd.StageArea.Center != StageArea.Center)
            {
                return false;
            }

            if (sd.GameAreaInnerLimits != GameAreaInnerLimits)
            {
                return false;
            }

            if (sd.GameArea.Center != GameArea.Center)
            {
                return false;
            }

            if (sd.GameArea.Size!= GameArea.Size)
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return FloorClipPlane.GetHashCode() ^ GameArea.Center.GetHashCode() ^ GameArea.Size.GetHashCode() ^ GameAreaInnerLimits.GetHashCode() ^ StageArea.Center.GetHashCode() ^ StageArea.Size.GetHashCode();
        }
    }
}
