using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class XmlTimeline {

	public enum TimelineItemType {
		place,
		activity
	}
	public class TimelineItem {
		public TimelineItemType type;
		public Place place;
		public Activity activity;

		public TimelineItem (Place place) {
			type = TimelineItemType.place;
			this.place = place;
		}

		public TimelineItem (Activity activity) {
			type = TimelineItemType.activity;
			this.activity = activity;
		}

		public override string ToString() {
			if (type == TimelineItemType.activity)
				return activity.ToString();
			else
				return place.ToString();
		}
	}

	public class Coordinates {
		public double lat;
		public double lon;
		public string ele = null;
		public DateTime? time = null;

		public Coordinates(string lat, string lon) {
			this.lat = Convert.ToDouble(lat);
			this.lon = Convert.ToDouble(lon);
		}
		public override string ToString() {
			return string.Format("Lat: {0}   Lon: {1}  Ele: {2}", lat, lon, ele);
		}
	}
	public class Place {
		public int placeID;
		public string name;
		public Coordinates position;
		public DateTime? startTime;
		public DateTime? endTime;
		public int Duration {
			get {
				if (!startTime.HasValue || !endTime.HasValue) {
					Console.WriteLine("Couldn't get duration in place " + name);
					return 0;
				}
				TimeSpan duration = endTime.Value - startTime.Value;
				return (int)duration.TotalSeconds;
			}
		}

		public Place(Coordinates position, string name, DateTime? startTime = null) {
			this.position = position;
			this.startTime = startTime;
			this.name = name;
		}

		public override string ToString() {
			string time = "";
			if (startTime.HasValue)
				time = startTime.Value.ToString("HH:mm");
			else
				time = "??:??";
			time += "-";
			if (endTime.HasValue)
				time += endTime.Value.ToString("HH:mm");
			else
				time += "??:??";
			return string.Format("{0} Place: Name={1}, Duration={2}", time, name, Duration);
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
				int temp = BurnedCalCalculator.Calcualate(activity, Duration, Speed, MainClass.weight);
				calories = temp;
				return calories.Value;
			}
		}
		public float Speed {
			get {
				return Distance * 1000 / (float)Duration * 3.6f;
			}
		}

		public Activity(ActivityType activity, Coordinates[] waypoints) {
			this.activity = activity;
			this.waypoints = waypoints;
			startTime = waypoints[0].time.Value;
			endTime = waypoints[waypoints.Length - 1].time.Value;
		}

		public void MargeWithNew(Coordinates[] waypoints) {
			List<Coordinates> tempMerge = waypoints.ToList();
			foreach (var item in waypoints) {
				tempMerge.Add(item);
			}
			this.waypoints = tempMerge.ToArray();
			endTime = waypoints[waypoints.Length - 1].time.Value;
			distance = null;
			calories = null;
		}

		public override string ToString() {
			return string.Format("{0}-{1} Activity: {2}, Duration={3}, Distance={4}, Calories={5}, Speed={6}]", startTime, endTime, activity, Duration.ToString("HH:mm"), Distance, Calories, Speed);
		}
	}
}

public class XmlReader {
	static List<XmlTimeline.TimelineItem> timelineItems = new List<XmlTimeline.TimelineItem>();

