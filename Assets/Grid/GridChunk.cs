using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridChunk : MonoBehaviour 
{
    public static readonly uint CHUNK_SIZE = 20;
    public static readonly int BLOCKS_PER_UNIT = 1;
    public static readonly float UNITS_PER_BLOCK = 1 / (float)BLOCKS_PER_UNIT;

    private List<Block>[,] gridData;

    public PlacedBlock mainPrefab;

    void Awake() {
        gridData = new List<Block>[CHUNK_SIZE, CHUNK_SIZE];
        for(uint x = 0; x < CHUNK_SIZE; x++) {
            for(uint y = 0; y < CHUNK_SIZE; y++) {
                gridData[x, y] = new List<Block>(2);
            }
        }
    }


    /**
     * Add block in base grid should be resposible for checking/placing the block at all grid locations for a multiblock
     * The block class is not user code and should contain logic for checking if allowed
     * Ideally there is some way of using a simple bool check for is present (maybe a common function like "not null")
     */
    public bool AddBlock(Block block, Vector2I blockCoord) {
        Debug.Assert(blockCoord.x >= 0 && blockCoord.x < CHUNK_SIZE);
        Debug.Assert(blockCoord.y >= 0 && blockCoord.y < CHUNK_SIZE);
        Debug.Assert(block != null, "Can not add 'nothing' to grid space!");

        gridData[blockCoord.x, blockCoord.y].Add(block);

        PlacedBlock placedBlock = Instantiate(mainPrefab);
        placedBlock.transform.parent = transform;
        placedBlock.transform.localPosition = ConvertBlockSpaceToLocalSpace(blockCoord);
        placedBlock.transform.localRotation = Quaternion.identity;

        placedBlock.Init(block);

        return true;
    }

    public List<Block> GetBlockColumn(Vector2I blockCoord) {
        Debug.Assert(blockCoord.x >= 0 && blockCoord.x < CHUNK_SIZE);
        Debug.Assert(blockCoord.y >= 0 && blockCoord.y < CHUNK_SIZE);
      
        return gridData[blockCoord.x, blockCoord.y] ?? new List<Block>();
    }

    /// <summary>
    /// Get the local space position of a block in this chunk
    /// This is local to THIS GRID and SUB-GAMEOBJECT
    /// </summary>
    /// <param name="chunkSpaceBlockPos"></param>
    /// <returns>Center of the block</returns>
    private Vector2 ConvertBlockSpaceToLocalSpace(Vector2I chunkSpaceBlockPos) {
        Debug.Assert(chunkSpaceBlockPos.x >= 0 && chunkSpaceBlockPos.x < CHUNK_SIZE);
        Debug.Assert(chunkSpaceBlockPos.y >= 0 && chunkSpaceBlockPos.y < CHUNK_SIZE);

        return (chunkSpaceBlockPos * UNITS_PER_BLOCK);
    }
}
