namespace ImmotionAR.ImmotionRoom.TrackingEngine.Tools
{
    using System;
    using System.Runtime.Serialization;
    using Meta.Numerics.Matrices;
    using Model;

    /// <summary>
    ///     Provides fancy utility functions to be used in Unity AR/VR projects
    /// </summary>
    internal class FancyUtilities
    {
        #region Vector and Joint conversions

        /// <summary>
        ///     Gets the vector3 location from a joint, with a default scaling factor of 10
        /// </summary>
        /// <returns>The vector3 location of the desired joint</returns>
        /// <param name="joint">The joint of interest</param>
        public static Vector3 GetVector3FromJoint(BodyJointData joint)
        {
            return new Vector3(joint.Position.X*10f, joint.Position.Y*10f, joint.Position.Z*10f);
        }

        /// <summary>
        ///     Gets the vector3 location from a joint, with a scaling factor provided by user
        /// </summary>
        /// <returns>The vector3 location of the desired joint</returns>
        /// <param name="joint">The joint of interest</param>
        /// <param name="scaleFactor">Scale factor for resulting vector</param>
        public static Vector3 GetVector3FromJoint(BodyJointData joint, float scaleFactor)
        {
            return new Vector3(joint.Position.X*scaleFactor, joint.Position.Y*scaleFactor, joint.Position.Z*scaleFactor);
        }

        #endregion

        #region Meta.Numerics to Unity vectors and matrices conversions

        /// <summary>
        ///     Converts a unity matrix to an equivalent Meta.Numerics square matrix
        /// </summary>
        /// <returns>The Meta.Numerics square matrix corresponding to the provided matrix</returns>
        /// <param name="matrix">A Unity3D transformation matrix</param>
        public static SquareMatrix UnityToMetaMat(Matrix4x4 matrix)
        {
            var result = new SquareMatrix(4);

            for (var i = 0; i < 4; i++)
                for (var j = 0; j < 4; j++)
                    result[i, j] = matrix[i, j];

            return result;
        }

        /// <summary>
        ///     Converts a unity vector to an equivalent Meta.Numerics row vector
        /// </summary>
        /// <returns>The Meta.Numerics vector corresponding to the provided vector</returns>
        /// <param name="vector">A Unity3D 3D Vector</param>
        public static RowVector UnityToMetaMat(Vector3 vector)
        {
            return new RowVector(vector.X, vector.Y, vector.Z);
        }

        /// <summary>
        ///     Converts a Meta.Numerics square matrix to an equivalent unity matrix.
        ///     If the provided matrix is smaller than 4x4, the resulting matrix is padded with 0s
        ///     If the provided matrix is greater than 4x4, it is cropped
        /// </summary>
        /// <returns>The Unity3D matrix corresponding to the provided matrix</returns>
        /// <param name="matrix">A Meta.numerics square transformation matrix</param>
        public static Matrix4x4 MetaMatToUnity(SquareMatrix matrix)
        {
            var result = new Matrix4x4();
            int rows = matrix.RowCount > 4 ? 4 : matrix.RowCount,
                cols = matrix.ColumnCount > 4 ? 4 : matrix.ColumnCount;

            for (var i = 0; i < rows; i++)
                for (var j = 0; j < cols; j++)
                    result[i, j] = (float) matrix[i, j];

            return result;
        }

        #endregion

        #region Unity Matrix4x4 serialization and deserialization

        /// <summary>
        ///     Serializes a Unity trasformation matrix, so that it can be written to a text file
        /// </summary>
        /// <returns>Serialized version of the matrix</returns>
        /// <param name="matrix">Matrix to be serialized</param>
        public static string UnityMatrixToFileString(Matrix4x4 matrix)
        {
            //let's write matrix to file with a little header and then all the values with a nice formatting
            var serializationString = "Calibration matrix = {\n";

            for (var r = 0; r < 4; r++)
            {
                serializationString += matrix[r, 0];

                for (var c = 1; c < 4; c++)
                    serializationString += ", " + matrix[r, c];

                serializationString += "\n";
            }

            serializationString += "}\n";

            return serializationString;
        }

        /// <summary>
        ///     Serializes an array of Unity trasformation matrices, so that they can be written to a text file
        /// </summary>
        /// <returns>Serialized version of the matrices array</returns>
        /// <param name="matrices">Matrices to be serialized</param>
        public static string UnityMatricesToFileString(Matrix4x4[] matrices)
        {
            //for each matrix, serialize it and end the string with a ";" delimiter
            var serializationString = "";

            foreach (var matrix in matrices)
                serializationString += UnityMatrixToFileString(matrix) + ";\n";

            return serializationString;
        }

