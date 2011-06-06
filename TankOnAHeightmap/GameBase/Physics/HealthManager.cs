
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
        public Health Health
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
        Health heal;
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
            rand = new Random(0);

            int x = (int)((float)rand.NextDouble() * 700f - 350f);
            int z = (int)((float)rand.NextDouble() * 700f - 350f);
            //heal = new Health(game, space, new Vector3(x, 0, z));
            //game.Components.Add(heal);

            //heal = new Health(game, space, new Vector3(-100, 0, -170));
            //game.Components.Add(heal);

        }

        /// <summary>
        /// Updates the First Aid World.
        /// </summary>
        /// <param name="gameTime">Object containing timing state of the game.</param>
        public override void Update(GameTime gameTime)
        {
            spawnTime += (float)gameTime.ElapsedGameTime.TotalSeconds;
            timeSinceLastSpawn += (float)gameTime.ElapsedGameTime.TotalSeconds; 

            int i = (int)spawnTime;
            if ((i % 20) == 0 && timeSinceLastSpawn>19)
            { // every 20 seconds drop healball on the map
                
                int x = (int)((float)rand.NextDouble() * 7000f - 3500f);
                int z = (int)((float)rand.NextDouble() * 7000f - 3500f);
                //heal = new Health(game, space, new Vector3(x, 40, z));
                //game.Components.Add(heal);
                timeSinceLastSpawn = 0;
            }
              //  Health heal = new Health(game, space, new Vector3(-100, 0, -170));

            //Console.WriteLine(i);
             base.Update(gameTime);
        }
    }
}