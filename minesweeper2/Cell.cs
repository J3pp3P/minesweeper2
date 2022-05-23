using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace minesweeper2 {
    class Cell {
        protected Texture2D _texture;
        private Texture2D _coverTexture;
        protected Vector2 _position;
        private bool _isClicked;
        private bool _flagged;

        public Cell(Game game, Vector2 position, string textureName, string coverTextureName)
        {
            _coverTexture = game.Content.Load<Texture2D>(coverTextureName);
            _texture = game.Content.Load<Texture2D>(textureName);
            _position = position;
            _isClicked = false;
            _flagged = false;
        }
        /*TODO: Cell klass
         * Metod för att klicka på en cell, 
         * returnerar true om _isClicked == false eller _flagged == false
         * returnerar false om _isClicked == true eller _flagged == true
         */
        public virtual bool click()
        {
            if (!_isClicked || !_flagged) {
                _isClicked = true;
                return true;
            }
            else {
                return false;
            }
        }
        /*
         * metod för att flagga en bomb
         * inträffade ett fel ska vara tvärtom
         * returnerar true om _isclicked är false
         * returnerar false om _isclicked är true
         */
        public bool flag()
        {
            if (!_isClicked) {
                return false;
            }
            else {
                _flagged = !_flagged;
                return true;
            }
        }

        //ånej jag råkade lägga till denna metod utan att planera detta
        public Rectangle getRetangle()
        {
            return new Rectangle((int)_position.X, (int)_position.Y, _texture.Width, _texture.Height);
        }
        public Texture2D Texture { get => _texture; set => _texture=value; }
        public Texture2D CoverTexture { get => _coverTexture; set => _coverTexture=value; }
        public Vector2 Position { get => _position; set => _position=value; }
        public bool Flagged { get => _flagged; set => _flagged=value; }
        public bool IsClicked { get => _isClicked; set => _isClicked=value; }
    }
}
