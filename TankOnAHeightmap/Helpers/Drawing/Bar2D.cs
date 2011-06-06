using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace TanksOnAHeightmap.Helpers.Drawing
{
    public class Bar2D
    {
        GraphicsDevice graphicsDevice;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D onePixelWhite;
        Game game;

        public Bar2D(GraphicsDevice graphicsDevice,Game game)
        {
            this.graphicsDevice = graphicsDevice;
            this.game = game;
            Initialize();
        }

        public void Initialize()
        {
            spriteBatch = new SpriteBatch(graphicsDevice);
            onePixelWhite = new Texture2D(graphicsDevice, 1, 1, false, SurfaceFormat.Color);
            onePixelWhite.SetData<Color>(new Color[] { Color.White });
            font = game.Content.Load<SpriteFont>("hudFont");
        }
        
        public void Draw(Rectangle bar, float barWidthNormalized, string label, GameTime gameTime, bool highlighted)
        {
            Color tintColor = Color.White;
            
            // if the bar is highlighted, we want to make it pulse with a red tint.
            if (highlighted)
            {
                // to do this, we'll first generate a value t, which we'll use to
                // determine how much tint to have.
                float t = (float)Math.Sin(10 * gameTime.TotalGameTime.TotalSeconds);

                // Sin varies from -1 to 1, and we want t to go from 0 to 1, so we'll 
                // scale it now.
                t = .5f + .5f * t;

                // finally, we'll calculate our tint color by using Lerp to generate
                // a color in between Red and White.
                tintColor = new Color(Vector4.Lerp(
                    Color.Red.ToVector4(), Color.White.ToVector4(), t));
            }

            // calculate how wide the bar should be, and then draw it.
            bar.Width = (int)(bar.Width * barWidthNormalized);
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);
            spriteBatch.Draw(onePixelWhite, bar, tintColor);

            // finally, draw the label to the left of the bar.
            Vector2 labelSize = font.MeasureString(label);
            Vector2 labelPosition = new Vector2(bar.X - 5 - labelSize.X, bar.Y);
            spriteBatch.DrawString(font, label, labelPosition, tintColor);
            spriteBatch.End();
        }
    }
}
