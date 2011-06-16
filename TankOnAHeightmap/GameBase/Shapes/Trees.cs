using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using LTreesLibrary.Trees;
using TanksOnAHeightmap.GameBase.Cameras;
using Microsoft.Xna.Framework.Graphics;
using TanksOnAHeightmap.Helpers.Drawing;

namespace TanksOnAHeightmap.GameBase.Shapes
{
    public class Trees : GameObject
    {

        public static float TREE_SCALE = 0.1f;
        public static bool DEBUG_MODE = false;

        SimpleTree tree;

        // Necessary services
        protected Terrain terrain;
        protected ChaseCamera camera;
        protected ContentManager content;
        protected GraphicsDeviceManager graphics;
        protected BoundingBox boundingBox;
        public BoundingSphere BoundingSphere
        {
            get
            {
                return tree.TrunkMesh.BoundingSphere; 
            }
        }
        public override BoundingBox BoundingBox
        {
            get
            {
                
                return boundingBox;
            }
        }
        public override Vector3 Position
        {
            get { return Transformation.Translate; }
            set 
            {
                Transformation.Translate = value;
                boundingBox = BoundingBox.CreateFromSphere(BoundingSphere);
                Matrix mat = Matrix.CreateScale(0.1f, 1, 0.1f);
                mat *= Transformation.Matrix;
                boundingBox.Min = Vector3.Transform(boundingBox.Min, mat);
                boundingBox.Max = Vector3.Transform(boundingBox.Max, mat);
            
            }
        }

        public Trees(Game game, ContentManager content, GraphicsDeviceManager graphics)
            : base(game)
        {

            this.content = content;
            camera = Game.Services.GetService(typeof(ChaseCamera)) as ChaseCamera;
            terrain = Game.Services.GetService(typeof(Terrain)) as Terrain;
            this.graphics = graphics;

            Transformation = new Transformation();
            Transformation.Scale = new Vector3(TREE_SCALE);

            

        }

        protected override void LoadContent()
        {
            
           TreeProfile profile = content.Load<TreeProfile>("Trees/Gardenwood");
           tree = profile.GenerateSimpleTree();
        }

        public override void Initialize()
        {
            LoadContent();
            
           
        }


        public override void Update(GameTime time)
        {
            //base.Update(time);
        }

        public override void Draw(GameTime time)
        {
        
            GraphicsDevice device = graphics.GraphicsDevice;
            //BoundingFrustum frustum = new BoundingFrustum(camera.View * camera.Projection);

            //Matrix mat = Matrix.CreateScale(0.1f, 1, 0.1f);
            //mat *= transformation.Matrix;
            //BoundingSphere sphere = new BoundingSphere(Transformation.Translate,20);
            //if (frustum.Intersects(sphere))
                    tree.DrawTrunk(Transformation.Matrix, camera.View, camera.Projection);



            if (DEBUG_MODE)
            { 
                Renderer.BoundingBox3D.Draw(BoundingBox, Color.Black, null);
            }
        }

    }
}
