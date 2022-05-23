using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;

namespace minesweeper2 {
    /*
     * TODO: meny
     * TODO: settingsgrej
     */
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private int _screenWidth, _screenHeight, _screenCenterY, _screenCenterX;
        Random rand = new Random();

        private const int GAME_WIDTH = 1400;
        private const int GAME_HEIGHT = 900;
        private const int CELL_SIZE = 48;
        private const int NUM_GRENADES = 50;
        private const int NUM_COINS = 5;
        private const int BOARD_SIZE_WIDTH = 15;
        private const int BOARD_SIZE_HEIGHT = 15;
        //+2 för att ha en border runt banan,  gör det lättare kolla antalet bomber
        private Cell[,] cells = new Cell[BOARD_SIZE_HEIGHT + 2, BOARD_SIZE_WIDTH + 2];
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = GAME_WIDTH;
            _graphics.PreferredBackBufferHeight = GAME_HEIGHT;
            _graphics.ApplyChanges();
            _screenWidth = GraphicsDevice.Viewport.Width;
            _screenHeight = GraphicsDevice.Viewport.Height;
            _screenCenterX = _screenWidth / 2;
            _screenCenterY = _screenHeight / 2;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            /*
             * Beräkna hur lång tid det tar innan man hittar en 8a på spelplanen
            int antalSlump = 0;
            int summa = 0;
            double average = 0;
            bool testaIgen = true;
            Stopwatch stopwatch = new Stopwatch();
            for (int x = 0; x < 100; x++) {
                antalSlump = 0;
                testaIgen = true;
                stopwatch.Reset();
                stopwatch.Start();
                while (testaIgen) {
                    antalSlump++;
                    loadCells(cells, NUM_GRENADES, NUM_COINS);
                    cells = shuffle(cells);
                    cells = countNearbyBombs(cells);

                    for (int i = 1; i < cells.GetLength(0)-1; i++) {
                        for (int j = 1; j < cells.GetLength(1)-1; j++) {
                            if (cells[i, j] is NormalCell) {
                                if (((NormalCell)cells[i, j]).NearbyGrenades == 8) {
                                    Debug.WriteLine("grattis!!! det tog " + antalSlump + " gånger");
                                    testaIgen = false;
                                }
                            }
                        }
                    }
                    
                }
                stopwatch.Stop();
                summa += antalSlump;
                Debug.WriteLine(stopwatch.ElapsedMilliseconds);
            }
            average = summa/100;
            Debug.WriteLine("average: " + average + "\nsumma: " + summa);
            */
            
            
            //Skapa 2D array med alla celler, bomber och coins, blanda sedan
            //Initiera positioner och storlek på alla celler
            loadCells(cells, NUM_GRENADES, NUM_COINS);
            cells = shuffle(cells);

            //Beräkna antalet bomber runt alla NormalCeller
            cells = countNearbyBombs(cells);

            //beräkna avståndet till närmsta coin för att ge cellen en korrekt bakgrundsfärg
            cells = distanceToCoin(cells);
             
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            /* TODO: click()
             * TODO: flag()
             * TODO: revealMultipleCells()
             * TODO: om det är en bomb, lägg till eventull animation och att spelaren förlorar
             * TODO: om det är ett coin, lägg till eentuell animation och att spelaren vinner om alla coins är hittade
             * TODO: tidtagning
             * TODO: skicka tiden Till servern i formatet string
             * TODO: håll ordning på antalet bomber och coins kvar
             */


            MouseState ms = Mouse.GetState();

            for (int i = 1; i < cells.GetLength(0)-1; i++) {
                for (int j = 1; j < cells.GetLength(1)-1; j++) {
                    if (ms.Position.X < cells[i, j].Position.X + CELL_SIZE && 
                        ms.Position.X > cells[i, j].Position.X &&
                        ms.Position.Y < cells[i, j].Position.Y + CELL_SIZE &&
                        ms.Position.Y > cells[i, j].Position.Y) {
                        if (ms.LeftButton == ButtonState.Pressed) {
                            cells[i, j].IsClicked = true;
                        }
                    }
                }
            }

                    base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();

