using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Game.Enums;

namespace Game.Entities {
    public class Room : Core.Entities.Entity {

        public readonly byte[] Urban_MaxPlayers = new byte[] { 8, 16, 20, 24 };
        public readonly byte[] Battle_MaxPlayers = new byte[] { 8, 16, 20, 24, 32 };

        public ChannelType Channel { get; private set; }
        public RoomState State { get; set; }

        public long UpTick = 0;
        public long DownTick = 0;
        public int LastTick { get; private set; }

        public byte Map { get; set; }
        public byte Type { get; private set; }
        public Mode Mode { get; set; }
        public byte Setting { get; set; }
        public byte LevelLimit { get; private set; }

        public bool HasPassword { get; private set; }
        public string Password { get; private set; }

        public byte MaximumPlayers { get; private set; }
        public byte PingLimit { get; set; }

        public bool Supermaster { get; private set; }
        public bool PremiumOnly { get; private set; }
        public bool EnableVoteKick { get; private set; }
        public bool AutoStart { get; set; }

        public object RoomLock = new object();

        public bool Running { get; set; }

        public sbyte[] Flags { get; private set; }

        public byte Master { get; private set; }
        public readonly ConcurrentDictionary<byte, Player> Players;

        public Objects.GameMode CurrentGameMode { get; private set; }
        public object _playerLock;

        public bool IsJoinable {
            get {
                return (Players.Count < MaximumPlayers);
            }
        }

        public Room(User master, uint id, string displayName, bool hasPassword, string password, byte maxPlayers, byte roomType, byte levelLimit, bool premiumOnly, bool enableVotekick)
            : base(id, "Room", displayName) {
            this.PingLimit = 2; // ALL - TEMP

            this.Players = new ConcurrentDictionary<byte, Player>();
            this.State = RoomState.Waiting; // Set the state to waiting :)
            this.Channel = master.Channel;
            this.HasPassword = hasPassword;

            this.CurrentGameMode = null;

            this.Supermaster = master.Inventory.Get("CC02") != null;

            if (hasPassword) {
                this.Password = password; // Use the password provided by the client.
            } else {
                this.Password = "NULL"; // Don't trus the client :)
            }

            switch (Channel) {
                case ChannelType.CQC: {
                        Mode = Mode.Explosive; // Explosive :) - TODO: Make this configurable
                        Setting = 3; // 7 Rounds :) - TODO: Make this configurable
                        MaximumPlayers = (byte)(8 * (maxPlayers + 1));
                        break;
                    }
                case ChannelType.Urban_Ops: {
                        Mode = Mode.Team_Death_Match;
                        Setting = 2;
                        MaximumPlayers = Urban_MaxPlayers[maxPlayers];
                        break;
                    }
                case ChannelType.Battle_Group: {
                        Mode = Mode.Team_Death_Match;
                        Setting = 3;
                        MaximumPlayers = Battle_MaxPlayers[maxPlayers];
                        break;
                    }
                case ChannelType.AI_Channel: {
                        MaximumPlayers = 4;
                        break;
                    }
                default: {
                        Mode = Mode.Team_Death_Match;
                        MaximumPlayers = 8;
                        break;
                    }
            }

            try {
                Map = Managers.MapManager.Instance.MapRotation[((byte)Channel - 1)][(byte)Mode].First();
            } catch {
                master.SetRoom(null, 0);
                master.Send(new Packets.RoomCreation(RoomCreationErrors.GenericError));
                return;
            }

            this._playerLock = new object();
            this.Type = roomType;
            this.LevelLimit = levelLimit;
            this.PremiumOnly = premiumOnly;
            this.EnableVoteKick = enableVotekick;

            // SET-UP THE MASTER SLOT //
            Master = 0;
            master.SetRoom(this, 0);
            Player p = new Player(0, master, Team.Derbaran) { Ready = true };
            Players.TryAdd(p.Id, p);
            // ASSIGN THE ID TO THE MASTER //

        }

