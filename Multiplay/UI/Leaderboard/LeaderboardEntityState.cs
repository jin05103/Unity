using System;
using Unity.Collections;
using Unity.Netcode;

public struct LeaderboardEntityState : INetworkSerializable, IEquatable<LeaderboardEntityState>
{
    public ulong clientId;
    public FixedString32Bytes PlayerName;
    public int Coins;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref Coins);
    }

    public bool Equals(LeaderboardEntityState other)
    {
        return clientId == other.clientId &&
                PlayerName.Equals(other.PlayerName) &&
                Coins == other.Coins;
    }
}