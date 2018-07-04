using System;

class MainClass {

	public static float weight = 83;

	public static void Main() {
		Console.WriteLine(BurnedCalCalculator.Calcualate(ActivityType.walking, (float)34/60, 5.3f, 83));
	}
}
