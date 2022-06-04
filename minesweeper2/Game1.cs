using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace minesweeper2 {
    /*
     * TODO: meny
     * TODO: settingsgrej
     */
    public class Game1 : Game {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont _text;
        private Texture2D _joakimVonAnka;
        private Texture2D _bigBomb;
        private Vector2 _bigBombPosition;
        private Vector2 _joakimVonAnkaPosition;
        private Vector2 _highscoreTitlePosition;
        private Vector2 _restartTextPosition;
        private Vector2 _coinTextPosition;
        private Vector2 _bombTextPosition;
        private Vector2 _timeTextPosition;
        private Rectangle _restartRectangle;
        private Rectangle _highscoreRectangle;
        //private TextBox _textBox;
        private int _screenWidth, _screenHeight;
        private bool _firstClick = true;
        private bool _gameOver = false;
        private bool _victory = false;
        private bool _running = true;
        private int _foundCoins;
        private int _foundBombs;
        private string _username = "Greger";
        Random rand = new Random();
        Stopwatch _gameTimer = new Stopwatch();
        string _time = "0";

        private const int GAME_WIDTH = 1500;
        private const int GAME_HEIGHT = 1000;
        private const int CELL_SIZE = 48;
        private const int NUM_GRENADES = 40;
        private const int NUM_COINS = 15;
        private const int BOARD_SIZE_WIDTH = 16;
        private const int BOARD_SIZE_HEIGHT = 16;
        private MouseState previousMS;

        //highscore content
        private Vector2[] _highscorePositions = new Vector2[10];
        private String[] _highscores = new string[0];

        //server stuff
        private TcpClient _clientSocket = new TcpClient();
        private NetworkStream _serverStream = default(NetworkStream);
        private const string IP_ADDRESS = "127.0.0.1";
        private const int PORT_NUMBER = 1234;

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
            //_screenCenterX = _screenWidth / 2;
            //_screenCenterY = _screenHeight / 2;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            requestHighscore();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            //ladda bilder
            _joakimVonAnka = Content.Load<Texture2D>("joakim_von_anka");
            _bigBomb = Content.Load<Texture2D>("bigBomb");
            //ladda text font
            _text = Content.Load<SpriteFont>("text");

            //vectorer och rektanglar
            _bigBombPosition = new Vector2(300, 300);
            _joakimVonAnkaPosition = new Vector2(300, 300);
            _timeTextPosition = new Vector2(_screenWidth-300, 50);
            _coinTextPosition = new Vector2(_screenWidth-300, 100);
            _bombTextPosition = new Vector2(_screenWidth-300, 150);
            _restartTextPosition = new Vector2(_screenWidth-300, 300);
            _highscoreTitlePosition = new Vector2(_screenWidth-300, 400);
            _restartRectangle = new Rectangle(_screenWidth-300, 300, (int)_text.MeasureString("Restart").X, (int)_text.MeasureString("Restart").Y);
            _highscoreRectangle = new Rectangle(_screenWidth-300, 300, (int)_text.MeasureString("Highscore").X, (int)_text.MeasureString("Highscore").Y);

            //ladda cells
            loadCells(cells, NUM_GRENADES, NUM_COINS);

            //highscores
            int scoreHeight = (int)_highscoreTitlePosition.Y + 30;
            for (int i = 0; i < _highscorePositions.Length; i++) {
                _highscorePositions[i] = new Vector2(_screenWidth-300, scoreHeight);
                scoreHeight += 30;
            }
            
        }

        protected override void Update(GameTime gameTime)
        { 
            //tangentbord och mus
            KeyboardElite.GetState();
            MouseState ms = Mouse.GetState(); 

            /*
             * TODO: om det är en bomb, lägg till eventull animation och att spelaren förlorar
             * TODO: om det är ett coin, lägg till eentuell animation och att spelaren vinner om alla coins är hittade
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
                requestHighscore();
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
                //loopar igenom hela listan med celler och kollar om en cell trycks
                for (int i = 1; i < cells.GetLength(0)-1; i++) {
                    for (int j = 1; j < cells.GetLength(1)-1; j++) {
                        if (cells[i, j].getRetangle().Contains(ms.Position)) {
                            //första celltrycket
                            if (_firstClick && ms.LeftButton == ButtonState.Released && previousMS.LeftButton == ButtonState.Pressed) {
                                _firstClick = false;
                                cells = shuffle(cells, i, j);
                                cells = distanceToCoin(cells);
                                cells = countNearbyBombs(cells);
                                _gameTimer.Start();
                                revealCells(i, j);
                            }
                            //celltryck
                            else {
                                if (ms.LeftButton == ButtonState.Released && previousMS.LeftButton == ButtonState.Pressed) {
                                    revealCells(i, j);
                                }
                                //flagga
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
            for (int i = 1; i < cells.GetLength(0)-1; i++) {
                for (int j = 1; j < cells.GetLength(1)-1; j++) {
                    if (cells[i, j] is Coin) {
                        ((Coin)cells[i, j]).Update();
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

            //text
            _spriteBatch.DrawString(_text, "Time: " + _time, _timeTextPosition, Color.Black);
            _spriteBatch.DrawString(_text, "Coins: " + _foundCoins + " / " + NUM_COINS, _coinTextPosition, Color.Black);
            _spriteBatch.DrawString(_text, "Bombs: " + _foundBombs + " / " + NUM_GRENADES, _bombTextPosition, Color.Black);
            _spriteBatch.DrawString(_text, "Restart", _restartTextPosition, Color.Black);
            _spriteBatch.DrawString(_text, "Highscore", _highscoreTitlePosition, Color.Black);
            for (int i = 0; i < Math.Min(_highscores.Length-1, 10); i++) {
                int index = i + 1;
                _spriteBatch.DrawString(_text, index + ": " + _highscores[i], _highscorePositions[i], Color.Black);
            }

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
                            _spriteBatch.Draw(((Coin)cells[i, j]).CoinTexture, ((Coin)cells[i, j]).getRectangleFace(), null, Color.White, ((Coin)cells[i, j]).Rotation, ((Coin)cells[i, j]).getCenter(), SpriteEffects.None, 0f);
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
                            _spriteBatch.Draw(cells[i, j].Texture, cells[i, j].getRetangle(), ((NormalCell)cells[i, j]).CellColor);
                            if (((NormalCell)cells[i, j]).NearbyGrenades != 0) {
                                _spriteBatch.Draw(((NormalCell)cells[i, j]).NumberTexture, cells[i, j].getRetangle(), Color.White);
                            }
                        }
                    }
                }
            }
            //om man vunnit/förlorat
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

        //metod för att trycka på cellerna
        private void revealCells(int i, int j)
        {
            //om cellen man trycker på inte är tryck
            if (cells[i, j].click() && !cells[i, j].Flagged) {
                if (cells[i, j] is Coin) {
                    _foundCoins++;
                    if (_foundCoins == NUM_COINS) {
                        _running = false;
                        _victory = true;
                        submitHighscore();
                        _gameTimer.Stop();
                        return;
                    }
                }else if(cells[i, j] is Grenade) {
                    _running = false;
                    _gameOver = true;
                    _gameTimer.Stop();
                    return;
                }
                
                //om cellen har 0 bomber runt ska de cellerna runt tryckas också
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
            //om cellen man trycker på är tryck går man igenom antalet flaggor runt är det samma som 
            //siffran i cellen trycks alla celler runt den tryckta cellen
            else {
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
                                if (!cells[x, y].IsClicked && !cells[x, y].Flagged) {
                                    revealCells(x, y);
                                }
                            }
                        }
                    }
                }
            }
        }
        //metod för att ladda in celler i början av varje nytt spel
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
                            cells[i, j] = new Coin(this, new Vector2(0, 0), "cell", "cell", "flag", "coin");
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
        //metod för att blanda alla celler
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

        //metod för att beräkna antalet bomber runt varje NormalCell
        private Cell[,] countNearbyBombs(Cell[,] cells)
        {
            for (int i = 1; i < cells.GetLength(0)-1; i++) {
                for (int j = 1; j < cells.GetLength(1)-1; j++) {
                    int numBombs = 0;
                    if (cells[i, j] is NormalCell) {
                        //lopar igenom cellerna runt cells[i, j]
                        for (int x = Math.Max(1, i-1); x < Math.Min(cells.GetLength(0)-1, i+2); x++) {
                            for (int y = Math.Max(1, j-1); y < Math.Min(cells.GetLength(1)-1, j+2); y++) {
                                if (cells[x, y] is Grenade) {
                                    numBombs++;
                                }
                            }
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
                                    //coindistance bestämmer vilken färg cellen ska ha
                                    int coinDistance = ((NormalCell)cells[x, y]).coinDistance(i, j, CELL_SIZE);
                                    if (coinDistance == 1) {
                                        ((NormalCell)cells[x, y]).CellColor = new Color(71, 165, 38, 255);
                                    }
                                    else if(coinDistance == 2) {
                                        ((NormalCell)cells[x, y]).CellColor = new Color(78, 197, 48, 255);
                                    }
                                    else if (coinDistance == 3) {
                                        ((NormalCell)cells[x, y]).CellColor = new Color(157, 230, 133, 255);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return cells;
        }
        //metod för att hämta topplistan
        private void requestHighscore()
        {
            _clientSocket = new TcpClient();
            _clientSocket.Connect(IP_ADDRESS, PORT_NUMBER);
            _serverStream = _clientSocket.GetStream();
            byte[] request = Encoding.ASCII.GetBytes("getHighscores$endl");
            _serverStream.Write(request, 0, request.Length);
            Thread clientThread = new Thread(getHighscore);
            clientThread.Start();
        }
        private void getHighscore()
        {
            _serverStream = _clientSocket.GetStream();
            byte[] highscoreBytes = new byte[_clientSocket.ReceiveBufferSize];
            _serverStream.Read(highscoreBytes, 0, _clientSocket.ReceiveBufferSize);
            string highScoreString = Encoding.ASCII.GetString(highscoreBytes);
            Debug.WriteLine(highScoreString);
            _highscores = highScoreString.Split("\r\n");
        }
        private void submitHighscore()
        {
            _clientSocket = new TcpClient();
            _clientSocket.Connect(IP_ADDRESS, PORT_NUMBER);
            _serverStream = _clientSocket.GetStream();
            byte[] request = Encoding.ASCII.GetBytes(_username + " " + _time + "$endl");
            _serverStream.Write(request, 0, request.Length);
        }
    }
}
