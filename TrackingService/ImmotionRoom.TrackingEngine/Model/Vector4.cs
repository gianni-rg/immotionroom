namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    public struct Vector4
    {
        //public const float kEpsilon = 1E-05f;

        public float X;

        public float Y;

        public float Z;

        public float W;

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
        //        throw new IndexOutOfRangeException("Invalid Vector4 index!");
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
        //                    throw new IndexOutOfRangeException("Invalid Vector4 index!");
        //                }
        //        }
        //    }
        //}

        //public float magnitude
        //{
        //    get
        //    {
        //        return Mathf.Sqrt(Vector4.Dot(this, this));
        //    }
        //}

        //public Vector4 normalized
        //{
        //    get
        //    {
        //        return Vector4.Normalize(this);
        //    }
        //}

        //public static Vector4 one
        //{
        //    get
        //    {
        //        return new Vector4(1f, 1f, 1f, 1f);
        //    }
        //}

        //public float sqrMagnitude
        //{
        //    get
        //    {
        //        return Vector4.Dot(this, this);
        //    }
        //}

        //public static Vector4 zero
        //{
        //    get
        //    {
        //        return new Vector4(0f, 0f, 0f, 0f);
        //    }
        //}

        public Vector4(float x, float y, float z, float w)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
        }

        //public Vector4(float X, float Y, float Z)
        //{
        //    this.X = X;
        //    this.Y = Y;
        //    this.Z = Z;
        //    this.W = 0f;
        //}

        //public Vector4(float X, float Y)
        //{
        //    this.X = X;
        //    this.Y = Y;
        //    this.Z = 0f;
        //    this.W = 0f;
        //}

        //public static float Distance(Vector4 a, Vector4 b)
        //{
        //    return Vector4.Magnitude(a - b);
        //}

        public static float Dot(Vector4 a, Vector4 b)
        {
            return a.X*b.X + a.Y*b.Y + a.Z*b.Z + a.W*b.W;
        }

        public override bool Equals(object other)
        {
            if (!(other is Vector4))
            {
                return false;
            }
            var vector4 = (Vector4) other;
            return (!X.Equals(vector4.X) || !Y.Equals(vector4.Y) || !Z.Equals(vector4.Z) ? false : W.Equals(vector4.W));
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() << 2 ^ Z.GetHashCode() >> 2 ^ W.GetHashCode() >> 1;
        }

        //public static Vector4 Lerp(Vector4 from, Vector4 to, float t)
        //{
        //    t = Mathf.Clamp01(t);
        //    return new Vector4(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t, from.Z + (to.Z - from.Z) * t, from.W + (to.W - from.W) * t);
        //}

        //public static float Magnitude(Vector4 a)
        //{
        //    return Mathf.Sqrt(Vector4.Dot(a, a));
        //}

        //public static Vector4 Max(Vector4 lhs, Vector4 rhs)
        //{
        //    return new Vector4(Mathf.Max(lhs.X, rhs.X), Mathf.Max(lhs.Y, rhs.Y), Mathf.Max(lhs.Z, rhs.Z), Mathf.Max(lhs.W, rhs.W));
        //}

        //public static Vector4 Min(Vector4 lhs, Vector4 rhs)
        //{
        //    return new Vector4(Mathf.Min(lhs.X, rhs.X), Mathf.Min(lhs.Y, rhs.Y), Mathf.Min(lhs.Z, rhs.Z), Mathf.Min(lhs.W, rhs.W));
        //}

        //public static Vector4 MoveTowards(Vector4 current, Vector4 target, float maxDistanceDelta)
        //{
        //    Vector4 vector4 = target - current;
        //    float single = vector4.magnitude;
        //    if (single <= maxDistanceDelta || single == 0f)
        //    {
        //        return target;
        //    }
        //    return current + ((vector4 / single) * maxDistanceDelta);
        //}

        //public static Vector4 Normalize(Vector4 a)
        //{
        //    float single = Vector4.Magnitude(a);
        //    if (single <= 1E-05f)
        //    {
        //        return Vector4.zero;
        //    }
        //    return a / single;
        //}

        //public void Normalize()
        //{
        //    float single = Vector4.Magnitude(this);
        //    if (single <= 1E-05f)
        //    {
        //        this = Vector4.zero;
        //    }
        //    else
        //    {
        //        this = this / single;
        //    }
        //}

        //public static Vector4 operator +(Vector4 a, Vector4 b)
        //{
        //    return new Vector4(a.X + b.X, a.Y + b.Y, a.Z + b.Z, a.W + b.W);
        //}

        //public static Vector4 operator /(Vector4 a, float d)
        //{
        //    return new Vector4(a.X / d, a.Y / d, a.Z / d, a.W / d);
        //}

        public static bool operator ==(Vector4 lhs, Vector4 rhs)
        {
            return SqrMagnitude(lhs - rhs) < 9.99999944E-11f;
        }

        //public static implicit operator Vector4(Vector3 v)
        //{
        //    return new Vector4(v.X, v.Y, v.Z, 0f);
        //}

        //public static implicit operator Vector3(Vector4 v)
        //{
        //    return new Vector3(v.X, v.Y, v.Z);
        //}

        //public static implicit operator Vector4(Vector2 v)
        //{
        //    return new Vector4(v.X, v.Y, 0f, 0f);
        //}

        //public static implicit operator Vector2(Vector4 v)
        //{
        //    return new Vector2(v.X, v.Y);
        //}

        public static bool operator !=(Vector4 lhs, Vector4 rhs)
        {
            return SqrMagnitude(lhs - rhs) >= 9.99999944E-11f;
        }

        //public static Vector4 operator *(Vector4 a, float d)
        //{
        //    return new Vector4(a.X * d, a.Y * d, a.Z * d, a.W * d);
        //}

        //public static Vector4 operator *(float d, Vector4 a)
        //{
        //    return new Vector4(a.X * d, a.Y * d, a.Z * d, a.W * d);
        //}

        public static Vector4 operator -(Vector4 a, Vector4 b)
        {
            return new Vector4(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
        }

        //public static Vector4 operator -(Vector4 a)
        //{
        //    return new Vector4(-a.X, -a.Y, -a.Z, -a.W);
        //}

        //public static Vector4 Project(Vector4 a, Vector4 b)
        //{
        //    return (b * Vector4.Dot(a, b)) / Vector4.Dot(b, b);
        //}

        //public static Vector4 Scale(Vector4 a, Vector4 b)
        //{
        //    return new Vector4(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
        //}

        //public void Scale(Vector4 scale)
        //{
        //    Vector4 vector4 = this;
        //    vector4.X = vector4.X * scale.X;
        //    Vector4 vector41 = this;
        //    vector41.Y = vector41.Y * scale.Y;
        //    Vector4 vector42 = this;
        //    vector42.Z = vector42.Z * scale.Z;
        //    Vector4 vector43 = this;
        //    vector43.W = vector43.W * scale.W;
        //}

        //public void Set(float new_x, float new_y, float new_z, float new_w)
        //{
        //    this.X = new_x;
        //    this.Y = new_y;
        //    this.Z = new_z;
        //    this.W = new_w;
        //}

        public static float SqrMagnitude(Vector4 a)
        {
            return Dot(a, a);
        }

        //public float SqrMagnitude()
        //{
        //    return Vector4.Dot(this, this);
        //}

        //public override string ToString()
        //{
        //    return Unitystring.Format("({0:F1}, {1:F1}, {2:F1}, {3:F1})", new object[] { this.X, this.Y, this.Z, this.W });
        //}

        //public string ToString(string format)
        //{
        //    return Unitystring.Format("({0}, {1}, {2}, {3})", new object[] { this.X.ToString(format), this.Y.ToString(format), this.Z.ToString(format), this.W.ToString(format) });
        //}
    }
}