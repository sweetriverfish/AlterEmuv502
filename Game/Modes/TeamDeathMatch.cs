using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Modes {
    class TeamDeathMatch : Objects.GameMode {
        private int intCurrentLeaderKills = 0;
        private int intMaximumKills = 0;

        private readonly object _syncObject;

        public TeamDeathMatch()
            : base(2, "TDM") {
            _syncObject = new object();
        }

        public override void Initilize(Entities.Room room) {
            base.Initilize(room);

            // Validate based on the room setting here.
            intCurrentLeaderKills = 0;
            intMaximumKills = 10 + (5 * room.Setting);

        }

        public override Enums.Team Winner() {
            return Enums.Team.None;
        }

        public override void Process() {

        }

        protected override void OnDeath(Entities.Player killer, Entities.Player target)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnObjectDestory() {
            throw new System.NotImplementedException();
        }

        public override bool IsGoalReached() {
            return (intCurrentLeaderKills >= intMaximumKills);
        }

        private void ProcessKill() {
            if (intCurrentLeaderKills >= intMaximumKills) {
                //this.Room.EndGame();
            }
        }

        public override void HandleExplosives(string[] blocks, Entities.Player p) {
            throw new NotImplementedException();
        }

        public override byte CurrentRoundTeamA() {
            return 0;
        }

        public override byte CurrentRoundTeamB() {
            return 0;
        }

        public override ushort ScoreboardA() {
            return 0;
        }

        public override ushort ScoreboardB() {
            return 0;
        }
    }
}
