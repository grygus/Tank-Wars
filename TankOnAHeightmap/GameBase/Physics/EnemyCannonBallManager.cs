
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace TanksOnAHeightmap
{
    public class EnemyCannonBallManager : DrawableGameComponent
    {
        private readonly List<EnemyCannonBall> cannonBalls = new List<EnemyCannonBall>();

        /// <summary>
        /// Stores cannon balls which have been destroyed mid-frame.  Removed in a subsequent update.
        /// </summary>
        private readonly Queue<EnemyCannonBall> cannonBallsToRemove = new Queue<EnemyCannonBall>();

        /// <summary>
        /// Constructs a new enemy manager.
        /// </summary>
        /// <param name="game">Game that the manager belongs to.</param>
        public EnemyCannonBallManager(Game game)
            : base(game)
        {

        }

        /// <summary>
        /// Draws the enemies of the world.
        /// </summary>
        /// <param name="gameTime">Object containing timing state of the game.</param>
        public override void Draw(GameTime gameTime)
        {
            foreach (EnemyCannonBall ball in cannonBalls)
            {
                ball.Draw();               
            }
            
            base.Draw(gameTime);
        }

        /// <summary>
        /// Updates the enemies of the world.
        /// </summary>
        /// <param name="gameTime">Object containing timing state of the game.</param>
        public override void Update(GameTime gameTime)
        {
            //Remove any dead cannon balls.
            while (cannonBallsToRemove.Count > 0)
            {
                EnemyCannonBall toRemove = cannonBallsToRemove.Dequeue();
                if (toRemove.IsActive)
                {
                    
                    cannonBalls.Remove(toRemove);
                    EnemyCannonBall.Factory.GiveBack(toRemove);
                }
            }
            var dt = (float) gameTime.ElapsedGameTime.TotalSeconds;
            foreach (EnemyCannonBall ball in cannonBalls)
            {
                ball.Update(dt);               
                
            }
            
        }

        /// <summary>
        /// Adds a new enemy to the game.
        /// </summary>
        /// <param name="position">Position at which to spawn the cannon ball.</param>
        /// <param name="initialVelocity">Initial velocity of the cannon ball.</param>
        public void AddCannonBall(Vector3 position, Vector3 initialVelocity)
        {

            cannonBalls.Add(EnemyCannonBall.Factory.Create(this, (Game as TanksOnAHeightmapGame).Physic.space, position, initialVelocity));
        }

        /// <summary>
        /// Enqueues a cannon ball to remove from the game.
        /// </summary>
        /// <param name="cannonBall">Cannon ball to remove from the world.</param>
        public void RemoveCannonBall(EnemyCannonBall cannonBall)
        {            
            cannonBallsToRemove.Enqueue(cannonBall);            
        }
    }
}