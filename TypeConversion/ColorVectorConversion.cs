using Assimp;
using OpenTK.Mathematics;

public static class ColorVectorConversion
{
    public static Vector3 ColorToVector(Color3D Color)
    {
        return new Vector3(Color.R, Color.G, Color.B);
    }
}
