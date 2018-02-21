using Core.Networking;
using Game.Entities;
using System.Collections.Generic;

namespace Game.Networking
{
    class NetworkTable : AbstractNetworkTable<User, ServerClient>
    {
        private Dictionary<ushort, GameDataHandler> gameHandlers = new Dictionary<ushort, GameDataHandler>();

        protected override void OnInitialize()
        {
            // Internal Packets//
            AddInternal(Core.Enums.InternalPackets.Connection, new Handlers.Internal.Connection());
            AddInternal(Core.Enums.InternalPackets.Authorization, new Handlers.Internal.Authorization());
            AddInternal(Core.Enums.InternalPackets.Ping, new Handlers.Internal.Ping());
            AddInternal(Core.Enums.InternalPackets.PlayerAuthorization, new Handlers.Internal.PlayerAuthorization());

            // External Packets //
            AddExternal((ushort)Enums.Packets.ServerTime, new Handlers.RequestServerTime());
            AddExternal((ushort)Enums.Packets.Authorization, new Handlers.Authorization());
            AddExternal((ushort)Enums.Packets.ChannelSelection, new Handlers.ChangeChannel());
            AddExternal((ushort)Enums.Packets.RoomCreation, new Handlers.RoomCreation());
            AddExternal((ushort)Enums.Packets.RoomJoin, new Handlers.RoomJoin());
            AddExternal((ushort)Enums.Packets.RoomLeave, new Handlers.RoomLeave());
            AddExternal((ushort)Enums.Packets.Equipment, new Handlers.Equipment());
            AddExternal((ushort)Enums.Packets.Chat, new Handlers.Chat());
            AddExternal((ushort)Enums.Packets.RoomList, new Handlers.RoomList());
            AddExternal((ushort)Enums.Packets.Itemshop, new Handlers.Itemshop());
            AddExternal((ushort)Enums.Packets.GamePacket, new Handlers.RoomData());
            AddExternal((ushort)Enums.Packets.Explosives, new Handlers.Explosives());
            AddExternal((ushort)Enums.Packets.Scoreboard, new Handlers.Scoreboard());
            AddExternal((ushort)Enums.Packets.Ping, new Handlers.Ping());

            AddGameHandlers();
        }

        private void AddGameHandlers()
        {
            // Game Data Handlers //
            AddGameHandler(Enums.GameSubs.Start, new Handlers.Game.Start());
            AddGameHandler(Enums.GameSubs.RoundEndConfirm, new Handlers.Game.Ingame.RoundReady());
            AddGameHandler(Enums.GameSubs.BackToLobby, new Handlers.Game.Ingame.BackToLobby());
            AddGameHandler(Enums.GameSubs.Ready, new Handlers.Game.ToggleReady());
            AddGameHandler(Enums.GameSubs.MapUpdate, new Handlers.Game.ChangeMap());
            AddGameHandler(Enums.GameSubs.ModeUpdate, new Handlers.Game.ChangeMode());
            AddGameHandler(Enums.GameSubs.ChangeRounds, new Handlers.Game.ChangeSubMode());
            AddGameHandler(Enums.GameSubs.ChangeKills, new Handlers.Game.ChangeSubMode());
            AddGameHandler(Enums.GameSubs.ChangeKills, new Handlers.Game.ChangeSubMode());
            AddGameHandler(Enums.GameSubs.SideUpdate, new Handlers.Game.ChangeSide());
            AddGameHandler(Enums.GameSubs.Userlimit, new Handlers.Game.ToggleUserlimit());
            AddGameHandler(Enums.GameSubs.Pinglimit, new Handlers.Game.ChangePinglimit());
            // Votekick
            AddGameHandler(Enums.GameSubs.Autostart, new Handlers.Game.ToggleAutostart());
            //
            AddGameHandler(Enums.GameSubs.ConfirmSpawn, new Handlers.Game.Ingame.ConfirmSpawn());
            AddGameHandler(Enums.GameSubs.Spawn, new Handlers.Game.Ingame.Spawn());
            //
            AddGameHandler(Enums.GameSubs.PlayerDamage, new Handlers.Game.Ingame.PlayerDamage());
            AddGameHandler(Enums.GameSubs.PlayerDeath, new Handlers.Game.Ingame.Death());

            //
            AddGameHandler(Enums.GameSubs.GetMission, new Handlers.Game.Setup());

            //
            AddGameHandler(Enums.GameSubs.Heal, new Handlers.Game.Ingame.Heal());
            AddGameHandler(Enums.GameSubs.WeaponSwapping, new Handlers.Game.Ingame.WeaponSwitch());
        }

        private void AddGameHandler(Enums.GameSubs subId, GameDataHandler handler)
        {
            if (!gameHandlers.ContainsKey((ushort)subId))
            {
                gameHandlers.Add((ushort)subId, handler);
            }
        }

        public GameDataHandler GetHandler(ushort id)
        {
            if (gameHandlers.ContainsKey(id))
            {
                return gameHandlers[id];
            }

            return null;
        }

        private static NetworkTable instance = null;
        public static NetworkTable Instance { get { if (instance == null) { instance = new NetworkTable(); } return instance; } }

    }
}
