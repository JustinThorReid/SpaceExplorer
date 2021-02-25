using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPack : Item
{
    [SerializeField]
    private Block[] blocksInPack;
    [HideInInspector]
    private byte activeIndex = 0;

    public void SetActive(byte index) {
        activeIndex = (byte)Mathf.Max(Mathf.Min(blocksInPack.Length - 1, index), 0);
    }

    public void IncrementActiveIndex(int amount) {
        activeIndex = (byte)Mathf.Max(Mathf.Min(blocksInPack.Length - 1, activeIndex + amount), 0);
    }

    public override Sprite GetPreviewSprite(byte rotation) {
        return this.blocksInPack[activeIndex].GetSpriteForRotation(rotation);
    }

    public override Block CreatesBlock() {
        return blocksInPack[activeIndex];
    }
}
