using System;
using System.Collections.Concurrent;

namespace Game.Modes {
    class Survival : Objects.GameMode {

        private ConcurrentDictionary<byte, Entities.Zombie> _zombies;

        private ushort[][] spawnTable;
        private ushort ushtSleepTime = 15;
        private ushort ushtZombiesSpawned = 0;
        private ushort ushtTotalZombiesSpawned = 0;

        private long lngPrepStart = 0;

        private bool blnDoSpawnCheck = false;
        private bool blnStarted = false;
        private bool blnPreparingWave = false;
        private byte bCurrentWave = 0;

        public Survival()
            : base(9, "Survival") {
            _zombies = null;
        }

        public override void Initilize(Entities.Room room) {
            base.Initilize(room);

            _zombies = new ConcurrentDictionary<byte, Entities.Zombie>();
            blnStarted = blnPreparingWave = blnDoSpawnCheck = false;

            ushtZombiesSpawned = ushtTotalZombiesSpawned = 0;
            ushtSleepTime = 15;

            SetupSpawnTable();
        }

        public override Enums.Team Winner() {
            return Enums.Team.None;
        }

        protected override void OnDeath(Entities.Player killer, Entities.Player target)
        {
        }

        protected override void OnObjectDestory() {
            throw new System.NotImplementedException();
        }

        public override bool IsGoalReached() {
            throw new System.NotImplementedException();
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

        private void SetupSpawnTable() {
            spawnTable = new ushort[21][];

            // Create an empty table.
            for (byte wave = 0; wave < spawnTable.Length; wave++) {
                spawnTable[wave] = new ushort[10];

                for (byte t = 0; t < spawnTable[wave].Length; t++) {
                    spawnTable[wave][t] = 0;
                }
            }

            // Set-up the table.

            // Wave 1
            SetWave(1, ZombieType.MadMan, 30);

            // Wave 2
            SetWave(2, ZombieType.MadMan, 32);
            SetWave(2, ZombieType.Maniac, 20);
        }

        private void SetWave(byte wave, ZombieType zombieType, ushort shtAmount) {
            if (wave > 0) {
                spawnTable[wave - 1][(byte)zombieType] = shtAmount;
            }
        }

        private void PrepareNextWave() {
            if (!blnPreparingWave) {
                blnPreparingWave = true;
                lngPrepStart = Environment.TickCount;

                //send(new SP_NEW_WAVE(0)); // Send wave end.

                if (bCurrentWave >= 10) // Start decreasing the wave wait time.
                {
                    int intSleep = (bCurrentWave - 8); // Makes the waves shorter - TODO: implement the real WarRock Time.
                    ushtSleepTime = (ushort)(15 - intSleep); // Set new sleep time-out.
                }
            }
        }

        private void StartNextWave() {
            if (blnPreparingWave) {
                blnPreparingWave = false;
                blnDoSpawnCheck = true;
                ushtZombiesSpawned = 0;
                bCurrentWave++;

                //send(new SP_NEW_WAVE(bCurrentWave -1)); // Send new wave!
            }
        }

        private ushort GetRemainingSpawns() {
            ushort ushtRemainingSpawns = 0;

            if (bCurrentWave > 0) {
                for (byte t = 0; t < spawnTable[bCurrentWave - 1].Length; t++) {
                    ushtRemainingSpawns += spawnTable[bCurrentWave - 1][t];
                }
            }

            return ushtRemainingSpawns;
        }

        private void SpawnZombie(ZombieType type) {
            // Set-up the user to follow.

            // Add zombie to the table :)

            // Send spawn packet to the players.
            //send(new SP_SPAWN_ZOMBIE(Zombie.ID, Zombie.FollowUser, ZombieSpawnPlace, Type));
        }

        private void RemoveZombie(byte slot) // Remove the zombie when it is dead.
        {
            Entities.Zombie zombie = null;
            if (_zombies.ContainsKey(slot)) {
                _zombies.TryRemove(slot, out zombie);
            }
        }

        private Entities.Zombie GetZombie(byte slot) // Gets the zombie class based on the slot id :)
        {
            Entities.Zombie zombie = null;
            if (_zombies.ContainsKey(slot)) {
                _zombies.TryGetValue(slot, out zombie);
            }

            return zombie;
        }

        public override void Process() {
            if (blnPreparingWave) // Is waiting between waves..
            {
                long lngResult = Environment.TickCount - lngPrepStart; // Calculate the diffrence :)

                if (lngResult >= ushtSleepTime) // Sleep time check.
                {
                    StartNextWave(); // Start new wave!
                }
            }

            if (!blnPreparingWave) // Wave is active
            {
                ushort ushtSpawnsRemaining = 0;
                if (bCurrentWave > 0) // We have an active wave!
                {
                    if (_zombies.Count < 28) // Less then
                    {
                        if (blnDoSpawnCheck) {
                            int intSpawnsToMake = 28 - _zombies.Count; // Calculate how many zombies we are going to spawn this tick!

                            // Do check once.. If the count is 0 disable it and wait for the wave to finish.
                            ushtSpawnsRemaining = GetRemainingSpawns();

                            if (ushtSpawnsRemaining < intSpawnsToMake) // Less spawns remaining then we can make :)
                                intSpawnsToMake = ushtSpawnsRemaining; // Push the value in the variable.

                            if (ushtSpawnsRemaining <= 0) // No spawns remaining
                                blnDoSpawnCheck = false;
                            else {
                                for (int i = 0; i < intSpawnsToMake; i++) {
                                    ushort spawnCount = 0;
                                    int maxValue = spawnTable[bCurrentWave - 1].Length - 1;
                                    byte bSpawnType = 0;
                                    Random rand = new Random();

                                    do // Loop until we have a slot to decrease.
                                    {
                                        bSpawnType = (byte)rand.Next(0, maxValue);
                                        spawnCount = spawnTable[bCurrentWave - 1][bSpawnType];
                                    } while (spawnCount <= 0);


                                    // Spawn This type of zombie
                                    ZombieType type = (ZombieType)bSpawnType; // Type.

                                    // Add Zombie to the table.
                                    SpawnZombie(type);

                                    // Decrease the remaining zombies in the table.
                                    spawnTable[bCurrentWave - 1][bSpawnType] -= 1;
                                    ushtZombiesSpawned++; // Increase zombies spawned during this wave.
                                    ushtTotalZombiesSpawned++; // Increase total zombie spawn count during the game.
                                }
                            }
                        }
                    }
                }

                if (ushtSpawnsRemaining <= 0 && _zombies.Count == 0) // No spawns remaining & no zombies alive!
                {
                    PrepareNextWave();
                }
            }
        }
    }

    public enum ZombieType : byte {
        MadMan = 0,
        Maniac,
        Grinder,
        Grounder,
        Heavy,
        Growler,
        Lover,
        Handgeman,
        Chariot,
        Crusher
    }
}
