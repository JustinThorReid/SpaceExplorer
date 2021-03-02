using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour {
    public static readonly uint BLOCKS_PER_UNIT = 2;
    public static readonly uint BLOCKS_PER_LARGE_BLOCK = 2;

    public static readonly float UNITS_PER_BLOCK = 1 / (float)BLOCKS_PER_UNIT;
    public static readonly float UNITS_PER_BLOCK_LARGE = UNITS_PER_BLOCK * BLOCKS_PER_LARGE_BLOCK;

    public static readonly Vector2 BLOCK_OFFSET = new Vector2(UNITS_PER_BLOCK / 2f, UNITS_PER_BLOCK / 2f);

    private Dictionary<Vector2I, List<Block>> blockData;
    private Dictionary<Vector2I, List<Block>> largeBlockData;

    void Start() {
        blockData = new Dictionary<Vector2I, List<Block>>();
        largeBlockData = new Dictionary<Vector2I, List<Block>>();

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
        Vector2I largeBlockPos = GetContainingLargeBlockPos(blockPos);
        List<Block> blocks;
        if(!largeBlockData.TryGetValue(largeBlockPos, out blocks)) {
            return new List<Block>().AsReadOnly();
        }

        return blocks.AsReadOnly();
    }

    /// <summary>
    /// Gets only small/regular blocks at a position (All block positions are in "small block" space)
    /// </summary>
    /// <param name="bigBlockPos"></param>
    /// <returns></returns>
    private ReadOnlyCollection<Block> GetSmallBlocksAtPos(Vector2I blockPos) {
        List<Block> blocks;
        if(!blockData.TryGetValue(blockPos, out blocks)) {
            return new List<Block>().AsReadOnly();
        }

        return blocks.AsReadOnly();
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
                miniBlocks.AddRange(GetSmallBlocksAtPos(thisBlockPos));
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
        Debug.Assert(block.gameObject.scene.rootCount == 0, "Attemping to add prefab instance to grid, expecting a prefab original");

        Vector2I placedSize = block.isLarge ? block.PlacedSize(rotation) * BLOCKS_PER_LARGE_BLOCK : block.PlacedSize(rotation);
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
            RemoveBlock(removed);
        } else {
            Vector2I largePos = GetContainingLargeBlockPos(blockPos);
            IOrderedEnumerable<Block> bigBlocks = GetLargeBlocksAtPos(largePos).OrderByDescending(block => block.layer);
            if(bigBlocks.Count() > 0) {
                removed = bigBlocks.First();
                RemoveBlock(removed);
            }
        }

        if(removed != null) {
            removed.OnRemove(this, blockPos);
        }

        return removed;
    }

    /// Remove a block from a location
    private void RemoveBlock(Block blockInstance) {
        Debug.Assert(blockInstance.gameObject.scene.rootCount != 0, "Attemping to remove prefab original from grid");

        Vector2I blockPos = blockInstance.blockPos;
        Vector2I placedSize = blockInstance.PlacedSize(blockInstance.rotation);
        Vector2I placePos = new Vector2I();
        if(blockInstance.isLarge) {
            Debug.Assert(blockPos.x % BLOCKS_PER_LARGE_BLOCK == 0, "Expected large block position is not a multiple of large block size");

            for(placePos.x = blockPos.x; placePos.x < blockPos.x + placedSize.x; placePos.x += (int)BLOCKS_PER_LARGE_BLOCK) {
                for(placePos.y = blockPos.y; placePos.y < blockPos.y + placedSize.y; placePos.y += (int)BLOCKS_PER_LARGE_BLOCK) {
                    removeData(largeBlockData, placePos, blockInstance);
                }
            }
        } else {
            for(placePos.x = blockPos.x; placePos.x < blockPos.x + placedSize.x; placePos.x++) {
                for(placePos.y = blockPos.y; placePos.y < blockPos.y + placedSize.y; placePos.y++) {
                    removeData(blockData, placePos, blockInstance);
                }
            }
        }

        Destroy(blockInstance.gameObject);
    }

    private void setData(Dictionary<Vector2I, List<Block>> dict, Vector2I pos, Block block) {
        List<Block> column = null;
        if(!dict.TryGetValue(pos, out column)) {
            column = new List<Block>(3);
            dict.Add(pos, column);
        }

        column.Add(block);
    }

    private void removeData(Dictionary<Vector2I, List<Block>> dict, Vector2I pos, Block block) {
        Debug.Assert(dict.ContainsKey(pos), "Attempting to remove data from dict that does not contain key");

        List<Block> column = null;
        if(dict.TryGetValue(pos, out column)) {
            column.Remove(block);
        }
    }

    /// <summary>
    /// blockPos and size are expected in small/regular size
    /// </summary>
    /// <param name="blockInstance"></param>
    /// <param name="blockPos"></param>
    /// <param name="size"></param>
    private void positionBlock(Block blockInstance, Vector2I blockPos, int rotation) {
        Vector2 position = blockPos * UNITS_PER_BLOCK + blockInstance.CenterOffset(rotation);

        blockInstance.transform.parent = transform;
        blockInstance.transform.localPosition = position;
    }

    private Block AddBlock(Block block, Vector2I blockPos, byte rotation) {
        Block placedBlock = Instantiate(block);

        Vector2I placedSize = block.PlacedSizeSmall(rotation);
        Vector2I placePos = new Vector2I();
        if(placedBlock.isLarge) {
            Debug.Assert(blockPos.x % BLOCKS_PER_LARGE_BLOCK == 0, "Expected large block position is not a multiple of large block size");

            for(placePos.x = blockPos.x; placePos.x < blockPos.x + placedSize.x; placePos.x += (int)BLOCKS_PER_LARGE_BLOCK) {
                for(placePos.y = blockPos.y; placePos.y < blockPos.y + placedSize.y; placePos.y += (int)BLOCKS_PER_LARGE_BLOCK) {
                    setData(largeBlockData, placePos, placedBlock);
                }
            }
        } else {
            for(placePos.x = blockPos.x; placePos.x < blockPos.x + placedSize.x; placePos.x++) {
                for(placePos.y = blockPos.y; placePos.y < blockPos.y + placedSize.y; placePos.y++) {
                    setData(blockData, placePos, placedBlock);
                }
            }
        }

        positionBlock(placedBlock, blockPos, rotation);
        placedBlock.Init(rotation);
        placedBlock.OnPlace(this, blockPos);
        return placedBlock;
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
        Debug.Assert(blockPos.x > -1000000, "Block position too low for negative rounding");
        Debug.Assert(blockPos.y > -1000000, "Block position too low for negative rounding");

        Vector2I result = (blockPos + new Vector2I(1000000, 1000000)) / BLOCKS_PER_UNIT;
        return (result * BLOCKS_PER_LARGE_BLOCK) - new Vector2I(1000000, 1000000);
    }
}
