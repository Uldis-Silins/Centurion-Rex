using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileFog : MonoBehaviour
{
    public Tilemap baseTilemap;
    public Tilemap fogTilemap;
    public TileBase[] fogTiles;

    private Vector3Int m_startPos;

    private void Start()
    {
        //foreach (var position in baseTilemap.cellBounds.allPositionsWithin)
        //{
        //    fogTilemap.SetTile(position, fogTiles[0]);
        //}
    }

    private void Update()
    {
        fogTilemap.SetTile(fogTilemap.WorldToCell(Random.insideUnitCircle * baseTilemap.localBounds.size * 0.5f), fogTiles[0]);
    }
}