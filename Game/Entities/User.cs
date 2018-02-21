using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

// DEBUG //
using System.Diagnostics;

using Core.Networking;
using Game.Objects.Inventory;
using Game.Managers;
using Game.Enums;
using Game.Networking;

namespace Game.Entities
{
    public class User : Core.Entities.Entity, IConnection
    {

        #region Connection Variables
        private Socket socket;
        private byte[] buffer = new byte[1024];
        private byte[] cacheBuffer = new byte[0];
        private bool isDisconnect = false;
        private uint packetCount = 0;
        #endregion

        #region Lobby Variables

        public Entities.Room Room { get; private set; }
        public Premium Premium { get; private set; }
        public ulong PremiumExpireDate { get; private set; }
        public long PremiumTimeInSeconds { get; private set; }
        public uint Kills { get; set; }
        public uint Headshots { get; set; }
        public uint Deaths { get; set; }
        public ulong XP { get; private set; }
        public uint Money { get; set; }
        public Inventory Inventory { get; private set; }
        public int LastRoomId { get; set; }
        public byte RoomSlot { get; private set; }
        public byte RoomListPage { get; set; }
        public uint Ping { get; private set; }

        private object pingLock = new object();
        private bool pingOk = true;
        private DateTime lastPingTime = DateTime.Now;

        public IPEndPoint RemoteEndPoint;
        public IPEndPoint LocalEndPoint;

        public ushort RemotePort;
        public ushort LocalPort;

        #endregion

        #region Information Variabled

        public uint BombsPlanted = 0;
        public uint BombsDefused = 0;
        public uint RoundsPlayed = 0;

        #endregion

