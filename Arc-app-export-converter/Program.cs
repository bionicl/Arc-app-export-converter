using System;

class MainClass {

	public static float weight = 83;

	[STAThread]
	public static void Main() {
		Console.Write("Input your weight (in kg): ");
		weight = Convert.ToInt32(Console.ReadLine());
		XmlReader xr = new XmlReader();
		xr.LoadFile();
		xr.ParseToJson();
	}
}
