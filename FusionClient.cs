using System;
using Dissonance.Networking;
using JetBrains.Annotations;

namespace Dissonance.Integrations.PhotonFusion
{
    public class FusionClient
        : BaseClient<FusionServer, FusionClient, FusionPeer>
    {
        [NotNull] private readonly FusionCommsNetwork _network;

        public FusionClient([NotNull] FusionCommsNetwork network)
            : base(network)
        {
            _network = network;
        }

        public override void Connect()
        {
            Connected();
        }

        protected override void ReadMessages()
        {
            _network.ReadClientMessages(this);
        }

        protected override void SendReliable(ArraySegment<byte> packet)
        {
            _network.SendToServer(packet, true);
        }

        protected override void SendUnreliable(ArraySegment<byte> packet)
        {
            _network.SendToServer(packet, false);
        }
    }
}
