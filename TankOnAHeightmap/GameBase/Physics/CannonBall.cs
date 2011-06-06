/*
      Copyright (C) 2010 Bepu Entertainment LLC.

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

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BEPUphysics;
using BEPUphysics.Entities;
using BEPUphysics.ResourceManagement;
using System;
using BEPUphysics.DataStructures;
using TanksOnAHeightmap.GameLogic;
using TanksOnAHeightmap.GameBase.Effects.ParticleSystems;
using Microsoft.Xna.Framework.Audio;

namespace TanksOnAHeightmap
{
    /// <summary>
    /// Projectile fired by the player's cannon.
    /// </summary>
    public class CannonBall
    {
        /// <summary>
        /// Constructs a new cannon ball.
        /// </summary>

        public static SoundEffect blast;  

        public CannonBall()
        {
            body = new Sphere(new Vector3(0, 0, 0), 30, 15);
            body.Tag = this;
            body.EventManager.InitialCollisionDetected += HandleImpact;

        }

        /// <summary>
        /// Prepares the instance for removal from the game.
        /// </summary>
        public void Destroy()
        {
            IsActive = false;
            Space.Remove(Body);
            Space = null;
        }

        /// <summary>
        /// Draws the cannon ball.
        /// </summary>
        public void Draw()
        {
            
            if (shouldDraw)
            {
                Matrix scaling = Matrix.CreateScale(body.Radius/3f); //
                Matrix worldMatrix = scaling * GraphicsTransform * body.WorldTransform;
               // CannonBall.Model.Draw(worldMatrix, (Manager.Game as TanksOnAHeightmapGame).viewMatrix, (Manager.Game as TanksOnAHeightmapGame).projectionMatrix);

                Model.CopyAbsoluteBoneTransformsTo(boneTransforms);
                foreach (ModelMesh mesh in Model.Meshes)
                {
                    foreach (BasicEffect effect in mesh.Effects)
                    {

                        effect.EnableDefaultLighting();
                        effect.PreferPerPixelLighting = true;
                        effect.AmbientLightColor = Color.White.ToVector3();

                        effect.World = boneTransforms[mesh.ParentBone.Index] * worldMatrix;
                        effect.View = (Manager.Game as TanksOnAHeightmapGame).viewMatrix;
                        effect.Projection = (Manager.Game as TanksOnAHeightmapGame).projectionMatrix;
                    }
                    mesh.Draw();
                }
            }
        }

        /// <summary>
        /// Handles the impact of the cannonball against some other object.
        /// </summary>
        /// <param name="sender">The cannonball.</param>
        /// <param name="other">Other entity involved in the collision.</param>
        /// <param name="pair">Collision pair between the two entities.</param>
        public void HandleImpact(Entity sender, Entity other, CollisionPair pair)
        {
            
            //Only handle the impact if it was the first impact.
            if (Space != null)
            {
                //Query the physics engine's broad phase collision detection system to determine which entities have 
                //bounding boxes that overlap the explosion volume.
                List<Entity> hitEntities = Resources.GetEntityList();
                var explosionVolume = new BoundingSphere(sender.CenterPosition, ExplosionRadius);
                 
                Space.BroadPhase.GetEntities(explosionVolume, hitEntities);
                foreach (Entity e in hitEntities)
                {

                    var enemy = e.Tag as TriangleMesh;
                    if (enemy != null)
                    {
                        // Terrain was hit!
                    } 
                    var enemy2 = e.Tag as Player;
                    if (enemy2 != null)
                    {
                        // Player tank was was hit!
                    }

                    var enemy3 = e.Tag as Enemy;
                    if (enemy3 != null)
                    {
                        //enemy3.Life -= 30;
                        //enemy3.IsHited = true;
                        //if (enemy3.Life < 1)
                        //{
                        //    enemy3.IsDead = true;
                        //    Space.Remove(enemy3.Tank_box);
                        //}
                        enemy3.ReceiveDamage(5);
                        enemy3.IsHited = true;
                        if (enemy3.IsDead)
                        {
                            Space.Remove(enemy3.Tank_box);
                        }
                       // Console.WriteLine("Hit" + enemy3.Life);
                    
                    } // enemy was hit                       

                    var enemy4 = e.Tag as Wall;
                    if (enemy4 != null && (e.IsDynamic == false))
                        e.BecomeDynamic(1f);
                  
                    var enemy5 = other.Tag as Building;
                    if (enemy5 != null)
                    {
                            Explosion explosion2 = new Explosion(sender, 500, ExplosionRadius, Space, game);
                            explosion2.Explode();
                    }

                    var enemy6 = other.Tag as EntityModel;
                    if (enemy6 != null)
                    {
                        Explosion explosion2 = new Explosion(sender, 500, ExplosionRadius, Space, game);
                        explosion2.Explode();
                    }
                }
                Explosion explosion = new Explosion(sender, 700, ExplosionRadius, Space, game);
                explosion.Explode();
                Manager.RemoveCannonBall(this);
                blast.Play();
            }
        }

        /// <summary>
        /// Initializes this cannon ball.
        /// </summary>
        /// <param name="manager">Manager of this cannonball.</param>
        /// <param name="space">Space that the physical form of this cannon ball resides in.</param>
        /// <param name="position">Point to put the cannon ball.</param>
        /// <param name="initialVelocity">Initial velocity of the cannon ball.</param>
        public void Initialize(CannonBallManager manager, Space space, Vector3 position, Vector3 initialVelocity)
        {
            
            Manager = manager;
            body.CollisionRules.Group = CannonBallCollisionGroup;
            Space = space;            
            Body.CenterPosition = position;
            Body.LinearVelocity = initialVelocity;
            Space.Add(Body);
          //  Body.AngularVelocity = new Vector3(2, -3, 1); //random-looking angular velocity.
            shouldDraw = false;
            numFramesSinceInitialized = 0;
            IsActive = true;
            
        }

        /// <summary>
        /// Updates the cannon ball.
        /// </summary>
        /// <param name="dt">Time progressed in the simulation.</param>
        public void Update(float dt)
        {
            //TODO: Do some fancy logic that updates with the cannonball!
            if (!shouldDraw)
            {
                numFramesSinceInitialized++;
                if (numFramesSinceInitialized > 1)
                    shouldDraw = true;
            }
        }

        /// <summary>
        /// Constructs and destroys cannon balls, maintaining a resource pool to avoid unnecessary allocations.
        /// </summary>
        public static class Factory
        {
            /// <summary>
            /// Stores projectiles for re-use.
            /// 
            /// Since projectiles will be constantly fired and destroyed, 'pooling' and re-using them prevents
            /// constant allocations.
            /// </summary>
            private static readonly ResourcePool<CannonBall> CannonBallPool = new UnsafeResourcePool<CannonBall>(100);

            /// <summary>
            /// Gets an initialized cannon ball from the resource pool.
            /// </summary>
            /// <param name="manager">Manager of the new cannonball.</param>
            /// <param name="space">Space to which the cannon ball belongs.</param>
            /// <param name="position">Position at which to spawn the cannon ball.</param>
            /// <param name="initialVelocity">Initial velocity of the cannon ball.</param>
            /// <returns>Newly initialized cannon ball.</returns>
            public static CannonBall Create(CannonBallManager manager, Space space, Vector3 position, Vector3 initialVelocity)
            {
                
                CannonBall toReturn = CannonBallPool.Take();
                toReturn.Initialize(manager, space, position, initialVelocity);
                return toReturn; 
            }

            /// <summary>
            /// Returns the enemy to the resource pool.
            /// </summary>
            public static void GiveBack(CannonBall cannonBall)
            {
                cannonBall.Destroy();
                CannonBallPool.GiveBack(cannonBall);
            }
        }

        #region Properties

        private readonly Sphere body;

        private float explosionRadius = 50;

        /// <summary>
        /// Gets and sets the collision group that cannon ball entities belong to.
        /// The collision group should have rules that ensure that a cannon ball can't hit another cannon ball.
        /// </summary>
        public static CollisionGroup CannonBallCollisionGroup { get; set; }

        public static Game game { get; set; }

        /// <summary>
        /// Gets the physical body of the enemy.
        /// </summary>
        public Sphere Body
        {
            get { return body; }
        }

        /// <summary>
        /// Gets and sets the explosion radius of the cannon ball on impact.
        /// </summary>
        public float ExplosionRadius
        {
            get { return explosionRadius; }
            set { explosionRadius = value; }
        }

        /// <summary>
        /// Gets the cannon ball manager that handles this instance.
        /// </summary>
        public CannonBallManager Manager { get; private set; }

        /// <summary>
        /// Gets the space that this enemy's physical form resides in.
        /// </summary>
        public Space Space { get; private set; }

        /// <summary>
        /// Gets whether or not this instance is currently active.  If false, 
        /// this instance belongs to the resource pool.
        /// </summary>
        public bool IsActive { get; private set; }

        #endregion

        #region Rendering objects

        // All enemies have the same appearance.  To simplify things, these are static variables set from the BlocksGame class.

        /// <summary>
        /// Per-bone transformations of the model.
        /// </summary>
        private static Matrix[] boneTransforms;

        private static Model model;

        private int numFramesSinceInitialized;

        /// <summary>
        /// When a cannonball is added to the space, the buffered state will not reflect the 
        /// current state until the next space update.  To avoid flickering ghost balls, the
        /// cannon ball will not be drawn until at least one space update has occurred.
        /// </summary>
        private bool shouldDraw;

        static CannonBall()
        {
            GraphicsTransform = Matrix.Identity;
        }

        /// <summary>
        /// Transformation to apply to the model prior to moving it into world space.
        /// </summary>
        public static Matrix GraphicsTransform { get; set; }

        /// <summary>
        /// The graphical model used to draw the enemy.
        /// </summary>
        public static Model Model
        {
            get { return model; }
            set
            {
                model = value;               
                boneTransforms = new Matrix[value.Bones.Count];
            }
        }

        #endregion
    }
}