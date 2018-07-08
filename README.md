Arc App GPX to Moves Json converter

![][image-1]

Convert GPX exported from [Arc App][1] to storyline Json format used by abandomed Moves app.

Converter generates all information used by Moves developers:
- Calculates burned calories based on movement speed and body weight
- Creates summary of all movement types
- Sometimes GPX file can contain two different places with the same name - if those are too far from each other, converter will split it into two different places

### Usage:
1. Download [newest release][2]
2. Generate GPX in Arc App and place file in the same folder with .exe (app support both single-day and month GPX files, make sure they end with *.gpx*)
3. Run app, input weight - json file will be generated in the same folder

### Notes:
- Step counts and Calories idle are not present in final export
- All places are marked as „user”. I hope in future updates Arc GPX will also include Foursquare id

[1]:	https://itunes.apple.com/app/arc-app-location-activity-tracker/id1063151918?mt=8
[2]:	https://github.com/bionicl/Arc-app-export-converter/releases/

[image-1]:	https://i.imgur.com/8vDVujB.png