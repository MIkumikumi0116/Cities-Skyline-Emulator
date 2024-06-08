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
                foreach(var problem in node.m_problems)
                {
                    if (problem.m_Problems1 == Notification.Problem1.RoadNotConnected)
                    {
                        list.Add(node.m_position.ToString());
                    }
                }
            }
            return Util.ConvertToJSON(list);
        }
    }
}
