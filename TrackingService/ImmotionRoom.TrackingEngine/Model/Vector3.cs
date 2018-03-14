namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    using System;

    public struct Vector3
    {

        //public const float kEpsilon = 1E-05f;

        public float X;

        public float Y;

        public float Z;

        //public static Vector3 back
        //{
        //    get
        //    {
        //        return new Vector3(0f, 0f, -1f);
        //    }
        //}

        //public static Vector3 down
        //{
        //    get
        //    {
        //        return new Vector3(0f, -1f, 0f);
        //    }
        //}

        public static Vector3 Forward
        {
            get { return new Vector3(0f, 0f, 1f); }
        }


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
        //        }
        //        throw new IndexOutOfRangeException("Invalid Vector3 index!");
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
        //            default:
        //                {
        //                    throw new IndexOutOfRangeException("Invalid Vector3 index!");
        //                }
        //        }
        //    }
        //}

        //public static Vector3 left
        //{
        //    get
        //    {
        //        return new Vector3(-1f, 0f, 0f);
        //    }
        //}

        public float Magnitude
        {
            get { return (float) Math.Sqrt(X*X + Y*Y + Z*Z); }
        }

        //public Vector3 normalized
        //{
        //    get
        //    {
        //        return Vector3.Normalize(this);
        //    }
        //}

        public static Vector3 One
        {
            get { return new Vector3(1f, 1f, 1f); }
        }

        //public static Vector3 right
        //{
        //    get
        //    {
        //        return new Vector3(1f, 0f, 0f);
        //    }
        //}

        //public float sqrMagnitude
        //{
        //    get
        //    {
        //        return this.X * this.X + this.Y * this.Y + this.Z * this.Z;
        //    }
        //}

        public static Vector3 Up
        {
            get { return new Vector3(0f, 1f, 0f); }
        }

        public static Vector3 Zero
        {
            get { return new Vector3(0f, 0f, 0f); }
        }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(float x, float y)
        {
            this.X = x;
            this.Y = y;
            this.Z = 0f;
        }

        //public static float Angle(Vector3 from, Vector3 to)
        //{
        //    return Mathf.Acos(Mathf.Clamp(Vector3.Dot(from.normalized, to.normalized), -1f, 1f)) * 57.29578f;
        //}

        //public static Vector3 ClampMagnitude(Vector3 vector, float maxLength)
        //{
        //    if (vector.sqrMagnitude <= maxLength * maxLength)
        //    {
        //        return vector;
        //    }
        //    return vector.normalized * maxLength;
        //}

        public static Vector3 Cross(Vector3 lhs, Vector3 rhs)
        {
            return new Vector3(lhs.Y * rhs.Z - lhs.Z * rhs.Y, lhs.Z * rhs.X - lhs.X * rhs.Z, lhs.X * rhs.Y - lhs.Y * rhs.X);
        }

        public static float Distance(Vector3 a, Vector3 b)
        {
            var vector3 = new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
            return (float) Math.Sqrt(vector3.X*vector3.X + vector3.Y*vector3.Y + vector3.Z*vector3.Z);
        }

        //public static float Dot(Vector3 lhs, Vector3 rhs)
        //{
        //    return lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z;
        //}

        public override bool Equals(object other)
        {
            if (!(other is Vector3))
            {
                return false;
            }
            var vector3 = (Vector3) other;
            return (!X.Equals(vector3.X) || !Y.Equals(vector3.Y) ? false : Z.Equals(vector3.Z));
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2;
        }

        //public static Vector3 Lerp(Vector3 from, Vector3 to, float t)
        //{
        //    t = Mathf.Clamp01(t);
        //    return new Vector3(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t, from.Z + (to.Z - from.Z) * t);
        //}

        //public static float Magnitude(Vector3 a)
        //{
        //    return Mathf.Sqrt(a.X * a.X + a.Y * a.Y + a.Z * a.Z);
        //}

        //public static Vector3 Max(Vector3 lhs, Vector3 rhs)
        //{
        //    return new Vector3(Mathf.Max(lhs.X, rhs.X), Mathf.Max(lhs.Y, rhs.Y), Mathf.Max(lhs.Z, rhs.Z));
        //}

        //public static Vector3 Min(Vector3 lhs, Vector3 rhs)
        //{
        //    return new Vector3(Mathf.Min(lhs.X, rhs.X), Mathf.Min(lhs.Y, rhs.Y), Mathf.Min(lhs.Z, rhs.Z));
        //}

        //public static Vector3 MoveTowards(Vector3 current, Vector3 target, float maxDistanceDelta)
        //{
        //    Vector3 vector3 = target - current;
        //    float single = vector3.magnitude;
        //    if (single <= maxDistanceDelta || single == 0f)
        //    {
        //        return target;
        //    }
        //    return current + ((vector3 / single) * maxDistanceDelta);
        //}

        //public static Vector3 Normalize(Vector3 value)
        //{
        //    float single = Vector3.Magnitude(value);
        //    if (single <= 1E-05f)
        //    {
        //        return Vector3.zero;
        //    }
        //    return value / single;
        //}

        //public void Normalize()
        //{
        //    float single = Vector3.Magnitude(this);
        //    if (single <= 1E-05f)
        //    {
        //        this = Vector3.zero;
        //    }
        //    else
        //    {
        //        this = this / single;
        //    }
        //}

        public static Vector3 operator +(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Vector3 operator /(Vector3 a, float d)
        {
            return new Vector3(a.X/d, a.Y/d, a.Z/d);
        }

        public static bool operator ==(Vector3 lhs, Vector3 rhs)
        {
            return SqrMagnitude(lhs - rhs) < 9.99999944E-11f;
        }

        public static bool operator !=(Vector3 lhs, Vector3 rhs)
        {
            return SqrMagnitude(lhs - rhs) >= 9.99999944E-11f;
        }

        public static Vector3 operator *(Vector3 a, float d)
        {
            return new Vector3(a.X*d, a.Y*d, a.Z*d);
        }

        public static Vector3 operator *(float d, Vector3 a)
        {
            return new Vector3(a.X*d, a.Y*d, a.Z*d);
        }

        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static Vector3 operator -(Vector3 a)
        {
            return new Vector3(-a.X, -a.Y, -a.Z);
        }

        //public static Vector3 Project(Vector3 vector, Vector3 onNormal)
        //{
        //    float single = Vector3.Dot(onNormal, onNormal);
        //    if (single < Mathf.Epsilon)
        //    {
        //        return Vector3.zero;
        //    }
        //    return (onNormal * Vector3.Dot(vector, onNormal)) / single;
        //}

        //public static Vector3 ProjectOnPlane(Vector3 vector, Vector3 planeNormal)
        //{
        //    return vector - Vector3.Project(vector, planeNormal);
        //}

        //public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal)
        //{
        //    return (-2f * Vector3.Dot(inNormal, inDirection) * inNormal) + inDirection;
        //}

        //public static Vector3 Scale(Vector3 a, Vector3 b)
        //{
        //    return new Vector3(a.X * b.X, a.Y * b.Y, a.Z * b.Z);
        //}

        //public void Scale(Vector3 scale)
        //{
        //    Vector3 vector3 = this;
        //    vector3.X = vector3.X * scale.X;
        //    Vector3 vector31 = this;
        //    vector31.Y = vector31.Y * scale.Y;
        //    Vector3 vector32 = this;
        //    vector32.Z = vector32.Z * scale.Z;
        //}

        //public void Set(float new_x, float new_y, float new_z)
        //{
        //    this.X = new_x;
        //    this.Y = new_y;
        //    this.Z = new_z;
        //}

        //public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, float maxSpeed)
        //{
        //    float single = Time.deltaTime;
        //    return Vector3.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, single);
        //}

        //public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime)
        //{
        //    float single = Time.deltaTime;
        //    return Vector3.SmoothDamp(current, target, ref currentVelocity, smoothTime, float.PositiveInfinity, single);
        //}

        //public static Vector3 SmoothDamp(Vector3 current, Vector3 target, ref Vector3 currentVelocity, float smoothTime, [DefaultValue("Mathf.Infinity")] float maxSpeed, [DefaultValue("Time.deltaTime")] float deltaTime)
        //{
        //    smoothTime = Mathf.Max(0.0001f, smoothTime);
        //    float single = 2f / smoothTime;
        //    float single1 = single * deltaTime;
        //    float single2 = 1f / (1f + single1 + 0.48f * single1 * single1 + 0.235f * single1 * single1 * single1);
        //    Vector3 vector3 = current - target;
        //    Vector3 vector31 = target;
        //    vector3 = Vector3.ClampMagnitude(vector3, maxSpeed * smoothTime);
        //    target = current - vector3;
        //    Vector3 vector32 = (currentVelocity + (single * vector3)) * deltaTime;
        //    currentVelocity = (currentVelocity - (single * vector32)) * single2;
        //    Vector3 vector33 = target + ((vector3 + vector32) * single2);
        //    if (Vector3.Dot(vector31 - current, vector33 - vector31) > 0f)
        //    {
        //        vector33 = vector31;
        //        currentVelocity = (vector33 - vector31) / deltaTime;
        //    }
        //    return vector33;
        //}

        public static float SqrMagnitude(Vector3 a)
        {
            return a.X*a.X + a.Y*a.Y + a.Z*a.Z;
        }

        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1}, {2:F1})", new object[] {X, Y, Z});
        }

        public string ToString(string format)
        {
            return string.Format("({0}, {1}, {2})", new object[] {X.ToString(format), Y.ToString(format), Z.ToString(format)});
        }
    }
}