using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class Item : MonoBehaviour
{
    private static float desiredSize = 0.5f;

    [SerializeField]
    private Block createsBlock;

    public virtual Block CreatesBlock() {
        return createsBlock;
    }

    private void Awake() {
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        float maxSize = Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        float scale = (desiredSize / maxSize);

        transform.localScale = new Vector3(scale, scale, 1);
    }

    public virtual Sprite GetPreviewSprite(byte rotation) {
        return this.createsBlock.GetSpriteForRotation(rotation);
    }
}
