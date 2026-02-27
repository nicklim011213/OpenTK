using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;

public class Light
{
    public int BufferHandle;
    public Light(Vector3 Pos, Vector3 Ambient, Vector3 Diffuse, Vector3 Specular)
    {
        GL.CreateBuffers(1, out BufferHandle);
        LightData LightInfo = new LightData
        {
            Pos = Pos,
            Ambient = Ambient,
            Diffuse = Diffuse,
            Specular = Specular
        };
        float[] Data = LightInfo.Data();

        GL.NamedBufferStorage(BufferHandle, sizeof(float) * 16, Data, BufferStorageFlags.None);
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, BufferHandle);
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct LightData
{
    public Vector3 Pos;
    private float Pad1 = 0.0f;
    public Vector3 Ambient;
    private float Pad2 = 0.0f;
    public Vector3 Diffuse;
    private float Pad3 = 0.0f;
    public Vector3 Specular;
    private float Pad4 = 0.0f;

    public float[] Data()
    {
        return new float[] { Pos.X, Pos.Y, Pos.Z, Pad1,
                             Ambient.X, Ambient.Y, Ambient.Z, Pad2,
                             Diffuse.X, Diffuse.Y, Diffuse.Z, Pad3,
                             Specular.X, Specular.Y, Specular.Z, Pad4 };
    }

    public LightData() { }
}