        public bool SwitchSide(Entities.Player p) {
            lock (_playerLock) {

                byte maxTeamSize = (byte)(MaximumPlayers / 2);
                byte teamCount = 0;
                Team newTeam = Team.Derbaran;

                byte startScanIndex = 0;
                byte endScanIndex = (byte)(MaximumPlayers / 2);

                if (p.Team == Team.Derbaran) {
                    teamCount = (byte)Players.Select(n => n.Value).Where(n => n.Team == Team.NIU).Count();
                    startScanIndex = (byte)(MaximumPlayers / 2);
                    endScanIndex = MaximumPlayers;
                    newTeam = Team.NIU;
                } else
                    teamCount = (byte)Players.Select(n => n.Value).Where(n => n.Team == Team.Derbaran).Count();


                if (teamCount < maxTeamSize) {
                    for (byte i = startScanIndex; i < endScanIndex; i++) {
                        if (!Players.ContainsKey(i)) {
                            Entities.Player plr = null;
                            // Remove Old Player
                            if (Players.ContainsKey(p.Id))
                                Players.TryRemove(p.Id, out plr);

                            // Insert new Player
                            plr = p;

                            if (Players.TryAdd(i, plr)) {
                                if (p.Id == Master)
                                    Master = i;
                                plr.Set(i, newTeam);
                                return true;
                            } else {
                                plr.User.Disconnect();
                            }

                            break;
                        }
                    }
                }
            }
            return false;
        }

        public bool Add(Entities.User u) {
            if (Players.Count < MaximumPlayers) {
                lock (_playerLock) {

                    Team targetTeam = Team.Derbaran;
                    byte startScanIndex = 0;
                    byte endScanIndex = (byte)(MaximumPlayers / 2);

                    var debTeam = Players.Select(n => n.Value).Where(n => n.Team == Team.Derbaran).Count();
                    var niuTeam = Players.Select(n => n.Value).Where(n => n.Team == Team.NIU).Count();

                    if (debTeam > niuTeam) {
                        targetTeam = Team.NIU;
                        startScanIndex = (byte)(MaximumPlayers / 2);
                        endScanIndex = MaximumPlayers;
                    }

                    for (byte i = startScanIndex; i < endScanIndex; i++) {
                        if (!Players.ContainsKey(i)) {
                            Player plr = new Player(i, u, targetTeam);

                            // Send Room //
                            if (Players.Count > 0) {
                                Send((new Packets.RoomPlayers(new ArrayList() { plr }).BuildEncrypted()));
                            } else {
                                Master = i;
                            }

                            // Add Player
                            Players.TryAdd(i, plr);
                            u.SetRoom(this, i);

                            // Send Join Packet //
                            u.Send(new Packets.RoomJoin(i, this));
                            u.Send(new Packets.RoomPlayers(new ArrayList(Players.Select(n => n.Value).Where(n => n.Id != i).ToArray())));

                            // Send to all spectators: TODO //

                            // SEND THE ROOM UPDATE TO THE LOBBY //
                            byte roomPage = (byte)Math.Floor((decimal)(this.ID / 8));
                            var targetList = Managers.ChannelManager.Instance.Get(this.Channel).Users.Select(n => n.Value).Where(n => n.RoomListPage == roomPage && n.Room == null);
                            if (targetList.Count() > 0) {
                                byte[] outBuffer = new Packets.RoomUpdate(this, true).BuildEncrypted();
                                foreach (Entities.User usr in targetList)
                                    usr.Send(outBuffer);
                            }

                            return true;
                        }
                    }

                }
            }

            return false;
        }

        public void HandleExplosives(string[] blocks, Entities.User u) {
            Player p = null;
            Players.TryGetValue(u.RoomSlot, out p);
            if (p != null) {
                CurrentGameMode.HandleExplosives(blocks, p);
            } else {
                u.Disconnect(); // No Player?
            }
        }

