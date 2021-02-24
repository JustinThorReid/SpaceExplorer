using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlacing : MonoBehaviour
{
    public Grid grid;
    public Transform highlight;
    
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update() {
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2I blockPos = grid.ConvertWorldSpaceToLargeBlockSpace(worldMousePos);
        Vector2 worldPos = grid.ConvertBlockSpaceToWorldSpace(blockPos);

        highlight.position = worldPos;
        highlight.localRotation = grid.transform.rotation;

        if(Input.GetButtonUp("Fire1")) {
            grid.TryAddBlock(CreateBlockInstance(0), blockPos);
        }
        if(Input.GetButtonUp("Fire2")) {
            grid.TryAddBlock(CreateBlockInstance(1), blockPos);
        }
    }

    private Block CreateBlockInstance(int type) {
        Block block = ScriptableObject.CreateInstance<Block>();

        if(type == 0) {
            block.layer = 0;
            block.hasCollider = true;
        }
        if(type == 1) {
            block.layer = 1;
            block.hasCollider = false;
        }
        return block;
    }
}
