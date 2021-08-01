﻿using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Trash;
using NebulaWorld.Trash;

namespace NebulaNetwork.PacketProcessors.Trash
{
    [RegisterPacketProcessor]
    class TrashSystemTrashRemovedProcessor : PacketProcessor<TrashSystemTrashRemovedPacket>
    {
        private PlayerManager playerManager;

        public TrashSystemTrashRemovedProcessor()
        {
            playerManager = MultiplayerHostSession.Instance != null ? MultiplayerHostSession.Instance.PlayerManager : null;
        }

        public override void ProcessPacket(TrashSystemTrashRemovedPacket packet, NetworkConnection conn)
        {
            bool valid = true;

            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
                else
                {
                    valid = false;
                }
            }

            if (valid)
            {
                using (TrashManager.RemoveTrashFromOtherPlayers.On())
                {
                    GameMain.data.trashSystem.container.RemoveTrash(packet.TrashId);
                }
            }
        }
    }
}