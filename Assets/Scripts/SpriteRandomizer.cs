using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteRandomizer : MonoBehaviour
{
    //[SerializeField]
    //SpriteRenderer spriteToRandomize;

    [SerializeField]
    GameObject[] sprites;

    [SerializeField]
    bool randomize = false;

    void Start()
    {
        if (randomize)
            SetRandomSprite();
    }

    void SetRandomSprite()
    {
        int length = sprites.Length;
        int chosenID = Random.Range(0, length);

        for (int i = 0; i < length; i++)
        {
            if (i != chosenID)
                sprites[i].SetActive(false);
            else
                sprites[i].SetActive(true);
        }
    }
}
