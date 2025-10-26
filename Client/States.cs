namespace Chess.Client.Cli
{
    internal enum PlayerStatus
    {
        Idle,
        WaitingRandomSession,
        WaitingTargetSession,
        Playing
    }


    internal class PlayerState
    {
        internal string Id { get; init; }
        internal PlayerStatus Status { get; set; }
        internal Session? Session { get; set; }

        internal PlayerState(string id)
        {
            Id = id;
            Status = PlayerStatus.Idle;
        }
    }

    
    internal class Session(string id, Color ownColor, Color otherPlayerColor, string otherPlayerId, GameHandlerDto gameHandlerDto)
    {
        internal string Id { get; init; } = id;
        internal string OtherPlayerId { get; init; } = otherPlayerId;
        internal PlayerDto WhitePlayer { get; init; } = gameHandlerDto.WhitePlayer;
        internal PlayerDto BlackPlayer { get; init; } = gameHandlerDto.BlackPlayer;
        internal Color OwnColor { get; init; } = ownColor;
        internal Color OtherPlayerColor { get; init; } = otherPlayerColor;
        internal GameHandlerDto GameHandlerDto { get; set; } = gameHandlerDto;
        internal List<FigureDto> Figures { get; set; } = gameHandlerDto.Field.Figures;
        internal int AmountMovesOnField { get; set; } = gameHandlerDto.Field.AmountMovesOnField;

        internal void UpdateSession(GameHandlerDto gameHandlerDto)
        {
            GameHandlerDto = gameHandlerDto;
            Figures = gameHandlerDto.Field.Figures;
            AmountMovesOnField = gameHandlerDto.Field.AmountMovesOnField;
        }
    }
}
