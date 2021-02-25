using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class GridChunk : MonoBehaviour 
{
    private List<Block>[,] gridData;

    private uint chunkSize;
    private float blockSize;

    public void Init(uint size, float blockSize) {
        this.chunkSize = size;
        this.blockSize = blockSize;

        gridData = new List<Block>[chunkSize, chunkSize];
        for (uint x = 0; x < chunkSize; x++)
        {
            for (uint y = 0; y < chunkSize; y++)
            {
                gridData[x, y] = new List<Block>(2);
            }
        }
    }

    /**
     * Add block in base grid should be resposible for checking/placing the block at all grid locations for a multiblock
     * The block class is not user code and should contain logic for checking if allowed
     * Ideally there is some way of using a simple bool check for is present (maybe a common function like "not null")
     */
    public bool AddBlock(Block block, Vector2I blockCoord, byte rotation) {
        Debug.Assert(blockCoord.x >= 0 && blockCoord.x < chunkSize);
        Debug.Assert(blockCoord.y >= 0 && blockCoord.y < chunkSize);
        Debug.Assert(block != null, "Can not add 'nothing' to grid space!");

        Block placedBlock = Instantiate(block);
        placedBlock.transform.parent = transform;
        placedBlock.transform.localPosition = ConvertBlockSpaceToLocalSpace(blockCoord);
        placedBlock.transform.localRotation = Quaternion.identity;
        placedBlock.Init(rotation);

        if(gridData[blockCoord.x, blockCoord.y] == null)
            gridData[blockCoord.x, blockCoord.y] = new List<Block>();
        gridData[blockCoord.x, blockCoord.y].Add(placedBlock);

        return true;
    }

    public ReadOnlyCollection<Block> GetBlockColumn(Vector2I blockCoord) {
        Debug.Assert(blockCoord.x >= 0 && blockCoord.x < chunkSize);
        Debug.Assert(blockCoord.y >= 0 && blockCoord.y < chunkSize);
      
        return gridData[blockCoord.x, blockCoord.y].AsReadOnly() ?? new List<Block>().AsReadOnly();
    }

    /// <summary>
    /// Get the local space position of a block in this chunk
    /// This is local to THIS GRID and SUB-GAMEOBJECT
    /// </summary>
    /// <param name="chunkSpaceBlockPos"></param>
    /// <returns>Center of the block</returns>
    private Vector2 ConvertBlockSpaceToLocalSpace(Vector2I chunkSpaceBlockPos) {
        Debug.Assert(chunkSpaceBlockPos.x >= 0 && chunkSpaceBlockPos.x < chunkSize);
        Debug.Assert(chunkSpaceBlockPos.y >= 0 && chunkSpaceBlockPos.y < chunkSize);

        return (chunkSpaceBlockPos * blockSize) + new Vector2(blockSize * 0.5f, blockSize * 0.5f);
    }
}
