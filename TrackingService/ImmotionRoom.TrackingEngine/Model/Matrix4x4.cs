namespace ImmotionAR.ImmotionRoom.TrackingEngine.Model
{
    using System;
    using Meta.Numerics.Matrices;

    public struct Matrix4x4
    {    
        public static readonly Matrix4x4 Identity = new Matrix4x4
        {
            M00 = 1f,
            M01 = 0f,
            M02 = 0f,
            M03 = 0f,
            M10 = 0f,
            M11 = 1f,
            M12 = 0f,
            M13 = 0f,
            M20 = 0f,
            M21 = 0f,
            M22 = 1f,
            M23 = 0f,
            M30 = 0f,
            M31 = 0f,
            M32 = 0f,
            M33 = 1f,
            IsIdentity = true,
        };

        public float M00;

        public float M10;

        public float M20;

        public float M30;

        public float M01;

        public float M11;

        public float M21;

        public float M31;

        public float M02;

        public float M12;

        public float M22;

        public float M32;

        public float M03;

        public float M13;

        public float M23;

        public float M33;
        
        //public static Matrix4x4 Identity
        //{
        //    get
        //    {
        //        return m_Identity;
        //    }
        //}

        // GIANNI TODO: better handling this case.. should remove JSON stuff from here...
        [Newtonsoft.Json.JsonIgnore]
        public Matrix4x4 Inverse
        {
            get
            {
                var sourceMatrixAsArray = new double[4, 4]
                {
                    {M00, M01, M02, M03},
                    {M10, M11, M12, M13},
                    {M20, M21, M22, M23},
                    {M30, M31, M32, M33}
                };

                var metaMatrix = UnityToMetaMat(this);
                
                var invertedMetaMatrix = metaMatrix.Inverse();

                return new Matrix4x4
                {
                    M00 = (float)invertedMetaMatrix[0, 0],
                    M01 = (float)invertedMetaMatrix[0, 1],
                    M02 = (float)invertedMetaMatrix[0, 2],
                    M03 = (float)invertedMetaMatrix[0, 3],
                    M10 = (float)invertedMetaMatrix[1, 0],
                    M11 = (float)invertedMetaMatrix[1, 1],
                    M12 = (float)invertedMetaMatrix[1, 2],
                    M13 = (float)invertedMetaMatrix[1, 3],
                    M20 = (float)invertedMetaMatrix[2, 0],
                    M21 = (float)invertedMetaMatrix[2, 1],
                    M22 = (float)invertedMetaMatrix[2, 2],
                    M23 = (float)invertedMetaMatrix[2, 3],
                    M30 = (float)invertedMetaMatrix[3, 0],
                    M31 = (float)invertedMetaMatrix[3, 1],
                    M32 = (float)invertedMetaMatrix[3, 2],
                    M33 = (float)invertedMetaMatrix[3, 3],
                };
            }
        }

        public bool IsIdentity;

        public float this[int row, int column]
        {
            get { return this[row + column*4]; }
            set { this[row + column*4] = value; }
        }

        public float this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                    {
                        return M00;
                    }
                    case 1:
                    {
                        return M10;
                    }
                    case 2:
                    {
                        return M20;
                    }
                    case 3:
                    {
                        return M30;
                    }
                    case 4:
                    {
                        return M01;
                    }
                    case 5:
                    {
                        return M11;
                    }
                    case 6:
                    {
                        return M21;
                    }
                    case 7:
                    {
                        return M31;
                    }
                    case 8:
                    {
                        return M02;
                    }
                    case 9:
                    {
                        return M12;
                    }
                    case 10:
                    {
                        return M22;
                    }
                    case 11:
                    {
                        return M32;
                    }
                    case 12:
                    {
                        return M03;
                    }
                    case 13:
                    {
                        return M13;
                    }
                    case 14:
                    {
                        return M23;
                    }
                    case 15:
                    {
                        return M33;
                    }
                }
                throw new IndexOutOfRangeException("Invalid matrix index!");
            }
            set
            {
                switch (index)
                {
                    case 0:
                    {
                        M00 = value;
                        break;
                    }
                    case 1:
                    {
                        M10 = value;
                        break;
                    }
                    case 2:
                    {
                        M20 = value;
                        break;
                    }
                    case 3:
                    {
                        M30 = value;
                        break;
                    }
                    case 4:
                    {
                        M01 = value;
                        break;
                    }
                    case 5:
                    {
                        M11 = value;
                        break;
                    }
                    case 6:
                    {
                        M21 = value;
                        break;
                    }
                    case 7:
                    {
                        M31 = value;
                        break;
                    }
                    case 8:
                    {
                        M02 = value;
                        break;
                    }
                    case 9:
                    {
                        M12 = value;
                        break;
                    }
                    case 10:
                    {
                        M22 = value;
                        break;
                    }
                    case 11:
                    {
                        M32 = value;
                        break;
                    }
                    case 12:
                    {
                        M03 = value;
                        break;
                    }
                    case 13:
                    {
                        M13 = value;
                        break;
                    }
                    case 14:
                    {
                        M23 = value;
                        break;
                    }
                    case 15:
                    {
                        M33 = value;
                        break;
                    }
                    default:
                    {
                        throw new IndexOutOfRangeException("Invalid matrix index!");
                    }
                }
            }
        }

        ///// <summary>
        ///// Public constructor.
        ///// Initializes matrix to zero
        ///// </summary>
        //public Matrix4x4()
        //{
        //    M00 = M01 = M02 = M03 = M10 = M11 = M12 = M13 = M20 = M21 = M22 = M23 = M30 = M31 = M32 = M33 = 0;  
        //}
        
        /// <summary>
        ///     Constructor with full initialization
        /// </summary>
        /// <param name="M00">First row first column data element</param>
        /// <param name="M01">First row second column data element</param>
        /// <param name="M02">First row third column data element</param>
        /// <param name="M03">First row fourth column data element</param>
        /// <param name="M10">Second row first column data element</param>
        /// <param name="M11">Second row second column data element</param>
        /// <param name="M12">Second row third column data element</param>
        /// <param name="M13">Second row fourth column data element</param>
        /// <param name="M20">Third row first column data element</param>
        /// <param name="M21">Third row second column data element</param>
        /// <param name="M22">Third row third column data element</param>
        /// <param name="M23">Third row fourth column data element</param>
        /// <param name="M30">Fourth row first column data element</param>
        /// <param name="M31">Fourth row second column data element</param>
        /// <param name="M32">Fourth row third column data element</param>
        /// <param name="M33">Fourth row fourth column data element</param>
        public Matrix4x4(float m00, float m01, float m02, float m03,
            float m10, float m11, float m12, float m13,
            float m20, float m21, float m22, float m23,
            float m30, float m31, float m32, float m33)
        {
            M00 = m00;
            M01 = m01;
            M02 = m02;
            M03 = m03;

            M10 = m10;
            M11 = m11;
            M12 = m12;
            M13 = m13;

            M20 = m20;
            M21 = m21;
            M22 = m22;
            M23 = m23;

            M30 = m30;
            M31 = m31;
            M32 = m32;
            M33 = m33;

            IsIdentity = false;
        }

        //public Matrix4x4 transpose
        //{
        //    get
        //    {
        //        return Matrix4x4.Transpose(this);
        //    }
        //}

        //public static Matrix4x4 zero
        //{
        //    get
        //    {
        //        Matrix4x4 matrix4x4 = new Matrix4x4()
        //        {
        //            M00 = 0f,
        //            M01 = 0f,
        //            M02 = 0f,
        //            M03 = 0f,
        //            M10 = 0f,
        //            M11 = 0f,
        //            M12 = 0f,
        //            M13 = 0f,
        //            M20 = 0f,
        //            M21 = 0f,
        //            M22 = 0f,
        //            M23 = 0f,
        //            M30 = 0f,
        //            M31 = 0f,
        //            M32 = 0f,
        //            M33 = 0f
        //        };
        //        return matrix4x4;
        //    }
        //}

        public override bool Equals(object other)
        {
            bool flag;
            if (!(other is Matrix4x4))
            {
                return false;
            }

            var matrix4x4 = (Matrix4x4) other;
            if (!GetColumn(0).Equals(matrix4x4.GetColumn(0)) || !GetColumn(1).Equals(matrix4x4.GetColumn(1)) || !GetColumn(2).Equals(matrix4x4.GetColumn(2)))
            {
                flag = false;
            }
            else
            {
                Vector4 column = GetColumn(3);
                flag = column.Equals(matrix4x4.GetColumn(3));
            }

            return flag;
        }

        public Vector4 GetColumn(int i)
        {
            return new Vector4(this[0, i], this[1, i], this[2, i], this[3, i]);
        }

        public override int GetHashCode()
        {
            int hashCode = GetColumn(0).GetHashCode();
            Vector4 column = GetColumn(1);
            Vector4 vector4 = GetColumn(2);
            Vector4 column1 = GetColumn(3);
            return hashCode ^ column.GetHashCode() << 2 ^ vector4.GetHashCode() >> 2 ^ column1.GetHashCode() >> 1;
        }

        public Vector4 GetRow(int i)
        {
            return new Vector4(this[i, 0], this[i, 1], this[i, 2], this[i, 3]);
        }


        public Vector3 MultiplyPoint(Vector3 v)
        {
            var vector3 = new Vector3();
            vector3.X = M00*v.X + M01*v.Y + M02*v.Z + M03;
            vector3.Y = M10*v.X + M11*v.Y + M12*v.Z + M13;
            vector3.Z = M20*v.X + M21*v.Y + M22*v.Z + M23;
            float single = M30*v.X + M31*v.Y + M32*v.Z + M33;
            single = 1f/single;
            vector3.X = vector3.X*single;
            vector3.Y = vector3.Y*single;
            vector3.Z = vector3.Z*single;
            return vector3;
        }

        public Vector3 MultiplyPoint3x4(Vector3 v)
        {
            var vector3 = new Vector3();

            vector3.X = M00*v.X + M01*v.Y + M02*v.Z + M03;
            vector3.Y = M10*v.X + M11*v.Y + M12*v.Z + M13;
            vector3.Z = M20*v.X + M21*v.Y + M22*v.Z + M23;

            return vector3;
        }

        //public Vector3 MultiplyVector(Vector3 v)
        //{
        //    Vector3 vector3 = new Vector3();
        //    vector3.X = this.M00 * v.X + this.M01 * v.Y + this.M02 * v.Z;
        //    vector3.Y = this.M10 * v.X + this.M11 * v.Y + this.M12 * v.Z;
        //    vector3.Z = this.M20 * v.X + this.M21 * v.Y + this.M22 * v.Z;
        //    return vector3;
        //}

        public static bool operator ==(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return (!(lhs.GetColumn(0) == rhs.GetColumn(0)) || !(lhs.GetColumn(1) == rhs.GetColumn(1)) || !(lhs.GetColumn(2) == rhs.GetColumn(2)) ? false : lhs.GetColumn(3) == rhs.GetColumn(3));
        }

        public static bool operator !=(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            return !(lhs == rhs);
        }

        public static Matrix4x4 operator *(Matrix4x4 lhs, Matrix4x4 rhs)
        {
            var matrix4x4 = new Matrix4x4
            {
                M00 = lhs.M00*rhs.M00 + lhs.M01*rhs.M10 + lhs.M02*rhs.M20 + lhs.M03*rhs.M30,
                M01 = lhs.M00*rhs.M01 + lhs.M01*rhs.M11 + lhs.M02*rhs.M21 + lhs.M03*rhs.M31,
                M02 = lhs.M00*rhs.M02 + lhs.M01*rhs.M12 + lhs.M02*rhs.M22 + lhs.M03*rhs.M32,
                M03 = lhs.M00*rhs.M03 + lhs.M01*rhs.M13 + lhs.M02*rhs.M23 + lhs.M03*rhs.M33,
                M10 = lhs.M10*rhs.M00 + lhs.M11*rhs.M10 + lhs.M12*rhs.M20 + lhs.M13*rhs.M30,
                M11 = lhs.M10*rhs.M01 + lhs.M11*rhs.M11 + lhs.M12*rhs.M21 + lhs.M13*rhs.M31,
                M12 = lhs.M10*rhs.M02 + lhs.M11*rhs.M12 + lhs.M12*rhs.M22 + lhs.M13*rhs.M32,
                M13 = lhs.M10*rhs.M03 + lhs.M11*rhs.M13 + lhs.M12*rhs.M23 + lhs.M13*rhs.M33,
                M20 = lhs.M20*rhs.M00 + lhs.M21*rhs.M10 + lhs.M22*rhs.M20 + lhs.M23*rhs.M30,
                M21 = lhs.M20*rhs.M01 + lhs.M21*rhs.M11 + lhs.M22*rhs.M21 + lhs.M23*rhs.M31,
                M22 = lhs.M20*rhs.M02 + lhs.M21*rhs.M12 + lhs.M22*rhs.M22 + lhs.M23*rhs.M32,
                M23 = lhs.M20*rhs.M03 + lhs.M21*rhs.M13 + lhs.M22*rhs.M23 + lhs.M23*rhs.M33,
                M30 = lhs.M30*rhs.M00 + lhs.M31*rhs.M10 + lhs.M32*rhs.M20 + lhs.M33*rhs.M30,
                M31 = lhs.M30*rhs.M01 + lhs.M31*rhs.M11 + lhs.M32*rhs.M21 + lhs.M33*rhs.M31,
                M32 = lhs.M30*rhs.M02 + lhs.M31*rhs.M12 + lhs.M32*rhs.M22 + lhs.M33*rhs.M32,
                M33 = lhs.M30*rhs.M03 + lhs.M31*rhs.M13 + lhs.M32*rhs.M23 + lhs.M33*rhs.M33
            };
            return matrix4x4;
        }

        //public static Vector4 operator *(Matrix4x4 lhs, Vector4 v)
        //{
        //    Vector4 vector4 = new Vector4();
        //    vector4.X = lhs.M00 * v.X + lhs.M01 * v.Y + lhs.M02 * v.Z + lhs.M03 * v.W;
        //    vector4.Y = lhs.M10 * v.X + lhs.M11 * v.Y + lhs.M12 * v.Z + lhs.M13 * v.W;
        //    vector4.Z = lhs.M20 * v.X + lhs.M21 * v.Y + lhs.M22 * v.Z + lhs.M23 * v.W;
        //    vector4.W = lhs.M30 * v.X + lhs.M31 * v.Y + lhs.M32 * v.Z + lhs.M33 * v.W;
        //    return vector4;
        //}

        //[WrapperlessIcall]
        //public static extern Matrix4x4 Ortho(float left, float right, float bottom, float top, float zNear, float zFar);

        //[WrapperlessIcall]
        //public static extern Matrix4x4 Perspective(float fov, float aspect, float zNear, float zFar);

        //public static Matrix4x4 Scale(Vector3 v)
        //{
        //    Matrix4x4 matrix4x4 = new Matrix4x4()
        //    {
        //        M00 = v.X,
        //        M01 = 0f,
        //        M02 = 0f,
        //        M03 = 0f,
        //        M10 = 0f,
        //        M11 = v.Y,
        //        M12 = 0f,
        //        M13 = 0f,
        //        M20 = 0f,
        //        M21 = 0f,
        //        M22 = v.Z,
        //        M23 = 0f,
        //        M30 = 0f,
        //        M31 = 0f,
        //        M32 = 0f,
        //        M33 = 1f
        //    };
        //    return matrix4x4;
        //}

        //public void SetColumn(int i, Vector4 v)
        //{
        //    this[0, i] = v.X;
        //    this[1, i] = v.Y;
        //    this[2, i] = v.Z;
        //    this[3, i] = v.W;
        //}

        //public void SetRow(int i, Vector4 v)
        //{
        //    this[i, 0] = v.X;
        //    this[i, 1] = v.Y;
        //    this[i, 2] = v.Z;
        //    this[i, 3] = v.W;
        //}

        //public void SetTRS(Vector3 pos, Quaternion q, Vector3 s)
        //{
        //    this = Matrix4x4.TRS(pos, q, s);
        //}

        public override string ToString()
        {
            return string.Format("{0:F5}\t{1:F5}\t{2:F5}\t{3:F5}\n{4:F5}\t{5:F5}\t{6:F5}\t{7:F5}\n{8:F5}\t{9:F5}\t{10:F5}\t{11:F5}\n{12:F5}\t{13:F5}\t{14:F5}\t{15:F5}\n", new object[] {M00, M01, M02, M03, M10, M11, M12, M13, M20, M21, M22, M23, M30, M31, M32, M33});
        }

        public string ToString(string format)
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\n{4}\t{5}\t{6}\t{7}\n{8}\t{9}\t{10}\t{11}\n{12}\t{13}\t{14}\t{15}\n", new object[] {M00.ToString(format), M01.ToString(format), M02.ToString(format), M03.ToString(format), M10.ToString(format), M11.ToString(format), M12.ToString(format), M13.ToString(format), M20.ToString(format), M21.ToString(format), M22.ToString(format), M23.ToString(format), M30.ToString(format), M31.ToString(format), M32.ToString(format), M33.ToString(format)});
        }

        //public static Matrix4x4 Transpose(Matrix4x4 m)
        //{
        //    return Matrix4x4.INTERNAL_CALL_Transpose(ref m);
        //}

        /// <summary>
        ///     Construct a translation-rotation-scale matrix
        /// </summary>
        /// <param name="pos">Translation vector</param>
        /// <param name="q">Rotation quaternion</param>
        /// <param name="s">Scale vector</param>
        /// <returns></returns>
        public static Matrix4x4 TRS(Vector3 pos, Quaternion q, Vector3 s)
        {
            return pos.ToTranslationMatrix()*q.ToRotationalMatrix()*s.ToScaleMatrix();
        }

        /// <summary>
        ///     Construct a translation-scale matrix
        /// </summary>
        /// <param name="pos">Translation vector</param>
        /// <param name="s">Scale vector</param>
        /// <returns></returns>
        public static Matrix4x4 TS(Vector3 pos, Vector3 s)
        {
            return pos.ToTranslationMatrix()*s.ToScaleMatrix();
        }

        // See: http://www.rkinteractive.com/blogs/SoftwareDevelopment/post/2013/05/21/Algorithms-In-C-Finding-The-Inverse-Of-A-Matrix.aspx
        // Given an nXn matrix A, solve n linear equations to find the inverse of A.
        private static double[][] InvertMatrix(double[][] A)
        {
            int n = A.Length;
            //e will represent each column in the identity matrix
            double[] e;
            //X will hold the inverse matrix to be returned
            var x = new double[n][];
            for (int i = 0; i < n; i++)
            {
                x[i] = new double[A[i].Length];
            }
            /*
            * solve will contain the vector solution for the LUP decomposition as we solve
            * for each vector of X.  We will combine the solutions into the double[][] array X.
            * */
            double[] solve;

            //Get the LU matrix and P matrix (as an array)
            Tuple<double[][], int[]> results = LUPDecomposition(A);

            double[][] LU = results.Item1;
            int[] P = results.Item2;

            /*
            * Solve AX = e for each column ei of the identity matrix using LUP decomposition
            * */
            for (int i = 0; i < n; i++)
            {
                e = new double[A[i].Length];
                e[i] = 1;
                solve = LUPSolve(LU, P, e);
                for (int j = 0; j < solve.Length; j++)
                {
                    x[j][i] = solve[j];
                }
            }
            return x;
        }

        // See: http://www.rkinteractive.com/blogs/SoftwareDevelopment/post/2013/05/14/Algorithms-In-C-Solving-A-System-Of-Linear-Equations.aspx
        // Given L,U,P and b solve for X.
        // Input the L and U matrices as a single matrix LU.
        // Return the solution as a double[].
        // LU will be a n+1xm+1 matrix where the first row and columns are zero.
        // This is for ease of computation and consistency with Cormen et al.
        // pseudocode.
        // The pi array represents the permutation matrix.
        // 
        private static double[] LUPSolve(double[][] LU, int[] pi, double[] b)
        {
            int n = LU.Length - 1;
            var x = new double[n + 1];
            var y = new double[n + 1];
            double suml = 0;
            double sumu = 0;
            double lij = 0;

            /*
            * Solve for Y using formward substitution
            * */
            for (int i = 0; i <= n; i++)
            {
                suml = 0;
                for (int j = 0; j <= i - 1; j++)
                {
                    /*
                    * Since we've taken L and U as a singular matrix as an input
                    * the value for L at index i and j will be 1 when i equals j, not LU[i][j], since
                    * the diagonal values are all 1 for L.
                    * */
                    if (i == j)
                    {
                        lij = 1;
                    }
                    else
                    {
                        lij = LU[i][j];
                    }
                    suml = suml + (lij*y[j]);
                }
                y[i] = b[pi[i]] - suml;
            }
            //Solve for X by using back substitution
            for (int i = n; i >= 0; i--)
            {
                sumu = 0;
                for (int j = i + 1; j <= n; j++)
                {
                    sumu = sumu + (LU[i][j]*x[j]);
                }
                x[i] = (y[i] - sumu)/LU[i][i];
            }
            return x;
        }

        // See: http://www.rkinteractive.com/blogs/SoftwareDevelopment/post/2013/05/07/Algorithms-In-C-LUP-Decomposition.aspx
        // Perform LUP decomposition on a matrix A.
        // Return L and U as a single matrix(double[][]) and P as an array of ints.
        // We implement the code to compute LU "in place" in the matrix A.
        // In order to make some of the calculations more straight forward and to 
        // match Cormen's et al. pseudocode the matrix A should have its first row and first columns
        // to be all 0.
        private static Tuple<double[][], int[]> LUPDecomposition(double[][] A)
        {
            int n = A.Length - 1;
            /*
            * pi represents the permutation matrix.  We implement it as an array
            * whose value indicates which column the 1 would appear.  We use it to avoid 
            * dividing by zero or small numbers.
            * */
            var pi = new int[n + 1];
            double p = 0;
            int kp = 0;
            int pik = 0;
            int pikp = 0;
            double aki = 0;
            double akpi = 0;

            //Initialize the permutation matrix, will be the identity matrix
            for (int j = 0; j <= n; j++)
            {
                pi[j] = j;
            }

            for (int k = 0; k <= n; k++)
            {
                /*
                * In finding the permutation matrix p that avoids dividing by zero
                * we take a slightly different approach.  For numerical stability
                * We find the element with the largest 
                * absolute value of those in the current first column (column k).  If all elements in
                * the current first column are zero then the matrix is singluar and throw an
                * error.
                * */
                p = 0;
                for (int i = k; i <= n; i++)
                {
                    if (Math.Abs(A[i][k]) > p)
                    {
                        p = Math.Abs(A[i][k]);
                        kp = i;
                    }
                }
                if (p == 0)
                {
                    throw new Exception("singular matrix");
                }
                /*
                * These lines update the pivot array (which represents the pivot matrix)
                * by exchanging pi[k] and pi[kp].
                * */
                pik = pi[k];
                pikp = pi[kp];
                pi[k] = pikp;
                pi[kp] = pik;

                /*
                * Exchange rows k and kpi as determined by the pivot
                * */
                for (int i = 0; i <= n; i++)
                {
                    aki = A[k][i];
                    akpi = A[kp][i];
                    A[k][i] = akpi;
                    A[kp][i] = aki;
                }

                /*
                    * Compute the Schur complement
                    * */
                for (int i = k + 1; i <= n; i++)
                {
                    A[i][k] = A[i][k]/A[k][k];
                    for (int j = k + 1; j <= n; j++)
                    {
                        A[i][j] = A[i][j] - (A[i][k]*A[k][j]);
                    }
                }
            }
            return Tuple.Create(A, pi);
        }

        #region Meta.Numerics to Unity vectors and matrices conversions

        /// <summary>
        /// Converts a unity matrix to an equivalent Meta.Numerics square matrix
        /// </summary>
        /// <returns>The Meta.Numerics square matrix corresponding to the provided matrix</returns>
        /// <param name="matrix">A Unity3D transformation matrix</param>
        public static SquareMatrix UnityToMetaMat(Matrix4x4 matrix)
        {
            SquareMatrix result = new SquareMatrix(4);

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    result[i, j] = matrix[i, j];

            return result;
        }

        /// <summary>
        /// Converts a unity vector to an equivalent Meta.Numerics row vector
        /// </summary>
        /// <returns>The Meta.Numerics vector corresponding to the provided vector</returns>
        /// <param name="vector">A Unity3D 3D Vector</param>
        public static RowVector UnityToMetaMat(Vector3 vector)
        {
            return new RowVector(new double[] { vector.X, vector.Y, vector.Z });
        }

        /// <summary>
        /// Converts a Meta.Numerics square matrix to an equivalent unity matrix.
        /// If the provided matrix is smaller than 4x4, the resulting matrix is padded with 0s
        /// If the provided matrix is greater than 4x4, it is cropped
        /// </summary>
        /// <returns>The Unity3D matrix corresponding to the provided matrix</returns>
        /// <param name="matrix">A Meta.numerics square transformation matrix</param>
        public static Matrix4x4 MetaMatToUnity(SquareMatrix matrix)
        {
            Matrix4x4 result = new Matrix4x4();
            int rows = matrix.RowCount > 4 ? 4 : matrix.RowCount,
                cols = matrix.ColumnCount > 4 ? 4 : matrix.ColumnCount;

            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                    result[i, j] = (float)matrix[i, j];

            return result;
        }

        #endregion
    }

     
}