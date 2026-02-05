using System.Text.Json.Serialization;

namespace Chess.Shared
{
    public record UserDataDto(string UserId, string? SessionId, Color? Color, string? OtherUserId);

    public record UserIdDto(string UserId);
    
    public record ChessMoveDto(int A, int B, int X, int Y, params MoveOptionDto[] Options);
    
    public record FigureDto(int A, int B, int AmountMovesOfFigure, FigureType Title, Color Color);

    public record PlayerDto(int AmountMovesOfPlayer, Color Color);

    public record FieldDto(List<FigureDto> Figures, int AmountMovesOnField);

    public record GameHandlerDto(PlayerDto WhitePlayer, PlayerDto BlackPlayer, string WhitePlayerId, string BlackPlayerId, FieldDto Field, 
                                 Color ColorOfMovingPlayer, Color? ColorOfCurrentPlayer, DateTime StartTime, DateTime? EndTime);

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(ReplacementOptionDto), "replacement")]
    public record MoveOptionDto();

    public record ReplacementOptionDto(string SelectedFigureType) : MoveOptionDto;
}
