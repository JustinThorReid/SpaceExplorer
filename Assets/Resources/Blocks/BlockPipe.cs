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

    public override void OnPlace(Grid grid, Vector2I gridBlockPos) {
        base.OnPlace(grid, gridBlockPos);

        pipeNetworkManager.AddPipe(grid, gridBlockPos, this);
    }

    public override void OnRemove(Grid grid, Vector2I gridBlockPos) {
        base.OnRemove(grid, gridBlockPos);

        pipeNetworkManager.RemovePipe(grid, gridBlockPos, this);
    }

    public bool HasConnectionPoint(int direction) {
        // Rotate the direction into "block space" before checking
        switch(Vector2I.RotateDirection(direction, -rotation)) {
            case 0:
                return pipeConnectionTop;
            case 1:
                return pipeConnectionRight;
            case 2:
                return pipeConnectionBottom;
            case 3:
                return pipeConnectionLeft;
            default:
                Debug.Assert(false, "Unhandled direction");
                return false;
        }
    }
}
