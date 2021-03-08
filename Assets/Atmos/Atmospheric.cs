using System;
using UnityEngine;

namespace Atmos
{
	/// <summary>
	/// Implemented by classes that contain gas in the world
	/// </summary>
	public interface Atmospheric
	{
		/// <summary>
		/// Called during the atmos tick on the atmos thread
		/// </summary>
		public void Tick();

		/// <summary>
		/// Draw internal state information at the location
		/// </summary>
		/// <param name="location"></param>
		public void DrawDebugGUI(Vector2 guiPoint);
	}
}
