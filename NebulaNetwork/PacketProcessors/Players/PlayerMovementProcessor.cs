﻿using NebulaModel.Attributes;
using Mirror;
using NebulaModel.Packets;
using NebulaModel.Packets.Players;
using NebulaWorld;

namespace NebulaNetwork.PacketProcessors.Players
{
    [RegisterPacketProcessor]
    public class PlayerMovementProcessor : PacketProcessor<PlayerMovement>
    {
        private PlayerManager playerManager;

        public PlayerMovementProcessor()
        {
            playerManager = MultiplayerHostSession.Instance != null ? MultiplayerHostSession.Instance.PlayerManager : null;
        }

        public override void ProcessPacket(PlayerMovement packet, NetworkConnection conn)
        {
            bool valid = true;
            if (IsHost)
            {
                Player player = playerManager.GetPlayer(conn);
                if (player != null)
                {
                    player.Data.LocalPlanetId = packet.LocalPlanetId;
                    player.Data.UPosition = packet.UPosition;
                    player.Data.Rotation = packet.Rotation;
                    player.Data.BodyRotation = packet.BodyRotation;
                    player.Data.LocalPlanetPosition = packet.LocalPlanetPosition;

                    playerManager.SendPacketToOtherPlayers(packet, player);
                }
                else
                {
                    valid = false;
                }
            }

            if (valid)
            {
                SimulatedWorld.UpdateRemotePlayerPosition(packet);
            }
        }
    }
}
