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

@register_skill("Build_Straight_Road")
def Build_Straight_Road(action, start_x, start_z, end_x, end_z, prefab_id) -> tuple[str]:
    """
    在指定的起点和终点之间构建直线道路，根据 prefab_id 指定的样式。

    参数：
    •  action: 字符串，"Build_Straight_Road"

    •  start_x: 浮点数，道路的起始 x 坐标

    •  start_z: 浮点数，道路的起始 z 坐标

    •  end_x: 浮点数，道路的终点 x 坐标

    •  end_z: 浮点数，道路的终点 z 坐标

    •  prefab_id: 整数，要建造的道路的 prefab id，请参阅道路 prefab 表以获取详细信息

    返回：
    •  status: 字符串，"ok" 或 "error"

    •  message: 字符串，成功："success"；失败：错误消息
    """

    data = {
        'action': 'Build_Straight_Road',
        'start_x': 0,
        'start_z': 0,
        'end_x': 100,
        'end_z': 100,
        'prefab_id': 89
    }

    return game_api.send(data)

@register_skill("Create_Building")
def Create_Building(action, pox_x, pox_z, angle, prefab_id) -> tuple[str]:
    """
    在指定位置和方向构建指定的建筑物。

    参数：
    •  action: 字符串，"Create_Building"

    •  pox_x: 整数，建筑物的 x 坐标

    •  pox_z: 整数，建筑物的 z 坐标

    •  angle: 浮点数，以弧度表示的建筑物方向

    •  prefab_id: 整数，要建造的建筑物的 prefab id，请参阅建筑物 prefab 表以获取详细信息

    返回：
    •  status: 字符串，"ok" 或 "error"

    •  message: 字符串，成功："success"；失败：错误消息
    """
    data = {
        'action': 'Create_Building',
        'pos_x': 0,
        'pos_z': 0,
        'angle': 3.14,
        'prefab_id': 1
    }
    return game_api.send(data)


@register_skill("Select_Zone")
def Select_Zone(action) -> tuple[str]:
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
        'action': 'Select_Zone',
        'start_pos_x': -100,
        'start_pos_z': -100,
        'end_pos_x': 100,
        'end_pos_z': 100,
        'zone_type': 3
    }

    return game_api.send(data)




@register_skill("Set_Pausing")
def Set_Pausing(action, pausing) :
    """
    Pause or resume the game.

    Parameters:
    action: string, "Set_Pausing"
    pausing: bool, true to pause the game; false to resume the game

    Returns:
    tuple[str]:
        status: string, "ok" or "error"
        message: string, "success" if succeeded; otherwise, an error message
    """

    # Create a dictionary with the pausing data
    data = {
        "action": "Set_Pausing",
        "pausing": pausing  # true to pause, false to resume
    }

    # Send the data to the game API
    game_api.send(data)


__all__ = [
    "Build_Straight_Road",
    "Create_Building",
    "Select_Zone",
    "Set_Pausing"
]