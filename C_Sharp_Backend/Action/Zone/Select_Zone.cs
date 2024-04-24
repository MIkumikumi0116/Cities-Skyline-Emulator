using System;
using System.Collections.Generic;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;



namespace Emulator_Backend{

    class Select_Zone: Action_Base{
        private ZoneManager zone_manager = null;

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
            this.zone_manager = Singleton<ZoneManager>.instance;
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_param_dict){
            if (!this.Check_parameter_validity(action_param_dict, out string parameter_validity_message)){
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            var start_pos_x = Convert.ToSingle(action_param_dict["start_pos_x"]);
            var start_pos_z = Convert.ToSingle(action_param_dict["start_pos_z"]);
            var end_pos_x   = Convert.ToSingle(action_param_dict["end_pos_x"]);
            var end_pos_z   = Convert.ToSingle(action_param_dict["end_pos_z"]);
            var zone_type   = Convert.ToInt32(action_param_dict["zone_type"]);

            this.Select_zone_performance(start_pos_x, start_pos_z, end_pos_x, end_pos_z, zone_type);

            return new Dictionary<string, object> {
                {"status",    "ok"},
                {"message",   "success"},
            };
        }

        private void Select_zone_performance(float start_pos_x, float start_pos_z, float end_pos_x, float end_pos_z, int zone_type){
            var start_pos            = new Vector2(start_pos_x,       start_pos_z);
            var end_pos              = new Vector2(end_pos_x,         end_pos_z);
            var direction            = new Vector2(Vector3.forward.x, Vector3.forward.z);
            var orthogonal_direction = new Vector2(direction.y,       0f - direction.x);

            var width         = Mathf.Round(((end_pos.x - start_pos.x) * direction.x            + (end_pos.y - start_pos.y) * direction.y           ) * 0.125f) * 8f;
            var height        = Mathf.Round(((end_pos.x - start_pos.x) * orthogonal_direction.x + (end_pos.y - start_pos.y) * orthogonal_direction.y) * 0.125f) * 8f;
            var width_offset  = (!(width  >= 0f)) ? (-4f) : 4f;
            var height_offset = (!(height >= 0f)) ? (-4f) : 4f;

            var selected_area = default(Quad2);
            selected_area.a = start_pos - direction * width_offset           - orthogonal_direction * height_offset;
            selected_area.b = start_pos - direction * width_offset           + orthogonal_direction * (height + height_offset);
            selected_area.c = start_pos + direction * (width + width_offset) + orthogonal_direction * (height + height_offset);
            selected_area.d = start_pos + direction * (width + width_offset) - orthogonal_direction * height_offset;

            if (width_offset == height_offset){
                var temp = selected_area.b;
                selected_area.b = selected_area.d;
                selected_area.d = temp;
            }

            var min_pos = selected_area.Min();
            var max_pos = selected_area.Max();

            var min_x_index = Mathf.Max((int)((min_pos.x - 46f) / 64f + 75f), 0);
            var min_y_index = Mathf.Max((int)((min_pos.y - 46f) / 64f + 75f), 0);
            var max_x_index = Mathf.Min((int)((max_pos.x + 46f) / 64f + 75f), 149);
            var max_y_index = Mathf.Min((int)((max_pos.y + 46f) / 64f + 75f), 149);

            for (var y_index = min_y_index; y_index <= max_y_index; y_index++){
                for (var x_index = min_x_index; x_index <= max_x_index; x_index++){
                    var block_index = this.zone_manager.m_zoneGrid[y_index * 150 + x_index];

                    while (block_index != 0){
                        var position = this.zone_manager.m_blocks.m_buffer[block_index].m_position;
                        var distance = Mathf.Max(
                            Mathf.Max(min_pos.x - 46f - position.x, min_pos.y - 46f - position.z),
                            Mathf.Max(position.x - max_pos.x - 46f, position.z - max_pos.y - 46f)
                        );

                        if (distance < 0f){
                            this.ApplyZoning(block_index, ref this.zone_manager.m_blocks.m_buffer[block_index], selected_area, zone_type);
                        }

                        block_index = this.zone_manager.m_blocks.m_buffer[block_index].m_nextGridBlock;
                    }
                }
            }
        }

        public void ApplyZoning(ushort blockIndex, ref ZoneBlock data, Quad2 selected_area, int zone_type_){
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

            var block_direction            = new Vector2(Mathf.Cos(data.m_angle), Mathf.Sin(data.m_angle)) * 8f;
            var orthogonal_block_direction = new Vector2(block_direction.y, 0f - block_direction.x);
            var block_pos                  = new Vector2(data.m_position.x, data.m_position.z);

            var row_count  = data.RowCount;
            var block_area = default(Quad2);
            block_area.a = block_pos - 4f * block_direction -              4f * orthogonal_block_direction;
            block_area.b = block_pos + 4f * block_direction -              4f * orthogonal_block_direction;
            block_area.c = block_pos + 4f * block_direction + (row_count - 4) * orthogonal_block_direction;
            block_area.d = block_pos - 4f * block_direction + (row_count - 4) * orthogonal_block_direction;
            if (!block_area.Intersect(selected_area)){
                return;
            }

            bool zone_changed_flag = false;
            for (var row_index = 0; row_index < row_count; row_index++){
                var row_offset = (row_index - 3.5f) * orthogonal_block_direction;

                for (var column_index = 0; column_index < 4; column_index++){
                    var column_offset = (column_index - 3.5f) * block_direction;

                    var zone_offset = block_pos + column_offset + row_offset;
                    if (!selected_area.Intersect(zone_offset)){
                        continue;
                    }

                    if (do_zoning_flag){
                        if (
                            (
                                zone_type == ItemClass.Zone.Unzoned ||
                                data.GetZone(column_index, row_index) == ItemClass.Zone.Unzoned
                            )
                            && data.SetZone(column_index, row_index, zone_type)
                        ){
                            zone_changed_flag = true;
                        }
                    }
                    else if (do_dezoning_flag && data.SetZone(column_index, row_index, ItemClass.Zone.Unzoned)){
                        zone_changed_flag = true;
                    }
                }
            }

            if (zone_changed_flag){
                data.RefreshZoning(blockIndex);
            }
        }
    }

}