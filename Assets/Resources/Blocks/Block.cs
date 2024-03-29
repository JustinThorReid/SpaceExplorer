using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Block : MonoBehaviour {
    public uint layer = 0;
    public bool hasCollider = true;
    public float mass = 80;
    public bool isLarge = true;
    public Vector2I size = Vector2I.ONE;

    public Vector2I SmallSize
    {
        get {
            return isLarge ? size * Grid.BLOCKS_PER_LARGE_BLOCK : size;
        }
    }

    [HideInInspector]
    public Vector2I blockPos;
    protected ShipManager ship;

    /// <summary>
    /// Given a block position relative to prefab block origin point, return a vec relative to placed block.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public Vector2I RotateBlock(Vector2I blockPos) {
        Vector2I result = new Vector2I(blockPos);

        switch(rotation) {
            case 0:
                return result;
            case 1:
                int t = result.x;
                result.x = result.y;
                result.y = -t + PlacedSize(rotation).y;
                return result;
            case 2:
                result.x = -result.x + PlacedSize(rotation).x;
                result.y = -result.y + PlacedSize(rotation).y;
                return result;
            case 3:
                int t2 = result.x;
                result.x = -result.y + PlacedSize(rotation).x;
                result.y = t2;
                return result;
        }

        throw new Exception("Unhandled rotation");
    }
    /// <summary>
    /// Given a block position relative to prefab block origin point, return a vec relative to placed block.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public Vector2I RotateBlockSmall(Vector2I blockPos) {
        Vector2I result = new Vector2I(blockPos);

        switch(rotation) {
            case 0:
                return result;
            case 1:
                int t = result.x;
                result.x = result.y;
                result.y = -t + PlacedSizeSmall(rotation).y;
                return result;
            case 2:
                result.x = -result.x + PlacedSizeSmall(rotation).x;
                result.y = -result.y + PlacedSizeSmall(rotation).y;
                return result;
            case 3:
                int t2 = result.x;
                result.x = -result.y + PlacedSizeSmall(rotation).x;
                result.y = t2;
                return result;
        }

        throw new Exception("Unhandled rotation");
    }

    /// <summary>
    /// Given a vec relative to prefab, return a vec relative to placed block in game units.
    /// This only works for children game objects after the block has been placed in the world, it rotates around the sprite center point, not block placement position
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public Vector2 RotateBlockSmall(Vector2 result) {
        switch(rotation) {
            case 0:
                return result;
            case 1:
                float t = result.x;
                result.x = result.y;
                result.y = -t;
                return result;
            case 2:
                result.x = -result.x;
                result.y = -result.y;
                return result;
            case 3:
                float t2 = result.x;
                result.x = -result.y;
                result.y = t2;
                return result;
        }

        throw new Exception("Unhandled rotation");
    }

    public Vector2I PlacedSize(int rotation) {
        Vector2I placedSize = size.Rotated(rotation);
        placedSize.x = Mathf.Abs(placedSize.x);
        placedSize.y = Mathf.Abs(placedSize.y);

        return placedSize;
    }

    public Vector2I PlacedSizeSmall(int rotation) {
        return isLarge ? this.PlacedSize(rotation) * Grid.BLOCKS_PER_LARGE_BLOCK : this.PlacedSize(rotation);
    }
    public Vector2 CenterOffset(int rotation) {
        return PlacedSizeSmall(rotation) * 0.5f * Grid.UNITS_PER_BLOCK;
    }

    public Sprite[] spritesRotated;
    [HideInInspector]
    public byte rotation = 0;
    public Item createsItem;

    public void Init(byte rotation) {
        this.rotation = rotation;
    }

    public bool CanBePlacedIn(List<Block> column) {
        foreach(Block existing in column) {
            if(existing.layer == this.layer)
                return false;
        }

        if(this.layer == 10) {
            return column.Exists(existing => {
                return existing.layer == 0;
            });
        } else {
            return true;
        }
    }

    public bool CanBeAttached(int direction, Block other) {
        return other.layer >= layer;
    }

    // Use this for initialization
    void Start() {
        Debug.Assert(spritesRotated.Length > 0, "Missing sprites");

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = (int)layer;
        sr.sprite = GetSpriteForRotation(rotation);

        if(hasCollider) {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        }
    }

    public Sprite GetSpriteForRotation(byte rotation) {
        return spritesRotated[rotation % spritesRotated.Length];
    }

    public virtual void OnPlace(ShipManager ship, Vector2I blockPos) {
        this.blockPos = blockPos;
        this.ship = ship;

        // TODO: Integrate better into block
        PipeConnections c = GetComponent<PipeConnections>();
        if(c != null) {
            c.OnPlace();
        }

        // TODO: Move rotating objects to known child
        int children = transform.childCount;
        for(int i = 0; i < children; i++) {
            Vector2 rotated = RotateBlockSmall(transform.GetChild(i).localPosition);
            transform.GetChild(i).localPosition = new Vector3(rotated.x, rotated.y, transform.GetChild(i).localPosition.z);

            transform.GetChild(i).localRotation = Quaternion.Euler((rotation +1) * 90, 90, 0);
        }
    }
    public virtual void OnRemove(ShipManager ship, Vector2I blockPos) {
    }
}
