using System.Numerics;
using SoulsFormats;

namespace MassConvertFromModel
{
    public static class Extensions
    {
        #region Vector

        public static Assimp.Vector3D ToAssimpVector3D(this Vector3 vector)
        {
            return new Assimp.Vector3D(vector.X, vector.Y, vector.Z);
        }

        public static Assimp.Vector3D ToAssimpVector3D(this Vector3 vector, Matrix4x4 transform)
        {
            vector = Vector3.Transform(vector, transform);
            return new Assimp.Vector3D(vector.X, vector.Y, vector.Z);
        }

        public static Assimp.Vector3D ToAssimpVector3D(this Vector4 vector)
        {
            return new Assimp.Vector3D(vector.X, vector.Y, vector.Z);
        }

        #endregion

        #region Matrix4x4

        /// <summary>
        /// Convert a System.Numerics.Matrix4x4 into an Assimp.Matrix4x4.
        /// </summary>
        /// <param name="mat4">A System.Numerics.Matrix4x4.</param>
        /// <returns>An Assimp.Matrix4x4.</returns>
        public static Assimp.Matrix4x4 ToAssimpMatrix4x4(this Matrix4x4 mat4)
        {
            return new Assimp.Matrix4x4(mat4.M11, mat4.M21, mat4.M31, mat4.M41,
                                        mat4.M12, mat4.M22, mat4.M32, mat4.M42,
                                        mat4.M13, mat4.M23, mat4.M33, mat4.M43,
                                        mat4.M14, mat4.M24, mat4.M34, mat4.M44);
        }

        /// <summary>
        /// Convert an Assimp.Matrix4x4 into a System.Numerics.Matrix4x4.
        /// </summary>
        /// <param name="mat4">An Assimp.Matrix4x4.</param>
        /// <returns>A System.Numerics.Matrix4x4.</returns>
        public static Matrix4x4 ToNumericsMatrix4x4(this Assimp.Matrix4x4 mat4)
        {
            return new Matrix4x4(mat4.A1, mat4.B1, mat4.C1, mat4.D1,
                                 mat4.A2, mat4.B2, mat4.C2, mat4.D2,
                                 mat4.A3, mat4.B3, mat4.C3, mat4.D3,
                                 mat4.A4, mat4.B4, mat4.C4, mat4.D4);
        }

        #endregion

        #region Transform

        /// <summary>
        /// Compute a transform starting from a given bone to the root bone.
        /// </summary>
        /// <param name="bone">The bone to start from.</param>
        /// <param name="bones">A list of all bones.</param>
        /// <returns>A transform.</returns>
        /// <exception cref="InvalidDataException">The parent index of a bone was outside of the provided bone array.</exception>
        public static Matrix4x4 ComputeTransform(this FLVER.Bone bone, IList<FLVER.Bone> bones)
        {
            var transform = bone.ComputeLocalTransform();
            while (bone.ParentIndex != -1)
            {
                if (!(bone.ParentIndex < -1) && !(bone.ParentIndex > bones.Count))
                {
                    bone = bones[bone.ParentIndex];
                    transform *= bone.ComputeLocalTransform();
                }
                else
                {
                    throw new InvalidDataException("Bone has a parent index outside of the provided bone array.");
                }
            }

            return transform;
        }

        public static Matrix4x4 ComputeTransform(this MDL4.Bone bone, IList<MDL4.Bone> bones)
        {
            var transform = bone.ComputeLocalTransform();
            while (bone.ParentIndex != -1)
            {
                if (!(bone.ParentIndex < -1) && !(bone.ParentIndex > bones.Count))
                {
                    bone = bones[bone.ParentIndex];
                    transform *= bone.ComputeLocalTransform();
                }
                else
                {
                    throw new InvalidDataException("Bone has a parent index outside of the provided bone array.");
                }
            }

            return transform;
        }

        public static Matrix4x4 ComputeTransform(this SMD4.Bone bone, IList<SMD4.Bone> bones)
        {
            var transform = bone.ComputeLocalTransform();
            while (bone.ParentIndex != -1)
            {
                if (!(bone.ParentIndex < -1) && !(bone.ParentIndex > bones.Count))
                {
                    bone = bones[bone.ParentIndex];
                    transform *= bone.ComputeLocalTransform();
                }
                else
                {
                    throw new InvalidDataException("Bone has a parent index outside of the provided bone array.");
                }
            }

            return transform;
        }

        #endregion
    }
}
