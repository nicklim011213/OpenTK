using OpenTK.Mathematics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using OpenTK.Graphics.OpenGL4;

public class Actor
{
    [JsonIgnore]
    public Model Internal_Model;
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 Scale;
    public string ModelFilePath;
    public Actor(string ModelFilePath, Vector3? Pos = null, Vector3? Scale = null, Quaternion? Rot = null)
    {
        this.Position = Pos ?? Vector3.Zero;
        this.Rotation = Rot ?? Quaternion.Identity;
        this.Scale = Scale ?? Vector3.One;
        this.ModelFilePath = ModelFilePath;
        Internal_Model = new Model(ModelFilePath, Position, this.Scale, Rotation);
    }
}

public class Model
{
    public long DiffuseTextureHandle = 0;
    public long SpecularTextureHandle = 0;

    public List<DrawSubmissionHandler> DrawSubmissions = new List<DrawSubmissionHandler>();

    public Model(string Filepath, Vector3 Pos, Vector3 Scale, Quaternion Rotation)
    {
        ModelData data = ObjectLoading.ReadModel(Filepath);
        List<MeshData> Meshes = data.Meshes;

        foreach (var Mesh in Meshes)
        {
            List<float> VertexData = new List<float>();
            List<int> IndexData = new List<int>();
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

            DrawSubmissionHandler Drawer = new DrawSubmissionHandler(VertexData, IndexData);
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

public partial class Stage
{
    public List<Light> Lights = [];
    public List<Actor> Actors = new List<Actor>();

    public void UploadActors()
    {
        foreach (var Actor in Actors)
        {
            foreach (var DrawSubmission in Actor.Internal_Model.DrawSubmissions)
            {
                DrawSubmission.UploadMeshTransform(Actor.Position, Actor.Scale, Actor.Rotation);
                DrawSubmission.Submit();
            }
        }
    }

    public void UploadLight()
    {

        LightData UploadData = new LightData()
        {
            Pos = new Vector4(Lights[0].Pos, 0.0f),
            Ambient = new Vector4(Lights[0].Ambient, 0.0f),
            Diffuse = new Vector4(Lights[0].Diffuse, 0.0f),
            Specular = new Vector4(Lights[0].Specular, 0.0f)
        };

        GL.CreateBuffers(1, out Lights[0].BufferHandle);
        unsafe
        {
            GL.NamedBufferStorage(Lights[0].BufferHandle, Marshal.SizeOf<LightData>(), ref UploadData, BufferStorageFlags.DynamicStorageBit);
        }
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, Lights[0].BufferHandle);
    }
}