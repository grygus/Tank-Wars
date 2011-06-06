using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.DataStructures;
using TanksOnAHeightmap.Helpers.Drawing;

namespace TanksOnAHeightmap
{
    /// <summary>
    /// Castle wall defending the cannon.
    /// </summary>
    public class Wall : DrawableGameComponent
    {
        #region Properties

        public readonly List<Entity> blocks = new List<Entity>();

        /// <summary>
        /// Detects when enemies approach the wall and detonates them.
        /// </summary>
       // private readonly DetectorVolume detector;

        private readonly Random random = new Random();

        Box toAddBox;
        private float health = 5000;
        public BoundingBox boundingBox;
        public BoundingBox BoundingBox  
        {
            get { return boundingBox; }
        }
        /// <summary>
        /// After the wall dies, this timer keeps track of how long it's been since a block was removed.
        /// Every time the time exceeds the DespawnInterval, a block is randomly removed.
        /// </summary>
        private float timeSinceLastBlockDespawn;

        /// <summary>
        /// Gets or sets the time between blocks being removed after the wall dies.
        /// </summary>
        public float DespawnInterval { get; set; }


        /// <summary>
        /// Gets the Explosion used by the wall.  Triggered on wall destruction.
        /// </summary>
        public Explosion Explosion { get; private set; }

        public float Health
        {
            get { return health; }
            set
            {
                if (health <= 0)
                {
                    health = 0;
                    Die();
                }
                else
                {
                    health = value;
                }
            }
        }

        /// <summary>
        /// Gets whether or not this wall is still alive.
        /// </summary>
        public bool IsAlive { get; private set; }

        /// <summary>
        /// Gets the space that owns the entities composing this wall.
        /// </summary>
        public Space Space { get; private set; }
        public Matrix orientationMatrix;
        public Matrix worldMatrix;
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
        public Wall(Game game, Space space,  Matrix detectorWorldMatrix, Matrix worldMatrix, int blocksAcross, int blocksTall, float wallLength, float wallHeight, float blockThickness)
            : base(game)
        {

            IsAlive = true;
            DespawnInterval = 0.2f;
            Space = space;
  
            float blockHeight = wallHeight / blocksTall;
            float blockWidth = wallLength / (blocksAcross + .5f);

            Vector3 BoduingMin;
            Vector3 BoduingMax;
            this.worldMatrix = worldMatrix;
            orientationMatrix = worldMatrix;
            orientationMatrix.Translation = Vector3.Zero;
            Vector3 position;
            float x, y = blockHeight * .5f;
            
            for (int i = 0; i < blocksTall; i++)
            {
                if (i % 2 == 0)
                {
                    x = -blockWidth * (blocksAcross * .5f);
                    
                    position = new Vector3(x, y, 0);
                    Vector3.Transform(ref position, ref worldMatrix, out position);
                    toAddBox = new Box(position, blockWidth / 2, blockHeight, blockThickness);
                    toAddBox.OrientationMatrix = orientationMatrix;
                    space.Add(toAddBox);
                    blocks.Add(toAddBox);

                    x += blockWidth * .75f;
                }
                else
                {
                    x = -blockWidth * (blocksAcross * .5f - .25f);
                    position = new Vector3(x + blockWidth * (blocksAcross - .25f), y, 0);
                    Vector3.Transform(ref position, ref worldMatrix, out position);
                    toAddBox = new Box(position, blockWidth / 2, blockHeight, blockThickness);
                    toAddBox.OrientationMatrix = orientationMatrix;
                    space.Add(toAddBox);
                    blocks.Add(toAddBox);
                    
                }

                
                
                for (int j = 0; j < blocksAcross; j++)
                {
                    position = new Vector3(x, y, 0);
                    Vector3.Transform(ref position, ref worldMatrix, out position);
                    toAddBox = new Box(position, blockWidth, blockHeight, blockThickness);
                    toAddBox.OrientationMatrix = orientationMatrix;
                    space.Add(toAddBox);
                    blocks.Add(toAddBox);
                    

                    x += blockWidth;
                }
                y += blockHeight;
            }

            // BoundingBox
            float bX,bZ;
            if (blocks[blocks.Count - 1].WorldTransform.Translation.X > 0)
                bX = blockThickness;
            else
                bX = -blockThickness;

            if (blocks[blocks.Count - 1].WorldTransform.Translation.Z > 0)
                bZ = blockThickness;
            else
                bZ = -blockThickness;

            BoduingMin = blocks[0].WorldTransform.Translation ;
            BoduingMax = blocks[blocks.Count-1].WorldTransform.Translation + new Vector3(bX,0,bZ);

            Vector3 min = Vector3.Zero;
            Vector3 max = Vector3.Zero;

            min.X = Math.Min(BoduingMin.X, BoduingMax.X);
            min.Y = Math.Min(BoduingMin.Y, BoduingMax.Y);
            min.Z = Math.Min(BoduingMin.Z, BoduingMax.Z);

            max.X = Math.Max(BoduingMin.X, BoduingMax.X);
            max.Y = Math.Max(BoduingMin.Y, BoduingMax.Y);
            max.Z = Math.Max(BoduingMin.Z, BoduingMax.Z);


            boundingBox = new BoundingBox(min, max);
            foreach (Box block in blocks)
            {
                block.EventManager.InitialCollisionDetected += EntityEntersVolume;
                block.Tag = this;
               
            }
        }
        public Vector3 Normal()
        {

            return Vector3.Transform(new Vector3(0,0,-1),orientationMatrix);
        }
        static Wall()
        {
            GraphicsTransform = Matrix.Identity;
        }

