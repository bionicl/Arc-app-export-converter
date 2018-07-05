using System;
using System.Collections.Generic;
using System.IO;

namespace ArcAppExportConverter.BurningCal {

	public class BurnMETValue {
		public ActivityType activity;
		public float minAvgSpeed; //in km/s
		public float metValue;

		public BurnMETValue (string activity, float minAvgSpeed, float metValue) {
			Enum.TryParse(activity, out this.activity);
			this.minAvgSpeed = minAvgSpeed;
			this.metValue = metValue;
		}
		public BurnMETValue(string[] array) : this(array[0], float.Parse(array[1]), float.Parse(array[2])) {

		}
	}

	public class BurnMETValuesImporter {
		
	}
}
