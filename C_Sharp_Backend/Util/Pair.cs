using JetBrains.Annotations;
using System;
using UnityEngine;

namespace Emulator_Backend {

    // 自定义 Pair 类表示一对值
    public class Pair<TFirst, TSecond>
        where TFirst : IEquatable<TFirst>
        where TSecond : IEquatable<TSecond> {
        public TFirst First { get; set; }
        public TSecond Second { get; set; }

        public Pair(TFirst first, TSecond second) {
            First = first;
            Second = second;
        }

        public static bool operator ==(Pair<TFirst, TSecond> a, Pair<TFirst, TSecond> b) {
            return a.First.Equals(b.First) && a.Second.Equals(b.Second);
        }
        public static bool operator !=(Pair<TFirst, TSecond> a, Pair<TFirst, TSecond> b) {
            return !(a == b);
        }
        public override bool Equals(object obj) {
            return obj is Pair<TFirst, TSecond> pair && First.Equals(pair.First) && Second.Equals(pair.Second);
        }

        public override int GetHashCode() {
            unchecked // 使用 unchecked 防止溢出，这在计算哈希码时是常见的
            {
                int hash = 17;
                hash = hash * 31 + (First != null ? First.GetHashCode() : 0);
                hash = hash * 31 + (Second != null ? Second.GetHashCode() : 0);
                return hash;
            }
        }

        public static Pair<TFirst, TSecond> MakePair(TFirst first, TSecond second) {
            return new Pair<TFirst, TSecond>(first, second);
        }

        public static Pair<ushort, ushort> MakeSortedPair(ushort first, ushort second) {
            if (first > second) {
                Util.swap(ref first, ref second);
            }
            return new Pair<ushort, ushort>(first, second);
        }
    }
}
