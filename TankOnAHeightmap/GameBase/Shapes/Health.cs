using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.DataStructures;
using TanksOnAHeightmap.GameLogic;

namespace TanksOnAHeightmap
{
    /// <summary>
    /// Castle wall defending the cannon.
    /// </summary>
    public class Health : DrawableGameComponent
    {
        #region Properties

        public bool IsAlive
        {
            get { return isAlive; }
            set { isAlive = value; }
        }
        public bool isAlive;

        public Vector3 Position
        {
            get { return body.CenterPosition; }
            //set { position = value; }
        }
        private Vector3 position;


        public BoundingBox BoundingBox  
        {
            get { return body.BoundingBox; }
        }
        float lifeTime;

        private Game game;

        public Space space { get; private set; }

        public Sphere body;

        public Sphere Body
        {
            get { return body; }
        }
        public Matrix Transformation
        {
            get { return body.WorldTransform; }
        }
        Matrix transformation;
        #endregion

        #region Rendering objects

        // All enemies have the same appearance.  To simplify things, these are static variables set from the BlocksGame class.

        /// <summary>
        /// Per-bone transformations of the model.
        /// </summary>
        private static Matrix[] boneTransforms;

        private static Model model;

        
        /// <summary>
        /// The graphical model used to draw the enemy.
        /// </summary>
        public static Model BlockModel
        {
            get { return model; }
            set
            {
                model = value;
                boneTransforms = new Matrix[value.Bones.Count];
                foreach (ModelMesh mesh in model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {
                        effect.LightingEnabled = true;
                        effect.EnableDefaultLighting();                        
                        effect.DirectionalLight0.Enabled = true;
                    }
                }
            }
        }

        /// <summary>
        /// Transformation to apply to the block models prior to moving it into world space.
        /// </summary>
        public static Matrix GraphicsTransform { get; set; }

        #endregion

        /// <summary>
        /// Constructs a new wall.
        /// </summary>
        /// <param name="game">Game that this component belongs to.</param>
        /// <param name="space">Space to which the entities of the wall belong to.</param>
        /// <param name="detectorModel">Volume to detect incoming enemies.</param>
        /// <param name="detectorWorldMatrix">Transformation matrix to apply to the detector volume.</param>
        /// <param name="worldMatrix">Transformation matrix which positions and orients the entities of the wall.</param>
        /// <param name="blocksAcross">Number of blocks across the wall.</param>
        /// <param name="blocksTall">Number of blocks tall.</param>
        /// <param name="wallLength">Total length of the wall.</param>
        /// <param name="wallHeight">Total height of the wall.</param>
        /// <param name="blockThickness">Thickness of the wall.</param>
        public Health(Game game, Space space, Vector3 position)
            : base(game)
        {
            this.space = space;
            this.game = game;
            body = new Sphere(position, 15, 5);
            body.Tag = this;
            body.EventManager.InitialCollisionDetected += EntityEntersVolume;
            space.Add(body);
            isAlive = true;
            lifeTime = 0f;

            /*Sphere box = body as Sphere;
            Matrix scaling = Matrix.CreateScale(body.Radius);
            Model CubeModel;
            CubeModel = game.Content.Load<Model>("Models/cube");
            EntityModel model1 = new EntityModel(body, CubeModel, scaling, game);
            game.Components.Add(model1); */

        }

        /// <summary>
        /// Draws the wall.
        /// </summary>
        /// <param name="gameTime">Time since last frame.</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Matrix scaling = Matrix.CreateScale(1.5f);
            Matrix worldMatrix = scaling * GraphicsTransform * body.WorldTransform;
            
                    BlockModel.CopyAbsoluteBoneTransformsTo(boneTransforms);
                    foreach (ModelMesh mesh in BlockModel.Meshes)
                    {
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            effect.EnableDefaultLighting();
                            effect.DiffuseColor = new Vector3(0.6f, 0.6f, 0.8f);

                            effect.FogEnabled = true;
                            effect.FogColor = new Vector3(0.5f, 0.5f, 0.8f);
                            effect.FogStart = 1500;
                            effect.FogEnd = 4200;
                            
                            effect.World = boneTransforms[mesh.ParentBone.Index] * worldMatrix;
                            effect.View = (Game as TanksOnAHeightmapGame).viewMatrix;
                            effect.Projection = (Game as TanksOnAHeightmapGame).projectionMatrix;
                        }
                        mesh.Draw();
                    }              
         
           
        }

        /// <summary>
        /// Updates the game component.
        /// </summary>
        /// <param name="gameTime">Time since the last frame.</param>
        public override void Update(GameTime gameTime)
        {
                 lifeTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
                 if (lifeTime > 120)
                 {
                     isAlive = false;
                     space.Remove(body);
                     
                 }
                 base.Update(gameTime);
        }
        static Health()
        {
            GraphicsTransform = Matrix.Identity;
        }

        /// <summary>
        /// Handles what happens when an entity enters the wall's hit volume.
        /// If it's an enemy, boom!!
        /// </summary>
        /// <param name="toucher">Entity touching the volume.</param>
        /// <param name="volume">Volume being touched.</param>
        public void EntityEntersVolume(Entity sender, Entity other, CollisionPair pair)
        {
            var enemy = other.Tag as TriangleMesh;
            if (enemy != null)
            {
                // Terrain was hit!
            }

            var enemy2 = other.Tag as Player;
            if (enemy2 != null)
            {
                if (enemy2.Life < 100)  // Player tank caught heath package!
                {
                    space.Remove(body);
                    game.Components.Remove(this);
                    Random rand = new Random(0);
                     IsAlive = false;
                     enemy2.Life = Math.Min(100, enemy2.Life + (int)((float)rand.NextDouble() * 30f + 20f) );
                 }                
            }
            var enemy3 = other.Tag as Enemy;
            if (enemy3 != null) // Enemy caught heath package!    
            {
                if (enemy3.Life < 150)
                {
                    space.Remove(body);  
                    game.Components.Remove(this);
                    Random rand = new Random(0);
                    IsAlive = false;
                    enemy3.Life = Math.Min(150, enemy3.Life + Math.Min(150, enemy3.Life + (int)((float)rand.NextDouble() * 30f + 30f)));
                }
            }                    

            var enemy4 = other.Tag as Wall;
            if (enemy4 != null && (other.IsDynamic == false))
                other.BecomeDynamic(1f);

            var enemy5 = other.Tag as Building;
            if (enemy5 != null)
            {

            } // Church was hit, not nice   
        }
    }
}