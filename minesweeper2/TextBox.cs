using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace minesweeper2 {
    class TextBox {
        //allt detta är kopierat
        //på grund av brist på tid och inte högt prioriterat
        public string CurrentText { get; set; }
        public Vector2 CurrentTextPosition { get; set; }
        public Vector2 CursorPosition { get; set; }
        public int AnimationTime { get; set; }
        public bool Visible { get; set; }
        public float LayerDepth { get; set; }
        public Vector2 Position { get; set; }
        public bool Selected { get; set; }
        public int CellWidth { get; set; }
        public int CellHeight { get; set; }
        private int _cursorHeight;
        private int _cursorWidth;
        private int _length;
        private bool _numericOnly;
        private Texture2D _texture;
        private Texture2D _cursorTexture;
        private Point _cursorDimensions;
        private SpriteFont _font;

        public TextBox(Game game, String textureName, String cursorTextureName, Point dimensions, Point cursorDimensions, Vector2 position, int length, bool numericOnly, bool visible, SpriteFont font, string text, float layerDepth)
        {
            _texture = game.Content.Load<Texture2D>(textureName); ;
            CellWidth = dimensions.X;
            CellHeight = dimensions.Y;
            _cursorWidth = cursorDimensions.X;
            _cursorHeight = cursorDimensions.Y;
            _length = length;
            _numericOnly = numericOnly;
            AnimationTime = 0;
            Visible = Visible;
            LayerDepth = layerDepth;
            Position = position;
            CursorPosition = new Vector2(position.X + 7, position.Y + 6);
            CurrentTextPosition = new Vector2(position.X + 7, position.Y + 3);
            CurrentText = String.Empty;
            _cursorTexture = game.Content.Load<Texture2D>(cursorTextureName);
            _cursorDimensions = cursorDimensions;
            Selected = false;
            _font = font;
            CurrentText = text;
        }
        
        public void Update()
        {
            AnimationTime++;
        }

        public bool isFlashingCursorVisible()
        {
            int time = AnimationTime % 60;
            if (time >= 0 && time < 31) {
                return true;
            }
            else {
                return false;
            }
        }

        public void AddMoreText(char text)
        {
            Vector2 spacing = new Vector2();
            KeyboardState keyboardState = KeyboardElite.GetState();
            bool lowerThisCharacter = true;

            if (_numericOnly && (int)Char.GetNumericValue(text) < 0 || (int)Char.GetNumericValue(text) > 9) {
                if (text != '\b') {
                    return;
                }
            }
            if (text != '\b') {
                if (CurrentText.Length < _length) {
                    if (lowerThisCharacter) {
                        text = Char.ToLower(text);
                    }

                    CurrentText += text;
                    spacing = _font.MeasureString(text.ToString());
                    CursorPosition = new Vector2(CursorPosition.X + spacing.X, CursorPosition.Y);
                }
            }
            else {
                if (CurrentText.Length > 0) {
                    spacing = _font.MeasureString(CurrentText.Substring(CurrentText.Length - 1));
                    CurrentText = CurrentText.Remove(CurrentText.Length - 1, 1);
                    CursorPosition = new Vector2(CursorPosition.X - spacing.X, CursorPosition.Y);
                }
            }
        }

        public void Render (SpriteBatch spriteBatch)
        {
            if (Visible) {
                spriteBatch.Draw(_texture, Position, Color.White);
                spriteBatch.DrawString(_font, CurrentText, CurrentTextPosition, Color.White, 0f, Vector2.Zero, 1.0f, SpriteEffects.None, LayerDepth);

                if (Selected && isFlashingCursorVisible()) {
                    Rectangle sourceRectangle = new Rectangle(0, 0, _cursorWidth, _cursorHeight);
                    Rectangle destinationRectangle = new Rectangle((int)CursorPosition.X, (int)CursorPosition.Y, _cursorWidth, _cursorHeight);

                    spriteBatch.Draw(_cursorTexture, destinationRectangle, sourceRectangle, Color.White, 0f, Vector2.Zero, SpriteEffects.None, LayerDepth);
                }
            }
        }
    }
}
