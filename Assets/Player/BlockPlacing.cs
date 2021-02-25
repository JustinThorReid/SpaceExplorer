using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlacing : MonoBehaviour
{
    public Grid grid;
    public Transform highlight;

    private int selectedItem = 0;
    private GameManager gameManager;

    private void Awake() {
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnGUI() {
        for(int i = 0; i < gameManager.allItems.Length; i++) {
            if(i == selectedItem)
                GUI.color = Color.red;
            else
                GUI.color = Color.white;

            GUI.Label(new Rect(10, 10 + 20 * i, 250, 20), gameManager.allItems[i].name);
        }
    }

    void Update() {
        if(Input.GetAxis("Mouse ScrollWheel") > 0) {
            selectedItem = Mathf.Min(selectedItem + 1, gameManager.allItems.Length - 1);
        }
        if(Input.GetAxis("Mouse ScrollWheel") < 0) {
            selectedItem = Mathf.Max(0, selectedItem - 1);
        }

        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2I blockPos;
        if(gameManager.allItems[selectedItem].createsBlock.isLarge) {
            blockPos = grid.ConvertWorldSpaceToLargeBlockSpace(worldMousePos);
            Vector2 worldPos = grid.ConvertBlockSpaceToWorldSpace(blockPos);
            highlight.position = worldPos + new Vector2(Grid.UNITS_PER_BLOCK_LARGE * 0.5f, Grid.UNITS_PER_BLOCK_LARGE * 0.5f);
            highlight.localScale = new Vector3(Grid.UNITS_PER_BLOCK_LARGE, Grid.UNITS_PER_BLOCK_LARGE, Grid.UNITS_PER_BLOCK_LARGE);
        } else {
            blockPos = grid.ConvertWorldSpaceToBlockSpace(worldMousePos);
            Vector2 worldPos = grid.ConvertBlockSpaceToWorldSpace(blockPos);
            highlight.position = worldPos + new Vector2(Grid.UNITS_PER_BLOCK * 0.5f, Grid.UNITS_PER_BLOCK * 0.5f);
            highlight.localScale = new Vector3(Grid.UNITS_PER_BLOCK, Grid.UNITS_PER_BLOCK, Grid.UNITS_PER_BLOCK);
        }

        highlight.localRotation = grid.transform.rotation;

        if(Input.GetButtonUp("Fire1")) {
            grid.TryAddBlock(CreateBlockInstance(selectedItem), blockPos);
        }
    }

    private Block CreateBlockInstance(int type) {
        return gameManager.allItems[selectedItem].createsBlock;
    }
}
