﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace minesweeper2 {
    class Grenade : Cell {
        public Grenade(Game game, Vector2 position, string textureName, string coverTextureName, string flagTextureName) 
                    : base(game, position, textureName, coverTextureName, flagTextureName)
        {
        }
        /*TODO: grenade click
         * protected override bool click()
         */
    }
}
