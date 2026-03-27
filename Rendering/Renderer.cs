using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;

public class Renderer
{
    public Stage Stage;
    public Shader DefaultShader;
    public FrameBuffer? FrameBuffer;
    public ShadowMapper? ShadowMapper;
    private void UpdateSceneData()
    {
        // Crate Matrix for Shadow Mapping
        Matrix4 LightProj = Matrix4.CreateOrthographic(20.0f, 20.0f, 0.1f, 100.0f);
        Matrix4 LightView = Matrix4.LookAt(Stage.Lights[0].Pos, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
        Matrix4 LightSpaceMatrix = LightView * LightProj;

        GL.UniformMatrix4(GL.GetUniformLocation(DefaultShader.Handle, "U_Perp"), false, ref Camera.Perp);
        GL.UniformMatrix4(GL.GetUniformLocation(DefaultShader.Handle, "U_View"), false, ref Camera.ViewMatrix);
        GL.Uniform4(GL.GetUniformLocation(DefaultShader.Handle, "U_camPos"), Camera.Pos.X, Camera.Pos.Y, Camera.Pos.Z, 1.0f);
        GL.UniformMatrix4(GL.GetUniformLocation(DefaultShader.Handle, "U_LightSpaceMatrix"), false, ref LightSpaceMatrix);

        GL.Uniform1(GL.GetUniformLocation(DefaultShader.Handle, "Shininess"), 32.0f);
    }

    public void Render()
    {
        // Update Camera Data
        Camera.UpdateVectors();
        Camera.UpdateCameraViewMatrix();

        if (ShadowMapper is ShadowMapper)
        {
            // Shadow Map Setup
            ShadowMapper.PreRender(Stage.Lights[0].Pos);

            // Do the render
            MainRenderPass();

            // Shadow Map Cleanup
            ShadowMapper.PostRender();

            // Use Base Shader Set the DepthMapTexture to binding 2 and uploading the binding to the shader
            DefaultShader.Use();
            GL.BindTextureUnit(2, ShadowMapper.DepthMapTextureHandle);
            GL.Uniform1(GL.GetUniformLocation(DefaultShader.Handle, "shadowMap"), 2);
        }

        // Use Shader
        DefaultShader.Use();
        UpdateSceneData();

        if (FrameBuffer is FrameBuffer)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBuffer.FrameBufferHandle);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
            GL.Enable(EnableCap.DepthTest);
        }

        MainRenderPass();

        if (FrameBuffer is FrameBuffer)
        {
            GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, FrameBuffer.FrameBufferHandle);
            GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

            GL.Disable(EnableCap.DepthTest);
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            GL.BindTextureUnit(0, FrameBuffer.FrameBufferTextureHandle);

            FrameBuffer.Draw();
        }
    }

    public void MainRenderPass()
    {
        // Upload Commands, VBO and EBO
        GlobalRenderVars.Upload();

        // Bind VAO Commands and Mesh SSBO Buffer
        GL.BindVertexArray(GlobalRenderVars.VAOHandle);
        GL.BindBuffer(BufferTarget.DrawIndirectBuffer, GlobalRenderVars.CommandBuffer);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, GlobalRenderVars.MeshDrawDataBuffer);

        // Draw
        GL.MultiDrawElementsIndirect(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, IntPtr.Zero, GlobalRenderVars.DrawCommandList.Count, 0);
    }
}

public class DrawSubmissionHandler
{
    public MeshDrawData MeshDraw;
    public DrawCommand DrawCommand;
    public int VBOOffset;
    public int VBOSize;
    public int EBOOffset;
    public int EBOSize;
    private long SpecularTextureHandle;
    private long DiffuseTextureHandle;

    public DrawSubmissionHandler(List<float> VertexData, List<int> IndexData)
    {
        GlobalRenderVars.AppendToVBOData(VertexData, out VBOOffset, out VBOSize);
        GlobalRenderVars.AppendToEBOData(IndexData, out EBOOffset, out EBOSize);

        DrawCommand = new DrawCommand()
        {
            Count = (uint)EBOSize,
            InstanceCount = 1,
            FirstIndex = (uint)EBOOffset,
            BaseVertex = (uint)(VBOOffset / 8), // Each vertex is 8 seperate floats
            BaseInstance = 0,
        };
    }

