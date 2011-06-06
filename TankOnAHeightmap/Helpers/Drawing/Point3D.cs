// =======================================================================
// Class Explanation: Line3D
// -------------------------
// This class draws a 3D line.
// =======================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TanksOnAHeightmap.GameBase.Cameras;

namespace TanksOnAHeightmap.Helpers.Drawing
{
    public class Point3D
    {
        GraphicsDevice graphicsDevice;

        Matrix mxWorldMatrix;
        Matrix mxViewMatrix;
        Matrix mxProjectionMatrix;

        BasicEffect fxBasicEffect;

        VertexPositionColor[] PointList;
        VertexBuffer vertexBuffer;


        public Point3D(GraphicsDevice graphicsdevice)
        {
            graphicsDevice = graphicsdevice;
            InitializeEffect();
            
        }

        private void InitializeEffect()
        {
            fxBasicEffect = new BasicEffect(graphicsDevice, null);
            fxBasicEffect.VertexColorEnabled = true;
            
            mxWorldMatrix = Matrix.CreateTranslation(0, 0, 0);
            fxBasicEffect.World = mxWorldMatrix;
            fxBasicEffect.View = Renderer.camera.View;
            fxBasicEffect.Projection = Renderer.camera.Projection;
        }

        // Set the vertex buffer to 1 vertex
        private void SetPointPosition(Vector3 vertex, Color color)
        {

            PointList = null;
            vertexBuffer = null;

            if (PointList == null)
            {
                PointList = new VertexPositionColor[1];

                PointList[0] = new VertexPositionColor(vertex, color);
            }

            if (vertexBuffer == null)
            {
                // Initialize the vertex buffer, allocating memory for each vertex.
                vertexBuffer = new VertexBuffer(graphicsDevice,
                    typeof(VertexPositionColor),2,
                    BufferUsage.None);
                
                // Set the vertex buffer data to the array of vertices.
                vertexBuffer.SetData<VertexPositionColor>(PointList);
            }
        }

        // Set the vertex buffer to an array of vertices
        private void SetPointPosition(Vector3[] vertices, Color color)
        {

            PointList = null;
            vertexBuffer = null;

            if (PointList == null)
            {
                PointList = new VertexPositionColor[vertices.Length];

                for (int i = 0; i < vertices.Length; i++)
                {
                    PointList[i] = new VertexPositionColor(vertices[i], color);
                }
            }

            if (vertexBuffer == null)
            {
                // Initialize the vertex buffer, allocating memory for each vertex.
                vertexBuffer = new VertexBuffer(graphicsDevice,
                    typeof(VertexPositionColor),vertices.Length,
                    BufferUsage.None);

                // Set the vertex buffer data to the array of vertices.
                vertexBuffer.SetData<VertexPositionColor>(PointList);
            }
        }

        // Draw 1 point
        public void Draw(Vector3 vertex, int pointsize, Color color)
        {
            SetPointPosition(vertex, color);

            // The effect is a compiled effect created and compiled elsewhere
            // in the application.
            fxBasicEffect.Begin();

            fxBasicEffect.View = Renderer.camera.View;
            fxBasicEffect.Projection = Renderer.camera.Projection;

            foreach (EffectPass pass in fxBasicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                graphicsDevice.RenderState.PointSize = pointsize;

                graphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                     PrimitiveType.PointList, PointList, 0, 1);
                
                graphicsDevice.RenderState.FillMode = FillMode.Solid;
                graphicsDevice.RenderState.PointSize = 1.0f;

                pass.End();
            }
            fxBasicEffect.End();
        }

        // Draw multiple points
        public void Draw(Vector3[] vertices, int pointsize, Color color)
        {
            SetPointPosition(vertices, color);

            // The effect is a compiled effect created and compiled elsewhere
            // in the application.
            fxBasicEffect.Begin();

            fxBasicEffect.View = Renderer.camera.View;
            fxBasicEffect.Projection = Renderer.camera.Projection;

            foreach (EffectPass pass in fxBasicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();

                graphicsDevice.RenderState.PointSize = pointsize;

                graphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                     PrimitiveType.PointList,PointList,0,  vertices.Length);

                graphicsDevice.RenderState.FillMode = FillMode.Solid;
                graphicsDevice.RenderState.PointSize = 1.0f;

                pass.End();
            }
            fxBasicEffect.End();
        }


        private void DrawPoints()
        {
            graphicsDevice.DrawUserPrimitives<VertexPositionColor>(
                PrimitiveType.PointList,
                PointList,
                0,  // index of the first vertex to draw
                PointList.Length   // number of primitives
            );
        }
    }
}

