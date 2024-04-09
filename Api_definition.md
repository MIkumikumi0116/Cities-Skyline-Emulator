# Overview
This document outlines the request and response formats, mapping table of prefab IDs and corresponding names in game

The frontend sends a GET request to `http://localhost:114514/`, specifying the action to be executed along with its corresponding parameters in JSON format. Every action request must include at least one parameter named "action", which specifies the action to be performed. Additional request parameters are described below.

The backend returns the response in JSON format, with images encoded in base64. Every response will contain at least two parameters: "status" and "message". The "status" parameter is either "ok" or "error", and the "message" parameter contains more detailed information. In most cases of successful execution, the "message" will be "success". Additional response parameters are described below.

# prefab table

## Road Prefab Table
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

## Building Prefab Table
| Name | Prefab ID |
|-------|-------|
| Medical Clinic | 0 |
| Hospital | 1 |
| Fire House | 280 |
| Fire Station | 279 |
| Police Station | 444 |
| Police Headquarters | 443 |

# Api definition

## Building

### Build_Straight_Road
Construct a straight road between the specified starting point and endpoint according to the style specified by prefab_id
- request parameter
  - action: string, "Build_Straight_Road"
  - start_x: float, start x coordinate of the road
  - start_z: float, start z coordinate of the road
  - end_x: float, end x coordinate of the road
  - end_z: float, end z coordinate of the road
  - prefab_id: int, prefab id of road to be built, check Road Prefab Table for detail
- response parameter
  - status: string, "ok" or "error"
  - message: string, succeed: "success"; failed: error message

### Create_Building
Construct a specified building at the designated location and orientation.
- request parameter
  - action: string, "Create_Building"
  - pox_x: int, x coordinate of the building
  - pox_z: int, z coordinate of the building
  - angle: float, building orientation expressed in radians
  - prefab_id: int, prefab id of building to be built, check Building Prefab Table for detail
- response parameter
  - status: string, "ok" or "error"
  - message: string, succeed: "success"; failed: error message

## Camera

### Get_Camera_Position
Get the current position coordinates (x, y, z) of the camera, unit: meters
- request parameter
  - action: string, "Get_Camera_Position"
- response parameter
  - status: string, "ok" or "error"
  - message: string, succeed: "success"; failed: error message
  - pos_x: float, camera's x coordinate
  - pos_y: float, ~
  - pos_z: float, ~

### Get_Camera_Rotation
Get the camera's orientation (horizontal and pitch angles), angle in degrees
- request parameter
  - action: string, "Get_Camera_Rotation"
- response parameter
  - status: string, "ok" or "error"
  - message: string, succeed: "success"; failed: error message
  - rot_yaw: float, horizontal angle
  - rot_pitch: float, pitch angle

### Move_Camera
Specify a vector (x, y, z) to move the camera by, in world coordinates or local coordinates (determined by the camera's orientation)
- request parameter
  - action: string, "Move_Camera"
  - pos_x: float, movement vector's x coordinate
  - pos_y: float, ~
  - pos_z: float, ~
  - relative_to_camera: bool, true: move based on the camera's local coordinate system; false: move based on the world coordinate system
- response parameter
  - status: string, "ok" or "error"
  - message: string, succeed: "success"; failed: error message

### Rotate_Camera
Specify horizontal and pitch rotation angles (in degrees) to rotate the camera
- request parameter
  - action: string, "Rotate_Camera"
  - rot_yaw: float, horizontal rotation angle
  - rot_pitch: float, pitch rotation angle
- response parameter
  - status: string, "ok" or "error"
  - message: string, succeed: "success"; failed: error message

### Set_Camera_Position
Specify a coordinate (x,y,z) in world coordinates to move the camera to the specific location
- request parameter
  - action: string, "Set_Camera_Position"
  - pos_x: float, target position x coordinate
  - pos_y: float, ~
  - pos_z: float, ~
- response parameter
  - status: string, "ok" or "error"
  - message: string, succeed: "success"; failed: error message

### Set_Camera_Rotation
Specify horizontal and pitch angles (in world coordinates) to rotate the camera to the specific orientation
- request parameter
  - action: string, "Set_Camera_Rotation"
  - rot_yaw: float, horizontal angle
  - rot_pitch: float, pitch angle
- response parameter
  - status: string, "ok" or "error"
  - message: string, succeed: "success"; failed: error message
