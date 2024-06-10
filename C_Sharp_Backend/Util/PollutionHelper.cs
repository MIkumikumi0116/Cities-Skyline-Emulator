using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emulator_Backend
{
    public class PollutionHelper
    {
        public static string GetPollution()
        {
            PollutionInfoViewPanel pollutionInfoViewPanel = UIView.library.Get<PollutionInfoViewPanel>(typeof(PollutionInfoViewPanel).Name);
            var dict = new Dictionary<object, object>();

            var ground = pollutionInfoViewPanel.groundPollution;
            var water = pollutionInfoViewPanel.waterPollution;

            dict.Add("ground", ground);
            dict.Add("water", water);

            return Util.ConvertToJSON<object>(dict);
        }

        public static string GetGarbage()
        {
            GarbageInfoViewPanel garbageInfoViewPanel = UIView.library.Get<GarbageInfoViewPanel>(typeof(GarbageInfoViewPanel).Name);
            var dict = new Dictionary<object, object>();
            if (garbageInfoViewPanel == null)
            {
                return Util.ConvertToJSON<object>(dict);
            }

            var m_LandfillUsage = garbageInfoViewPanel.GetType().GetField("m_LandfillUsage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(garbageInfoViewPanel) as UILabel;
            var m_LandfillCapacity = garbageInfoViewPanel.GetType().GetField("m_LandfillCapacity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(garbageInfoViewPanel) as UILabel;
            var m_LandfillStorage = garbageInfoViewPanel.GetType().GetField("m_LandfillStorage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(garbageInfoViewPanel) as UILabel;
            var m_IncineratorCapacity = garbageInfoViewPanel.GetType().GetField("m_IncineratorCapacity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(garbageInfoViewPanel) as UILabel;
            var m_GarbageProduction = garbageInfoViewPanel.GetType().GetField("m_GarbageProduction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(garbageInfoViewPanel) as UILabel;

            dict.Add("LandfillUsage", m_LandfillUsage.text);
            dict.Add("LandfillCapacity", m_LandfillCapacity.text);
            dict.Add("LandfillStorage", m_LandfillStorage.text);
            dict.Add("IncineratorCapacity", m_IncineratorCapacity.text);
            dict.Add("GarbageProduction", m_GarbageProduction.text);

            return Util.ConvertToJSON<object>(dict);
        }

        public static string GetSewage()
        {
            var waterInfoViewPanel = UIView.library.Get<WaterInfoViewPanel>(typeof(WaterInfoViewPanel).Name);
            var dict = new Dictionary<object, object>();
            if (waterInfoViewPanel == null)
            {
                return Util.ConvertToJSON<object>(dict);
            }

            var m_SewageCapacity = waterInfoViewPanel.GetType().GetField("m_SewageCapacity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(waterInfoViewPanel) as UILabel;
            var m_SewageAccumulation = waterInfoViewPanel.GetType().GetField("m_SewageAccumulation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(waterInfoViewPanel) as UILabel;

            dict.Add("SewageCapacity", m_SewageCapacity.text);
            dict.Add("SewageAccumulation", m_SewageAccumulation.text);

            return Util.ConvertToJSON<object>(dict);
        }
    }
}
