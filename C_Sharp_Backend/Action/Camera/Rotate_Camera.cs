﻿using System;
using System.Collections.Generic;
using UnityEngine;



namespace Emulator_Backend{

    public class Rotate_Camera: Action_Base{
        private CameraController camera_controller = null;

        public Rotate_Camera() {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action",    "string"},
                {"rot_pitch", "float"},
                {"rot_yaw",   "float"},
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

            float rot_pitch = Convert.ToSingle(action_param_dict["rot_pitch"]);
            float rot_yaw   = Convert.ToSingle(action_param_dict["rot_yaw"]);

            this.Rotate_camera_perform(rot_pitch, rot_yaw);

            return new Dictionary<string, object> {
                {"status",  "ok"},
                {"message", "success"}
            };
        }

        private void Rotate_camera_perform(float rot_pitch, float rot_yaw){
            var current_rot = this.camera_controller.m_targetAngle;
            var delta_rot   = new Vector2(rot_pitch, rot_yaw);
            var new_rot     = current_rot + delta_rot;

            this.camera_controller.m_targetAngle = new_rot;
        }
    }

}
