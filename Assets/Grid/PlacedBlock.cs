using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlacedBlock : MonoBehaviour
{
    public Sprite hull;
    public Sprite panel;

    private Block blockData;

    public void Init(Block blockData) {
        this.blockData = blockData;
    }

    // Use this for initialization
    void Start() {
        Debug.Assert(blockData != null, "PlacedBlock started without block data defined");

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.rendererPriority = (int)blockData.layer;

        if(blockData.layer == 0) {
            sr.sprite = hull;
        } else {
            sr.sprite = panel;
        }

        if(blockData.hasCollider) {
            BoxCollider2D collider = gameObject.AddComponent<BoxCollider2D>();
        }
    }
}
