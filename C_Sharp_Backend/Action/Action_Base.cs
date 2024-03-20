using System.Collections.Generic;



namespace Emulator_Backend{

    public abstract class Action_Base{
        public Dictionary<string, string> parameter_type_dict;

        public abstract Dictionary<string, object> Perform_action(Dictionary<string, object> action_dict);

        public bool Check_parameter_validity(Dictionary<string, object> action_dict, out string parameter_validity_message){
            parameter_validity_message = "";

            foreach (var item in parameter_type_dict){
                var parameter_name = item.Key;
                var parameter_type = item.Value;

                if (!action_dict.ContainsKey(parameter_name)){
                    parameter_validity_message += "missing parameter: " + parameter_name + "\n";
                    continue;
                }

                switch (parameter_type){
                    case "int":
                        if (!(action_dict[parameter_name] is int))
                            parameter_validity_message += "parameter type mismatch: " + parameter_name + " is not an int\n";
                        break;
                    case "float":
                        if (!(action_dict[parameter_name] is float || action_dict[parameter_name] is int))
                            parameter_validity_message += "parameter type mismatch: " + parameter_name + " is not a float\n";
                        break;
                    case "uint":
                        if (!(action_dict[parameter_name] is int value && value >= 0))
                            parameter_validity_message += "parameter type mismatch: " + parameter_name + " is not an uint\n";
                        break;
                    case "bool":
                        if (!(action_dict[parameter_name] is bool))
                            parameter_validity_message += "parameter type mismatch: " + parameter_name + " is not a bool\n";
                        break;
                }
            }

            return string.IsNullOrEmpty(parameter_validity_message);
        }
    }

}
