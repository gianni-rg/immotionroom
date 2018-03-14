namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    using System;

    public struct Vector2
    {
        //public const float kEpsilon = 1E-05f;

        public float X;

        public float Y;

        //public static Vector2 down
        //{
        //    get
        //    {
        //        return new Vector2(0f, -1f);
        //    }
        //}

        //public float this[int index]
        //{
        //    get
        //    {
        //        int num = index;
        //        if (num == 0)
        //        {
        //            return this.X;
        //        }
        //        if (num != 1)
        //        {
        //            throw new IndexOutOfRangeException("Invalid Vector2 index!");
        //        }
        //        return this.Y;
        //    }
        //    set
        //    {
        //        int num = index;
        //        if (num == 0)
        //        {
        //            this.X = value;
        //        }
        //        else
        //        {
        //            if (num != 1)
        //            {
        //                throw new IndexOutOfRangeException("Invalid Vector2 index!");
        //            }
        //            this.Y = value;
        //        }
        //    }
        //}

        //public static Vector2 left
        //{
        //    get
        //    {
        //        return new Vector2(-1f, 0f);
        //    }
        //}

        public float Magnitude
        {
            get { return (float) Math.Sqrt(X*X + Y*Y); }
        }

        public Vector2 Normalized
        {
            get
            {
                Vector2 vector2 = new Vector2(this.X, this.Y);
                vector2.Normalize();
                return vector2;
            }
        }

        //public static Vector2 one
        //{
        //    get
        //    {
        //        return new Vector2(1f, 1f);
        //    }
        //}

        //public static Vector2 right
        //{
        //    get
        //    {
        //        return new Vector2(1f, 0f);
        //    }
        //}

        public float SqrMagnitude
        {
            get { return X*X + Y*Y; }
        }

        //public static Vector2 up
        //{
        //    get
        //    {
        //        return new Vector2(0f, 1f);
        //    }
        //}

        public static Vector2 Zero
        {
            get
            {
                return new Vector2(0f, 0f);
            }
        }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static float Angle(Vector2 from, Vector2 to)
        {
            return (float)Math.Acos(MathUtilities.Clamp(Vector2.Dot(from.Normalized, to.Normalized), -1f, 1f)) * 57.29578f;
        }

        //public static Vector2 ClampMagnitude(Vector2 vector, float maxLength)
        //{
        //    if (vector.SqrMagnitude <= maxLength * maxLength)
        //    {
        //        return vector;
        //    }
        //    return vector.normalized * maxLength;
        //}

        public static float Distance(Vector2 a, Vector2 b)
        {
            return (a - b).Magnitude;
        }

        public static float Dot(Vector2 lhs, Vector2 rhs)
        {
            return lhs.X * rhs.X + lhs.Y * rhs.Y;
        }

        //public override bool Equals(object other)
        //{
        //    if (!(other is Vector2))
        //    {
        //        return false;
        //    }
        //    Vector2 vector2 = (Vector2)other;
        //    return (!this.X.Equals(vector2.X) ? false : this.Y.Equals(vector2.Y));
        //}

        //public override int GetHashCode()
        //{
        //    return this.X.GetHashCode() ^ this.Y.GetHashCode() << 2;
        //}

        //public static Vector2 Lerp(Vector2 from, Vector2 to, float t)
        //{
        //    t = Mathf.Clamp01(t);
        //    return new Vector2(from.X + (to.X - from.X) * t, from.Y + (to.Y - from.Y) * t);
        //}

        //public static Vector2 Max(Vector2 lhs, Vector2 rhs)
        //{
        //    return new Vector2(Mathf.Max(lhs.X, rhs.X), Mathf.Max(lhs.Y, rhs.Y));
        //}

        //public static Vector2 Min(Vector2 lhs, Vector2 rhs)
        //{
        //    return new Vector2(Mathf.Min(lhs.X, rhs.X), Mathf.Min(lhs.Y, rhs.Y));
        //}

        //public static Vector2 MoveTowards(Vector2 current, Vector2 target, float maxDistanceDelta)
        //{
        //    Vector2 vector2 = target - current;
        //    float single = vector2.magnitude;
        //    if (single <= maxDistanceDelta || single == 0f)
        //    {
        //        return target;
        //    }
        //    return current + ((vector2 / single) * maxDistanceDelta);
        //}

        public void Normalize()
        {
            float single = this.Magnitude;
            if (single <= 1E-05f)
            {
                this = Vector2.Zero;
            }
            else
            {
                this = this / single;
            }
        }

        public static Vector2 operator +(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X + b.X, a.Y + b.Y);
        }

        public static Vector2 operator /(Vector2 a, float d)
        {
            return new Vector2(a.X / d, a.Y / d);
        }

        //public static bool operator ==(Vector2 lhs, Vector2 rhs)
        //{
        //    return Vector2.SqrMagnitude(lhs - rhs) < 9.99999944E-11f;
        //}

        //public static implicit operator Vector2(Vector3 v)
        //{
        //    return new Vector2(v.X, v.Y);
        //}

        //public static implicit operator Vector3(Vector2 v)
        //{
        //    return new Vector3(v.X, v.Y, 0f);
        //}

        //public static bool operator !=(Vector2 lhs, Vector2 rhs)
        //{
        //    return Vector2.SqrMagnitude(lhs - rhs) >= 9.99999944E-11f;
        //}

        //public static Vector2 operator *(Vector2 a, float d)
        //{
        //    return new Vector2(a.X * d, a.Y * d);
        //}

        public static Vector2 operator *(float d, Vector2 a)
        {
            return new Vector2(a.X * d, a.Y * d);
        }

        public static Vector2 operator -(Vector2 a, Vector2 b)
        {
            return new Vector2(a.X - b.X, a.Y - b.Y);
        }

        //public static Vector2 operator -(Vector2 a)
        //{
        //    return new Vector2(-a.X, -a.Y);
        //}

        //public static Vector2 Reflect(Vector2 inDirection, Vector2 inNormal)
        //{
        //    return (-2f * Vector2.Dot(inNormal, inDirection) * inNormal) + inDirection;
        //}

        //public static Vector2 Scale(Vector2 a, Vector2 b)
        //{
        //    return new Vector2(a.X * b.X, a.Y * b.Y);
        //}

        //public void Scale(Vector2 scale)
        //{
        //    Vector2 vector2 = this;
        //    vector2.X = vector2.X * scale.X;
        //    Vector2 vector21 = this;
        //    vector21.Y = vector21.Y * scale.Y;
        //}

        //public void Set(float new_x, float new_y)
        //{
        //    this.X = new_x;
        //    this.Y = new_y;
        //}

        //[ExcludeFromDocs]
        //public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime, float maxSpeed)
        //{
        //    float single = Time.deltaTime;
        //    return Vector2.SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, single);
        //}

        //[ExcludeFromDocs]
        //public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime)
        //{
        //    float single = Time.deltaTime;
        //    return Vector2.SmoothDamp(current, target, ref currentVelocity, smoothTime, float.PositiveInfinity, single);
        //}

        //public static Vector2 SmoothDamp(Vector2 current, Vector2 target, ref Vector2 currentVelocity, float smoothTime, [DefaultValue("Mathf.Infinity")] float maxSpeed, [DefaultValue("Time.deltaTime")] float deltaTime)
        //{
        //    smoothTime = Mathf.Max(0.0001f, smoothTime);
        //    float single = 2f / smoothTime;
        //    float single1 = single * deltaTime;
        //    float single2 = 1f / (1f + single1 + 0.48f * single1 * single1 + 0.235f * single1 * single1 * single1);
        //    Vector2 vector2 = current - target;
        //    Vector2 vector21 = target;
        //    vector2 = Vector2.ClampMagnitude(vector2, maxSpeed * smoothTime);
        //    target = current - vector2;
        //    Vector2 vector22 = (currentVelocity + (single * vector2)) * deltaTime;
        //    currentVelocity = (currentVelocity - (single * vector22)) * single2;
        //    Vector2 vector23 = target + ((vector2 + vector22) * single2);
        //    if (Vector2.Dot(vector21 - current, vector23 - vector21) > 0f)
        //    {
        //        vector23 = vector21;
        //        currentVelocity = (vector23 - vector21) / deltaTime;
        //    }
        //    return vector23;
        //}

        //public static float SqrMagnitude(Vector2 a)
        //{
        //    return a.X * a.X + a.Y * a.Y;
        //}

        //public float SqrMagnitude()
        //{
        //    return this.X * this.X + this.Y * this.Y;
        //}

        public override string ToString()
        {
            return string.Format("({0:F1}, {1:F1})", new object[] {X, Y});
        }

        public string ToString(string format)
        {
            return string.Format("({0}, {1})", new object[] {X.ToString(format), Y.ToString(format)});
        }

        public static implicit operator Vector2(Vector3 v)
        {
            return new Vector2(v.X, v.Y);
        }

        public static implicit operator Vector3(Vector2 v)
        {
            return new Vector3(v.X, v.Y, 0f);
        }
    }
}