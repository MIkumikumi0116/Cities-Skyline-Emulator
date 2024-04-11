using System.Collections.Generic;
using UnityEngine;



namespace Emulator_Backend{

    public class Get_Camera_Rotation: Action_Base{
        private CameraController camera_controller = null;

        public Get_Camera_Rotation() {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action", "string"}
            };
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_param_dict){
            if (this.camera_controller == null){
                this.camera_controller = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
            }

            if (!this.Check_parameter_validity(action_param_dict, out string parameter_validity_message)){
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            this.Get_camera_rotation_perform(out float rot_pitch, out float rot_yaw);

            return new Dictionary<string, object> {
                {"status",    "ok"},
                {"message",   "success"},
                {"rot_pitch",  rot_pitch},
                {"rot_yaw",    rot_yaw},
            };
        }

        private void Get_camera_rotation_perform(out float rot_pitch, out float rot_yaw){
            var rot = this.camera_controller.m_targetAngle;

            rot_pitch = rot.x;
            rot_yaw   = rot.y;
        }
    }

}
