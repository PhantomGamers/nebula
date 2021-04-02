﻿using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Planet;
using NebulaModel.Packets.Processors;
using System.Collections.Generic;

namespace NebulaHost.PacketProcessors.Planet
{
    [RegisterPacketProcessor]
    public class PlanetDataRequestProcessor : IPacketProcessor<PlanetDataRequest>
    {
        public void ProcessPacket(PlanetDataRequest packet, NebulaConnection conn)
        {
            Dictionary<int, byte[]> planetDataToReturn = new Dictionary<int, byte[]>();

            foreach (int planetId in packet.PlanetIDs)
            {
                PlanetData planet = GameMain.galaxy.PlanetById(planetId);
                Log.Info($"Returning terrain for {planet.name}");

                // NOTE: The following has been picked-n-mixed from "PlanetModelingManager.PlanetComputeThreadMain()"
                // This method is **costly** - do not run it more than is required!
                // It generates the planet on the host and then sends it to the client

                PlanetAlgorithm planetAlgorithm = PlanetModelingManager.Algorithm(planet);

                if (planet.data == null)
                {
                    planet.data = new PlanetRawData(planet.precision);
                    planet.modData = planet.data.InitModData(planet.modData);
                    planet.data.CalcVerts();
                    planet.aux = new PlanetAuxData(planet);
                    planetAlgorithm.GenerateTerrain(planet.mod_x, planet.mod_y);
                    planetAlgorithm.CalcWaterPercent();
                }

                if (planet.factory == null)
                {
                    if (planet.type != EPlanetType.Gas)
                    {
                        planetAlgorithm.GenerateVegetables();
                        planetAlgorithm.GenerateVeins(false);
                    }
                }

                using (BinaryUtils.Writer writer = new BinaryUtils.Writer())
                {
                    planet.ExportRuntime(writer.BinaryWriter);
                    planetDataToReturn.Add(planetId, writer.CloseAndGetBytes());
                }
            }

            conn.SendPacket(new PlanetDataResponse(planetDataToReturn));
        }
    }
}
