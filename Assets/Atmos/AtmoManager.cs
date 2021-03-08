using System;
using System.Collections.Generic;
using UnityEngine;

namespace Atmos
{
	/// <summary>
	/// Implemented by classes that contain gas in the world
	/// </summary>
	public class AtmoManager : MonoBehaviour
	{
		[SerializeField]
		private bool showDebug = false;

		private List<Atmospheric> atmoObjects = new List<Atmospheric>();




        // https://en.wikipedia.org/wiki/Altitude_sickness
        // http://www.mide.com/pages/air-pressure-at-altitude-calculator
        public static readonly float IDEAL_PRESSURE = 101.3f; // 1atm
        public static readonly float MIN_SURVIVABLE_PRESSURE = 54f; // "Extreme" altitude is 5000m or 54kpa
        public static readonly float MIN_SAFE_PRESSURE = 85f; // Mt everest is 0.333atm, sickness starts at 1500m or 84.821kpa or 0.85atm
        public static readonly float MAX_SAFE_PRESSURE = 500f;

        public static readonly float TILE_VOLUME = 2500; // Liters in a tile
        public static readonly float MIN_ACTIVE_MOLES = 0.001f;

        // People take in 21% oxy and output 16% oxy (normally)
        // So convert 5% of total oxy moles taken in in one breath to co2
        public static readonly float BREATH_VOLUME = 0.5f; // Liters in a player breath
        // You absorb 0.000218 moles of oxy at normal room kpa/temp


        public static readonly float T0C = 273.15f; /// 0degC
        public static readonly float T20C = 293.15f; /// 20degC - Room temp
        public static readonly float TCMB = 2.7f; /// -270.3degC - Space

        public static readonly float R_IDEAL_GAS_EQUATION = 8.31f;	//kPa*L/(K*mol)

        public static float GetMoles(float kpa, float liters, float kelvin) {
            return ((kpa * liters) / R_IDEAL_GAS_EQUATION) / kelvin;
        }


        private void OnGUI() {
			if(!showDebug)
				return;

			foreach(Atmospheric atmo in atmoObjects) {
				if(atmo is Component) {
					Vector3 pos = ((Component)atmo).transform.position;
					Vector2 screenPos = Camera.main.WorldToScreenPoint(pos);
					screenPos.y = Screen.height - screenPos.y; // GUI space is upsidedown from normal screen space
					atmo.DrawDebugGUI(screenPos);
                }
            }
		}

		public void AddAtmosphericObject(Atmospheric obj) {
			atmoObjects.Add(obj);
        }

		public void RemoveAtmosphericObject(Atmospheric obj) {
			atmoObjects.Remove(obj);
        }

	}
}
