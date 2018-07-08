using System;
using System.Collections.Generic;
using System.IO;

class MainClass {

	public static float weight = 83;

	[STAThread]
	public static void Main() {
		if (PlacesManager.Loaded)
			Console.WriteLine("Places initialised");
		SetupWeight();

		foreach (var item in ReturnFilePath()) {
			XmlReader xr = new XmlReader(item);

			// Split into days
			List<XmlReader> daysInXml = XmlReader.Split(xr);
			foreach (var item2 in daysInXml) {
				Console.WriteLine("PROGRAM There are segments in this xml: " + item2.timelineItems.Count);
			}
			JsonParser.Parse(daysInXml, "test.json");
		}

		// On finish
		PlacesManager.SavePlaces();
	}

	static void SetupWeight() {
		Console.Write("Input your weight (in kg): ");
		weight = Convert.ToInt32(Console.ReadLine());
	}

	static string[] ReturnFilePath() {
		string appDirectory = Directory.GetCurrentDirectory();
		string[] files = Directory.GetFiles(appDirectory, "*.gpx");
		return files;
	}

}
