using System.Collections.Concurrent;

using Game.Entities;

namespace Game.Managers
{
    class UserManager
    {
        public int Peak { get; private set; }

        public readonly ConcurrentDictionary<uint, User> Sessions;

        public UserManager()
        {
            Sessions = new ConcurrentDictionary<uint, User>();
        }

        public bool Add(uint sessionId, User u)
        {
            if (!Sessions.TryAdd(sessionId, u))
                return false;

            u.SetSession(sessionId);

            if (Sessions.Count > Peak)
                Peak = Sessions.Count;

            return true;
        }

        public User Get(uint sessionId)
        {
            Sessions.TryGetValue(sessionId, out var result);

            return result;
        }

        public void Remove(uint sessionId)
        {
            if (Sessions.TryRemove(sessionId, out var u))
            {
                if (u.Authenticated)
                {
                    // SAVE THE PLAYER DATA //
                    string query = string.Concat("UPDATE user_details SET kills = '", u.Kills, "', deaths = '", u.Deaths, "', headshots = '", u.Headshots, "', xp = '", u.XP, "', play_time = '0', rounds_played = '", u.RoundsPlayed, "', bombs_planted = '", u.BombsPlanted, "', bombs_defused = '", u.BombsDefused, "' WHERE id = ", u.ID, ";");
                    Databases.Game.Query(query);
                }
            }

            // TELL THE AUTH SERVER THAT THE SESSION IS EXPIRED //
            Program.AuthServer.Send(new Packets.Internal.PlayerAuthorization(sessionId));
        }

        private static UserManager instance = null;
        public static UserManager Instance { get { if (instance == null) instance = new UserManager(); return instance; } set { } }
    }
}