	public static void LoadFile(string path = "def") {
		StreamReader sr = new StreamReader("2018-07-04.gpx");

		// Ignore first 2 lines
		sr.ReadLine();
		sr.ReadLine();

		// Loop
		timelineItems.Clear();
		while (true) {
			string line = sr.ReadLine().Replace("\t", "");
			if (line.StartsWith("<wpt", StringComparison.Ordinal))
				GetPlace(line, sr);
			else if (line.StartsWith("<trk", StringComparison.Ordinal))
				GetMove(sr);
			else if (line.StartsWith("</gpx", StringComparison.Ordinal))
				break;
		};
		sr.Close();
		SetStartEnd();
		foreach (var item in timelineItems) {
			if (item.type == XmlTimeline.TimelineItemType.activity)
				Console.ForegroundColor = ConsoleColor.Red;
			else
				Console.ForegroundColor = ConsoleColor.DarkBlue;
			Console.WriteLine(item.ToString());
		}
	}
	static void GetPlace(string line, StreamReader sr) {
		XmlTimeline.Coordinates location = HelpMethods.GetLatLon(line);
		string name = "";
		if (!line.EndsWith("/>", StringComparison.Ordinal)) {
			string nameLine = sr.ReadLine().Replace("\t", "");
			name = HelpMethods.LeaveCenterFromString(nameLine, "<name>", "</name>");
			sr.ReadLine();
		}
		DateTime? startTime = null;
		if (timelineItems.Count >= 1 && timelineItems.Last().type == XmlTimeline.TimelineItemType.activity)
			startTime = timelineItems.Last().activity.endTime;
		timelineItems.Add(new XmlTimeline.TimelineItem(new XmlTimeline.Place(location, name, startTime)));

	}
	static void GetMove(StreamReader sr) {
		// Type
		string typeLine = sr.ReadLine().Replace("\t", "");
		if (typeLine == "<trkseg />") {
			sr.ReadLine();
			return;
		}
		typeLine = HelpMethods.LeaveCenterFromString(typeLine, "<type>", "</type>");
		ActivityType type = ActivityType.walking;
		Enum.TryParse(typeLine, out type);

		// Track points
		string line = sr.ReadLine().Replace("\t", "");
		List<XmlTimeline.Coordinates> coords = new List<XmlTimeline.Coordinates>();
		while (true) {
			line = sr.ReadLine().Replace("\t", "");
			if (line == "</trkseg>")
				break;
			else
				AddWaypoint(line, sr, coords);
		}
		if (coords.Count >= 2) {
			if (timelineItems[timelineItems.Count - 1].type == XmlTimeline.TimelineItemType.activity &&
				timelineItems[timelineItems.Count - 1].activity.activity == type) {
				timelineItems[timelineItems.Count - 1].activity.MargeWithNew(coords.ToArray());
			} else {
				XmlTimeline.Activity newActivity = new XmlTimeline.Activity(type, coords.ToArray());
				AddTimeToPreviousPlace(newActivity);
				timelineItems.Add(new XmlTimeline.TimelineItem(newActivity));
				AddTimeToPreviousPlace(newActivity);
			}
		}
		sr.ReadLine();
	}
	static void AddWaypoint(string line, StreamReader sr, List<XmlTimeline.Coordinates> coords) {
		XmlTimeline.Coordinates location = HelpMethods.GetLatLon(line);
		location.ele = HelpMethods.LeaveCenterFromString(sr.ReadLine().Replace("\t", ""), "<ele>", "</ele>");
		location.time = HelpMethods.ParseIso8601(
			HelpMethods.LeaveCenterFromString(
				sr.ReadLine().Replace("\t", ""),
				"<time>",
				"</time>"));
		sr.ReadLine();
		coords.Add(location);
	}
	static void AddTimeToPreviousPlace(XmlTimeline.Activity activity) {
		if (timelineItems.Count >= 1) {
			if (timelineItems.Last().type == XmlTimeline.TimelineItemType.place)
				timelineItems.Last().place.endTime = activity.startTime;
		}
	}
	static void SetStartEnd() {
		if (timelineItems.First().type == XmlTimeline.TimelineItemType.place) {
			DateTime time = timelineItems.First().place.endTime.Value;
			DateTime newTime = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, time.Kind);
			timelineItems.First().place.startTime = newTime;
		}

		if (timelineItems.Last().type == XmlTimeline.TimelineItemType.place) {
			DateTime time = timelineItems.Last().place.startTime.Value;
			DateTime newTime = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59, time.Kind);
			timelineItems.Last().place.endTime = newTime;
		}
	}
}

public static class HelpMethods {

	// Copied from
	// https://stackoverflow.com/questions/6366408/calculating-distance-between-two-latitude-and-longitude-geocoordinates

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

	public static XmlTimeline.Coordinates GetLatLon(string line) {
		string lat = "";
		string lon = "";
		bool captureMode = false;
		string capture = "";
		foreach (char item in line) {
			if (item == '"') {
				captureMode = !captureMode;
				if (captureMode == false) {
					if (lat == "")
						lat = capture;
					else
						lon = capture;
					capture = "";
				}
			} else if (captureMode) {
				capture += item;
			}
		}
		return new XmlTimeline.Coordinates(lat, lon);
	}
	public static string LeaveCenterFromString(string text, string removeLeft, string removeRight) {
		string temp = text;
		temp = temp.Replace(removeLeft, "");
		temp = temp.Replace(removeRight, "");
		return temp;
	}
}
