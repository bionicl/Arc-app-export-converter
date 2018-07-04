using System;

public class XmlTimeline {
	public class Coordinates {
		public double lat;
		public double lon;
		public string ele = null;
		public DateTime time;

	}
	public class Place {
		public int placeID;
		public string name;
		public Coordinates position;
		public DateTime startTime;
		public DateTime endTime;
		public int Duration {
			get {
				TimeSpan duration = endTime - startTime;
				return (int)duration.TotalSeconds;
			}
		}
	}
	public class Activity {
		public ActivityType activity;
		public Coordinates[] waypoints;
		public DateTime startTime;
		public DateTime endTime;

		float? distance;
		int? calories;
		float CalculateDistance() {
			if (waypoints.Length <= 1)
				return 0;
			float totalDistance = 0;
			for (int i = 0; i < waypoints.Length - 1; i++) {
				totalDistance += HelpMethods.DistanceTo(waypoints[i], waypoints[i + 1]);
			}
			return totalDistance;
		}


		public int Duration {
			get {
				if (endTime == null)
					endTime = DateTime.Now;
				TimeSpan duration = endTime - startTime;
				return (int)duration.TotalSeconds;
			}
		}
		public float Distance {
			get {
				if (distance.HasValue)
					return distance.Value;
				float temp = CalculateDistance();
				distance = temp;
				return distance.Value;
			}
		}
		public int Calories {
			get {
				if (calories.HasValue)
					return calories.Value;
				int temp = BurnedCalCalculator.Calcualate(activity, Duration, Distance, MainClass.weight);
				calories = temp;
				return calories.Value;
			}
		}
		public float Speed {
			get {
				return Distance / (float)Duration * 3.6f;
			}
		}
	}
}

public class XmlReader {
	
}

// Copied from
// https://stackoverflow.com/questions/6366408/calculating-distance-between-two-latitude-and-longitude-geocoordinates

public static class HelpMethods {
	public static float DistanceTo(XmlTimeline.Coordinates wp1, XmlTimeline.Coordinates wp2, char unit = 'K') {
		double rlat1 = Math.PI * wp1.lat / 180;
		double rlat2 = Math.PI * wp2.lat / 180;
		double theta = wp1.lon - wp2.lon;
		double rtheta = Math.PI * theta / 180;
		double dist =
			Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
			Math.Cos(rlat2) * Math.Cos(rtheta);
		dist = Math.Acos(dist);
		dist = dist * 180 / Math.PI;
		dist = dist * 60 * 1.1515;

		float distF = (float)dist;

		switch (unit) {
			case 'K': //Kilometers -> default
				return distF * 1.609344f;
			case 'N': //Nautical Miles 
				return distF * 0.8684f;
			case 'M': //Miles
				return distF;
		}

		return distF;
	}

	public static DateTime ParseIso8601 (string iso8601Time) {
		return DateTime.Parse(iso8601Time, null, System.Globalization.DateTimeStyles.RoundtripKind);
	}
}
