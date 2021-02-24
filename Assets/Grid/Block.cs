using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : ScriptableObject {
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
                return existing.layer == 1;
            });
        } else {
            return true;
        }
    }
}
