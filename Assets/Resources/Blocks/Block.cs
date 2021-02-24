using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Block : MonoBehaviour {
    public uint layer = 0;
    public bool hasCollider = true;
    public float mass = 80;
    public bool isLarge = true;

    public bool CanBePlacedIn(List<Block> column) {
        foreach(Block existing in column) {
            if(existing.layer == this.layer)
                return false;
        }

        if(this.layer == 0) {
            return column.Exists(existing => {
                return existing.layer == 5;
            });
        } else {
            return true;
        }
    }

    // Use this for initialization
    void Start() {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.rendererPriority = (int)layer;

        if(hasCollider) {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        }
    }
}
