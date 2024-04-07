using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Emulator_Backend {
    public class Util {
        public static void swap<T>(ref T a, ref T b) {
            T temp = a;
            a = b;
            b = temp;
        }
    }
}
