using System;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;



namespace Emulator_Backend{

    public class Create_Building: Action_Interface{

        public Create_Building() { }   

        public Dictionary<string, object> Perform_action(Dictionary<string, object> action_dict){
            if (!this.Check_parameter_validity(action_dict, out string parameter_validity_message)){
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            var pos_x     = Convert.ToSingle(action_dict["pos_x"]);
            var pos_z     = Convert.ToSingle(action_dict["pos_z"]);
            var angle     = Convert.ToSingle(action_dict["angle"]);
            var prefab_id = Convert.ToUInt32(action_dict["prefab_id"]);

            this.Create_building_perform(pos_x, pos_z, angle, prefab_id);

            return new Dictionary<string, object> {
                {"status", "ok"},
                {"message", "success"}
            };
        }

        public bool Check_parameter_validity(Dictionary<string, object> action_dict, out string parameter_validity_message){
            if (
                !action_dict.ContainsKey("pos_x") ||
                !action_dict.ContainsKey("pos_z") ||
                !action_dict.ContainsKey("angle") ||
                !action_dict.ContainsKey("prefab_id")
            ){
                parameter_validity_message = "missing parameters, Create_Building takes parameters: action(string), pos_x(float), pos_z(float), angle(float), prefab_id(int)";
                return false;
            }

            if (
                !((action_dict["pos_x"] is float || action_dict["pos_x"] is int) &&
                  (action_dict["pos_z"] is float || action_dict["pos_z"] is int) &&
                  (action_dict["angle"] is float || action_dict["angle"] is int))
            ){
                parameter_validity_message = "parameter type mismatch, Create_Building takes parameters: action(string), pos_x(float), pos_z(float), angle(float), prefab_id(int)";
                return false;
            }

            if (!(action_dict["prefab_id"] is int)){
                parameter_validity_message = "prefab_id must be an integer";
                return false;
            }

            if ((int)action_dict["prefab_id"] < 0){
                parameter_validity_message = "prefab_id must be a non-negative integer";
                return false;
            }

            parameter_validity_message = "";
            return true;
        }
    
        private void Create_building_perform(float pos_x, float pos_z, float angle, uint prefab_id){
            var height = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(new Vector3(pos_x, 0, pos_z));
            var pos = new Vector3(pos_x, height, pos_z);

            if (Singleton<BuildingManager>.instance.CreateBuilding(
                out _, ref Singleton<SimulationManager>.instance.m_randomizer,
                PrefabCollection<BuildingInfo>.GetPrefab(prefab_id),
                pos,
                angle,
                0,
                Singleton<SimulationManager>.instance.m_currentBuildIndex)
            ){
                Singleton<SimulationManager>.instance.m_currentBuildIndex++;
            }
        }
    }

}
