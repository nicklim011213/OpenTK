using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

public static class FrameBuffer
{
    public static float[] FrameBufferData = {
        // positions   // texCoords
        -1.0f,  1.0f,  0.0f, 1.0f,
        -1.0f, -1.0f,  0.0f, 0.0f,
         1.0f, -1.0f,  1.0f, 0.0f,

        -1.0f,  1.0f,  0.0f, 1.0f,
         1.0f, -1.0f,  1.0f, 0.0f,
         1.0f,  1.0f,  1.0f, 1.0f
    };

    // Try changing the fragment shader to get different effects
    public static Shader FrameBufferShader = new Shader("VertexFB.vs", "FragmentFB.fs");
    public static uint FrameBufferHandle = 0;
    public static uint RenderBufferHandle = 0;
    public static bool PreRenderDone = false;
    public static int TextureHandle = 0;
    public static int VAO = 0;
    public static int VBO = 0;

    public static void PreRender()
    {
        // Create Frame Buffer
        GL.CreateFramebuffers(1, out FrameBufferHandle);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);
        TextureHandle = Texture.GenerateFrameBufferTexture(FrameBufferHandle);

        // Set up framebuffer VAO
        GL.CreateVertexArrays(1, out VAO);
        GL.CreateBuffers(1, out VBO);

        // Upload Frame Buffer VBO
        GL.NamedBufferData(VBO, FrameBufferData.Length * sizeof(float), FrameBufferData, BufferUsageHint.StaticDraw);
        GL.VertexArrayVertexBuffer(VAO, 0, VBO, IntPtr.Zero, sizeof(float) * 4);

        // Set Up XY Pos
        GL.EnableVertexArrayAttrib(VAO, 0);
        GL.VertexArrayAttribFormat(VAO,0,2,VertexAttribType.Float,false,0);
        GL.VertexArrayAttribBinding(VAO, 0, 0);

        // Set up Texture UV
        GL.EnableVertexArrayAttrib(VAO, 1);
        GL.VertexArrayAttribFormat(VAO,1,2,VertexAttribType.Float,false,sizeof(float) * 2);
        GL.VertexArrayAttribBinding(VAO, 1, 0);

        PreRenderDone = true;
    }
}

public partial class Stage
{
    uint ShadowMapFBHandle;
    uint DepthTextureHandle;
    bool ShadowMapPassPre = false;
    Shader ShadowMappingShader = new Shader("Depth.vs", "Depth.fs");
    public Light light;


    public void Render()
    {
        if (!FrameBuffer.PreRenderDone)
        {
            FrameBuffer.PreRender();
        }
        ShadowMappingPass();

        // Bind Frame Buffer
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBuffer.FrameBufferHandle);
        GL.Viewport(0, 0, 1920, 1080);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
        GL.Enable(EnableCap.DepthTest);

        // Light Space Matrix
        Matrix4 LightProj = Matrix4.CreateOrthographic(50.0f, 50.0f, 0.1f, 100.0f);
        Matrix4 LightView = Matrix4.LookAt(light.Pos, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
        Matrix4 LightSpaceMatrix = LightView * LightProj;

        Camera.UpdateCameraViewMatrix();
        Camera.RefreshUniformData();

        foreach (var Instance in Instances)
        {
            // Select Shader
            Instance.Model.FallBackShader.Use();
            var Shader = Instance.Model.FallBackShader;
            GL.ProgramUniformMatrix4(Shader.Handle, GL.GetUniformLocation(Shader.Handle, "LightSpaceMatrix"), false, ref LightSpaceMatrix);

            // Shader Transform Uniform
            var TransformMatrix = Matrix4.CreateScale(Instance.Scale) * Matrix4.CreateFromQuaternion(Instance.Rotation) * Matrix4.CreateTranslation(Instance.Position);
            GL.ProgramUniformMatrix4(Shader.Handle, GL.GetUniformLocation(Instance.Model.FallBackShader.Handle, "model"), false, ref TransformMatrix);

            for (int RenderVarIndex = 0; RenderVarIndex != Instance.Model.RenderVars.Count; RenderVarIndex++)
            {
                // Retrieve Mesh
                var Mesh = Instance.Model.Meshes[RenderVarIndex];
                var RenderVars = Instance.Model.RenderVars[RenderVarIndex];

                // Set Mat Textures
                GL.BindTextureUnit(0, Mesh.DiffuseFilePath != null ? Texture.TextureLookup[Mesh.DiffuseFilePath] : Texture.TextureLookup["Default"]);
                GL.BindTextureUnit(1, Mesh.SpecularFilePath != null ? Texture.TextureLookup[Mesh.SpecularFilePath] : Texture.TextureLookup["Default"]);
                GL.ProgramUniform1(Shader.Handle, Shader.MatShininessLoc, 32.0f);

                // Draw Mesh
                GL.BindVertexArray(RenderVars.VAO);
                GL.DrawElements(BeginMode.Triangles, (int)RenderVars.EBOCount, DrawElementsType.UnsignedInt, 0);
            }
        }

        // bind default Frame Buffer
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // Render Frame Buffer
        FrameBuffer.FrameBufferShader.Use();
        GL.BindVertexArray(FrameBuffer.VAO);
        GL.Disable(EnableCap.DepthTest);
        GL.BindTextureUnit(0, FrameBuffer.TextureHandle);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }

