﻿using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    class DysonSphereRemoveLayerProcessor : PacketProcessor<DysonSphereRemoveLayerPacket>
    {
        private PlayerManager playerManager;

        public DysonSphereRemoveLayerProcessor()
        {
            playerManager = MultiplayerHostSession.Instance != null ? MultiplayerHostSession.Instance.PlayerManager : null;
        }

        public override void ProcessPacket(DysonSphereRemoveLayerPacket packet, NetworkConnection conn)
        {
            bool valid = true;
            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                    playerManager.SendPacketToOtherPlayers(packet, player);
                else
                    valid = false;
            }

            if (valid)
            {
                using (DysonSphereManager.IsIncomingRequest.On())
                {
                    GameMain.data.dysonSpheres[packet.StarIndex]?.RemoveLayer(packet.LayerId);
                }
            }
        }
    }
}
