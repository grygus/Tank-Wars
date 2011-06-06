/*
      Copyright (C) 2009 Bepu Entertainment LLC.

      This software source code is provided 'as-is', without 
      any express or implied warranty.  In no event will the authors be held 
      liable for any damages arising from the use of this software.

      Permission is granted to anyone to use this software for any purpose,
      including commercial applications, and to alter it and redistribute it
      freely, subject to the following restrictions:

      1. The origin of this software must not be misrepresented; you must not
         claim that you wrote the original software. If you use this software
         in a product, an acknowledgment in the product documentation would be
         appreciated but is not required.
      2. Altered source versions must be plainly marked as such, and must not be
         misrepresented as being the original software.
      3. This notice may not be removed or altered from any source distribution.

    Contact us at:
    contact@bepu-games.com
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Diagnostics;
using BEPUphysics.Entities;

namespace TanksOnAHeightmap
{
    /// <summary>
    /// Component that draws a model following the position and orientation of a BEPUphysics entity.
    /// </summary>
    public class EntityModel : DrawableGameComponent
    {
        /// <summary>
        /// Entity that this model follows.
        /// </summary>
        Entity entity;
        Model model;
        /// <summary>
        /// Base transformation to apply to the model.
        /// </summary>
        public Matrix Transform;
        Matrix[] boneTransforms;


        /// <summary>
        /// Creates a new EntityModel.
        /// </summary>
        /// <param name="entity">Entity to attach the graphical representation to.</param>
        /// <param name="model">Graphical representation to use for the entity.</param>
        /// <param name="transform">Base transformation to apply to the model before moving to the entity.</param>
        /// <param name="game">Game to which this component will belong.</param>
        public EntityModel(Entity entity, Model model, Matrix transform, Game game)
            : base(game)
        {
            this.entity = entity;
            this.model = model;
            this.Transform = transform;

            //Collect any bone transformations in the model itself.
            //The default cube model doesn't have any, but this allows the EntityModel to work with more complicated shapes.
            boneTransforms = new Matrix[model.Bones.Count];
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            //Notice that the entity's worldTransform property is being accessed here.
            //This property is returns a rigid transformation representing the orientation
            //and translation of the entity combined.
            //There are a variety of properties available in the entity, try looking around
            //in the list to familiarize yourself with it.
            Matrix worldMatrix = Transform * entity.WorldTransform;


            model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {

                    effect.EnableDefaultLighting();
                    effect.DiffuseColor = new Vector3(0.4f, 0.3f, 0.7f);

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
    }
}
