// =======================================================================
// Class Explanation: Renderer
// ---------------------------
// Used to render primitive shapes for debugging
// =======================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using TanksOnAHeightmap.GameBase.Cameras;

namespace TanksOnAHeightmap.Helpers.Drawing
{
    public static class Renderer
    {
        // Important Game members
        private static GraphicsDevice graphicsDevice;
        public static ChaseCamera camera;
        private static Game game;

        public static GraphicsDevice GraphicsDevice
        {
            get { return graphicsDevice; }
            set { graphicsDevice = value; }
        }


        public static void Initialize(Game p_game,GraphicsDevice graphicsdevice)
        {
            graphicsDevice = graphicsdevice;
            game = p_game;
            camera = game.Services.GetService(typeof(ChaseCamera)) as ChaseCamera;
            InitializeGraphics();
        }

        private static void InitializeGraphics()
        {
            m_3DLine = new Line3D(graphicsDevice);
            m_2DBar = new Bar2D(graphicsDevice, game);
            //m_3DPoint = new Point3D(graphicsDevice);
            m_BoundingSphere = new BoundingSphere3D(graphicsDevice);
            m_BoundingBox = new BoundingBox3D(graphicsDevice);
        }

        // Draw graphic shapes
        private static Line3D m_3DLine;
        private static Bar2D m_2DBar;
        //private static Point3D m_3DPoint;
        private static BoundingSphere3D m_BoundingSphere;
        private static BoundingBox3D m_BoundingBox;

        public static BoundingSphere3D BoundingSphere3D
        {
            get { return m_BoundingSphere; }
        }

        public static Bar2D Bar2D
        {
            get { return m_2DBar; }
        }

        public static BoundingBox3D BoundingBox3D
        {
            get { return m_BoundingBox; }
        }

        public static Line3D Line3D
        {
            get { return m_3DLine; }
        }

        //public static Point3D Point3D
        //{
         //   get { return m_3DPoint; }
        //}
    }

}