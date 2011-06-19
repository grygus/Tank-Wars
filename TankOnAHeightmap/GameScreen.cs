using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TanksOnAHeightmap.GameBase.Cameras;
using TanksOnAHeightmap.GameBase.Helpers;
using TanksOnAHeightmap.GameLogic;
using TanksOnAHeightmap.Helpers;
using TanksOnAHeightmap.Helpers.Drawing;

namespace TanksOnAHeightmap
{
    internal sealed class GameScreen : DrawableGameComponent
    {
        // Text

        private readonly Player _player;
        private ChaseCamera _camera;

        // Frame counter helper
        private Rectangle _barEnemy = new Rectangle(125, 145, 85, 40);
        private Rectangle _barHealth = new Rectangle(125, 305, 85, 40);
        private Rectangle _barPrey = new Rectangle(125, 225, 85, 40);
        private int _currentlySelectedWeight;
        private FrameCounterHelper _frameCounter;
        private SpriteFont _hudFont;
        private SpriteFont _consolasFont;

        private InputHelper _inputHelper;
        private bool _isDragging;
        private Vector2 _lastTouchPoint;
        private SpriteBatch _spriteBatch;
        private SpriteFont _spriteFont;

        public GameScreen(Game game, Player player)
            : base(game)
        {
            _player = player;
            DrawOrder = 999;
        }

        public override void Initialize()
        {
            // Frame counter
            _inputHelper = Game.Services.GetService(typeof (InputHelper)) as InputHelper;
            _camera = Game.Services.GetService(typeof (ChaseCamera)) as ChaseCamera;
            _frameCounter = new FrameCounterHelper(Game);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create SpriteBatch and add services
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            
            // Font 2D
            _spriteFont = Game.Content.Load<SpriteFont>(GameAssetsPath.FONTS_PATH + "BerlinSans");
            _hudFont = Game.Content.Load<SpriteFont>("hudFont");
            _consolasFont = Game.Content.Load<SpriteFont>(GameAssetsPath.FONTS_PATH + "Consolas");
            base.LoadContent();
        }

        #region Drawing
        private void DrawHud()
        {
            _spriteBatch.DrawString(_spriteFont
                                    , "Health: " + _player.Life + "/" + _player.MaxLife
                                    , new Vector2(10, 5)
                                    , Color.White
                );
            _spriteBatch.DrawString(_spriteFont
                                    , "Enemies Alive: " + _player.NumEnemiesAlive + "/" + _player.EnemyList.Length
                                    , new Vector2(10, 45)
                                    , Color.White
                );
            if (_player.AimEnemy != null)
                _spriteBatch.DrawString(_spriteFont
                                        , "Enemy Life: " + _player.AimEnemy.Life
                                        , new Vector2(Game.GraphicsDevice.Viewport.Width/2.0f, 10)
                                        , Color.White
                    );
            _spriteBatch.DrawString(_spriteFont
                                    , "FPS: " + _frameCounter.LastFrameFps
                                    , new Vector2(10, 75)
                                    , Color.Red
                );
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1.0f, 255);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

