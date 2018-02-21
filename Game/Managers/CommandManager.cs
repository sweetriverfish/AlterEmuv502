using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;

namespace Game.Managers {
    class CommandManager {
        private ConcurrentDictionary<String, Objects.Command> _commandList;

        public CommandManager() {
            _commandList = new ConcurrentDictionary<string, Objects.Command>();
            Load();
        }

        public void Load() {
            _commandList.Clear();
        }

        private void AddCommand(string name, Objects.Command command) {
            if (!_commandList.ContainsKey(name)) {
                _commandList.TryAdd(name, command);
            }
        }

        public void GetHandler() {

        }
    }
}
