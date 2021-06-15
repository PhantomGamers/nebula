﻿namespace NebulaModel.Packets.Factory
{
    public class PasteBuildingSettingUpdate
    {
        /*        public int EntityId { get; set; }
                public int RecipeId { get; set; }
                public bool ResearchMode { get; set; }
                public int FilterId { get; set; }
                public int OrbitId { get; set; }
                public int Mode { get; set; }
                public ERecipeType RecipeType { get; set; }*/
        public int ObjectId { get; set; }
        public BuildingType Type { get; set; }
        public int ItemId { get; set; }
        public int ModelIndex { get; set; }
        public float Yaw { get; set; }
        public ERecipeType RecipeType { get; set; }
        public int RecipeId { get; set; }
        public int FilterId { get; set; }
        public int Mode0 { get; set; }
        public int Mode1 { get; set; }
        public int Mode2 { get; set; }
        public int Mode3 { get; set; }
        public int[] Parameters { get; set; }
        public int[] InserterItemIds { get; set; }
        public int[] InserterLengths { get; set; }
        public int[] InserterFilters { get; set; }
        public int PlanetId { get; set; }

        public PasteBuildingSettingUpdate() { }

        public PasteBuildingSettingUpdate(int objectId, BuildingParameters clipboard, int planetId)
        {
            ObjectId = objectId;
            Type = clipboard.type;
            ItemId = clipboard.itemId;
            ModelIndex = clipboard.modelIndex;
            Yaw = clipboard.yaw;
            RecipeType = clipboard.recipeType;
            RecipeId = clipboard.recipeId;
            FilterId = clipboard.filterId;
            Mode0 = clipboard.mode0;
            Mode1 = clipboard.mode1;
            Mode2 = clipboard.mode2;
            Mode3 = clipboard.mode3;
            Parameters = clipboard.parameters;
            InserterItemIds = clipboard.inserterItemIds;
            InserterLengths = clipboard.inserterLengths;
            InserterFilters = clipboard.inserterFilters;
            PlanetId = planetId;
        }

        public BuildingParameters GetBuildingSettings()
        {
            BuildingParameters result = new BuildingParameters();
            result.type = Type;
            result.itemId = ItemId;
            result.modelIndex = ModelIndex;
            result.yaw = Yaw;
            result.recipeType = RecipeType;
            result.recipeId = RecipeId;
            result.filterId = FilterId;
            result.mode0 = Mode0;
            result.mode1 = Mode1;
            result.mode2 = Mode2;
            result.mode3 = Mode3;
            result.parameters = Parameters;
            result.inserterItemIds = InserterItemIds;
            result.inserterLengths = InserterLengths;
            result.inserterFilters = InserterFilters;
            return result;
        }
    }
}
