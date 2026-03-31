using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTKEngine.Exposed_Features.EngineObjects;

namespace OpenTKEngine.Obsecured_Features.Rendering
{
    public static class Camera_Renderer
    {
        // Shader Unifrom Data
        public static Matrix4 ViewMatrix;
        public static Matrix4 Perp = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 1920f / 1080f, 0.1f, 100.0f);
        private static int UBOHandle = -1;

        public static void UpdateCameraViewMatrix()
        {
            ViewMatrix = Matrix4.LookAt(Camera.Pos, Camera.Front + Camera.Pos, Camera.Up);
        }

        public static void Init()
        {
            GL.CreateBuffers(1, out UBOHandle);
            GL.NamedBufferData(UBOHandle, 144, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, UBOHandle);
        }

        public static void RefreshUniformData()
        {
            if (UBOHandle == -1)
            {
                Init();
            }
            GL.NamedBufferSubData(UBOHandle, 0, 64, ref Perp);
            GL.NamedBufferSubData(UBOHandle, 64, 64, ref ViewMatrix);
            Vector4 PaddedCamPos = new(Camera.Pos, 0);
            GL.NamedBufferSubData(UBOHandle, 128, 16, ref PaddedCamPos);
        }
    }
}