        /// <summary>
        /// Draws the wall.
        /// </summary>
        /// <param name="gameTime">Time since last frame.</param>
        public override void Draw(GameTime gameTime)
        {           
            foreach (Box block in blocks)
            {
                Matrix scaling = Matrix.CreateScale(0.025f);
                Matrix worldMatrix = scaling*GraphicsTransform * Matrix.CreateScale(block.Width, block.Height, block.Length) * block.WorldTransform;


                    BlockModel.CopyAbsoluteBoneTransformsTo(boneTransforms);
                    foreach (ModelMesh mesh in BlockModel.Meshes)
                    {
                        foreach (BasicEffect effect in mesh.Effects)
                        {
                            effect.EnableDefaultLighting();


                            effect.FogEnabled = true;
                            effect.FogColor = new Vector3(0.5f, 0.5f, 0.8f);
                            effect.FogStart = 1000;
                            effect.FogEnd = 3200;
                           
                           effect.SpecularColor = new Vector3(0.1f, 0, 0);
                            effect.World = boneTransforms[mesh.ParentBone.Index] * worldMatrix;
                            effect.View = (Game as TanksOnAHeightmapGame).viewMatrix;
                            effect.Projection = (Game as TanksOnAHeightmapGame).projectionMatrix;
                              

                        }
                        mesh.Draw();
                    }
              
            }

            //Renderer.BoundingBox3D.Draw(boundingBox, Color.Black, null);
           //Renderer.Line3D.Draw(boundingBox.Max, boundingBox.Max + Normal()*40, Color.Black, null);
            base.Draw(gameTime);
        }

        /// <summary>
        /// Updates the game component.
        /// </summary>
        /// <param name="gameTime">Time since the last frame.</param>
        public override void Update(GameTime gameTime)
        {

            if (!IsAlive)
            {

                timeSinceLastBlockDespawn += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (timeSinceLastBlockDespawn > DespawnInterval)
                {
                    timeSinceLastBlockDespawn -= DespawnInterval;
                    if (blocks.Count > 0)
                    {                        
                        int index = random.Next(blocks.Count);
                        if (blocks[index].IsDynamic)
                        {
                            Space.Remove(blocks[index]);
                            blocks[index].IsAlwaysActive = false;
                            blocks[index].IsActive = false;
                           // blocks[index].Teleport(new Vector3(-1000, -1000, 0));
                            blocks.RemoveAt(index);
                        }
                       // else
                         //   blocks[index].BecomeDynamic(1f);

                        
                    }
                }
            }
            if (blocks.Count == 0)
                 base.Update(gameTime);
        }

        /// <summary>
        /// Kills off the wall.
        /// </summary>
        private void Die()
        {
            IsAlive = false;
        }

        /// <summary>
        /// Handles what happens when an entity enters the wall's hit volume.
        /// If it's an enemy, boom!!
        /// </summary>
        /// <param name="toucher">Entity touching the volume.</param>
        /// <param name="volume">Volume being touched.</param>
        public void EntityEntersVolume(Entity sender, Entity other, CollisionPair pair)
        {
            IsAlive = false;
            //Health -= 5001; //Remove the graphics too.
           // Space.Remove(sender);
           // Console.WriteLine("Touch!");
        }
      /*  private void EntityEntersVolume(Entity toucher, DetectorVolume volume)
        {
            
            var enemy = toucher.Tag as CannonBall;
            /* var enemy = toucher.Tag as Enemy;
            if (enemy != null)
            {
                //Console.WriteLine("Wall " + "Hit" + "!");
                Health -= 5000;
                //enemy.Manager.RemoveEnemy(enemy);
            }
        }*/
    }
}