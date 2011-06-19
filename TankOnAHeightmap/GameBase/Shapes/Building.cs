using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.DataStructures;
using TanksOnAHeightmap.GameBase;

namespace TanksOnAHeightmap
{
    /// <summary>
    /// Castle wall defending the cannon.
    /// </summary>
    public class Building : GameObject
    {
        #region Properties

        public override Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }
        private Vector3 position;


        public override BoundingBox BoundingBox  
        {
            get { return entity.BoundingBox; }
        }

        public Space Space { get; private set; }

        public Entity entity;

        public Entity Entity
        {
            get { return entity; }
        }
        #endregion

        #region Rendering objects

        // All enemies have the same appearance.  To simplify things, these are static variables set from the BlocksGame class.

        /// <summary>
        /// Per-bone transformations of the model.
        /// </summary>
        private Matrix[] boneTransforms;

        public Model model;

        private float scale;
        private string _resources;

        /// <summary>
        /// The graphical model used to draw the enemy.
        /// </summary>
        public Model BlockModel
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

        

        public Building(Game game, Space space,  Vector3 position, Entity entity, float scale, string resources)
            : base(game)
        {
            Space = space;
            this.position = position;
            this.scale = scale;
            _resources = resources;

            this.entity = entity;
            entity.CenterPosition += position;
            entity.Tag = this;
            entity.EventManager.InitialCollisionDetected += EntityEntersVolume;
            space.Add(entity);

           /* Box box = entity as Box;
            Matrix scaling = Matrix.CreateScale(box.Width, box.Height, box.Length);
            Model CubeModel;
            CubeModel = game.Content.Load<Model>("Models/cube");
            EntityModel model1 = new EntityModel(entity, CubeModel, scaling, game);
            game.Components.Add(model1); */

        }

        protected override void LoadContent()
        {
            BlockModel = Game.Content.Load<Model>(_resources);
        }
        /// <summary>
        /// Draws the wall.
        /// </summary>
        /// <param name="gameTime">Time since last frame.</param>
        public override void Draw(GameTime gameTime)
        {

            Matrix scaling = Matrix.CreateScale(scale);
                Matrix worldMatrix = scaling * Matrix.CreateTranslation(position);
            
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
         
            base.Draw(gameTime);
        }

        /// <summary>
        /// Updates the game component.
        /// </summary>
        /// <param name="gameTime">Time since the last frame.</param>
        public override void Update(GameTime gameTime)
        {         
                 base.Update(gameTime);
        }


        /// <summary>
        /// Handles what happens when an entity enters the wall's hit volume.
        /// If it's an enemy, boom!!
        /// </summary>
        /// <param name="toucher">Entity touching the volume.</param>
        /// <param name="volume">Volume being touched.</param>
        public void EntityEntersVolume(Entity sender, Entity other, CollisionPair pair)
        {

        }
    }
}