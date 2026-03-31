using OpenTK.Mathematics;
using OpenTKEngine.Obsecured_Features.Assets;

namespace OpenTKEngine.Obsecured_Features.Rendering
{
    public class ObjectRenderData
    {
        public long DiffuseTextureHandle = 0;
        public long SpecularTextureHandle = 0;

        public List<DrawSubmissionHandler> DrawSubmissions = [];

        public ObjectRenderData(string Filepath)
        {
            ModelData data = ObjectLoading.ReadModel(Filepath);
            List<MeshData> Meshes = data.Meshes;

            foreach (var Mesh in Meshes)
            {
                List<float> VertexData = [];
                List<int> IndexData = [];
                int vertexOffset = VertexData.Count / 8;

                // Gather Vertex Data
                for (int i = 0; i != Mesh.Vertices.Count; i += 3)
                {
                    int vertexIndex = i / 3;

                    VertexData.AddRange(Mesh.Vertices.GetRange(i, 3));
                    VertexData.AddRange(Mesh.Normals.GetRange(i, 3));
                    VertexData.AddRange(Mesh.UVs.GetRange(vertexIndex * 2, 2));
                }

                // Gather Index Data
                foreach (var index in Mesh.Indices)
                {
                    IndexData.Add(index + vertexOffset);
                }

                DrawSubmissionHandler Drawer = new(VertexData, IndexData);
                // Create Textures in Cache
                if (Mesh.DiffuseFilePath is string DiffusePath)
                {
                    Texture.CreateOrRetrieveTexture(DiffusePath, 0);
                    Drawer.AddTexture(Texture.TextureLookup[DiffusePath], 'D');
                }

                if (Mesh.SpecularFilePath is string SpecularPath)
                {
                    Texture.CreateOrRetrieveTexture(SpecularPath, 0);
                    Drawer.AddTexture(Texture.TextureLookup[SpecularPath], 'S');
                }

                DrawSubmissions.Add(Drawer);
            }
        }
    }
}
