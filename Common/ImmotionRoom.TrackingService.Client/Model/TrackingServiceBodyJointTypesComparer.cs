namespace ImmotionAR.ImmotionRoom.TrackingService.DataClient.Model
{
    using System.Collections.Generic;

    public class TrackingServiceBodyJointTypesComparer : IEqualityComparer<TrackingServiceBodyJointTypes>
    {
        public static readonly TrackingServiceBodyJointTypesComparer Instance = new TrackingServiceBodyJointTypesComparer();

        public bool Equals(TrackingServiceBodyJointTypes typeA, TrackingServiceBodyJointTypes typeB)
        {
            return typeA == typeB;
        }

        public int GetHashCode(TrackingServiceBodyJointTypes type)
        {
            return type.GetHashCode();
        }
    }
}