using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace minesweeper2 {

    class Coin : NormalCell {
        public Coin(Game game, Vector2 position, string textureName, string coverTextureName, string flagTextureName)
                : base(game, position, textureName, coverTextureName, flagTextureName)
        {
            _color = Color.Gold;
        }
        /*TODO: Coin klass
         * fel i planering ska vara int
         * 
         * returnerar en double
         */
        
         /*  
         * protected override bool click(){
         * }
         */

    }
}
