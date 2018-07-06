using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

public class Place {
	public class PlaceVisit {
		public DateTime startTime;
		public DateTime endTime;

		public PlaceVisit(DateTime startTime, DateTime endTime) {
			this.startTime = startTime;
			this.endTime = endTime;
		}
	}
	public int id;
	public string name;
	public JsonMoves.Day.Segment.Place.Location location;
	public List<PlaceVisit> visits = new List<PlaceVisit>();

	public Place(int id, string name, JsonMoves.Day.Segment.Place.Location location) {
		this.id = id;
		this.name = name;
		this.location = location;
	}

	public void AddVisit(DateTime startTime, DateTime endTime) {
		visits.Add(new PlaceVisit(startTime, endTime));
	}
}

public class PlacesSave {
	public List<Place> places = new List<Place>();
	public int currentId;

	public PlacesSave(List<Place> places, int currentId) {
		this.places = places;
		this.currentId = currentId;
	}
}

public class PlacesManager {
	static bool loaded;
	static string placesFileName = "places.json";

	static List<Place> places = new List<Place>();
	static int currentId;

	public static bool Loaded {
		get {
			if (!loaded) {
				LoadPlaces();
				loaded = true;
			}
			return true;
		}
	}

	public static int ReturnPlaceId(JsonMoves.Day.Segment.Place place, DateTime startTime, DateTime endTime) {
		foreach (var item in places) {
			if (place.name == item.name) {
				item.AddVisit(startTime, endTime);
				return item.id;
			}
		}
		Place newPlace = new Place(currentId++, place.name, place.location);
		places.Add(newPlace);
		newPlace.AddVisit(startTime, endTime);
		return newPlace.id;
	}

	// Loading files
	static void LoadPlaces() {
		if (File.Exists(placesFileName)) {
			StreamReader sr = new StreamReader(placesFileName);
			string line = sr.ReadLine();
			sr.Close();
			PlacesSave save = JsonConvert.DeserializeObject<PlacesSave>(line);
			places = save.places;
			currentId = save.currentId;
		}
	}
	public static void SavePlaces() {
		StreamWriter sw = new StreamWriter(placesFileName);
		sw.Write(JsonConvert.SerializeObject(new PlacesSave(places, currentId)));
		sw.Close();
		Console.WriteLine("Places database saved to file " + placesFileName);
	}

}
