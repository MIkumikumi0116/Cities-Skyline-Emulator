using System;
using System.Collections.Generic;
using ColossalFramework;



namespace Emulator_Backend{

    public class Set_Pausing: Action_Base{
        private SimulationManager simulation_manager = null;

        public Set_Pausing() {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action",  "string"},
                {"pausing", "bool"}
            };
        }

        public override void On_enable(){
            this.simulation_manager = Singleton<SimulationManager>.instance;
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_param_dict){
            if (!this.Check_parameter_validity(action_param_dict, out string parameter_validity_message)){
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            bool pausing = Convert.ToBoolean(action_param_dict["pausing"]);

            this.Set_pausing_perform(pausing);

            return new Dictionary<string, object> {
                {"status",  "ok"},
                {"message", "success"}
            };
        }

        private void Set_pausing_perform(bool pausing){
            this.simulation_manager.SimulationPaused = pausing;
        }
    }

}