            for (int i = 1; i < cells.GetLength(0)-1; i++) {
                for (int j = 1; j < cells.GetLength(1)-1; j++) {
                    if (cells[i, j] is Coin) {
                        //Coin
                        if (!cells[i, j].IsClicked) {
                            _spriteBatch.Draw(cells[i, j].CoverTexture, cells[i, j].getRetangle(), Color.Gray);
                        }
                        else {
                            _spriteBatch.Draw(cells[i, j].Texture, cells[i, j].getRetangle(), Color.Gold);
                            if (((NormalCell)cells[i, j]).NearbyGrenades != 0) {
                                _spriteBatch.Draw(((Coin)cells[i, j]).NumberTexture, cells[i, j].getRetangle(), Color.White);
                            }
                        }
                    }
                    //Bomb
                    else if (cells[i, j] is Grenade) {
                        if (!cells[i, j].IsClicked) {
                            _spriteBatch.Draw(cells[i, j].CoverTexture, cells[i, j].getRetangle(), Color.Gray);
                        }
                        else {
                            _spriteBatch.Draw(cells[i, j].Texture, cells[i, j].getRetangle(), Color.White);
                        }
                    }
                    //NormalCell
                    else {
                        if (!cells[i, j].IsClicked) {
                            _spriteBatch.Draw(cells[i, j].CoverTexture, cells[i, j].getRetangle(), Color.Gray);
                        }
                        else {
                            _spriteBatch.Draw(cells[i, j].Texture, cells[i, j].getRetangle(), ((NormalCell)cells[i, j]).Color);
                            if (((NormalCell)cells[i, j]).NearbyGrenades != 0) {
                                _spriteBatch.Draw(((NormalCell)cells[i, j]).NumberTexture, cells[i, j].getRetangle(), Color.White);
                            }
                        }
                    }


                }
            }
            // TODO: måla ut massa grejer
            _spriteBatch.End();
            base.Draw(gameTime);
        }
        private Cell[,] loadCells(Cell[,] cells, int numGrenades, int numCoins)
        {
            for (int i = 0; i < cells.GetLength(0); i++) {
                for (int j = 0; j < cells.GetLength(1); j++) {
                    //om i och j är border på brädet blir det cell
                    if (i == 0 || i == cells.GetLength(0)-1 || j == 0 || j == cells.GetLength(1)-1) {
                        cells[i, j] = new NormalCell(this, new Vector2(0, 0), "cell", "cell");
                    }
                    else {
                        if (numGrenades > 0) {
                            cells[i, j] = new Grenade(this, new Vector2(0, 0), "Bomb", "cell");
                            numGrenades--;
                        }
                        else if (numCoins > 0) {
                            cells[i, j] = new Coin(this, new Vector2(0, 0), "cell", "cell");
                            numCoins--;
                        }
                        else {
                            cells[i, j] = new NormalCell(this, new Vector2(0, 0), "cell", "cell");
                        }
                    }
                    cells[i, j].Position = new Vector2(i*CELL_SIZE, j*CELL_SIZE);
                }
            }
            return cells;
        }
        private Cell[,] shuffle(Cell[,] cells)
        {
            Cell temp;
            Vector2 tempPosition;

            for (int i = 1; i < cells.GetLength(0)-1; i++) {
                for (int j = 1; j < cells.GetLength(1)-1; j++) {
                    int x = rand.Next(1, cells.GetLength(0)-1);
                    int y = rand.Next(1, cells.GetLength(1)-1);
                    tempPosition = cells[i, j].Position;
                    cells[i, j].Position = cells[x, y].Position;
                    cells[x, y].Position = tempPosition;
                    temp = cells[i, j];
                    cells[i, j] = cells[x, y];
                    cells[x, y] = temp;
                }
            }
            return cells;
        }
        private Cell[,] countNearbyBombs(Cell[,] cells)
        {
            for (int i = 1; i < cells.GetLength(0)-1; i++) {
                for (int j = 1; j < cells.GetLength(1)-1; j++) {
                    int numBombs = 0;
                    if (cells[i, j] is NormalCell) {
                        if (cells[i-1, j] is Grenade) {
                            numBombs++;
                        }
                        if (cells[i-1, j-1] is Grenade) {
                            numBombs++;
                        }
                        if (cells[i-1, j+1] is Grenade) {
                            numBombs++;
                        }
                        if (cells[i, j+1] is Grenade) {
                            numBombs++;
                        }
                        if (cells[i, j-1] is Grenade) {
                            numBombs++;
                        }
                        if (cells[i+1, j-1] is Grenade) {
                            numBombs++;
                        }
                        if (cells[i+1, j] is Grenade) {
                            numBombs++;
                        }
                        if (cells[i+1, j+1] is Grenade) {
                            numBombs++;
                        }
                        if (numBombs != 0) {
                            ((NormalCell)cells[i, j]).NearbyGrenades = numBombs;
                            ((NormalCell)cells[i, j]).NumberTexture = this.Content.Load<Texture2D>(numBombs.ToString());
                        }
                    }
                }
            }
            return cells;
        }
        private Cell[,] distanceToCoin(Cell[,] cells)
        {
            //De 2 första for looparna letar efter ett coin
            for (int i = 1; i < cells.GetLength(0)-1; i++) {
                for (int j = 1; j < cells.GetLength(1)-1; j++) {
                    if (cells[i, j] is Coin) {
                        //De 2 andra for looparna går igenom de celler som ska bli färglagda rund coinet
                        for (int x = Math.Max(1, i-3); x < Math.Min(cells.GetLength(0)-1, i+4); x++) {
                            for (int y = Math.Max(1, j-3); y < Math.Min(cells.GetLength(1)-1, j+4); y++) {
                                if (cells[x, y] is NormalCell && !(cells[x, y] is Coin)) {

                                    int coinDistance = ((NormalCell)cells[x, y]).coinDistance(i, j, CELL_SIZE);
                                    if (coinDistance == 1) {
                                        ((NormalCell)cells[x, y]).Color = new Color(71, 165, 38, 255);
                                    }
                                    else if(coinDistance == 2) {
                                        ((NormalCell)cells[x, y]).Color = new Color(78, 197, 48, 255);
                                    }
                                    else if (coinDistance == 3) {
                                        ((NormalCell)cells[x, y]).Color = new Color(157, 230, 133, 255);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return cells;
        }
    }
}
