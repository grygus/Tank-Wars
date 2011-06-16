
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;


namespace TanksOnAHeightmap
{
    /// <summary>
    /// Shoots cannonballs at incoming evil boxes.
    /// </summary>
    public class Cannon : DrawableGameComponent
    {
        #region Properties


        private float timeBetweenShots = .5f;
        public float TimeBetweenShots
        {
            get { return timeBetweenShots; }
            set { timeBetweenShots = value; }
        }
        private float timeSinceLastFire;

        Tank tank;

        public static SoundEffect fire;              
   
        #endregion



        public Cannon(Game game, Tank Tank)
            : base(game)
            
            { 
                tank = Tank;
                
           }
 

        /// <summary>
        /// Updates the cannon.
        /// </summary>
        /// <param name="gameTime">Snapshot of the game's timing.</param>
        public override void Update(GameTime gameTime)
        {

            timeSinceLastFire = Math.Min(timeSinceLastFire + (float)gameTime.ElapsedGameTime.TotalSeconds, TimeBetweenShots);
                           
        }


        /// <summary>
        /// Fires a cannon ball at the target location.
        /// </summary>
        public void Fire()
        {
            
            if (timeSinceLastFire >= TimeBetweenShots)// && initialVelocity.X != -9999)
            {                
                timeSinceLastFire -= TimeBetweenShots;                
                fire.Play();
               // MediaPlayer.Play(fire);
                //SoundEffect.
                //MediaPlayer.Volume = 0.5f;

                (Game as TanksOnAHeightmapGame).Physic.CannonBallManager.AddCannonBall(Vector3.Transform(tank.cannonBone.Transform.Translation+tank.turretBone.Transform.Translation,
                                                                                                        tank.WorldMatrix),
                                                                                        1000 * Vector3.Transform(tank.turretBone.Transform.Forward + new Vector3(0,tank.cannonBone.Transform.Forward.Y,0),
                                                                                                        tank.Orientation));
                
            }
        }

    }
}