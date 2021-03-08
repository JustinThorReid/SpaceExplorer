using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public struct ConnectionSocket
{
    public Vector2I relativeLoc;
    public sbyte dir;
}

[RequireComponent(typeof(Block))]
[ExecuteAlways]
public class PipeConnections : MonoBehaviour
{
    /// <summary>
    /// For now all conections are generic
    /// In the future they should be grouped by type/name
    /// </summary>
    [SerializeField]
    [Header("Locations in small block system")]
    private Vector2I[] _connectionPoints;
    [HideInInspector]
    public ConnectionSocket[] socketsOriginal;
    public ConnectionSocket[] sockets;

    private void Awake() {
        socketsOriginal = new ConnectionSocket[_connectionPoints.Length];
        Block block = GetComponent<Block>();
        for(int i = 0; i < _connectionPoints.Length; i++) {
            Vector2I loc = _connectionPoints[i];
            socketsOriginal[i].relativeLoc = loc;

            if(loc.x < 0) {
                socketsOriginal[i].dir = Vector2I.DIR_RIGHT;
            } else if(loc.x >= block.SmallSize.x) {
                socketsOriginal[i].dir = Vector2I.DIR_LEFT;
            } else if(loc.y < 0) {
                socketsOriginal[i].dir = Vector2I.DIR_UP;
            } else {
                socketsOriginal[i].dir = Vector2I.DIR_DOWN;
            }
        }
    }

    public void OnPlace() {
        Block block = GetComponent<Block>();
        sockets = new ConnectionSocket[socketsOriginal.Length];
        for(int i = 0; i < socketsOriginal.Length; i++) {
            sockets[i].dir = Vector2I.RotateDirection(socketsOriginal[i].dir, block.rotation);
            sockets[i].relativeLoc = block.RotateBlockSmall(socketsOriginal[i].relativeLoc);
        }
    }

    private void OnDrawGizmos() {
        if(Application.isPlaying)
            return;

        Block block = GetComponent<Block>();
        Gizmos.color = Color.yellow;
        foreach(Vector2I blockPos in _connectionPoints) {
            Vector2I bottomLeft = blockPos;
            Vector2I topLeft = bottomLeft + Vector2I.UP;
            Vector2I topRight = topLeft + Vector2I.RIGHT;
            Vector2I bottomright = topRight + Vector2I.DOWN;
            Vector2 offset = new Vector2(transform.position.x, transform.position.y) - block.SmallSize * Grid.UNITS_PER_BLOCK * 0.5f;

            Gizmos.DrawLine(offset + bottomLeft * Grid.UNITS_PER_BLOCK, offset + topLeft * Grid.UNITS_PER_BLOCK);
            Gizmos.DrawLine(offset + topLeft * Grid.UNITS_PER_BLOCK, offset + topRight * Grid.UNITS_PER_BLOCK);
            Gizmos.DrawLine(offset + topRight * Grid.UNITS_PER_BLOCK, offset + bottomright * Grid.UNITS_PER_BLOCK);
            Gizmos.DrawLine(offset + bottomright * Grid.UNITS_PER_BLOCK, offset + bottomLeft * Grid.UNITS_PER_BLOCK);
        }
    }

}
