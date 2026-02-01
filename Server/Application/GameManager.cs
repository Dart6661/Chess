using System.Collections.Concurrent;

namespace Chess.Server
{
    internal class GameManager
    {
        private readonly ConcurrentDictionary<string, ConnectedUser> connectedUsers = [];
        private readonly ConcurrentDictionary<string, ConnectedUser> playingUsers = [];
        private readonly ConcurrentDictionary<string, ConnectedUser> waitingRandomUsers = [];
        private readonly ConcurrentDictionary<string, ConnectedUser> waitingSpecificUsers = [];
        private readonly ConcurrentDictionary<string, GameSession> activeSessions = [];

        internal void AddUser(ConnectedUser user, UserType type)
        {
            switch (type) 
            {
                case (UserType.ConnectedUser): connectedUsers.TryAdd(user.Id, user); break;
                case (UserType.PlayingUser): playingUsers.TryAdd(user.Id, user); break;
                case (UserType.WaitingRandomUser): waitingRandomUsers.TryAdd(user.Id, user); break;
                case (UserType.WaitingSpecificUser): waitingSpecificUsers.TryAdd(user.Id, user); break;
            }
        }

        internal void RemoveUser(ConnectedUser user, UserType type)
        {
            switch (type)
            {
                case (UserType.ConnectedUser):
                    playingUsers.TryRemove(user.Id, out _);
                    waitingRandomUsers.TryRemove(user.Id, out _);
                    waitingSpecificUsers.TryRemove(user.Id, out _);
                    connectedUsers.TryRemove(user.Id, out _);
                    break;
                case (UserType.PlayingUser): playingUsers.TryRemove(user.Id, out _); break; 
                case (UserType.WaitingRandomUser): waitingRandomUsers.TryRemove(user.Id, out _); break;
                case (UserType.WaitingSpecificUser): waitingSpecificUsers.TryRemove(user.Id, out _); break;
            }
        }

        internal ConnectedUser? GetUser(string userId, UserType type)
        {
            ConnectedUser? user;
            switch (type)
            {
                case (UserType.ConnectedUser): connectedUsers.TryGetValue(userId, out user); break;
                case (UserType.PlayingUser): playingUsers.TryGetValue(userId, out user); break;
                case (UserType.WaitingRandomUser): waitingRandomUsers.TryGetValue(userId, out user); break;
                case (UserType.WaitingSpecificUser): waitingSpecificUsers.TryGetValue(userId, out user); break;
                case (_): throw new NotImplementedException();
            }
            return user;
        }

        internal ConnectedUser? GetAnyUser(UserType type)
        {
            switch (type) 
            {
                case (UserType.ConnectedUser): 
                    if (!UsersListEmpty(UserType.ConnectedUser)) return connectedUsers.First().Value; 
                    return null;
                case (UserType.PlayingUser): 
                    if (!UsersListEmpty(UserType.PlayingUser)) return playingUsers.First().Value; 
                    return null;
                case (UserType.WaitingRandomUser): 
                    if (!UsersListEmpty(UserType.WaitingRandomUser)) return waitingRandomUsers.First().Value; 
                    return null;
                case (UserType.WaitingSpecificUser): 
                    if (!UsersListEmpty(UserType.WaitingSpecificUser)) return waitingSpecificUsers.First().Value; 
                    return null;
                case (_): throw new NotImplementedException();
            }
        }

        internal void AddSession(GameSession session) => activeSessions.TryAdd(session.Id, session);

        internal void RemoveSession(GameSession session) => activeSessions.TryRemove(session.Id,out _);

        internal GameSession? GetSession(string sessionId)
        {
            activeSessions.TryGetValue(sessionId, out GameSession? session);
            return session;
        }

        internal bool UsersListEmpty(UserType type)
        {
            switch (type)
            {
                case (UserType.ConnectedUser): return connectedUsers.IsEmpty;
                case (UserType.PlayingUser): return playingUsers.IsEmpty;
                case (UserType.WaitingRandomUser): return waitingRandomUsers.IsEmpty;
                case (UserType.WaitingSpecificUser): return waitingSpecificUsers.IsEmpty;
                case (_): throw new NotImplementedException();
            }
        }

        internal bool ActiveSessionListEmpty() => activeSessions.IsEmpty;
    }
}
