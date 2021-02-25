using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPipe : Block
{
    public static PipeNetworkManager pipeNetworkManager;
    [SerializeField]
    private bool pipeConnectionTop;
    [SerializeField]
    private bool pipeConnectionRight;
    [SerializeField]
    private bool pipeConnectionLeft;
    [SerializeField]
    private bool pipeConnectionBottom;

    private void Awake() {
        if(pipeNetworkManager == null)
            pipeNetworkManager = FindObjectOfType<PipeNetworkManager>();
    }
}
