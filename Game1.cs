using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Minesweeper
{
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public const string lastGameFile = "lastgame.bin";
        public const int ScreenWidth = 800, ScreenHeight = 600;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        MouseState oldState;
        KeyboardState oldKeyState;
        GameBoard gameBoard;
        GameMenu menu;

        Rectangle menuButtonRectangle;
        Texture2D menuButtonTexture;
        bool OnMenuButton = false;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = ScreenWidth;
            graphics.PreferredBackBufferHeight = ScreenHeight;
            Content.RootDirectory = "Content";
        }

        public void ToggleFullscreen()
        {
            if (graphics.IsFullScreen)
            {
                graphics.PreferredBackBufferWidth = ScreenWidth;
                graphics.PreferredBackBufferHeight = ScreenHeight;
                graphics.IsFullScreen = false;
                graphics.ApplyChanges();
            }
            else
            {
                graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                graphics.IsFullScreen = true;
                graphics.ApplyChanges();
            }
            gameBoard.ScreenSizeChanged(graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
        }

        protected override void Initialize()
        {
            //Show the mouse cursor
            this.IsMouseVisible = true;
            
            //Create game board
            gameBoard = new GameBoard(this);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures
            spriteBatch = new SpriteBatch(GraphicsDevice);
            //Tell game board to load content
            gameBoard.LoadContent(spriteBatch);

            menuButtonTexture = Content.Load<Texture2D>("MenuImage");
            menuButtonRectangle = new Rectangle(5, 5, menuButtonTexture.Width, menuButtonTexture.Height);
        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            //Gets the current key and mouse state
            KeyboardState keyState = Keyboard.GetState();
            MouseState mouseState = Mouse.GetState();
            //If there is no old mouse state, set it to blank mouse state
            if (oldState == null)
            {
                oldState = new MouseState();
            }
            //If there is no old keyboard state, set it to blank keyboard state
            if (oldKeyState == null)
            {
                oldKeyState = new KeyboardState();
            }
            //Ask menu to handle click if it's not null
            if (menu != null)
            {
                menu.HandleClick(mouseState, oldState);
            }
            //If click isn't handled, get gameBoard to handle it
            //Checks menu as handleClick returns false if menubutton was pressed
            else if (!handleClick(mouseState, oldState) && menu == null)
            {
                gameBoard.HandleClick(mouseState, oldState);
            }
            //Check F11 for toogle fullscreen
            if (keyState.IsKeyDown(Keys.F11) && oldKeyState.IsKeyUp(Keys.F11))
            {
                ToggleFullscreen();
            }
            //Set the old state of the mouse
            oldState = mouseState;
            oldKeyState = keyState;
            base.Update(gameTime);
        }

        private bool handleClick(MouseState state, MouseState old)
        {
            OnMenuButton = menuButtonRectangle.Contains(state.X, state.Y);
            //If left button was pressed for first time
            if (state.LeftButton == ButtonState.Pressed && old.LeftButton == ButtonState.Released && OnMenuButton)
            {
                //menu = new GameMenu(this, spriteBatch);
                //OnMenuButton = false;
                gameBoard.NewGame(7, 7, 7);
            }
            return OnMenuButton;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            //Draw the game board
            gameBoard.Draw();
            //Draw menu button
            spriteBatch.Draw(menuButtonTexture, menuButtonRectangle, (OnMenuButton) ? Color.Gray : Color.White);

            spriteBatch.End();

            base.Draw(gameTime);
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            base.OnExiting(sender, args);

            BoardState bs = gameBoard.GetBoardState();
            if (bs == null)
            {
                File.Delete(lastGameFile);
                return;
            }
            BinaryFormatter bf = new BinaryFormatter();
            try
            {
                using (FileStream fs = File.Open(lastGameFile, FileMode.Create))
                {
                    bf.Serialize(fs, bs);
                }
            }
            catch
            {

            }
        }
    }
}
