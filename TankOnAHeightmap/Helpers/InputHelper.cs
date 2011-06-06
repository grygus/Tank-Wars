using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace TanksOnAHeightmap.GameBase.Helpers
{
    public class InputHelper
    {
        PlayerIndex playerIndex;

        // Keyboard
        KeyboardState keyboardState;
        KeyboardState lastKeyboardState;
        Dictionary<Buttons, Keys> keyboardMap;

        // Game pad
        GamePadState gamePadState;
        GamePadState lastGamePadState;

        public InputHelper(PlayerIndex playerIndex)
            : this(playerIndex, null)
        {
        }

        public InputHelper(PlayerIndex playerIndex, Dictionary<Buttons, Keys> keyboardMap)
        {
            this.playerIndex = playerIndex;
            this.keyboardMap = keyboardMap;
        }

        public void Update()
        {
            lastGamePadState = gamePadState;
            gamePadState = GamePad.GetState(playerIndex);

            if (!gamePadState.IsConnected)
            {
                lastKeyboardState = keyboardState;
                keyboardState = Keyboard.GetState(playerIndex);

            }
        }

        public float GetLeftTrigger()
        {
            float value = 0;

            if (gamePadState.IsConnected)
                value = gamePadState.Triggers.Left;
            else if (keyboardMap != null)
            {
                if (keyboardState.IsKeyDown(keyboardMap[Buttons.LeftTrigger]))
                    value = 1;
            }

            return value;
        }

        public float GetRightTrigger()
        {
            float value = 0;

            if (gamePadState.IsConnected)
                value = gamePadState.Triggers.Right;
            else if (keyboardMap != null)
            {
                if (keyboardState.IsKeyDown(keyboardMap[Buttons.RightTrigger]))
                    value = 1;
            }

            return value;
        }

        public Vector2 GetLeftThumbStick()
        {
            Vector2 thumbPosition = Vector2.Zero;

            if (gamePadState.IsConnected)
                thumbPosition = gamePadState.ThumbSticks.Left;
            else if (keyboardMap != null)
            {
                if (keyboardState.IsKeyDown(keyboardMap[Buttons.LeftThumbstickUp]))
                    thumbPosition.Y = 1;
                else if (keyboardState.IsKeyDown(keyboardMap[Buttons.LeftThumbstickDown]))
                    thumbPosition.Y = -1;
                if (keyboardState.IsKeyDown(keyboardMap[Buttons.LeftThumbstickRight]))
                    thumbPosition.X = 1;
                else if (keyboardState.IsKeyDown(keyboardMap[Buttons.LeftThumbstickLeft]))
                    thumbPosition.X = -1;
            }

            return thumbPosition;
        }

        public Vector2 GetRightThumbStick()
        {
            Vector2 thumbPosition = Vector2.Zero;

            if (gamePadState.IsConnected)
                thumbPosition = gamePadState.ThumbSticks.Right;
            else if (keyboardMap != null)
            {
                if (keyboardState.IsKeyDown(keyboardMap[Buttons.RightThumbstickUp]))
                    thumbPosition.Y = 1;
                else if (keyboardState.IsKeyDown(keyboardMap[Buttons.RightThumbstickDown]))
                    thumbPosition.Y = -1;
                if (keyboardState.IsKeyDown(keyboardMap[Buttons.RightThumbstickRight]))
                    thumbPosition.X = 1;
                else if (keyboardState.IsKeyDown(keyboardMap[Buttons.RightThumbstickLeft]))
                    thumbPosition.X = -1;
            }

            return thumbPosition;
        }

        public bool IsKeyPressed(Buttons button)
        {
            bool pressed = false;

            if (gamePadState.IsConnected)
                pressed = gamePadState.IsButtonDown(button);
            else if (keyboardMap != null)
            {
                Keys key = keyboardMap[button];
                pressed = keyboardState.IsKeyDown(key);
            }

            return pressed;
        }

        public bool IsKeyJustPressed(Keys key)
        {
            bool pressed = false;

            /*if (gamePadState.IsConnected)
                pressed = (gamePadState.IsButtonDown(button) && lastGamePadState.IsButtonUp(button));
            else if (keyboardMap != null)
            {
                Keys key = keyboardMap[button];*/
                pressed = (keyboardState.IsKeyDown(key) && lastKeyboardState.IsKeyUp(key));
            //}

            return pressed;
        }
    }

}
