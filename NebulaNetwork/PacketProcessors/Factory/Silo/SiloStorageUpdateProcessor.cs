﻿#region

using NebulaAPI;
using NebulaModel.Networking;
using NebulaModel.Packets;
using NebulaModel.Packets.Factory.Silo;

#endregion

namespace NebulaNetwork.PacketProcessors.Factory.Silo;

[RegisterPacketProcessor]
internal class SiloStorageUpdateProcessor : PacketProcessor<SiloStorageUpdatePacket>
{
    public override void ProcessPacket(SiloStorageUpdatePacket packet, NebulaConnection conn)
    {
        var pool = GameMain.galaxy.PlanetById(packet.PlanetId)?.factory?.factorySystem?.siloPool;
        if (pool != null && packet.SiloIndex != -1 && packet.SiloIndex < pool.Length && pool[packet.SiloIndex].id != -1)
        {
            pool[packet.SiloIndex].bulletCount = packet.ItemCount;
            pool[packet.SiloIndex].bulletInc = packet.ItemInc;
        }
    }
}
