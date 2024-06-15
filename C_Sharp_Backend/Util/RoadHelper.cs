using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emulator_Backend
{
    public static class RoadHelper
    {
        public static string GetRoadNotification()
        {
            var netManager = Singleton<NetManager>.instance;
            var list = new List<object>();
            foreach (var node in netManager.m_nodes.m_buffer)
            {
                foreach (var problem in node.m_problems)
                {
                    if (problem.m_Problems1 == Notification.Problem1.RoadNotConnected)
                    {
                        list.Add(node.m_position.ToString());
                    }
                }
            }
            return Util.ConvertToJSON(list);
        }

        public static string GetRoadTrafficDensity()
        {
            var netManager = Singleton<NetManager>.instance;
            var dict = new Dictionary<object, object>();
            for (int i = 0; i < netManager.m_segments.m_buffer.Length; i++)
            {
                var segment = netManager.m_segments.m_buffer[i];
                if (segment.m_trafficDensity == 0 && segment.m_startNode == 0 && segment.m_endNode == 0)
                {
                    continue;
                }
                dict[i] = $"{segment.m_trafficDensity}+{segment.m_startNode}+{segment.m_endNode}";
            }
            return Util.ConvertToJSON<object>(dict);
        }

        public static string GetRoadNodeInfo(int id)
        {
            var netManager = Singleton<NetManager>.instance;
            var dict = new Dictionary<object, object>();
            if (id < 0 || id >= netManager.m_nodes.m_buffer.Length)
            {
                return Util.ConvertToJSON<object>(dict);
            }
            var node = netManager.m_nodes.m_buffer[id];
            dict["position"] = node.m_position.ToString();
            return Util.ConvertToJSON<object>(dict);
        }

        public static string GetZoneInfo()
        {
            var buildingManager = Singleton<BuildingManager>.instance;
            var dict = new Dictionary<object, object>();

            var targets = new List<ItemClass.Service> {
                    ItemClass.Service.Residential,
                    ItemClass.Service.Commercial,
                    ItemClass.Service.Industrial,
                    ItemClass.Service.Office,
                };
            for (int i = 0; i < buildingManager.m_buildings.m_buffer.Length; i++)
            {
                var building = buildingManager.m_buildings.m_buffer[i];
                var item = building.Info.GetService();

                if (targets.Count > 0 && !targets.Contains(item))
                {
                    continue;
                }

                var subItem = building.Info.GetSubService();
                dict[i] = $"{building.m_position}+{subItem}";
            }
            return Util.ConvertToJSON<object>(dict);
        }
    }
}
