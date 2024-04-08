namespace Emulator_Backend {
    public static class Util {
        public static void Swap<T>(ref T a, ref T b) {
            T temp = a;
            a      = b;
            b      = temp;
        }
    }
}
