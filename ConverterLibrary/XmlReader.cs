using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public class XmlTimeline {

	public enum TimelineItemType {
		place,
		activity
	}
	public class TimelineItem {
		public TimelineItemType type;
		public Place place;
		public Activity activity;

		public TimelineItem(Place place) {
			type = TimelineItemType.place;
			this.place = place;
		}

		public TimelineItem(Activity activity) {
			type = TimelineItemType.activity;
			this.activity = activity;
		}

		public override string ToString() {
			if (type == TimelineItemType.activity)
				return activity.ToString();
			else
				return place.ToString();
		}

		//Time
		public DateTime ReturnDate() {
			DateTime tempDate = new DateTime();
			if (type == TimelineItemType.activity)
				tempDate = activity.startTime;
			else
				tempDate = place.startTime.Value;
			return new DateTime(tempDate.Year, tempDate.Month, tempDate.Day, 12, 0, 0, tempDate.Kind);
		}

		public DateTime EndTime {
			get {
				if (type == TimelineItemType.activity)
					return activity.endTime;
				else
					return place.endTime.Value;
			}
			set {
				if (type == TimelineItemType.activity)
					activity.endTime = value;
				else
					place.endTime = value;
			}
		}

		public DateTime StartTime {
			get {
				if (type == TimelineItemType.activity)
					return activity.startTime;
				else
					return place.startTime.Value;
			}
			set {
				if (type == TimelineItemType.activity)
					activity.startTime = value;
				else
					place.startTime = value;
			}
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
		public Coordinates(double lat, double lon) {
			this.lat = lat;
			this.lon = lon;
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
		public string ele;
		public string link;

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

		public Place(Coordinates position, string name, DateTime? startTime = null, string ele = null, string link = null) {
			this.position = position;
			this.startTime = startTime;
			this.name = name;
			this.link = link;
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
				int temp = BurnedCalCalculator.Calcualate(activity, Duration, Speed, 80);
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
			List<Coordinates> tempMerge = this.waypoints.ToList();
			foreach (var item in waypoints) {
				tempMerge.Add(item);
			}
			this.waypoints = tempMerge.ToArray();
			endTime = waypoints[waypoints.Length - 1].time.Value;
			distance = null;
			calories = null;
		}

		public override string ToString() {
			return string.Format("{0}-{1} Activity: {2}, Duration={3}, Distance={4}, Calories={5}, Speed={6}]", startTime.ToString("HH:mm"), endTime.ToString("HH:mm"), activity, Duration, Distance, Calories, Speed);
		}
	}
}

public class ActivitySummary {
	public ActivityType activity;
	public float duration;
	public float distance;
	public float calories;

	public ActivitySummary(List<XmlTimeline.Activity> list, int id) {
		activity = (ActivityType)id;
		foreach (var item in list) {
			duration += item.Duration;
		}
		foreach (var item in list) {
			distance += item.Distance;
		}
		foreach (var item in list) {
			calories += item.Calories;
		}
	}

	public override string ToString() {
		return string.Format("Activity: {0}  Duration: {1}, Distance: {2}, Calories: {3}", activity, duration, distance, calories);
	}

	public JsonMoves.Day.Summary ToMoves() {
		return new JsonMoves.Day.Summary(activity, duration, distance, calories);
	}
}

public class XmlReader {
	public List<XmlTimeline.TimelineItem> timelineItems = new List<XmlTimeline.TimelineItem>();
	List<XmlTimeline.Activity>[] activitySummary = new List<XmlTimeline.Activity>[10];
	public DateTime date;
	public ActivitySummary[] summary = new ActivitySummary[10];
	public string originalName;
	public float weight;

	// Activity and places loading
	public XmlReader(string path, bool isPath, float weight) {
		this.weight = weight;
		if (isPath) {
			originalName = path.Replace(".gpx", "");
			LoadFile(path);
		} else {
			LoadString(path);
		}
	}
	public XmlReader(List<XmlTimeline.TimelineItem> timelineItems) {
		for (int i = 0; i < 10; i++) {
			activitySummary[i] = new List<XmlTimeline.Activity>();
		}
		this.timelineItems = timelineItems;
		//SetStartEnd();
		SetSummary();
		SetXmlDate();
	}

	public void Load(StreamReader sr) {
		for (int i = 0; i < 10; i++) {
			activitySummary[i] = new List<XmlTimeline.Activity>();
		}

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
		SetSummary();
		SetXmlDate();

		//Display();
	}

	public void LoadString(string text) {
		MemoryStream mStrm = new MemoryStream(Encoding.UTF8.GetBytes(text));
		StreamReader tempSteamR = new StreamReader(mStrm, System.Text.Encoding.UTF8, true);
		Load(tempSteamR);
	}
	public void LoadFile(string path) {
		StreamReader sr = new StreamReader(path);
		Load(sr);
	}

	void GetPlace(string line, StreamReader sr) {
		XmlTimeline.Coordinates location = HelpMethods.GetLatLon(line);

		// time
		DateTime startTime = new DateTime();
		string tempLine = sr.ReadLine().Replace("\t", "");
		if (!tempLine.StartsWith("<time>")) {
			while (tempLine != "</wpt>") {
				tempLine = sr.ReadLine().Replace("\t", "");
			}
			return;
		}
		startTime = HelpMethods.ParseIso8601(
			HelpMethods.LeaveCenterFromString(
				tempLine,
				"<time>",
				"</time>"));

		// ele
		string ele = "";
		tempLine = sr.ReadLine().Replace("\t", "");
		if (tempLine.StartsWith("<ele>")) {
			ele = HelpMethods.LeaveCenterFromString(tempLine, "<ele>", "</ele>");
			tempLine = sr.ReadLine().Replace("\t", "");
		}

		// name (if exist)
		string name = "";
		if (tempLine.StartsWith("<name>")) {
			name = HelpMethods.LeaveCenterFromString(tempLine.Replace("&amp;", "&").Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'"), "<name>", "</name>");
			tempLine = sr.ReadLine().Replace("\t", "");
		}
		string link = "";
		if (tempLine.StartsWith("<link")) {
			link = HelpMethods.LeaveCenterFromString(tempLine, "<link href=\"", "\" />");
			tempLine = sr.ReadLine().Replace("\t", "");
		}

		// If previous is place
		if (timelineItems.Count >= 1 && timelineItems.Last().type == XmlTimeline.TimelineItemType.place)
			timelineItems.Last().place.endTime = startTime;

		//if (timelineItems.Count >= 1 && timelineItems.Last().type == XmlTimeline.TimelineItemType.activity)
		//	startTime = timelineItems.Last().activity.endTime;
		timelineItems.Add(new XmlTimeline.TimelineItem(new XmlTimeline.Place(location, name, startTime, ele, link)));

	}
	void GetMove(StreamReader sr) {
		// Type
		string line = sr.ReadLine().Replace("\t", "");
		if (line == "<trkseg />") {
			sr.ReadLine();
			return;
		}
		ActivityType type = ActivityType.car;
		if (line.StartsWith("<type>", StringComparison.CurrentCulture)) {
			line = HelpMethods.LeaveCenterFromString(line, "<type>", "</type>");
			Enum.TryParse(line, out type);

			// Track points
			line = sr.ReadLine().Replace("\t", "");
		}
		if (line == "<trkseg />") {
			sr.ReadLine();
			return;
		}
		List<XmlTimeline.Coordinates> coords = new List<XmlTimeline.Coordinates>();
		while (true) {
			line = sr.ReadLine().Replace("\t", "");
			if (line == "</trkseg>")
				break;
			else {
				AddWaypoint(line, sr, coords);
			}
		}
		if (coords.Count >= 2) {
			if (timelineItems.Count > 0 &&
				timelineItems[timelineItems.Count - 1].type == XmlTimeline.TimelineItemType.activity &&
				timelineItems[timelineItems.Count - 1].activity.activity == type) {
				timelineItems[timelineItems.Count - 1].activity.MargeWithNew(coords.ToArray());
			} else {
				XmlTimeline.Activity newActivity = new XmlTimeline.Activity(type, coords.ToArray());
				AddTimeToPreviousPlace(newActivity);
				timelineItems.Add(new XmlTimeline.TimelineItem(newActivity));
				AddTimeToPreviousPlace(newActivity);
				//activitySummary[(int)type].Add(newActivity);
			}
		}
		sr.ReadLine();
	}
	void AddWaypoint(string line, StreamReader sr, List<XmlTimeline.Coordinates> coords) {
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
	void AddTimeToPreviousPlace(XmlTimeline.Activity activity) {
		if (timelineItems.Count >= 1) {
			if (timelineItems.Last().type == XmlTimeline.TimelineItemType.place)
				timelineItems.Last().place.endTime = activity.startTime;
		}
	}

	// End calculations
	void SetStartEnd() {
		if (timelineItems.First().type == XmlTimeline.TimelineItemType.place && !timelineItems.First().place.startTime.HasValue) {
			DateTime time = timelineItems.First().place.endTime.Value;
			DateTime newTime = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0, time.Kind);
			timelineItems.First().place.startTime = newTime;
		}

		if (timelineItems.Last().type == XmlTimeline.TimelineItemType.place && !timelineItems.Last().place.endTime.HasValue) {
			DateTime time = timelineItems.Last().place.startTime.Value;
			DateTime newTime = new DateTime(time.Year, time.Month, time.Day, 23, 59, 59, time.Kind);
			timelineItems.Last().place.endTime = newTime;
		}
	}
	void SetXmlDate() {
		DateTime tempDate = new DateTime();

		if (timelineItems.Count > 1) {
			tempDate = timelineItems.First().EndTime;
		} else {
			DateTime tempEndTime = timelineItems[0].EndTime;
			if (tempEndTime.Day - timelineItems[0].StartTime.Day == 2) {
				tempDate = timelineItems.First().StartTime;
				tempDate.AddDays(1);
			} else if (tempEndTime.Hour == 23 && tempEndTime.Minute == 59 && tempEndTime.Second == 59)
				tempDate = tempEndTime;
			else
				tempDate = timelineItems[0].StartTime;
		}

		date = new DateTime(tempDate.Year, tempDate.Month, tempDate.Day, 12, 0, 0, tempDate.Kind);
	}
	void SetSummary() {
		foreach (var item in timelineItems) {
			if (item.type == XmlTimeline.TimelineItemType.activity) {
				activitySummary[(int)item.activity.activity].Add(item.activity);
			}
		}
		for (int i = 0; i < 10; i++) {
			summary[i] = new ActivitySummary(activitySummary[i], i);
		}
	}

	// Export options
	void Display() {
		foreach (var item in timelineItems) {
			if (item.type == XmlTimeline.TimelineItemType.activity)
				Console.ForegroundColor = ConsoleColor.Red;
			else
				Console.ForegroundColor = ConsoleColor.DarkBlue;
			Console.WriteLine(item.ToString());
		}
		Console.WriteLine();
		Console.ForegroundColor = ConsoleColor.DarkGray;
		foreach (var item in summary) {
			if (item.duration > 0)
				Console.WriteLine(item.ToString());
		}
	}
	public void ParseToJson() {
		List<XmlReader> tempList = new List<XmlReader> {
			this
		};
		JsonParser.Parse(tempList, originalName + ".json");
	}

	// Split xml file
	public static List<XmlReader> Split(XmlReader xml) {
		List<XmlTimeline.TimelineItem> timelineItems = xml.timelineItems;

		DateTime? currentDate = null;
		List<XmlReader> output = new List<XmlReader>();
		List<XmlTimeline.TimelineItem> tempList = new List<XmlTimeline.TimelineItem>();
		XmlTimeline.TimelineItem lastItem = timelineItems[0];

		foreach (var item in timelineItems) {
			if (!currentDate.HasValue) {
				currentDate = item.ReturnDate();
			}

			if (currentDate == item.ReturnDate()) {
				tempList.Add(item);
				lastItem = item;
			} else {
				XmlReader tempXml = new XmlReader(tempList.ToArray().ToList()); // stupid workaround to clone
				output.Add(tempXml);
				tempList.Clear();
				currentDate = item.ReturnDate();
				tempList.Add(lastItem);
				tempList.Add(item);
				lastItem = item;
			}
		}
		output.Add(new XmlReader(tempList.ToArray().ToList()));
		tempList.Clear();

		return output;
	}
}