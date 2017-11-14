using System;
using System.Collections.Generic;
using System.Text;

namespace ClientCard
{
    class CoincheEncodeMessage
    {
        static CARD_NAME name;
        static CARD_COLOR color;
        public static CoincheEncodeMessage instance = null;
        private String[] _cardName = {"ace", "seven", "height", "nine", "ten", "jack", "queen", "king"};
        private CARD_NAME[]  card_names = { CARD_NAME.Ace, CARD_NAME.Seven, CARD_NAME.Height, CARD_NAME.Nine, CARD_NAME.Ten, CARD_NAME.Jack, CARD_NAME.Queen, CARD_NAME.King};
        private String[] _cardColor = {"spade", "hearth", "diamond", "club"};
        private CARD_COLOR[] card_colors = {CARD_COLOR.Spade, CARD_COLOR.Hearth, CARD_COLOR.Diamond, CARD_COLOR.Club};
        private String[] _trumps = {"none", "spade", "hearth", "diamond", "club", "all"};
        private TRUMP[] _trumpsCard = {TRUMP.Tnone, TRUMP.Tspade, TRUMP.Thearth, TRUMP.Tdiamond, TRUMP.Tclub, TRUMP.Tall};
        private Action _runnable = null;
        private String[] _wordTab;

        private CoincheEncodeMessage(Action runnable)
        {
            _runnable = runnable;
        }

        public static CoincheEncodeMessage getInstance(Action runnable)
        {
            if (instance == null)
                instance = new CoincheEncodeMessage(runnable);
            return (instance);
        }

        public PlayerMsg encode(String message)
        {
            switch (message.ToLower())
            {
                case "quit":
                    _runnable.Invoke();
                    break;
                case "help":
                    Console.WriteLine("---   Help   ---");
                    Console.WriteLine("- differents helps menues exists -");
                    Console.WriteLine("- Commands are not case sensitives -");
                    Console.WriteLine("help commands");
                    Console.WriteLine("help CARD_COLOR");
                    Console.WriteLine("help CARD_NAME");
                    Console.WriteLine("help TRUMPS");
                    break;
                case "help commands":
                    Console.WriteLine("---   Available commands   ---");
                    Console.WriteLine("PLAYCARD $(CARD_NAME) $(CARD_COLOR)");
                    Console.WriteLine("DEAL $(your deal) $(TRUMP)  ----- Your deal can be \"PASS\"");
                    break;
                case "help card_color":
                    Console.WriteLine("---   Differents card's colors   ---");
                    Console.WriteLine("- SPADE");
                    Console.WriteLine("- HEARTH");
                    Console.WriteLine("- DIAMOND");
                    Console.WriteLine("- CLUB");
                    break;
                case "help card_name":
                    Console.WriteLine("---  Differents card's names   ---");
                    Console.WriteLine("- ACE");
                    Console.WriteLine("- KING");
                    Console.WriteLine("- QUEEN");
                    Console.WriteLine("- JACK");
                    Console.WriteLine("- TEN");
                    Console.WriteLine("- NINE");
                    Console.WriteLine("- SEVEN");
                    break;
                case "help trumps":
                    Console.WriteLine("---   Differents Trumps   ---");
                    Console.WriteLine("- SPADE");
                    Console.WriteLine("- HEARTH");
                    Console.WriteLine("- DIAMOND");
                    Console.WriteLine("- CLUB");
                    Console.WriteLine("- NONE");
                    Console.WriteLine("- ALL");
                    break;
                case "hand":
                    handCommand();
                    break;
                default:
                    return (this.getCoincheProto(message));
            }
            return (null);
        }

        private void handCommand()
        {
            int idx = 0;

            if (Hand.getInstance().getCardName(idx) == null)
                Console.WriteLine("---   You don't own any card   ---");
        else
        {
                Console.WriteLine("---   Card(s) you own   ---");
                while (Hand.getInstance().getCardName(idx) != null)
                {
                    Console.WriteLine("- " + Hand.getInstance().getCardName(idx) + " of "
                                                + Hand.getInstance().getCardColor(idx));
                    idx++;
                }
            }
        }

        private PlayerMsg getCoincheProto(String message)
        {
            this._wordTab = message.Split(' ');
            switch (this._wordTab[0].ToLower())
            {
                case "playcard":
                    return (computePlayCard());
                case "deal":
                    return (computeDeal());
                default:
                    Console.WriteLine("Command Doesn't exist, try the 'help' command");
                    break;
            }
            return (null);
        }

        private PlayerMsg computePlayCard()
        {
            int i = 0;
            int j = 0;

            try
            {
                if (_wordTab.Length != 3)
                {
                    return (null);
                }
                while (i <= 8)
                {
                    if (i == 8)
                    {
                        Console.WriteLine("Invalid command. try the \"help\" command.");
                        return (null);
                    }
                    if (_wordTab[1].ToLower().CompareTo(_cardName[i]) == 0)
                    {
                        name = (CARD_NAME)card_names[i];
                        break;
                    }
                    i++;
                }
                while (j <= 4)
                {
                    if (j == 4)
                    {
                        Console.WriteLine("Invalid command. try the \"help\" command.");
                        return (null);
                    }
                    if (_wordTab[2].ToLower().CompareTo(_cardColor[j]) == 0)
                    {
                        color = (CARD_COLOR)card_colors[j];
                        if (!Hand.getInstance().playCard(_cardName[i], _cardColor[j]))
                        {
                            Console.WriteLine("You don't own this card.");
                            Console.WriteLine("Enter \"HAND\" command to see which cards you own");
                            return (null);
                        }
                        return (new PlayerMsg
                        {
                            TypeMessage = PlayerMsg.Types.TypeMessage.Playcard,
                            PlayCard = new PlayCard
                            {
                                Color = color,
                                Name = name
                            }
                        });
                    }
                    j++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong, try again.");
            }
            return (null);
        }

        private PlayerMsg computeDeal()
        {
            try
            {
                if (_wordTab.Length == 2)
                {
                    if (_wordTab[1].ToLower().CompareTo("pass") == 0)
                    {
                        return (new PlayerMsg
                        {
                            TypeMessage = PlayerMsg.Types.TypeMessage.Deal,
                            Deal = new Deal
                            {
                                Deal_ = -1,
                                Trump = TRUMP.Tnone
                            }
                        });
                    }
                    else
                    {
                        Console.WriteLine("Invalid command. try the \"help\" command.");
                        return (null);
                    }
                }
                else
                {
                    if (_wordTab.Length != 3)
                    {
                        Console.WriteLine("Invalid command. try the \"help\" command.");
                        return (null);
                    }
                    for (int i = 0; i <= 6; i++)
                    {
                        if (i == 6)
                        {
                            Console.WriteLine("Invalid command. try the \"help\" command.");
                            return (null);
                        }
                        if (_wordTab[2].ToLower().CompareTo(_trumps[i]) == 0)
                        {
                            return (new PlayerMsg
                            {
                                TypeMessage = PlayerMsg.Types.TypeMessage.Deal,
                                Deal = new Deal
                                {
                                    Deal_ = int.Parse(_wordTab[1]),
                                    Trump = _trumpsCard[i]
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Something went wrong, maybe you didn't enter an integer as first parameter.");
                return (null);
            }
            return (null);
        }
    }
}
