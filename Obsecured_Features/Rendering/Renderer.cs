using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;
using OpenTKEngine.Exposed_Features.EngineObjects;
using OpenTKEngine.Obsecured_Features.Assets;
using OpenTKEngine.Obsecured_Features.IO;

namespace OpenTKEngine.Obsecured_Features.Rendering
{
    public class Renderer
    {
        public Stage Stage;
        public Shader DefaultShader;
        public FrameBuffer? FrameBuffer;
        public ShadowMapper? ShadowMapper;

        public Renderer(Stage Stage, Shader Shader)
        {
            this.Stage = Stage;
            this.DefaultShader = Shader;
            GL.CreateBuffers(1, out this.Stage.Lights[0].BufferHandle);
            GL.NamedBufferStorage(this.Stage.Lights[0].BufferHandle, Marshal.SizeOf<LightData>(), IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
        }

        public void UploadStageData()
        {
            if (Stage is not null)
            {
                // Uploads each Actor to the GPU
                foreach (var Actor in Stage.Actors)
                {
                    foreach (var DrawSubmission in Actor.Internal_Model.DrawSubmissions)
                    {
                        DrawSubmission.UploadMeshTransform(Actor.Position, Actor.Scale, Actor.Rotation);
                        DrawSubmission.Submit();
                    }
                }

                // Uploads first Light to the GPU
                LightData UploadData = new()
                {
                    Pos = new Vector4(Stage.Lights[0].Pos, 0.0f),
                    Ambient = new Vector4(Stage.Lights[0].Ambient, 0.0f),
                    Diffuse = new Vector4(Stage.Lights[0].Diffuse, 0.0f),
                    Specular = new Vector4(Stage.Lights[0].Specular, 0.0f),
                };

                unsafe
                {
                    GL.NamedBufferSubData(Stage.Lights[0].BufferHandle, 0, Marshal.SizeOf<LightData>(), ref UploadData);
                }
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, Stage.Lights[0].BufferHandle);
            }
        }

        private void UpdateSceneData()
        {
            // Crate Matrix for Shadow Mapping
            Matrix4 LightProj = Matrix4.CreateOrthographic(20.0f, 20.0f, 0.1f, 100.0f);
            Matrix4 LightView = Matrix4.LookAt(Stage.Lights[0].Pos, new Vector3(0.0f, 0.0f, 0.0f), new Vector3(0.0f, 1.0f, 0.0f));
            Matrix4 LightSpaceMatrix = LightView * LightProj;

            GL.UniformMatrix4(GL.GetUniformLocation(DefaultShader.Handle, "U_Perp"), transpose: false, ref Camera_Renderer.Perp);
            GL.UniformMatrix4(GL.GetUniformLocation(DefaultShader.Handle, "U_View"), transpose: false, ref Camera_Renderer.ViewMatrix);
            GL.Uniform4(GL.GetUniformLocation(DefaultShader.Handle, "U_camPos"), Camera.Pos.X, Camera.Pos.Y, Camera.Pos.Z, 1.0f);
            GL.UniformMatrix4(GL.GetUniformLocation(DefaultShader.Handle, "U_LightSpaceMatrix"), transpose: false, ref LightSpaceMatrix);

            GL.Uniform1(GL.GetUniformLocation(DefaultShader.Handle, "Shininess"), 32.0f);
        }

        public void Render()
        {
            // Update Camera Data
            Camera_Input.UpdateVectors();
            Camera_Renderer.UpdateCameraViewMatrix();

            if (ShadowMapper is not null)
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

            if (FrameBuffer is not null)
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, FrameBuffer.FrameBufferHandle);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
                GL.Enable(EnableCap.DepthTest);
            }

            MainRenderPass();

            if (FrameBuffer is not null)
            {
                GL.BindFramebuffer(FramebufferTarget.ReadFramebuffer, FrameBuffer.FrameBufferHandle);
                GL.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

                GL.Disable(EnableCap.DepthTest);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.BindTextureUnit(0, FrameBuffer.FrameBufferTextureHandle);

                FrameBuffer.Draw();
            }
        }

        public static void MainRenderPass()
        {
            // Upload Commands, VBO and EBO
            GeometryBuffer.Upload();

            // Bind VAO Commands and Mesh SSBO Buffer
            GL.BindVertexArray(GeometryBuffer.VAOHandle);
            GL.BindBuffer(BufferTarget.DrawIndirectBuffer, GeometryBuffer.CommandBuffer);
            GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, GeometryBuffer.MeshDrawDataBuffer);

            // Draw
            GL.MultiDrawElementsIndirect(PrimitiveType.Triangles, DrawElementsType.UnsignedInt, IntPtr.Zero, GeometryBuffer.DrawCommandList.Count, 0);
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
            GeometryBuffer.AppendToVBOData(VertexData, out VBOOffset, out VBOSize);
            GeometryBuffer.AppendToEBOData(IndexData, out EBOOffset, out EBOSize);

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
            GeometryBuffer.MeshDrawDataBufferData.Add(this.MeshDraw);
            GeometryBuffer.AppendDrawCall(this.DrawCommand);
        }
    }
}
