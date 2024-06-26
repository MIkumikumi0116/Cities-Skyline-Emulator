﻿using System;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;



namespace Emulator_Backend{

    public class Create_Building: Action_Base{
        TerrainManager    terrain_manager;
        BuildingManager   building_manager;
        SimulationManager simulation_manager;

        public Create_Building() {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action",    "string"},
                {"pos_x",     "float"},
                {"pos_z",     "float"},
                {"angle",     "float"},
                {"prefab_id", "uint"}
            };
        }

        public override void On_level_loaded(){
            this.terrain_manager    = Singleton<TerrainManager>.instance;
            this.building_manager   = Singleton<BuildingManager>.instance;
            this.simulation_manager = Singleton<SimulationManager>.instance;
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_param_dict){
            if (!this.Check_parameter_validity(action_param_dict, out string parameter_validity_message)){
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            var pos_x     = Convert.ToSingle(action_param_dict["pos_x"]);
            var pos_z     = Convert.ToSingle(action_param_dict["pos_z"]);
            var angle     = Convert.ToSingle(action_param_dict["angle"]);
            var prefab_id = Convert.ToUInt32(action_param_dict["prefab_id"]);

            this.Create_building_perform(pos_x, pos_z, angle, prefab_id);

            return new Dictionary<string, object> {
                {"status",  "ok"},
                {"message", "success"}
            };
        }

        private void Create_building_perform(float pos_x, float pos_z, float angle, uint prefab_id){
            var height = this.terrain_manager.SampleRawHeightSmooth(new Vector3(pos_x, 0, pos_z));
            var pos = new Vector3(pos_x, height, pos_z);

            this.building_manager.CreateBuilding(
                out _,
                ref this.simulation_manager.m_randomizer,
                PrefabCollection<BuildingInfo>.GetPrefab(prefab_id),
                pos,
                angle,
                0,
                this.simulation_manager.m_currentBuildIndex
            );

            this.simulation_manager.m_currentBuildIndex++;
        }
    }

}
