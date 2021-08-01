﻿using NebulaModel.Attributes;
using NebulaModel.DataStructures;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Universe;
using NebulaWorld.Universe;

namespace NebulaNetwork.PacketProcessors.Universe
{
    [RegisterPacketProcessor]
    public class DysonSphereAddLayerProcessor : PacketProcessor<DysonSphereAddLayerPacket>
    {
        private PlayerManager playerManager;

        public DysonSphereAddLayerProcessor()
        {
            playerManager = MultiplayerHostSession.Instance != null ? MultiplayerHostSession.Instance.PlayerManager : null;
        }

        public override void ProcessPacket(DysonSphereAddLayerPacket packet, NetworkConnection conn)
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
                    GameMain.data.dysonSpheres[packet.StarIndex]?.AddLayer(packet.OrbitRadius, DataStructureExtensions.ToQuaternion(packet.OrbitRotation), packet.OrbitAngularSpeed);
                }
            }
        }
    }
}
