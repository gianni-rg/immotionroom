namespace ImmotionAR.ImmotionRoom.Protocol
{
    using System.Collections.Generic;

    // Dictionary Enum Optimization
    // See: http://www.codeproject.com/Articles/33528/Accelerating-Enum-Based-Dictionaries-with-Generic
    // See: http://www.somasim.com/blog/2015/08/c-performance-tips-for-unity-part-2-structs-and-enums/
    // See: http://stackoverflow.com/questions/7143948/efficiency-of-using-iequalitycomparer-in-dictionary-vs-hashcode-and-equals

    public class SensorBodyJointTypesComparer : IEqualityComparer<SensorBodyJointTypes>
    {
        public static readonly SensorBodyJointTypesComparer Instance = new SensorBodyJointTypesComparer();

        public bool Equals(SensorBodyJointTypes typeA, SensorBodyJointTypes typeB)
        {
            return typeA == typeB;
        }

        public int GetHashCode(SensorBodyJointTypes type)
        {
            return type.GetHashCode();
        }
    }
}