# Api_definition

## Overview
This document outlines the request and response formats.

The frontend sends a GET request to `http://localhost:114514/`, specifying the action to be executed along with its corresponding parameters in JSON format. Every action request must include at least one parameter named "action", which specifies the action to be performed. Additional request parameters are described below.

The backend returns the response in JSON format, with images encoded in base64. Every response will contain at least two parameters: "status" and "message". The "status" parameter is either "ok" or "error", and the "message" parameter contains more detailed information. In most cases of successful execution, the "message" will be "success". Additional response parameters are described below.

### build road
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
