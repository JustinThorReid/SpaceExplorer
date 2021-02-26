using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PipeNetwork
{
    private Dictionary<Vector2I, BlockPipe> pipeBlocks = new Dictionary<Vector2I, BlockPipe>();
    public IReadOnlyDictionary<Vector2I, BlockPipe> pipes => pipeBlocks;
    public readonly int networkID;

    public PipeNetwork(int networkID, Vector2I location, BlockPipe pipe) {
        this.networkID = networkID;
        pipeBlocks.Add(location, pipe);
    }

    public void AddConnectingPipe(Vector2I location, BlockPipe pipe) {
        Debug.Assert(ConnectedNeighbors(location, pipe).Count > 0, "Adding connected pipe that does not have connections");

        pipeBlocks.Add(location, pipe);
    }

    public List<(Vector2I, BlockPipe)> ConnectedNeighbors(Vector2I location, BlockPipe pipe) {
        List<(Vector2I, BlockPipe)> result = new List<(Vector2I, BlockPipe)>(4);

        for(int i = 0; i < Vector2I.DIRECTIONS.Length; i++) {
            if(!pipe.HasConnectionPoint(i))
                continue;

            Vector2I dir = Vector2I.DIRECTIONS[i];
            Vector2I testPos = location + dir;
            BlockPipe testPipe;

            if(pipeBlocks.TryGetValue(testPos, out testPipe)) {
                if(testPipe.HasConnectionPoint(Vector2I.RotateDirection(i, 2))) {
                    result.Add((testPos, testPipe));
                }
            }
        }

        return result;
    } 

    /// <summary>
    /// True if there is a pipe at location with a connected socket in direction
    /// </summary>
    /// <param name="location"></param>
    /// <param name="direction"></param>
    /// <returns></returns>
    public bool HasConnection(Vector2I location, int direction) {
        BlockPipe sourcePipe;
        if(!pipeBlocks.TryGetValue(location, out sourcePipe)) {
            return false;
        }

        BlockPipe destPipe;
        if(!pipeBlocks.TryGetValue(location + Vector2I.DIRECTIONS[direction], out destPipe)) {
            return false;
        }

        return sourcePipe.HasConnectionPoint(direction) && destPipe.HasConnectionPoint(Vector2I.RotateDirection(direction, 2));
    }

    public void AddPipeNetwork(PipeNetwork other) {
        other.pipeBlocks.ToList().ForEach(x => this.pipeBlocks.Add(x.Key, x.Value));
    }
}
