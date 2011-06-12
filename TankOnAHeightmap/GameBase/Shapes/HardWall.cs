using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.DataStructures;

namespace TanksOnAHeightmap
{
    /// <summary>
    /// Castle wall defending the cannon.
    /// </summary>
    public class HardWall : DrawableGameComponent
    {
        #region Properties

        public readonly List<Entity> blocks = new List<Entity>();

        Box toAddBox;

        public BoundingBox boundingBox;
        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
        }

        /// <summary>
        /// Gets the space that owns the entities composing this wall.
        /// </summary>
        public Space Space { get; private set; }

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
        public HardWall(Game game, Space space, Matrix detectorWorldMatrix, Matrix worldMatrix, int blocksAcross, int blocksTall, float wallLength, float wallHeight, float blockThickness)
            : base(game)
        {

            Space = space;

            float blockHeight = wallHeight / blocksTall;
            float blockWidth = wallLength / (blocksAcross + .5f);

            Vector3 BoduingMin;
            Vector3 BoduingMax;

            Matrix orientationMatrix = worldMatrix;
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
            float bX, bZ;
            if (blocks[blocks.Count - 1].WorldTransform.Translation.X > 0)
                bX = blockThickness;
            else
                bX = -blockThickness;

            if (blocks[blocks.Count - 1].WorldTransform.Translation.Z > 0)
                bZ = blockThickness;
            else
                bZ = -blockThickness;

            BoduingMin = blocks[0].WorldTransform.Translation;
            BoduingMax = blocks[blocks.Count - 1].WorldTransform.Translation + new Vector3(bX, 0, bZ);

            boundingBox = new BoundingBox(BoduingMin, BoduingMax);

            foreach (Box block in blocks)
            {
                block.Tag = this;
            }

        }

        static HardWall()
        {
            GraphicsTransform = Matrix.Identity;
        }

        /// <summary>
        /// Draws the wall.
        /// </summary>
        /// <param name="gameTime">Time since last frame.</param>
        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            foreach (Box block in blocks)
            {
                Matrix scaling = Matrix.CreateScale(0.025f);
                Matrix worldMatrix = scaling * GraphicsTransform * Matrix.CreateScale(block.Width, block.Height, block.Length) * block.WorldTransform;


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
            
        }

        /// <summary>
        /// Updates the game component.
        /// </summary>
        /// <param name="gameTime">Time since the last frame.</param>
        public override void Update(GameTime gameTime)
        {

        }


    }
}