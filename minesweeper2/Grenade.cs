using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace minesweeper2 {
    class Grenade : Cell {
        public Grenade(Game game, Vector2 position, string cellTextureName, string coverTextureName, string flagTextureName) 
                    : base(game, position, cellTextureName, coverTextureName, flagTextureName)
        {
        }
        /*TODO: grenade click
         * protected override bool click(){
         * planen var att animera något
         * }
         */
    }
}
