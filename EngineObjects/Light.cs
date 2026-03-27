using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

public class Light
{
    [JsonIgnore]
    public int BufferHandle;
    public Vector3 Pos;
    public Vector3 Ambient;
    public Vector3 Diffuse;
    public Vector3 Specular;

    public Light(Vector3 Pos, Vector3 Ambient, Vector3 Diffuse, Vector3 Specular)
    {
        this.Pos = Pos;
        this.Ambient = Ambient;
        this.Diffuse = Diffuse;
        this.Specular = Specular;
    }
}
