using System.Linq;

namespace Game.Objects
{
    public abstract class GameMode
    {
        /*
         
         * NOTE TO MYSELF: RE-PROGRAMING THIS BULLSHIT CLASS BECAUSE IT'S SHIT
         * SECOND NOTE: THIS IS SHIT
         * THIRD NOTE: DO SOMETHING ABOUT IT
         * Time: 19 APRIL 2014
         
         */
        public byte Id { get; private set; }
        public string Name { get; private set; }
        public Entities.Room Room { get; private set; }
        public bool Initilized { get; protected set; }
        public bool FreezeTick { get; protected set; }

        public GameMode(byte id, string name)
        {
            this.Id = id;
            this.Name = name;
            this.Initilized = false;
            this.FreezeTick = false;
        }

        public virtual void Initilize(Entities.Room room)
        {
            this.Room = room;
        }

        // Core functions //
        public abstract bool IsGoalReached();
        public abstract void Process();

        public virtual byte SpawnSlot()
        {
            return 0;
        }

        // Score Board //
        public abstract byte CurrentRoundTeamA();
        public abstract byte CurrentRoundTeamB();
        public abstract ushort ScoreboardA();
        public abstract ushort ScoreboardB();

        // Player Input //
        public abstract void HandleExplosives(string[] blocks, Entities.Player p);

        public virtual void OnDamage(Networking.GameDataHandler handler)
        {
            bool isPlayer = handler.GetBool(2);
            byte targetId = handler.GetByte(3);

            Entities.Player p = null;

            try
            {
                handler.Room.Players.TryGetValue(targetId, out p);
            }
            catch { p = null; }

            if (p != null)
            {
                string weaponCode = handler.GetString(22).ToUpper();
                if (weaponCode.Length == 4)
                {
                    if (isPlayer)
                    {
                        Objects.Items.ItemData itemData = null;
                        try
                        {
                            itemData = Managers.ItemManager.Instance.Items.Values.Where(n => n.Code == weaponCode).First();
                        }
                        catch { itemData = null; }

                        if (itemData != null && itemData.IsWeapon)
                        {
                            Objects.Items.Weapon weapon = (Objects.Items.Weapon)itemData;
                            if (weapon != null)
                            {
                                Objects.Inventory.Item item = handler.Player.User.Inventory.Equipment.Get(handler.Player.Class, weaponCode);
                                if (item != null)
                                {
                                    if (handler.Player.IsAlive && handler.Player.Health > 0)
                                    {
                                        if (p.IsAlive && p.Health > 0)
                                        {
                                            uint boneId = handler.GetuInt(11);
                                            if (boneId > 0)
                                            {
                                                byte realBoneId = 0;
                                                bool head = false;
                                                short remainingHealth = 0;
                                                bool useRadius = handler.GetBool(10);

                                                ushort previousHealth = p.Health;
                                                short damageTaken = 0;

                                                if (!useRadius)
                                                {
                                                    switch ((boneId - handler.Player.User.SessionID))
                                                    {
                                                        case 1237:
                                                            {
                                                                realBoneId = 0; // Head
                                                                head = true;
                                                                break;
                                                            }
                                                        case 1239:
                                                            {
                                                                realBoneId = 1; // Chest
                                                                break;
                                                            }
                                                        case 1241:
                                                            {
                                                                realBoneId = 2;
                                                                break;
                                                            }
                                                        default:
                                                            {
                                                                Log.Instance.WriteLine("Unknown Bone :: " + (boneId - handler.Player.User.SessionID) + " :: " + boneId);
                                                                //handler.Player.User.Disconnect();
                                                                break;
                                                            }
                                                    }

                                                    damageTaken = (short)((float)weapon.Power * ((float)weapon.PowerPersonal[realBoneId] / 100));
                                                }
                                                else
                                                {
                                                    damageTaken = (short)((1000 / 100) * boneId);
                                                }


                                                if (handler.Room.Mode != Enums.Mode.Free_For_All && handler.Player.Team == p.Team)
                                                    damageTaken = 0;

                                                remainingHealth = (short)((short)p.Health - damageTaken);

                                                if (remainingHealth < 0)
                                                    damageTaken = (short)p.Health;

                                                if (remainingHealth <= 0)
                                                {
                                                    handler.type = Enums.GameSubs.PlayerDeath;
                                                    p.AddDeaths();
                                                    handler.Player.AddKill(head);
                                                    OnDeath(handler.Player, p);
                                                }
                                                else
                                                {
                                                    p.Health -= (ushort)damageTaken;
                                                }

                                                handler.Set(12, p.Health);
                                                handler.Set(13, previousHealth);
                                                handler.respond = true;

                                                //System.Log.Instance.WriteLine("DAMAGE :: " + handler.Player.User.Displayname + " -> " + p.User.Displayname + ": " + damageTaken); 

                                            }
                                            else
                                            {
                                                handler.Player.User.Disconnect();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    handler.Player.User.Disconnect();
                                }

                            }
                            else
                            {
                                handler.Player.User.Disconnect();
                            }
                        }
                        else
                        {
                            handler.Player.User.Disconnect();
                        }
                    }
                    else
                    {
                        handler.Player.User.Disconnect();
                    }
                }
            }
        }

        public abstract Enums.Team Winner();

        public virtual void OnObjectDamage(Networking.GameDataHandler handler)
        {

        }

        protected abstract void OnDeath(Entities.Player killer, Entities.Player target);
        protected abstract void OnObjectDestory();
    }
}
