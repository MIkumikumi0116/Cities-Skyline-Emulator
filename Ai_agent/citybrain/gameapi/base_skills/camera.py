import io
import json
import base64
import requests
from PIL import Image
from citybrain.config import Config
from citybrain.gameapi import GameApiProvider
from citybrain.gameio.skill_registry import register_skill, post_skill_wait

config = Config()
game_api = GameApiProvider()


@register_skill("Get_Camera_Position")
def Get_Camera_Position(action) -> tuple[str, float]:
    """
    获取摄像机的当前位置坐标 (x, y, z)，单位：米。

    参数：
    •  action: 字符串，"Get_Camera_Position"

    返回：
    •  status: 字符串，"ok" 或 "error"

    •  message: 字符串，成功："success"；失败：错误消息

    •  pos_x: 浮点数，摄像机的 x 坐标

    •  pos_y: 浮点数

    •  pos_z: 浮点数
    """
    data = {
        'action': 'Get_Camera_Position'
    }
    return game_api.send(data)

@register_skill("Get_Camera_Rotation")
def Get_Camera_Rotation(action) -> tuple[str, float]:
    """
    获取摄像机的方向（水平和俯仰角度），角度以度为单位。

    参数：
    •  action: 字符串，"Get_Camera_Rotation"

    返回：
    •  status: 字符串，"ok" 或 "error"

    •  message: 字符串，成功："success"；失败：错误消息

    •  rot_yaw: 浮点数，水平角度

    •  rot_pitch: 浮点数，俯仰角度
    """
    data = {
        'action': 'Get_Camera_Rotation'
    }
    return game_api.send(data)

@register_skill("Move_Camera")
def Move_Camera(action, pos_x, pos_y, pos_z, relative_to_camera) -> tuple[str]:
    """
    指定一个向量 (x, y, z) 来移动摄像机，世界坐标或局部坐标（由摄像机的方向确定）。

    参数：
    •  action: 字符串，"Move_Camera"

    •  pos_x: 浮点数，移动向量的 x 坐标

    •  pos_y: 浮点数

    •  pos_z: 浮点数

    •  relative_to_camera: 布尔值，true：基于摄像机的局部坐标系移动；false：基于世界坐标系移动

    返回：
    •  status: 字符串，"ok" 或 "error"

    •  message: 字符串，成功："success"；失败：错误消息
    """
    data = {
        'action': 'Move_Camera',
        'pos_x': 0,
        'pos_y': 10,
        'pos_z': 0,
        'relative_to_camera': False
    }
    return game_api.send(data)

@register_skill("Rotate_Camera")
def Rotate_Camera(action, rot_yaw, rot_pitch) -> tuple[str]:
    """
    指定水平和俯仰旋转角度（以度为单位）来旋转摄像机。

    参数：
    •  action: 字符串，"Rotate_Camera"

    •  rot_yaw: 浮点数，水平旋转角度

    •  rot_pitch: 浮点数，俯仰旋转角度

    返回：
    •  status: 字符串，"ok" 或 "error"

    •  message: 字符串，成功："success"；失败：错误消息
    """
    data = {
        'action': 'Rotate_Camera',
        'rot_pitch': 100,
        'rot_yaw': 0
    }
    return game_api.send(data)

@register_skill("Set_Camera_Position")
def Set_Camera_Position(action, pos_x, pos_y, pos_z) -> tuple[str]:
    """
    指定世界坐标中的一个坐标 (x, y, z) 来将摄像机移动到特定位置。

    参数：
    •  action: 字符串，"Set_Camera_Position"

    •  pos_x: 浮点数，目标位置的 x 坐标

    •  pos_y: 浮点数

    •  pos_z: 浮点数

    返回：
    •  status: 字符串，"ok" 或 "error"

    •  message: 字符串，成功："success"；失败：错误消息
    """
    data = {
        'action': 'Set_Camera_Position',
        'pos_x': 100,
        'pos_y': 4000,
        'pos_z': 100
    }
    return game_api.send(data)

@register_skill("Set_Camera_Rotation")
def Set_Camera_Rotation(action, rot_yaw, rot_pitch) -> tuple[str]:
    """
    指定水平和俯仰角度（以世界坐标）来将摄像机旋转到特定方向。

    参数：
    •  action: 字符串，"Set_Camera_Rotation"

    •  rot_yaw: 浮点数，水平角度

    •  rot_pitch: 浮点数，俯仰角度

    返回：
    •  status: 字符串，"ok" 或 "error"

    •  message: 字符串，成功："success"；失败：错误消息
    """
    data = {
        'action': 'Set_Camera_Rotation',
        'rot_pitch': 100,
        'rot_yaw': 0
    }
    return game_api.send(data)

@register_skill("Get_Screen_Shot")
def Get_Screen_Shot(action) -> tuple[str]:
    """
    捕获游戏的完整截图，包括风景和 UI。

    参数：
    •  action: 字符串，"Get_Screen_Shot"

    返回：
    •  status: 字符串，"ok" 或 "error"

    •  message: 字符串，成功："success"；失败：错误消息

    •  screen_shot_base64: 字符串，png 图像的 base64 编码
    """
    data = {
        'action': 'Get_Screen_Shot',
    }

    return game_api.send(data)


__all__ = [
    "Get_Camera_Position",
    "Get_Camera_Rotation",
    "Move_Camera",
    "Rotate_Camera",
    "Set_Camera_Position",
    "Set_Camera_Rotation",
    "Get_Screen_Shot"
]