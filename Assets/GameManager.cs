using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [HideInInspector]
    public Item[] allItems;
    [HideInInspector]
    public Block[] allBlocks;
    [SerializeField]
    private Grid gridPrefab;
    void Awake()
    {
        allItems = Resources.LoadAll<Item>("Blocks");
        Debug.Log("Loaded " + allBlocks.Length + " items");
        allBlocks = Resources.LoadAll<Block>("Blocks");
        Debug.Log("Loaded " + allBlocks.Length + " blocks");
    }

    public Grid createGrid(Vector2 position, Quaternion rotation, Block initialBlock, byte blockRotation) {
        Grid grid = Instantiate<Grid>(gridPrefab);
        grid.transform.position = position;
        grid.transform.rotation = rotation;

        grid.ForcePlaceBlock(initialBlock, Vector2I.ZERO, blockRotation);
        return grid;
    }

    public Grid[] allGrids() {
        return FindObjectsOfType<Grid>();
    }
}
