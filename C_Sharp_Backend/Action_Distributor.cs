using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Text.RegularExpressions;



namespace Emulator_Backend{
    public class Http_Server{
        private readonly HttpListener http_listener;
        private readonly Thread http_listener_thread;
        private readonly Queue<HttpListenerContext> http_context_queue = new Queue<HttpListenerContext>();
        private readonly object http_context_queue_lock = new object();

        public Http_Server(string prefix){
            this.http_listener = new HttpListener();
            this.http_listener.Prefixes.Add(prefix);
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

        public void Stop(){
            this.http_listener.Stop();
            this.http_listener_thread.Interrupt();
        }
    }

    public static class Json_Utility{
        public static Dictionary<string, object> Decode_json(string json_str){
            var json_dict = new Dictionary<string, object>();
            var regex     = new Regex("\"(.*?)\":\\s*(\".*?\"|-?\\d+\\.\\d+|-?\\d+)");
            var matches   = regex.Matches(json_str);
            foreach (Match match in matches){
                string key = match.Groups[1].Value;
                string value = match.Groups[2].Value;

                if (value.StartsWith("\"")){ //string
                    json_dict[key] = value.Trim('"');
                }
                else if (value.Contains(".")){ //float
                    json_dict[key] = float.Parse(value);
                }
                else{ //int
                    json_dict[key] = int.Parse(value);
                }
            }

            return json_dict;
        }

        public static string Encode_json(Dictionary<string, object> json_dict){
            var json_str = new StringBuilder();
            json_str.Append("{");

            foreach (var key_value in json_dict){
                string key = key_value.Key;
                object value = key_value.Value;

                json_str.Append($"\"{key}\":");
                if (value is string value_str){
                    json_str.Append($"\"{Escape_string(value_str)}\"");
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

        private static string Escape_string(string input_str){
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
    
    public class Action_Distributor: UnityEngine.MonoBehaviour{
        private const string HTTP_PREFIX = "http://localhost:11451/";
        private readonly Http_Server http_server = new Http_Server(Action_Distributor.HTTP_PREFIX);

        void FixedUpdate() {
            if (!this.http_server.Try_pop_request(out HttpListenerContext http_context)) {
                return;
            }

            string request_str;
            using (StreamReader stream_reader = new StreamReader(http_context.Request.InputStream, http_context.Request.ContentEncoding)) {
                request_str = stream_reader.ReadToEnd();
            }

            Dictionary<string, object> action_dict = null;
            Dictionary<string, object> response_dict = null;
            try {
                action_dict = Json_Utility.Decode_json(request_str);
            }
            catch (System.Exception) {
                response_dict = new Dictionary<string, object>{
                    {"status",  "error"},
                    {"message", "Invalid JSON format"}
                };
            }

            if (response_dict == null) {
                response_dict = this.Dispatch_action(action_dict);
            }

            http_context.Response.ContentType     = "application/json";
            http_context.Response.ContentEncoding = Encoding.UTF8;
            http_context.Response.StatusCode      = 200;

            using (Stream output_stream = http_context.Response.OutputStream)
            using (StreamWriter stream_writer = new StreamWriter(output_stream, http_context.Response.ContentEncoding)){
                stream_writer.Write(Json_Utility.Encode_json(response_dict));
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
                case "build_road":
                    return new Build_Road().Perform_action(action_dict);
                default:
                    return new Dictionary<string, object>{
                        {"status", "error"},
                        {"message", "unknown action"}
                    };
            }
        }

        public void Stop(){
            this.http_server.Stop();
        }
    }

}