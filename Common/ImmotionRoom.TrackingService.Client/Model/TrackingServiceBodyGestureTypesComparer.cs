namespace ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model
{
    using System.Collections.Generic;

    public class TrackingServiceBodyGestureTypesComparer : IEqualityComparer<TrackingServiceBodyGestureTypes>
    {
        public static readonly TrackingServiceBodyGestureTypesComparer Instance = new TrackingServiceBodyGestureTypesComparer();

        public bool Equals(TrackingServiceBodyGestureTypes typeA, TrackingServiceBodyGestureTypes typeB)
        {
            return typeA == typeB;
        }

        public int GetHashCode(TrackingServiceBodyGestureTypes type)
        {
            return type.GetHashCode();
        }
    }
}