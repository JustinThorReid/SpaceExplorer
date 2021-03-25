using Atmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(PipeConnections))]
public class BlockThruster : Block, Atmos.Atmospheric
{
    [SerializeField]
    private GameObject thrusterEfect;
    [SerializeField]
    private float internalTankSize = 1000;

    private bool hasFuel {
        get {
            return gasMix.GetPressure() > 10;
        }
    } //TODO: Eventually this should be a fuel interface with properties like thrust force, color, isp, etc
    private bool isActive;

    private GasMix gasMix;
    private PipeConnections connections;

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
        // Fill a local tank with fuel, gasMix or buckets worth
        List<GasMix> mixes = new List<GasMix>(connections.sockets.Length);
        mixes.Add(gasMix);

        foreach(ConnectionSocket socket in connections.sockets) {
            PipeNetwork network = ship.pipes.GetConnectingNetwork(blockPos + socket.relativeLoc, socket.dir);
            if(network != null) {
                mixes.Add(network.GetGasMix());
            }
        }

        GasMix.Mix(mixes);

        if(isActive && hasFuel) {
            gasMix.RemoveGas(Gas.Fuel, 1);
        }
    }
    public override void OnPlace(ShipManager ship, Vector2I gridBlockPos) {
        base.OnPlace(ship, gridBlockPos);

        ship.atmo.AddAtmosphericObject(this);
    }

    public override void OnRemove(ShipManager ship, Vector2I blockPos) {
        base.OnRemove(ship, blockPos);

        ship.atmo.RemoveAtmosphericObject(this);
    }

    private void Awake() {
        connections = GetComponent<PipeConnections>();
        gasMix = new GasMix(internalTankSize);
    }

    private void Update() {
        if(Input.GetButton("Jump")) {
            this.isActive = true;
        } else {
            this.isActive = false;
        }

        if(isActive && hasFuel) {
            thrusterEfect.SetActive(true);
        } else {
            thrusterEfect.SetActive(false);
        }
    }


}
