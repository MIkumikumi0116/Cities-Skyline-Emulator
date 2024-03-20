import json
import requests



data = {
    "action": "Build_Straight_Road",
    "start_x": 600,
    "start_z": 100,
    "end_x": 0,
    "end_z": 0,
    "prefab_id": 89
}

data = {
    "action": "Create_Building",
    "pos_x": 0,
    "pos_z": 0,
    "angle": 3.14,
    "prefab_id": 1
}

data = {
    "action": "Get_Camera_Position"
}

data = {
    "action": "Get_Camera_Rotation"
}

data = {
    "action": "Move_Camera",
    "pos_x": 100,
    "pos_y": 0,
    "pos_z": 0,
    "relative_to_camera": False
}

data = {
    "action": "Move_Camera",
    "pos_x": 100,
    "pos_y": 0,
    "pos_z": 0,
    "relative_to_camera": True
}

data = {
    "action": "Rotate_Camera",
    "rot_pitch": 100,
    "rot_yaw": 0
}

data = {
    "action": "Set_Camera_Position",
    "pos_x": 100,
    "pos_y": 100,
    "pos_z": 100
}

data = {
    "action": "Set_Camera_Rotation",
    "rot_pitch": 100,
    "rot_yaw": 0
}



json_data = json.dumps(data)
response = requests.post("http://localhost:11451/", data = json_data, headers = {"Content-Type": "application/json"})
response_json = json.loads(response.text.replace('\ufeff', ''))

print(response_json)
