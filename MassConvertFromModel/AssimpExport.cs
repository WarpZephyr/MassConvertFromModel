using SoulsFormats;
using Assimp;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace MassConvertFromModel
{
    /// <summary>
    /// Exports SoulsFormats models using AssimpNet.
    /// </summary>
    internal static class AssimpExport
    {
        public static bool ExportModel(FLVER2 model, string outFolder, string outPath, string type)
        {
            Scene scene = ToAssimpScene(model);
            PathHandler.EnsureFolderExists(outFolder);
            return new AssimpContext().ExportFile(scene, outPath, type);
        }

        public static bool ExportModel(FLVER0 model, string outFolder, string outPath, string type)
        {
            Scene scene = ToAssimpScene(model);
            PathHandler.EnsureFolderExists(outFolder);
            return new AssimpContext().ExportFile(scene, outPath, type);
        }

        public static bool ExportModel(MDL4 model, string outFolder, string outPath, string type)
        {
            Scene scene = ToAssimpScene(model);
            PathHandler.EnsureFolderExists(outFolder);
            return new AssimpContext().ExportFile(scene, outPath, type);
        }

        public static bool ExportModel(SMD4 model, string outFolder, string outPath, string type)
        {
            Scene scene = ToAssimpScene(model);
            PathHandler.EnsureFolderExists(outFolder);
            return new AssimpContext().ExportFile(scene, outPath, type);
        }

        public static bool ExportModel(byte[] bytes, string outFolder, string outPath, string type)
        {
            if (FLVER2.IsRead(bytes, out FLVER2 flver2))
            {
                return ExportModel(flver2, outFolder, outPath, type);
            }
            else if (FLVER0.IsRead(bytes, out FLVER0 flver0))
            {
                return ExportModel(flver0, outFolder, outPath, type);
            }
            else if (MDL4.IsRead(bytes, out MDL4 mdl4))
            {
                return ExportModel(mdl4, outFolder, outPath, type);
            }
            else if (SMD4.IsRead(bytes, out SMD4 smd4))
            {
                return ExportModel(smd4, outFolder, outPath, type);
            }

            return false;
        }

        /// <summary>
        /// Converts a FLVER0 into an Assimp Scene.
        /// </summary>
        public static Scene ToAssimpScene(FLVER0 model)
        {
            var scene = new Scene();
            var aiRootNode = new Node();
            scene.RootNode = aiRootNode;
            aiRootNode.Transform = Assimp.Matrix4x4.Identity;

            // Add bone nodes
            Node[] boneArray = new Node[model.Bones.Count];
            for (int i = 0; i < model.Bones.Count; i++)
            {
                var bone = model.Bones[i];
                Node parentNode;
                if (bone.ParentIndex == -1)
                {
                    parentNode = aiRootNode;
                }
                else
                {
                    parentNode = boneArray[bone.ParentIndex];
                }
                var aiNode = new Node(bone.Name, parentNode);

                // Get local transform
                aiNode.Transform = bone.ComputeLocalTransform().ToAssimpMatrix4x4();

                parentNode.Children.Add(aiNode);
                boneArray[i] = aiNode;
            }

            // Add materials
            foreach (var material in model.Materials)
            {
                Material newMaterial = new()
                {
                    Name = material.Name,
                };

                scene.Materials.Add(newMaterial);
            }

            // Add meshes
            for (int meshIndex = 0; meshIndex < model.Meshes.Count; meshIndex++)
            {
                var mesh = model.Meshes[meshIndex];
                var newMesh = new Mesh("Mesh_M" + meshIndex, PrimitiveType.Triangle);
                bool hasBones = mesh.BoneIndices.Length != 0;

                // Prepare a bone map
                var boneMap = new Dictionary<int, Bone>(mesh.BoneIndices.Length);

                // Add vertices
                for (int vertexIndex = 0; vertexIndex < mesh.Vertices.Count; vertexIndex++)
                {
                    var vertex = mesh.Vertices[vertexIndex];

                    // If the mesh has bones set the weights of this vertex to the correct bone
                    if (hasBones)
                    {
                        // Get the local bone index from NormalW then the bone array bone index from the mesh
                        var boneIndex = mesh.BoneIndices[vertex.NormalW];
                        var bone = model.Bones[boneIndex];
                        var transform = bone.ComputeTransform(model.Bones);

                        newMesh.Vertices.Add(vertex.Position.ToAssimpVector3D(transform));
                        newMesh.Normals.Add(vertex.Normal.ToAssimpVector3D());
                        newMesh.BiTangents.Add(vertex.Bitangent.ToAssimpVector3D());
                        foreach (var tangent in vertex.Tangents)
                        {
                            newMesh.Tangents.Add(tangent.ToAssimpVector3D());
                        }

                        // If the bone map does not already have the bone add it
                        if (!boneMap.ContainsKey(boneIndex))
                        {
                            var aiBone = new Bone();
                            var boneNode = boneArray[boneIndex];
                            aiBone.Name = boneNode.Name;

                            Matrix4x4.Invert(transform, out Matrix4x4 transformInverse);
                            aiBone.OffsetMatrix = transformInverse.ToAssimpMatrix4x4();
                            boneMap.Add(boneIndex, aiBone);
                        }

                        // Add this vertex weight to it's bone
                        boneMap[boneIndex].VertexWeights.Add(new VertexWeight(vertexIndex, 1f));
                    }
                    else
                    {
                        newMesh.Vertices.Add(vertex.Position.ToAssimpVector3D());
                        newMesh.Normals.Add(vertex.Normal.ToAssimpVector3D());
                        newMesh.BiTangents.Add(vertex.Bitangent.ToAssimpVector3D());
                        foreach (var tangent in vertex.Tangents)
                        {
                            newMesh.Tangents.Add(tangent.ToAssimpVector3D());
                        }
                    }

                    // Add UVs
                    if (vertex.UVs.Count > 0)
                    {
                        var uv1 = vertex.UVs[0];
                        var aiTextureCoordinate = new Vector3D(uv1.X, uv1.Y, 0f);
                        newMesh.TextureCoordinateChannels[0].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Vector3D(1, 1, 1);
                        newMesh.TextureCoordinateChannels[0].Add(aiTextureCoordinate);
                    }

                    if (vertex.UVs.Count > 1)
                    {
                        var uv2 = vertex.UVs[1];
                        var aiTextureCoordinate = new Vector3D(uv2.X, uv2.Y, 0f);
                        newMesh.TextureCoordinateChannels[1].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Vector3D(1, 1, 1);
                        newMesh.TextureCoordinateChannels[1].Add(aiTextureCoordinate);
                    }

                    if (vertex.UVs.Count > 2)
                    {
                        var uv3 = vertex.UVs[2];
                        var aiTextureCoordinate = new Vector3D(uv3.X, uv3.Y, 0f);
                        newMesh.TextureCoordinateChannels[2].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Vector3D(1, 1, 1);
                        newMesh.TextureCoordinateChannels[2].Add(aiTextureCoordinate);
                    }

                    if (vertex.UVs.Count > 3)
                    {
                        var uv4 = vertex.UVs[3];
                        var aiTextureCoordinate = new Vector3D(uv4.X, uv4.Y, 0f);
                        newMesh.TextureCoordinateChannels[3].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Vector3D(1, 1, 1);
                        newMesh.TextureCoordinateChannels[3].Add(aiTextureCoordinate);
                    }

                    for (int uv = 0; uv < newMesh.TextureCoordinateChannelCount; uv++)
                    {
                        newMesh.UVComponentCount[uv] = 2;
                    }

                    // Add vertex colors
                    if (vertex.Colors.Count > 0)
                    {
                        var color = vertex.Colors[0];
                        newMesh.VertexColorChannels[0].Add(new Color4D(color.R, color.G, color.B, color.A));
                    }

                    if (vertex.Colors.Count > 1)
                    {
                        var color = vertex.Colors[1];
                        newMesh.VertexColorChannels[1].Add(new Color4D(color.R, color.G, color.B, color.A));
                    }
                }

                newMesh.Bones.AddRange(boneMap.Values);

                // Add faces
                var faceVertexIndices = mesh.GetFaceIndices(model.Header.Version);
                foreach (int[] indices in faceVertexIndices)
                {
                    newMesh.Faces.Add(new Face(indices));
                }

                newMesh.MaterialIndex = mesh.MaterialIndex;
                scene.Meshes.Add(newMesh);

                var meshNode = new Node($"Mesh_{meshIndex}", scene.RootNode);
                meshNode.Transform = Assimp.Matrix4x4.Identity;

                meshNode.MeshIndices.Add(meshIndex);
                scene.RootNode.Children.Add(meshNode);
            }

            return scene;
        }

        /// <summary>
        /// Converts a FLVER2 into an Assimp Scene.
        /// </summary>
        public static Scene ToAssimpScene(FLVER2 model)
        {
            var scene = new Scene();
            var aiRootNode = new Node();
            scene.RootNode = aiRootNode;
            aiRootNode.Transform = Assimp.Matrix4x4.Identity;

            // Add bone nodes
            Node[] boneArray = new Node[model.Bones.Count];
            for (int i = 0; i < model.Bones.Count; i++)
            {
                var bone = model.Bones[i];
                Node parentNode;
                if (bone.ParentIndex == -1)
                {
                    parentNode = aiRootNode;
                }
                else
                {
                    parentNode = boneArray[bone.ParentIndex];
                }
                var aiNode = new Node(bone.Name, parentNode);

                // Get local transform
                aiNode.Transform = bone.ComputeLocalTransform().ToAssimpMatrix4x4();

                parentNode.Children.Add(aiNode);
                boneArray[i] = aiNode;
            }

            // Add materials
            foreach (var material in model.Materials)
            {
                Material newMaterial = new()
                {
                    Name = material.Name,
                };

                scene.Materials.Add(newMaterial);
            }

            // Add meshes
            for (int meshIndex = 0; meshIndex < model.Meshes.Count; meshIndex++)
            {
                var mesh = model.Meshes[meshIndex];
                var newMesh = new Mesh("Mesh_M" + meshIndex, PrimitiveType.Triangle);
                bool hasBones = mesh.BoneIndices.Count != 0;

                // Prepare a bone map
                var boneMap = new Dictionary<int, Bone>(mesh.BoneIndices.Count);

                // Add vertices
                for (int vertexIndex = 0; vertexIndex < mesh.Vertices.Count; vertexIndex++)
                {
                    var vertex = mesh.Vertices[vertexIndex];

                    // If the mesh has bones set the weights of this vertex to the correct bone
                    if (hasBones)
                    {
                        // Get the local bone indice from NormalW then the bone array bone indice from the mesh
                        var boneIndice = mesh.BoneIndices[vertex.NormalW];
                        var bone = model.Bones[boneIndice];
                        var transform = bone.ComputeTransform(model.Bones);

                        newMesh.Vertices.Add(vertex.Position.ToAssimpVector3D(transform));
                        newMesh.Normals.Add(vertex.Normal.ToAssimpVector3D());
                        newMesh.BiTangents.Add(vertex.Bitangent.ToAssimpVector3D());
                        foreach (var tangent in vertex.Tangents)
                        {
                            newMesh.Tangents.Add(tangent.ToAssimpVector3D());
                        }

                        // If the bone map does not already have the bone add it
                        if (!boneMap.ContainsKey(boneIndice))
                        {
                            var aiBone = new Bone();
                            var boneNode = boneArray[boneIndice];
                            aiBone.Name = boneNode.Name;

                            Matrix4x4.Invert(transform, out Matrix4x4 transformInverse);
                            aiBone.OffsetMatrix = transformInverse.ToAssimpMatrix4x4();
                            boneMap.Add(boneIndice, aiBone);
                        }

                        // Add this vertex weight to it's bone
                        boneMap[boneIndice].VertexWeights.Add(new VertexWeight(vertexIndex, 1f));
                    }
                    else
                    {
                        newMesh.Vertices.Add(vertex.Position.ToAssimpVector3D());
                        newMesh.Normals.Add(vertex.Normal.ToAssimpVector3D());
                        newMesh.BiTangents.Add(vertex.Bitangent.ToAssimpVector3D());
                        foreach (var tangent in vertex.Tangents)
                        {
                            newMesh.Tangents.Add(tangent.ToAssimpVector3D());
                        }
                    }

                    // Add UVs
                    if (vertex.UVs.Count > 0)
                    {
                        var uv1 = vertex.UVs[0];
                        var aiTextureCoordinate = new Vector3D(uv1.X, uv1.Y, 0f);
                        newMesh.TextureCoordinateChannels[0].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Vector3D(1, 1, 1);
                        newMesh.TextureCoordinateChannels[0].Add(aiTextureCoordinate);
                    }

                    if (vertex.UVs.Count > 1)
                    {
                        var uv2 = vertex.UVs[1];
                        var aiTextureCoordinate = new Vector3D(uv2.X, uv2.Y, 0f);
                        newMesh.TextureCoordinateChannels[1].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Vector3D(1, 1, 1);
                        newMesh.TextureCoordinateChannels[1].Add(aiTextureCoordinate);
                    }

                    if (vertex.UVs.Count > 2)
                    {
                        var uv3 = vertex.UVs[2];
                        var aiTextureCoordinate = new Vector3D(uv3.X, uv3.Y, 0f);
                        newMesh.TextureCoordinateChannels[2].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Vector3D(1, 1, 1);
                        newMesh.TextureCoordinateChannels[2].Add(aiTextureCoordinate);
                    }

                    if (vertex.UVs.Count > 3)
                    {
                        var uv4 = vertex.UVs[3];
                        var aiTextureCoordinate = new Vector3D(uv4.X, uv4.Y, 0f);
                        newMesh.TextureCoordinateChannels[3].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Vector3D(1, 1, 1);
                        newMesh.TextureCoordinateChannels[3].Add(aiTextureCoordinate);
                    }

                    for (int uv = 0; uv < newMesh.TextureCoordinateChannelCount; uv++)
                    {
                        newMesh.UVComponentCount[uv] = 2;
                    }

                    // Add vertex colors
                    if (vertex.Colors.Count > 0)
                    {
                        var color = vertex.Colors[0];
                        newMesh.VertexColorChannels[0].Add(new Color4D(color.R, color.G, color.B, color.A));
                    }

                    if (vertex.Colors.Count > 1)
                    {
                        var color = vertex.Colors[1];
                        newMesh.VertexColorChannels[1].Add(new Color4D(color.R, color.G, color.B, color.A));
                    }
                }

                newMesh.Bones.AddRange(boneMap.Values);

                // Add faces
                foreach (var faceset in mesh.FaceSets)
                {
                    var indices = faceset.Triangulate(mesh.Vertices.Count < ushort.MaxValue);
                    for (int i = 0; i < indices.Count - 2; i += 3)
                    {
                        newMesh.Faces.Add(new Face(new int[] { indices[i], indices[i + 1], indices[i + 2] }));
                    }
                }

                newMesh.MaterialIndex = mesh.MaterialIndex;
                scene.Meshes.Add(newMesh);

                var meshNode = new Node($"Mesh_{meshIndex}", scene.RootNode);
                meshNode.Transform = Assimp.Matrix4x4.Identity;

                meshNode.MeshIndices.Add(meshIndex);
                scene.RootNode.Children.Add(meshNode);
            }

            return scene;
        }

        /// <summary>
        /// Converts an MDL4 into an Assimp Scene.
        /// </summary>
        public static Scene ToAssimpScene(MDL4 model)
        {
            var scene = new Scene();
            var aiRootNode = new Node();
            scene.RootNode = aiRootNode;
            aiRootNode.Transform = Assimp.Matrix4x4.Identity;

            // Add bone nodes
            Node[] boneArray = new Node[model.Bones.Count];
            for (int i = 0; i < model.Bones.Count; i++)
            {
                var bone = model.Bones[i];
                Node parentNode;
                if (bone.ParentIndex == -1)
                {
                    parentNode = aiRootNode;
                }
                else
                {
                    parentNode = boneArray[bone.ParentIndex];
                }
                var aiNode = new Node(bone.Name, parentNode);

                // Get local transform
                aiNode.Transform = bone.ComputeLocalTransform().ToAssimpMatrix4x4();

                parentNode.Children.Add(aiNode);
                boneArray[i] = aiNode;
            }

            // Add materials
            foreach (var material in model.Materials)
            {
                Material newMaterial = new()
                {
                    Name = material.Name,
                };

                scene.Materials.Add(newMaterial);
            }

            // Add meshes
            for (int meshIndex = 0; meshIndex < model.Meshes.Count; meshIndex++)
            {
                var mesh = model.Meshes[meshIndex];
                var newMesh = new Mesh("Mesh_M" + meshIndex, PrimitiveType.Triangle);
                bool hasBones = mesh.BoneIndices.Length != 0;

                // Prepare a bone map
                var boneMap = new Dictionary<int, Bone>(mesh.BoneIndices.Length);

                // Add vertices
                for (int vertexIndex = 0; vertexIndex < mesh.Vertices.Count; vertexIndex++)
                {
                    var vertex = mesh.Vertices[vertexIndex];

                    // If the mesh has bones set the weights of this vertex to the correct bone
                    if (hasBones)
                    {
                        // Get the local bone indice from NormalW then the bone array bone indice from the mesh
                        var boneIndice = mesh.BoneIndices[(int)vertex.Normal.W];
                        var bone = model.Bones[boneIndice];
                        var transform = bone.ComputeTransform(model.Bones);

                        newMesh.Vertices.Add(vertex.Position.ToAssimpVector3D(transform));
                        newMesh.Normals.Add(vertex.Normal.ToAssimpVector3D());
                        newMesh.BiTangents.Add(vertex.Bitangent.ToAssimpVector3D());
                        newMesh.Tangents.Add(vertex.Tangent.ToAssimpVector3D());

                        // If the bone map does not already have the bone add it
                        if (!boneMap.ContainsKey(boneIndice))
                        {
                            var aiBone = new Bone();
                            var boneNode = boneArray[boneIndice];
                            aiBone.Name = boneNode.Name;

                            Matrix4x4.Invert(transform, out Matrix4x4 transformInverse);

                            aiBone.OffsetMatrix = transformInverse.ToAssimpMatrix4x4();
                            boneMap.Add(boneIndice, aiBone);
                        }

                        // Add this vertex weight to it's bone
                        boneMap[boneIndice].VertexWeights.Add(new VertexWeight(vertexIndex, 1f));
                    }
                    else
                    {
                        newMesh.Vertices.Add(vertex.Position.ToAssimpVector3D());
                        newMesh.Normals.Add(vertex.Normal.ToAssimpVector3D());
                        newMesh.BiTangents.Add(vertex.Bitangent.ToAssimpVector3D());
                        newMesh.Tangents.Add(vertex.Tangent.ToAssimpVector3D());
                    }

                    // Add UVs
                    if (vertex.UVs.Count > 0)
                    {
                        var uv1 = vertex.UVs[0];
                        var aiTextureCoordinate = new Vector3D(uv1.X, uv1.Y, 0f);
                        newMesh.TextureCoordinateChannels[0].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Vector3D(1, 1, 1);
                        newMesh.TextureCoordinateChannels[0].Add(aiTextureCoordinate);
                    }

                    if (vertex.UVs.Count > 1)
                    {
                        var uv2 = vertex.UVs[1];
                        var aiTextureCoordinate = new Vector3D(uv2.X, uv2.Y, 0f);
                        newMesh.TextureCoordinateChannels[1].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Vector3D(1, 1, 1);
                        newMesh.TextureCoordinateChannels[1].Add(aiTextureCoordinate);
                    }

                    if (vertex.UVs.Count > 2)
                    {
                        var uv3 = vertex.UVs[2];
                        var aiTextureCoordinate = new Vector3D(uv3.X, uv3.Y, 0f);
                        newMesh.TextureCoordinateChannels[2].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Vector3D(1, 1, 1);
                        newMesh.TextureCoordinateChannels[2].Add(aiTextureCoordinate);
                    }

                    if (vertex.UVs.Count > 3)
                    {
                        var uv4 = vertex.UVs[3];
                        var aiTextureCoordinate = new Vector3D(uv4.X, uv4.Y, 0f);
                        newMesh.TextureCoordinateChannels[3].Add(aiTextureCoordinate);
                    }
                    else
                    {
                        var aiTextureCoordinate = new Vector3D(1, 1, 1);
                        newMesh.TextureCoordinateChannels[3].Add(aiTextureCoordinate);
                    }

                    for (int uv = 0; uv < newMesh.TextureCoordinateChannelCount; uv++)
                    {
                        newMesh.UVComponentCount[uv] = 2;
                    }

                    // Add vertex colors
                    var color = vertex.Color;
                    newMesh.VertexColorChannels[0].Add(new Color4D(color.R, color.G, color.B, color.A));
                }

                newMesh.Bones.AddRange(boneMap.Values);

                // Add faces
                var faceVertexIndices = mesh.GetFaceIndices();
                foreach (int[] indices in faceVertexIndices)
                {
                    newMesh.Faces.Add(new Face(indices));
                }

                newMesh.MaterialIndex = mesh.MaterialIndex;
                scene.Meshes.Add(newMesh);

                var meshNode = new Node($"Mesh_{meshIndex}", scene.RootNode);
                meshNode.Transform = Assimp.Matrix4x4.Identity;

                meshNode.MeshIndices.Add(meshIndex);
                scene.RootNode.Children.Add(meshNode);
            }

            return scene;
        }

        /// <summary>
        /// Converts an SMD4 into an Assimp Scene.
        /// </summary>
        public static Scene ToAssimpScene(SMD4 model)
        {
            var scene = new Scene();
            var aiRootNode = new Node();
            scene.RootNode = aiRootNode;
            aiRootNode.Transform = Assimp.Matrix4x4.Identity;

            // Add bone nodes
            Node[] boneArray = new Node[model.Bones.Count];
            for (int i = 0; i < model.Bones.Count; i++)
            {
                var bone = model.Bones[i];
                Node parentNode;
                if (bone.ParentIndex == -1)
                {
                    parentNode = aiRootNode;
                }
                else
                {
                    parentNode = boneArray[bone.ParentIndex];
                }
                var aiNode = new Node(bone.Name, parentNode);

                // Get local transform
                aiNode.Transform = bone.ComputeLocalTransform().ToAssimpMatrix4x4();

                parentNode.Children.Add(aiNode);
                boneArray[i] = aiNode;
            }

            // Add materials
            Material newMaterial = new()
            {
                Name = "default"
            };
            scene.Materials.Add(newMaterial);

            // Add meshes
            for (int meshIndex = 0; meshIndex < model.Meshes.Count; meshIndex++)
            {
                var mesh = model.Meshes[meshIndex];
                var newMesh = new Mesh("Mesh_M" + meshIndex, PrimitiveType.Triangle);
                bool hasBones = mesh.BoneIndices.Length != 0;

                // Prepare a bone map
                var boneMap = new Dictionary<int, Bone>(mesh.BoneIndices.Length);

                // Add vertices
                for (int vertexIndex = 0; vertexIndex < mesh.Vertices.Count; vertexIndex++)
                {
                    var vertex = mesh.Vertices[vertexIndex];

                    // If the mesh has bones set the weights of this vertex to the correct bone
                    if (hasBones)
                    {
                        // Get the local bone indice from BoneIndex then the bone array bone indice from the mesh
                        var boneIndice = mesh.BoneIndices[vertex.BoneIndices[0]];
                        var bone = model.Bones[boneIndice];
                        var transform = bone.ComputeTransform(model.Bones);

                        newMesh.Vertices.Add(vertex.Position.ToAssimpVector3D(transform));

                        // If the bone map does not already have the bone add it
                        if (!boneMap.ContainsKey(boneIndice))
                        {
                            var aiBone = new Bone();
                            var boneNode = boneArray[boneIndice];
                            aiBone.Name = boneNode.Name;

                            Matrix4x4.Invert(transform, out Matrix4x4 transformInverse);
                            aiBone.OffsetMatrix = transformInverse.ToAssimpMatrix4x4();
                            boneMap.Add(boneIndice, aiBone);
                        }

                        // Add this vertex weight to it's bone
                        boneMap[boneIndice].VertexWeights.Add(new VertexWeight(vertexIndex, 1f));
                    }
                    else
                    {
                    
                        newMesh.Vertices.Add(vertex.Position.ToAssimpVector3D());
                    }
                }

                newMesh.Bones.AddRange(boneMap.Values);

                // Add faces
                var faceVertexIndices = mesh.GetFaceIndices();
                foreach (int[] indices in faceVertexIndices)
                {
                    newMesh.Faces.Add(new Face(indices));
                }

                newMesh.MaterialIndex = mesh.MaterialIndex;
                scene.Meshes.Add(newMesh);

                var meshNode = new Node($"Mesh_{meshIndex}", scene.RootNode);
                meshNode.Transform = Assimp.Matrix4x4.Identity;

                meshNode.MeshIndices.Add(meshIndex);
                scene.RootNode.Children.Add(meshNode);
            }

            return scene;
        }

        /// <summary>
        /// Get the extension for a type.
        /// </summary>
        /// <param name="type">A supported Assimp type as a string.</param>
        /// <returns>A file extension without the dot.</returns>
        public static string GetExtension(string type)
        {
            return type switch
            {
                "fbx" => "fbx",
                "fbxa" => "fbx",
                "collada" => "dae",
                "obj" => "obj",
                _ => type
            };
        }
    }
}
