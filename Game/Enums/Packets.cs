namespace Game.Enums {
    public enum Packets : ushort {
        ServerTime = 0x6100,
        Authorization = 0x6200,
        Ping = 0x6400,
        ChannelSelection = 0x7001,
        RoomList = 0x7200,
        RoomListUpdate = 0x7210,
        RoomCreation = 0x7300,
        RoomJoin = 0x7310,
        RoomLeave = 0x7340,
        Chat = 0x7400,
        PlayerInfo = 0x7500,
        MapData = 0x7510,
        Equipment = 0x7512,
        Explosives = 0x7520,
        GamePacket = 0x7530,
        GameCountDown = 0x7531,
        GameTick = 0x7540,
        Scoreboard = 0x7550,
        EndGame = 0x7560,
        Itemshop = 0x7600,
        Markt = 0x7700,
        UpdateInventory = 0x7900,
        LevelUp = 0x7920 // Old: 0x8200
    }
}

