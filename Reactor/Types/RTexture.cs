﻿// Author:
//       Gabriel Reiser <gabe@reisergames.com>
//
// Copyright (c) 2010-2016 Reiser Games, LLC.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Drawing;
using System.IO;
using Newtonsoft.Json;
using Reactor.Platform.OpenGL;
using Reactor.Utilities;
using Rectangle = Reactor.Math.Rectangle;

namespace Reactor.Types
{
    public class RTexture : IDisposable
    {
        internal static RTexture2D defaultWhite = new RTexture2D(true);

        [JsonIgnore] private bool bound;

        [JsonIgnore] public Rectangle Bounds;

        [JsonProperty("filename")] public string Filename;

        [JsonIgnore] public uint Id;

        [JsonProperty("name")] public string Name;

        protected RPixelFormat pixelFormat = RPixelFormat.Rgba;
        protected PixelType pixelType = PixelType.UnsignedByte;
        internal TextureTarget textureTarget;

        #region IDisposable implementation

        public void Dispose()
        {
            if (bound) Unbind();

            if (Id != 0)
            {
                Allocator.UInt32_1[0] = Id;
                GL.DeleteTextures(1, Allocator.UInt32_1);
            }
        }

        #endregion

        internal void LoadFromData(byte[] data, string name, bool isCompressed)
        {
            if (isCompressed)
                try
                {
                    ImageDDS.LoadFromData(data, name, out Id, out textureTarget, out pixelFormat, out pixelType);
                }
                catch (Exception e)
                {
                    RLog.Error("Error loading texture for: " + name);
                    RLog.Error(e.Message);
                    RLog.Error(e);
                }
            else
                try
                {
                    ImageGDI.LoadFromData(data, out Id, out textureTarget, out pixelFormat, out pixelType, out Bounds);
                }
                catch (Exception e)
                {
                    RLog.Error("Error loading texture for: " + name);
                    RLog.Error(e.Message);
                    RLog.Error(e);
                }

            if (Id == 0 || textureTarget == 0) RLog.Error("Error generating OpenGL texture for: " + name);

            CreateProperties(TextureTarget.Texture2D);
            RLog.Info("Texture loaded for: " + name);
            Name = name.TrimEnd(Path.GetExtension(name).ToCharArray());
            Filename = name;
            Unbind();
        }

        internal void LoadFromBitmap(Bitmap bitmap)
        {
            try
            {
                ImageGDI.LoadFromBitmap(ref bitmap, out Id, out textureTarget, out pixelFormat, out pixelType,
                    out Bounds);
            }
            catch (Exception e)
            {
                RLog.Error("Error loading texture from bitmap...");
                RLog.Error(e);
            }

            if (Id == 0 || textureTarget == 0) RLog.Error("Error generating OpenGL texture from bitmap");
            CreateProperties(TextureTarget.Texture2D);
            Bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
        }

        internal void LoadFromDisk(string filename)
        {
            if (Path.GetExtension(filename).ToLower() == ".dds")
                try
                {
                    ImageDDS.LoadFromDisk(RFileSystem.Instance.GetFilePath(filename), out Id, out textureTarget,
                        out pixelFormat, out pixelType);
                }
                catch (Exception e)
                {
                    RLog.Error("Error loading texture from: " + filename);
                    RLog.Error(e);
                }
            else
                try
                {
                    ImageGDI.LoadFromDisk(RFileSystem.Instance.GetFilePath(filename), out Id, out textureTarget,
                        out pixelFormat, out pixelType, out Bounds);
                }
                catch (Exception e)
                {
                    RLog.Error("Error loading texture from: " + filename);
                    RLog.Error(e);
                    return;
                }

            if (Id == 0 || textureTarget == 0)
            {
                RLog.Error("Error generating OpenGL texture from: " + filename);
                return;
            }

            CreateProperties(TextureTarget.Texture2D);
            RLog.Info("Texture loaded from: " + filename);
            Name = filename.TrimEnd(Path.GetExtension(filename).ToCharArray());
            Filename = filename;
            Unbind();
        }

        internal void Bind()
        {
            GL.BindTexture(textureTarget, Id);
            REngine.CheckGLError();
            bound = true;
        }

        internal void SetActive(RTextureLayer layer)
        {
            GL.ActiveTexture((int)layer);
            REngine.CheckGLError();
        }

        internal void Unbind()
        {
            GL.BindTexture(textureTarget, 0);
            bound = false;
        }

        public void SetTextureMagFilter(RTextureMagFilter value)
        {
            Bind();
            GL.TexParameteri(textureTarget, TextureParameterName.TextureMagFilter, (int)value);
            REngine.CheckGLError();
            Unbind();
        }

        public void SetTextureMinFilter(RTextureMinFilter value)
        {
            Bind();
            GL.TexParameteri(textureTarget, TextureParameterName.TextureMinFilter, (int)value);
            REngine.CheckGLError();
            Unbind();
        }

        public void SetTextureWrapMode(RTextureWrapMode modeS, RTextureWrapMode modeT)
        {
            Bind();
            GL.TexParameteri(textureTarget, TextureParameterName.TextureWrapS, (int)modeS);
            REngine.CheckGLError();
            GL.TexParameteri(textureTarget, TextureParameterName.TextureWrapT, (int)modeT);
            REngine.CheckGLError();
            Unbind();
        }

        public RTextureMagFilter GetTextureMagFilter()
        {
            if (Id != 0)
            {
                GL.GetTexParameteriv(textureTarget, GetTextureParameter.TextureMagFilter, Allocator.Int32_1);
                return (RTextureMagFilter)Allocator.Int32_1[0];
            }

            return 0;
        }

