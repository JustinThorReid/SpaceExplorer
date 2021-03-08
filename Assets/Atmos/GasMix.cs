using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Atmos
{
	public class GasCount
	{
		public Gas gas;
		public float mols = 0;
		internal GasCount(Gas type) {
			gas = type;
        }
    }

	public class GasMix
	{
		public float volume { get; private set; }
		public float temperature { get; private set; }
		private List<GasCount> mols = new List<GasCount>();

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
			var existing = mols.Find(pair => {
				return pair.gas == type;
			});
			if(existing == null) {
				existing = new GasCount(type);
				mols.Add(existing);
            }

			existing.mols += molsToAdd;

			// TODO: Raise/lower temp based on specific heat of other mix
        }

		/// <summary>
		/// Take all gas and volume from other. Does not delete other, or change its volume, does remove other's gasses.
		/// </summary>
		/// <param name="other"></param>
		public void Absorb(GasMix other) {
			this.volume += other.volume;

			other.mols.ForEach(gas => {
				this.AddGas(gas.gas, gas.mols);
			});
			other.mols.Clear();
        }

		/// <summary>
		/// Add volume to this mix
		/// </summary>
		/// <param name="volumeToAd"></param>
		public void ChangeVolume(float volumeToAdd) {
			volume += volumeToAdd;
        }

		public IReadOnlyList<GasCount> GetCurrentGasses() {
			return mols.AsReadOnly();
		}

		/// <summary>
		/// Add up all moles from all contained gasses
		/// </summary>
		/// <returns></returns>
		internal float GetTotalMoles() {
			return mols.Sum(gc => gc.mols);
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
			mols.ToList().ForEach(gasCount => {
				specificHeat += (gasCount.mols / totalMols) * gasCount.gas.specificHeat;
			});

			return specificHeat;
        }

		/// <summary>
		/// Split this gas mix by removing some volume, like cutting a pipe in half.
		/// </summary>
		/// <param name="volumeToRemove"></param>
		/// <returns></returns>
		public GasMix SplitGasMix(float volumeToRemove) {
			volumeToRemove = Mathf.Min(volumeToRemove, volume);
			float pcntToTake = (volumeToRemove / volume);
			float pcntToKeep = 1 - pcntToTake;

			GasMix result = new GasMix(volumeToRemove);
			volume -= volumeToRemove;

			mols.ForEach(gasCount => { 
				result.AddGas(gasCount.gas, gasCount.mols * pcntToTake);
				gasCount.mols *= pcntToKeep;
			});

			return result;
		}
	}
}
