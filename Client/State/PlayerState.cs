namespace Chess.Client.Cli
{
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
}