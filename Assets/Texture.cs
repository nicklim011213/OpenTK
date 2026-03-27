using StbImageSharp;
using OpenTK.Graphics.OpenGL4;

public static class Texture
{
    public static Dictionary<string, int> TextureLookup = new Dictionary<string, int>();

    public static void CreateTexture(string Filepath)
    {
        ImageResult TextureFile = ImageResult.FromStream(File.OpenRead(Filepath), ColorComponents.RedGreenBlueAlpha);
        int TextureHandle = -1;
        GL.CreateTextures(TextureTarget.Texture2D, 1, out TextureHandle);
        GL.TextureStorage2D(TextureHandle, 1, SizedInternalFormat.Srgb8Alpha8, TextureFile.Width, TextureFile.Height);
        GL.TextureSubImage2D(TextureHandle, 0, 0, 0, TextureFile.Width, TextureFile.Height, PixelFormat.Rgba, PixelType.UnsignedByte, TextureFile.Data);

        GL.TextureParameter(TextureHandle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TextureParameter(TextureHandle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.TextureParameter(TextureHandle, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
        GL.TextureParameter(TextureHandle, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
        GL.GenerateTextureMipmap(TextureHandle);
        TextureLookup.TryAdd(Filepath, TextureHandle);
    }

    public static void CreateBlankTexture()
    {
        int TextureHandle = -1;
        GL.CreateTextures(TextureTarget.Texture2D, 1, out TextureHandle);
        GL.TextureStorage2D(TextureHandle, 1, SizedInternalFormat.DepthComponent16, 1024, 1024);
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

    public static int GenerateFrameBufferTexture(uint FrameBufferHandle)
    {
        // Generate Color Texture 1920X1080 Texture for use in frame buffer
        int TextureHandle = 0;
        GL.CreateTextures(TextureTarget.Texture2D, 1, out TextureHandle);
        GL.TextureStorage2D(TextureHandle, 1, SizedInternalFormat.Srgb8Alpha8, 1920, 1080);

        // Set for Lerp while sampling texel 
        GL.TextureParameter(TextureHandle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TextureParameter(TextureHandle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        // Attach Texture to FrameBuffer
        GL.NamedFramebufferTexture((int)FrameBufferHandle, FramebufferAttachment.ColorAttachment0, TextureHandle, 0);

        // Generate Depth + Stencil Texture 1920x1080
        int DepthStencilTextureHandle = 0;
        GL.CreateTextures(TextureTarget.Texture2D, 1, out DepthStencilTextureHandle);
        GL.TextureStorage2D(DepthStencilTextureHandle, 1, SizedInternalFormat.Depth24Stencil8, 1920, 1080);

        // Attach Depth + Stencil Texture to frame buffer
        GL.NamedFramebufferTexture((int)FrameBufferHandle, FramebufferAttachment.DepthStencilAttachment, DepthStencilTextureHandle, 0);

        // Return the color buffer handle
        return TextureHandle;
    }

    public static void CreateSafeDefault()
    {
        ImageResult TextureFile = ImageResult.FromStream(File.OpenRead("Assets\\Models\\Default.png"), ColorComponents.RedGreenBlueAlpha);
        int TextureHandle = -1;
        GL.CreateTextures(TextureTarget.Texture2D, 1, out TextureHandle);
        GL.TextureStorage2D(TextureHandle, 1, SizedInternalFormat.Srgb8Alpha8, TextureFile.Width, TextureFile.Height);
        GL.TextureSubImage2D(TextureHandle, 0, 0, 0, TextureFile.Width, TextureFile.Height, PixelFormat.Rgba, PixelType.UnsignedByte, TextureFile.Data);

        GL.TextureParameter(TextureHandle, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TextureParameter(TextureHandle, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
        GL.GenerateTextureMipmap(TextureHandle);

        TextureLookup.TryAdd("Default", TextureHandle);
    }
}