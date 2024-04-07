using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace Emulator_Backend {

    /**
      * Refer to: 我的算法竞赛模板 https://github.com/YXHXianYu/YXHXianYu-XCPC-Template?tab=readme-ov-file#2-%E8%AE%A1%E7%AE%97%E5%87%A0%E4%BD%95
      * 参考保证正确。如果出现错误，99%是我抄错了
      *
      * 注：Point重载了GetHashCode()。但是Point的==不满足传递性(即a=b且b=c无法得到a=c)，所以重载hash会导致一些奇怪的边界问题，目前可能会发生这个情况！
      *
      */
    public class Point: IEquatable<Point> { // 这里使用IEquatable<Point>的原因是，为了让Pair<>中可以使用Point作为key
        public static readonly float EPS = 1e-2f;
        public static int sign(float a) {
            return a < -EPS ? -1 : a > EPS ? 1 : 0;
        }
        public static int cmp(float a, float b) {
            return sign(a - b);
        }

        public float x { get; set; }
        public float y { get; set; }

        public Point() {
            x = 0;
            y = 0;
        }

        public Point(float x, float y) {
            this.x = x;
            this.y = y;
        }

        public static Point operator +(Point a, Point b) {
            return new Point(a.x + b.x, a.y + b.y);
        }
        public static Point operator -(Point a, Point b) {
            return new Point(a.x - b.x, a.y - b.y);
        }
        public static Point operator *(Point a, float d) {
            return new Point(a.x * d, a.y * d);
        }
        public static Point operator *(float d, Point a) {
            return new Point(a.x * d, a.y * d);
        }
        public static Point operator /(Point a, float d) {
            return new Point(a.x / d, a.y / d);
        }
        public static bool operator ==(Point a, Point b) {
            return cmp(a.x, b.x) == 0 && cmp(a.y, b.y) == 0;
        }
        public static bool operator !=(Point a, Point b) {
            return !(a == b);
        }

        public static implicit operator Vector3(Point v) {
            throw new NotImplementedException();
        }

        public override bool Equals(object obj) {
            return obj is Point point &&
                   x == point.x &&
                   y == point.y;
        }
        public bool Equals(Point point) {
            return x == point.x &&
                   y == point.y;
        }
        public override string ToString() {
            return "(" + x + ", " + y + ")";
        }
        public override int GetHashCode() { // 继承IEquatable<Point> 要求对GetHashCode进行重载
            unchecked // 使用 unchecked 防止溢出，这在计算哈希码时是常见的
            {
                int hash = 17;
                hash = hash * 31 + ((int)(x * 100) / 100.0f).GetHashCode();
                hash = hash * 31 + ((int)(y * 100) / 100.0f).GetHashCode();
                return hash;
            }
        }

        public float dot(Point p) { // dot product
            return x * p.x + y * p.y;
        }
        public float det(Point p) { // cross product
            return x * p.y - y * p.x;
        }
        public float distTo(Point p) { // distance to another point
            return (this - p).abs();
        }
        public float abs() { // magnitude
            return (float)Math.Sqrt((float)this.abs2());
        }
        public float abs2() { // sqrMagnitude
            return x * x + y * y;
        }
        
        public static float cross(Point p1, Point p2, Point p3) { // cross product in 2D space
            return (p2.x - p1.x) * (p3.y - p1.y) - (p3.x - p1.x) * (p2.y - p1.y);
        }
        public static float crossOp(Point p1, Point p2, Point p3) { // cross product in 2D space, but ignore the magnitude
            return sign(cross(p1, p2, p3));
        }
        public static bool intersect(float l1, float r1, float l2, float r2) { // 区间相交
            if (l1 > r1) Util.swap(ref l1, ref r1); if (l2 > r2) Util.swap(ref l2, ref r2);
            return !(cmp(r1, l2) == -1 || cmp(r2, l1) == -1);
        }
        public static bool intersect(Point p1, Point p2, Point q1, Point q2) { // 线段相交
            return intersect(p1.x, p2.x, q1.x, q2.x)
                && intersect(p1.y, p2.y, q1.y, q2.y)
                && crossOp(p1, p2, q1) * crossOp(p1, p2, q2) <= 0
                && crossOp(q1, q2, p1) * crossOp(q1, q2, p2) <= 0;
        }
        public static Point getIntersection(Point p1, Point p2, Point q1, Point q2) { // 求两直线交点
            float a1 = cross(q1, q2, p1), a2 = -cross(q1, q2, p2);
            return (p1 * a2 + p2 * a1) / (a1 + a2);
        }
    }
}
