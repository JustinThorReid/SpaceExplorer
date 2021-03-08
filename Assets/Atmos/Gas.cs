using System;

namespace Atmos
{
	public class Gas
	{
		public static readonly Gas Oxygen = new Gas("O2", 21.1f);
		public static readonly Gas Hydrogen = new Gas("H2", 20.4f);
		public static readonly Gas Fuel = new Gas("Fuel", 20.638f); // 34/66 o2/h2 https://thermtest.com/thermal-resources/rule-of-mixtures

		public String name { get; private set; }
		public float specificHeat { get; private set; }
		protected Gas(String name, float specificHeat) {
			this.name = name;
			this.specificHeat = specificHeat;
		}
	}
}
