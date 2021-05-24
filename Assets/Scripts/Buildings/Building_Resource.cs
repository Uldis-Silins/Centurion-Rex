using System.Collections.Generic;
using UnityEngine;

public class Building_Resource : Building_Base
{
    [System.Serializable]
    public class FactionSprite
    {
        public OwnerType type;
        public Sprite sprite;
    }

    public List<Player_Controller> playerControllers;
    public SelectableManager selectableManager;

    public FactionSprite[] factionSprites;

    public OwnerType ownerFaction;
    public GameObject fovObject;
    public float captureRadius = 5f;

    public float productionPerSecond;

    private Player_Controller m_currentPlayerController;
    private Player_Controller.Building m_building;

    private float m_resourceTimer;

    protected override void Awake()
    {
        base.Awake();
        m_building = new Player_Controller.Building(this, BuildingType.ResourceProduction);
    }

    private void Start()
    {
        if(ownerFaction == OwnerType.None)
        {
            selectableManager.RegisterSelectable(this);
        }

        if(ownerFaction != OwnerType.Player)
        {
            fovObject.SetActive(false);
        }

        if(ownerFaction != OwnerType.None)
        {
            SetOwner(ownerFaction);
        }
    }

    private void Update()
    {
        if (Player_Controller.currentGameState != GameState.Playing) return;

        if(m_currentPlayerController != null && m_resourceTimer <= 0f)
        {
            m_currentPlayerController.AddResource(productionPerSecond);
            m_resourceTimer = 1f;
        }

        m_resourceTimer -= Time.deltaTime;
    }

    public void SetOwner(OwnerType faction)
    {
        if (ownerFaction == faction) return;

        if(m_currentPlayerController != null)
        {
            for (int i = m_currentPlayerController.ownedBuildings.Count - 1; i >= 0; i--)
            {
                if(m_currentPlayerController.ownedBuildings[i].selectable == (this as ISelecteble))
                {
                    m_currentPlayerController.ownedBuildings.RemoveAt(i);
                    break;
                }
            }
        }

        for (int i = 0; i < playerControllers.Count; i++)
        {
            if(playerControllers[i].ownedByPlayer && faction == OwnerType.Player)
            {
                fovObject.SetActive(true);
                m_currentPlayerController = playerControllers[i];
            }
            else if(!playerControllers[i].ownedByPlayer && faction == OwnerType.Enemy)
            {
                fovObject.SetActive(false);
                m_currentPlayerController = playerControllers[i];
            }
        }

        if (!m_currentPlayerController.ownedBuildings.Contains(m_building))
        {
            m_currentPlayerController.ownedBuildings.Add(m_building);
        }

        ownerFaction = faction;
        spriteRenderer.sprite = GetFactionSprite(faction);
    }

    private Sprite GetFactionSprite(OwnerType type)
    {
        for (int i = 0; i < factionSprites.Length; i++)
        {
            if(factionSprites[i].type == type)
            {
                return factionSprites[i].sprite;
            }
        }

        return null;
    }
}