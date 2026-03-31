using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OpenTKEngine.Exposed_Features.EngineObjects
{
    public static class Camera
    {
        // Direction Vectors
        public static Vector3 Pos = new(0.0f, 0.0f, 1.0f);

        // Movement Values
        public static float CamSpeed = 5.0f;
        public static float Sensativity = 0.002f;

        // Obsecured Values
        internal static Vector3 Front = -Vector3.UnitZ;
        internal static Vector3 Up = Vector3.UnitY;
        internal static Vector3 Right = Vector3.UnitX;
    }
}

