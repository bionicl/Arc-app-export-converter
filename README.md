Arc App GPX to Moves Json converter

![][image-1]

Convert GPX exported from [Arc App][1] to storyline Json format used by abandomed Moves app.

Converter generates all information used by Moves developers:
- Calculates burned calories based on movement speed and body weight
- Creates summary of all movement types

### Usage:
1. Download [newest release][2]
2. Generate GPX in Arc App and place file named „file.gpx” in the same folder with .exe
3. Run app, input weight - json file will be generated in the same folder

### Notes:
- Step counts and Calories idle are not present in final export
- For now you can convert only one-day gpx into one-day storyline json
- All places are marked as „user”. I hope in future updates Arc GPX will also include Foursquare id

### TODO:
- Export many files/month file into a single json
- Create place id’s with smart place grouping based on name and coordinates

[1]:	https://itunes.apple.com/app/arc-app-location-activity-tracker/id1063151918?mt=8
[2]:	https://github.com/bionicl/Arc-app-export-converter/releases/

[image-1]:	https://i.imgur.com/8vDVujB.png