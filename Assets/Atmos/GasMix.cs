using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Atmos
{
	public class GasMix
	{
		public float volume { get; private set; }
		public float temperature { get; private set; }
		private Dictionary<Gas, float> mols = new Dictionary<Gas, float>();

		/// <summary>
		/// Create an empty GasMix with volume
		/// </summary>
		/// <param name="liters"></param>
		public GasMix(float liters) {
			Debug.Assert(liters > 0, "Gas mix with volume of 0 is not allowed.");
			this.volume = liters;
			this.temperature = AtmoManager.T20C;
		}

		public void AddGas(Gas type, float molsToAdd) {
			float currentMols = 0;
			mols.TryGetValue(type, out currentMols);
			mols.Add(type, currentMols + molsToAdd);

			// TODO: Raise/lower temp based on specific heat of other mix
        }

		public List<KeyValuePair<Gas, float>> GetCurrentGasses() {
			return mols.ToList();
		}

		/// <summary>
		/// Add up all moles from all contained gasses
		/// </summary>
		/// <returns></returns>
		internal float GetTotalMoles() {
			return mols.Values.Sum();
		}

		/// <summary>
		/// kilopascals
		/// </summary>
		/// <returns></returns>
		internal float GetPressure() {
			if(volume > 0) // to prevent division by zero
				return (GetTotalMoles() * AtmoManager.R_IDEAL_GAS_EQUATION * temperature) / volume;
			return 0;
		}

		/// <summary>
		/// Get specific heat of the mix. https://thermtest.com/thermal-resources/rule-of-mixtures
		/// </summary>
		/// <returns></returns>
		internal float GetSpecificHeat() {
			float totalMols = GetTotalMoles();
			float specificHeat = 0;
			mols.ToList().ForEach(pair => {
				specificHeat += (pair.Value / totalMols) * pair.Key.specificHeat;
			});

			return specificHeat;
        }
	}
}
