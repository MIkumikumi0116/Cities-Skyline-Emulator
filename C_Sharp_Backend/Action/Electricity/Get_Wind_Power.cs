using System;
using System.Collections.Generic;
using UnityEngine;



namespace Emulator_Backend
{

    public class Get_Wind_Power : Action_Base
    {

        public Get_Wind_Power()
        {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action", "string"},
                {"start_pos_x", "float"},
                {"start_pos_z", "float"},
                {"radius", "float"},
                {"step", "int"},
                {"ignore_weather", "bool"}
            };
        }

        public override void On_enable()
        {
            ;
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_param_dict)
        {
            if (!this.Check_parameter_validity(action_param_dict, out string parameter_validity_message))
            {
                return new Dictionary<string, object>
                {
                    {"status",  "error"},
                    {"message", parameter_validity_message}
                };
            }
            
            var start_pos_x = Convert.ToSingle(action_param_dict["start_pos_x"]);
            var start_pos_z = Convert.ToSingle(action_param_dict["start_pos_z"]);
            var radius = Convert.ToSingle(action_param_dict["radius"]);
            var step = Convert.ToInt32(action_param_dict["step"]);
            var ignore_weather = Convert.ToBoolean(action_param_dict["ignore_weather"]);

            var result = ElectricityHelper.GetWindPower(new Vector3(start_pos_x, 0, start_pos_z), radius, step, ignore_weather);

            return new Dictionary<string, object>
            {
                {"status",  "ok"},
                {"message", "success"},
                {"result",  result}
            };
        }

    }

}
