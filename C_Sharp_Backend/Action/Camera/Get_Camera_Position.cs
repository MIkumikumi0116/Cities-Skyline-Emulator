﻿using System.Collections.Generic;
using UnityEngine;



namespace Emulator_Backend{

    public class Get_Camera_Position: Action_Base{
        private CameraController controller = null;

        public Get_Camera_Position() {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action", "string"}
            };
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_dict){
            if (!this.Check_parameter_validity(action_dict, out string parameter_validity_message)){
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            this.Get_camera_position_perform(out float pos_x, out float pos_y, out float pos_z);

            return new Dictionary<string, object> {
                {"status",  "ok"},
                {"message", "success"},
                {"pos_x",    pos_x},
                {"pos_y",    pos_y},
                {"pos_z",    pos_z}
            };
        }

        private void Get_camera_position_perform(out float pos_x, out float pos_y, out float pos_z){
            if (this.controller == null){
                this.controller = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
            }

            var pos = this.controller.m_targetPosition;

            pos_x = pos.x;
            pos_y = pos.y;
            pos_z = pos.z;
        }
    }

}
