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
        private SpriteFont _timeFont;
        private SpriteFont _coinsLeft;
        private SpriteFont _bombsLeft;
        private SpriteFont _restart;
        private SpriteFont _highscore;
        private Texture2D _joakimVonAnka;
        private Texture2D _bigBomb;
        private Vector2 _bigBombPosition;
        private Vector2 _joakimVonAnkaPosition;
        private Vector2 _highscorePosition;
        private Vector2 _restartPosition;
        private Rectangle _restartRectangle;
        private Rectangle _highscoreRectangle;
        private int _screenWidth, _screenHeight, _screenCenterY, _screenCenterX;
        private bool _firstClick = true;
        private bool _gameOver = false;
        private bool _victory = false;
        private bool _running = true;
        private int _foundCoins;
        private int _foundBombs;
        Random rand = new Random();
        Stopwatch _gameTimer = new Stopwatch();
        string _time = "0";

        private const int GAME_WIDTH = 1400;
        private const int GAME_HEIGHT = 900;
        private const int CELL_SIZE = 48;
        private const int NUM_GRENADES = 40;
        private const int NUM_COINS = 15;
        private const int BOARD_SIZE_WIDTH = 16;
        private const int BOARD_SIZE_HEIGHT = 16;
        private MouseState previousMS;
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
            _joakimVonAnka = Content.Load<Texture2D>("joakim_von_anka");
            _bigBomb = Content.Load<Texture2D>("bigBomb");
            _timeFont = Content.Load<SpriteFont>("time");
            _coinsLeft = Content.Load<SpriteFont>("coinsLeft");
            _bombsLeft = Content.Load<SpriteFont>("bombsLeft");
            _highscore = Content.Load<SpriteFont>("highscore");
            _restart = Content.Load<SpriteFont>("restart");

            _bigBombPosition = new Vector2(300, 300);
            _joakimVonAnkaPosition = new Vector2(300, 300);
            _highscorePosition = new Vector2(_screenWidth-300, 400);
            _restartPosition = new Vector2(_screenWidth-300, 300);
            _restartRectangle = new Rectangle(_screenWidth-300, 300, (int)_restart.MeasureString("Restart").X, (int)_restart.MeasureString("Restart").Y);
            _highscoreRectangle = new Rectangle(_screenWidth-300, 300, (int)_restart.MeasureString("Highscore").X, (int)_restart.MeasureString("Highscore").Y);

            //load cells
            loadCells(cells, NUM_GRENADES, NUM_COINS);
            
        }

        protected override void Update(GameTime gameTime)
        { 
            KeyboardElite.GetState();
            MouseState ms = Mouse.GetState(); 

            /* TODO: click()
             * flag()
             * revealMultipleCells()
             * TODO: om det är en bomb, lägg till eventull animation och att spelaren förlorar
             * TODO: om det är ett coin, lägg till eentuell animation och att spelaren vinner om alla coins är hittade
             * TODO: tidtagning
             * TODO: skicka tiden Till servern i formatet string
             * TODO: håll ordning på antalet bomber och coins kvar
             */
            //tid logik
            _time = _gameTimer.ElapsedMilliseconds.ToString();
            if (_time.Length > 3) {
                _time = _time.Substring(0, _time.Length-3);
            }
            else {
                _time = "0";
            }

            //reset
            if (_restartRectangle.Contains(ms.Position) && ms.LeftButton == ButtonState.Released && previousMS.LeftButton == ButtonState.Pressed) {
                _running = true;
                _firstClick = true;
                _victory = false;
                _gameOver = false;
                _gameTimer.Restart();
                _gameTimer.Stop();
                _foundCoins = 0;
                _foundBombs = 0;
                cells = new Cell[BOARD_SIZE_HEIGHT + 2, BOARD_SIZE_WIDTH + 2];
                loadCells(cells, NUM_GRENADES, NUM_COINS);
            }

            if (_running) {
                for (int i = 1; i < cells.GetLength(0)-1; i++) {
                    for (int j = 1; j < cells.GetLength(1)-1; j++) {
                        if (cells[i, j].getRetangle().Contains(ms.Position)) {
                            if (_firstClick && ms.LeftButton == ButtonState.Released && previousMS.LeftButton == ButtonState.Pressed) {
                                _firstClick = false;
                                cells = shuffle(cells, i, j);
                                cells = distanceToCoin(cells);
                                cells = countNearbyBombs(cells);
                                _gameTimer.Start();
                                revealCells(i, j);
                            }
                            else {
                                if (ms.LeftButton == ButtonState.Released && previousMS.LeftButton == ButtonState.Pressed) {
                                    revealCells(i, j);
                                }
                                if (ms.RightButton == ButtonState.Released && previousMS.RightButton == ButtonState.Pressed && !_firstClick) {
                                    if(cells[i, j].flag()) {
                                        if (cells[i, j].Flagged) {
                                            _foundBombs++;
                                        }
                                        else {
                                            _foundBombs--;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            previousMS = ms;
            base.Update(gameTime);
        }



        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            _spriteBatch.Begin();
            _spriteBatch.DrawString(_timeFont, "Time: " + _time, new Vector2(_screenWidth -300, 50), Color.Black);
            _spriteBatch.DrawString(_coinsLeft, "Coins: " + _foundCoins + " / " + NUM_COINS, new Vector2(_screenWidth -300, 100), Color.Black);
            _spriteBatch.DrawString(_bombsLeft, "Bombs: " + _foundBombs + " / " + NUM_GRENADES, new Vector2(_screenWidth -300, 150), Color.Black);
            _spriteBatch.DrawString(_restart, "Restart", _restartPosition, Color.Black);
            _spriteBatch.DrawString(_highscore, "Highscore", _highscorePosition, Color.Black);


            for (int i = 1; i < cells.GetLength(0)-1; i++) {
                for (int j = 1; j < cells.GetLength(1)-1; j++) {
                    if (cells[i, j] is Coin) {
                        //Coin
                        if (!cells[i, j].IsClicked) {
                            _spriteBatch.Draw(cells[i, j].CoverTexture, cells[i, j].getRetangle(), cells[i, j].CoverColor);
                            if (cells[i, j].Flagged) {
                                _spriteBatch.Draw(cells[i, j].FlagTexture, cells[i, j].getRetangle(), Color.White);
                            }
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
                            _spriteBatch.Draw(cells[i, j].CoverTexture, cells[i, j].getRetangle(), cells[i, j].CoverColor);
                            if (cells[i, j].Flagged) {
                                _spriteBatch.Draw(cells[i, j].FlagTexture, cells[i, j].getRetangle(), Color.White);
                            }
                        }
                        else {
                            _spriteBatch.Draw(cells[i, j].Texture, cells[i, j].getRetangle(), Color.White);
                        }
                    }
                    //NormalCell
                    else {
                        if (!cells[i, j].IsClicked) {
                            _spriteBatch.Draw(cells[i, j].CoverTexture, cells[i, j].getRetangle(), cells[i, j].CoverColor);
                            if (cells[i, j].Flagged) {
                                _spriteBatch.Draw(cells[i, j].FlagTexture, cells[i, j].getRetangle(), Color.White);
                            }
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
            if (_victory) {
                _spriteBatch.Draw(_joakimVonAnka, _joakimVonAnkaPosition, Color.White);
            }
            if (_gameOver) {
                _spriteBatch.Draw(_bigBomb, _bigBombPosition, Color.White);
            }
            // TODO: måla ut massa grejer
            _spriteBatch.End();
            base.Draw(gameTime);
        }
        private void revealCells(int i, int j)
        {
            //om cellen man trycker på inte är tryck
            if (!cells[i, j].IsClicked && !cells[i, j].Flagged) {
                cells[i, j].click();
                if (cells[i, j] is Coin) {
                    _foundCoins++;
                    if (_foundCoins == NUM_COINS) {
                        _running = false;
                        _victory = true;
                        _gameTimer.Stop();
                        return;
                    }
                }else if(cells[i, j] is Grenade) {
                    _running = false;
                    _gameOver = true;
                    _gameTimer.Stop();
                    return;
                }
                
                if (cells[i, j] is NormalCell && ((NormalCell)cells[i, j]).NearbyGrenades == 0) {
                    for (int x = Math.Max(1, i-1); x <= Math.Min(cells.GetLength(0)-2, i+1); x++) {
                        for (int y = Math.Max(1, j-1); y <= Math.Min(cells.GetLength(0)-2, j+1); y++) {
                            if (!cells[x, y].IsClicked && !cells[x, y].Flagged) {
                                revealCells(x, y);
                            }
                        }
                    }
                }
            }
            else {
                //om cellen man trycker på är tryck går man igenom antalet flaggor runt är det samma som 
                //siffran i cellen trycks alla celler runt den tryckta cellen
                int nearbyFlaggedCells = 0;
                if (cells[i, j] is NormalCell) {
                    for (int x = Math.Max(1, i-1); x <= Math.Min(cells.GetLength(0)-2, i+1); x++) {
                        for (int y = Math.Max(1, j-1); y <= Math.Min(cells.GetLength(0)-2, j+1); y++) {
                            if (cells[x, y].Flagged) {
                                nearbyFlaggedCells++;
                            }
                        }
                    }
                    if (nearbyFlaggedCells == ((NormalCell)cells[i, j]).NearbyGrenades) {
                        for (int x = Math.Max(1, i-1); x <= Math.Min(cells.GetLength(0)-2, i+1); x++) {
                            for (int y = Math.Max(1, j-1); y <= Math.Min(cells.GetLength(0)-2, j+1); y++) {
                                if (!cells[x, y].IsClicked) {
                                    revealCells(x, y);
                                }
                            }
                        }
                    }
                }
            }
        }
        private Cell[,] loadCells(Cell[,] cells, int numGrenades, int numCoins)
        {
            for (int i = 0; i < cells.GetLength(0); i++) {
                for (int j = 0; j < cells.GetLength(1); j++) {
                    //om i och j är border på brädet blir det cell
                    if (i == 0 || i == cells.GetLength(0)-1 || j == 0 || j == cells.GetLength(1)-1) {
                        cells[i, j] = new NormalCell(this, new Vector2(0, 0), "cell", "cell", "flag");
                    }
                    else {
                        if (numGrenades > 0) {
                            cells[i, j] = new Grenade(this, new Vector2(0, 0), "Bomb", "cell", "flag");
                            numGrenades--;
                        }
                        else if (numCoins > 0) {
                            cells[i, j] = new Coin(this, new Vector2(0, 0), "cell", "cell", "flag");
                            numCoins--;
                        }
                        else {
                            cells[i, j] = new NormalCell(this, new Vector2(0, 0), "cell", "cell", "flag");
                        }
                    }
                    cells[i, j].Position = new Vector2(i*CELL_SIZE, j*CELL_SIZE);
                }
            }
            return cells;
        }
        private Cell[,] shuffle(Cell[,] cells, int firstClickX, int firstClickY)
        {
            bool reroll = true;
            for (int i = 1; i < cells.GetLength(0)-1; i++) {
                for (int j = 1; j < cells.GetLength(1)-1; j++) {
                    reroll = true;
                    while (reroll) {
                        int x = rand.Next(1, cells.GetLength(0)-1);
                        int y = rand.Next(1, cells.GetLength(1)-1);
                        /*om forloopens [i, j] cell är en bomb får den inte flyttas in i 3x3 rutan runt första tryckets position
                         * men bomben måste flyttas ut ur 3x3 rutan om i och j är innanför 3x3 rutan, 
                         * då måste man se till att den byts mot en cell som inte är en bomb
                         */
                        if (cells[i, j] is Grenade) {
                            if (i >= firstClickX-1 && i <= firstClickX+1 && j <= firstClickY+1 && j >= firstClickY-1) {
                                if (x >= firstClickX-1 && x <= firstClickX+1 && y <= firstClickY+1 && y >= firstClickY-1) {
                                    reroll = true;
                                }
                                else if (cells[x, y] is Grenade) {
                                    reroll = true;
                                }
                                else {
                                    reroll = false;
                                    swap(i, j, x, y);
                                }
                            }
                            else {
                                if (x >= firstClickX-1 && x <= firstClickX+1 && y <= firstClickY+1 && y >= firstClickY-1) {
                                    reroll = true;
                                }
                                else {
                                    reroll = false;
                                    swap(i, j, x, y);
                                }
                            }
                        }
                        else {
                            if (i >= firstClickX-1 && i <= firstClickX+1 && j <= firstClickY+1 && j >= firstClickY-1) {
                                if (cells[x, y] is Grenade) {
                                    reroll = true;
                                }
                                else {
                                    reroll = false;
                                    swap(i, j, x, y);
                                }
                            }
                            else {
                                reroll = false;
                                swap(i, j, x, y);
                            }
                        }
                    }
                }
            }
            return cells;
        }
        //byter plats på cell1 och cell2
        private void swap(int cell1X, int cell1Y, int cell2X, int cell2Y)
        {
            Cell temp;
            Vector2 tempPosition;
            tempPosition = cells[cell1X, cell1Y].Position;
            cells[cell1X, cell1Y].Position = cells[cell2X, cell2Y].Position;
            cells[cell2X, cell2Y].Position = tempPosition;
            temp = cells[cell1X, cell1Y];
            cells[cell1X, cell1Y] = cells[cell2X, cell2Y];
            cells[cell2X, cell2Y] = temp;
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
