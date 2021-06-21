﻿using HarmonyLib;
using NebulaModel.Attributes;
using NebulaModel.Logger;
using NebulaModel.Networking;
using NebulaModel.Packets.Factory;
using NebulaModel.Packets.Processors;
using NebulaWorld.Factory;
using System.Collections.Generic;
using UnityEngine;

namespace NebulaHost.PacketProcessors.Factory.Entity
{
    [RegisterPacketProcessor]
    class CreatePrebuildsRequestProcessor : IPacketProcessor<CreatePrebuildsRequest>
    {
        public void ProcessPacket(CreatePrebuildsRequest packet, NebulaConnection conn)
        {
            PlanetData planet = GameMain.galaxy.PlanetById(packet.PlanetId);
            if (planet.factory == null)
            {
                Log.Warn($"planet.factory was null create new one");
                planet.factory = GameMain.data.GetOrCreateFactory(planet);
            }

            PlayerAction_Build pab = GameMain.mainPlayer.controller?.actionBuild;
            BuildTool[] buildTools = pab.tools;
            BuildTool buildTool = null;
            for (int i = 0; i < buildTools.Length; i++)
            {
                if (buildTools[i].GetType().ToString() == packet.BuildToolType)
                {
                    buildTool = buildTools[i];
                    break;
                }
            }
            if (pab != null && buildTool != null)
            {
                FactoryManager.TargetPlanet = packet.PlanetId;
                FactoryManager.PacketAuthor = packet.AuthorId;

                //Make backup of values that are overwritten
                List<BuildPreview> tmpList = new List<BuildPreview>();

                PlanetFactory tmpFactory = null;
                NearColliderLogic tmpNearcdLogic = null;
                PlanetPhysics tmpPlanetPhysics = null;
                float tmpBuildArea = GameMain.mainPlayer.mecha.buildArea;
                PlanetData tmpData = null;
                bool loadExternalPlanetData = GameMain.localPlanet != planet;

                //Load temporary planet data, since host is not there
                if (loadExternalPlanetData)
                {
                    tmpFactory = buildTool.factory;
                    tmpNearcdLogic = buildTool.actionBuild.nearcdLogic;
                    tmpPlanetPhysics = buildTool.actionBuild.planetPhysics;
                    tmpData = GameMain.mainPlayer.planetData;
                }

                //Create Prebuilds from incoming packet and prepare new position
                tmpList.AddRange(buildTool.buildPreviews);
                buildTool.buildPreviews.Clear();
                buildTool.buildPreviews.AddRange(packet.GetBuildPreviews());
                using (FactoryManager.EventFromServer.On())
                {
                    FactoryManager.EventFactory = planet.factory;

                    //Check if some mandatory variables are missing
                    if (planet.physics == null || planet.physics.colChunks == null)
                    {
                        planet.physics = new PlanetPhysics(planet);
                        planet.physics.Init();
                    }
                    if (planet.aux == null)
                    {
                        planet.aux = new PlanetAuxData(planet);
                    }

                    //Set temporary Local Planet / Factory data that are needed for original methods CheckBuildConditions() and CreatePrebuilds()
                    buildTool.factory = planet.factory;
                    pab.factory = planet.factory;
                    pab.noneTool.factory = planet.factory;
                    pab.planetPhysics = planet.physics;
                    pab.nearcdLogic = planet.physics.nearColliderLogic;
                    AccessTools.Property(typeof(global::Player), "planetData").SetValue(GameMain.mainPlayer, planet, null);

                    //Check if prebuilds can be build (collision check, height check, etc)
                    GameMain.mainPlayer.mecha.buildArea = float.MaxValue;
                    bool canBuild = false;
                    using (FactoryManager.IgnoreBasicBuildConditionChecks.On())
                    {
                        if (packet.BuildToolType == typeof(BuildTool_Click).ToString())
                        {
                            canBuild = ((BuildTool_Click)buildTool).CheckBuildConditions();
                        }
                        else if (packet.BuildToolType == typeof(BuildTool_Path).ToString())
                        {
                            canBuild = ((BuildTool_Path)buildTool).CheckBuildConditions();
                        }
                        else if (packet.BuildToolType == typeof(BuildTool_Inserter).ToString())
                        {
                            canBuild = ((BuildTool_Inserter)buildTool).CheckBuildConditions();
                        }
                        canBuild &= CheckBuildingConnections(buildTool.buildPreviews, planet.factory.entityPool, planet.factory.prebuildPool);
                    }

                    UnityEngine.Debug.Log(buildTool.buildPreviews[0].condition);

                    if (canBuild)
                    {
                        FactoryManager.PacketAuthor = packet.AuthorId;
                        CheckAndFixConnections(buildTool, planet);

                        if (packet.BuildToolType == typeof(BuildTool_Click).ToString())
                        {
                            ((BuildTool_Click)buildTool).CreatePrebuilds();
                        }
                        else if (packet.BuildToolType == typeof(BuildTool_Path).ToString())
                        {
                            FactoryManager.IsFromClient = true;
                            ((BuildTool_Path)buildTool).CreatePrebuilds();
                        }
                        else if (packet.BuildToolType == typeof(BuildTool_Inserter).ToString())
                        {
                            ((BuildTool_Inserter)buildTool).CreatePrebuilds();
                        }

                        FactoryManager.PacketAuthor = -1;
                    }

                    //Revert changes back to the original planet
                    if (loadExternalPlanetData)
                    {
                        planet.physics.Free();
                        planet.physics = null;
                        buildTool.factory = tmpFactory;
                        pab.factory = tmpFactory;
                        pab.noneTool.factory = tmpFactory;
                        pab.planetPhysics = tmpPlanetPhysics;
                        pab.nearcdLogic = tmpNearcdLogic;
                        AccessTools.Property(typeof(global::Player), "planetData").SetValue(GameMain.mainPlayer, tmpData, null);
                    }

                    GameMain.mainPlayer.mecha.buildArea = tmpBuildArea;
                    FactoryManager.EventFactory = null;
                }

                buildTool.buildPreviews.Clear();
                buildTool.buildPreviews.AddRange(tmpList);

                FactoryManager.TargetPlanet = FactoryManager.PLANET_NONE;
                FactoryManager.PacketAuthor = -1;
            }
        }