        public RTextureMinFilter GetTextureMinFilter()
        {
            if (Id != 0)
            {
                GL.GetTexParameteriv(textureTarget, GetTextureParameter.TextureMinFilter, Allocator.Int32_1);
                return (RTextureMinFilter)Allocator.Int32_1[0];
            }

            return 0;
        }

        public RTextureWrapMode GetTextureWrapModeS()
        {
            if (Id != 0)
            {
                GL.GetTexParameteriv(textureTarget, GetTextureParameter.TextureWrapS, Allocator.Int32_1);
                return (RTextureWrapMode)Allocator.Int32_1[0];
            }

            return 0;
        }

        public RTextureWrapMode GetTextureWrapModeT()
        {
            if (Id != 0)
            {
                GL.GetTexParameteriv(textureTarget, GetTextureParameter.TextureWrapT, Allocator.Int32_1);
                return (RTextureWrapMode)Allocator.Int32_1[0];
            }

            return 0;
        }

        public RPixelFormat GetPixelFormat()
        {
            if (Id != 0)
                return pixelFormat;
            return 0;
        }

        public void SetPixelFormat(RPixelFormat format)
        {
            if (Id != 0) pixelFormat = format;
        }

        protected bool isPowerOfTwo(uint x)
        {
            while (x % 2 == 0 && x > 1) /* While x is even and > 1 */
                x /= 2;
            return x == 1;
        }

        public T[] GetData<T>() where T : struct
        {
            GL.ActiveTexture((int)TextureUnit.Texture0);
            REngine.CheckGLError();
            Bind();
            REngine.CheckGLError();
            var pixels = new T[Bounds.Width * Bounds.Height];
            unsafe
            {
                fixed (T* ptr = &pixels[0])
                {
                    var p = new IntPtr(ptr);
                    GL.GetTexImage(textureTarget, 0, (PixelFormat)pixelFormat, pixelType, p);
                }
            }

            REngine.CheckGLError();
            Unbind();
            REngine.CheckGLError();

            return pixels;
        }

        public void SetData<T>(T[] data, RPixelFormat format, int x, int y, int width, int height,
            bool packAlignment = true) where T : struct
        {
            if (!packAlignment)
                GL.PixelStorei(PixelStoreParameter.UnpackAlignment, 1);
            Bind();
            unsafe
            {
                fixed (T* ptr = &data[0])
                {
                    var p = new IntPtr(ptr);
                    GL.TexSubImage2D(textureTarget, 0, x, y, width, height, (PixelFormat)format, PixelType.UnsignedByte,
                        p);
                }
            }

            REngine.CheckGLError();
            Unbind();
            if (!packAlignment)
                GL.PixelStorei(PixelStoreParameter.PackAlignment, 1);
        }

        public void SetData(RColor[] colors, RPixelFormat format, int x, int y, int width, int height)
        {
            Bind();
            unsafe
            {
                fixed (RColor* ptr = &colors[0])
                {
                    var p = new IntPtr(ptr);
                    GL.TexSubImage2D(textureTarget, 0, x, y, width, height, (PixelFormat)format, PixelType.UnsignedByte,
                        p);
                }
            }

            REngine.CheckGLError();
            Unbind();
            REngine.CheckGLError();
        }

        public void GenerateMipmaps()
        {
            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
        }

        protected void CreateProperties(TextureTarget target, bool mipmapped = false)
        {
            textureTarget = target;
            Bind();
            GL.TexParameteri(textureTarget, TextureParameterName.TextureMinFilter, (int)TextureParameter.Nearest);
            REngine.CheckGLError();
            GL.TexParameteri(textureTarget, TextureParameterName.TextureMagFilter, (int)TextureParameter.Nearest);
            REngine.CheckGLError();
            GL.TexParameteri(textureTarget, TextureParameterName.TextureWrapS, (int)TextureParameter.Repeat);
            REngine.CheckGLError();
            GL.TexParameteri(textureTarget, TextureParameterName.TextureWrapR, (int)TextureParameter.Repeat);
            REngine.CheckGLError();
            GL.TexParameteri(textureTarget, TextureParameterName.TextureWrapT, (int)TextureParameter.ClampToEdge);
            REngine.CheckGLError();
            //SetTextureMagFilter(RTextureMagFilter.Linear);
            //SetTextureMinFilter(RTextureMinFilter.Linear);
            //SetTextureWrapMode(RTextureWrapMode.Repeat, RTextureWrapMode.Repeat);
            Unbind();
            REngine.CheckGLError();
        }
    }

    public enum RTextureMagFilter
    {
        Nearest = TextureParameter.Nearest,
        Linear = TextureParameter.Linear
    }

    public enum RTextureMinFilter
    {
        Nearest = TextureParameter.Nearest,

        Linear = TextureParameter.Linear
        //NearestMipmapNearest = TextureParameter.NearestMipMapNearest,
        //LinearMipmapNearest = TextureParameter.LinearMipMapNearest,
        //NearestMipmapLinear = TextureParameter.NearestMipMapLinear,
        //LinearMipmapLinear = TextureParameter.LinearMipMapLinear,
    }

    public enum RTextureWrapMode
    {
        Clamp = TextureParameter.ClampToEdge,
        ClampToBorder = TextureParameter.ClampToBorder,
        Repeat = TextureParameter.Repeat,
        Mirror = TextureParameter.MirroredRepeat
    }
}