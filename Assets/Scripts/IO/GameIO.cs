using UnityEngine;

namespace CenturionRex.IO
{
    public static class GameIO
    {
        [System.Serializable]
        class GameStateData
        {
            [System.Serializable]
            public class UnitData{

            }

            [System.Serializable]
            public class BuildingData{

            }

            [System.Serializable]
            public class ResourceData{
                public float wine = 0;
            }

            [System.Serializable]
            public class GameplayData{

            }

            [SerializeField] public UnitData unitData = new UnitData();
            [SerializeField] public BuildingData buildingData = new BuildingData();
            [SerializeField] public ResourceData resourceData = new ResourceData();
            [SerializeField] public GameplayData gameplayData = new GameplayData(); 
        }


        public static bool Save()
        {
            if(!GlobalManager.IsGame) {
                Debug.LogError("Cant save when not in game");
                return false;
            }

            GameStateData gameStateData = Pack();
            GameStateIO.SaveGameProcessor.Save(gameStateData);
            Debug.Log("SAVE");

            return true;
        }

        public static bool Load()
        {
            if(!GlobalManager.IsGame) {
                Debug.LogError("Cant save when not in game");
                return false;
            }

            GameStateData gameStateData = new GameStateData();

            if (GameStateIO.SaveGameProcessor.IsHashValid())
            {
                gameStateData = GameStateIO.SaveGameProcessor.Load<GameStateData>();
            }
            else
            {
                gameStateData = default(GameStateData);
            }

            Unpack(gameStateData);
            Debug.Log("LOAD");
            return true;
        }

        private static GameStateData Pack(){

            GameStateData gameStateData = new GameStateData();

            Player_Controller playerController = GameObject.FindObjectOfType<Player_Controller>();
            gameStateData.resourceData.wine = playerController.currentResources;

            return gameStateData;
        }

        private static void Unpack(GameStateData gameStateData)
        {
            Player_Controller playerController = GameObject.FindObjectOfType<Player_Controller>();
            playerController.currentResources = gameStateData.resourceData.wine;
        }
    }
}