using System.Collections.Generic;



namespace Emulator_Backend{

    public abstract class Action_Base{
        public Dictionary<string, string> parameter_type_dict;

        public abstract Dictionary<string, object> Perform_action(Dictionary<string, object> action_param_dict);

        public bool Check_parameter_validity(Dictionary<string, object> action_param_dict, out string parameter_validity_message){
            parameter_validity_message = "";

            foreach (var item in parameter_type_dict){
                var parameter_name = item.Key;
                var parameter_type = item.Value;

                if (!action_param_dict.ContainsKey(parameter_name)){
                    parameter_validity_message += "missing parameter: " + parameter_name + "\n";
                    continue;
                }

                switch (parameter_type){
                    case "int":
                        if (!(action_param_dict[parameter_name] is int))
                            parameter_validity_message += "parameter type mismatch: " + parameter_name + " is not an int\n";
                        break;
                    case "float":
                        if (!(action_param_dict[parameter_name] is float || action_param_dict[parameter_name] is int))
                            parameter_validity_message += "parameter type mismatch: " + parameter_name + " is not a float\n";
                        break;
                    case "uint":
                        if (!(action_param_dict[parameter_name] is int value && value >= 0))
                            parameter_validity_message += "parameter type mismatch: " + parameter_name + " is not an uint\n";
                        break;
                    case "bool":
                        if (!(action_param_dict[parameter_name] is bool))
                            parameter_validity_message += "parameter type mismatch: " + parameter_name + " is not a bool\n";
                        break;
                }
            }

            return string.IsNullOrEmpty(parameter_validity_message);
        }

        public virtual void On_enable(){}  //启动游戏

        public virtual void On_created(){} // 开始载入存档

        public virtual void On_level_loaded(){} //完成载入存档

        public virtual void On_level_unloading(){} //开始退出存档

        public virtual void On_released(){} //退出退出存档完成

        public virtual void On_disable(){} //退出游戏
    }

}
