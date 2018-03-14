namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    using System.Collections.Generic;

    public class BodyGestureTypesComparer : IEqualityComparer<BodyGestureTypes>
    {
        public static readonly BodyGestureTypesComparer Instance = new BodyGestureTypesComparer();

        public bool Equals(BodyGestureTypes typeA, BodyGestureTypes typeB)
        {
            return typeA == typeB;
        }

        public int GetHashCode(BodyGestureTypes type)
        {
            return type.GetHashCode();
        }
    }
}