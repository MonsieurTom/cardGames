using System;
using System.Collections.Generic;
using System.Text;

namespace ClientCard
{
    class CoincheDecodeMessage
    {
        private String[] _cardName = {"ace", "seven", "height", "nine", "ten", "jack", "queen", "king"};
        private String[] _trumps = {"none", "spade", "hearth", "diamond", "club", "all"};
        private String[] _cardColor = {"spade", "hearth", "diamond", "club"};
        private static CoincheDecodeMessage instance;

        private CoincheDecodeMessage()
        { }

        public static CoincheDecodeMessage getInstance()
        {
            if (instance == null)
                instance = new CoincheDecodeMessage();
            return (instance);
        }

        public void decodeMessage(ServerMsg message, Action runnable)
        {
            switch (message.TypeMessage)
            {
                case ServerMsg.Types.TypeMessage.Drawcard:
                    Console.WriteLine("You just draw a card :");
                    Console.WriteLine(message.DrawCard.Name.ToString() + " of " + message.DrawCard.Color.ToString());
                    Hand.getInstance().addCard(message.DrawCard.Name.ToString(), message.DrawCard.Color.ToString());
                    break;
                case ServerMsg.Types.TypeMessage.Winner:
                    Console.WriteLine("---   END   ---");
                    Console.WriteLine("Team : " + message.Winner.DealerTeam + " has taken the deal.");
                    if (message.Winner.WinDeal)
                        Console.WriteLine("They did manage to complete the deal.");
                else
                    Console.WriteLine("They didn't manage to complete the deal.");
                    Console.WriteLine("Stats : ");
                    Console.WriteLine("Team 1 : " + message.Winner.Team1Score + " pts.");
                    Console.WriteLine("Team 2 : " + message.Winner.Team2Score + " pts.");
                    Environment.Exit(0);
                    break;
                case ServerMsg.Types.TypeMessage.Dealwinner:
                    Console.WriteLine("Player : " + message.DealWinner.Player + " from team : " + message.DealWinner.Team
                            + " Won the deal with : " + message.DealWinner.Deal + " and trump : " + message.DealWinner.Trump + ".");
                    break;
                case ServerMsg.Types.TypeMessage.Stackwinner:
                    Console.WriteLine("Player : " + message.StackWinner.Player + " from team : " + message.StackWinner.Team
                    + " Won the stack.");
                    Console.WriteLine("He won " + +message.StackWinner.Score + " points.");
                    break;
                case ServerMsg.Types.TypeMessage.Playedcard:
                    Console.WriteLine("Player : " + message.PlayedCard.Player + " from team : " + message.PlayedCard.Team
                            + " played a " + message.PlayedCard.Name + " of " + message.PlayedCard.Color);
                    break;
                case ServerMsg.Types.TypeMessage.Playerdealed:
                    if (message.PlayerDealed.Deal == -1)
                        Console.WriteLine("Player : " + message.PlayerDealed.Player + " from team : " + message.PlayerDealed.Team
                        + " did pass the deal.");
                else
                    Console.WriteLine("Player : " + message.PlayerDealed.Player + " from team : " + message.PlayerDealed.Team
                    + " has deal " + message.PlayerDealed.Deal + " points on " + message.PlayerDealed.Trump);
                    break;
                case ServerMsg.Types.TypeMessage.Actionvalidation:
                    if (message.ActionValidation.Accepted)
                    {
                        Hand.getInstance().deletePlayedCard();
                        Console.WriteLine("Action accepted.");
                    }
                    else if (!message.ActionValidation.Accepted)
                        Console.WriteLine("Action refused.");
                    break;
                case ServerMsg.Types.TypeMessage.Playerstate:
                    switch (message.PlayerState.State)
                    {
                        case PlayerState.Types.State.Playing:
                            Console.WriteLine("You just entered a game's table.");
                            Console.WriteLine("Your ID is : " + message.PlayerState.Player + ".");
                            Console.WriteLine("Your team is : " + message.PlayerState.Team + ".");
                            break;
                        case PlayerState.Types.State.Yourturn:
                            Console.WriteLine("It is your time to play.");
                            break;
                        case PlayerState.Types.State.Waitinggame:
                            Console.WriteLine("No game's table are available.");
                            Console.WriteLine("Entering the queue to find other to play with and form a game's table.");
                            break;
                        case PlayerState.Types.State.Leave:
                            Console.WriteLine("Someone has leave the game.");
                            Console.WriteLine("everybody on this game's table is being kick.");
                            Environment.Exit(0);
                            break;
                        case PlayerState.Types.State.Deal:
                            Console.WriteLine("It's your time to deal.");
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
        }    
    }
}
