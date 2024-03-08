# Overview
This document outlines the request and response formats, and records a mapping table of prefab IDs and their corresponding names in the game.

The frontend sends a GET request to `http://localhost:114514/`, specifying the action to be executed along with its corresponding parameters in JSON format. Every action request must include at least one parameter named "action", which specifies the action to be performed. Additional request parameters are described below.

The backend returns the response in JSON format, with images encoded in base64. Every response will contain at least two parameters: "status" and "message". The "status" parameter is either "ok" or "error", and the "message" parameter contains more detailed information. In most cases of successful execution, the "message" will be "success". Additional response parameters are described below.

# Prefab table
| Name | Prefab ID |
|-------|-------|
| Gravel Road | 88 |
| Gravel Road Elevated | 128 |
| Basic Road | 86 |
| Basic Road Decoration Grass | 101 |
| Basic Road Decoration Trees | 91 |
| Basic Road Elevated | 94 |
| Basic Road Slope | 114 |
| Basic Road Tunnel | 113|
| Small Road with Median | 196 |
| Small Road with Tree Median | 201 |
| Small Road with Grass Median | 195 |
| Small Road with Wide Sidewalks | 164 |
| Small Road with Median Elevated | 198 |
| Small Road with Median Slope | 199 |
| Small Road with Median Tunnel | 200 |
| Small 4 Lane Road | 140 |
| Small 4 Lane Road Elevated | 142 |
| Small 4 Lane Road Slope | 143 |
| Small 4 Lane Road Tunnel | 144 |
| Small 4 Lane Road with Bus Lanes | 145 |
| Small 4 Lane Road with Bus Lanes Elevated | 147 |
| Small 4 Lane Road with Bus Lanes Slope | 148 |
| Small 4 Lane Road with Bus Lanes Tunnel | 149 |
| Medium Road | 89 |
| Medium Road Decoration Grass | 100 |
| Medium Road Decoration Trees | 99 |
| Medium Road Elevated | 96 |
| Medium Road Slope | 115 |
| Medium Road Tunnel | 116 |
| Medium Road with Wide Sidewalks | 203 |
| Medium Road with Wide Sidewalks Grass | 208 |
| Medium Road with Wide Sidewalks Trees | 209 |
| Medium Road with Wide Sidewalks Elevated | 205 |
| Medium Road with Wide Sidewalks Slope | 206 |
| Medium Road with Wide Sidewalks Tunnel | 207 |
| Large Road Elevated | 95 |
| Large Road Slope | 119 |
| Large Road Tunnel | 120 |
| Large Road | 87 |
| Large Road Decoration Trees | 105 |
| Large Road Decoration Grass | 102 |
| Large Road with Median | 183 |
| Large Road with Grass Median | 188 |
| Large Road with Tree Median | 189 |
| Large Road with Median Elevated | 185 |
| Large Road with Median Slope | 186 |
| Large Road with Median Tunnel | 187 |
| Large Road with Tree Median and Bus Lanes | 190 |
| Large Road with Tree Median and Bus Lanes Elevated | 192 |
| Large Road with Tree Median and Bus Lanes Slope | 193 |
| Large Road with Tree Median and Bus Lanes Tunnel | 194 |
| HighwayRamp | 36 |
| HighwayRampElevated | 39 |
| HighwayRamp Slope | 125 |
| HighwayRamp Tunnel | 126 |
| Highway | 32 |
| Highway Barrier | 92 |
| Highway Elevated | 33 |
| Highway Slope | 123 |
| Highway Tunnel | 124 |

# Api definition

## build road
Construct a straight road between the specified starting point and endpoint according to the style specified by prefab_id
- request parameter
  - action: string, "build_road"
  - start_x: float, start x coordinate of the road
  - start_z: float, ~
  - end_x: float, ~
  - end_z: float, ~
  - prefab_id: int, prefab id of road to be built (144 for example)
- response parameter
  - status: "ok" or "error"
  - message: succeed: "success"; failed: error message
