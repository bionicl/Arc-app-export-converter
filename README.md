Arc App GPX to Moves Json converter

![][image-1]

Convert GPX exported from Arc App to storyline Json format used by abandomed Moves app.

Converter generates all information used by Moves developers:
- Calculates burned calories based on movement speed and body weight
- Creates summary of all movement types

### Usage:
1. Download from release
2. Place file named „file.gpx” in the same folder ass downloaded app
3. Run, input weight - json file will be generated in the same folder

### Notes:
- Step counts and Calories idle are not present in final export
- For now you can convert only one-day gpx into one-day storyline json
- All places are marked as „user”. I hope in future updates Arc GPX will also include Foursquare id

### TODO:
- Export many files/month file into a single json
- Create place id’s with smart place grouping based on name and coordinates

[image-1]:	https://i.imgur.com/8vDVujB.png