    public void ShadowMappingPass()
    {
        if (!ShadowMapPassPre)
        {
            ShadowMappingInit();
        }

        // Bind Frame Buffer with Depth Attachment
        GL.Viewport(0, 0, 2048, 2048);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, ShadowMapFBHandle);
        GL.DrawBuffer(DrawBufferMode.None);
        GL.ReadBuffer(ReadBufferMode.None);
        GL.Clear(ClearBufferMask.DepthBufferBit);
        GL.Enable(EnableCap.DepthTest);

        // Set Up Light Matricies
        //Matrix4 LightProj = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 1920f / 1080f, 0.1f, 100.0f);
        Matrix4 LightProj = Matrix4.CreateOrthographic(40.0f, 40.0f, 0.1f, 100.0f);
        Matrix4 LightView = Matrix4.LookAt(light.Pos, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
        Matrix4 LightSpaceMatrix = LightView * LightProj;

        ShadowMappingShader.Use();
        GL.ProgramUniformMatrix4(ShadowMappingShader.Handle, GL.GetUniformLocation(ShadowMappingShader.Handle, "lightSpaceMatrix"), false, ref LightSpaceMatrix);

        GL.CullFace(TriangleFace.Front);

        // Render Scene with FBO Attached
        foreach (var Instance in Instances)
        {
            var TransformMatrix = Matrix4.CreateScale(Instance.Scale) * Matrix4.CreateFromQuaternion(Instance.Rotation) * Matrix4.CreateTranslation(Instance.Position);
            GL.ProgramUniformMatrix4(ShadowMappingShader.Handle, GL.GetUniformLocation(ShadowMappingShader.Handle, "model"), false, ref TransformMatrix);

            for (int RenderVarIndex = 0; RenderVarIndex != Instance.Model.RenderVars.Count; RenderVarIndex++)
            {
                var Mesh = Instance.Model.Meshes[RenderVarIndex];
                var RenderVars = Instance.Model.RenderVars[RenderVarIndex];

                // Bind Diffuse for alpha Testing
                GL.BindTextureUnit(0, Mesh.DiffuseFilePath != null ? Texture.TextureLookup[Mesh.DiffuseFilePath] : Texture.TextureLookup["Default"]);

                // Draw mesh (no textures needed for depth pass)
                GL.BindVertexArray(RenderVars.VAO);
                GL.DrawElements(BeginMode.Triangles, (int)RenderVars.EBOCount, DrawElementsType.UnsignedInt, 0);
            }
        }

        GL.CullFace(TriangleFace.Back);

        // unbind and continue
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // Bind texture for future use
        GL.BindTextureUnit(2, DepthTextureHandle);

        // Clean up before calling render
        GL.Viewport(0, 0, 1920, 1080);
        GL.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);
    }

    public void ShadowMappingInit()
    {
        // Create a FBO
        GL.CreateFramebuffers(1, out ShadowMapFBHandle);

        // Create a depth map texture
        GL.CreateTextures(TextureTarget.Texture2D, 1, out DepthTextureHandle);
        GL.TextureStorage2D(DepthTextureHandle, 1, SizedInternalFormat.DepthComponent24, 2048, 2048);
        GL.NamedFramebufferTexture(ShadowMapFBHandle, FramebufferAttachment.DepthAttachment, DepthTextureHandle, 0);

        // Shadow Mapping Texture Parameters
        GL.TextureParameter(DepthTextureHandle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TextureParameter(DepthTextureHandle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TextureParameter(DepthTextureHandle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        GL.TextureParameter(DepthTextureHandle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        float[] borderColor = { 1.0f, 1.0f, 1.0f, 1.0f };
        GL.TextureParameter(DepthTextureHandle, TextureParameterName.TextureBorderColor, borderColor);

        // Set flag
        ShadowMapPassPre = true;
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