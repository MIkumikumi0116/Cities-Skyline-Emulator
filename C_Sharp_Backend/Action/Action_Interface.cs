using System.Collections.Generic;



namespace Emulator_Backend{

    public interface Action_Interface{
        Dictionary<string, object> Perform_action(Dictionary<string, object> action_dict);

        bool Check_parameter_validity(Dictionary<string, object> action_dict, out string parameter_validity_message);
    }

}
