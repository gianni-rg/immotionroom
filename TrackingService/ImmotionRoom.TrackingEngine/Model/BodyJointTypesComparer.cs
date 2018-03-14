namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    using System.Collections.Generic;
    
    internal class BodyJointTypesComparer : IEqualityComparer<BodyJointTypes>
    {
        public static readonly BodyJointTypesComparer Instance = new BodyJointTypesComparer();

        public bool Equals(BodyJointTypes typeA, BodyJointTypes typeB)
        {
            return typeA == typeB;
        }

        public int GetHashCode(BodyJointTypes type)
        {
            return type.GetHashCode();
        }
    }
}