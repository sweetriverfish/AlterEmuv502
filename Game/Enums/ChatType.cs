namespace Game.Enums {
    public enum ChatType : byte {
        Notice1 = 1,
        Notice2,
        LobbyToChannel,
        RoomToAll,
        RoomToTeam,
        Whisper,
        LobbyToAll = 8,
        Clan = 10
    }
}