        public User(Socket socket)
            : base(0, "Unknown", "Unknown")
        {
            this.socket = socket;
            this.Room = null;
            this.Ping = 0;
            this.LastRoomId = -1;

            isDisconnect = false;

            this.socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnDataReceived), null);
            Send(new Core.Packets.Connection(Core.Constants.xOrKeySend));
        }

        public static void WriteLine(string logText)
        {
            DateTime _DTN = DateTime.Now;
            StackFrame _SF = new StackTrace().GetFrame(2);
            Console.Write("[" + _DTN.ToLongTimeString() + ":" + _DTN.Millisecond.ToString() + "] [");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(_SF.GetMethod().ReflectedType.Name + "." + _SF.GetMethod().Name);
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("] » ");
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Log.Instance.WriteLine(logText);
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        public void OnAuthorize(uint id, string name, string displayname)
        {
            this.ID = id;
            this.Name = name;
            this.Displayname = displayname;

            // LOAD PLAYER INFORMATION //
            if (!Load())
            {
                Send(new Packets.Authorization(Packets.Authorization.ErrorCodes.BadSynchronization));
                return;
            }

            // LOAD INVENTORY //
            Inventory = new Inventory(this);
            Inventory.Load();

            this.Authenticated = true;
            this.RoomListPage = 0;

            // Send the packet back //
            Send(new Packets.Authorization(this));
            SendPing();

            if (Inventory.ExpiredItems.Count > 0)
            {
                Send(new Packets.UpdateInventory(this));
                Inventory.ExpiredItems.Clear();
            }
        }

        private bool Load()
        {
            MySqlDataReader result = Databases.Game.Select(
                 new string[] { "kills", "deaths", "headshots", "xp", "money", "premium", "premium_expiredate", "play_time", "rounds_played", "bombs_planted", "bombs_defused" },
                 "user_details",
                 new Dictionary<string, object>()
                            {
                                { "id", this.ID },
                            }
                 );

            if (result == null) return false;
            if (result.HasRows && result.Read())
            {
                try
                {
                    this.Kills = result.GetUInt32(0);
                    this.Deaths = result.GetUInt32(1);
                    this.Headshots = result.GetUInt32(2);
                    this.XP = result.GetUInt64(3);
                    this.Money = result.GetUInt32(4);
                    this.Premium = (Premium)result.GetByte(5);
                    this.PremiumExpireDate = result.GetUInt64(6);

                    this.RoundsPlayed = result.GetUInt32("rounds_played");
                    this.BombsPlanted = result.GetUInt32("bombs_planted");
                    this.BombsDefused = result.GetUInt32("bombs_defused");

                    result.Close();
                    return true;
                }
                catch { result.Close(); return false; }
            }
            else
            {
                result.Close();
                string query = string.Concat("INSERT INTO user_details (`id`, `kills`, `deaths`, `headshots`, `xp`, `money`, `premium`, `premium_expiredate`, `play_time`) VALUES ('", this.ID, "', '0', '0', '0', '0', '50000', '0', '0', '0');");
                Databases.Game.Query(query);
                return Load();
            }
        }

        public void SetRoom(Room room, byte slot)
        {
            this.Room = room;
            this.RoomSlot = slot;
            this.RoomListPage = 0;

            if (room != null)
                LastRoomId = (int)room.ID;
        }

        public void SetChannel(ChannelType type)
        {
            if (Channel != type)
            {
                ChannelManager.Instance.Remove(Channel, this); // Remove from old
                Channel = type; // change
                RoomListPage = 0;

                if (!ChannelManager.Instance.Add(Channel, this))
                    this.Disconnect(); // Failed to join :'(
            }
        }

        public void EndGame(long moneyEarned, long xpEarned)
        {
            // Store old XP.
            ulong oldXP = this.XP;
            if (((long)oldXP + xpEarned) > 0)
            {
                if (xpEarned < 0)
                {
                    XP -= (ulong)Math.Abs(xpEarned);
                }
                else
                {
                    XP += (ulong)xpEarned;
                }

            }
            else
            {
                XP = 0;
            }


            if (((long)Money + moneyEarned) > 0)
                if (moneyEarned < 0)
                    Money -= (uint)Math.Abs(moneyEarned);
                else
                    Money += (uint)moneyEarned;
            else
                Money = 0;

            // Detect level changes.
            byte oldLevel = Core.LevelCalculator.GetLevelforExp(oldXP);
            byte currentLevel = Core.LevelCalculator.GetLevelforExp(XP);

            if (currentLevel > oldLevel)
            { // Gained a level or more, send level up packet.
                // Calculate the diffrence.
                byte levelsGained = (byte)(currentLevel - oldLevel);
                uint lvlMoneyEarned = levelsGained * Config.LEVEL_UP_MONEY_REWARD;
                // Apply the Money & Send packet.
                Money += lvlMoneyEarned;
                uint nowTimeStamp = (uint)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                Send(new Packets.LevelUp(this, lvlMoneyEarned));

                // Log query
                Databases.Game.Insert("game_levels_gained", new Dictionary<string, object>() {
                    { "user_id", this.ID },
                    { "game_id", 0 }, // TODO
                    { "current_level", currentLevel},
                    { "levels_gained", levelsGained },
                    { "timestamp", nowTimeStamp }
                });
            }

            // Money update
            Databases.Game.AsyncQuery("UPDATE user_details SET money=" + this.Money + " WHERE id=" + this.ID);

        }

        private void UpdatePremiumState()
        {
            if (PremiumExpireDate > 0 || Premium != Enums.Premium.Free2Play)
            {
                uint currentTimestamp = (uint)DateTime.Now.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
                PremiumTimeInSeconds = (long)(PremiumExpireDate - currentTimestamp);
                if (PremiumTimeInSeconds <= 0)
                { // The Premium expired.
                    PremiumTimeInSeconds = 0;
                    PremiumExpireDate = 0;
                    Premium = Enums.Premium.Free2Play;
                    // Execute a database query to make sure it's updated.
                    Databases.Game.AsyncQuery("UPDATE user_details SET premium=" + (byte)Premium + ", premium_expiredate=" + PremiumExpireDate + " WHERE id=" + this.ID);
                }
            }
            else
            {
                PremiumTimeInSeconds = 0;
                Premium = Enums.Premium.Free2Play;
            }
        }

        public void SendPing()
        {
            lock (pingLock)
            {
                if (!pingOk)
                {
                    Disconnect();
                    return;
                }

                UpdatePremiumState();

                pingOk = false;
                Send(new Packets.Ping(this));
            }
        }

        public void PingReceived()
        {
            lock (pingLock)
            {
                this.pingOk = true;
                TimeSpan pingDiff = DateTime.Now - this.lastPingTime;
                this.Ping = (uint)pingDiff.TotalMilliseconds;
            }
        }

        public void SetSession(uint sessionId)
        {
            if (sessionId > 0)
            {
                SessionID = sessionId;
            }
            else
            {
                this.Disconnect();
            }
        }

        private void OnDataReceived(IAsyncResult iAr)
        {
            try
            {
                int bytesReceived = socket.EndReceive(iAr);

                if (bytesReceived > 0)
                {
                    byte[] packetBuffer = new byte[bytesReceived];

                    // Decrypt the bytes with the xOrKey.
                    for (int i = 0; i < bytesReceived; i++)
                    {
                        packetBuffer[i] = (byte)(this.buffer[i] ^ Core.Constants.xOrKeyReceive);
                    }

                    int oldLength = cacheBuffer.Length;
                    Array.Resize(ref cacheBuffer, oldLength + bytesReceived);
                    Array.Copy(packetBuffer, 0, cacheBuffer, oldLength, packetBuffer.Length);

                    int startIndex = 0; // Determs where the bytes should split
                    for (int i = 0; i < cacheBuffer.Length; i++)
                    { // loop trough our cached buffer.
                        if (cacheBuffer[i] == 0x0A)
                        { // Found a complete packet
                            byte[] newPacket = new byte[i - startIndex]; // determ the new packet size.
                            for (int j = 0; j < (i - startIndex); j++)
                            {
                                newPacket[j] = cacheBuffer[startIndex + j]; // copy the buffer to the buffer of the new packet.
                            }
                            packetCount++;
                            // Instant handeling
                            InPacket inPacket = new InPacket(newPacket);
                            if (inPacket != null)
                            {
                                if (inPacket.Id > 0)
                                {
                                    PacketHandler<User> pHandler = NetworkTable.Instance.FindExternal(inPacket);
                                    if (pHandler != null)
                                    {
                                        try
                                        {
                                            pHandler.Handle(this, inPacket);
                                        }
                                        catch (Exception e) { Log.Instance.WriteError(e.ToString()); }
                                    }
                                }
                            }
                            // Increase start index.
                            startIndex = i + 1;
                        }
                    }

                    if (startIndex > 0)
                    {
                        byte[] fullCopy = cacheBuffer;
                        Array.Resize(ref cacheBuffer, (cacheBuffer.Length - startIndex));
                        for (int i = 0; i < (cacheBuffer.Length - startIndex); i++)
                        {
                            cacheBuffer[i] = fullCopy[startIndex + i];
                        }
                        fullCopy = null;
                    }

                    socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(OnDataReceived), null);
                }
                else
                {
                    Disconnect();
                }
            }
            catch
            {
                Disconnect();
            }
        }

        public void Send(byte[] sendBuffer)
        {
            try { socket.BeginSend(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), null); }
            catch { Disconnect(); }
        }

        public void Send(Core.Networking.OutPacket outPacket)
        {
            try
            {
                byte[] sendBuffer = outPacket.BuildEncrypted();
                socket.BeginSend(sendBuffer, 0, sendBuffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
            }
            catch { Disconnect(); }
        }

        private void SendCallback(IAsyncResult iAr)
        {
            try { socket.EndSend(iAr); }
            catch { Disconnect(); }
        }

        public void Disconnect()
        {
            if (isDisconnect) return;
            isDisconnect = true;

            WriteLine("User disconnected.");

            if (Channel > ChannelType.None)
                ChannelManager.Instance.Remove(Channel, this);

            if (SessionID > 0)
                UserManager.Instance.Remove(SessionID);



            try { socket.Close(); } catch { }
        }

        public ChannelType Channel { get; private set; }
        public uint SessionID { get; private set; }
        public bool Authenticated { get; private set; }
    }
}
