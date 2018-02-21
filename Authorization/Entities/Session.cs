using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Authorization.Entities {
    class Session : Core.Entities.Entity {

        public uint DatabaseID { get; private set; }
        public uint SessionID { get; private set; }
        public bool IsActivated { get; private set; }
        public bool IsEnded { get; private set; }

        public DateTime Created { get; private set; }
        public DateTime Activated { get; private set; }

        private byte currentServer = 0;

        public Session(uint sessionId, uint userId, string username, string displayname) : base(userId, username, displayname){
            this.SessionID = sessionId;
            Created = DateTime.Now.ToUniversalTime();
            IsActivated = false;
            IsEnded = false;

            // Session Table - TODO
            //Databases.Game.Insert(
            //    "sessions",
            //    new Dictionary<string, object>()
            //    {
            //        { "sessionId"   , this.SessionID },
            //        { "userId"      , this.ID },
            //        { "creation"    , Created.ToUnixTimestamp()}
            //    });
        }

        public void UpdateDisplayname(string displayname) {
            this.Displayname = displayname;
        }

        public void Activate(byte serverId) {
            if (IsActivated) return;
            IsActivated = true;
            this.currentServer = serverId;
            Activated = DateTime.Now;

            lock (Program.sessionLock)
            {
                Program.onlinePlayers++;
                if (Program.onlinePlayers > Program.playerPeak)
                    Program.playerPeak = Program.onlinePlayers;
                Program.totalPlayers++;
            }
        }

        public void End() {
            // TODO: Save to database.
            lock (Program.sessionLock)
            {
                Program.onlinePlayers--;
            }

            IsEnded = true;
            Managers.SessionManager.Instance.Remove(this.SessionID);
        }

    }

    public static class TimeManupilation
    {
        public static uint ToUnixTimestamp(this DateTime Created)
        {
            return (uint)Created.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }
    }
}
