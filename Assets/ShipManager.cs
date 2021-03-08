using Atmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AtmoManager))]
[RequireComponent(typeof(Grid))]
[RequireComponent(typeof(PipeNetworkManager))]
public class ShipManager : MonoBehaviour
{
    public AtmoManager atmo { get; private set; }
    public Grid grid { get; private set; }
    public PipeNetworkManager pipes { get; private set; }

    private void Awake() {
        atmo = GetComponent<AtmoManager>();
        grid = GetComponent<Grid>();
        pipes = GetComponent<PipeNetworkManager>();
    }
}
