using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerLegs : MonoBehaviour {
    private static readonly float walkSpeed = 2;
    private static readonly float jumpSpeed = 9.81f * 0.5f;

    // Update is called once per frame
    void FixedUpdate() {
            Vector2 walkVector = new Vector2(Input.GetAxis("Horizontal") * walkSpeed, Input.GetAxisRaw("Vertical") * walkSpeed);

            if(Input.GetButtonDown("Jump")) {
                walkVector += (Vector2)(walkVector.normalized * jumpSpeed);
            }

            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            rb.velocity = walkVector;
    }
}
