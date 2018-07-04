using System;

public enum ActivityType {
	walking,
	cycling,
	running,
	car,
	transport,
	train,
	bus,
	motorcycle,
	airplane,
	boat
}

public static class BurnedCalCalculator {
	public static float[] activityTypeMultiplayer = { 0.79f, 1.03f, 0.37f };

	public static int Calcualate(ActivityType type, float time, float avgSpeed, float weight) {
		if ((int)type > 2)
			return 0;
		float value = activityTypeMultiplayer[(int)type] * time * avgSpeed * weight;
		return (int)Math.Round(value);
	}

}
