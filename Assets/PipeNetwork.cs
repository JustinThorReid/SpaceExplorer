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

    public PipeNetwork(int networkID, IEnumerable<(Vector2I, BlockPipe)> pipes) {
        Debug.Assert(pipes.Count() > 0, "Attemping to create pipe network without any pipes");

        this.networkID = networkID;
        foreach(var tuple in pipes) {
            pipeBlocks.Add(tuple.Item1, tuple.Item2);
        }
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

    public List<(Vector2I, BlockPipe)> ConnectedNeighbors(Vector2I location) {
        BlockPipe pipe = GetBlockPipe(location);

        if(pipe == null) {
            return new List<(Vector2I, BlockPipe)>(0);
        }

        return ConnectedNeighbors(location, pipe);
    }

    public BlockPipe GetBlockPipe(Vector2I location) {
        BlockPipe pipe = null;
        pipeBlocks.TryGetValue(location, out pipe);
        return pipe;
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
        other.pipeBlocks.ToList().ForEach(x => {
            this.pipeBlocks.Add(x.Key, x.Value);
            other.RemovePipe(x.Key);
        });
    }

    public void RemovePipe(Vector2I location) {
        Debug.Assert(pipeBlocks.ContainsKey(location), "RemovePipe: Network has no pipe at location");
        pipeBlocks.Remove(location);
    }

    public void RemovePipes(IEnumerable<(Vector2I, BlockPipe)> pipes) {
        foreach(var tuple in pipes) {
            RemovePipe(tuple.Item1);
        }
    }

    public HashSet<(Vector2I, BlockPipe)> GetConnectedChain(Vector2I startLocation) {
        Debug.Assert(pipeBlocks.ContainsKey(startLocation), "Attempting to get connected pipe chain for an invalid start location");

        HashSet<(Vector2I, BlockPipe)> foundLocations = new HashSet<(Vector2I, BlockPipe)>();
        HashSet<(Vector2I, BlockPipe)> checkLocations = new HashSet<(Vector2I, BlockPipe)>();
        checkLocations.Add((startLocation, GetBlockPipe(startLocation)));

        while(checkLocations.Count > 0) {
            (Vector2I checkLocation, BlockPipe checkPipe) = checkLocations.First();
            List<(Vector2I, BlockPipe)> testConnections = ConnectedNeighbors(checkLocation);

            foreach(var tuple in testConnections) {
                if(!foundLocations.Contains(tuple) && !checkLocations.Contains(tuple)) {
                    checkLocations.Add(tuple);
                }
            }

            checkLocations.Remove((checkLocation, checkPipe));
            foundLocations.Add((checkLocation, checkPipe));
        }

        return foundLocations;
    }
}
