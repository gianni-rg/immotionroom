﻿namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    using System;

    public struct Quaternion
    {
        //public const float kEpsilon = 1E-06f;

        public float X;

        public float Y;

        public float Z;

        public float W;

        //public Vector3 eulerAngles
        //{
        //    get
        //    {
        //        return Quaternion.Internal_ToEulerRad(this) * 57.29578f;
        //    }
        //    set
        //    {
        //        this = Quaternion.Internal_FromEulerRad(value * 0.0174532924f);
        //    }
        //}

        //public static Quaternion identity
        //{
        //    get
        //    {
        //        return new Quaternion(0f, 0f, 0f, 1f);
        //    }
        //}

        //public float this[int index]
        //{
        //    get
        //    {
        //        switch (index)
        //        {
        //            case 0:
        //                {
        //                    return this.X;
        //                }
        //            case 1:
        //                {
        //                    return this.Y;
        //                }
        //            case 2:
        //                {
        //                    return this.Z;
        //                }
        //            case 3:
        //                {
        //                    return this.W;
        //                }
        //        }
        //        throw new IndexOutOfRangeException("Invalid Quaternion index!");
        //    }
        //    set
        //    {
        //        switch (index)
        //        {
        //            case 0:
        //                {
        //                    this.X = value;
        //                    break;
        //                }
        //            case 1:
        //                {
        //                    this.Y = value;
        //                    break;
        //                }
        //            case 2:
        //                {
        //                    this.Z = value;
        //                    break;
        //                }
        //            case 3:
        //                {
        //                    this.W = value;
        //                    break;
        //                }
        //            default:
        //                {
        //                    throw new IndexOutOfRangeException("Invalid Quaternion index!");
        //                }
        //        }
        //    }
        //}

        public Quaternion(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        //public static float Angle(Quaternion a, Quaternion b)
        //{
        //    float single = Quaternion.Dot(a, b);
        //    return Mathf.Acos(Mathf.Min(Mathf.Abs(single), 1f)) * 2f * 57.29578f;
        //}

        /// <summary>
        ///     Creates a Quaternion from its axis angle representation
        /// </summary>
        /// <param name="angle">Rotation angle, in degrees</param>
        /// <param name="axis">Axis of rotation</param>
        /// <returns>Rotation quaternion</returns>
        public static Quaternion AngleAxis(float angle, Vector3 axis)
        {
            //code from http://www.euclideanspace.com/maths/geometry/rotations/conversions/angleToQuaternion/
            angle = (float)(angle * Math.PI / 180.0f);
            var s = (float) Math.Sin(angle/2);

            return new Quaternion(axis.X*s,
                axis.Y*s,
                axis.Z*s,
                (float) Math.Cos(angle/2));
        }

        //[Obsolete("Use Quaternion.AngleAxis instead. This function was deprecated because it uses radians instead of degrees")]
        //public static Quaternion AxisAngle(Vector3 axis, float angle)
        //{
        //    return Quaternion.INTERNAL_CALL_AxisAngle(ref axis, angle);
        //}

        //public static float Dot(Quaternion a, Quaternion b)
        //{
        //    return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        //}

        //public override bool Equals(object other)
        //{
        //    if (!(other is Quaternion))
        //    {
        //        return false;
        //    }
        //    Quaternion quaternion = (Quaternion)other;
        //    return (!this.X.Equals(quaternion.X) || !this.Y.Equals(quaternion.Y) || !this.Z.Equals(quaternion.Z) ? false : this.W.Equals(quaternion.W));
        //}

        //public static Quaternion Euler(float x, float y, float z)
        //{
        //    return Quaternion.Internal_FromEulerRad(new Vector3(x, y, z) * 0.0174532924f);
        //}

        //public static Quaternion Euler(Vector3 euler)
        //{
        //    return Quaternion.Internal_FromEulerRad(euler * 0.0174532924f);
        //}

        //[Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees")]
        //public static Quaternion EulerAngles(float x, float y, float z)
        //{
        //    return Quaternion.Internal_FromEulerRad(new Vector3(x, y, z));
        //}

        //[Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees")]
        //public static Quaternion EulerAngles(Vector3 euler)
        //{
        //    return Quaternion.Internal_FromEulerRad(euler);
        //}

        //[Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees")]
        //public static Quaternion EulerRotation(float x, float y, float z)
        //{
        //    return Quaternion.Internal_FromEulerRad(new Vector3(x, y, z));
        //}

        //[Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees")]
        //public static Quaternion EulerRotation(Vector3 euler)
        //{
        //    return Quaternion.Internal_FromEulerRad(euler);
        //}

        //public static Quaternion FromToRotation(Vector3 fromDirection, Vector3 toDirection)
        //{
        //    return Quaternion.INTERNAL_CALL_FromToRotation(ref fromDirection, ref toDirection);
        //}

        //public override int GetHashCode()
        //{
        //    return this.X.GetHashCode() ^ this.Y.GetHashCode() << 2 ^ this.Z.GetHashCode() >> 2 ^ this.W.GetHashCode() >> 1;
        //}

        //[WrapperlessIcall]
        //private static extern Quaternion INTERNAL_CALL_AngleAxis(float angle, ref Vector3 axis);

        //[WrapperlessIcall]
        //private static extern Quaternion INTERNAL_CALL_AxisAngle(ref Vector3 axis, float angle);

        //[WrapperlessIcall]
        //private static extern Quaternion INTERNAL_CALL_FromToRotation(ref Vector3 fromDirection, ref Vector3 toDirection);

        //[WrapperlessIcall]
        //private static extern Quaternion INTERNAL_CALL_Internal_FromEulerRad(ref Vector3 euler);

        //[WrapperlessIcall]
        //private static extern void INTERNAL_CALL_Internal_ToAxisAngleRad(ref Quaternion q, out Vector3 axis, out float angle);

        //[WrapperlessIcall]
        //private static extern Vector3 INTERNAL_CALL_Internal_ToEulerRad(ref Quaternion rotation);

        //[WrapperlessIcall]
        //private static extern Quaternion INTERNAL_CALL_Inverse(ref Quaternion rotation);

        //[WrapperlessIcall]
        //private static extern Quaternion INTERNAL_CALL_Lerp(ref Quaternion from, ref Quaternion to, float t);

        //[WrapperlessIcall]
        //private static extern Quaternion INTERNAL_CALL_LookRotation(ref Vector3 forward, ref Vector3 upwards);

        //[WrapperlessIcall]
        //private static extern Quaternion INTERNAL_CALL_Slerp(ref Quaternion from, ref Quaternion to, float t);

        //[WrapperlessIcall]
        //private static extern Quaternion INTERNAL_CALL_UnclampedSlerp(ref Quaternion from, ref Quaternion to, float t);

        //private static Quaternion Internal_FromEulerRad(Vector3 euler)
        //{
        //    return Quaternion.INTERNAL_CALL_Internal_FromEulerRad(ref euler);
        //}

        //private static void Internal_ToAxisAngleRad(Quaternion q, out Vector3 axis, out float angle)
        //{
        //    Quaternion.INTERNAL_CALL_Internal_ToAxisAngleRad(ref q, out axis, out angle);
        //}

        //private static Vector3 Internal_ToEulerRad(Quaternion rotation)
        //{
        //    return Quaternion.INTERNAL_CALL_Internal_ToEulerRad(ref rotation);
        //}

        //public static Quaternion Inverse(Quaternion rotation)
        //{
        //    return Quaternion.INTERNAL_CALL_Inverse(ref rotation);
        //}

        //public static Quaternion Lerp(Quaternion from, Quaternion to, float t)
        //{
        //    return Quaternion.INTERNAL_CALL_Lerp(ref from, ref to, t);
        //}

        //public static Quaternion LookRotation(Vector3 forward, [DefaultValue("Vector3.up")] Vector3 upwards)
        //{
        //    return Quaternion.INTERNAL_CALL_LookRotation(ref forward, ref upwards);
        //}

        //[ExcludeFromDocs]
        //public static Quaternion LookRotation(Vector3 forward)
        //{
        //    Vector3 vector3 = Vector3.up;
        //    return Quaternion.INTERNAL_CALL_LookRotation(ref forward, ref vector3);
        //}

        //public static bool operator ==(Quaternion lhs, Quaternion rhs)
        //{
        //    return Quaternion.Dot(lhs, rhs) > 0.999999f;
        //}

        //public static bool operator !=(Quaternion lhs, Quaternion rhs)
        //{
        //    return Quaternion.Dot(lhs, rhs) <= 0.999999f;
        //}

        //public static Quaternion operator *(Quaternion lhs, Quaternion rhs)
        //{
        //    return new Quaternion(lhs.W * rhs.X + lhs.X * rhs.W + lhs.Y * rhs.Z - lhs.Z * rhs.Y, lhs.W * rhs.Y + lhs.Y * rhs.W + lhs.Z * rhs.X - lhs.X * rhs.Z, lhs.W * rhs.Z + lhs.Z * rhs.W + lhs.X * rhs.Y - lhs.Y * rhs.X, lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z);
        //}

        //public static Vector3 operator *(Quaternion rotation, Vector3 point)
        //{
        //    Vector3 vector3 = new Vector3();
        //    float single = rotation.X * 2f;
        //    float single1 = rotation.Y * 2f;
        //    float single2 = rotation.Z * 2f;
        //    float single3 = rotation.X * single;
        //    float single4 = rotation.Y * single1;
        //    float single5 = rotation.Z * single2;
        //    float single6 = rotation.X * single1;
        //    float single7 = rotation.X * single2;
        //    float single8 = rotation.Y * single2;
        //    float single9 = rotation.W * single;
        //    float single10 = rotation.W * single1;
        //    float single11 = rotation.W * single2;
        //    vector3.x = (1f - (single4 + single5)) * point.x + (single6 - single11) * point.y + (single7 + single10) * point.z;
        //    vector3.y = (single6 + single11) * point.x + (1f - (single3 + single5)) * point.y + (single8 - single9) * point.z;
        //    vector3.z = (single7 - single10) * point.x + (single8 + single9) * point.y + (1f - (single3 + single4)) * point.z;
        //    return vector3;
        //}

        //public static Quaternion RotateTowards(Quaternion from, Quaternion to, float maxDegreesDelta)
        //{
        //    float single = Quaternion.Angle(from, to);
        //    if (single == 0f)
        //    {
        //        return to;
        //    }
        //    float single1 = Mathf.Min(1f, maxDegreesDelta / single);
        //    return Quaternion.UnclampedSlerp(from, to, single1);
        //}

        //public void Set(float new_x, float new_y, float new_z, float new_w)
        //{
        //    this.X = new_x;
        //    this.Y = new_y;
        //    this.Z = new_z;
        //    this.W = new_w;
        //}

        //[Obsolete("Use Quaternion.AngleAxis instead. This function was deprecated because it uses radians instead of degrees")]
        //public void SetAxisAngle(Vector3 axis, float angle)
        //{
        //    this = Quaternion.AxisAngle(axis, angle);
        //}

        //[Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees")]
        //public void SetEulerAngles(float x, float y, float z)
        //{
        //    this.SetEulerRotation(new Vector3(x, y, z));
        //}

        //[Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees")]
        //public void SetEulerAngles(Vector3 euler)
        //{
        //    this = Quaternion.EulerRotation(euler);
        //}

        //[Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees")]
        //public void SetEulerRotation(float x, float y, float z)
        //{
        //    this = Quaternion.Internal_FromEulerRad(new Vector3(x, y, z));
        //}

        //[Obsolete("Use Quaternion.Euler instead. This function was deprecated because it uses radians instead of degrees")]
        //public void SetEulerRotation(Vector3 euler)
        //{
        //    this = Quaternion.Internal_FromEulerRad(euler);
        //}

        //public void SetFromToRotation(Vector3 fromDirection, Vector3 toDirection)
        //{
        //    this = Quaternion.FromToRotation(fromDirection, toDirection);
        //}

        //[ExcludeFromDocs]
        //public void SetLookRotation(Vector3 view)
        //{
        //    this.SetLookRotation(view, Vector3.up);
        //}

        //public void SetLookRotation(Vector3 view, [DefaultValue("Vector3.up")] Vector3 up)
        //{
        //    this = Quaternion.LookRotation(view, up);
        //}

        //public static Quaternion Slerp(Quaternion from, Quaternion to, float t)
        //{
        //    return Quaternion.INTERNAL_CALL_Slerp(ref from, ref to, t);
        //}

        //public void ToAngleAxis(out float angle, out Vector3 axis)
        //{
        //    Quaternion.Internal_ToAxisAngleRad(this, out axis, out angle);
        //    angle = angle * 57.29578f;
        //}

        //[Obsolete("Use Quaternion.ToAngleAxis instead. This function was deprecated because it uses radians instead of degrees")]
        //public void ToAxisAngle(out Vector3 axis, out float angle)
        //{
        //    Quaternion.Internal_ToAxisAngleRad(this, out axis, out angle);
        //}

        //[Obsolete("Use Quaternion.eulerAngles instead. This function was deprecated because it uses radians instead of degrees")]
        //public Vector3 ToEuler()
        //{
        //    return Quaternion.Internal_ToEulerRad(this);
        //}

        //[Obsolete("Use Quaternion.eulerAngles instead. This function was deprecated because it uses radians instead of degrees")]
        //public static Vector3 ToEulerAngles(Quaternion rotation)
        //{
        //    return Quaternion.Internal_ToEulerRad(rotation);
        //}

        //[Obsolete("Use Quaternion.eulerAngles instead. This function was deprecated because it uses radians instead of degrees")]
        //public Vector3 ToEulerAngles()
        //{
        //    return Quaternion.Internal_ToEulerRad(this);
        //}

        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", new object[] {X, Y, Z, W});
        }

        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2}, {3})", new object[] {X.ToString(format), Y.ToString(format), Z.ToString(format), W.ToString(format)});
        }

        //private static Quaternion UnclampedSlerp(Quaternion from, Quaternion to, float t)
        //{
        //    return Quaternion.INTERNAL_CALL_UnclampedSlerp(ref from, ref to, t);
        //}
    }
}