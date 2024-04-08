using System;
using System.Collections.Generic;
using UnityEngine;



namespace Emulator_Backend{

    public class Move_Camera: Action_Base{
        private CameraController controller = null;

        public Move_Camera() {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action", "string"},
                {"pos_x",  "float"},
                {"pos_y",  "float"},
                {"pos_z",  "float"},
                {"relative_to_camera", "bool"}
            };
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
            bool relative_to_camera = Convert.ToBoolean(action_param_dict["relative_to_camera"]);

            this.Move_camera_perform(pos_x, pos_y, pos_z, relative_to_camera);

            return new Dictionary<string, object> {
                {"status",  "ok"},
                {"message", "success"}
            };
        }

        private void Move_camera_perform(float pos_x, float pos_y, float pos_z, bool relative_to_camera){
            if (this.controller == null){
                this.controller = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraController>();
            }

            var current_pos = this.controller.m_targetPosition;
            var delta_pos   = new Vector3(pos_x, pos_y, pos_z);

            Vector3 new_pos;
            if (relative_to_camera){
                var rotation  = this.controller.m_targetAngle;
                var direction = Quaternion.Euler(rotation.x, rotation.y, 0);
                new_pos       = current_pos + direction * delta_pos;
            }
            else {
                new_pos = current_pos + delta_pos;
            }

            this.controller.m_targetPosition = new_pos;
            this.controller.m_targetSize = new_pos.y;
        }
    }

}
