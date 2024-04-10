﻿using System;
using UnityEngine;



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
        public float X_pos { get; set; }
        public float Y_pos { get; set; }

        public Point() {
            this.X_pos = 0;
            this.Y_pos = 0;
        }
        public Point(float x, float y) {
            this.X_pos = x;
            this.Y_pos = y;
        }

        public static Point operator +(Point a, Point b) {
            return new Point(a.X_pos + b.X_pos, a.Y_pos + b.Y_pos);
        }
        public static Point operator -(Point a, Point b) {
            return new Point(a.X_pos - b.X_pos, a.Y_pos - b.Y_pos);
        }
        public static Point operator *(Point a, float d) {
            return new Point(a.X_pos * d, a.Y_pos * d);
        }
        public static Point operator *(float d, Point a) {
            return new Point(a.X_pos * d, a.Y_pos * d);
        }
        public static Point operator /(Point a, float d) {
            return new Point(a.X_pos / d, a.Y_pos / d);
        }
        public static bool operator ==(Point a, Point b) {
            return Point.Cmp(a.X_pos, b.X_pos) == 0 && Point.Cmp(a.Y_pos, b.Y_pos) == 0;
        }
        public static bool operator !=(Point a, Point b) {
            return !(a == b);
        }
        public static implicit operator Vector3(Point v) {
            throw new NotImplementedException();
        }

        public static int Sign(float float_value) {
            return float_value < -EPS ? -1 : float_value > EPS ? 1 : 0;
        }
        public static int Cmp(float float_value_1, float float_value_2) {
            return Point.Sign(float_value_1 - float_value_2);
        }
        public static float Cross(Point p1, Point p2, Point p3) { // cross product in 2D space
            return (p2.X_pos - p1.X_pos) * (p3.Y_pos - p1.Y_pos) - (p3.X_pos - p1.X_pos) * (p2.Y_pos - p1.Y_pos);
        }
        public static float Cross_Op(Point p1, Point p2, Point p3) { // cross product in 2D space, but ignore the magnitude
            return Point.Sign(Point.Cross(p1, p2, p3));
        }
        public static bool Intersect(float interval_1_left, float interval_1_right, float interval_2_left, float interval_2_right) { // 区间相交
            if (interval_1_left > interval_1_right){
                Util.Swap(ref interval_1_left, ref interval_1_right);
            }
            if (interval_2_left > interval_2_right){
                Util.Swap(ref interval_2_left, ref interval_2_right);
            }

            var intersect_flag = !(Point.Cmp(interval_1_right, interval_2_left) == -1 || Point.Cmp(interval_2_right, interval_1_left) == -1);

            return intersect_flag;
        }
        public static bool Intersect(Point p1, Point p2, Point q1, Point q2) { // 线段相交
            return (
                Point.Intersect(p1.X_pos, p2.X_pos, q1.X_pos, q2.X_pos) &&
                Point.Intersect(p1.Y_pos, p2.Y_pos, q1.Y_pos, q2.Y_pos) &&
                Point.Cross_Op(p1, p2, q1) * Point.Cross_Op(p1, p2, q2) <= 0  &&
                Point.Cross_Op(q1, q2, p1) * Point.Cross_Op(q1, q2, p2) <= 0
            );
        }
        public static Point Get_intersection_point(Point p1, Point p2, Point q1, Point q2) { // 求两直线交点
            var a1 =  Point.Cross(q1, q2, p1);
            var a2 = -Point.Cross(q1, q2, p2);

            var intersection_point = (p1 * a2 + p2 * a1) / (a1 + a2);

            return intersection_point;
        }

        public override bool Equals(object obj) {
            return(
                obj is Point point &&
                this.X_pos == point.X_pos &&
                this.Y_pos == point.Y_pos
            );
        }
        public bool Equals(Point point) {
            return(
                this.X_pos == point.X_pos &&
                this.Y_pos == point.Y_pos
            );
        }
        public override string ToString() {
            return "(" + this.X_pos + ", " + this.Y_pos + ")";
        }
        public override int GetHashCode() { // 继承IEquatable<Point> 要求对GetHashCode进行重载
            unchecked{ // 使用 unchecked 防止溢出，这在计算哈希码时是常见的
                int hash = 17;
                hash = hash * 31 + ((int)(this.X_pos * 100) / 100.0f).GetHashCode();
                hash = hash * 31 + ((int)(this.Y_pos * 100) / 100.0f).GetHashCode();

                return hash;
            }
        }

        public float Dot(Point p) { // dot product
            return this.X_pos * p.X_pos + this.Y_pos * p.Y_pos;
        }
        public float Det(Point p) { // cross product
            return this.X_pos * p.Y_pos - this.Y_pos * p.X_pos;
        }
        public float Dist_To(Point p) { // distance to another point
            return (this - p).Abs();
        }
        public float Abs() { // magnitude
            return (float)Math.Sqrt((float)(this.X_pos * this.X_pos + this.Y_pos * this.Y_pos));
        }
    }
}