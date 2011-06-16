
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using BEPUphysics;

namespace TanksOnAHeightmap
{
    public class HealthManager : DrawableGameComponent
    {

        Game game;
        Space space;
        float spawnTime;
        public List<Health> Health
        {
            get
            {
                return heal;
            }
            set
            {
                heal = value;
            }
        }
        List<Health> heal;
        float timeSinceLastSpawn;
        Random rand;
        /// <summary>
        /// Constructs a new Health manager.
        /// </summary>
        /// <param name="game">Game that the manager belongs to.</param>
        public HealthManager(Game game, Space space) : base(game)
        {
            this.game = game;
            this.space = space;
            spawnTime = 1f;
            timeSinceLastSpawn = 0;
            rand = new Random();
            Health = new List<Health>();

        }

        /// <summary>
        /// Updates the First Aid World.
        /// </summary>
        /// <param name="gameTime">Object containing timing state of the game.</param>
        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < Health.Count; i++)
            {
                if (!Health[i].IsAlive)
                {
                    game.Components.Remove(Health[i]);
                    Health.RemoveAt(i);
                }

                
            }


            spawnTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            timeSinceLastSpawn += (float)gameTime.ElapsedGameTime.TotalSeconds; 

            if ((((int)spawnTime) % 20) == 0 && timeSinceLastSpawn > 19)
            { // every 20 seconds drop healball on the map
                AddRandomHealth();
                timeSinceLastSpawn = 0;
            }
            base.Update(gameTime);
        }

        public Vector3 GetNearestHealthPosition(Vector3 position)
        {
            float distance = float.MaxValue;
            float tmpDistance;
            int selectedIndex = -1;
            for (int i = 0; i < Health.Count; i++)
            {
                tmpDistance = Vector3.Distance(position, Health[i].Position);
                if (tmpDistance < distance)
                {
                    distance = tmpDistance;
                    selectedIndex = i;
                }
            }

            return (selectedIndex < 0) ? Vector3.Zero : Health[selectedIndex].Position;
        }

        public void AddRandomHealth()
        {
            int x = (int)((float)rand.NextDouble() * 7000f - 3500f);
            int z = (int)((float)rand.NextDouble() * 7000f - 3500f);
            
            Health tmpHealth = new Health(game, space, new Vector3(x, 80, z));
            game.Components.Add(tmpHealth);
            Health.Add(tmpHealth);
            tmpHealth.DrawOrder = 102;
        }
    }
}