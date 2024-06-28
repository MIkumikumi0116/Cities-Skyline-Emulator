
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


@register_skill("Get_Population_Info")
def Get_Population_Info() -> tuple[str, dict]:
    """
    获取人口信息

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - population_info: dict, information about population
    """
    data = {
        "action": "Get_Population_Info"
    }
    return game_api.send(data)


@register_skill("Get_Education_Info")
def Get_Education_Info() -> tuple[str, dict]:
    """
    获取教育信息

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - education_info: dict, information about education
    """
    data = {
        "action": "Get_Education_Info"
    }
    return game_api.send(data)


@register_skill("Get_Happiness_Info")
def Get_Happiness_Info() -> tuple[str, dict]:
    """
    获取幸福度信息

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - happiness_info: dict, information about happiness levels
    """
    data = {
        "action": "Get_Happiness_Info"
    }
    return game_api.send(data)


@register_skill("Get_Economy_Info")
def Get_Economy_Info(action_type: str) -> tuple[str, dict]:
    """
    获取经济信息

    Parameters:
    - action_type: string, "overview", "income", "expense", "budget"

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - economy_info: dict, information about the economy
    """
    data = {
        "action": "Get_Economy_Info",
        "type": action_type
    }
    return game_api.send(data)


@register_skill("Get_UnConnected_Road")
def Get_UnConnected_Road() -> tuple[str, list]:
    """
    获取未连接的道路信息

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - unconnected_roads: list, list of unconnected roads
    """
    data = {
        "action": "Get_UnConnected_Road"
    }
    return game_api.send(data)


@register_skill("Get_TrafficDensity_Info")
def Get_TrafficDensity_Info() -> tuple[str, dict]:
    """
    获取所有道路的流量信息

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - traffic_density_info: dict, information about traffic density
    """
    data = {
        "action": "Get_TrafficDensity_Info"
    }
    return game_api.send(data)


@register_skill("Get_RoadNode_Info")
def Get_RoadNode_Info(node_id: int) -> tuple[str, dict]:
    """
    获取道路节点的信息，包括起点和终点的坐标

    Parameters:
    - node_id: int, ID of the road node

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - road_info: dict, information about the road nodes
    """
    data = {
        "action": "Get_RoadNode_Info",
        "node_id": node_id
    }
    return game_api.send(data)


@register_skill("Get_PublicTransport_Info")
def Get_PublicTransport_Info() -> tuple[str, dict]:
    """
    获取公共交通信息

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - public_transport_info: dict, information about public transport
    """
    data = {
        "action": "Get_PublicTransport_Info"
    }
    return game_api.send(data)


@register_skill("Get_Pollution_Info")
def Get_Pollution_Info() -> tuple[str, dict]:
    """
    获取污染信息

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - pollution_info: dict, information about pollution levels
    """
    data = {
        "action": "Get_Pollution_Info"
    }
    return game_api.send(data)


@register_skill("Get_Sewage_Info")
def Get_Sewage_Info() -> tuple[str, dict]:
    """
    获取污水处理信息

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - sewage_info: dict, information about sewage processing
    """
    data = {
        "action": "Get_Sewage_Info"
    }
    return game_api.send(data)


@register_skill("Get_Garbage_Info")
def Get_Garbage_Info() -> tuple[str, dict]:
    """
    获取垃圾处理信息

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - garbage_info: dict, information about garbage processing
    """
    data = {
        "action": "Get_Garbage_Info"
    }
    return game_api.send(data)


@register_skill("Get_Zone_Info")
def Get_Zone_Info() -> tuple[str, dict]:
    """
    获取区域的信息，包括商业区、工业区和住宅区的坐标与建筑类型

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - zone_info: dict, information about different zones
    """
    data = {
        "action": "Get_Zone_Info"
    }
    return game_api.send(data)


@register_skill("Get_Electricity_Notification")
def Get_Electricity_Notification() -> tuple[str, dict]:
    """
    获取缺电建筑的坐标

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - coordinates: dict, dictionary with building id as key and coordinates as value
    """
    data = {
        "action": "Get_Electricity_Notification"
    }
    return game_api.send(data)


@register_skill("Get_Water_Notification")
def Get_Water_Notification() -> tuple[str, dict]:
    """
    获取缺水建筑的坐标

    Return:
    - status: string, "ok" or "error"
    - message: string, succeed: "success"; failed: error message
    - coordinates: dict, dictionary with building id as key and coordinates as value
    """
    data = {
        "action": "Get_Water_Notification"
    }
    return game_api.send(data)

