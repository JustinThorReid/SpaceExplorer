using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour {
    public static readonly uint BLOCKS_PER_UNIT = 2;
    public static readonly uint BLOCKS_PER_LARGE_BLOCK = 2;

    public static readonly uint BLOCKS_PER_CHUNK = 40;
    public static readonly uint BLOCKS_PER_CHUNK_LARGE = BLOCKS_PER_CHUNK / BLOCKS_PER_LARGE_BLOCK;

    public static readonly float UNITS_PER_BLOCK = 1 / (float)BLOCKS_PER_UNIT;
    public static readonly float UNITS_PER_BLOCK_LARGE = UNITS_PER_BLOCK * BLOCKS_PER_LARGE_BLOCK;

    public static readonly Vector2 BLOCK_OFFSET = new Vector2(UNITS_PER_BLOCK / 2f, UNITS_PER_BLOCK / 2f);

    public GridChunk chunkPrefab;

    private Dictionary<Vector2I, GridChunk> chunks;
    private Dictionary<Vector2I, GridChunk> largeChunks;

    void Start() {
        chunks = new Dictionary<Vector2I, GridChunk>();
        largeChunks = new Dictionary<Vector2I, GridChunk>();

        Debug.Assert(GetContainingLargeBlockPos(new Vector2I(0, 0)) == new Vector2I(0, 0));
        Debug.Assert(GetContainingLargeBlockPos(new Vector2I(1, 0)) == new Vector2I(0, 0));
        Debug.Assert(GetContainingLargeBlockPos(new Vector2I(2, 0)) == new Vector2I(2, 0));
        Debug.Assert(GetContainingLargeBlockPos(new Vector2I(-1, 0)) == new Vector2I(-2, 0));
        Debug.Assert(GetContainingLargeBlockPos(new Vector2I(-2, 0)) == new Vector2I(-2, 0));
        Debug.Assert(GetContainingLargeBlockPos(new Vector2I(-3, 0)) == new Vector2I(-4, 0));
    }

    /// <summary>
    /// Gets only large blocks at a position (All block positions are in "small block" space)
    /// </summary>
    /// <param name="bigBlockPos"></param>
    /// <returns></returns>
    private ReadOnlyCollection<Block> GetLargeBlocksAtPos(Vector2I blockPos) {
        Vector2I chunkPos = ConvertBlockPosToChunkPos(blockPos);
        GridChunk chunk;
        if(!largeChunks.TryGetValue(chunkPos, out chunk)) {
            return new List<Block>().AsReadOnly();
        }

        blockPos = blockPos - (chunkPos * BLOCKS_PER_CHUNK);
        return chunk.GetBlockColumn(blockPos / BLOCKS_PER_LARGE_BLOCK);
    }

    /// <summary>
    /// Gets only small/regular blocks at a position (All block positions are in "small block" space)
    /// </summary>
    /// <param name="bigBlockPos"></param>
    /// <returns></returns>
    private ReadOnlyCollection<Block> GetSmallBlocksAtPos(Vector2I blockPos) {
        Vector2I chunkPos = ConvertBlockPosToChunkPos(blockPos);
        GridChunk chunk;
        if(!chunks.TryGetValue(chunkPos, out chunk)) {
            return new List<Block>().AsReadOnly();
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

        ReadOnlyCollection<Block> bigBlocks = GetLargeBlocksAtPos(largePos);
        ReadOnlyCollection<Block> miniBlocks = GetSmallBlocksAtPos(blockPos);

        return bigBlocks.Concat(miniBlocks).ToList();
    }

    public Block TryAddBlock(Block block, Vector2I blockPos, byte rotation) {
        Debug.Assert(block.gameObject.scene == null, "Attemping to add prefab instance to grid, expecting a prefab original");

        Vector2I placedSize = block.size.Rotated(rotation);
        Vector2I testPos = new Vector2I();
        for(testPos.x = blockPos.x; testPos.x < blockPos.x + placedSize.x; testPos.x++) {
            for(testPos.y = blockPos.y; testPos.y < blockPos.y + placedSize.y; testPos.y++) {
                if(!block.CanBePlacedIn(GetIntersectingBlocks(testPos))) {
                    return null;
                }
            }
        }

        return AddBlock(block, blockPos, rotation);
    }

    /// <summary>
    /// Remove a block at blockPos. First remove small blocks then large blocks. For each column set remove upper layer items first.
    /// TODO: Better logic around removing needed to make sure supports aren't removed before objects they support
    /// TODO: Specific block to remove should be specified by pixel position
    /// </summary>
    /// <param name="blockPos"></param>
    /// <returns></returns>
    public Block TryRemoveBlock(Vector2I blockPos) {
        Block removed = null;
        IOrderedEnumerable<Block> blocks = GetSmallBlocksAtPos(blockPos).OrderByDescending(block => block.layer);
        if(blocks.Count() > 0) {
            removed = blocks.First();
            RemoveBlock(removed, blockPos);
        } else {
            Vector2I largePos = GetContainingLargeBlockPos(blockPos);
            IOrderedEnumerable<Block> bigBlocks = GetLargeBlocksAtPos(largePos).OrderByDescending(block => block.layer);
            if(bigBlocks.Count() > 0) {
                removed = bigBlocks.First();
                RemoveBlock(removed, largePos);
            }
        }

        if(removed != null) {
            removed.OnRemove(this, blockPos);
        }

        return removed;
    }

    /// Remove a block from a location
    private void RemoveBlock(Block blockInstance, Vector2I blockPos) {
        Vector2I chunkPos = ConvertBlockPosToChunkPos(blockPos);
        GridChunk chunk;
        Debug.Assert(blockInstance.gameObject.scene != null, "Attemping to remove prefab original from grid");

        if(blockInstance.isLarge) {
            Debug.Assert(blockPos.x % BLOCKS_PER_LARGE_BLOCK == 0, "Expected large block position is not a multiple of large block size");

            if(!largeChunks.TryGetValue(chunkPos, out chunk)) {
                throw new System.Exception("Chunk does not exist during block remove");
            }

            blockPos = (blockPos - (chunkPos * BLOCKS_PER_CHUNK)) / BLOCKS_PER_LARGE_BLOCK;
        } else {
            if(!chunks.TryGetValue(chunkPos, out chunk)) {
                throw new System.Exception("Chunk does not exist during block remove");
            }

            blockPos = blockPos - (chunkPos * BLOCKS_PER_CHUNK);
        }

        chunk.RemoveBlock(blockInstance, blockPos);
        Destroy(blockInstance.gameObject);
    }

    private Block AddBlock(Block block, Vector2I blockPos, byte rotation) {
        Vector2I chunkPos = ConvertBlockPosToChunkPos(blockPos);
        Block placedBlock = Instantiate(block);



        if(block.isLarge) {
            Debug.Assert(blockPos.x % BLOCKS_PER_LARGE_BLOCK == 0, "Expected large block position is not a multiple of large block size");

            GridChunk chunk;
            if(!largeChunks.TryGetValue(chunkPos, out chunk)) {
                chunk = CreateNewLargeChunk(chunkPos);
            }

            // Get block pos local to the chunk
            Vector2I chunkBlockPos = (blockPos - (chunkPos * BLOCKS_PER_CHUNK)) / BLOCKS_PER_LARGE_BLOCK;
            chunk.AddBlock(placedBlock, chunkBlockPos, rotation);
        } else {
            GridChunk chunk;
            if(!chunks.TryGetValue(chunkPos, out chunk)) {
                chunk = CreateNewChunk(chunkPos);
            }

            // Get block pos local to the chunk
            Vector2I chunkBlockPos = blockPos - (chunkPos * BLOCKS_PER_CHUNK);
            chunk.AddBlock(placedBlock, chunkBlockPos, rotation);
        }

        placedBlock.OnPlace(this, blockPos);
        return placedBlock;
    }

    private GridChunk CreateNewChunk(Vector2I chunkPos) {
        Debug.Assert(!chunks.ContainsKey(chunkPos));

        Vector3 chunkLocalSpace = ConvertChunkPosToLocalSpace(chunkPos);
        
        GridChunk chunk = Instantiate(chunkPrefab);
        chunk.name = "Chunk " + chunkPos;
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
        chunk.name = "Large Chunk " + chunkPos;
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
        Vector2I result = (blockPos + new Vector2I(1000000, 1000000)) / BLOCKS_PER_UNIT;
        return (result * BLOCKS_PER_LARGE_BLOCK) - new Vector2I(1000000, 1000000);
    }
}
