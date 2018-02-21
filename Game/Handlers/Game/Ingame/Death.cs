using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Handlers.Game.Ingame {
    class Death : Networking.GameDataHandler {
        protected override void Handle() {
            if (Room.State == Enums.RoomState.Playing) {
                respond = true;
            }
        }
    }
}
