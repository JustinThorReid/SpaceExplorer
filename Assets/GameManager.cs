using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Item[] allItems;

    void Awake()
    {
        allItems = Resources.LoadAll<Item>("Blocks");
    }

}
