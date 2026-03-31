using StbImageSharp;
using OpenTK.Graphics.OpenGL4;

namespace OpenTKEngine.Obsecured_Features.Assets
{
    public static class Texture
    {
        public static Dictionary<string, int> TextureLookup = [];

        public static void CreateTexture(string Filepath)
        {
            ImageResult TextureFile = ImageResult.FromStream(File.OpenRead(Filepath), ColorComponents.RedGreenBlueAlpha);
            GL.CreateTextures(TextureTarget.Texture2D, 1, out int TextureHandle);
            GL.TextureStorage2D(TextureHandle, 1, SizedInternalFormat.Srgb8Alpha8, TextureFile.Width, TextureFile.Height);
            GL.TextureSubImage2D(TextureHandle, 0, 0, 0, TextureFile.Width, TextureFile.Height, PixelFormat.Rgba, PixelType.UnsignedByte, TextureFile.Data);

            GL.TextureParameter(TextureHandle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TextureParameter(TextureHandle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TextureParameter(TextureHandle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TextureParameter(TextureHandle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.GenerateTextureMipmap(TextureHandle);
            TextureLookup.TryAdd(Filepath, TextureHandle);
        }

        public static void CreateOrRetrieveTexture(string Filepath, int BindingLoc)
        {
            if (TextureLookup.ContainsKey(Filepath))
            {
                GL.BindTextureUnit(BindingLoc, TextureLookup[Filepath]);
            }
            else
            {
                CreateTexture(Filepath);
                GL.BindTextureUnit(BindingLoc, TextureLookup[Filepath]);
            }
        }

        public static void CreateSafeDefault()
        {
            ImageResult TextureFile = ImageResult.FromStream(File.OpenRead("Assets\\Models\\Default.png"), ColorComponents.RedGreenBlueAlpha);
            GL.CreateTextures(TextureTarget.Texture2D, 1, out int TextureHandle);
            GL.TextureStorage2D(TextureHandle, 1, SizedInternalFormat.Srgb8Alpha8, TextureFile.Width, TextureFile.Height);
            GL.TextureSubImage2D(TextureHandle, 0, 0, 0, TextureFile.Width, TextureFile.Height, PixelFormat.Rgba, PixelType.UnsignedByte, TextureFile.Data);
            GL.TextureParameter(TextureHandle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TextureParameter(TextureHandle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.GenerateTextureMipmap(TextureHandle);

            TextureLookup.TryAdd("Default", TextureHandle);
        }
    }
}