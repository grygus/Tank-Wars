using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TanksOnAHeightmap.GameBase.Cameras;
using TanksOnAHeightmap.GameLogic;
using TanksOnAHeightmap.Helpers;
using TanksOnAHeightmap.Helpers.Drawing;
using Microsoft.Xna.Framework.Input.Touch;
using TanksOnAHeightmap.GameBase.Helpers;


namespace TanksOnAHeightmap
{
    class GameScreen : DrawableGameComponent
    {
        // Text
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;
        SpriteFont hudFont;

        Player player;

        // Frame counter helper
        FrameCounterHelper frameCounter;

        //Bars
        int currentlySelectedWeight;
        Rectangle barEnemy = new Rectangle(125, 145, 85, 40);
        Rectangle barPrey = new Rectangle(125, 225, 85, 40);
        Rectangle barHealth = new Rectangle(125, 305, 85, 40);
        

        Vector2 lastTouchPoint;
        bool isDragging;
        InputHelper inputHelper;

        protected ChaseCamera camera;

        public GameScreen(Game game, Player player)
            : base(game)
        {
            this.player = player;
            this.DrawOrder = 999;
        }

        public override void Initialize()
        {
            // Frame counter
            inputHelper  = Game.Services.GetService(typeof(InputHelper)) as InputHelper;
            camera       = Game.Services.GetService(typeof(ChaseCamera)) as ChaseCamera;
            frameCounter = new FrameCounterHelper(Game);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create SpriteBatch and add services
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Font 2D
            spriteFont = Game.Content.Load<SpriteFont>(GameAssetsPath.FONTS_PATH +
                "BerlinSans");
            hudFont = Game.Content.Load<SpriteFont>("hudFont");
            
            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update(GameTime gameTime)
        {
            // Restart game
            // gameLevel = LevelCreator.CreateLevel(Game, currentLevel);

            
            // Update player
            //gameLevel.Player.Update(gameTime);
            //UpdateWeaponTarget();

            // Update camera
            //BaseCamera activeCamera = gameLevel.CameraManager.ActiveCamera;
            //activeCamera.Update(gameTime);

            // Update light position
            //PointLight cameraLight = gameLevel.LightManager["CameraLight"] as PointLight;
            //cameraLight.Position = activeCamera.Position;

            // Update enemies
            /*foreach (Enemy enemy in gameLevel.EnemyList)
            {
                if (enemy.BoundingSphere.Intersects(activeCamera.Frustum) ||
                    enemy.State == Enemy.EnemyState.ChasePlayer ||
                    enemy.State == Enemy.EnemyState.AttackPlayer)

                    enemy.Update(gameTime);

            }*/

            // Update scene objects
            //gameLevel.SkyDome.Update(gameTime);
            //gameLevel.Terrain.Update(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1.0f, 255);
            
            //BaseCamera activeCamera = gameLevel.CameraManager.ActiveCamera;

            //gameLevel.SkyDome.Draw(gameTime);
            //gameLevel.Terrain.Draw(gameTime);
            //gameLevel.Player.Draw(gameTime);

            // Draw enemies
            /*foreach (Enemy enemy in gameLevel.EnemyList)
            {
                if (enemy.BoundingSphere.Intersects(activeCamera.Frustum))
                    enemy.Draw(gameTime);
            }*/

            spriteBatch.Begin( SpriteSortMode.Deferred,BlendState.AlphaBlend);
            if (player.IsDead || player.NumEnemiesAlive == 0)
            {
                spriteBatch.DrawString(spriteFont, "GAME OVER", new Vector2(320, 320), Color.White);

                spriteBatch.DrawString(spriteFont, "Press R to restart", new Vector2(320, 220), Color.White);
            }
            // Project weapon target
            //weaponTargetPosition = GraphicsDevice.Viewport.Project(weaponTargetPosition,
            //    activeCamera.Projection, activeCamera.View, Matrix.Identity);

            // Draw weapon target
            /*int weaponRectangleSize = GraphicsDevice.Viewport.Width / 40;
            if (activeCamera == gameLevel.CameraManager["FPSCamera"])
                spriteBatch.Draw(weaponTargetTexture, new Rectangle(
                    (int)(weaponTargetPosition.X - weaponRectangleSize * 0.5f),
                    (int)(weaponTargetPosition.Y - weaponRectangleSize * 0.5f),
                    weaponRectangleSize, weaponRectangleSize),
                    (aimEnemy == null) ? Color.White : Color.Red);*/

            // Draw GUI text
            spriteBatch.DrawString(spriteFont, "Health: " + player.Life + "/" +
                player.MaxLife, new Vector2(10, 5), Color.White);
            //spriteBatch.DrawString(spriteFont, "Bullets: " + gameLevel.Player.Weapon.BulletsCount + "/" +
                //gameLevel.Player.Weapon.MaxBullets, new Vector2(10, 25), Color.Green);
            spriteBatch.DrawString(spriteFont, "Enemies Alive: "  + player.NumEnemiesAlive + "/" +
                player.EnemyList.Length, new Vector2(10, 45), Color.White);

            if(player.AimEnemy != null)
                spriteBatch.DrawString(spriteFont, "Enemy Life: " + player.AimEnemy.Life, 
                    new Vector2(base.Game.GraphicsDevice.Viewport.Width/2.0f, 10), Color.White);
                

            spriteBatch.DrawString(spriteFont, "FPS: " + frameCounter.LastFrameFps, new Vector2(10, 75),
                Color.Red);
            //Renderer.Bar2D.Draw(new Rectangle(20, 80, 100, 100), 1f, "test", gameTime, true);

            //TODO: Clicking on tank will draw properties of this tank
            //Renderer.Bar2D.Draw(barEnemy, Enemy.FuzzyEnemyWeight, "Enemy:"+Enemy.FuzzyEnemyWeight.ToString("N2"), gameTime, currentlySelectedWeight == 0);
            //Renderer.Bar2D.Draw(barPrey, Enemy.FuzzyPreyWeight, "Prey:" + Enemy.FuzzyPreyWeight.ToString("N2"), gameTime, currentlySelectedWeight == 1);
            //Renderer.Bar2D.Draw(barHealth, Enemy.FuzzyHealthWeight, "Health:" + Enemy.FuzzyHealthWeight.ToString("N2"), gameTime, currentlySelectedWeight == 2);

            if (Enemy.SelectedTankParameters != null)
            {   
                int i = 0;
                foreach(KeyValuePair<string,float> kvp in Enemy.SelectedTankParameters)
                {
                    spriteBatch.DrawString(hudFont, "" + kvp.Key + " : " + kvp.Value.ToString("N2"), new Vector2(25, 380 + i * 35), Color.White);
                    i += 1;
                }
             }

            spriteBatch.End();

            base.Draw(gameTime);

            frameCounter.Update(gameTime);
        }

        public void HandleInput(GamePadState currentGamePadState,
            KeyboardState currentKeyboardState, HeightMapInfo heightMapInfo)
        {

            
            float changeAmount = 0;
            
            if (inputHelper.IsKeyJustPressed(Keys.Up))
            {
                currentlySelectedWeight--;
                if (currentlySelectedWeight < 0)
                    currentlySelectedWeight = 2;
            }

            if (inputHelper.IsKeyJustPressed(Keys.Down))
            {
                currentlySelectedWeight = (currentlySelectedWeight + 1) % 3;
            }

            MouseState touchState = Mouse.GetState();
            if (touchState.LeftButton == ButtonState.Pressed)
            {
                Enemy chosenEnemy = ChooseEnemy(camera.Projection, camera.View, touchState);
            }
            // Interpert touch screen presses - get only the first one for this specific case
            if (touchState.LeftButton == ButtonState.Pressed && isDragging == false)
            {

                // Save first touch coordinates
                lastTouchPoint = new Vector2(touchState.X, touchState.Y);

                isDragging = true;

                // Create a rectangle for the touch point
                Rectangle touch = new Rectangle((int)lastTouchPoint.X, (int)lastTouchPoint.Y, 20, 20);

                // Check for collision with the bars
                if (barEnemy.Intersects(touch))
                    currentlySelectedWeight = 0;
                else if (barPrey.Intersects(touch))
                    currentlySelectedWeight = 1;
                else if (barHealth.Intersects(touch))
                    currentlySelectedWeight = 2;

                changeAmount = 0;
                
            }
            else if (touchState.LeftButton == ButtonState.Released)
            {
                // Make coordinates irrelevant
                if (isDragging)
                {
                    lastTouchPoint.X = -1;
                    lastTouchPoint.Y = -1;
                    isDragging = false;
                }
            }
            else if (isDragging == true)
            {
                if (isDragging && currentlySelectedWeight > -1)
                {
                    float DragDelta = touchState.X - lastTouchPoint.X;

                    if (DragDelta > 0.01)
                        changeAmount = 1;
                    else if (DragDelta < -0.01)
                        changeAmount = -1.0f;
                }
            }

            if (currentKeyboardState.IsKeyDown(Keys.Right) ||
                currentGamePadState.IsButtonDown(Buttons.DPadRight))
            {
                changeAmount = 1;
            }
            if (currentKeyboardState.IsKeyDown(Keys.Left) ||
                currentGamePadState.IsButtonDown(Buttons.DPadLeft))
            {
                changeAmount = -1f;
            }

            changeAmount *= .025f;

            // Apply to the changeAmount to the currentlySelectedWeight
            switch (currentlySelectedWeight)
            {
                case 0:
                    //Enemy.FuzzyEnemyWeight += changeAmount;
                    break;
                case 1:
                    //Enemy.FuzzyPreyWeight += changeAmount;
                    break;
                case 2:
                    //Enemy.FuzzyHealthWeight += changeAmount;
                    break;
                default:
                    break;
            }
        }

        private Enemy ChooseEnemy(Matrix projectction, Matrix view, MouseState state)
        {
            int mouseX = state.X;
            int mouseY = state.Y;

            Vector3 nearsource = new Vector3((float)mouseX, (float)mouseY, 0f);
            Vector3 farsource = new Vector3((float)mouseX, (float)mouseY, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearsource, projectction, view, world);
            
            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farsource, projectction, view, world);

            // Create a ray from the near clip plane to the far clip plane.
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            Ray pickRay = new Ray(nearPoint, direction);


            int selectedIndex;
            float selectedDistance = float.MaxValue;
            Enemy chosen = null;
            for(int i = 0; i < Enemy.Units.Length; i += 1 )
            {
                BoundingSphere sphere = Enemy.Units[i].tank.BoundingSphere;
                sphere.Center = Enemy.Units[i].Transformation.Translation;
                Nullable<float> result = pickRay.Intersects(sphere);
                if (result.HasValue)
                {
                    if (result.Value < selectedDistance)
                    {
                        selectedIndex = i;
                        selectedDistance = result.Value;
                        chosen = Enemy.Units[i];
                        Enemy.Select(i);
                    }
                }


            }
            return chosen;
        }
    }
}
