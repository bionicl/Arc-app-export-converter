using System;
using System.Collections.Generic;
using System.IO;

class MainClass {

	public static float weight = 83;

	[STAThread]
	public static void Main() {
		if (PlacesManager.Loaded) {
			Console.ForegroundColor = ConsoleColor.DarkGray;
			Console.WriteLine("Places initialised");
		}
		SetupWeight();

		foreach (var item in ReturnFilePath()) {
			XmlReader xr = new XmlReader(item);

			// Split into days
			List<XmlReader> daysInXml = XmlReader.Split(xr);
			JsonParser.Parse(daysInXml, xr.originalName + ".json");
		}

		// On finish
		PlacesManager.SavePlaces();
	}

	static void SetupWeight() {
		Console.ResetColor();
		Console.Write("Input your weight (in kg): ");
		weight = Convert.ToInt32(Console.ReadLine());
	}

	static string[] ReturnFilePath() {
		string appDirectory = Directory.GetCurrentDirectory();
		string[] files = Directory.GetFiles(appDirectory, "*.gpx");
		return files;
	}

}
