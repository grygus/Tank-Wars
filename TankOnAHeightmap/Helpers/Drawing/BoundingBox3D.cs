using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TanksOnAHeightmap.Helpers.Drawing
{
    public class BoundingBox3D
    {
        Line3D line;
        Vector3[] vertices = new Vector3[8];

        public BoundingBox3D(GraphicsDevice graphicsdevice)
        {
            line = new Line3D(graphicsdevice);
        }

        public void Draw(BoundingBox boundingbox, Color color,Matrix? World = null)
        {
            UpdateVerts(boundingbox);
            line.Draw(vertices[0], vertices[6], color, World);
            line.Draw(vertices[6], vertices[4], color, World);
            line.Draw(vertices[4], vertices[2], color, World);
            line.Draw(vertices[2], vertices[0], color, World);
            line.Draw(vertices[1], vertices[3], color, World);
            line.Draw(vertices[3], vertices[5], color, World);
            line.Draw(vertices[5], vertices[7], color, World);
            line.Draw(vertices[7], vertices[1], color, World);
            line.Draw(vertices[0], vertices[5], color, World);
            line.Draw(vertices[3], vertices[6], color, World);
            line.Draw(vertices[4], vertices[1], color, World);
            line.Draw(vertices[7], vertices[2], color, World);
        }

        public void UpdateVerts(BoundingBox bb)
        {
            vertices[0] = bb.Min;
            vertices[1] = bb.Max;
            vertices[2] = new Vector3(bb.Max.X, bb.Min.Y, bb.Min.Z);
            vertices[3] = new Vector3(bb.Min.X, bb.Max.Y, bb.Max.Z);
            vertices[4] = new Vector3(bb.Max.X, bb.Max.Y, bb.Min.Z);
            vertices[5] = new Vector3(bb.Min.X, bb.Min.Y, bb.Max.Z);
            vertices[6] = new Vector3(bb.Min.X, bb.Max.Y, bb.Min.Z);
            vertices[7] = new Vector3(bb.Max.X, bb.Min.Y, bb.Max.Z);
        }
    }
}