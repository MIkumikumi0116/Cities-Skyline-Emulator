using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Emulator_Backend
{
    public class Get_Population_Info : Action_Base
    {
        public Get_Population_Info()
        {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action", "string"},
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

            var result = CitizenHelper.GetPopulationStatistics();

            return new Dictionary<string, object>
            {
                {"status",  "ok"},
                {"message", "success"},
                {"result",  result}
            };
        }
    }
}
