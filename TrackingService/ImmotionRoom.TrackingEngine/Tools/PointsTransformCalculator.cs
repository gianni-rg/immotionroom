namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta.Numerics.Matrices;
    using ImmotionAR.ImmotionRoom.TrackingEngine.Model;

    /// <summary>
    /// Points transform calculator class.
    /// This is a helper class that finds the transformation matrix that best represent the transform between two set of points
    /// </summary>
    internal class PointsTransformCalculator
    {
        /// <summary>
        /// Computes the roto-translation matrix that transform the point set A to point set B.
        /// It is the RT matrix that best fits between the two sets of points.
        /// Registration algorithm has been taken from http://nghiaho.com/?page_id=671
        /// </summary>
        /// <returns>The RT matrix matching the two points sets</returns>
        /// <param name="pointsSlave">The first set of points. These are the points that must be transformed TO the pointsMaster ones</param>
        /// <param name="pointsMaster">The second set of points. These are the reference points</param>
        public static Matrix4x4 FindRTmatrix(List<Vector3> pointsSlave, List<Vector3> pointsMaster)
        {
            //if not enough points, return the identity
            if (pointsMaster.Count < 1 || pointsSlave.Count < 1)
                return Matrix4x4.Identity;

            //calculate centroid of two points sets
            Vector3 masterCentroid = FindPointsCentroid(pointsMaster),
            slaveCentroid = FindPointsCentroid(pointsSlave);

            //compute the H matrix
            SquareMatrix H = FindH(pointsSlave, slaveCentroid, pointsMaster, masterCentroid);

            //compute the RT marix
            Matrix4x4 returnMatrix;
            FindRT(H, slaveCentroid, masterCentroid, out returnMatrix);

            //return it
            return returnMatrix;
        }

        /// <summary>
        /// Calculates the centroid of a set of points
        /// </summary>
        /// <returns>The vector3 centroid of the provided points array</returns>
        /// <param name="points">The points of interest</param>
        public static Vector3 FindPointsCentroid(List<Vector3> points)
        {
            Vector3 sum = Vector3.Zero;

            if (points.Count > 0)
            {
                foreach (Vector3 v in points)
                    sum += v;

                sum /= points.Count;
            }

            return sum;
        }

        /// <summary>
        /// Finds the H matrix relative to two sets of point.
        /// The H matrix is a helper matrix useful for finding the rotation between two sets of 3D points
        /// (see http://nghiaho.com/?page_id=671 for further details)
        /// </summary>
        /// <returns>The h matrix</returns>
        /// <param name="pointsA">List of A points (slave positions)/param>
        /// <param name="centroidA">Centroid of A points</param>
        /// <param name="pointsB">List of B points (master positions)/param>
        /// <param name="centroidB">Centroid of B points</param>
        private static SquareMatrix FindH(List<Vector3> pointsA, Vector3 centroidA, List<Vector3> pointsB, Vector3 centroidB)
        {
            SquareMatrix h = new SquareMatrix(3);

            for (int i = 0; i < pointsA.Count && i < pointsB.Count; i++)
            {
                Vector3 normalizPointA = pointsA[i] - centroidA;
                Vector3 normalizPointB = pointsB[i] - centroidB;

                h = h + (SquareMatrix)(FancyUtilities.UnityToMetaMat(normalizPointA).Transpose() * FancyUtilities.UnityToMetaMat(normalizPointB));
            }

            return h;
        }

        /// <summary>
        /// Finds the roto-translation matrix between two set of points
        /// (see http://nghiaho.com/?page_id=671 for further details on the algorithm)
        /// </summary>
        /// <param name="H">The H matrix between the two set of points as computed by FindH method</param>
        /// <param name="centroidA">Centroid of A (slave) points</param>
        /// <param name="centroidB">Centroid of B (master) points</param>
        /// <param name="rtMatrix">The resulting RT matrix.</param>
        private static void FindRT(SquareMatrix H, Vector3 centroidA, Vector3 centroidB, out Matrix4x4 rtMatrix)
        {
            //compute rotation
            var svd = H.SingularValueDecomposition();
            SquareMatrix rot = svd.RightTransformMatrix() * svd.LeftTransformMatrix().Transpose();

            if ((rot.LUDecomposition().Determinant()) < 0)
            {
                rot[0, 2] *= -1;
                rot[1, 2] *= -1;
                rot[2, 2] *= -1;
            }

            //compute translation
            ColumnVector transl = -rot * FancyUtilities.UnityToMetaMat(centroidA).Transpose() + FancyUtilities.UnityToMetaMat(centroidB).Transpose();

            rtMatrix = FancyUtilities.MetaMatToUnity(rot);
            rtMatrix[0, 3] = (float)transl[0];
            rtMatrix[1, 3] = (float)transl[1];
            rtMatrix[2, 3] = (float)transl[2];
            rtMatrix[3, 3] = 1.0f;
        }
    }
}