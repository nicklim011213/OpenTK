using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTKEngine.Exposed_Features.EngineObjects;


namespace OpenTKEngine.Obscured_Features.IO
{
    public static class Camera_Input
    {
        internal static bool FirstMove = true;
        internal static Vector2 LastMousePos;
        internal static float Pitch;
        internal static float Yaw = -MathHelper.PiOver2;

        public static void UpdateMovement(FrameEventArgs args, KeyboardState keyboardState)
        {
            if (keyboardState.IsKeyDown(Keys.W))
            {
                Camera.Pos += Camera.CamSpeed * Camera.Front * (float)args.Time;
            }
            else if (keyboardState.IsKeyDown(Keys.S))
            {
                Camera.Pos -= Camera.CamSpeed * Camera.Front * (float)args.Time;
            }
            else if (keyboardState.IsKeyDown(Keys.D))
            {
                Camera.Pos += Camera.Right * Camera.CamSpeed * (float)args.Time;
            }
            else if (keyboardState.IsKeyDown(Keys.A))
            {
                Camera.Pos -= Camera.Right * Camera.CamSpeed * (float)args.Time;
            }
            else if (keyboardState.IsKeyDown(Keys.Space))
            {
                Camera.Pos += Camera.Up * Camera.CamSpeed * (float)args.Time;
            }
            else if (keyboardState.IsKeyDown(Keys.LeftControl))
            {
                Camera.Pos -= Camera.Up * Camera.CamSpeed * (float)args.Time;
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

                    Pitch -= DeltaY * Camera.Sensativity;
                    Yaw += DeltaX * Camera.Sensativity;

                    Pitch = MathHelper.Clamp(Pitch, MathHelper.DegreesToRadians(-85.0f), MathHelper.DegreesToRadians(85.0f));
                }
            }
        }
        public static void UpdateVectors()
        {
            Camera.Front = new Vector3(MathF.Cos(Camera_Input.Pitch) * MathF.Cos(Camera_Input.Yaw), MathF.Sin(Camera_Input.Pitch), MathF.Cos(Camera_Input.Pitch) * MathF.Sin(Camera_Input.Yaw));
            Camera.Front = Vector3.Normalize(Camera.Front);

            Camera.Right = Vector3.Normalize(Vector3.Cross(Camera.Front, Vector3.UnitY));
            Camera.Up = Vector3.Normalize(Vector3.Cross(Camera.Right, Camera.Front));
        }
    }
}
