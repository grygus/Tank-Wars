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

namespace TanksOnAHeightmap.Helpers.Drawing
{
    public class Line3D
    {
        GraphicsDevice graphicsDevice;

        Matrix mxWorldMatrix;
        Matrix mxViewMatrix;
        Matrix mxProjectionMatrix;

        BasicEffect fxBasicEffect;
        VertexPositionColor[] PointList;
        VertexBuffer vertexBuffer;

        int points = 2;
        short[] lineListIndices;

        public Line3D(GraphicsDevice graphicsdevice)
        {
            graphicsDevice = graphicsdevice;
            Initialize();
        }

        public void Initialize()
        {
            InitializeEffect();
            SetPointPosition(new Vector3(0, 0, 0), new Vector3(1, 1, 1), Color.White);
            InitializeLineList();
        }

        private void InitializeEffect()
        {

            fxBasicEffect = new BasicEffect(graphicsDevice);
            fxBasicEffect.VertexColorEnabled = true;

            mxWorldMatrix = Matrix.CreateTranslation(0, 0, 0);
            fxBasicEffect.World = mxWorldMatrix;
            fxBasicEffect.View = Renderer.camera.View;
            fxBasicEffect.Projection = Renderer.camera.Projection;
        }


        private void SetPointPosition(Vector3 start, Vector3 end, Color color)
        {

            if (PointList == null)
            {
                PointList = new VertexPositionColor[points];



                PointList[0] = new VertexPositionColor(
                            new Vector3(start.X, start.Y, start.Z), color);

                PointList[1] = new VertexPositionColor(
                            new Vector3(end.X, end.Y, end.Z), color);
            }
            else
            {
                PointList[0].Position.X = start.X;
                PointList[0].Position.Y = start.Y;
                PointList[0].Position.Z = start.Z;
                PointList[0].Color = color;

                PointList[1].Position.X = end.X;
                PointList[1].Position.Y = end.Y;
                PointList[1].Position.Z = end.Z;
                PointList[1].Color = color;
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


        private void InitializeLineList()
        {
            // Initialize an array of indices of type short.
            lineListIndices = new short[2];

            // Populate the array with references to indices in the vertex buffer
            for (int i = 0; i < 1; i++)
            {
                lineListIndices[i * 2] = (short)(i);
                lineListIndices[(i * 2) + 1] = (short)(i + 1);
            }
        }


        public void Draw(Vector3 start, Vector3 end, Color color,Matrix? World = null)
        {
            SetPointPosition(start, end, color);
            
            // The effect is a compiled effect created and compiled elsewhere
            // in the application.
            if(World != null){
                fxBasicEffect.World = (Matrix)World;
            }
            else
            {
                fxBasicEffect.World = Matrix.CreateTranslation(0, 0, 0);
            }
            fxBasicEffect.View = Renderer.camera.View;
            fxBasicEffect.Projection = Renderer.camera.Projection;

            foreach (EffectPass pass in fxBasicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();

                DrawLine();
            }
        }


        private void DrawLine()
        {
            graphicsDevice.DrawUserIndexedPrimitives<VertexPositionColor>(
                PrimitiveType.LineList,
                PointList,
                0,  // vertex buffer offset to add to each element of the index buffer
                2,  // number of vertices in pointList
                lineListIndices,  // the index buffer
                0,  // first index element to read
                1   // number of primitives to draw
            );

        }





    }
}