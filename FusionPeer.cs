using System;
using Fusion;

namespace Dissonance.Integrations.PhotonFusion
{
    /// <summary>
    /// Represents another peer in a Fusion/Dissonance voice chat session
    /// </summary>
    public readonly struct FusionPeer
        : IEquatable<FusionPeer>
    {
        public readonly PlayerRef PlayerRef;
        public readonly bool IsLoopback;

        public FusionPeer(PlayerRef playerRef, bool loopback)
        {
            PlayerRef = playerRef;
            IsLoopback = loopback;
        }

        public bool Equals(FusionPeer other)
        {
            return PlayerRef == other.PlayerRef
                && IsLoopback == other.IsLoopback;
        }

        public override bool Equals(object obj)
        {
            return obj is FusionPeer other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return PlayerRef.GetHashCode()
                    + (IsLoopback ? 1 : 0);
            }
        }
    }
}
