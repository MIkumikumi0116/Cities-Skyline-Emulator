using System;
using System.Collections;
using System.Collections.Generic;
using C_Sharp_Backend.Action.Zone;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;



namespace Emulator_Backend{

    class Select_Zone: Action_Base{
        private ZoneManager       zone_manager       = null;
        private CameraController  camera_controller  = null;
        private SimulationManager simulation_manager = null;
        private OZoneTool oZoneTool = null;

        private float start_pos_x;
        private float start_pos_z;
        private float end_pos_x;
        private float end_pos_z;
        private int   zone_type;

        public Select_Zone(){
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action",      "string"},
                {"start_pos_x", "float"},
                {"start_pos_z", "float"},
                {"end_pos_x",   "float"},
                {"end_pos_z",   "float"},
                {"zone_type",   "int"}
            };
        }

        public override void On_enable(){
            this.zone_manager       = Singleton<ZoneManager>.instance;
            this.camera_controller  = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
            this.simulation_manager = Singleton<SimulationManager>.instance;
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_param_dict){
            if (!this.Check_parameter_validity(action_param_dict, out string parameter_validity_message)){
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            this.start_pos_x = Convert.ToSingle(action_param_dict["start_pos_x"]);
            this.start_pos_z = Convert.ToSingle(action_param_dict["start_pos_z"]);
            this.end_pos_x   = Convert.ToSingle(action_param_dict["end_pos_x"]);
            this.end_pos_z   = Convert.ToSingle(action_param_dict["end_pos_z"]);
            this.zone_type   = Convert.ToInt32(action_param_dict["zone_type"]);

            if (oZoneTool == null)
            {
                oZoneTool = GameObject.FindObjectOfType<OZoneTool>();
            }
            oZoneTool.m_startPosition = new Vector3(this.start_pos_x, 0, this.start_pos_z);
            oZoneTool.m_mousePosition = new Vector3(this.end_pos_x, 0, this.end_pos_z);
            oZoneTool.m_startDirection = Vector3.forward;
            oZoneTool.m_zoning = true;
            oZoneTool.m_zone = (ItemClass.Zone)this.zone_type;
            oZoneTool.ApplyZoning();
            // Singleton<SimulationManager>.instance.AddAction(Select_zone_performance_());

            // this.Select_zone_performance_(start_pos_x, start_pos_z, end_pos_x, end_pos_z, zone_type);

            return new Dictionary<string, object> {
                {"status",    "ok"},
                {"message",   "success"},
            };
        }

        private IEnumerator Select_zone_performance_(){
            this.Select_zone_performance(this.start_pos_x, this.start_pos_z, this.end_pos_x, this.end_pos_z, this.zone_type);

            yield return 0;
        }




        private void Select_zone_performance(float start_pos_x, float start_pos_z, float end_pos_x, float end_pos_z, int zone_type){
            var start_pos = new Vector2(start_pos_x, start_pos_z);
            var end_pos   = new Vector2(end_pos_x,   end_pos_z);

            // var camera_pos           = this.camera_controller.m_targetPosition;
            // var direction            = (new Vector2(camera_pos.x, camera_pos.z) - start_pos).normalized;
            // var orthogonal_direction = new Vector2(direction.y, -direction.x);

            var camera_direction = this.camera_controller.transform.forward;
            var direction = new Vector2(camera_direction.x, camera_direction.z).normalized;
            var orthogonal_direction = new Vector2(direction.y, -direction.x);

            var width         = Mathf.Round(((end_pos.x - start_pos.x) * direction.x            + (end_pos.y - start_pos.y) * direction.y)            * 0.125f) * 8f;
            var height        = Mathf.Round(((end_pos.x - start_pos.x) * orthogonal_direction.x + (end_pos.y - start_pos.y) * orthogonal_direction.y) * 0.125f) * 8f;
            var width_offset  = (!(width  >= 0f)) ? (-4f) : 4f;
            var height_offset = (!(height >= 0f)) ? (-4f) : 4f;

            var selected_area = default(Quad2);
            selected_area.a   = start_pos - direction *  width_offset          - orthogonal_direction *  height_offset;
            selected_area.b   = start_pos - direction *  width_offset          + orthogonal_direction * (height_offset + height);
            selected_area.c   = start_pos + direction * (width_offset + width) + orthogonal_direction * (height_offset + height);
            selected_area.d   = start_pos + direction * (width_offset + width) - orthogonal_direction *  height_offset;

            if (width_offset == height_offset){
                var temp = selected_area.b;
                selected_area.b = selected_area.d;
                selected_area.d = temp;
            }

            var min_pos     = selected_area.Min();
            var max_pos     = selected_area.Max();
            var min_x_index = Mathf.Max((int)((min_pos.x - 46f) / 64f + 75f), 0);
            var min_y_index = Mathf.Max((int)((min_pos.y - 46f) / 64f + 75f), 0);
            var max_x_index = Mathf.Min((int)((max_pos.x + 46f) / 64f + 75f), 149);
            var max_y_index = Mathf.Min((int)((max_pos.y + 46f) / 64f + 75f), 149);

            for (int y_index = min_y_index; y_index <= max_y_index; y_index++){
                for (int x_index = min_x_index; x_index <= max_x_index; x_index++){
                    ushort block_index = this.zone_manager.m_zoneGrid[y_index * 150 + x_index];
                    while (block_index != 0){
                        var block_pos = this.zone_manager.m_blocks.m_buffer[block_index].m_position;

                        var distance = Mathf.Max(
                            Mathf.Max(
                                min_pos.x - block_pos.x - 46f,
                                min_pos.y - block_pos.z - 46f
                            ),
                            Mathf.Max(
                                block_pos.x - max_pos.x - 46f,
                                block_pos.z - max_pos.y - 46f
                            )
                        );

                        if (distance < 0f){
                            var zone_block = this.zone_manager.m_blocks.m_buffer[block_index];
                            this.Apply_zoning(block_index, ref zone_block, selected_area, zone_type);
                        }

                        block_index = this.zone_manager.m_blocks.m_buffer[block_index].m_nextGridBlock;
                    }
                }
            }
        }

        private void Apply_zoning(ushort block_index, ref ZoneBlock zone_block, Quad2 selected_area, int zone_type_){
            var zone_type = (ItemClass.Zone)zone_type_;

            bool do_zoning_flag;
            bool do_dezoning_flag;
            if (zone_type == ItemClass.Zone.Unzoned){
                do_zoning_flag   = false;
                do_dezoning_flag = true;
            }
            else{
                do_zoning_flag   = true;
                do_dezoning_flag = false;
            }

            var block_direction            = new Vector2(Mathf.Cos(zone_block.m_angle), Mathf.Sin(zone_block.m_angle)) * 8f;
            var orthogonal_block_direction = new Vector2(block_direction.y, -block_direction.x);
            var block_pos                  = new Vector2(zone_block.m_position.x, zone_block.m_position.z);

            var row_count  = zone_block.RowCount;
            var block_area = default(Quad2);
            block_area.a   = block_pos - 4f * block_direction - 4f * orthogonal_block_direction;
            block_area.b   = block_pos + 4f * block_direction - 4f * orthogonal_block_direction;
            block_area.c   = block_pos + 4f * block_direction + (row_count - 4) * orthogonal_block_direction;
            block_area.d   = block_pos - 4f * block_direction + (row_count - 4) * orthogonal_block_direction;

            if (!block_area.Intersect(selected_area)){
                return;
            }

            bool zone_changed_flag = false;
            for (int row_index = 0; row_index < row_count; row_index++){
                var row_offset = (row_index - 3.5f) * orthogonal_block_direction;

                for (int column_index = 0; column_index < 4; column_index++){
                    var column_offset = (column_index - 3.5f) * block_direction;

                    var zone_offset = block_pos + column_offset + row_offset;
                    if (!selected_area.Intersect(zone_offset)){
                        continue;
                    }

                    if (do_zoning_flag){
                        if (
                            (
                                zone_type == ItemClass.Zone.Unzoned ||
                                zone_block.GetZone(column_index, row_index) == ItemClass.Zone.Unzoned
                            ) &&
                            zone_block.SetZone(column_index, row_index, zone_type)
                        ){
                            zone_changed_flag = true;

                            Debug.Log(column_index + " " + row_index + " " + zone_type + " " + zone_block.GetZone(column_index, row_index) + " " + zone_block.GetZone(column_index, row_index).ToString());
                        }
                    }
                    else if (do_dezoning_flag && zone_block.SetZone(column_index, row_index, ItemClass.Zone.Unzoned)){
                        zone_changed_flag = true;
                    }
                }
            }

            if (zone_changed_flag){
                zone_block.RefreshZoning(block_index);
            }
        }
    }
}