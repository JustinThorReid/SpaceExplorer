using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {
    public GridChunk chunkPrefab;

    private Dictionary<Vector2I, GridChunk> chunks;

    void Start() {
        chunks = new Dictionary<Vector2I, GridChunk>();
    }

    public List<Block> GetBlockColumn(Vector2I blockPos) {
        Vector2I chunkPos = ConvertBlockPosToChunkPos(blockPos);
        GridChunk chunk;
        if(!chunks.TryGetValue(chunkPos, out chunk)) {
            return new List<Block>();
        }

        blockPos = blockPos - (chunkPos * GridChunk.CHUNK_SIZE);
        return chunk.GetBlockColumn(blockPos);
    }

    public bool TryAddBlock(Block block, Vector2I blockPos) {
        if(!block.CanBePlacedIn(GetBlockColumn(blockPos))) {
            return false;
        }

        return AddBlock(block, blockPos);
    }

    private bool AddBlock(Block block, Vector2I blockPos) {
        Vector2I chunkPos = ConvertBlockPosToChunkPos(blockPos);

        GridChunk chunk;
        if(!chunks.TryGetValue(chunkPos, out chunk)) {
            chunk = CreateNewChunk(chunkPos);
        }

        // Get block pos local to the chunk
        blockPos = blockPos - (chunkPos * GridChunk.CHUNK_SIZE);
        return chunk.AddBlock(block, blockPos);
    }

    private GridChunk CreateNewChunk(Vector2I chunkPos) {
        Debug.Assert(!chunks.ContainsKey(chunkPos));

        Vector3 chunkLocalSpace = ConvertChunkPosToLocalSpace(chunkPos);
        
        GridChunk chunk = Instantiate(chunkPrefab);
        chunk.transform.parent = transform;
        chunk.transform.localPosition = chunkLocalSpace;
        chunk.transform.localRotation = Quaternion.identity;
        chunks.Add(chunkPos, chunk);

        return chunk;
    }

    private Vector2 ConvertChunkPosToLocalSpace(Vector2I chunkPos) {
        return chunkPos * GridChunk.CHUNK_SIZE * GridChunk.UNITS_PER_BLOCK;
    }

    /// <summary>
    /// Gets the chunk pos that would contain this block
    /// </summary>
    /// <param name="blockPos"></param>
    /// <returns></returns>
    private Vector2I ConvertBlockPosToChunkPos(Vector2I blockPos) {
        Vector2I chunkPos = blockPos / GridChunk.CHUNK_SIZE;

        if(blockPos.x < 0)
            chunkPos.x = (blockPos.x + 1) / (int)GridChunk.CHUNK_SIZE - 1;
        if(blockPos.y < 0)
            chunkPos.y = (blockPos.y + 1) / (int)GridChunk.CHUNK_SIZE - 1;

        return chunkPos;
    }

    public Vector2I ConvertWorldSpaceToBlockSpace(Vector3 worldPos) {
        return ConvertLocalSpaceToBlockSpace(gameObject.transform.InverseTransformPoint(worldPos));
    }

    public Vector2 ConvertBlockSpaceToWorldSpace(Vector2I blockPos) {
        return transform.TransformPoint(ConvertBlockSpaceToLocalSpace(blockPos));
    }

    public Vector2I ConvertLocalSpaceToBlockSpace(Vector2 localPos) {
        localPos += new Vector2(GridChunk.UNITS_PER_BLOCK / 2f, GridChunk.UNITS_PER_BLOCK / 2f);
        Vector2I blockPos = Vector2I.Truncate(localPos / GridChunk.UNITS_PER_BLOCK);

        if(localPos.y < 0) {
            blockPos.y--;
        }
        if(localPos.x < 0) {
            blockPos.x--;
        }

        return blockPos;
    }

    /// <summary>
    /// Get the local space position of a block in game units
    /// </summary>
    /// <param name="blockPos"></param>
    /// <returns>Center of the block</returns>
    public Vector2 ConvertBlockSpaceToLocalSpace(Vector2I blockPos) {
        return blockPos * GridChunk.UNITS_PER_BLOCK;
    }
}
