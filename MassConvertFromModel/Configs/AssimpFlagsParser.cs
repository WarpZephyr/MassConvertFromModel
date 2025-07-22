using Assimp;
using System.IO;

namespace MassConvertFromModel.Configs
{
    /// <summary>
    /// Parses assimp flags from the assimp flags config.
    /// </summary>
    internal static class AssimpFlagsParser
    {
        /// <summary>
        /// Parses the file on the specified path into assimp flags.
        /// </summary>
        /// <param name="path">The path to the file to parse.</param>
        /// <returns>Assimp flags parsed from the specified file.</returns>
        public static PostProcessSteps Parse(string path)
        {
            PostProcessSteps flags = PostProcessSteps.None;
            string[] lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                var parsedLine = LineParser.ParseLine(line);
                if (!string.IsNullOrWhiteSpace(parsedLine))
                {
                    flags |= ToFlag(parsedLine);
                }
            }

            return flags;
        }

        /// <summary>
        /// Converts the specified line into an assimp flag.
        /// </summary>
        /// <param name="parsedLine">The line to parse.</param>
        /// <returns>An assimp flag.</returns>
        private static PostProcessSteps ToFlag(string parsedLine)
        {
            switch (parsedLine)
            {
                case "CalculateTangentSpace":
                    return PostProcessSteps.CalculateTangentSpace;
                case "JoinIdenticalVertices":
                    return PostProcessSteps.JoinIdenticalVertices;
                case "MakeLeftHanded":
                    return PostProcessSteps.MakeLeftHanded;
                case "Triangulate":
                    return PostProcessSteps.Triangulate;
                case "RemoveComponent":
                    return PostProcessSteps.RemoveComponent;
                case "GenerateNormals":
                    return PostProcessSteps.GenerateNormals;
                case "GenerateSmoothNormals":
                    return PostProcessSteps.GenerateSmoothNormals;
                case "SplitLargeMeshes":
                    return PostProcessSteps.SplitLargeMeshes;
                case "PreTransformVertices":
                    return PostProcessSteps.PreTransformVertices;
                case "LimitBoneWeights":
                    return PostProcessSteps.LimitBoneWeights;
                case "ValidateDataStructure":
                    return PostProcessSteps.ValidateDataStructure;
                case "ImproveCacheLocality":
                    return PostProcessSteps.ImproveCacheLocality;
                case "RemoveRedundantMaterials":
                    return PostProcessSteps.RemoveRedundantMaterials;
                case "FixInFacingNormals":
                    return PostProcessSteps.FixInFacingNormals;
                case "SortByPrimitiveType":
                    return PostProcessSteps.SortByPrimitiveType;
                case "FindDegenerates":
                    return PostProcessSteps.FindDegenerates;
                case "FindInvalidData":
                    return PostProcessSteps.FindInvalidData;
                case "GenerateUVCoords":
                    return PostProcessSteps.GenerateUVCoords;
                case "TransformUVCoords":
                    return PostProcessSteps.TransformUVCoords;
                case "FindInstances":
                    return PostProcessSteps.FindInstances;
                case "OptimizeMeshes":
                    return PostProcessSteps.OptimizeMeshes;
                case "OptimizeGraph":
                    return PostProcessSteps.OptimizeGraph;
                case "FlipUVs":
                    return PostProcessSteps.FlipUVs;
                case "FlipWindingOrder":
                    return PostProcessSteps.FlipWindingOrder;
                case "SplitByBoneCount":
                    return PostProcessSteps.SplitByBoneCount;
                case "Debone":
                    return PostProcessSteps.Debone;
                case "GlobalScale":
                    return PostProcessSteps.GlobalScale;
                case "EmbedTextures":
                    return PostProcessSteps.EmbedTextures;
                case "ForceGenerateNormals":
                    return PostProcessSteps.ForceGenerateNormals;
                case "DropNormals":
                    return PostProcessSteps.DropNormals;
                case "GenerateBoundingBoxes":
                    return PostProcessSteps.GenerateBoundingBoxes;
                default:
                    return PostProcessSteps.None;
            }
        }
    }
}
