using ColossalFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Emulator_Backend
{
    public static class ElectricityHelper
    {
        /// <summary>
        /// smaple wind power at a specific point
        /// </summary>
        /// <param name="point"></param>
        /// <param name="radius"></param>
        /// <param name="step"></param>
        /// <returns></returns>
        public static string GetWindPower(Vector3 point, float radius, int step, bool ignoreWeather)
        {
            var weatherManager = Singleton<WeatherManager>.instance;
            var terrainManager = Singleton<TerrainManager>.instance;
            var result = new Dictionary<Vector3, float>();
            var mapWidth = point.x + radius;
            var mapHeight = point.z + radius;
            var sampleStep = step;
            Debug.Log($"mapWidth: {mapWidth}, mapHeight: {mapHeight}, sampleStep: {sampleStep}");
            for (float x = point.x - radius; x <= mapWidth ; x += sampleStep)
            {
                for (float z = point.z - radius; z <= mapHeight; z += sampleStep)
                {
                    var y = terrainManager.SampleRawHeightSmooth(new Vector3(x, 0, z));
                    Vector3 position = new Vector3(x, y, z);
                    float windStrength = weatherManager.SampleWindSpeed(position, ignoreWeather);
                    result.Add(position, windStrength);
                    Debug.Log($"position: {position}, windStrength: {windStrength}");
                }
            }

            // convert to json string
            var json = "{";
            foreach (var item in result)
            {
                json += $"\"{item.Key}\": {item.Value},";
            }
            json = json.TrimEnd(',');
            json += "}";
            return json;
        }

        public static string GetElectricityNotification()
        {
            var buildingManager = Singleton<BuildingManager>.instance;
            var result = new Dictionary<object, object>();

            for (var i = 0; i < buildingManager.m_buildings.m_buffer.Length; i++)
            {
                var building = buildingManager.m_buildings.m_buffer[i];
                var problems = building.m_problems;
                foreach (var problem in problems)
                {
                    if (problem == Notification.Problem1.Electricity)
                    {
                        result.Add(i, building.m_position);
                    }
                }
            }

            return Util.ConvertToJSON<object>(result);
        }

        public static string GetWaterNotification()
        {
            var buildingManager = Singleton<BuildingManager>.instance;
            var result = new Dictionary<object, object>();

            for (var i = 0; i < buildingManager.m_buildings.m_buffer.Length; i++)
            {
                var building = buildingManager.m_buildings.m_buffer[i];
                var problems = building.m_problems;
                foreach (var problem in problems)
                {
                    if (problem == Notification.Problem1.Water)
                    {
                        result.Add(i, building.m_position);
                    }
                }
            }

            return Util.ConvertToJSON<object>(result);
        }
    }
}
