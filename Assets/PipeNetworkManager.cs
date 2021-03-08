using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PipeNetworkManager : MonoBehaviour
{
    List<PipeNetwork> pipeNetworks = new List<PipeNetwork>();
    private int lastNetworkID = -1;
    [SerializeField]
    private bool showDebug = false;

    private void OnDrawGizmos() {
        if(!showDebug)
            return;

        Color[] colors = { Color.red, Color.blue, Color.green, Color.magenta, Color.white, Color.black};

        Grid grid = GetComponent<Grid>();
        pipeNetworks.ToList().ForEach(network => {
            Gizmos.color = colors[network.networkID % colors.Length];

            network.pipes.ToList().ForEach(entry => {
                Vector2 center = grid.ConvertBlockSpaceToWorldSpace(entry.Key) + Grid.BLOCK_OFFSET;

                for(sbyte i = 0; i < 4; i++) {
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
    }

    private void OnGUI() {
        if(!showDebug)
            return;

        Color[] colors = { Color.red, Color.blue, Color.green, Color.magenta, Color.white, Color.black };

        Grid grid = GetComponent<Grid>();
        pipeNetworks.ToList().ForEach(network => {
            Gizmos.color = colors[network.networkID % colors.Length];

            Vector3 pos = network.pipes.First().Value.transform.position;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(pos);
            screenPos.y = Screen.height - screenPos.y; // GUI space is upsidedown from normal screen space
            GUI.Label(new Rect(screenPos, new Vector2(100, 20)), "Net: " + network.networkID + " vol: " + network.GetGasVolume());
        });
    }

    private PipeNetwork GetNetworkContainingPipe(Vector2I location, BlockPipe pipe) {
        PipeNetwork result = null;
        foreach(PipeNetwork pipeNetwork in pipeNetworks) {
            if(pipeNetwork.GetBlockPipe(location) != null) { 
                Debug.Assert(result == null, "Found a pipe that is in 2 networks at the same time");
                result = pipeNetwork;
            }
        }

        return result;
    }

    public void AddPipe(Vector2I location, BlockPipe pipeToAdd) {
        // Try to join with an existing network
        List<PipeNetwork> connectedNetworks = new List<PipeNetwork>(4);
        foreach(PipeNetwork pipeNetwork in pipeNetworks) {
            if(pipeNetwork.ConnectedNeighbors(location, pipeToAdd).Count > 0) {
                connectedNetworks.Add(pipeNetwork);
            }
        }

        if(connectedNetworks.Count == 0) {
            CreateNetwork(location, pipeToAdd);
        } else if(connectedNetworks.Count == 1) {
            AddConnectingPipe(connectedNetworks[0], location, pipeToAdd);
        } else {
            MergePipeNetworks(connectedNetworks, location, pipeToAdd);
        }
    }

    private PipeNetwork CreateNetwork(Vector2I location, BlockPipe pipe) {
        lastNetworkID++;
        PipeNetwork pipeNetwork = new PipeNetwork(lastNetworkID, location, pipe);
        pipeNetworks.Add(pipeNetwork);

        return pipeNetwork;
    }

    private PipeNetwork CreateNetwork(PipeNetwork originalNetwork, IEnumerable<(Vector2I, BlockPipe)> pipes) {
        lastNetworkID++;
        PipeNetwork pipeNetwork = new PipeNetwork(lastNetworkID, originalNetwork, pipes);
        pipeNetworks.Add(pipeNetwork);

        Debug.Assert(!originalNetwork.pipes.ContainsKey(pipes.First().Item1), "Original network still has pipes after creating new network");

        return pipeNetwork;
    }

    public void RemovePipe(Vector2I location, BlockPipe pipeToRemove) {
        PipeNetwork network = GetNetworkContainingPipe(location, pipeToRemove);

        List<(Vector2I, BlockPipe)> testLocations = network.ConnectedNeighbors(location);
        network.RemovePipe(location);

        // Decompose network
        if(testLocations.Count == 0) {
            DeleteNetwork(network);
        } else if(testLocations.Count == 1) {
            // Network needs no changes
        } else {
            for(int i = 1; i < testLocations.Count; i++) {
                (Vector2I testLocation, BlockPipe pipe) = testLocations[i];

                // Make sure the start loc still exists, it may have been added to a network already
                if(network.GetBlockPipe(testLocation) != null) {
                    var chain = network.GetConnectedChain(testLocation);
                    if(!chain.Contains(testLocations[0])) {
                        CreateNetwork(network, chain);
                    }
                }
            }
        }
    }

    private void AddConnectingPipe(PipeNetwork network, Vector2I location, BlockPipe pipe) {
        network.AddConnectingPipe(location, pipe);
    }

    private void MergePipeNetworks(List<PipeNetwork> pipeNetworksToMerge, Vector2I location, BlockPipe pipe) {
        PipeNetwork primary = pipeNetworksToMerge[0];
        AddConnectingPipe(primary, location, pipe);

        for(int i = 1; i < pipeNetworksToMerge.Count; i++) {
            primary.AddPipeNetwork(pipeNetworksToMerge[i]);
            DeleteNetwork(pipeNetworksToMerge[i]);
        }
    }

    private void DeleteNetwork(PipeNetwork network) {
        Debug.Assert(network.pipes.Count == 0, "Deleteing network that still has pipes. Missing connections");
        pipeNetworks.Remove(network);

    }

    /// <summary>
    /// Get the network if any that has a pipe in from pos with an opening in toDir
    /// </summary>
    /// <param name="from"></param>
    /// <param name="toDir"></param>
    /// <returns></returns>
    public PipeNetwork GetConnectingNetwork(Vector2I from, sbyte toDir) {
        PipeNetwork networkWithPipe = pipeNetworks.Find(network => {
            return network.pipes.ContainsKey(from);
        });

        if(networkWithPipe == null) {
            return null;
        }

        BlockPipe pipe = networkWithPipe.GetBlockPipe(from);
        Debug.Assert(pipe != null, "Network contains from key but no pipe found");

        if(pipe.HasConnectionPoint(toDir)) {
            return networkWithPipe;
        }
        return null;
    }
}
