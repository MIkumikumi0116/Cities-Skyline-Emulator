import json
import requests



data = {
    "action": "Build_Straight_Road",
    "start_x": -600,
    "start_z": -100,
    "end_x": 300,
    "end_z": -100,
    "prefab_id": 101
}

json_data = json.dumps(data)
response = requests.post("http://localhost:11451/", data = json_data, headers = {"Content-Type": "application/json"})
response_json = json.loads(response.text.replace('\ufeff', ''))

print(response_json)
