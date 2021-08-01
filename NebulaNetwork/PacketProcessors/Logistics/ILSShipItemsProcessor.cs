﻿using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Logistics;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Logistics
{
    [RegisterPacketProcessor]
    class ILSShipItemsProcessor : PacketProcessor<ILSShipItems>
    {
        private PlayerManager playerManager;
        public ILSShipItemsProcessor()
        {
            playerManager = MultiplayerHostSession.Instance != null ? MultiplayerHostSession.Instance.PlayerManager : null;
        }
        public override void ProcessPacket(ILSShipItems packet, NetworkConnection conn)
        {
            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
            }

            // TODO: Shouldn't we call this also on host ??
            if (IsClient)
            {
                SimulatedWorld.OnILSShipItemsUpdate(packet);
            }
        }
    }
}
