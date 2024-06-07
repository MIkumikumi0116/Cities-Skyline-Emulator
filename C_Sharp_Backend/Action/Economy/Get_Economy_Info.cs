using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emulator_Backend
{
    public class Get_Economy_Info : Action_Base
    {
        public Get_Economy_Info()
        {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action", "string"},
                {"type", "string"}
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

            var result = string.Empty;

            var type = action_param_dict["type"].ToString();
            switch (type)
            {
                case "overview":
                    result = EconomyHelper.GetEconomyInfo();
                    break;
                case "income":
                    result = EconomyHelper.GetIncomeInfo();
                    break;
                case "expense":
                    result = EconomyHelper.GetExpenses();
                    break;
                case "budget":
                    result = EconomyHelper.GetBudgetInfo();
                    break;
                default:
                    return new Dictionary<string, object>
                    {
                        {"status",  "error"},
                        {"message", "Invalid type"}
                    };
            }

            return new Dictionary<string, object>
            {
                {"status",  "ok"},
                {"message", "success"},
                {"result",  result}
            };
        }
    }
}