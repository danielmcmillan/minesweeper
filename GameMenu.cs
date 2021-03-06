﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Minesweeper
{
    public class GameMenu
    {
        Game1 game;
        SpriteBatch spriteBatch;
        SpriteFont menuFont;

        public GameMenu(Game1 game, SpriteBatch spriteBatch)
        {
            this.game = game;
            this.spriteBatch = spriteBatch;
            //Load the font for the numbers written on uncovered squares
            menuFont = game.Content.Load<SpriteFont>("MenuFont");
        }

        public bool HandleClick(MouseState state, MouseState old)
        {
            return true;
        }

        public void Draw()
        {
            
        }
    }
}
