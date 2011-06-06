using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace TanksOnAHeightmap.Helpers.Drawing
{
    /// <summary>
    /// Wraps a VertexBuffer and IndexBuffer into a constructable object.
    /// </summary>
    /// <typeparam name="T">The type of vertex the object will use.</typeparam>
    public class GeometricObject<T> where T : struct, IVertexType
    {
        // are we in the middle of a Begin/End pair?
        private bool inConstruction = false;

        // lists for construction
        private List<T> vertexList = new List<T>();
        private List<ushort> indexList = new List<ushort>();

        // our graphics device
        private GraphicsDevice graphicsDevice;

        // buffers for drawing
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;

        // details about what we're drawing
        private PrimitiveType primitiveType;
        private int vertexCount;
        private int indexCount;
        private int primitiveCount;

        public GeometricObject(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
        }

        /// <summary>
        /// Begins the construction of the object.
        /// </summary>
        /// <param name="primitiveType">The type of primitives to be drawn.</param>
        public void Begin(PrimitiveType primitiveType)
        {
            if (inConstruction)
                throw new InvalidOperationException("Cannot Begin until End has been called.");

            // clear our lists
            vertexList.Clear();
            indexList.Clear();

            // dipose the old buffers
            if (vertexBuffer != null)
                vertexBuffer.Dispose();
            if (indexBuffer != null)
                indexBuffer.Dispose();

            // store the primitive type
            this.primitiveType = primitiveType;

            // flag us as in construction
            inConstruction = true;
        }

        /// <summary>
        /// Ends the construction of the object.
        /// </summary>
        public void End()
        {
            if (!inConstruction)
                throw new InvalidOperationException("Cannot End until Begin has been called.");
            if (vertexList.Count == 0)
                throw new InvalidOperationException("The primitive has no vertices.");
            if (indexList.Count == 0)
                throw new InvalidOperationException("The primitive has no indices.");

            // get our counts
            vertexCount = vertexList.Count;
            indexCount = indexList.Count;

            // get our primitive count
            switch (primitiveType)
            {
                case PrimitiveType.LineList:
                    primitiveCount = indexCount / 2;
                    break;
                case PrimitiveType.LineStrip:
                    primitiveCount = indexCount - 1;
                    break;
                case PrimitiveType.TriangleList:
                    primitiveCount = indexCount / 3;
                    break;
                case PrimitiveType.TriangleStrip:
                    primitiveCount = indexCount - 2;
                    break;
            }

            // create our buffers
            vertexBuffer = new VertexBuffer(graphicsDevice, typeof(T), vertexCount, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertexList.ToArray());

            indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indexCount, BufferUsage.WriteOnly);
            indexBuffer.SetData(indexList.ToArray());

            // we're out of construction mode
            inConstruction = false;
        }

        /// <summary>
        /// Adds a vertex to the object.
        /// </summary>
        /// <remarks>
        /// Vertices can only be added in between a Begin and End pair.
        /// </remarks>
        /// <param name="vertex">The vertex to add.</param>
        public void AddVertex(T vertex)
        {
            if (!inConstruction)
                throw new InvalidOperationException("Cannot AddVertex outside of a Begin/End pair.");
            vertexList.Add(vertex);
        }

        /// <summary>
        /// Adds vertices to the object.
        /// </summary>
        /// <remarks>
        /// Vertices can only be added in between a Begin and End pair.
        /// </remarks>
        /// <param name="vertices">The vertices to add.</param>
        public void AddVertices(params T[] vertices)
        {
            if (!inConstruction)
                throw new InvalidOperationException("Cannot AddVertices outside of a Begin/End pair.");
            vertexList.AddRange(vertices);
        }

        /// <summary>
        /// Adds an index to the object.
        /// </summary>
        /// <remarks>
        /// Indices can only be added in between a Begin and End pair.
        /// </remarks>
        /// <param name="index">The index to add.</param>
        public void AddIndex(ushort index)
        {
            if (!inConstruction)
                throw new InvalidOperationException("Cannot AddIndex outside of a Begin/End pair.");
            indexList.Add(index);
        }

        /// <summary>
        /// Adds indices to the object.
        /// </summary>
        /// <remarks>
        /// Indices can only be added in between a Begin and End pair.
        /// </remarks>
        /// <param name="indices">The indices to add.</param>
        public void AddIndices(params ushort[] indices)
        {
            if (!inConstruction)
                throw new InvalidOperationException("Cannot AddIndices outside of a Begin/End pair.");
            indexList.AddRange(indices);
        }

        /// <summary>
        /// Draws the object with the given effect.
        /// </summary>
        /// <remarks>
        /// Objects must be constructed using a Begin/End pair and the Add* methods
        /// before you can call Draw.
        /// 
        /// The object does not set any values on the effect so all matrices and other
        /// parameters should be set before calling Draw.
        /// </remarks>
        /// <param name="effect">The effect with which to draw the object.</param>
        public void Draw(Effect effect)
        {
            if (inConstruction)
                throw new InvalidOperationException("Cannot Draw in between a Begin/End pair.");
            if (vertexBuffer == null || indexBuffer == null)
                throw new InvalidOperationException("Cannot Draw until the primitive is constructed.");

            // set our buffers
            graphicsDevice.SetVertexBuffer(vertexBuffer);
            graphicsDevice.Indices = indexBuffer;

            // draw the primitives with the effect
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(primitiveType, 0, 0, vertexCount, 0, primitiveCount);
            }
        }
    }
}
