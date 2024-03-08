using System;
using Dissonance.Extensions;
using Fusion;
using JetBrains.Annotations;
using UnityEngine;

namespace Dissonance.Integrations.PhotonFusion
{
    /// <summary>
    /// Handles sending and receiving RPCs for Dissonance FusionCommsNetwork
    /// </summary>
    internal class RpcHandler
        : SimulationBehaviour
    {
        private static FusionCommsNetwork _instance;
        private static NetworkRunner _runner;

        public static void Initialize(FusionCommsNetwork instance, NetworkRunner runner)
        {
            Debug.LogWarning("Initalizing ");
            _instance = instance;
            _runner = runner;
        }

        [NotNull] private static T[] ConvertToArray<T>(ArraySegment<T> segment)
            where T : struct
        {
            var arr = new T[segment.Count];
            segment.CopyToSegment(arr);
            return arr;
        }

        public static void SendToServer(FusionCommsNetwork instance, ArraySegment<byte> packet, bool reliable)
        {
            if (_instance != instance)
                throw new InvalidOperationException("Cannot send from mismatched instance");

            if (reliable)
                SendToServerReliableRPC(_runner, ConvertToArray(packet));
            else
                SendToServerUnreliableRPC(_runner, ConvertToArray(packet));
        }

        public static bool SendToClient(FusionCommsNetwork instance, FusionPeer dest, ArraySegment<byte> packet, bool reliable)
        {
            if (_instance != instance)
                throw new InvalidOperationException("Cannot send from mismatched instance");
            if (_runner.GetRpcTargetStatus(dest.PlayerRef) == RpcTargetStatus.Unreachable)
                return false;

            if (reliable)
                SendToClientReliableRPC(_runner, ConvertToArray(packet), dest.PlayerRef);
            else
                SendToClientUnreliableRPC(_runner, ConvertToArray(packet), dest.PlayerRef);

            return true;
        }

        // ReSharper disable UnusedParameter.Local
        #pragma warning disable IDE0060
        // Justification: RPCs (defined below this point) have some "unused" parameters, which are required to define an RPC (e.g. NetworkRunner and PlayerRef)

        [Rpc(InvokeLocal = false, InvokeResim = false, TickAligned = false, Channel = RpcChannel.Reliable, HostMode = RpcHostMode.SourceIsHostPlayer)]
        // ReSharper disable once UnusedParameter.Local (required part of RPC signature)
        private static void SendToServerReliableRPC(NetworkRunner runner, byte[] data, RpcInfo info = default)
        {
            if (_instance == null)
            {
                Debug.LogWarning("Received Dissonance message, but there is no Dissonance comms network");
                return;
            }

            _instance.DeliverMessageToServer(data, info.Source);
        }

        [Rpc(InvokeLocal = false, InvokeResim = false, TickAligned = false, Channel = RpcChannel.Unreliable, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private static void SendToServerUnreliableRPC(NetworkRunner runner, byte[] data, RpcInfo info = default)
        {
            if (_instance == null)
            {
                Debug.LogWarning("Received Dissonance message, but there is no Dissonance comms network");
                return;
            }

            _instance.DeliverMessageToServer(data, info.Source);
        }


        [Rpc(InvokeLocal = false, InvokeResim = false, TickAligned = false, Channel = RpcChannel.Reliable, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private static void SendToClientReliableRPC(NetworkRunner runner, byte[] data, [RpcTarget] PlayerRef player)
        {
            if (_instance == null)
            {
                Debug.LogWarning("Received Dissonance message, but there is no Dissonance comms network");
                return;
            }

            _instance.DeliverMessageToClient(data);
        }

        [Rpc(InvokeLocal = false, InvokeResim = false, TickAligned = false, Channel = RpcChannel.Unreliable, HostMode = RpcHostMode.SourceIsHostPlayer)]
        private static void SendToClientUnreliableRPC(NetworkRunner runner, byte[] data, [RpcTarget] PlayerRef player)
        {
            if (_instance == null)
            {
                Debug.LogWarning("Received Dissonance message, but there is no Dissonance comms network");
                return;
            }

            _instance.DeliverMessageToClient(data);
        }
    }
}
