using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Objects {
    public abstract class Command {

        public Command() {
            // TODO: Add permision system.
        }

        public bool Handle(Entities.User u) {
            return Process(u);
        }

        protected abstract bool Process(Entities.User u);
    }
}
