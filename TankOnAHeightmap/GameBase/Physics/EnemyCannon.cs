
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TanksOnAHeightmap.GameLogic;
using Microsoft.Xna.Framework.Audio;


namespace TanksOnAHeightmap
{
    /// <summary>
    /// Shoots cannonballs at incoming evil boxes.
    /// </summary>
    public class EnemyCannon : DrawableGameComponent
    {
        #region Properties

        private Vector3 initialVelocity;
        public Vector3 Position = Vector3.Zero;
        public Vector3 rotation = Vector3.Zero;

        private float timeBetweenShots = 1f;
        public float TimeBetweenShots
        {
            get { return timeBetweenShots; }
            set { timeBetweenShots = value; }
        }
        private float timeSinceLastFire;

        Tank tank;
        Enemy enemy;
        Game game;

        public static SoundEffect fire;  
        #endregion



        public EnemyCannon(Game game, Tank Tank, Enemy enemy)
            : base(game)
            
            { 
                tank = Tank;
                this.enemy = enemy;
                this.game = game;
                
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
                enemy.EnemyCannonBallManager.AddCannonBall(enemy.tank.Position+Vector3.Transform(new Vector3(0,50,-50),tank.Orientation),
                                                           1000 *enemy.tank.ForwardVector);
                  
            }
        }

        public static Tank playerTank { get; set; }

    }
}