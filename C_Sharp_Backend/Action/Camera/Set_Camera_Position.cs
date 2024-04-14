using System;
using System.Collections.Generic;
using UnityEngine;



namespace Emulator_Backend{

    public class Set_Camera_Position: Action_Base{
        private CameraController camera_controller = null;

        public Set_Camera_Position() {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action", "string"},
                {"pos_x",  "float"},
                {"pos_y",  "float"},
                {"pos_z",  "float"},
            };
        }

        public override void On_enable(){
            this.camera_controller = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_param_dict){
            if (!this.Check_parameter_validity(action_param_dict, out string parameter_validity_message)){
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            float pos_x = Convert.ToSingle(action_param_dict["pos_x"]);
            float pos_y = Convert.ToSingle(action_param_dict["pos_y"]);
            float pos_z = Convert.ToSingle(action_param_dict["pos_z"]);

            this.Set_camera_position_perform(pos_x, pos_y, pos_z);

            return new Dictionary<string, object> {
                {"status",  "ok"},
                {"message", "success"}
            };
        }

        private void Set_camera_position_perform(float pos_x, float pos_y, float pos_z){
            var new_pos = new Vector3(pos_x, pos_y, pos_z);
            this.camera_controller.m_targetPosition = new_pos;
            this.camera_controller.m_targetSize     = pos_y;
        }
    }

}
