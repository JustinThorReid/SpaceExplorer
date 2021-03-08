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

    [HideInInspector]
    public Vector2I blockPos;

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

    public virtual void OnPlace(Grid grid, Vector2I blockPos) {
        this.blockPos = blockPos;

        // TODO: This should be handled generically with events or interface
        PipeConnections connectionComp = GetComponent<PipeConnections>();
        if(connectionComp != null) {
            connectionComp.OnPlace(grid, blockPos);
        }
    }
    public virtual void OnRemove(Grid grid, Vector2I blockPos) {
    }
}