            // Draw GAME OVER message
            if (_player.IsDead || _player.NumEnemiesAlive == 0)
            {
                _spriteBatch.DrawString(_spriteFont, "GAME OVER", new Vector2(320, 320), Color.White);
                _spriteBatch.DrawString(_spriteFont, "Press R to restart", new Vector2(320, 220), Color.White);
            }
            else
            {
                DrawHud();

                //Draw Unit Info if Selected
                if (!showPlayerStatus)
                {
                    Enemy selected = Enemy.GetSelectedUnit();
                    if (selected != null)
                    {
                        Renderer.Bar2D.Draw(_barEnemy,
                                            selected.FuzzyBrain.FuzzyEnemyWeight,
                                            "Enemy:" + selected.FuzzyBrain.FuzzyEnemyWeight.ToString("N2"),
                                            gameTime,
                                            _currentlySelectedWeight == 0);

                        Renderer.Bar2D.Draw(_barPrey,
                                            selected.FuzzyBrain.FuzzyPreyWeight,
                                            "Prey:" + selected.FuzzyBrain.FuzzyPreyWeight.ToString("N2"),
                                            gameTime,
                                            _currentlySelectedWeight == 1);

                        Renderer.Bar2D.Draw(_barHealth,
                                            selected.FuzzyBrain.FuzzyHealthWeight,
                                            "Health:" + selected.FuzzyBrain.FuzzyHealthWeight.ToString("N2"),
                                            gameTime,
                                            _currentlySelectedWeight == 2);
                        if (selected.FuzzyBrain.fuzzyParameters != null)
                        {
                            Dictionary<string, float>.Enumerator enumerator = selected.FuzzyBrain.fuzzyParameters.GetEnumerator();
                            for (int i = 0; i < selected.FuzzyBrain.fuzzyParameters.Count; i += 1)
                            {
                                enumerator.MoveNext();
                                var kvp = enumerator.Current;
                                _spriteBatch.DrawString(_hudFont
                                                        , "" + kvp.Key + " : " + kvp.Value.ToString("N2")
                                                        , new Vector2(25, 380 + i * 35)
                                                        , Color.White
                                    );
                            }
                        }
                    }
                }
                else
                {
                    Renderer.Bar2D.Draw(_barEnemy,
                                            _player.FuzzyControl.FuzzyDecision.FuzzyEnemyWeight,
                                            "Enemy:" + _player.FuzzyControl.FuzzyDecision.FuzzyEnemyWeight.ToString("N2"),
                                            gameTime,
                                            _currentlySelectedWeight == 0);

                    Renderer.Bar2D.Draw(_barPrey,
                                        _player.FuzzyControl.FuzzyDecision.FuzzyPreyWeight,
                                        "Prey:" + _player.FuzzyControl.FuzzyDecision.FuzzyPreyWeight.ToString("N2"),
                                        gameTime,
                                        _currentlySelectedWeight == 1);

                    Renderer.Bar2D.Draw(_barHealth,
                                        _player.FuzzyControl.FuzzyDecision.FuzzyHealthWeight,
                                        "Health:" + _player.FuzzyControl.FuzzyDecision.FuzzyHealthWeight.ToString("N2"),
                                        gameTime,
                                        _currentlySelectedWeight == 2);
                    if (_player.FuzzyControl.FuzzyDecision.fuzzyParameters != null)
                    {
                        Dictionary<string, float>.Enumerator enumerator = _player.FuzzyControl.FuzzyDecision.fuzzyParameters.GetEnumerator();
                        for (int i = 0; i < _player.FuzzyControl.FuzzyDecision.fuzzyParameters.Count; i += 1)
                        {
                            enumerator.MoveNext();
                            var kvp = enumerator.Current;
                            _spriteBatch.DrawString(_hudFont
                                                    , "" + kvp.Key + " : " + kvp.Value.ToString("N2")
                                                    , new Vector2(25, 380 + i * 35)
                                                    , Color.White
                                );
                        }
                    }

                    ShowForce(_player.Force.Description);
                    ShowForceVectors(_player.Position,_player.Force.Vectors);
                }
            }

            _spriteBatch.End();
            base.Draw(gameTime);
            _frameCounter.Update(gameTime);
        }

        private Color[] colorTab = {
                                       Color.Green, Color.Red, Color.Blue, Color.Pink, Color.Orange, Color.Violet,
                                       Color.Cyan,Color.Olive,Color.Peru,Color.Salmon,Color.Brown,Color.Yellow
                                   };
        private void ShowForceVectors(Vector3 position,List<Vector3> vectors)
        {
            if (vectors.Count > 0)
            {
                for (int i = 0; i < vectors.Count; i++)
                {
                    Renderer.Line3D.Draw(position, position + vectors[i], colorTab[i]);
                }
            }
        }

        public void ShowForce(List<string> description)
        {
            Vector2 maxSize = Vector2.Zero;
            Vector2 tmpSize;
            int height = (int)_consolasFont.MeasureString("I").Y + _consolasFont.LineSpacing;

            if (description.Count > 0)
            {
                foreach (string s in description)
                {
                    tmpSize = _consolasFont.MeasureString(s);
                    if (tmpSize.X > maxSize.X)
                    {
                        maxSize.X = tmpSize.X;
                    }
                    maxSize.Y += tmpSize.Y + _consolasFont.LineSpacing;
                }

                Vector2 offset = new Vector2();
                offset.X = GraphicsDevice.Viewport.Width - (int)maxSize.X;
                offset.Y = GraphicsDevice.Viewport.Height - (int) maxSize.Y;

                Texture2D onePixelWhite = new Texture2D(GraphicsDevice, 1, 1, false,SurfaceFormat.Color);
                
                Rectangle rec = new Rectangle((int)offset.X-10, (int)offset.Y-10, (int)maxSize.X+10, (int)maxSize.Y+10);

                Color col = new Color(0.0f,0.0f,0.2f,0.8f);
                onePixelWhite.SetData<Color>(new Color[] { col });

                _spriteBatch.Draw(onePixelWhite, rec, col);

                for (int i = 0; i < description.Count;i++ )
                {
                    string s = description[i];
                    _spriteBatch.DrawString(_consolasFont, s, offset, colorTab[i]);
                    offset.Y += height;
                }
            }
        }
        #endregion

