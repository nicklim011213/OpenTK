using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

public partial class Stage
{
    uint FrameBufferHandle = 0;
    uint RenderBufferHandle = 0;
    bool PreRenderDone = false;

    public void PreRender()
    {
        GL.CreateFramebuffers(1, out FrameBufferHandle);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);
        Texture.GenerateFrameBufferTexture(FrameBufferHandle);

        GL.CreateRenderbuffers(1, out RenderBufferHandle);
        GL.NamedRenderbufferStorage(RenderBufferHandle, RenderbufferStorage.Depth24Stencil8, 1920, 1080);
        GL.NamedFramebufferRenderbuffer(FrameBufferHandle, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, RenderBufferHandle);
    }

    public void Render()
    {
        Camera.UpdateCameraViewMatrix();
        Camera.RefreshUniformData();

        foreach (var Instance in Instances)
        {
            // Select Shader
            Instance.Model.FallBackShader.Use();
            var Shader = Instance.Model.FallBackShader;

            // Shader Transform Uniform
            var TransformMatrix = Matrix4.CreateScale(Instance.Scale) * Matrix4.CreateFromQuaternion(Instance.Rotation) * Matrix4.CreateTranslation(Instance.Position);
            GL.ProgramUniformMatrix4(Shader.Handle, GL.GetUniformLocation(Instance.Model.FallBackShader.Handle, "model"), false, ref TransformMatrix);

            for (int RenderVarIndex = 0; RenderVarIndex != Instance.Model.RenderVars.Count; RenderVarIndex++)
            {
                // Retrieve Mesh
                var Mesh = Instance.Model.Meshes[RenderVarIndex];
                var RenderVars = Instance.Model.RenderVars[RenderVarIndex];

                // Set Mat Textures
                GL.ProgramUniform1(Shader.Handle, Shader.MatDiffuseLoc, 0);
                GL.ProgramUniform1(Shader.Handle, Shader.MatSpecularLoc, 1);
                GL.ProgramUniform1(Shader.Handle, Shader.MatShininessLoc, 32.0f);

                // If it has a diffuse file. load it
                if (Mesh.DiffuseFilePath is string DiffuseFilePath)
                {
                    Texture.CreateOrRetrieveTexture(DiffuseFilePath, 0);
                }

                // If it has Specular file, load it
                if (Mesh.SpecularFilePath is string SpecularFilePath)
                {
                    Texture.CreateOrRetrieveTexture(SpecularFilePath, 1);
                }

                // Draw Mesh
                GL.BindVertexArray(RenderVars.VAO);
                GL.DrawElements(BeginMode.Triangles, (int)RenderVars.EBOCount, DrawElementsType.UnsignedInt, 0);
            }
        }
    }
}
public struct RenderVars
{
    public int Vertex_VBO;
    public int Normal_VBO;
    public int UV_VBO;
    public int EBO;
    public int VAO;
    public int EBOCount;
}
public class Model
{
    public Shader FallBackShader;
    public List<RenderVars> RenderVars = [];
    public List<MeshData> Meshes = [];
    public Model(string FilePath)
    {
        // Default Shader
        FallBackShader = new Shader("Vertex.vs", "Fragment.fs");

        ModelData Data = ObjectLoading.ReadModel(FilePath);
        Meshes = Data.Meshes;

        for (int MeshIndex = 0; MeshIndex != Meshes.Count; MeshIndex++)
        {
            var CurrentMesh = Data.Meshes[MeshIndex];
            RenderVars MeshRenderVars = new RenderVars();

            // Set up buffers
            GL.CreateVertexArrays(1, out MeshRenderVars.VAO);
            GL.CreateBuffers(1, out MeshRenderVars.Vertex_VBO);
            GL.CreateBuffers(1, out MeshRenderVars.Normal_VBO);
            GL.CreateBuffers(1, out MeshRenderVars.UV_VBO);
            GL.CreateBuffers(1, out MeshRenderVars.EBO);

            // Add VBO and EBO Data
            GL.NamedBufferData(MeshRenderVars.Vertex_VBO, CurrentMesh.Vertices.Count * sizeof(float), CurrentMesh.Vertices.ToArray(), BufferUsageHint.StaticDraw);
            GL.NamedBufferData(MeshRenderVars.Normal_VBO, CurrentMesh.Normals.Count * sizeof(float), CurrentMesh.Normals.ToArray(), BufferUsageHint.StaticDraw);
            GL.NamedBufferData(MeshRenderVars.UV_VBO, CurrentMesh.UVs.Count * sizeof(float), CurrentMesh.UVs.ToArray(), BufferUsageHint.StaticDraw);
            GL.NamedBufferData(MeshRenderVars.EBO, CurrentMesh.Indices.Count * sizeof(int), CurrentMesh.Indices.ToArray(), BufferUsageHint.StaticDraw);
            MeshRenderVars.EBOCount = CurrentMesh.Indices.Count;

            // Set up VAO for Vertex
            GL.VertexArrayVertexBuffer(MeshRenderVars.VAO, 0, MeshRenderVars.Vertex_VBO, 0, sizeof(float) * 3);
            GL.VertexArrayAttribFormat(MeshRenderVars.VAO, 0, 3, VertexAttribType.Float, false, 0);
            GL.VertexArrayAttribBinding(MeshRenderVars.VAO, 0, 0);
            GL.EnableVertexArrayAttrib(MeshRenderVars.VAO, 0);

            // Set up VAO for Normal
            GL.VertexArrayVertexBuffer(MeshRenderVars.VAO, 1, MeshRenderVars.Normal_VBO, 0, sizeof(float) * 3);
            GL.VertexArrayAttribFormat(MeshRenderVars.VAO, 1, 3, VertexAttribType.Float, false, 0);
            GL.VertexArrayAttribBinding(MeshRenderVars.VAO, 1, 1);
            GL.EnableVertexArrayAttrib(MeshRenderVars.VAO, 1);

            // Set up VAO for UVs
            GL.VertexArrayVertexBuffer(MeshRenderVars.VAO, 2, MeshRenderVars.UV_VBO, 0, sizeof(float) * 2);
            GL.VertexArrayAttribFormat(MeshRenderVars.VAO, 2, 2, VertexAttribType.Float, false, 0);
            GL.VertexArrayAttribBinding(MeshRenderVars.VAO, 2, 2);
            GL.EnableVertexArrayAttrib(MeshRenderVars.VAO, 2);

            // Attach EBO
            GL.VertexArrayElementBuffer(MeshRenderVars.VAO, MeshRenderVars.EBO);

            if (Meshes[MeshIndex].DiffuseFilePath is string DiffusePath)
            {
                Texture.CreateOrRetrieveTexture(DiffusePath, 0);
            }
            if (Meshes[MeshIndex].SpecularFilePath is string SpecularPath)
            {
                Texture.CreateOrRetrieveTexture(SpecularPath, 1);
            }

            // Store Rendervars with mesh
            RenderVars.Add(MeshRenderVars);
        }
    }
}