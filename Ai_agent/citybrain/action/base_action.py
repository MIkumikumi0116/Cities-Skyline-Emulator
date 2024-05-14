import io
import json
import base64
import requests
from PIL import Image
import datetime


# data = {
#     'action': 'Build_Straight_Road',
#     'start_x': 0,
#     'start_z': -200,
#     'end_x': 500,
#     'end_z': 300,
#     'prefab_id': 89
# }

# data = {
#     'action': 'Create_Building',
#     'pos_x': 0,
#     'pos_z': 0,
#     'angle': 3.14,
#     'prefab_id': 1
# }

data = {
    'action': 'Get_Camera_Position'
}

# data = {
#     'action': 'Get_Camera_Rotation'
# }

# data = {
#     'action': 'Move_Camera',
#     'pos_x': 0,
#     'pos_y': 10,
#     'pos_z': 0,
#     'relative_to_camera': False
# }

# data = {
#     'action': 'Move_Camera',
#     'pos_x': 100,
#     'pos_y': 0,
#     'pos_z': 0,
#     'relative_to_camera': True
# }

# data = {
#     'action': 'Rotate_Camera',
#     'rot_pitch': 100,
#     'rot_yaw': 0
# }

# data = {
#     'action': 'Set_Camera_Position',
#     'pos_x': 100,
#     'pos_y': 4000,
#     'pos_z': 100
# }

# data = {
#     'action': 'Set_Camera_Rotation',
#     'rot_pitch': 100,
#     'rot_yaw': 0
# }

# data = {
#     'action': 'Get_Screen_Shot''
# }


class ActionProvider:
    def __init__(self)->None:pass

    def _send(self,data):
        print(data)
        json_data = json.dumps(data)
        response = requests.post('http://localhost:11451/', data=json_data,
                                 headers={'Content-Type': 'application/json'})
        response_json = json.loads(response.text.replace('\ufeff', ''))
        return response_json



    def Get_Screen_Shot(self)->str:
        data = {'action': 'Get_Screen_Shot',}
        response_json=self._send(data)
        base64_str = response_json['screen_shot_base64']
        image_data = base64.b64decode(base64_str)
        image_file = io.BytesIO(image_data)
        image = Image.open(image_file)
        outdir='./tmp/screen_shot.png'
        current_time = datetime.datetime.now()
        time_str = current_time.strftime("%Y%m%d_%H%M%S_%f")
        image_filename = f"image_{time_str}.png"
        image.save(f'D:\\tmp\\{image_filename }')
        return outdir







# def build_roads(test_case):
#     for road in test_case:
#         data = {
#             'action': 'Build_Straight_Road',
#             'start_x': road[0][0],
#             'start_z': road[0][1],
#             'end_x':   road[1][0],
#             'end_z':   road[1][1],
#             'prefab_id': 89
#         }
#
#         send(data)

build_roads_test_case = [
    ((0, 0), (200, 0)),
    ((200, 0), (400, 0)),
    ((400, 0), (600, 0)),

    ((100, -300), (100, 300)),

    ((-200, -200), (200, 200)),


    ((200, -300), (200, 0)),
    ((200, 0), (200, 300)),

    ((300, -300), (300, -20)),
    ((300, 20), (300, 300)),
    ((300, -20), (300, 20)),

    ((400, 20), (400, -100)),
    ((400, 20), (400, -100)), # 完全重合的两条路径

    ((400, 120), (400, -100)), # 部分重合的两条路径
    # 上面这个case有个bug，是因为segment部分重合的原因，不好修复

    ((0, 0), (600, 0)),

    ((0, 100), (600, -100)), # 左上 -> 右下
    ((0, -200), (500, 300)), # 左下 -> 右上
]

# build_roads(build_roads_test_case)