    public void AddTexture(int TextureHandle, char Type)
    {
        if (Type == 'S')
        {
            SpecularTextureHandle = GL.Arb.GetTextureHandle(TextureHandle);
            GL.Arb.MakeTextureHandleResident(SpecularTextureHandle);
            this.MeshDraw.SpecularTexture = (ulong)SpecularTextureHandle;
        }
        else if (Type == 'D')
        {
            DiffuseTextureHandle = GL.Arb.GetTextureHandle(TextureHandle);
            GL.Arb.MakeTextureHandleResident(DiffuseTextureHandle);
            this.MeshDraw.DiffuseTexture = (ulong)DiffuseTextureHandle;
        }
    }

    public void UploadMeshTransform(Vector3 Pos, Vector3 Scale, Quaternion Rot)
    {
        this.MeshDraw.Model = Matrix4.CreateScale(Scale) * Matrix4.CreateFromQuaternion(Rot) * Matrix4.CreateTranslation(Pos);
    }

    public void Submit()
    {
        GlobalRenderVars.MeshDrawDataBufferData.Add(this.MeshDraw);
        GlobalRenderVars.AppendDrawCall(this.DrawCommand);
    }
}

public class FrameBuffer
{
    public uint FrameBufferHandle = 0;
    public uint FrameBufferTextureHandle = 0;
    private uint FrameBufferDepthStencilTexture = 0;

    public static int FrameBufferVAO = 0;
    public static int FrameBufferVBO = 0;
    public static float[] FrameBufferVertexData = new float[30]
    {
        -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
         1.0f, -1.0f, 0.0f, 1.0f, 0.0f,
         1.0f,  1.0f, 0.0f, 1.0f, 1.0f,

        -1.0f, -1.0f, 0.0f, 0.0f, 0.0f,
         1.0f,  1.0f, 0.0f, 1.0f, 1.0f,
        -1.0f,  1.0f, 0.0f, 0.0f, 1.0f
    };
    public static Shader FrameBufferShader;

    public FrameBuffer()
    {
        // Create Depth stencil texture and Color Texture
        GL.CreateFramebuffers(1, out FrameBufferHandle);
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBufferHandle);

        GL.CreateTextures(TextureTarget.Texture2D, 1, out FrameBufferTextureHandle);
        GL.TextureStorage2D(FrameBufferTextureHandle, 1, SizedInternalFormat.Rgba16, 1920, 1080);

        GL.TextureParameter(FrameBufferTextureHandle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TextureParameter(FrameBufferTextureHandle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);

        GL.NamedFramebufferTexture(FrameBufferHandle, FramebufferAttachment.ColorAttachment0, FrameBufferTextureHandle, 0);

        GL.CreateTextures(TextureTarget.Texture2D, 1, out FrameBufferDepthStencilTexture);
        GL.TextureStorage2D(FrameBufferDepthStencilTexture, 1, SizedInternalFormat.Depth24Stencil8, 1920, 1080);

        GL.NamedFramebufferTexture(FrameBufferHandle, FramebufferAttachment.DepthStencilAttachment, FrameBufferDepthStencilTexture, 0);
        GL.NamedFramebufferDrawBuffer(FrameBufferHandle, DrawBufferMode.ColorAttachment0);

        // Create VAO VBO for frame buffer quad
        GL.CreateBuffers(1, out FrameBufferVBO);
        GL.CreateVertexArrays(1, out FrameBufferVAO);
        GL.VertexArrayVertexBuffer(FrameBufferVAO, 0, FrameBufferVBO, 0, sizeof(float) * 5);

        // XYZ
        GL.VertexArrayAttribFormat(FrameBufferVAO, 0, 3, VertexAttribType.Float, false, 0);
        GL.VertexArrayAttribBinding(FrameBufferVAO, 0, 0);

        // UV
        GL.VertexArrayAttribFormat(FrameBufferVAO, 1, 2, VertexAttribType.Float, false, sizeof(float) * 3);
        GL.VertexArrayAttribBinding(FrameBufferVAO, 1, 0);

        // Eanble VAO Attributes
        GL.EnableVertexArrayAttrib(FrameBufferVAO, 0);
        GL.EnableVertexArrayAttrib(FrameBufferVAO, 1);

        // Bind Vertex Data
        GL.NamedBufferData(FrameBufferVBO, sizeof(float) * 30, FrameBufferVertexData, BufferUsageHint.StaticDraw);

        //Setup Shader
        FrameBufferShader = new Shader("VertexFB.vs", "FragmentFB.fs");
    }

