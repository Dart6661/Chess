namespace Chess.Shared
{
    public static class Mapper 
    {
        public static FigureDto FigureToDto(Figure figure) => new(figure.A, figure.B, figure.AmountMovesOfFigure, figure.Title, figure.Color);

        public static PlayerDto PlayerToDto(Player player) => new(player.AmountMovesOfPlayer, player.Color);

        public static FieldDto FieldToDto(Field field)
        {
            List<FigureDto> figuresDto = [];
            for (int i = 0; i < 8;  i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    Figure? cell = field.GetCell(i, j);
                    if (cell != null) figuresDto.Add(FigureToDto(cell));
                }
            }
            return new(figuresDto, field.AmountMovesOnField);
        }

        public static GameHandlerDto GameHandlerToDto(GameHandler gameHandler, string WhitePlayerId, string BlackPlayerId, Color? currentPlayerColor = null) => new(
            PlayerToDto(gameHandler.whitePlayer),
            PlayerToDto(gameHandler.blackPlayer),
            WhitePlayerId,
            BlackPlayerId,
            FieldToDto(gameHandler.field),
            gameHandler.GetMovingPlayer().Color,
            currentPlayerColor,
            gameHandler.StartTime, 
            gameHandler.EndTime
        );

        public static MoveOption DtoToMoveOption(MoveOptionDto moveOptionDto)
        {
            return moveOptionDto switch
            {
                ReplacementOptionDto replacementOptionDto => new ReplacementOption(Type.GetType(replacementOptionDto.SelectedFigureType) 
                    ?? throw new InvalidOperationException($"cannot resolve type '{replacementOptionDto.SelectedFigureType}'")),
                _ => throw new NotSupportedException( $"unknown MoveOptionDto type: {moveOptionDto.GetType().Name}")
            };
        }
    }
}