        /// <summary>
        ///     Deserializes a Unity trasformation matrix, from a string serialized with UnityMatrixToFileString
        /// </summary>
        /// <returns>Matrix deserialized from the string</returns>
        /// <param name="fileString">String with matrix serialization</param>
        /// <exception cref="System.Runtime.Serialization.SerializationException">
        ///     Thrown if the string does not contain a valid
        ///     matrix
        /// </exception>
        public static Matrix4x4 UnityMatrixFromFileString(string fileString)
        {
            //skip all header informations:
            //retain only substring inside the curly braces { }
            int openCurlyIdx = fileString.IndexOf('{'), closedCurlyIdx = fileString.IndexOf("}");

            if (openCurlyIdx < 0 || closedCurlyIdx < 0 || openCurlyIdx > closedCurlyIdx)
                throw new SerializationException("The provided string does not represent a valid Matrix object");

            var serializationString = fileString.Substring(openCurlyIdx + 1, closedCurlyIdx - openCurlyIdx - 1);

            //tokenize the string into values
            char[] delimiters = {'\r', '\n', ' ', ','};
            var values = serializationString.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            if (values.Length != 16)
                throw new SerializationException("The provided string does not represent a valid Matrix object");

            //assign values to matrix elements
            var returnMatrix = new Matrix4x4();

            for (var i = 0; i < 16; i++)
            {
                float value = 0;

                if (!float.TryParse(values[i], out value))
                    throw new SerializationException("The provided string does not represent a valid Matrix object");

                returnMatrix[i/4, i%4] = value;
            }

            return returnMatrix;
        }

        /// <summary>
        ///     Deserializes an array of Unity trasformation matrix, from a string serialized with UnityMatricesToFileString
        /// </summary>
        /// <returns>Matrices deserialized from the string</returns>
        /// <param name="fileString">String with matrices serialization</param>
        /// <exception cref="System.Runtime.Serialization.SerializationException">
        ///     Thrown if the string does not contain valid
        ///     matrices
        /// </exception>
        public static Matrix4x4[] UnityMatricesFromFileString(string fileString)
        {
            //tokenize the string into values
            char[] delimiters = {';'};
            var values = fileString.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);
            //create matrix and push data
            var returnMatrices = new Matrix4x4[values.Length - 1]; //-1 because last found string is only a newline character

            for (var i = 0; i < values.Length - 1; i++)
            {
                returnMatrices[i] = UnityMatrixFromFileString(values[i]);
            }

            return returnMatrices;
        }

        #endregion

        #region Maths utilities

        /// <summary>
        ///     Calculate atan2 and clamp result in range [-pi, +pi]
        /// </summary>
        /// <returns>The desired atan2</returns>
        /// <param name="Y">The Y coordinate</param>
        /// <param name="X">The X coordinate</param>
        public static float ClampedAtan2(float y, float x)
        {
            var orientation = (float) Math.Atan2(y, x);

            //clamp orientation in [-pi, +pi] range 
            while (orientation < -Math.PI)
                orientation += 2*(float) Math.PI;

            while (orientation > +Math.PI)
                orientation -= 2*(float) Math.PI;

            return orientation;
        }

        /// <summary>
        ///     Calculate orientation of line segment that goes from startJoint to endJoint, in the XZ plane (Y component is
        ///     discarded)
        /// </summary>
        /// <returns>The joints segment XZ orientation</returns>
        /// <param name="startJoint">Start joint</param>
        /// <param name="endJoint">End joint</param>
        public static float BetweenJointsXZOrientation(Vector3 startJoint, Vector3 endJoint)
        {
            var diffJoint = endJoint - startJoint;

            return ClampedAtan2(diffJoint.Z, diffJoint.X);
        }

        /// <summary>
        ///     Calculate module of line segment that goes from startJoint to endJoint, in the XZ plane (Y component is discarded)
        /// </summary>
        /// <returns>The joints segment XZ orientation</returns>
        /// <param name="startJoint">Start joint</param>
        /// <param name="endJoint">End joint</param>
        public static float BetweenJointsXZDistance(Vector3 startJoint, Vector3 endJoint)
        {
            return (float) Math.Sqrt(BetweenJointsXZSqrDistance(startJoint, endJoint));
        }

        /// <summary>
        ///     Calculate squared module of line segment that goes from startJoint to endJoint, in the XZ plane (Y component is
        ///     discarded)
        /// </summary>
        /// <returns>The joints segment XZ orientation</returns>
        /// <param name="startJoint">Start joint</param>
        /// <param name="endJoint">End joint</param>
        public static float BetweenJointsXZSqrDistance(Vector3 startJoint, Vector3 endJoint)
        {
            var diffJoint = endJoint - startJoint;

            return new Vector2(diffJoint.X, diffJoint.Z).SqrMagnitude;
        }

        /// <summary>
        ///     Adjusts an orientation so that it stays in range [-pi, +pi) of distance from a given orientation reference
        /// </summary>
        /// <returns>The orientation adjusted to stay near the reference</returns>
        /// <param name="orientation">Orientation</param>
        /// <param name="orientationReference">Orientation reference</param>
        public static float AdjustOrientation(float orientation, float orientationReference)
        {
            //clamp orientation in [-pi, +pi] range from reference
            while (orientation <= orientationReference - Math.PI)
                orientation += 2*(float) Math.PI;

            while (orientation > orientationReference + Math.PI)
                orientation -= 2*(float) Math.PI;

            return orientation;
        }

        #endregion
    }
}