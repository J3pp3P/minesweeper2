using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace minesweeper2 {
    class NormalCell : Cell {
        protected int _nearbyGrenades;
        protected Color _color;
        private Texture2D _numberTexture;
        private int _coinDistance;


        public NormalCell(Game game, Vector2 position, string textureName, string coverTextureName, string flagTextureName)
                        : base(game, position, textureName, coverTextureName, flagTextureName)
        {
            _color = Color.White;
            _coinDistance = 0;
            _nearbyGrenades = 0;
        }

        //Metod för att beräkna avståndet till alla normalCells
        public int coinDistance(int coinX, int coinY, int cellSize)
        {
            int tileX = (int)_position.X / cellSize;
            int tileY = (int)_position.Y / cellSize;
            //närmast coinet
            if ((coinX-tileX == 1 || coinX-tileX == 0 || coinX-tileX == -1) && 
                (coinY-tileY == 1 || coinY-tileY == 0 || coinY-tileY == -1)) {
                _coinDistance = 1;
                return 1;
            }
            //2 steg från coinet
            else if ((((coinX-tileX == 2 || coinX-tileX ==-2) && coinY-tileY != 3 && coinY-tileY != -3) || 
                    (coinY-tileY != 3 && coinY-tileY != -3 && coinX-tileX != 3 && coinX-tileX != -3)) && _coinDistance != 1) {
                _coinDistance = 2;
                return 2;
            }
            //3 steg från coinet
            else if (_coinDistance != 1 && _coinDistance != 2) {
                _coinDistance = 3;
                return 3;
            }
            else {
                return _coinDistance;
            }
        }

        public int NearbyGrenades { get => _nearbyGrenades; set => _nearbyGrenades=value; }
        public Texture2D NumberTexture { get => _numberTexture; set => _numberTexture=value; }
        public Color Color { get => _color; set => _color=value; }
        public int CoinDistance { get => _coinDistance; set => _coinDistance=value; }
    }
}
