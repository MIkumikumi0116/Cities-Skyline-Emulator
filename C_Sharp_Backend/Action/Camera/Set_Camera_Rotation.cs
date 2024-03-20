using System.Collections.Generic;
using UnityEngine;



namespace Emulator_Backend{

    public class Set_Camera_Rotation: Action_Base{
        private CameraController controller = null;

        public Set_Camera_Rotation() {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action",    "string"},
                {"rot_pitch", "float"},
                {"rot_yaw",   "float"}
            };
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_dict){
            if (!this.Check_parameter_validity(action_dict, out string parameter_validity_message)){
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            float rot_pitch = (float)action_dict["rot_pitch"];
            float rot_yaw   = (float)action_dict["rot_yaw"];

            this.Set_camera_rotation_perform(rot_pitch, rot_yaw);

            return new Dictionary<string, object> {
                {"status",  "ok"},
                {"message", "success"}
            };
        }

        private void Set_camera_rotation_perform(float rot_pitch, float rot_yaw){
            if (this.controller == null){
                this.controller = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
            }

            var new_rot = new Vector2(rot_pitch, rot_yaw);
            this.controller.m_targetAngle = new_rot;
        }
    }

}