        public void CheckAndFixConnections(BuildTool buildTool, PlanetData planet)
        {
            //Check and fix references to prebuilds
            Vector3 tmpVector = Vector3.zero;
            foreach (BuildPreview preview in buildTool.buildPreviews)
            {
                //Check only, if buildPreview has some connection to another prebuild
                if (preview.coverObjId < 0)
                {
                    tmpVector = preview.lpos;
                    if (planet.factory.prebuildPool[-preview.coverObjId].id != 0)
                    {
                        //Prebuild exists, check if it is same prebuild that client wants by comparing prebuild positions
                        if (tmpVector == planet.factory.prebuildPool[-preview.coverObjId].pos)
                        {
                            //Position of prebuilds are same, everything is OK.
                            continue;
                        }
                    }
                    // Prebuild does not exists, check what is the new ID of the finished building that was constructed from prebuild
                    // or
                    // Positions of prebuilds are different, which means this is different prebuild and we need to find ID of contructed building
                    foreach (EntityData entity in planet.factory.entityPool)
                    {
                        // `entity.pos == tmpVector` does not work in every cases (rounding errors?).
                        if ((entity.pos - tmpVector).sqrMagnitude < 0.1f)
                        {
                            preview.coverObjId = entity.id;
                            break;
                        }
                    }
                }
            }
        }

        public bool CheckBuildingConnections(List<BuildPreview> buildPreviews, EntityData[] entityPool, PrebuildData[] prebuildPool)
        {
            //Check if some entity that is suppose to be connected to this building is missing
            for (int i = 0; i < buildPreviews.Count; i++)
            {
                var buildPreview = buildPreviews[i];
                int inputObjId = buildPreview.inputObjId;
                if (inputObjId > 0)
                {
                    if (inputObjId >= entityPool.Length || entityPool[inputObjId].id == 0)
                    {
                        return false;
                    }
                }
                else if (inputObjId < 0)
                {
                    inputObjId = -inputObjId;
                    if (inputObjId >= prebuildPool.Length || prebuildPool[inputObjId].id == 0)
                    {
                        return false;
                    }
                }

                int outputObjId = buildPreview.outputObjId;
                if (outputObjId > 0)
                {
                    if (outputObjId >= entityPool.Length || entityPool[outputObjId].id == 0)
                    {
                        return false;
                    }
                }
                else if (outputObjId < 0)
                {
                    outputObjId = -outputObjId;
                    if (outputObjId >= prebuildPool.Length || prebuildPool[outputObjId].id == 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
