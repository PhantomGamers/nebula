﻿using Mirror;
using NebulaModel.Networking.Serialization;
using System;

namespace NebulaModel.Networking
{
    public static class NebulaConnection
    {
        public struct NebulaMessage : NetworkMessage
        {
            public ArraySegment<byte> Payload;
        }
        public static NetPacketProcessor PacketProcessor { get; set; }
        public static void SendPacket<T>(this NetworkConnection connection, T packet) where T : class, new()
        {
            NebulaModel.Logger.Log.Info($"Sending packet of type {packet.GetType()}");
            NebulaMessage msg = new NebulaMessage()
            {
                Payload = new ArraySegment<byte>(PacketProcessor.Write(packet))
            };
            connection.Send(msg);
        }
    }
}
