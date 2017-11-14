using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    class Card
    {
        public CARD_COLOR color;
        public CARD_NAME name;

        public Card(CARD_COLOR colors, CARD_NAME names)
        {
            color = colors;
            name = names;
        }
    }
}
