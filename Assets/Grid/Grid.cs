using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour {
    public static readonly uint BLOCKS_PER_UNIT = 2;
    public static readonly uint BLOCKS_PER_LARGE_BLOCK = 2;

    public static readonly uint BLOCKS_PER_CHUNK = 40;
    public static readonly uint BLOCKS_PER_CHUNK_LARGE = BLOCKS_PER_CHUNK / BLOCKS_PER_LARGE_BLOCK;

    public static readonly float UNITS_PER_BLOCK = 1 / (float)BLOCKS_PER_UNIT;
    public static readonly float UNITS_PER_BLOCK_LARGE = UNITS_PER_BLOCK * BLOCKS_PER_LARGE_BLOCK;

    public GridChunk chunkPrefab;

    private Dictionary<Vector2I, GridChunk> chunks;
    private Dictionary<Vector2I, GridChunk> largeChunks;

    void Start() {
        chunks = new Dictionary<Vector2I, GridChunk>();
        largeChunks = new Dictionary<Vector2I, GridChunk>();
    }

    /// <summary>
    /// Gets only large blocks at a position (All block positions are in "small block" space)
    /// </summary>
    /// <param name="bigBlockPos"></param>
    /// <returns></returns>
    private List<Block> GetLargeBlocksAtPos(Vector2I blockPos) {
        Vector2I chunkPos = ConvertBlockPosToChunkPos(blockPos);
        GridChunk chunk;
        if(!largeChunks.TryGetValue(chunkPos, out chunk)) {
            return new List<Block>();
        }

        blockPos = blockPos - (chunkPos * BLOCKS_PER_CHUNK);
        return chunk.GetBlockColumn(blockPos / BLOCKS_PER_LARGE_BLOCK);
    }

    /// <summary>
    /// Gets only small/regular blocks at a position (All block positions are in "small block" space)
    /// </summary>
    /// <param name="bigBlockPos"></param>
    /// <returns></returns>
    private List<Block> GetSmallBlocksAtPos(Vector2I blockPos) {
        Vector2I chunkPos = ConvertBlockPosToChunkPos(blockPos);
        GridChunk chunk;
        if(!chunks.TryGetValue(chunkPos, out chunk)) {
            return new List<Block>();
        }

        blockPos = blockPos - (chunkPos * BLOCKS_PER_CHUNK);
        return chunk.GetBlockColumn(blockPos);
    }

    /// <summary>
    /// Get all regular blocks in the area covered by a large block
    /// </summary>
    /// <param name="blockPos"></param>
    /// <returns></returns>
    private List<Block> GetBlocksInLargeBlockArea(Vector2I blockPos) {
        Debug.Assert(blockPos.x % BLOCKS_PER_LARGE_BLOCK == 0, "Expected large block position not a multiple of large block size");

        List<Block> miniBlocks = new List<Block>();
        for(int x = blockPos.x; x < BLOCKS_PER_LARGE_BLOCK + blockPos.x; x++) {
            for(int y = blockPos.y; y < BLOCKS_PER_LARGE_BLOCK + blockPos.y; y++) {
                Vector2I thisBlockPos = new Vector2I(x, y);
                Vector2I chunkPos = ConvertBlockPosToChunkPos(thisBlockPos);

                GridChunk chunk;
                if(!chunks.TryGetValue(chunkPos, out chunk)) {
                    continue;
                }

                thisBlockPos = thisBlockPos - (chunkPos * BLOCKS_PER_CHUNK);
                miniBlocks.AddRange(chunk.GetBlockColumn(thisBlockPos));
            }
        }

        return miniBlocks;
    }

    public List<Block> GetIntersectingBlocks(Vector2I blockPos) {
        Vector2I largePos = GetContainingLargeBlockPos(blockPos);

        List<Block> bigBlocks = GetLargeBlocksAtPos(largePos);
        List<Block> miniBlocks = GetSmallBlocksAtPos(blockPos);

        bigBlocks.AddRange(miniBlocks);
        return bigBlocks;
    }

    public bool TryAddBlock(Block block, Vector2I bigBlockPos) {
        if(!block.CanBePlacedIn(GetIntersectingBlocks(bigBlockPos))) {
            return false;
        }

        return AddBlock(block, bigBlockPos);
    }

    private bool AddBlock(Block block, Vector2I blockPos) {
        Vector2I chunkPos = ConvertBlockPosToChunkPos(blockPos);

        if(block.isLarge) {
            Debug.Assert(blockPos.x % BLOCKS_PER_LARGE_BLOCK == 0, "Expected large block position not a multiple of large block size");

            GridChunk chunk;
            if(!largeChunks.TryGetValue(chunkPos, out chunk)) {
                chunk = CreateNewLargeChunk(chunkPos);
            }

            // Get block pos local to the chunk
            blockPos = blockPos - (chunkPos * BLOCKS_PER_CHUNK);
            return chunk.AddBlock(block, blockPos / BLOCKS_PER_LARGE_BLOCK);
        } else {
            GridChunk chunk;
            if(!chunks.TryGetValue(chunkPos, out chunk)) {
                chunk = CreateNewChunk(chunkPos);
            }

            // Get block pos local to the chunk
            blockPos = blockPos - (chunkPos * BLOCKS_PER_CHUNK);
            return chunk.AddBlock(block, blockPos);
        }
    }

    private GridChunk CreateNewChunk(Vector2I chunkPos) {
        Debug.Assert(!chunks.ContainsKey(chunkPos));

        Vector3 chunkLocalSpace = ConvertChunkPosToLocalSpace(chunkPos);
        
        GridChunk chunk = Instantiate(chunkPrefab);
        chunk.transform.parent = transform;
        chunk.transform.localPosition = chunkLocalSpace;
        chunk.transform.localRotation = Quaternion.identity;
        chunk.Init(BLOCKS_PER_CHUNK, UNITS_PER_BLOCK);
        chunks.Add(chunkPos, chunk);

        return chunk;
    }

    private GridChunk CreateNewLargeChunk(Vector2I chunkPos)
    {
        Debug.Assert(!largeChunks.ContainsKey(chunkPos));

        Vector3 chunkLocalSpace = ConvertChunkPosToLocalSpace(chunkPos);

        GridChunk chunk = Instantiate(chunkPrefab);
        chunk.transform.parent = transform;
        chunk.transform.localPosition = chunkLocalSpace;
        chunk.transform.localRotation = Quaternion.identity;
        chunk.Init(BLOCKS_PER_CHUNK_LARGE, UNITS_PER_BLOCK_LARGE);
        largeChunks.Add(chunkPos, chunk);

        return chunk;
    }

    private Vector2 ConvertChunkPosToLocalSpace(Vector2I chunkPos) {
        return chunkPos * BLOCKS_PER_CHUNK * UNITS_PER_BLOCK;
    }

    /// <summary>
    /// Gets the chunk pos that would contain this block
    /// </summary>
    /// <param name="blockPos"></param>
    /// <returns></returns>
    private Vector2I ConvertBlockPosToChunkPos(Vector2I blockPos) {
        Vector2I chunkPos = blockPos / BLOCKS_PER_CHUNK;

        if(blockPos.x < 0)
            chunkPos.x = (blockPos.x + 1) / (int)BLOCKS_PER_CHUNK - 1;
        if(blockPos.y < 0)
            chunkPos.y = (blockPos.y + 1) / (int)BLOCKS_PER_CHUNK - 1;

        return chunkPos;
    }

    public Vector2I ConvertWorldSpaceToBlockSpace(Vector3 worldPos) {
        return ConvertLocalSpaceToBlockSpace(gameObject.transform.InverseTransformPoint(worldPos));
    }

    public Vector2I ConvertWorldSpaceToLargeBlockSpace(Vector3 worldPos) {
        return GetContainingLargeBlockPos(ConvertLocalSpaceToBlockSpace(gameObject.transform.InverseTransformPoint(worldPos)));
    }    

    public Vector2 ConvertBlockSpaceToWorldSpace(Vector2I blockPos) {
        return transform.TransformPoint(ConvertBlockSpaceToLocalSpace(blockPos));
    }

    public Vector2I ConvertLocalSpaceToBlockSpace(Vector2 localPos) {
        localPos += new Vector2(UNITS_PER_BLOCK / 2f, UNITS_PER_BLOCK / 2f);
        Vector2I blockPos = Vector2I.Truncate(localPos / UNITS_PER_BLOCK);

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
        return blockPos * UNITS_PER_BLOCK;
    }

    private Vector2I GetContainingLargeBlockPos(Vector2I blockPos) {
        return (blockPos / BLOCKS_PER_LARGE_BLOCK) * BLOCKS_PER_LARGE_BLOCK;
    }
}
