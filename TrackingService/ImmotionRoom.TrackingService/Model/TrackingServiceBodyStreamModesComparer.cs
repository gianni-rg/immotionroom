namespace ImmotionAR.ImmotionRoom.TrackingService.Model
{
    using System.Collections.Generic;

    public class TrackingServiceBodyStreamModesComparer : IEqualityComparer<TrackingServiceSceneDataStreamModes>
    {
        public static readonly TrackingServiceBodyStreamModesComparer Instance = new TrackingServiceBodyStreamModesComparer();

        public bool Equals(TrackingServiceSceneDataStreamModes typeA, TrackingServiceSceneDataStreamModes typeB)
        {
            return typeA == typeB;
        }

        public int GetHashCode(TrackingServiceSceneDataStreamModes type)
        {
            return type.GetHashCode();
        }
    }
}