using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using TanksOnAHeightmap.GameBase.Effects.ParticleSystems;
using TanksOnAHeightmap.GameBase.Effects;

namespace TanksOnAHeightmap
{
    /// <summary>
    /// Handles radial impulse applications on nearby objects when activated.
    /// </summary>
    public class Explosion : DrawableGameComponent
    {
        /// <summary>
        /// Re-used list of entities hit by the explosion.
        /// </summary>
        private readonly List<Entity> affectedEntities = new List<Entity>();

        Entity entity;
        protected ParticleSystem explosionParticles;
        protected ParticleSystem explosionSmokeParticles;
        /// <summary>
        /// Constructs an explosion.
        /// </summary>
        /// <param name="pos">Initial position of the explosion.</param>
        /// <param name="explosionMagnitude">Base strength of the blast as applied in units of impulse.</param>
        /// <param name="maxDist">Maximum radius of effect.</param>
        /// <param name="containingSpace">Space in which the explosion resides.</param>
        public Explosion(Entity entity, float explosionMagnitude, float maxDist, Space containingSpace, Game game) :base(game)
        {
            this.entity = entity;
            Position = entity.WorldTransform.Translation;
            Magnitude = explosionMagnitude;
            MaxDistance = maxDist;
            Space = containingSpace;

            explosionParticles = Game.Services.GetService(typeof(ExplosionParticleSystem)) as ExplosionParticleSystem;
            explosionSmokeParticles = Game.Services.GetService(typeof(ExplosionSmokeParticleSystem)) as ExplosionSmokeParticleSystem;
            
        }

        /// <summary>
        /// Gets or sets the base strength of the blast.
        /// </summary>
        public float Magnitude { get; set; }

        /// <summary>
        /// Gets or sets the maximum distance that the explosion will affect.
        /// </summary>
        public float MaxDistance { get; set; }

        /// <summary>
        /// Gets or sets the current position of the explosion.
        /// </summary>
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets or sets the space that the explosion will explode in.
        /// </summary>
        public Space Space { get; set; }

        /// <summary>
        /// Detonates the explosion, applying impulses to applicable physically simulated entities.
        /// </summary>
        public void Explode()
        {
            affectedEntities.Clear();
            //Ask the broadphase system which entities are in the explosion region.
            Space.BroadPhase.GetEntities(new BoundingSphere(Position, MaxDistance), affectedEntities);

            for (int i = 0; i < 30; i++)
                //explosionParticles.AddParticle(tank.Position+new Vector3(0,40,0), new Vector3(0.0f,0.0f,0.0f));
                explosionParticles.AddParticle(Position + new Vector3(0, 15, 0), entity.WorldTransform.Forward * 30);
            for (int i = 0; i < 50; i++)
                explosionSmokeParticles.AddParticle(Position + new Vector3(0, 15, 0), entity.WorldTransform.Forward * 60);

            foreach (Entity e in affectedEntities)
            {
                //Don't bother applying impulses to kinematic entities; they have infinite inertia.
                if (e.IsDynamic)
                {
                    Vector3 offset = e.InternalCenterPosition - Position;
                    float distanceSquared = offset.LengthSquared();
                    if (distanceSquared > Toolbox.Epsilon) //Be kind to the engine and don't give it a value divided by zero.
                    {
                        //This applies a force inversely proportional to the distance.
                        //Note the distanceSquared in the denominator.  This normalizes the
                        //offset and then further divides by the distance, resulting in a linear explosion falloff.
                        //A quadratic falloff could be accomplished by including an extra distance term in the denominator.
                        e.ApplyLinearImpulse(offset * (Magnitude / (distanceSquared)));
                        //The above only applies a linear impulse, which is quick and usually sufficient to look like an explosion.
                        //If you want some extra chaotic spinning, try applying an angular impulse.
                        //Using e.ApplyImpulse with an appropriate impulse location or e.ApplyAngularImpulse will do the job.

                        //Since ApplyLinearImpulse doesn't wake up an entity by default, force the exploded entity awake.
                        e.IsActive = true;
                    }
                    else
                    {
                        e.ApplyLinearImpulse(new Vector3(0, Magnitude, 0));
                    }
                }
            }
        }
    }
}