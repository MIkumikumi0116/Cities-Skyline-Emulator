using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Emulator_Backend{

    public class Screen_Shot_Manager: MonoBehaviour{
        public Texture2D screen_shot_storage_texture;
        public readonly object texture_lock = new object();

        private int frame_index = 0;

        void Update () {
            this.frame_index += 1;

            if (this.frame_index == 30){
                StartCoroutine(Update_screen_shot());
                this.frame_index = 0;
            }
        }

        public IEnumerator Update_screen_shot(){
            yield return new WaitForEndOfFrame();

            Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            texture.Apply();

            lock(this.texture_lock){
                this.screen_shot_storage_texture = texture;
            }
        }
    }

    public class Get_Screen_Shot: Action_Base{
        private Screen_Shot_Manager screen_shot_manager     = null;
        private readonly GameObject screen_shot_game_object = new GameObject("Screen_Shot_Object");

        public Get_Screen_Shot() {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action", "string"}
            };
        }

        public override void On_enable(){
            this.screen_shot_game_object.AddComponent<Screen_Shot_Manager>();
            this.screen_shot_manager = this.screen_shot_game_object.GetComponent<Screen_Shot_Manager>();
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_param_dict){
            if (!this.Check_parameter_validity(action_param_dict, out string parameter_validity_message)){
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            var screen_shot_base64_str = this.Get_screen_shot_perform();

            return new Dictionary<string, object> {
                {"status",    "ok"},
                {"message",   "success"},
                {"screen_shot_base64",  screen_shot_base64_str},
            };
        }

        private string Get_screen_shot_perform(){
            Texture2D screen_shot_texture;
            lock(this.screen_shot_manager.texture_lock){
                screen_shot_texture = this.screen_shot_manager.screen_shot_storage_texture;
            }

            var screen_shot_bytes      = screen_shot_texture.EncodeToPNG();
            var screen_shot_base64_str = Convert.ToBase64String(screen_shot_bytes);

            return screen_shot_base64_str;
        }
    }

}