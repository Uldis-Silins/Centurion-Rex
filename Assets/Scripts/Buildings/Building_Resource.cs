using System.Collections.Generic;
using UnityEngine;

public class Building_Resource : MonoBehaviour
{
    public List<Player_Controller> playerControllers;

    public FactionType ownerFaction;
    public GameObject fovObject;
    public float captureRadius = 5f;

    public float productionPerSecond;

    private Player_Controller m_currentPlayerController;

    private float m_resourceTimer;

    private void Start()
    {
        if(ownerFaction != FactionType.Player)
        {
            fovObject.SetActive(false);
        }

        if(ownerFaction != FactionType.None)
        {
            SetOwner(ownerFaction);
        }
    }

    private void Update()
    {
        if(ownerFaction == FactionType.Player && m_resourceTimer <= 0f)
        {
            m_currentPlayerController.AddResource(productionPerSecond);
            m_resourceTimer = 1f;
        }

        m_resourceTimer -= Time.deltaTime;
    }

    public void  SetOwner(FactionType faction)
    {
        for (int i = 0; i < playerControllers.Count; i++)
        {
            if(playerControllers[i].ownedByPlayer && faction == FactionType.Player)
            {
                fovObject.SetActive(true);
                m_currentPlayerController = playerControllers[i];
                ownerFaction = faction;
                break;
            }
            else if(playerControllers[i].ownedByPlayer && faction == FactionType.Enemy)
            {
                fovObject.SetActive(false);
                m_currentPlayerController = playerControllers[i];
                ownerFaction = faction;
                break;
            }
        }
    }
}