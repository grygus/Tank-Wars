using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEPUphysics;
using BEPUphysics.Entities;
using Microsoft.Xna.Framework;
using TanksOnAHeightmap.Helpers.Drawing;

namespace TanksOnAHeightmap.GameBase.Shapes
{
    public class Prey : Building
    {
        public int Life { get; set; }
        public bool IsDead { get; set; }
        public bool IsUnderAttack { get; set; }
        public bool IsHit { get; set; }
        public float DangerTime { get; set; }

        public Prey(Game game, Space space, Vector3 position, Entity entity, float scale, string resources) :
            base(game, space, position, entity, scale, resources)
        {
            Life = 1000;
        }

        public virtual void ReceiveDamage(int damageValue)
        {
            Life = Math.Max(0, Life - damageValue);
            if (Life <= 0)
                IsDead = true;

            IsHit = true;

        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            var elapsedTimeSeconds = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (IsHit)
            {
                DangerTime = 2;
                IsUnderAttack = true;
            }

            if (DangerTime > 0)
                DangerTime -= elapsedTimeSeconds;
            else
                IsUnderAttack = false;
            IsHit = false;
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            Vector3 center = Position;
            center.Y = BoundingBox.Min.Y;
            Renderer.BoundingSphere3D.DrawCircle(new BoundingSphere(center, 500), Color.GreenYellow);
            Renderer.BoundingSphere3D.DrawCircle(new BoundingSphere(center, 501), Color.GreenYellow);
            Renderer.BoundingSphere3D.DrawCircle(new BoundingSphere(center, 502), Color.GreenYellow);
            Renderer.BoundingSphere3D.DrawCircle(new BoundingSphere(center, 503), Color.GreenYellow);
            Renderer.BoundingSphere3D.DrawCircle(new BoundingSphere(center, 504), Color.GreenYellow);

            //Renderer.BoundingBox3D.Draw(BoundingBox,Color.Red);
        }
        
    }
}
