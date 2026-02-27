using OpenTK.Mathematics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public static class Camera
{
    // Direction Vectors
    public static Vector3 Pos = new(0.0f, 0.0f, -10.0f);
    private static Vector3 Front = -Vector3.UnitZ;
    private static Vector3 Up = Vector3.UnitY;
    private static Vector3 Right = Vector3.UnitX;

    // Rotation Values
    private static bool FirstMove = true;
    private static Vector2 LastMousePos;
    private static float Pitch;
    private static float Yaw = -MathHelper.PiOver2;

    // Movement Values
    public static float CamSpeed = 5.0f;
    public static float Sensativity = 0.002f;

    // Shader Unifrom Data
    public static Matrix4 ViewMatrix;
    public static Matrix4 Perp = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(45.0f), 1920f / 1080f, 0.1f, 100.0f);
    private static int UBOHandle = -1;

    public static void UpdateCameraViewMatrix()
    {
        ViewMatrix = Matrix4.LookAt(Pos, Front + Pos, Up);
    }

    public static void UpdateMovement(FrameEventArgs args, KeyboardState keyboardState)
    {
        if (keyboardState.IsKeyDown(Keys.W))
        {
            Pos += CamSpeed * Front * (float)args.Time;
        }
        else if (keyboardState.IsKeyDown(Keys.S))
        {
            Pos -= CamSpeed * Front * (float)args.Time;
        }
        else if (keyboardState.IsKeyDown(Keys.D))
        {
            Pos += Right * CamSpeed * (float)args.Time;
        }
        else if (keyboardState.IsKeyDown(Keys.A))
        {
            Pos -= Right * CamSpeed * (float)args.Time;
        }
        else if (keyboardState.IsKeyDown(Keys.Space))
        {
            Pos += Up * CamSpeed * (float)args.Time;
        }
        else if (keyboardState.IsKeyDown(Keys.LeftControl))
        {
            Pos -= Up * CamSpeed * (float)args.Time;
        }
    }

    public static void UpdateMouseMovement(FrameEventArgs args, MouseState mouseState, bool CursorGrabbed)
    {
        if (CursorGrabbed)
        {
            if (FirstMove)
            {
                FirstMove = false;
                LastMousePos = new Vector2(mouseState.X, mouseState.Y);
            }
            else
            {
                var DeltaX = mouseState.X - LastMousePos.X;
                var DeltaY = mouseState.Y - LastMousePos.Y;
                LastMousePos = new Vector2 { X = mouseState.X, Y = mouseState.Y };

                Pitch -= DeltaY * Sensativity;
                Yaw += DeltaX * Sensativity;

                Pitch = MathHelper.Clamp(Pitch, MathHelper.DegreesToRadians(-85.0f), MathHelper.DegreesToRadians(85.0f));
            }
        }
    }

    public static void UpdateVectors()
    {
        Front = new Vector3(MathF.Cos(Pitch) * MathF.Cos(Yaw), MathF.Sin(Pitch), MathF.Cos(Pitch) * MathF.Sin(Yaw));
        Front = Vector3.Normalize(Front);

        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
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
        Vector4 PaddedCamPos = new Vector4(Pos, 0);
        GL.NamedBufferSubData(UBOHandle, 128, 16, ref PaddedCamPos);
    }
}

