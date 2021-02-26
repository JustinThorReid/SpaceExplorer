using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPlacing : MonoBehaviour
{
    public Grid grid;
    public SpriteRenderer highlight;

    private int selectedItem = 0;
    private byte rotation = 0;
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
        // Select hotbar item
        if(Input.GetKeyDown(KeyCode.Alpha1))
            selectedItem = 0;
        if(Input.GetKeyDown(KeyCode.Alpha2))
            selectedItem = 1;
        if(Input.GetKeyDown(KeyCode.Alpha3))
            selectedItem = 2;
        if(Input.GetKeyDown(KeyCode.Alpha4))
            selectedItem = 3;
        selectedItem = Mathf.Max(0, Mathf.Min(selectedItem, gameManager.allItems.Length - 1));

        // Select kit item
        if(gameManager.allItems[selectedItem] is ItemPack) {
            if(Input.GetAxis("Mouse ScrollWheel") > 0) {
                ((ItemPack)gameManager.allItems[selectedItem]).IncrementActiveIndex(1);
            }
            if(Input.GetAxis("Mouse ScrollWheel") < 0) {
                ((ItemPack)gameManager.allItems[selectedItem]).IncrementActiveIndex(-1);
            }
        }

        // Rotate
        if(Input.GetKeyDown(KeyCode.R)) {
            rotation = (byte)((rotation + 1) % 4);
        }

        // Set Sprite
        highlight.sprite = gameManager.allItems[selectedItem].GetPreviewSprite(rotation);
        highlight.color = new Color(1, 1, 1, 0.5f);

        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2I blockPos;
        if(gameManager.allItems[selectedItem].CreatesBlock().isLarge) {
            blockPos = grid.ConvertWorldSpaceToLargeBlockSpace(worldMousePos);
            Vector2 worldPos = grid.ConvertBlockSpaceToWorldSpace(blockPos);
            highlight.transform.position = worldPos + new Vector2(Grid.UNITS_PER_BLOCK_LARGE * 0.5f, Grid.UNITS_PER_BLOCK_LARGE * 0.5f);
            highlight.transform.localScale = new Vector3(Grid.UNITS_PER_BLOCK_LARGE, Grid.UNITS_PER_BLOCK_LARGE, Grid.UNITS_PER_BLOCK_LARGE);
        } else {
            blockPos = grid.ConvertWorldSpaceToBlockSpace(worldMousePos);
            Vector2 worldPos = grid.ConvertBlockSpaceToWorldSpace(blockPos);
            highlight.transform.position = worldPos + new Vector2(Grid.UNITS_PER_BLOCK * 0.5f, Grid.UNITS_PER_BLOCK * 0.5f);
            highlight.transform.localScale = new Vector3(Grid.UNITS_PER_BLOCK, Grid.UNITS_PER_BLOCK, Grid.UNITS_PER_BLOCK);
        }

        highlight.transform.localRotation = grid.transform.rotation;

        if(Input.GetButtonUp("Fire1")) {
            grid.TryAddBlock(CreateBlockInstance(selectedItem), blockPos, rotation);
        }
        if(Input.GetButtonUp("Fire2")) {
            //TODO: A game manager that controls both grids and items should be responsible for spawning items/blocks
            Block removedBlock = grid.TryRemoveBlock(blockPos);
            Item spawnedItem = Instantiate(removedBlock.createsItem);
            spawnedItem.transform.position = grid.ConvertBlockSpaceToWorldSpace(blockPos);
        }
    }

    private Block CreateBlockInstance(int type) {
        return gameManager.allItems[selectedItem].CreatesBlock();
    }
}
