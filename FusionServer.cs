using System;
using Dissonance.Networking;
using JetBrains.Annotations;

namespace Dissonance.Integrations.PhotonFusion
{
    public class FusionServer
        : BaseServer<FusionServer, FusionClient, FusionPeer>
    {
        [NotNull] private readonly FusionCommsNetwork _network;

        public FusionServer([NotNull] FusionCommsNetwork network)
        {
            _network = network;
        }

        protected override void ReadMessages()
        {
            _network.ReadServerMessages(this);
        }

        protected override void SendReliable(FusionPeer connection, ArraySegment<byte> packet)
        {
            _network.SendToClient(connection, packet, true);
        }

        protected override void SendUnreliable(FusionPeer connection, ArraySegment<byte> packet)
        {
            _network.SendToClient(connection, packet, false);
        }

        internal new void ClientDisconnected(FusionPeer connection)
        {
            base.ClientDisconnected(connection);
        }
    }
}
