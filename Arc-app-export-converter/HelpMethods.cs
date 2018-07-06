using System;

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

	public static DateTime ParseIso8601(string iso8601Time) {
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

	public static string ConvertToIso1601(DateTime time) {
		string output = time.ToString(Iso1601Format);
		return output.Replace(":", "");
	}
	public static string Iso1601Format = "yyyyMMddTHHmmsszzz";

	public static JsonMoves.ActivityGroup ReturnGroup(ActivityType type) {
		switch (type) {
			case ActivityType.walking:
				return JsonMoves.ActivityGroup.walking;
			case ActivityType.running:
				return JsonMoves.ActivityGroup.running;
			case ActivityType.cycling:
				return JsonMoves.ActivityGroup.cycling;
			default:
				return JsonMoves.ActivityGroup.transport;
		}
	}
}