        private bool showPlayerStatus;
        public void HandleInput(GamePadState currentGamePadState,
                                KeyboardState currentKeyboardState, HeightMapInfo heightMapInfo)
        {
            float changeAmount = 0;

            if (_inputHelper.IsKeyJustPressed(Keys.Up))
            {
                _currentlySelectedWeight--;
                if (_currentlySelectedWeight < 0)
                    _currentlySelectedWeight = 2;
            }

            if (_inputHelper.IsKeyJustPressed(Keys.Down))
            {
                _currentlySelectedWeight = (_currentlySelectedWeight + 1)%3;
            }

            MouseState touchState = Mouse.GetState();
            if (touchState.LeftButton == ButtonState.Pressed)
            {
                ChooseEnemy(_camera.Projection, _camera.View, touchState);
            }
            // Interpert touch screen presses - get only the first one for this specific case
            if (touchState.LeftButton == ButtonState.Pressed && _isDragging == false)
            {
                // Save first touch coordinates
                _lastTouchPoint = new Vector2(touchState.X, touchState.Y);


                // Create a rectangle for the touch point
                var touch = new Rectangle((int) _lastTouchPoint.X, (int) _lastTouchPoint.Y, 20, 20);
                _isDragging = true;
                // Check for collision with the bars
                if (_barEnemy.Intersects(touch))
                    _currentlySelectedWeight = 0;
                else if (_barPrey.Intersects(touch))
                    _currentlySelectedWeight = 1;
                else if (_barHealth.Intersects(touch))
                    _currentlySelectedWeight = 2;
                else
                    _isDragging = false;

                changeAmount = 0;
            }
            else if (touchState.LeftButton == ButtonState.Released)
            {
                // Make coordinates irrelevant
                if (_isDragging)
                {
                    _lastTouchPoint.X = -1;
                    _lastTouchPoint.Y = -1;
                    _isDragging = false;
                }
            }
            else if (_isDragging)
            {
                if (_isDragging && _currentlySelectedWeight > -1)
                {
                    float dragDelta = touchState.X - _lastTouchPoint.X;

                    if (dragDelta > 0.01)
                        changeAmount = 1;
                    else if (dragDelta < -0.01)
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
            if (_inputHelper.IsKeyJustPressed(Keys.Space))
            {
                showPlayerStatus = !showPlayerStatus;
            }

            if(!showPlayerStatus)
            {
                Enemy selected = Enemy.GetSelectedUnit();
                if (selected != null)
                {
                    switch (_currentlySelectedWeight)
                    {
                        case 0:
                            selected.FuzzyBrain.FuzzyEnemyWeight += changeAmount;
                            break;
                        case 1:
                            selected.FuzzyBrain.FuzzyPreyWeight += changeAmount;
                            break;
                        case 2:
                            selected.FuzzyBrain.FuzzyHealthWeight += changeAmount;
                            break;
                        default:
                            break;
                    }
                }
            }
            else
            {   Enemy.ClearSelection();
                switch (_currentlySelectedWeight)
                {
                    case 0:
                        _player.FuzzyControl.FuzzyDecision.FuzzyEnemyWeight += changeAmount;
                        break;
                    case 1:
                        _player.FuzzyControl.FuzzyDecision.FuzzyPreyWeight += changeAmount;
                        break;
                    case 2:
                        _player.FuzzyControl.FuzzyDecision.FuzzyHealthWeight += changeAmount;
                        break;
                    default:
                        break;
                }
            }
             
        }

        //TODO: Move to Enemy class And separate into 2 methods
        private void ChooseEnemy(Matrix projectction, Matrix view, MouseState state)
        {
            int mouseX = state.X;
            int mouseY = state.Y;

            var nearsource = new Vector3(mouseX, mouseY, 0f);
            var farsource = new Vector3(mouseX, mouseY, 1f);

            Matrix world = Matrix.CreateTranslation(0, 0, 0);

            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearsource, projectction, view, world);

            Vector3 farPoint = GraphicsDevice.Viewport.Unproject(farsource, projectction, view, world);

            // Create a ray from the near clip plane to the far clip plane.
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            var pickRay = new Ray(nearPoint, direction);


            float selectedDistance = float.MaxValue;
            Enemy chosen = null;
            for (int i = 0; i < Enemy.Units.Length; i += 1)
            {
                BoundingSphere sphere = Enemy.Units[i].tank.BoundingSphere;
                sphere.Center = Enemy.Units[i].Transformation.Translation;
                float? result = pickRay.Intersects(sphere);
                if (result.HasValue)
                {
                    if (result.Value < selectedDistance)
                    {
                        selectedDistance = result.Value;
                        chosen = Enemy.Units[i];
                        Enemy.Select(i);
                    }
                }
            }

            if( chosen == null)
                Enemy.ClearSelection();
        }
    }
}