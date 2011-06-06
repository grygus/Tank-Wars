using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

namespace TanksOnAHeightmap.Helpers
{
    public class FrameCounterHelper : GameComponent
    {
        // Frame counter
        int elapsedTime;
        int fpsCount;
        int currentFps;

        #region Properties
        public int LastFrameFps
        {
            get
            {
                return currentFps;
            }
        }
        #endregion

        public FrameCounterHelper(Game game)
            : base(game)
        {
            elapsedTime = 0;
            fpsCount = 0;
            currentFps = 0;
        }

        public override void Update(GameTime time)
        {
            elapsedTime += time.ElapsedGameTime.Milliseconds;
            fpsCount++;

            if (elapsedTime >= 1000)
            {
                currentFps = (int)(fpsCount * 1000.0f / elapsedTime);
                elapsedTime = 0;
                fpsCount = 0;

                elapsedTime -= 1000;
            }
        }
    }
}
