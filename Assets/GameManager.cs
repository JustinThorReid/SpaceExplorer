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

    void Awake()
    {
        allItems = Resources.LoadAll<Item>("Blocks");
        Debug.Log("Loaded " + allBlocks.Length + " items");
        allBlocks = Resources.LoadAll<Block>("Blocks");
        Debug.Log("Loaded " + allBlocks.Length + " blocks");
    }

}
