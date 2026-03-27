using OpenTK.Mathematics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct DrawCommand
{
    public uint Count;
    public uint InstanceCount;
    public uint FirstIndex;
    public uint BaseVertex;
    public uint BaseInstance;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct MeshDrawData
{
    public Matrix4 Model;
    public ulong DiffuseTexture;
    public ulong SpecularTexture;
}

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct LightData
{
    public Vector4 Pos;
    public Vector4 Ambient;
    public Vector4 Diffuse;
    public Vector4 Specular;

    public LightData() { }
}

