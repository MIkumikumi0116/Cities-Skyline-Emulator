using System;
using System.Collections.Generic;
using UnityEngine;



namespace Emulator_Backend
{

    public class Get_Map_Wind_Power : Action_Base
    {

        public Get_Map_Wind_Power()
        {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action", "string"},
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

            var step = Convert.ToInt32(action_param_dict["step"]);
            var ignore_weather = Convert.ToBoolean(action_param_dict["ignore_weather"]);

            var result = ElectricityHelper.GetWindPower(new Vector3(0, 0, 0), GameAreaManager.AREAGRID_CELL_SIZE, step, ignore_weather);

            return new Dictionary<string, object>
            {
                {"status",  "ok"},
                {"message", "success"},
                {"result",  result}
            };
        }
    }
}
