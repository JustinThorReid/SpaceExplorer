using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteAlways]
public class PipeConnections : MonoBehaviour
{
    public static PipeNetworkManager pipeNetworkManager;
    private void Awake() {
        if(pipeNetworkManager == null)
            pipeNetworkManager = FindObjectOfType<PipeNetworkManager>();
    }

    /// <summary>
    /// For now all conections are generic
    /// In the future they should be grouped by type/name
    /// </summary>
    [SerializeField]
    private Vector2I[] pipeConnections;

    private void OnDrawGizmos() {
        if(Application.isPlaying)
            return;

        Gizmos.color = Color.yellow;
        foreach(Vector2I blockPos in pipeConnections) {
            Vector2I bottomLeft = blockPos;
            Vector2I topLeft = bottomLeft + Vector2I.UP;
            Vector2I topRight = topLeft + Vector2I.RIGHT;
            Vector2I bottomright = topRight + Vector2I.DOWN;
            Vector2 offset = new Vector2(transform.position.x, transform.position.y);

            Gizmos.DrawLine(offset + bottomLeft * Grid.UNITS_PER_BLOCK, offset + topLeft * Grid.UNITS_PER_BLOCK);
            Gizmos.DrawLine(offset + topLeft * Grid.UNITS_PER_BLOCK, offset + topRight * Grid.UNITS_PER_BLOCK);
            Gizmos.DrawLine(offset + topRight * Grid.UNITS_PER_BLOCK, offset + bottomright * Grid.UNITS_PER_BLOCK);
            Gizmos.DrawLine(offset + bottomright * Grid.UNITS_PER_BLOCK, offset + bottomLeft * Grid.UNITS_PER_BLOCK);
        }
    }

    public void OnPlace(ShipManager ship, Vector2I gridBlockPos) {
    }
}
