using System.Collections.Concurrent;

using Game.Entities;

namespace Game.Managers
{
    class UserManager {

        public readonly ConcurrentDictionary<uint, User> Sessions;
        private int playerPeak = 0;

        public UserManager() {
            Sessions = new ConcurrentDictionary<uint, User>();
        }

        public bool Add(uint sessionId, User u) {
            if (!Sessions.ContainsKey(sessionId))
            {
                u.SetSession(sessionId);
                if (Sessions.TryAdd(sessionId, u))
                {
                    if (Sessions.Count > playerPeak)
                        playerPeak = Sessions.Count;
                    return true;
                }
            }
            return false;
        }

        public User Get(uint sessionId) {
            User u = null;
            if (Sessions.ContainsKey(sessionId)) {
                try { Sessions.TryGetValue(sessionId, out u); }
                catch { u = null; }
                
            }
            return u;
        }

        public void Remove(uint sessionId) {
            if (Sessions.ContainsKey(sessionId))
            {
                User u = null;
                Sessions.TryRemove(sessionId, out u);

                if (u != null) {
                    if (u.Authenticated) {
                        // SAVE THE PLAYER DATA //
                        string query = string.Concat("UPDATE user_details SET kills = '", u.Kills ,"', deaths = '", u.Deaths ,"', headshots = '", u.Headshots ,"', xp = '", u.XP ,"', play_time = '0', rounds_played = '", u.RoundsPlayed ,"', bombs_planted = '", u.BombsPlanted ,"', bombs_defused = '", u.BombsDefused ,"' WHERE id = ", u.ID, ";");
                        Databases.Game.Query(query);
                    }
                }

                // TELL THE AUTH SERVER THAT THE SESSION IS EXPIRED //
                Program.AuthServer.Send(new Packets.Internal.PlayerAuthorization(sessionId));
            }
        }

        public int Peak
        {
            get
            {
                return this.playerPeak;
            }
        }

        private static UserManager instance = null;
        public static UserManager Instance { get { if (instance == null) instance = new UserManager(); return instance; } set { } }
    }
}
