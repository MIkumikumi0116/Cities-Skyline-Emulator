using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emulator_Backend
{
    public static class TrafficHelper
    {
        public static string GetTrafficStatistics()
        {
            PublicTransportInfoViewPanel publicTransportInfoViewPanel = UIView.library.Get<PublicTransportInfoViewPanel>(typeof(PublicTransportInfoViewPanel).Name);

            UILabel m_BusCitizens = publicTransportInfoViewPanel.GetType().GetField("m_BusCitizens", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_BusTourists = publicTransportInfoViewPanel.GetType().GetField("m_BusTourists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_TrolleybusCitizens = publicTransportInfoViewPanel.GetType().GetField("m_TrolleybusCitizens", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_TrolleybusTourists = publicTransportInfoViewPanel.GetType().GetField("m_TrolleybusTourists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_MetroCitizens = publicTransportInfoViewPanel.GetType().GetField("m_MetroCitizens", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_MetroTourists = publicTransportInfoViewPanel.GetType().GetField("m_MetroTourists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_TrainCitizens = publicTransportInfoViewPanel.GetType().GetField("m_TrainCitizens", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_TrainTourists = publicTransportInfoViewPanel.GetType().GetField("m_TrainTourists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_ShipCitizens = publicTransportInfoViewPanel.GetType().GetField("m_ShipCitizens", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_ShipTourists = publicTransportInfoViewPanel.GetType().GetField("m_ShipTourists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_PlaneCitizens = publicTransportInfoViewPanel.GetType().GetField("m_PlaneCitizens", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_PlaneTourists = publicTransportInfoViewPanel.GetType().GetField("m_PlaneTourists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_TaxiCitizens = publicTransportInfoViewPanel.GetType().GetField("m_TaxiCitizens", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_TaxiTourists = publicTransportInfoViewPanel.GetType().GetField("m_TaxiTourists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_TramCitizens = publicTransportInfoViewPanel.GetType().GetField("m_TramCitizens", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_TramTourists = publicTransportInfoViewPanel.GetType().GetField("m_TramTourists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_MonorailCitizens = publicTransportInfoViewPanel.GetType().GetField("m_MonorailCitizens", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_MonorailTourists = publicTransportInfoViewPanel.GetType().GetField("m_MonorailTourists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_CableCarCitizens = publicTransportInfoViewPanel.GetType().GetField("m_CableCarCitizens", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_CableCarTourists = publicTransportInfoViewPanel.GetType().GetField("m_CableCarTourists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_TotalCitizens = publicTransportInfoViewPanel.GetType().GetField("m_TotalCitizens", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;
            UILabel m_TotalTourists = publicTransportInfoViewPanel.GetType().GetField("m_TotalTourists", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(publicTransportInfoViewPanel) as UILabel;

            Dictionary<object, object> data = new Dictionary<object, object>();
            data.Add("BusCitizens", m_BusCitizens.text);
            data.Add("BusTourists", m_BusTourists.text);
            data.Add("TrolleybusCitizens", m_TrolleybusCitizens.text);
            data.Add("TrolleybusTourists", m_TrolleybusTourists.text);
            data.Add("MetroCitizens", m_MetroCitizens.text);
            data.Add("MetroTourists", m_MetroTourists.text);
            data.Add("TrainCitizens", m_TrainCitizens.text);
            data.Add("TrainTourists", m_TrainTourists.text);
            data.Add("ShipCitizens", m_ShipCitizens.text);
            data.Add("ShipTourists", m_ShipTourists.text);
            data.Add("PlaneCitizens", m_PlaneCitizens.text);
            data.Add("PlaneTourists", m_PlaneTourists.text);
            data.Add("TaxiCitizens", m_TaxiCitizens.text);
            data.Add("TaxiTourists", m_TaxiTourists.text);
            data.Add("TramCitizens", m_TramCitizens.text);
            data.Add("TramTourists", m_TramTourists.text);
            data.Add("MonorailCitizens", m_MonorailCitizens.text);
            data.Add("MonorailTourists", m_MonorailTourists.text);
            data.Add("CableCarCitizens", m_CableCarCitizens.text);
            data.Add("CableCarTourists", m_CableCarTourists.text);
            data.Add("TotalCitizens", m_TotalCitizens.text);
            data.Add("TotalTourists", m_TotalTourists.text);

            return Util.ConvertToJSON<object>(data);
        }
    }
}
