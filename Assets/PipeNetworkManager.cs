using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PipeNetworkManager : MonoBehaviour
{
    Dictionary<Grid, List<PipeNetwork>> pipeNetworks = new Dictionary<Grid, List<PipeNetwork>>();
    private int lastNetworkID = -1;
    [SerializeField]
    private bool showDebug = false;

    private void OnDrawGizmos() {
        if(!showDebug)
            return;

        Color[] colors = { Color.red, Color.blue, Color.green, Color.magenta, Color.white, Color.black};

        pipeNetworks.ToList().ForEach(gridNetwork => {
            Grid grid = gridNetwork.Key;

            gridNetwork.Value.ForEach(network => {
                Gizmos.color = colors[network.networkID % colors.Length];

                network.pipes.ToList().ForEach(entry => {
                    Vector2 center = grid.ConvertBlockSpaceToWorldSpace(entry.Key) + Grid.BLOCK_OFFSET;

                    for(int i = 0; i < 4; i++) {
                        if(entry.Value.HasConnectionPoint(i)) {
                            Vector2 dest = grid.transform.TransformPoint(Vector2I.DIRECTIONS[i] * Grid.BLOCK_OFFSET.x) + new Vector3(center.x, center.y, 0);

                            if(network.HasConnection(entry.Key, i)) {
                                Gizmos.DrawLine(center, dest);
                            } else {
                                Gizmos.DrawLine(center, dest);
                                Gizmos.DrawSphere(dest, 0.1f);
                            }
                        }
                    }
                });
            });
        });
    }

    private List<PipeNetwork> GetNetworksForGrid(Grid grid) {
        List<PipeNetwork> gridNetworks;
        if(!pipeNetworks.TryGetValue(grid, out gridNetworks)) {
            gridNetworks = new List<PipeNetwork>();
            pipeNetworks.Add(grid, gridNetworks);
        }

        return gridNetworks;
    }

    private PipeNetwork GetNetworkContainingPipe(List<PipeNetwork> gridNetworks, Vector2I location, BlockPipe pipe) {
        PipeNetwork result = null;
        foreach(PipeNetwork pipeNetwork in gridNetworks) {
            if(pipeNetwork.GetBlockPipe(location) != null) { 
                Debug.Assert(result == null, "Found a pipe that is in 2 networks at the same time");
                result = pipeNetwork;
            }
        }

        return result;
    }

    public void AddPipe(Grid grid, Vector2I location, BlockPipe pipeToAdd) {
        List<PipeNetwork> gridNetworks = GetNetworksForGrid(grid);

        // Try to join with an existing network
        List<PipeNetwork> connectedNetworks = new List<PipeNetwork>(4);
        foreach(PipeNetwork pipeNetwork in gridNetworks) {
            if(pipeNetwork.ConnectedNeighbors(location, pipeToAdd).Count > 0) {
                connectedNetworks.Add(pipeNetwork);
            }
        }

        if(connectedNetworks.Count == 0) {
            CreateNetwork(gridNetworks, location, pipeToAdd);
        } else if(connectedNetworks.Count == 1) {
            AddConnectingPipe(connectedNetworks[0], location, pipeToAdd);
        } else {
            MergePipeNetworks(gridNetworks, connectedNetworks, location, pipeToAdd);
        }
    }

    private PipeNetwork CreateNetwork(List<PipeNetwork> gridNetworks, Vector2I location, BlockPipe pipe) {
        lastNetworkID++;
        PipeNetwork pipeNetwork = new PipeNetwork(lastNetworkID, location, pipe);
        gridNetworks.Add(pipeNetwork);

        return pipeNetwork;
    }

    private PipeNetwork CreateNetwork(List<PipeNetwork> gridNetworks, IEnumerable<(Vector2I, BlockPipe)> pipes) {
        lastNetworkID++;
        PipeNetwork pipeNetwork = new PipeNetwork(lastNetworkID, pipes);
        gridNetworks.Add(pipeNetwork);

        return pipeNetwork;
    }

    public void RemovePipe(Grid grid, Vector2I location, BlockPipe pipeToRemove) {
        List<PipeNetwork> gridNetworks = GetNetworksForGrid(grid);
        PipeNetwork network = GetNetworkContainingPipe(gridNetworks, location, pipeToRemove);

        List<(Vector2I, BlockPipe)> testLocations = network.ConnectedNeighbors(location);
        network.RemovePipe(location);

        // Decompose network
        if(testLocations.Count == 0) {
            DeleteNetwork(gridNetworks, network);
        } else if(testLocations.Count == 1) {
            // Network needs no changes
        } else {
            for(int i = 1; i < testLocations.Count; i++) {
                (Vector2I testLocation, BlockPipe pipe) = testLocations[i];

                // Make sure the start loc still exists, it may have been added to a network already
                if(network.GetBlockPipe(testLocation) != null) {
                    var chain = network.GetConnectedChain(testLocation);
                    if(!chain.Contains(testLocations[0])) {
                        network.RemovePipes(chain);
                        CreateNetwork(gridNetworks, chain);
                    }
                }
            }
        }
    }

    private void AddConnectingPipe(PipeNetwork network, Vector2I location, BlockPipe pipe) {
        network.AddConnectingPipe(location, pipe);
    }

    private void MergePipeNetworks(List<PipeNetwork> gridNetworks, List<PipeNetwork> pipeNetworks, Vector2I location, BlockPipe pipe) {
        PipeNetwork primary = pipeNetworks[0];
        AddConnectingPipe(primary, location, pipe);

        for(int i = 1; i < pipeNetworks.Count; i++) {
            primary.AddPipeNetwork(pipeNetworks[i]);
            DeleteNetwork(gridNetworks, pipeNetworks[i]);
        }
    }

    private void DeleteNetwork(List<PipeNetwork> gridNetworks, PipeNetwork network) {
        Debug.Assert(network.pipes.Count == 0, "Deleteing network that still has pipes. Missing connections");
        gridNetworks.Remove(network);

    }
}
