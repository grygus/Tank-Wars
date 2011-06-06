
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;

namespace TanksOnAHeightmap
{
    public class CannonBallManager : DrawableGameComponent
    {
        private readonly List<CannonBall> cannonBalls = new List<CannonBall>();

        /// <summary>
        /// Stores cannon balls which have been destroyed mid-frame.  Removed in a subsequent update.
        /// </summary>
        private readonly Queue<CannonBall> cannonBallsToRemove = new Queue<CannonBall>();

        /// <summary>
        /// Constructs a new enemy manager.
        /// </summary>
        /// <param name="game">Game that the manager belongs to.</param>
        public CannonBallManager(Game game)
            : base(game)
        {

        }

        /// <summary>
        /// Draws the enemies of the world.
        /// </summary>
        /// <param name="gameTime">Object containing timing state of the game.</param>
        public override void Draw(GameTime gameTime)
        {
            foreach (CannonBall ball in cannonBalls)
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
                CannonBall toRemove = cannonBallsToRemove.Dequeue();
                if (toRemove.IsActive)
                {
                    
                    cannonBalls.Remove(toRemove);
                    CannonBall.Factory.GiveBack(toRemove);
                }
            }
            var dt = (float) gameTime.ElapsedGameTime.TotalSeconds;
            foreach (CannonBall ball in cannonBalls)
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
           
            cannonBalls.Add(CannonBall.Factory.Create(this, (Game as TanksOnAHeightmapGame).Physic.space, position, initialVelocity));
        }

        /// <summary>
        /// Enqueues a cannon ball to remove from the game.
        /// </summary>
        /// <param name="cannonBall">Cannon ball to remove from the world.</param>
        public void RemoveCannonBall(CannonBall cannonBall)
        {            
            cannonBallsToRemove.Enqueue(cannonBall);            
        }
    }
}