using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Minesweeper
{
    public class GameBoard
    {
        Game1 game;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D squaresTexture;
        const int textureGridSize = 152;

        enum TextureYOffset
        {
            DarkSquare = 0,
            LightSquare = textureGridSize,
            GraySquare = textureGridSize * 2,
            Mine = textureGridSize * 3,
            Flag = textureGridSize * 4,
            Cross = textureGridSize * 5
        }

        BoardState state;
        bool started = false;
        bool lost = false;
        int xDie = -1;
        int yDie = -1;
        int screenWidth = Game1.ScreenWidth, screenHeight = Game1.ScreenHeight;
        int targetWidth, targetHeight;
        Rectangle currentBoard;
        int squareSize;
        int mouseOverX = -1;
        int mouseOverY = -1;
        bool mouseHeld = false;

        public GameBoard(Game1 game)
        {
            this.game = game;
            state = new BoardState();
            ScreenSizeChanged(Game1.ScreenWidth, Game1.ScreenHeight);
        }

        public GameBoard(Game1 game, BoardState oldState)
        {
            this.game = game;
            state = oldState;
            started = true;
            ScreenSizeChanged(Game1.ScreenWidth, Game1.ScreenHeight);
        }

        //Update the location of size of the board
        public void ScreenSizeChanged(int width, int height)
        {
            screenWidth = width;
            screenHeight = height;
            targetWidth = width - 4;
            targetHeight = height - 80;

            if (state.Squares != null)
            {
                updateCurrentBoard();
            }
        }

        private void updateCurrentBoard()
        {
            //Get a size for the squares
            int squareWidth = targetWidth / getBoardWidth();
            int squareHeight = targetHeight / getBoardHeight();
            squareSize = (squareWidth < squareHeight) ? squareWidth : squareHeight;
            int boardWidth = squareSize * getBoardWidth();
            int boardHeight = squareSize * getBoardHeight();
            currentBoard = new Rectangle(screenWidth / 2 - boardWidth / 2, screenHeight / 2 - boardHeight / 2,
                boardWidth, boardHeight);
        }

        private int getBoardWidth()
        {
            return state.Squares.GetLength(0);
        }

        private int getBoardHeight()
        {
            return state.Squares.GetLength(1);
        }

        public void NewGame(int width, int height, int mines)
        {
            started = false;
            state.MineCount = mines;
            lost = false;
            xDie = -1;
            yDie = -1;
            //Make sure settings for game are valid
            if (mines > (width - 1) * (height - 1) || width < 6 || height < 6)
            {
                //return;
            }
            state.Squares = new Square[width, height];
            state.SquaresLeft = state.Squares.Length - mines; //The number of non-bomb squares to uncover
            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    state.Squares[x, y] = new Square();
                }
            }
            updateCurrentBoard();
        }

        public void LoadContent(SpriteBatch spriteBatch)
        {
            this.spriteBatch = spriteBatch;
            //Load the font for the numbers written on uncovered squares
            font = game.Content.Load<SpriteFont>("MenuFont");
            //Load square textures
            squaresTexture = game.Content.Load<Texture2D>("SquareTexturesLarge");
        }

        public bool HandleClick(MouseState mouseState, MouseState oldMouseState)
        {
            if (currentBoard.Contains(mouseState.X, mouseState.Y))
            {
                if (lost) //Don't allow input if game is over, new game should be started
                {
                    mouseOverX = mouseOverY = -1;
                    return false;
                }
                mouseOverX = (mouseState.X - currentBoard.Left) / squareSize;
                mouseOverY = (mouseState.Y - currentBoard.Top) / squareSize;
                mouseHeld = mouseState.LeftButton == ButtonState.Pressed;
                if (mouseOverX < getBoardWidth() && mouseOverY < getBoardHeight() &&
                    state.Squares[mouseOverX, mouseOverY].Number < 0) //If square isn't already uncovered
                {
                    if (mouseState.LeftButton == ButtonState.Released && oldMouseState.LeftButton == ButtonState.Pressed &&
                        !state.Squares[mouseOverX, mouseOverY].IsFlagged)
                    {
                        if (started)
                        {
                            //Left click released on square, uncover it
                            uncoverSquare(mouseOverX, mouseOverY);
                        }
                        else
                        {
                            //Generate field with no bombs near pressed square
                            generateField(mouseOverX, mouseOverY);
                        }
                    }
                    else if (mouseState.RightButton == ButtonState.Pressed && oldMouseState.RightButton == ButtonState.Released)
                    {
                        //Square was right clicked, flag it
                        state.Squares[mouseOverX, mouseOverY].IsFlagged = !state.Squares[mouseOverX, mouseOverY].IsFlagged;
                    }
                }
            }
            else
            {
                mouseOverX = -1;
                mouseOverY = -1;
                mouseHeld = false;
            }
            return mouseOverX > -1;
        }

        private void generateField(int xPressed, int yPressed)
        {
            started = true;
            //Place the mines
            Random rndm = new Random();
            int minesLeft = state.MineCount;
            while (minesLeft > 0)
            {
                int x = rndm.Next(0, getBoardWidth());
                int y = rndm.Next(0, getBoardHeight());
                if (!state.Squares[x, y].IsMine && !adjacentOrEqual(new Point(x, y), new Point(xPressed, yPressed)))
                {
                    state.Squares[x, y].IsMine = true;
                    --minesLeft;
                }
            }
            uncoverSquare(xPressed, yPressed);
        }

        private bool adjacentOrEqual(Point one, Point two)
        {
            return (one.X == two.X || one.X == two.X - 1 || one.X == two.X + 1) &&
                (one.Y == two.Y || one.Y == two.Y - 1 || one.Y == two.Y + 1);
        }

        private List<Point> adjacentSquares(int sqrx, int sqry)
        {
            List<Point> adjSquares = new List<Point>();
            for (int x = -1; x <= 1; ++x)
            {
                for (int y = -1; y <= 1; ++y)
                {
                    if (x == 0 && y == 0) continue;
                    int adjx = sqrx + x;
                    int adjy = sqry + y;
                    //Square exists
                    if (adjx >= 0 && adjy >= 0 && adjx < getBoardWidth() && adjy < getBoardHeight())
                    {
                        adjSquares.Add(new Point(adjx, adjy));
                    }
                }
            }
            return adjSquares;
        }

        private int bombCount(List<Point> adjSquares)
        {
            int bombCount = 0;
            foreach (Point sqr in adjSquares)
            {
                if (state.Squares[sqr.X, sqr.Y].IsMine)
                {
                    ++bombCount;
                }
            }
            return bombCount;
        }

        //Returns how many bombs there were around it
        private void uncoverSquare(int x, int y)
        {
            //If square exists
            if (state.Squares != null && getBoardWidth() > x && getBoardHeight() > y)
            {
                if (state.Squares[x, y].IsMine)
                {
                    //Uncovering a mine, game over
                    xDie = x;
                    yDie = y;
                    gameOver(false);
                    return;
                }
                --state.SquaresLeft; //One less square to uncover
                state.Squares[x, y].IsFlagged = false; //Unflag square
                List<Point> adjSquares = adjacentSquares(x, y); //Get list of adjacent square positions
                int bombs = bombCount(adjSquares); //Count the number of surrounding bombs
                state.Squares[x, y].Number = bombs; //Set the squares number
                if (bombs == 0) //Surrounding squares should be automatically uncovered if none of them are bombs
                {
                    foreach (Point sqr in adjSquares)
                    {
                        //Uncover the square if it's still covered
                        if (state.Squares[sqr.X, sqr.Y].Number < 0)
                        {
                            uncoverSquare(sqr.X, sqr.Y);
                        }
                    }
                }
                //Check if there are no more squares uncovered
                if (state.SquaresLeft <= 0)
                {
                    gameOver(true);
                }
            }
        }

        private void gameOver(bool won)
        {
            lost = true;
        }

        public void Draw()
        {
            if (currentBoard != null && currentBoard.Left > 0)
                spriteBatch.DrawString(font, string.Format("X: {0}, Y: {1}", mouseOverX, mouseOverY), new Vector2(currentBoard.Left, 5), Color.White);
            if (state.Squares != null)
            {
                //Draw the squares
                for (int x = 0; x < getBoardWidth(); ++x)
                {
                    for (int y = 0; y < getBoardHeight(); ++y)
                    {
                        Square square = state.Squares[x, y];
                        TextureYOffset squareTexture;
                        //The square is uncovered, or game is over and it's a bomb or a flag
                        if (square.Number > -1 || (lost && (square.IsMine || square.IsFlagged)))
                        {
                            squareTexture = TextureYOffset.GraySquare;
                        }
                        else //Square is covered
                        {
                            //If the mouse is over this square, make it shiny. If it's flagged and mouse is pressed on it,
                            //it shouldn't be shiny
                            if (mouseOverX == x && mouseOverY == y && !(square.IsFlagged && mouseHeld))
                            {
                                squareTexture = (mouseHeld) ? TextureYOffset.GraySquare : TextureYOffset.LightSquare;
                            }
                            else
                            {
                                squareTexture = TextureYOffset.DarkSquare;
                            }
                        }
                        //Draw the square with the texture that has been chosen
                        Rectangle squareRect = new Rectangle(currentBoard.Left + x * squareSize,
                            currentBoard.Top + y * squareSize, squareSize, squareSize);
                        spriteBatch.Draw(squaresTexture, squareRect,
                            new Rectangle(0, (int)squareTexture, textureGridSize, textureGridSize), Color.White);

                        //Show bombs if the game has ended
                        if (lost && (square.IsMine || square.IsFlagged)) //Show bomb on flagged aswell, but with cross ontop
                            spriteBatch.Draw(squaresTexture, squareRect, new Rectangle(0, (int)TextureYOffset.Mine,
                                textureGridSize, textureGridSize), (xDie == x && yDie == y) ? Color.Red : Color.White);

                        if (square.Number > 0)
                        {
                            //Draw number for square
                            spriteBatch.Draw(squaresTexture, squareRect, new Rectangle(textureGridSize,
                                (square.Number - 1) * textureGridSize, textureGridSize, textureGridSize), Color.White);
                        }
                        else if (square.IsFlagged)
                        {
                            if (lost && !square.IsMine) //Flag is wrong
                            {
                                //Draw a cross on square
                                spriteBatch.Draw(squaresTexture, squareRect,
                                    new Rectangle(0, (int)TextureYOffset.Cross, textureGridSize, textureGridSize), Color.White);
                            }
                            else
                            {
                                //Draw flag on square
                                spriteBatch.Draw(squaresTexture, squareRect,
                                    new Rectangle(0, (int)TextureYOffset.Flag, textureGridSize, textureGridSize), Color.White);
                            }
                        }
                    }
                }
            }
        }

        public BoardState GetBoardState()
        {
            if (!started || lost)
                return null;
            return state;
        }
    }
}