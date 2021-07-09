using UnityEngine;

namespace CenturionRex.IO
{
    public static class GameIO
    {
        class GameStateData
        {
            public class UnitData{

            }

            public class BuildingData{

            }

            public class ResourceData{

            }

            public class GameplayData{

            }

            [SerializeField] public UnitData unitData;
            [SerializeField] public BuildingData buildingData;
            [SerializeField] public ResourceData resourceData;
            [SerializeField] public GameplayData gameplayData; 
        }

        static GameStateData gameStateData = new GameStateData();

        public static void Save()
        {
            GameStateIO.SaveGameProcessor.Save(gameStateData);
            Debug.Log("SAVE");
        }

        public static void Load()
        {
            if (GameStateIO.SaveGameProcessor.IsHashValid())
            {
                gameStateData = GameStateIO.SaveGameProcessor.Load<GameStateData>();
            }
            else
            {
                gameStateData = default(GameStateData);
            }

            //APPLY
            Debug.Log("LOAD");
        }
    }
}