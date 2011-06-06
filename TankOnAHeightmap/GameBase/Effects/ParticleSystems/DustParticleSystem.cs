#region File Description
//-----------------------------------------------------------------------------
// ProjectileTrailParticleSystem.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace TanksOnAHeightmap.GameBase.Effects.ParticleSystems
{
    /// <summary>
    /// Custom particle system for leaving smoke trails behind the rocket projectiles.
    /// </summary>

    class DustParticleSystem : ParticleSystem
    {
        public ParticleSettings Settings
        {
            set
            {
                InitializeSettings(value);
            }
        }

        public DustParticleSystem(Game game, ContentManager content)
            : base(game, content)
        { }


        protected override void InitializeSettings(ParticleSettings settings)
        {
            settings.TextureName = "fire";
            settings.MaxParticles = 500;

            settings.Duration = TimeSpan.FromSeconds(2);

            settings.MinHorizontalVelocity = 0;
            settings.MaxHorizontalVelocity = 5;

            settings.MinVerticalVelocity = -10;
            settings.MaxVerticalVelocity = 40;

            settings.Gravity = new Vector3(0, -20, 0);

            settings.EndVelocity = 0;

            //settings.MinColor = Color.LightGray;
           // settings.MaxColor = Color.White;

            settings.MinColor = Color.OrangeRed;
            settings.MaxColor = Color.Orange;

            settings.MinRotateSpeed = -2;
            settings.MaxRotateSpeed = 2;

            settings.MinStartSize = 7;
            settings.MaxStartSize = 7;

            settings.MinEndSize = 160;
            settings.MaxEndSize = 160;
        }
    }
}
