using System;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;



namespace Emulator_Backend{

    public class Build_Straight_Road: Action_Interface{
        const float SEGMENT_PITCH = 80;

        private readonly Dictionary<Vector3, ushort> position_to_node_cache_dict = new Dictionary<Vector3, ushort>();

        public Build_Straight_Road() { }

        public Dictionary<string, object> Perform_action(Dictionary<string, object> action_dict){
            if (!this.Check_parameter_validity(action_dict, out string parameter_validity_message)){
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            var start_x   = Convert.ToSingle(action_dict["start_x"]);
            var start_z   = Convert.ToSingle(action_dict["start_z"]);
            var end_x     = Convert.ToSingle(action_dict["end_x"]);
            var end_z     = Convert.ToSingle(action_dict["end_z"]);
            var prefab_id = Convert.ToUInt32(action_dict["prefab_id"]);

            this.Build_straight_road_perform(start_x, start_z, end_x, end_z, prefab_id);

            return new Dictionary<string, object> {
                {"status", "ok"},
                {"message", "success"}
            };
        }

        public bool Check_parameter_validity(Dictionary<string, object> action_dict, out string parameter_validity_message){
            if (
                !action_dict.ContainsKey("start_x") ||
                !action_dict.ContainsKey("start_z") ||
                !action_dict.ContainsKey("end_x")   ||
                !action_dict.ContainsKey("end_z")   ||
                !action_dict.ContainsKey("prefab_id")
            ){
                parameter_validity_message = "missing parameters, Build_Straight_Road takes parameters: action(string), start_x(float), start_z(float), end_x(float), end_z(float), prefab_id(int)";
                return false;
            }

            if (
                !((action_dict["start_x"] is float || action_dict["start_x"] is int) &&
                  (action_dict["start_z"] is float || action_dict["start_z"] is int) &&
                  (action_dict["end_x"]   is float || action_dict["end_x"]   is int) &&
                  (action_dict["end_z"]   is float || action_dict["end_z"]   is int))
            ){
                parameter_validity_message = "parameter type mismatch, Build_Straight_Road takes parameters: action(string), start_x(float), start_z(float), end_x(float), end_z(float), prefab_id(int)";
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

        private void Build_straight_road_perform(float start_x, float start_z, float end_x, float end_z, uint prefab_id){
            var start_pos = new Vector3(start_x, 0, start_z);
            var end_pos   = new Vector3(end_x,   0, end_z);
            var delta     = end_pos - start_pos;
            var direction = delta.normalized;
            var length    = delta.magnitude;

            float delta_pos = 0;
            for (; delta_pos <= length - Build_Straight_Road.SEGMENT_PITCH; delta_pos += Build_Straight_Road.SEGMENT_PITCH){
                this.Make_segment(
                    start_pos + direction * delta_pos,
                    start_pos + direction * (delta_pos + Build_Straight_Road.SEGMENT_PITCH),
                    prefab_id
                );
            }
            this.Make_segment(
                start_pos + direction * delta_pos,
                end_pos,
                prefab_id
            );
        }

        private ushort Make_segment(Vector3 start_pos, Vector3 end_pos, uint prefab_id){
            var start_node_id = this.Get_or_make_node(start_pos, out Vector3 output_start_pos, prefab_id);
            var end_node_id   = this.Get_or_make_node(end_pos,   out Vector3 output_end_pos,   prefab_id);
            Vector3 direction = (output_end_pos - output_start_pos).normalized;

            if (
                Singleton<NetManager>.instance.CreateSegment(
                    out ushort segment_id,
                    ref Singleton<SimulationManager>.instance.m_randomizer,
                    PrefabCollection<NetInfo>.GetPrefab(prefab_id),
                    start_node_id,
                    end_node_id,
                    direction,
                    -direction,
                    SimulationManager.instance.m_currentBuildIndex,
                    SimulationManager.instance.m_currentBuildIndex,
                    false
                )
            ){
                ++SimulationManager.instance.m_currentBuildIndex;
            }
            else{
                throw new Exception("Error creating segment");
            }

            return segment_id;
        }

        private ushort Get_or_make_node(Vector3 input_node_pos, out Vector3 output_node_pos, uint prefab_id){
            input_node_pos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(new Vector3(input_node_pos.x, 0, input_node_pos.z));

            input_node_pos.x = (int)(input_node_pos.x * 100) / 100.0f;
            input_node_pos.y = (int)(input_node_pos.y * 100) / 100.0f;
            input_node_pos.z = (int)(input_node_pos.z * 100) / 100.0f;

            output_node_pos = input_node_pos;

            if (this.position_to_node_cache_dict.ContainsKey(input_node_pos)){
                return this.position_to_node_cache_dict[input_node_pos];
            }
            else{
                if (
                    Singleton<NetManager>.instance.CreateNode(
                        out ushort node_id,
                        ref Singleton<SimulationManager>.instance.m_randomizer,
                        PrefabCollection<NetInfo>.GetPrefab(prefab_id),
                        input_node_pos,
                        SimulationManager.instance.m_currentBuildIndex
                    )
                ){
                    ++SimulationManager.instance.m_currentBuildIndex;
                    this.position_to_node_cache_dict[input_node_pos] = node_id;
                    return node_id;
                }
                else{
                    throw new Exception("Error creating node " + input_node_pos.x + ", " + input_node_pos.y + "at" + input_node_pos);
                }
            }
        }
    }

}