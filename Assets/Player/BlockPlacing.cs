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
        if(Input.GetKeyDown(KeyCode.Alpha5))
            selectedItem = 4;
        if(Input.GetKeyDown(KeyCode.Alpha6))
            selectedItem = 5;
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
        if(BlockToPlace(selectedItem).isLarge) {
            blockPos = grid.ConvertWorldSpaceToLargeBlockSpace(worldMousePos);
        } else {
            blockPos = grid.ConvertWorldSpaceToBlockSpace(worldMousePos);
        }

        Block blockToPlace = BlockToPlace(selectedItem);
        Vector2 blockCenterOffset = blockToPlace.CenterOffset(rotation);
        Vector2 worldPos = grid.ConvertBlockSpaceToWorldSpace(blockPos);

        highlight.transform.position = blockCenterOffset + worldPos;
        highlight.transform.localRotation = grid.transform.rotation;

        if(Input.GetButtonUp("Fire1")) {
            grid.TryAddBlock(BlockToPlace(selectedItem), blockPos, rotation);
        }
        if(Input.GetButtonUp("Fire2")) {
            //TODO: A game manager that controls both grids and items should be responsible for spawning items/blocks
            Block removedBlock = grid.TryRemoveBlock(blockPos);

            if(removedBlock != null) {
                Item spawnedItem = Instantiate(removedBlock.createsItem);
                spawnedItem.transform.position = grid.ConvertBlockSpaceToWorldSpace(blockPos);
            }
        }
    }

    private Block BlockToPlace(int type) {
        return gameManager.allItems[selectedItem].CreatesBlock();
    }
}
