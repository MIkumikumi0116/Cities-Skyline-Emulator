using System;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;



namespace Emulator_Backend
{
    public class Build_Road{
        const float ROAD_PITCH = 100;
        const float temp_fixed_height = 120;

        readonly Dictionary<Vector3, ushort> position_to_node_cache_dict = new Dictionary<Vector3, ushort>();

        public Dictionary<string, object> Perform_action(Dictionary<string, object> action_dict){
            string parameter_validity_message;
            if (!this.Check_parameter_validity(action_dict, out parameter_validity_message)){
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            float start_x  = Convert.ToSingle(action_dict["start_x"]);
            float start_z  = Convert.ToSingle(action_dict["start_z"]);
            float end_x    = Convert.ToSingle(action_dict["end_x"]);
            float end_z    = Convert.ToSingle(action_dict["end_z"]);
            uint prefab_id = Convert.ToUInt32(action_dict["prefab_id"]);

            this.Make_road(start_x, start_z, end_x, end_z, prefab_id);

            return new Dictionary<string, object> {
                {"status", "ok"},
                {"message", "success"}
            };
        }

        bool Check_parameter_validity(Dictionary<string, object> action_dict, out string parameter_validity_message){
            if (
                !action_dict.ContainsKey("start_x") ||
                !action_dict.ContainsKey("start_z") ||
                !action_dict.ContainsKey("end_x")   ||
                !action_dict.ContainsKey("end_z")   ||
                !action_dict.ContainsKey("prefab_id")
            ){
                parameter_validity_message = "missing parameters, build road takes parameters: action(string), start_x(float), start_z(float), end_x(float), end_z(float), prefab_id(int)";
                return false;
            }

            if (
                !(action_dict["start_x"] is float &&
                  action_dict["start_z"] is float &&
                  action_dict["end_x"]   is float &&
                  action_dict["end_z"]   is float)
            ){
                parameter_validity_message = "parameter type mismatch, build road takes parameters: action(string), start_x(float), start_z(float), end_x(float), end_z(float), prefab_id(int)";
                return false;
            }

            if (!(action_dict["prefab_id"] is int)){
                parameter_validity_message = "prefab_id must be an integer";
                return false;
            }

            if ((int)action_dict["prefab_id"] < 0){
                parameter_validity_message = "prefab_id must be a positive integer";
                return false;
            }

            parameter_validity_message = "";
            return true;
        }

        void Make_road(float start_x, float start_z, float end_x, float end_z, uint prefab_id){
            var start_pos = new Vector3(start_x, temp_fixed_height, start_z);
            var end_pos   = new Vector3(end_x,   temp_fixed_height, end_z);
            var delta     = end_pos - start_pos;
            var direction = delta.normalized;
            var length    = delta.magnitude;

            float delta_pos = 0;
            for (; delta_pos <= length - Build_Road.ROAD_PITCH; delta_pos += Build_Road.ROAD_PITCH){
                this.Make_segment(
                    start_pos + direction * delta_pos,
                    start_pos + direction * (delta_pos + Build_Road.ROAD_PITCH),
                    prefab_id
                );
            }
            this.Make_segment(
                start_pos + direction * delta_pos,
                end_pos,
                prefab_id
            );
        }

        private Vector3 Rounding(Vector3 pos){
            pos.x = (int)(pos.x * 100) / 100.0f;
            pos.y = (int)(pos.y * 100) / 100.0f;
            pos.z = (int)(pos.z * 100) / 100.0f;
            return pos;
        }

        private ushort Get_or_make_node(Vector3 node_pos){
            node_pos = this.Rounding(node_pos);

            if (this.position_to_node_cache_dict.ContainsKey(node_pos)){
                return this.position_to_node_cache_dict[node_pos];
            }
            else{
                var net_Manager = Singleton<NetManager>.instance;
                if (net_Manager.CreateNode(
                    out ushort node_id,
                    ref SimulationManager.instance.m_randomizer,
                    PrefabCollection<NetInfo>.GetPrefab(144),
                    node_pos,
                    SimulationManager.instance.m_currentBuildIndex
                )){
                    ++SimulationManager.instance.m_currentBuildIndex;
                    this.position_to_node_cache_dict[node_pos] = node_id;
                    return node_id;
                }
                else{
                    throw new Exception("Error creating node " + node_pos.x + ", " + node_pos.y + "at" + node_pos);
                }
            }
        }

        private ushort Make_segment(Vector3 start_pos, Vector3 end_pos, uint prefab_id){
            var netManager    = Singleton<NetManager>.instance;
            var start_node_id = this.Get_or_make_node(start_pos);
            var end_node_id   = this.Get_or_make_node(end_pos);
            Vector3 direction = new Vector3(
                end_pos.x - start_pos.x,
                end_pos.y - start_pos.y,
                end_pos.z - start_pos.z
            ).normalized;

            if (netManager.CreateSegment(
                out ushort segment_id,
                ref SimulationManager.instance.m_randomizer,
                PrefabCollection<NetInfo>.GetPrefab(prefab_id),
                start_node_id,
                end_node_id,
                direction,
                -direction,
                SimulationManager.instance.m_currentBuildIndex,
                SimulationManager.instance.m_currentBuildIndex,
                false
            )){
                ++SimulationManager.instance.m_currentBuildIndex;
            }
            else{
                throw new Exception("Error creating segment");
            }

            return segment_id;
        }
    }
}