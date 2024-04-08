using System;
using System.Threading;
using System.Collections.Generic;
using ColossalFramework.UI;



namespace Emulator_Backend{

    public class Set_Edge_Scrolling_Option: Action_Base{
        private OptionsGameplayPanel panel = null;

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

        public override void On_enable(){
            int time = 25000; //milliseconds

            Timer timer = new Timer(this.On_enable_callback, false, time, Timeout.Infinite);
        }

        private void On_enable_callback(object is_enable){
            var is_enable_flag = Convert.ToBoolean(is_enable);

            if (this.panel == null){
                var options_main_panel = UIView.library.Get<OptionsMainPanel>("OptionsPanel");
                this.panel = options_main_panel.GetComponentInChildren<OptionsGameplayPanel>();
            }

            this.panel.edgeScrolling = is_enable_flag;
        }
    }

}
