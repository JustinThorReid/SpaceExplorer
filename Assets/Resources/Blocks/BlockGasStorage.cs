using Atmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PipeConnections))]
public class BlockGasStorage : Block, Atmospheric
{
    [SerializeField]
    private float tankSizeLiters = 0;
    [SerializeField]
    private float initialFuelMols = 0;

    private GasMix gasMix;
    private PipeConnections connections;

    private void Awake() {
        connections = GetComponent<PipeConnections>();
        gasMix = new GasMix(tankSizeLiters);

        if(initialFuelMols > 0)
            gasMix.AddGas(Gas.Fuel, initialFuelMols);
    }

    public override void OnPlace(ShipManager ship, Vector2I gridBlockPos) {
        base.OnPlace(ship, gridBlockPos);

        ship.atmo.AddAtmosphericObject(this);
    }

    public override void OnRemove(ShipManager ship, Vector2I blockPos) {
        base.OnRemove(ship, blockPos);

        ship.atmo.RemoveAtmosphericObject(this);
    }

    void Atmospheric.DrawDebugGUI(Vector2 location) {
        const float step = 20;

        GUI.Label(new Rect(location, new Vector2(150, step)), "Vol: " + gasMix.volume);
        location.y += step;
        GUI.Label(new Rect(location, new Vector2(150, step)), "Temp: " + gasMix.temperature);
        location.y += step;
        GUI.Label(new Rect(location, new Vector2(150, step)), "Pres: " + gasMix.GetPressure());

        foreach(GasCount gas in gasMix.GetCurrentGasses()) {
            location.y += step;
            GUI.Label(new Rect(location, new Vector2(150, step)), gas.gas.name + ": " + gas.mols);
        }
    }

    void Atmospheric.Tick() {
        List<GasMix> mixes = new List<GasMix>(connections.sockets.Length);
        mixes.Add(gasMix);

        foreach(ConnectionSocket socket in connections.sockets) {
            PipeNetwork network = ship.pipes.GetConnectingNetwork(blockPos + socket.relativeLoc, socket.dir);
            if(network != null) {
                mixes.Add(network.GetGasMix());
            }
        }

        GasMix.Mix(mixes);
    }
}
