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
    [SerializeField]
    private float _volume = 10; // 25 for medium-large pipes
    public float volume
    {
        get {
            return _volume;

        }
    }

    private void Awake() {
        if(pipeNetworkManager == null)
            pipeNetworkManager = FindObjectOfType<PipeNetworkManager>();
    }

    public override void OnPlace(ShipManager ship, Vector2I gridBlockPos) {
        base.OnPlace(ship, gridBlockPos);

        ship.pipes.AddPipe(gridBlockPos, this);
    }

    public override void OnRemove(ShipManager ship, Vector2I gridBlockPos) {
        base.OnRemove(ship, gridBlockPos);

        ship.pipes.RemovePipe(gridBlockPos, this);
    }

    public bool HasConnectionPoint(sbyte direction) {
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
