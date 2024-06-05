using System.Collections.Generic;

namespace Emulator_Backend {
    public static class Util {
        public static void Swap<T>(ref T a, ref T b) {
            T temp = a;
            a      = b;
            b      = temp;
        }

        public static string ConvertToJSON<T>(Dictionary<object, object> obj) {
            var json = "{";
            foreach (var item in obj)
            {
                json += $"\"{item.Key}\": {item.Value},";
            }
            json = json.TrimEnd(',');
            json += "}";
            return json;
        }
    }
}
