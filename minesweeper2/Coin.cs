using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace minesweeper2 {

    class Coin : NormalCell {
        private Texture2D _coinTexture;
        private bool _firstClick;
        private float _rotation;
        private float _speed;
        private float _halfWidth;
        private float _halfHeight;

        public Coin(Game game, Vector2 position, string cellTextureName, string coverTextureName, string flagTextureName ,string coinTextureName)
                : base(game, position, cellTextureName, coverTextureName, flagTextureName)
        {
            _cellColor = Color.Gold;
            _coinTexture = game.Content.Load<Texture2D>(coinTextureName);
            _halfWidth = _coinTexture.Width / 2;
            _halfHeight = _coinTexture.Height / 2;
            _rotation = 0;
            
            _firstClick =true;
            _speed = 0f;
        }

        public override bool click()
        {
            if (_firstClick) {
                _firstClick = false;
            }
            else {
                _speed += 0.05f;
                
            }
            return base.click();
        }

        

        
        public Vector2 getCenter()
        {
            return new Vector2(_halfWidth, _halfHeight);
        }
        public Rectangle getRectangleFace()
        {
            return new Rectangle((int)_position.X + _coinTexture.Width/2, (int)_position.Y + _coinTexture.Height/2, _coinTexture.Width, _coinTexture.Height);
        }

        public void Update()
        {
            _rotation += _speed;
            _speed *= 0.99f;
        }

        public Texture2D CoinTexture { get => _coinTexture; set => _coinTexture=value; }
        public float Rotation { get => _rotation; set => _rotation=value; }

        /*  
        * 
        * planen var att animera ett runt coin som rör sig lite när man trycker på den
        * istället för att endast använda färgen guld 
        * }
        */

    }
}
