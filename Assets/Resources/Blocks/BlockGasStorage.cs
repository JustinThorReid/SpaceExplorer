using Atmos;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockGasStorage : Block, Atmospheric
{
    [SerializeField]
    private float tankSizeLiters = 0;
    [SerializeField]
    private float initialFuelMols = 0;

    private GasMix gasMix;

    private void Awake() {
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
        throw new System.NotImplementedException();
    }
}
