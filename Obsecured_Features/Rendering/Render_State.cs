using OpenTK.Graphics.OpenGL4;
using System.Runtime.InteropServices;

namespace OpenTKEngine.Obsecured_Features.Rendering
{
    public static class GeometryBuffer
    {
        // VAO Data
        public static int VAOHandle;

        // VBO Data
        private static int VBOHandle;
        private static List<float> VBOData = [];
        private static float[] VBODataCache = [];
        private static bool VBOCacheDirty = false;

        // EBO Data
        private static int EBOHandle;
        private static List<int> EBOData = [];
        private static int[] EBODataCache = [];
        private static bool EBOCacheDirty = false;

        // Buffers
        public static int MeshDrawDataBuffer;
        public static List<MeshDrawData> MeshDrawDataBufferData = [];

        // Command Buffer Data
        public static int CommandBuffer;
        public static List<DrawCommand> DrawCommandList = [];
        private static DrawCommand[] DrawCommandCache = [];
        private static bool DrawCommandCacheDirty = false;

        public static void Init()
        {

            // Create Vertex Array Vertex Buffer and Element Buffer
            GL.CreateVertexArrays(1, out VAOHandle);
            GL.CreateBuffers(1, out VBOHandle);
            GL.CreateBuffers(1, out EBOHandle);

            // Init VAO Vertex Data Format
            GL.VertexArrayVertexBuffer(VAOHandle, 0, VBOHandle, 0, sizeof(float) * 8);
            GL.VertexArrayAttribFormat(VAOHandle, 0, 3, VertexAttribType.Float, normalized: false, 0);
            GL.VertexArrayAttribBinding(VAOHandle, 0, 0);

            GL.VertexArrayAttribFormat(VAOHandle, 1, 3, VertexAttribType.Float, normalized: false, sizeof(float) * 3);
            GL.VertexArrayAttribBinding(VAOHandle, 1, 0);

            GL.VertexArrayAttribFormat(VAOHandle, 2, 2, VertexAttribType.Float, normalized: false, sizeof(float) * 6);
            GL.VertexArrayAttribBinding(VAOHandle, 2, 0);

            GL.EnableVertexArrayAttrib(VAOHandle, 0);
            GL.EnableVertexArrayAttrib(VAOHandle, 1);
            GL.EnableVertexArrayAttrib(VAOHandle, 2);

            // Bind EBO TO VAO
            GL.VertexArrayElementBuffer(VAOHandle, EBOHandle);

            // Create a draw command buffer
            GL.CreateBuffers(1, out CommandBuffer);
            GL.CreateBuffers(1, out MeshDrawDataBuffer);
        }

        //Appends VBO data and marks to refresh cache
        public static void AppendToVBOData(ICollection<float> Data, out int StartOffset, out int Size)
        {
            StartOffset = VBOData.Count;
            Size = Data.Count;
            VBOCacheDirty = true;
            VBOData.AddRange(Data);
        }

        //Appends EBO data and marks to refresh cache
        public static void AppendToEBOData(ICollection<int> Data, out int StartOffset, out int Size)
        {
            StartOffset = EBOData.Count;
            Size = Data.Count;
            EBOCacheDirty = true;
            EBOData.AddRange(Data);
        }

        public static void AppendDrawCall(DrawCommand Command)
        {
            DrawCommandCacheDirty = true;
            DrawCommandList.Add(Command);
        }

        // Upload data to GPU buffer
        public static void Upload()
        {
            // Upload VBO Data
            if (VBOCacheDirty)
            {
                VBODataCache = [.. VBOData];
                VBOCacheDirty = false;
                GL.NamedBufferData(VBOHandle, VBODataCache.Length * sizeof(float), VBODataCache, BufferUsageHint.DynamicDraw);
            }

            // Upload EBO Data
            if (EBOCacheDirty)
            {
                EBODataCache = [.. EBOData];
                EBOCacheDirty = false;
                GL.NamedBufferData(EBOHandle, EBODataCache.Length * sizeof(int), EBODataCache, BufferUsageHint.DynamicDraw);
            }

            // Upload Draw Command List
            if (DrawCommandCacheDirty)
            {
                DrawCommandCache = DrawCommandList.ToArray();
                DrawCommandCacheDirty = false;
                unsafe
                {
                    GL.NamedBufferData(CommandBuffer, Marshal.SizeOf<DrawCommand>() * DrawCommandCache.Length, DrawCommandCache, BufferUsageHint.StaticDraw);
                }
            }

            // Upload MeshTextureDataBuffer
            var DrawDataBufferData = MeshDrawDataBufferData.ToArray();
            unsafe
            {
                GL.NamedBufferData(MeshDrawDataBuffer, Marshal.SizeOf<MeshDrawData>() * DrawDataBufferData.Length, DrawDataBufferData, BufferUsageHint.StaticDraw);
            }
        }
    }
}