    public void Draw()
    {
        FrameBufferShader.Use();
        GL.BindVertexArray(FrameBufferVAO);
        GL.DrawArrays(PrimitiveType.Triangles, 0, 6);
    }
}

public class ShadowMapper
{
    public uint DepthMapFrameBufferHandle = 0;
    public uint ShadowResolutionWidth = 2048;
    public uint ShadowResolutionHeight = 2048;
    public uint DepthMapTextureHandle = 0;

    public Shader ShadowMappingShader;

    public ShadowMapper()
    {
        // Create Frame Buffer
        GL.CreateFramebuffers(1, out DepthMapFrameBufferHandle);

        // Create Depth Texture
        GL.CreateTextures(TextureTarget.Texture2D, 1, out DepthMapTextureHandle);
        GL.TextureStorage2D(DepthMapTextureHandle, 1, SizedInternalFormat.DepthComponent24, (int)ShadowResolutionWidth, (int)ShadowResolutionHeight);
        GL.TextureParameter(DepthMapTextureHandle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
        GL.TextureParameter(DepthMapTextureHandle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
        GL.TextureParameter(DepthMapTextureHandle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToBorder);
        GL.TextureParameter(DepthMapTextureHandle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToBorder);
        GL.TextureParameter(DepthMapTextureHandle, TextureParameterName.TextureCompareMode, (int)All.None);

        // Border Color
        float[] borderColor = { 1f, 1f, 1f, 1f };
        GL.TextureParameter(DepthMapTextureHandle, TextureParameterName.TextureBorderColor, borderColor);

        // Bind Frame Buffer and attach texture attachment
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, DepthMapFrameBufferHandle);
        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, TextureTarget.Texture2D, DepthMapTextureHandle, 0);

        // No need to use a color attachment
        GL.ReadBuffer(ReadBufferMode.None);
        GL.DrawBuffer(DrawBufferMode.None);

        // Unbind to clean up
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        ShadowMappingShader = new Shader("ShadowPass.vs", "ShadowPass.fs");
    }

    public void PreRender(Vector3 LightPos)
    {
        // Use and setup Depth Shader
        ShadowMappingShader.Use();
        Matrix4 LightProj = Matrix4.CreateOrthographic(20.0f, 20.0f, 0.1f, 100.0f);
        Matrix4 LightView = Matrix4.LookAt(LightPos, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
        Matrix4 LightSpaceMatrix = LightView * LightProj;
        GL.UniformMatrix4(GL.GetUniformLocation(ShadowMappingShader.Handle, "U_LightSpaceMatrix"), false, ref LightSpaceMatrix);

        // Adjust viewport size to match shadow map res
        GL.Viewport(0, 0, (int)ShadowResolutionWidth, (int)ShadowResolutionHeight);

        // Bind FBO
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, DepthMapFrameBufferHandle);
        GL.Clear(ClearBufferMask.DepthBufferBit);

        // Double Check Depth Testing is Enabled
        GL.Enable(EnableCap.DepthTest);
        GL.DepthFunc(DepthFunction.Less);

        // Peterpanning fix
        GL.CullFace(TriangleFace.Front);

        // Bind SSBO for MeshDrawDataBuffer
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, GlobalRenderVars.MeshDrawDataBuffer);
    }

    public void PostRender()
    {
        // Unbind Shadow Pass Frame Buffer
        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

        // Clear Color and Depth and reset size + Cull
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        GL.Viewport(0, 0, 1920, 1080);
        GL.CullFace(TriangleFace.Back);
    }
}
