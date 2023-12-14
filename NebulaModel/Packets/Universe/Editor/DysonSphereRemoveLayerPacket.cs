﻿namespace NebulaModel.Packets.Universe;

public class DysonSphereRemoveLayerPacket
{
    public DysonSphereRemoveLayerPacket() { }

    public DysonSphereRemoveLayerPacket(int starIndex, int layerId)
    {
        StarIndex = starIndex;
        LayerId = layerId;
    }

    public int StarIndex { get; set; }
    public int LayerId { get; set; }
}
