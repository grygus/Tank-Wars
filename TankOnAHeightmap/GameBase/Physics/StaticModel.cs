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
    /// Component that draws a model.
    /// </summary>
    public class StaticModel : DrawableGameComponent
    {
        Model model;
        /// <summary>
        /// Base transformation to apply to the model.
        /// </summary>
        public Matrix Transform;
        Matrix[] boneTransforms;


        /// <summary>
        /// Creates a new StaticModel.
        /// </summary>
        /// <param name="model">Graphical representation to use for the entity.</param>
        /// <param name="transform">Base transformation to apply to the model before moving to the entity.</param>
        /// <param name="game">Game to which this component will belong.</param>
        public StaticModel(Model model, Matrix transform, Game game)
            : base(game)
        {
            this.model = model;
            this.Transform = transform;

            //Collect any bone transformations in the model itself.
            //The default cube model doesn't have any, but this allows the StaticModel to work with more complicated shapes.
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
            model.CopyAbsoluteBoneTransformsTo(boneTransforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.World = boneTransforms[mesh.ParentBone.Index] * Transform;
                    effect.View = (Game as TanksOnAHeightmapGame).viewMatrix;
                    effect.Projection = (Game as TanksOnAHeightmapGame).projectionMatrix;
                }
                mesh.Draw();
            }
            base.Draw(gameTime);
        }
    }
}
