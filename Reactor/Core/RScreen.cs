﻿//
// RScreen.cs
//
// Author:
//       Gabriel Reiser <gabriel@reisergames.com>
//
// Copyright (c) 2015 2014
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
using System.Collections.Generic;
using System.Drawing.Text;
using Reactor.Types;
using Reactor.Geometry;
using Reactor.Math;
using OpenTK.Graphics.OpenGL;
using Reactor.Types.States;


namespace Reactor
{
    public class RScreen : RSingleton<RScreen>
    {
        static List<RFont> Fonts = new List<RFont>();
        static RFont defaultFont = new RFont();
        bool initialized=false;
        RMeshBuilder quad;
        RShader defaultShader;
        RCamera2d camera2d;
        RCamera oldCamera;
        RVertexBuffer vertexQuad2D;
        RIndexBuffer indexQuad2D;
        RVertexData2D[] quadVerts;
        RBlendState blendState;
        public RScreen()
        {
            camera2d = new RCamera2d();
        }

        public RCamera2d Camera
        {
            get { return camera2d; }
            set { camera2d = value; }
        }

        public void Init()
        {
            blendState = RBlendState.AlphaBlend;
            defaultShader = new RShader();
            defaultShader.Load(RShaderResources.Basic2dEffectVert, RShaderResources.Basic2dEffectFrag, null);
            Fonts.Add(defaultFont);
            quad = new RMeshBuilder();
            quad.CreateFullscreenQuad();
            quadVerts = new RVertexData2D[4];
            quadVerts[0] = new RVertexData2D(new Vector2(-1, -1), new Vector2(0, 0));
            quadVerts[1] = new RVertexData2D(new Vector2(1, -1), new Vector2(1, 0));
            quadVerts[2] = new RVertexData2D(new Vector2(1, 1), new Vector2(1, 1));
            quadVerts[3] = new RVertexData2D(new Vector2(-1, 1), new Vector2(0, 1));
            vertexQuad2D = new RVertexBuffer(quadVerts[0].Declaration, 4, RBufferUsage.WriteOnly);
            vertexQuad2D.SetData<RVertexData2D>(quadVerts);
            indexQuad2D = new RIndexBuffer(typeof(short), 6, RBufferUsage.WriteOnly);
            indexQuad2D.SetData<short>(new short[6]{0,1,2,0,2,3}, 0, 6);
            initialized = true;
        }

        public void Begin()
        {
            oldCamera = REngine.Instance.GetCamera();
            REngine.Instance.SetCamera(camera2d);
            GL.Disable(EnableCap.DepthTest);

            //GL.Viewport(0, (int)viewport.Width, 0, (int)viewport.Height);
            //blendState.ColorWriteChannels = RColorWriteChannels.All;
            GL.Enable(EnableCap.Blend);
            blendState.PlatformApplyState();
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            //GL.FrontFace(FrontFaceDirection.Ccw);
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            REngine.CheckGLError();
            GL.Disable(EnableCap.CullFace);

            camera2d.Update();

            
        }

