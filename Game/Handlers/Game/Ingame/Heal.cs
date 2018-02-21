namespace Game.Handlers.Game.Ingame
{
    class Heal : Networking.GameDataHandler
    {
        protected override void Handle()
        {
            if (Room.State == Enums.RoomState.Playing)
            {
                byte targetSlot = GetByte(2); // The target 
                bool heal = GetBool(5); // If user needed to be healed or killed
                bool isBoxStation = false; // If it's an medic station or just normal healing
                ushort healing = 0;

                //if (heal)
                {
                    if (targetSlot >= 0 && targetSlot <= Room.MaximumPlayers)
                    {
                        Entities.Player p = null;
                        try
                        {
                            Room.Players.TryGetValue(targetSlot, out p);
                        }
                        catch { p = null; }


                        if (p != null)
                        {

                            if (isBoxStation)
                            {
                                healing += 500;
                            }
                            else
                            {
                                switch (Player.Weapon)
                                {
                                    case 77:
                                        {
                                            if (p.Health < 1000)
                                                healing += 300;

                                            break;
                                        }
                                    case 82:
                                        {
                                            if (p.Health < 300)
                                                healing = 300;
                                            else
                                                healing += 100;

                                            break;
                                        }

                                }
                            }

                            p.Health += healing;

                            if (p.Health > 1000)
                                p.Health = 1000;

                            respond = true;
                            Set(3, Player.Health);
                        }
                    }
                }
            }
        }
    }
}
