using System;
using System.Threading;
using System.Collections.Generic;
using ColossalFramework.UI;



namespace Emulator_Backend{

    public class Set_Edge_Scrolling_Option: Action_Base{
        const int DELAY_TIME = 25000; //milliseconds

        private OptionsGameplayPanel options_gameplay_panel = null;

        public Set_Edge_Scrolling_Option(){
            this.parameter_type_dict = new Dictionary<string, string>(){
                {"action", "string"},
                {"is_enable", "bool"},
            };
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_param_dict){
            return new Dictionary<string, object> {
                {"status", "error"},
                {"message", "Set_Edge_Scrolling_Option should not be called directly"}
            };
        }

        public override void On_level_loaded(){
            this.options_gameplay_panel = UIView.library.Get<OptionsMainPanel>("OptionsPanel").GetComponentInChildren<OptionsGameplayPanel>();

            Timer timer = new Timer(
                this.On_level_loaded_callback,
                false,
                Set_Edge_Scrolling_Option.DELAY_TIME,
                Timeout.Infinite
            );
        }

        private void On_level_loaded_callback(object is_enable){
            var is_enable_flag = Convert.ToBoolean(is_enable);

            this.options_gameplay_panel.edgeScrolling = is_enable_flag;
        }
    }

}
