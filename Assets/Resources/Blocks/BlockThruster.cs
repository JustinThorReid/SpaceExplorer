using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockThruster : Block
{
    [SerializeField]
    private GameObject thrusterEfect;

    private void Update() {
        if(Input.GetButton("Jump")) {
            thrusterEfect.SetActive(true);
        } else {
            thrusterEfect.SetActive(false);
        }
    }
}