        public void Remove(Entities.User u) {
            if (u.Room != null) {
                if (u.Room.ID == this.ID) {
                    lock (this.RoomLock) {
                        Player p;
                        byte oldSlot = 0;

                        lock (_playerLock) {
                            if (Players.ContainsKey(u.RoomSlot)) {
                                Players.TryRemove(u.RoomSlot, out p);

                                if (p != null) {
                                    oldSlot = u.RoomSlot;
                                    p.User.SetRoom(null, 0);
                                } else
                                    u.SetRoom(null, 0);

                                if (Players.Count > 0) {
                                    // Asign new master //
                                    if (u.RoomSlot == Master) {

                                        // Remove the suppermaster buff :)
                                        this.Supermaster = false;

                                        // Calculate the priority level.
                                        int[] priority = new int[MaximumPlayers];
                                        for (byte i = 0; i < MaximumPlayers; i++) {
                                            if (Players.ContainsKey(i)) {
                                                if (Players[i] != null) {
                                                    priority[i] = 1 + (int)Players[i].User.Premium;
                                                } else {
                                                    priority[i] = 0; // no priority.
                                                }
                                            } else {
                                                priority[i] = 0; // no priority.
                                            }
                                        }

                                        // Find the new master.
                                        sbyte newMaster = -1;
                                        int masterPrior = 0;
                                        for (byte j = 0; j < MaximumPlayers; j++) {
                                            if (priority[j] > masterPrior) { // A player with higher piority has been found.
                                                newMaster = (sbyte)j; // set the master slot
                                                masterPrior = priority[j]; // store the piority.
                                            }
                                        }

                                        // We have found a new master!
                                        if (newMaster >= 0) {
                                            Master = (byte)newMaster;
                                        } else {
                                            Destory();
                                        }
                                    }
                                } else {
                                    Destory();
                                }

                                byte[] pBuffer = new Packets.RoomLeave(u, oldSlot, this).BuildEncrypted();
                                if (Players.Count > 0)
                                    Send(pBuffer);

                                u.Send(pBuffer);
                            }
                        }
                    }
                }
            }
        }

        public void Destory() {
            EndGame(Team.None);

            // Remove from the room Manager //
            Managers.ChannelManager.Instance.Get(Channel).Remove(this);
        }

        public void Send(byte[] buffer) {
            lock (_playerLock) {
                foreach (KeyValuePair<byte, Player> entry in Players) {
                    if (entry.Value != null)
                        entry.Value.Send(buffer);
                }
            }
        }

        public void Start() {
            UpTick = 0;
            DownTick = 1800000;
            LastTick = -1;

            Objects.Map map = Managers.MapManager.Instance.Get(Map);
            byte flagCount = map.Flags;
            byte[] spawnFlags = map.SpawnFlags;
            this.Flags = new sbyte[flagCount];
            for (byte i = 0; i < 0; i++)
                Flags[i] = -1;

            Flags[spawnFlags[0]] = 0;
            Flags[spawnFlags[1]] = 1;


            foreach (Player p in Players.Values) {
                p.Reset();
                p.RoundStart();
            }

            switch (Mode) {
                case Enums.Mode.Explosive: {
                        CurrentGameMode = new Modes.Explosive();
                        break;
                    }
                case Enums.Mode.Free_For_All: {
                        CurrentGameMode = new Modes.FreeForAll();
                        break;
                    }
                case Enums.Mode.Team_Death_Match: {
                        CurrentGameMode = new Modes.TeamDeathMatch();
                        break;
                    }
            }

            CurrentGameMode.Initilize(this);
            Running = false;
        }

        public void EndGame(Team winning) {
            if (this.State == RoomState.Playing)
            {
                Log.Instance.WriteLine(ID + " EndGame().");

                Player[] players = Players.Values.ToArray();
                foreach (Player p in Players.Values)
                {
                    p.EndGame();
                    Log.Instance.WriteLine("Sending Endgame Packet for: " + p.User.Displayname);
                    p.Send((new Packets.EndGame(this, players, winning)).BuildEncrypted());
                }


                if (CurrentGameMode != null)
                {
                    CurrentGameMode = null;
                }

                this.State = RoomState.Waiting;
            }
        }

        public void Process() {
            while (State == RoomState.Playing) {
                if (CurrentGameMode != null && CurrentGameMode.Initilized) {
                    if (!CurrentGameMode.IsGoalReached()) {
                        CurrentGameMode.Process();
                        if (CurrentGameMode != null && !CurrentGameMode.FreezeTick) {
                            if (LastTick != DateTime.Now.Second) {
                                LastTick = DateTime.Now.Second;
                                UpTick += 1000;
                                DownTick -= 1000;
                                Send((new Packets.GameTick(this)).BuildEncrypted());
                            }
                        } else {
                            LastTick = -1;
                        }
                    } else {
                        if (CurrentGameMode != null)
                            EndGame(CurrentGameMode.Winner());
                    }
                }
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
