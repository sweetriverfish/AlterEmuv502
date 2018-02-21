using System;
using Game.Enums;

namespace Game.Entities
{
    public class Player
    {
        public readonly User User;

        public byte Id { get; private set; }
        public Team Team { get; private set; }

        public Classes Class { get; private set; }
        public ushort Health { get; set; }
        public ushort Weapon { get; set; }
        public bool Ready { get; set; }
        public bool IsAlive { get; set; }

        public ushort Kills { get; private set; }
        public ushort Heads { get; private set; }
        public ushort Deaths { get; private set; }
        public ushort Flags { get; private set; }
        public ushort Headshots { get; private set; }
        public ushort Assists { get; private set; }
        public short Points { get; private set; }

        public byte RoundsPlayed { get; set; }
        public byte BombsPlanted { get; set; }
        public byte BombsDefused { get; set; }

        public long MoneyEarned { get; private set; }
        public long XPEarned { get; private set; }

        public bool CanSpawn { get; private set; }
        public bool RoundWait { get; private set; }
        public bool InLobby { get; set; }

        public DateTime LastSwitch { get; private set; }

        public Player(byte id, User user, Team team)
        {
            this.Id = id;
            this.User = user;
            this.Team = team;
            this.RoundWait = false;
            this.InLobby = true;
            this.CanSpawn = true;

            Reset();

            IsAlive = true;
            LastSwitch = DateTime.Now.AddSeconds(-5);
        }

        public void ToggleReady() {
            Ready = !Ready;
        }

        public void Reset() {
            Class = Classes.Engineer;
            Health = 1000;

            Kills = 0;
            Heads = 0;
            Deaths = 0;
            Flags = 0;
            Headshots = 0;
            Assists = 0;
            Points = 0;
            Weapon = 0;

            RoundsPlayed = 0;
            BombsPlanted = 0;
            BombsDefused = 0;

            MoneyEarned = 0;
            XPEarned = 0;
            Ready = false;
            IsAlive = true;
        }

        public void Set(byte newSlot, Team newTeam) {
            this.Id = newSlot;
            this.Team = newTeam;
            this.LastSwitch = DateTime.Now;
            User.SetRoom(User.Room, newSlot);
        }

        public void StartGame() {
            InLobby = false;
            IsAlive = true;
        }

        public void EndGame() {
            InLobby = false;

            if (User.Room.Master == Id)
                Ready = true;

            // Pre-check
            bool blnHasDoubleUp = (User.Inventory.Get("CC05") != null);


            // Calculate the xp rates
            double expRate = (User.Room.Supermaster) ? 1.05 : 1.0;
            double[] PremiumBonus = new double[] { 0, 0.2, 0.3, 0.5 };
            expRate += PremiumBonus[(byte)User.Premium];
            expRate += (User.Inventory.Get("CD01") != null) ? 0.3 : 0; // 30% EXP UP
            expRate += (User.Inventory.Get("CD02") != null) ? 0.2 : 0; // 20% EXP UP
            expRate += blnHasDoubleUp ? 0.25 : 0; // DOUBLE UP (25%)

            // Calculate the earned exp and dinar rates.
            double dinarRate = (User.Room.Supermaster) ? 1.10 : 1.0;
            dinarRate += blnHasDoubleUp ? 0.25 : 0;

            // Grab the global rates.
            double globalXPRate = Config.EXP_RATE;
            double globalDinarRate = Config.DINAR_RATE;

            // Determ if global XP event is  running //
            //if (Managers.EventManager.GlobalEvent) {
            //    fltGlobalXPRate = fltGlobalXPRate + fltGlobalXPRate * Managers.EventManager.xpRate;
            //    fltGlobalDinarRate = fltGlobalDinarRate + fltGlobalDinarRate * Managers.EventManager.DinarRate;
            //}

            // Calculate the earned XP.
            double xPEarned = 20 + (double)(this.Points) * 4 * expRate;
            xPEarned = xPEarned * globalXPRate; // Apply XP Event.

            // Calculate the earned dinar.
            double dinarEarned = 50 + ((double)this.Points * 3) * dinarRate;
            dinarEarned = dinarEarned * globalDinarRate; // Apply XP Event.

            // Convert the earned value.
            this.XPEarned = (long)Math.Ceiling(xPEarned);
            this.MoneyEarned = (long)Math.Ceiling(dinarEarned);

            // Save the earned exp and dinar
            User.EndGame(XPEarned, MoneyEarned);
        }

        public void BackToLobby() {
            InLobby = true;
        }

        public void EndRound() {
            RoundsPlayed += 1;
            User.RoundsPlayed += 1;

            RoundWait = true;
        }

        public void RoundReady() {
            RoundWait = false;
        }

        public void RoundStart() {
            CanSpawn = true;
            Health = 1000;
            IsAlive = true;
        }

        public void Send(byte[] buffer) {
            if (User != null)
                User.Send(buffer);
        }

        public void Spawn(Enums.Classes Class) {
            Health = 1000;
            IsAlive = true;
            this.Class = Class;

            if (User.Room.Mode == Mode.Explosive)
                CanSpawn = false;
        }

        public void Suicide() {
            AddDeaths();
            this.Points -= 6; // Decrease with 5.
        }

        public void AddKill(bool head) {
            this.Kills += 1;
            this.User.Kills += 1;

            if (head) {
                this.Heads += 1;
                this.User.Headshots += 1;
            }

            this.Points += 5;
        }

        public void AddDeaths() {
            this.Deaths += 1;
            this.User.Deaths += 1;
            this.Points += 1;
            this.Health = 0;
            this.IsAlive = false;
        }

        public short GetPoints() {
            return this.Points;
        }
    }
}
