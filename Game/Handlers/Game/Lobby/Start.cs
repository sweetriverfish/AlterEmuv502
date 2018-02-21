using System;
using System.Linq;

namespace Game.Handlers.Game {
    class Start : Networking.GameDataHandler {
        protected override void Handle() {
            if (Room.Master == Player.Id) {
                if (Room.State == Enums.RoomState.Waiting) {
                    lock (Room._playerLock) {
                        if (Room.Players.All(p => p.Value.Ready)) {
                            var debTeam = Room.Players.Select(n => n.Value).Where(n => n.Team == Enums.Team.Derbaran).Count();
                            var niuTeam = Room.Players.Select(n => n.Value).Where(n => n.Team == Enums.Team.NIU).Count();
                            int teamDifference = Math.Abs(debTeam - niuTeam);
                            if (teamDifference <= Config.MAXIMUM_TEAM_DIFFRENCE)
                            {
                                respond = updateLobby = true;
                                type = Enums.GameSubs.StartReply;
                                Room.State = Enums.RoomState.Playing;
                                Set(2, Room.Map);
                                Room.Start();
                            }
                        }
                    }
                }
            } else {
                Player.User.Disconnect(); // Cheating? - Not a master!
            }
        }
    }
}