        public void End()
        {
            
            REngine.Instance.SetCamera(oldCamera);
            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);
            GL.FrontFace(FrontFaceDirection.Ccw);
            GL.CullFace(CullFaceMode.Back);
            GL.DepthFunc(DepthFunction.Less);
            GL.Disable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.Zero);
            REngine.CheckGLError();
            GL.DepthMask(true);
            REngine.CheckGLError();

        }
        void InitCheck()
        {
            if(!initialized)
                throw new ReactorException("You must first call Init() before using RScreen.");
        }

        public RFont LoadFont(string path, int size)
        {
            InitCheck();
            RFont font = new RFont();
            font.Load(RFileSystem.Instance.GetFilePath(path), size);
            return font;

        }

        public RFont LoadTextureFont(string fontName, int size)
        {
            InitCheck();
            return null;
        }

        public void RenderFullscreenQuad()
        {
            InitCheck();
            quad.Render();
        }

        public void RenderFullscreenQuad(RShader shader)
        {
            InitCheck();
            quad.SetShader(shader);
            quad.Render();
        }

        public void RenderTexture(RTexture texture, Rectangle bounds)
        {
            RenderTexture(texture, bounds, RColor.White);
        }
        public void RenderTexture(RTexture texture, Rectangle bounds, RColor color)
        {
           RenderTexture(texture, bounds, color, Matrix.Identity, false);
        }
        public void RenderTexture(RTexture texture, Rectangle bounds, RColor color, Matrix matrix, bool font)
        {
            RViewport viewport = REngine.Instance._viewport;
            UpdateQuad(bounds);
            blendState.PlatformApplyState();
            if(font)
            {
                GL.Enable(EnableCap.Blend);
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
            }
            defaultShader.Bind();
            defaultShader.SetSamplerValue(RTextureLayer.DIFFUSE, texture);
            vertexQuad2D.Bind();
            vertexQuad2D.BindVertexArray();
            indexQuad2D.Bind();


            defaultShader.SetUniformValue("projection", camera2d.Projection);
            defaultShader.SetUniformValue("view", camera2d.View);
            defaultShader.SetUniformValue("diffuse_color", color.ToVector4());
            defaultShader.SetUniformValue("model", matrix);
            defaultShader.SetUniformValue("font", font);
            vertexQuad2D.VertexDeclaration.Apply(defaultShader, IntPtr.Zero);


            GL.DrawElements(PrimitiveType.Triangles, indexQuad2D.IndexCount, DrawElementsType.UnsignedShort, IntPtr.Zero);
            REngine.CheckGLError();

            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.DstAlpha);
            GL.Disable(EnableCap.Blend);
            indexQuad2D.Unbind();
            vertexQuad2D.UnbindVertexArray();
            vertexQuad2D.Unbind();
            defaultShader.Unbind();

        }

        public void RenderText(RFont font, Vector2 penPoint, string text)
        {
            RenderText(font, penPoint, text, RColor.White);
        }
        public void RenderText(RFont font, Vector2 penPoint, string text, RColor color)
        {
            text = text.Replace("\r\n", "\n");
            char lastChar = '\0';
            Vector2 originalPoint = penPoint;
            foreach(char c in text)
            {
                if(c == ' ')
                {
                    penPoint.X += font.Kerning(lastChar, c).X+font.SpaceWidth;
                    lastChar = ' ';
                    continue;
                }
                if(c == '\t')
                {
                    penPoint.X += (font.Kerning(lastChar, c).X+(font.SpaceWidth * 2));
                    continue;
                }
                if(c == '\r' || c=='\n')
                {
                    penPoint.Y += font.LineHeight + (font.font.Height>>6);
                    penPoint.X = originalPoint.X+font.Kerning(lastChar, c).X;
                    continue;
                }
                penPoint.X += font.Kerning(lastChar, c).X;
                RTextureGlyph glyph = font.GetGlyph(c);
                int x0 = (int)(penPoint.X + (glyph.bitmapLeft));
                int y0 = (int)(penPoint.Y - (glyph.bitmapTop));
                //penPoint.X += glyph.Offset.X;

                RenderTexture(glyph, new Rectangle(x0, y0, (int)glyph.Bounds.Width, (int)glyph.Bounds.Height), color, Matrix.Identity, true);
                penPoint.X += glyph.advance.X;
                lastChar = c;
            }
            //font.RenderText(defaultShader, text, penPoint.X, penPoint.Y, size, size);
        }

        internal void RenderFPS(int fps)
        {
            Begin();
            RenderText(defaultFont, new Vector2(5, 30), String.Format("{0}fps",fps));
            End();
        }
        void UpdateQuad(Rectangle placement)
        {
            quadVerts[0].Position = new Vector2(placement.X, placement.Y);
            quadVerts[0].TexCoord = new Vector2(0, 0);
            quadVerts[1].Position = new Vector2(placement.X + placement.Width, placement.Y);
            quadVerts[1].TexCoord = new Vector2(1, 0);
            quadVerts[2].Position = new Vector2(placement.X + placement.Width, placement.Y + placement.Height);
            quadVerts[2].TexCoord = new Vector2(1, 1);
            quadVerts[3].Position = new Vector2(placement.X, placement.Y + placement.Height);
            quadVerts[3].TexCoord = new Vector2(0, 1);
            vertexQuad2D.SetData<RVertexData2D>(quadVerts);
        }

        public RBlendState BlendState
        {
            get { return blendState; } set { blendState = value;}
        }
    }
}
