using Atmos;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PipeNetwork
{
    private Dictionary<Vector2I, BlockPipe> pipeBlocks = new Dictionary<Vector2I, BlockPipe>();

    public IReadOnlyDictionary<Vector2I, BlockPipe> pipes => pipeBlocks;

    public readonly int networkID;
    private GasMix gasMix;

    public PipeNetwork(int networkID, Vector2I location, BlockPipe pipe) {
        this.networkID = networkID;
        pipeBlocks.Add(location, pipe);
        gasMix = new GasMix(pipe.volume);
    }

    /// <summary>
    /// Create a new pipe network by taking away the pipes from the other network
    /// </summary>
    /// <param name="networkID"></param>
    /// <param name="other"></param>
    /// <param name="pipes"></param>
    public PipeNetwork(int networkID, PipeNetwork other, IEnumerable<(Vector2I, BlockPipe)> pipes) {
        Debug.Assert(pipes.Count() > 0, "Attemping to create pipe network without any pipes");

        float totalVolume = 0;

        this.networkID = networkID;
        foreach((Vector2I loc, BlockPipe pipe) in pipes) {
            totalVolume += pipe.volume;

            other.pipeBlocks.Remove(loc);
            pipeBlocks.Add(loc, pipe);
        }

        gasMix = other.gasMix.SplitGasMix(totalVolume);
    }

    public float GetGasVolume() {
        return gasMix.volume;
    }

    /// <summary>
    /// Get the gas mix contained by this network
    /// </summary>
    /// <returns></returns>
    public GasMix GetGasMix() {
        // TODO: On atmos tick take gasmix changes into account
        return gasMix;
    }

    public void AddConnectingPipe(Vector2I location, BlockPipe pipe) {
        Debug.Assert(ConnectedNeighbors(location, pipe).Count > 0, "Adding connected pipe that does not have connections");

        pipeBlocks.Add(location, pipe);
        gasMix.ChangeVolume(pipe.volume);
    }

    public List<(Vector2I, BlockPipe)> ConnectedNeighbors(Vector2I location, BlockPipe pipe) {
        List<(Vector2I, BlockPipe)> result = new List<(Vector2I, BlockPipe)>(4);

        for(sbyte i = 0; i < Vector2I.DIRECTIONS.Length; i++) {
            if(!pipe.HasConnectionPoint(i))
                continue;

            Vector2I dir = Vector2I.DIRECTIONS[i];
            Vector2I testPos = location + dir;
            BlockPipe testPipe;

            if(pipeBlocks.TryGetValue(testPos, out testPipe)) {
                if(testPipe.HasConnectionPoint(Vector2I.RotateDirection((int)i, 2))) {
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
    public bool HasConnection(Vector2I location, sbyte direction) {
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
        gasMix.Absorb(other.gasMix);

        other.pipeBlocks.ToList().ForEach(x => {
            this.pipeBlocks.Add(x.Key, x.Value);
            other.pipeBlocks.Remove(x.Key);
        });
    }

    public void RemovePipe(Vector2I location) {
        Debug.Assert(pipeBlocks.ContainsKey(location), "RemovePipe: Network has no pipe at location");

        BlockPipe pipe;
        if(pipeBlocks.TryGetValue(location, out pipe))
            gasMix.ChangeVolume(pipe.volume * -1);

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
