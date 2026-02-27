using Assimp;
using OpenTK.Windowing.Common.Input;
using System;
using StbImageSharp;
static class ObjectLoading
{
    public static ModelData ReadModel(string FileName)
    {
        string FullPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Models", FileName);
        AssimpContext Context = new();
        Scene Model = Context.ImportFile(FullPath, 
              PostProcessSteps.Triangulate 
            | PostProcessSteps.OptimizeGraph
            //| PostProcessSteps.FlipUVs
            | PostProcessSteps.OptimizeMeshes
            );

        ModelData ModelResult = new ModelData();
        List<Mesh> Meshes = Model.Meshes;

        foreach (Mesh mesh in Meshes)
        {
            MeshData data = new MeshData();

            // UVs Indicies Verticies and Normals
            foreach (var Face in mesh.Faces)
            {
                data.Indices.AddRange(Face.Indices);
            }
            if (mesh.TextureCoordinateChannelCount >= 1)
            {
                data.UVs = mesh.TextureCoordinateChannels[0].SelectMany(Coord => new float[] { Coord.X, Coord.Y }).ToList();
            }
            data.Normals.AddRange(mesh.Normals.SelectMany(Normal => new float[] { Normal.X, Normal.Y, Normal.Z }));
            data.Vertices.AddRange(mesh.Vertices.SelectMany(Vertex => new float[] { Vertex.X, Vertex.Y, Vertex.Z }));

            // Get Mesh Material
            var CurrentMaterial = Model.Materials[mesh.MaterialIndex];

            // Checking Textures
            data.DiffuseFilePath = CurrentMaterial.GetMaterialTextureCount(TextureType.Diffuse) > 0 ? Path.Combine(Path.GetDirectoryName(FullPath), CurrentMaterial.GetMaterialTextures(TextureType.Diffuse)[0].FilePath) : null;
            data.SpecularFilePath = CurrentMaterial.GetMaterialTextureCount(TextureType.Specular) > 0 ? Path.Combine(Path.GetDirectoryName(FullPath), CurrentMaterial.GetMaterialTextures(TextureType.Specular)[0].FilePath) : null;
            data.AmbientFilePath = CurrentMaterial.GetMaterialTextureCount(TextureType.Ambient) > 0 ? Path.Combine(Path.GetDirectoryName(FullPath), CurrentMaterial.GetMaterialTextures(TextureType.Ambient)[0].FilePath) : null;

            // Check Mat Colors
            data.DiffuseColor = CurrentMaterial.HasColorDiffuse ? new Color3D(CurrentMaterial.ColorDiffuse.R, CurrentMaterial.ColorDiffuse.G, CurrentMaterial.ColorDiffuse.B) : new Color3D(0.7f, 0.7f, 0.7f);
            data.SpecularColor = CurrentMaterial.HasColorSpecular ? new Color3D(CurrentMaterial.ColorSpecular.R, CurrentMaterial.ColorSpecular.G, CurrentMaterial.ColorSpecular.B) : new Color3D(0.5f, 0.5f, 0.5f);
            data.AmbientColor = CurrentMaterial.HasColorAmbient ? new Color3D(CurrentMaterial.ColorAmbient.R, CurrentMaterial.ColorAmbient.G, CurrentMaterial.ColorAmbient.B) : new Color3D(0.1f, 0.1f, 0.1f);

            // Add Mesh Data
            ModelResult.Meshes.Add(data);
        }
        return ModelResult;
    }
}

public struct ModelData
{
    public List<MeshData> Meshes = [];
    public ModelData(){}
}

public struct MeshData
{
    public List<float> Vertices = [];
    public List<int> Indices = [];
    public List<float> Normals = [];
    public List<float> UVs = [];

    public string? DiffuseFilePath;
    public string? AmbientFilePath;
    public string? SpecularFilePath;

    public Color3D DiffuseColor;
    public Color3D AmbientColor;
    public Color3D SpecularColor;

    public MeshData(){}
}
