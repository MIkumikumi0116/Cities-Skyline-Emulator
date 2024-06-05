using ColossalFramework;
using ColossalFramework.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Emulator_Backend
{
    public static class CitizenHelper
    {
        private static int GetValue(int value, int total)
        {
            float num = (float)value / (float)total;
            return Mathf.Clamp(Mathf.FloorToInt(num * 100f), 0, 100);
        }

        public static string GetPopulationStatistics()
        {
            PopulationInfoViewPanel populationInfoViewPanel = UIView.library.Get<PopulationInfoViewPanel>(typeof(PopulationInfoViewPanel).Name);

            // var population = Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_populationData.m_finalCount;
            var population = populationInfoViewPanel.population;
            var workers = populationInfoViewPanel.workers;
            var workplaces = populationInfoViewPanel.workplaces;
            var unemployed = populationInfoViewPanel.unemployed;

            var child = Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_childData.m_finalCount;
            var teen = Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_teenData.m_finalCount;
            var young = Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_youngData.m_finalCount;
            var adult = Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_adultData.m_finalCount;
            var senior = Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_seniorData.m_finalCount;

            var birth = Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_birthData.m_finalCount;
            var death = Singleton<DistrictManager>.instance.m_districts.m_buffer[0].m_deathData.m_finalCount;

            Dictionary<object, object> populationData = new Dictionary<object, object>
            {
                { "population", population },
                { "workers", workers },
                { "workplaces", workplaces },
                { "unemployed", unemployed },
                { "child", child },
                { "teen", teen },
                { "young", young },
                { "adult", adult },
                { "senior", senior },
                { "birth", birth },
                { "death", death }
            };

            var json = Util.ConvertToJSON<object>(populationData);
            return json;
        }
    
        public static string GetEducationStatistics()
        {
            EducationInfoViewPanel educationInfoViewPanel = UIView.library.Get<EducationInfoViewPanel>(typeof(EducationInfoViewPanel).Name);

            var uneducatedLegendLbl = educationInfoViewPanel.Find<UILabel>("UneducatedAmount");
            var educatedLegendLbl = educationInfoViewPanel.Find<UILabel>("EducatedAmount");
            var wellEducatedLegendLbl = educationInfoViewPanel.Find<UILabel>("WellEducatedAmount");
            var highlyEducatedLegendLbl = educationInfoViewPanel.Find<UILabel>("HighlyEducatedAmount");

            Debug.Log($"uneducated: {uneducatedLegendLbl.text}");
            Debug.Log($"educated: {educatedLegendLbl.rawText}");

            var uneducated = int.Parse(uneducatedLegendLbl.text.Replace("%","").Trim());
            var educated = int.Parse(educatedLegendLbl.text.Replace("%", "").Trim());
            var wellEducated = int.Parse(wellEducatedLegendLbl.text.Replace("%", "").Trim());
            var highlyEducated = int.Parse(highlyEducatedLegendLbl.text.Replace("%", "").Trim());

            var dict = new Dictionary<object, object>
            {
                { "uneducated", uneducated },
                { "educated", educated },
                { "wellEducated", wellEducated },
                { "highlyEducated", highlyEducated }
            };

            var json = Util.ConvertToJSON<object>(dict);
            return json;
        }

        public static string GetHappinessStatistics()
        {
            HappinessInfoViewPanel happinessInfoViewPanel = UIView.library.Get<HappinessInfoViewPanel>(typeof(HappinessInfoViewPanel).Name);

            var residentialHappiness = happinessInfoViewPanel.residentialHappiness;
            var commercialHappiness = happinessInfoViewPanel.commercialHappiness;
            var industrialHappiness = happinessInfoViewPanel.industrialHappiness;
            var officeHappiness = happinessInfoViewPanel.officeHappiness;

            var dict = new Dictionary<object, object>
            {
                { "residentialHappiness", residentialHappiness },
                { "commercialHappiness", commercialHappiness },
                { "industrialHappiness", industrialHappiness },
                { "officeHappiness", officeHappiness }
            };

            var json = Util.ConvertToJSON<object>(dict);
            return json;
        }
    }
}
