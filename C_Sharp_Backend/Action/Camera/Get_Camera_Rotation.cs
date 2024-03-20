using System.Collections.Generic;
using UnityEngine;



namespace Emulator_Backend{

    public class Get_Camera_Rotation: Action_Base{
        private CameraController controller = null;

        public Get_Camera_Rotation() {
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

            this.Get_camera_rotation_perform(out float rot_pitch, out float rot_yaw);

            return new Dictionary<string, object> {
                {"status",    "ok"},
                {"message",   "success"},
                {"rot_pitch",  rot_pitch},
                {"rot_yaw",    rot_yaw},
            };
        }

        private void Get_camera_rotation_perform(out float rot_pitch, out float rot_yaw){
            if (this.controller == null){
                this.controller = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
            }

            var rot = this.controller.m_targetAngle;

            rot_pitch = rot.x;
            rot_yaw   = rot.y;
        }
    }

}
