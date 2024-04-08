using ColossalFramework.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions;



namespace Emulator_Backend
{
    public class Set_Edge_Scrolling_Option : Action_Base
    {
        private OptionsGameplayPanel panel;

        public Set_Edge_Scrolling_Option()
        {
            this.parameter_type_dict = new Dictionary<string, string>
            {
                {"action", "string"},
                {"is_enable", "bool"},
            };
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_dict)
        {
            if (!this.Check_parameter_validity(action_dict, out string parameter_validity_message))
            {
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            var is_enable = Convert.ToBoolean(action_dict["is_enable"]);

            this.Set_edge_scrolling_option_perform(is_enable);

            return new Dictionary<string, object> {
                {"status",  "ok"},
                {"message", "success"}
            };
        }

        // Be careful, this delay function has no return value.
        // Parameter `time` is in millisecond.
        public void Perform_action_delay(Dictionary<string, object> action_dict, int time)
        {
            Timer timer = new Timer(Perform_action_delay_callback, action_dict, time, Timeout.Infinite);
        }

        private void Perform_action_delay_callback(object state)
        {
            this.Perform_action((Dictionary<string, object>)(state));
        }

        private void Set_edge_scrolling_option_perform(bool is_enable)
        {
            if (this.panel == null)
            {
                var options_main_panel = UIView.library.Get<OptionsMainPanel>("OptionsPanel");
                this.panel = options_main_panel.GetComponentInChildren<OptionsGameplayPanel>();
            }
            this.panel.edgeScrolling = is_enable;
        }

    }
}
