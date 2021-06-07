﻿using HarmonyLib;
using NebulaModel.Attributes;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld;
using NebulaWorld.Factory;
using System.Collections.Generic;

namespace NebulaClient.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    class CreatePrebuildsRequestProcessor : IPacketProcessor<CreatePrebuildsRequest>
    {
        public void ProcessPacket(CreatePrebuildsRequest packet, NebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            if (planet.factory == null)
            {
                // We only execute the code if the client has loaded the factory at least once.
                // Else it will get it once it goes to the planet for the first time. 
                return;
            }

            PlayerAction_Build pab = GameMain.mainPlayer.controller?.actionBuild;
            BuildTool_Click btc = GameMain.mainPlayer.controller?.actionBuild.clickTool;
            if (pab != null && btc != null)
            {
                FactoryManager.TargetPlanet = packet.PlanetId;

                //Make backup of values that are overwritten
                List<BuildPreview> tmpList = btc.buildPreviews;
                //bool tmpConfirm = pab.waitConfirm;
                //UnityEngine.Vector3 tmpPos = pab.previewPose.position;
                //UnityEngine.Quaternion tmpRot = pab.previewPose.rotation;
                PlanetFactory tmpFactory = btc.factory;
                PlanetPhysics tmpPlanetPhysics = (PlanetPhysics)AccessTools.Field(typeof(PlayerAction_Build), "planetPhysics").GetValue(pab);

                //Create Prebuilds from incomming packet
                btc.buildPreviews.Clear();
                btc.buildPreviews.AddRange(packet.GetBuildPreviews());
                //pab.waitConfirm = true;
                using (FactoryManager.EventFromServer.On())
                {
                    FactoryManager.EventFactory = planet.factory;
                    //pab.previewPose.position = new UnityEngine.Vector3(packet.PosePosition.x, packet.PosePosition.y, packet.PosePosition.z);
                    //pab.previewPose.rotation = new UnityEngine.Quaternion(packet.PoseRotation.x, packet.PoseRotation.y, packet.PoseRotation.z, packet.PoseRotation.w);
                    btc.factory = planet.factory;

                    //Create temporary physics for spawning building's colliders
                    if (planet.physics == null || planet.physics.colChunks == null)
                    {
                        planet.physics = new PlanetPhysics(planet);
                        planet.physics.Init();
                    }

                    //Take item from the inventory if player is author of the build
                    if (packet.AuthorId == LocalPlayer.PlayerId)
                    {
                        foreach (BuildPreview buildPreview in btc.buildPreviews)
                        {
                            if (GameMain.mainPlayer.inhandItemId == buildPreview.item.ID && GameMain.mainPlayer.inhandItemCount > 0)
                            {
                                GameMain.mainPlayer.UseHandItems(1);
                            }
                            else
                            {
                                int num = 1;
                                GameMain.mainPlayer.package.TakeTailItems(ref buildPreview.item.ID, ref num, false);
                            }
                        }
                    }

                    AccessTools.Field(typeof(PlayerAction_Build), "planetPhysics").SetValue(GameMain.mainPlayer.controller.actionBuild, planet.physics);
                    btc.CreatePrebuilds();
                    FactoryManager.EventFactory = null;
                }

                // TODO: FIX THE BELOW

                //Author has to call this for the continuous belt building
/*                if (packet.AuthorId == LocalPlayer.PlayerId)
                {
                    pab.AfterPrebuild();
                }*/

                /* TODO: Oh *boy* is this one going to be a fun one to fix - they've actually refactored the AfterPrebuild method
                 * to happen at each of the relevant times the cmd.mode condition would have been met in every tool 
                 *
                 * Cod's Suggestion: Transpile everything :) */

                //Revert changes back
                AccessTools.Field(typeof(PlayerAction_Build), "planetPhysics").SetValue(GameMain.mainPlayer.controller.actionBuild, tmpPlanetPhysics);
                btc.factory = tmpFactory;
                //pab.waitConfirm = tmpConfirm;
                //pab.previewPose.position = tmpPos;
                //pab.previewPose.rotation = tmpRot;
                btc.buildPreviews.Clear();
                btc.buildPreviews.AddRange(tmpList);

                FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
            }
        }
    }
}
