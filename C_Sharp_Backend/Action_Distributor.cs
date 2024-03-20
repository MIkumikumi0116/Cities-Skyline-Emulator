using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using C_Sharp_Backend.Action.Camera;



namespace Emulator_Backend{

    public class Action_Distributor: UnityEngine.MonoBehaviour{
        private readonly Http_Server http_server   = new Http_Server();
        private readonly Json_Utility json_utility = new Json_Utility();

        private readonly Build_Straight_Road build_straight_road_action = new Build_Straight_Road();
        private readonly Create_Building     create_building_action     = new Create_Building();
        private readonly Get_Camera_Position get_camera_position_action = new Get_Camera_Position();
        private readonly Get_Camera_Rotation get_camera_rotation_action = new Get_Camera_Rotation();
        private readonly Set_Camera_Position set_camera_position_action = new Set_Camera_Position();
        private readonly Set_Camera_Rotation set_camera_rotation_action = new Set_Camera_Rotation();
        private readonly Move_Camera         move_camera_action         = new Move_Camera();
        private readonly Rotate_Camera       rotate_camera_action       = new Rotate_Camera();



        private void FixedUpdate(){
            if (!this.http_server.Try_pop_request(out HttpListenerContext http_context)){
                return;
            }

            string request_str;
            using (StreamReader stream_reader = new StreamReader(http_context.Request.InputStream, http_context.Request.ContentEncoding)){
                request_str = stream_reader.ReadToEnd();
            }

            Dictionary<string, object> request_dict  = null;
            Dictionary<string, object> response_dict = null;
            try{
                request_dict = this.json_utility.Decode_json(request_str);
            }
            catch (System.Exception){
                response_dict = new Dictionary<string, object>{
                    {"status",  "error"},
                    {"message", "Invalid JSON format"}
                };
            }

            if (response_dict == null){
                response_dict = this.Dispatch_action(request_dict);
            }

            http_context.Response.ContentType     = "application/json";
            http_context.Response.ContentEncoding = Encoding.UTF8;
            http_context.Response.StatusCode      = 200;

            using (Stream output_stream = http_context.Response.OutputStream)
            using (StreamWriter stream_writer = new StreamWriter(output_stream, http_context.Response.ContentEncoding)){
                stream_writer.Write(this.json_utility.Encode_json(response_dict));
            }

            http_context.Response.Close();
        }

        private Dictionary<string, object> Dispatch_action(Dictionary<string, object> action_dict){
            if (!action_dict.ContainsKey("action")){
                return new Dictionary<string, object>{
                    {"status", "error"},
                    {"message", "no action specified"}
                };
            }

            if (!(action_dict["action"] is string action_string)){
                return new Dictionary<string, object>{
                    {"status", "error"},
                    {"message", "action should be a string"}
                };
            }

            switch (action_string){
                case "Build_Straight_Road":
                    return this.build_straight_road_action.Perform_action(action_dict);
                case "Create_Building":
                    return this.create_building_action.Perform_action(action_dict);
                case "Get_Camera_Position":
                    return this.get_camera_position_action.Perform_action(action_dict);
                // case "Get_Camera_Rotation":
                //     return this.get_camera_rotation_action.Perform_action(action_dict);
                // case "Set_Camera_Position":
                //     return this.set_camera_position_action.Perform_action(action_dict);
                // case "Set_Camera_Rotation":
                //     return this.set_camera_rotation_action.Perform_action(action_dict);
                // case "Move_Camera":
                //     return this.move_camera_action.Perform_action(action_dict);
                // case "Rotate_Camera":
                //     return this.rotate_camera_action.Perform_action(action_dict);
                default:
                    return new Dictionary<string, object>{
                        {"status", "error"},
                        {"message", "unknown action"}
                    };
            }
        }
    }

    public class Http_Server{
        private const string HTTP_PREFIX = "http://localhost:11451/";

        private readonly HttpListener http_listener;
        private readonly Thread http_listener_thread;
        private readonly Queue<HttpListenerContext> http_context_queue = new Queue<HttpListenerContext>();
        private readonly object http_context_queue_lock = new object();

        public Http_Server(){
            this.http_listener = new HttpListener();
            this.http_listener.Prefixes.Add(HTTP_PREFIX);
            this.http_listener.Start();

            this.http_listener_thread = new Thread(this.Monitoring_request);
            this.http_listener_thread.Start();
        }

        private void Monitoring_request(){
            while (true){
                var http_context = this.http_listener.GetContext();
                lock (this.http_context_queue_lock){
                    this.http_context_queue.Enqueue(http_context);
                }
            }
        }

        public bool Try_pop_request(out HttpListenerContext http_context){
            lock (this.http_context_queue_lock){
                if (this.http_context_queue.Count == 0){
                    http_context = default;
                    return false;
                }
                else{
                    http_context = this.http_context_queue.Dequeue();
                    return true;
                }
            }
        }

    }

    public class Json_Utility{
        public Dictionary<string, object> Decode_json(string json_str){
            var json_dict = new Dictionary<string, object>();
            var regex     = new Regex("\"(.*?)\":\\s*(\".*?\"|-?\\d+\\.\\d+|-?\\d+)");
            var matches   = regex.Matches(json_str);
            foreach (Match match in matches){
                string key   = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                if (value.StartsWith("\"")){
                    json_dict[key] = value.Trim('"');    //string
                }
                else if (value.Contains(".")){
                    json_dict[key] = float.Parse(value); //float
                }
                else{
                    json_dict[key] = int.Parse(value);   //int
                }
            }

            return json_dict;
        }

        public string Encode_json(Dictionary<string, object> json_dict){
            var json_str = new StringBuilder();
            json_str.Append("{");

            foreach (var key_value in json_dict){
                string key   = key_value.Key;
                object value = key_value.Value;

                json_str.Append($"\"{key}\":");
                if (value is string value_str){
                    json_str.Append($"\"{this.Escape_string(value_str)}\"");
                }
                else{
                    json_str.Append(value);
                }
                json_str.Append(", ");
            }

            if (json_dict.Count > 0){ // remove last comma and space
                json_str.Length -= 2;
            }

            json_str.Append("}");

            return json_str.ToString();
        }

        private string Escape_string(string input_str){
            if (string.IsNullOrEmpty(input_str)){
                return input_str;
            }

            return input_str
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\b", "\\b")
                .Replace("\f", "\\f");
        }
    }

}