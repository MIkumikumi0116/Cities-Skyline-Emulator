using System;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;
using static NetInfo;
using System.Linq;

namespace Emulator_Backend {

    public class Build_Segment_Base : Action_Base {
        const float SEGMENT_PITCH = 80;
        const float SEGMENT_PITCH_LOAD_MAX = 110; // 好像系统自带的一些奇怪segment长度过长，把这些在On_Enable()加载时也筛掉

        // 注意事项：
        // 1. 因为游戏内有很多空的Node和Segment，所以这里用Dictionary来存储。同时，我们不会将position==(0,0,0)和startNode==0的Node和Segment存储进去
        // 2. 这三个数组尽可能保证只在这三个函数中写入：Make_segment_not_considering_intersection(), Release_segment(), Get_or_make_node()
        private Dictionary<ushort, Vector3> nodes;
        private Dictionary<ushort, Pair<ushort, ushort>> segments;
        private Dictionary<Vector3, ushort> position_to_node;
        private Dictionary<Pair<ushort, ushort>, ushort> two_node_to_segment; // 注意！Key.First <= Key.Second

        private readonly bool IS_DEBUG_MODE = true;

        public Build_Segment_Base() {
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action",    "string"},
                {"start_x",   "float"},
                {"start_z",   "float"},
                {"end_x",     "float"},
                {"end_z",     "float"},
                {"prefab_id", "uint"}
            };
        }

        public void On_Enable() {
            nodes = new Dictionary<ushort, Vector3>();
            segments = new Dictionary<ushort, Pair<ushort, ushort>>();
            position_to_node = new Dictionary<Vector3, ushort>();
            two_node_to_segment = new Dictionary<Pair<ushort, ushort>, ushort>();

            // Load nodes
            for (ushort i = 0; i < Singleton<NetManager>.instance.m_nodeCount; i++) {
                if (Singleton<NetManager>.instance.m_nodes.m_buffer[i].m_position.x == 0) continue;

                var position = Singleton<NetManager>.instance.m_nodes.m_buffer[i].m_position;
                position = Process_vector3(position);
                nodes.Add(i, position);
                position_to_node[position] = i;
            }
            // Load segments
            for (ushort i = 0; i < Singleton<NetManager>.instance.m_segmentCount; i++) {
                if (Singleton<NetManager>.instance.m_segments.m_buffer[i].m_startNode == 0) continue;
                if (Singleton<NetManager>.instance.m_segments.m_buffer[i].m_endNode == 0) continue;
                var start = Singleton<NetManager>.instance.m_segments.m_buffer[i].m_startNode;
                var end = Singleton<NetManager>.instance.m_segments.m_buffer[i].m_endNode;
                if ((nodes[start] - nodes[end]).magnitude > Build_Segment_Base.SEGMENT_PITCH_LOAD_MAX) continue;

                var two_node = Pair<ushort, ushort>.MakeSortedPair(start, end);
                segments.Add(i, two_node);
                two_node_to_segment[two_node] = i;
            }

            if(IS_DEBUG_MODE) Debug.Log("Build_Segment_Base enabled");
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_dict) {
            if (!this.Check_parameter_validity(action_dict, out string parameter_validity_message)) {
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            var start_x = Convert.ToSingle(action_dict["start_x"]);
            var start_z = Convert.ToSingle(action_dict["start_z"]);
            var end_x = Convert.ToSingle(action_dict["end_x"]);
            var end_z = Convert.ToSingle(action_dict["end_z"]);
            var prefab_id = Convert.ToUInt32(action_dict["prefab_id"]);

            this.Build_straight_road_perform(start_x, start_z, end_x, end_z, prefab_id);

            return new Dictionary<string, object> {
                {"status",  "ok"},
                {"message", "success"}
            };
        }

        private void Build_straight_road_perform(float start_x, float start_z, float end_x, float end_z, uint prefab_id) {
            var start_pos = new Vector3(start_x, 0, start_z);
            var end_pos = new Vector3(end_x, 0, end_z);
            var delta = end_pos - start_pos;
            var direction = delta.normalized;
            var length = delta.magnitude;

            float delta_pos = 0;
            for (; delta_pos < length - Build_Segment_Base.SEGMENT_PITCH; delta_pos += Build_Segment_Base.SEGMENT_PITCH) {
                this.Make_segments(
                    start_pos + direction * delta_pos,
                    start_pos + direction * (delta_pos + Build_Segment_Base.SEGMENT_PITCH),
                    prefab_id
                );
            }
            this.Make_segments(
                start_pos + direction * delta_pos,
                end_pos,
                prefab_id
            );
        }



        private void Make_segments(Vector3 start, Vector3 end, uint prefab_id) {

            List<Pair<Point, ushort>> intersections = new List<Pair<Point, ushort>>();

            Point start_p = new Point(start.x, start.z);
            Point end_p = new Point(end.x, end.z);

            if (IS_DEBUG_MODE) Debug.Log(" ============== Start Make Segments =========== ");

            // 1. 遍历所有segment求交点，得到 intersections[..]，记录交点本身和对应边
            foreach (KeyValuePair<ushort, Pair<ushort, ushort>> segment in segments) {
                if (nodes[segment.Value.First] == null || nodes[segment.Value.Second] == null) {
                    Debug.LogError("nodes is null in Make_segments!");
                    continue;
                }
                Point segment_p1 = new Point(nodes[segment.Value.First].x, nodes[segment.Value.First].z);
                Point segment_p2 = new Point(nodes[segment.Value.Second].x, nodes[segment.Value.Second].z);
                if (segment_p1 == segment_p2 || segment_p1 == start_p || segment_p2 == start_p || segment_p1 == end_p || segment_p2 == end_p) continue;

                // 先确定相交才可以求交点，因为第一个函数是“两线段是否相交”，第二个函数是“两直线求交点”
                if (!Point.intersect(start_p, end_p, segment_p1, segment_p2)) continue; // 不相交，则直接continue
                Point intersection = Point.getIntersection(start_p, end_p, segment_p1, segment_p2); // 相交，求交点

                if (IS_DEBUG_MODE) Debug.Log(intersection + " between " + start_p + ", " + end_p + "; " + segment_p1 + ", " + segment_p2);

                intersections.Add(Pair<Point, ushort>.MakePair(intersection, segment.Key));
            }

            // 2.1 去重！
            //intersections = intersections.Distinct().ToList();

            // 2.2 将所有交点按照距离stx和stz的距离排序
            intersections.Sort((a, b) => {
                float dist_a = (a.First - start_p).abs();
                float dist_b = (b.First - start_p).abs();
                return dist_a.CompareTo(dist_b);
            });

            //foreach (var intersect in intersections) {
            //    var segment = segments[intersect.Second];
            //    Debug.Log(intersect.First + " between " + nodes[segment.First] + ", " + nodes[segment.Second]);
            //}

            /*
            3. 遍历每个交点，假设当前遍历到交点i
               - 删除对应边
               - 在交点处新增node
               - 连接node与上个点和原边的两个点（不可递归）
               - 将上个点替换为当前node
            */
            Vector3 last_p = start;
            for(ushort i = 0; i < intersections.Count; i++) {
                Point intersect = intersections[i].First;
                ushort segment_id = intersections[i].Second;
                Vector3 segment_node1 = nodes[segments[segment_id].First];
                Vector3 segment_node2 = nodes[segments[segment_id].Second];

                if (IS_DEBUG_MODE) Debug.Log("An intersection in " + intersect + " with segment from " + segment_node1 + " to " + segment_node2);

                // 删除对应边
                this.Release_Segment(segment_id);

                // 在交点处新增node
                Vector3 intersection = new Vector3(intersect.x, 0, intersect.y);
                this.Get_or_make_node(ref intersection, prefab_id);

                // 连接 node 与 上个点和原边的两个点（不可递归）
                if ((last_p - intersection).sqrMagnitude >= 0.1) { // sqrMagnitude复杂度低，反正这里都是经验值，不如来点小优化
                    this.Make_segment_not_considering_intersection(ref last_p, ref intersection, prefab_id);
                }
                if ((segment_node1 - intersection).sqrMagnitude >= 0.1) {
                    this.Make_segment_not_considering_intersection(ref segment_node1, ref intersection, prefab_id);
                }
                if ((segment_node2 - intersection).sqrMagnitude >= 0.1) {
                    this.Make_segment_not_considering_intersection(ref segment_node2, ref intersection, prefab_id);
                }

                // 将上个点(last_p)替换为当前node
                last_p = intersection;
            }

            // 4. 将末尾node连向(edx, edz)（不可递归）
            this.Make_segment_not_considering_intersection(ref last_p, ref end, prefab_id);
        }

        private ushort Make_segment_not_considering_intersection(ref Vector3 start_pos, ref Vector3 end_pos, uint prefab_id) {
            var start_node_id = this.Get_or_make_node(ref start_pos, prefab_id);
            var end_node_id = this.Get_or_make_node(ref end_pos, prefab_id);
            Vector3 direction = (end_pos - start_pos).normalized;

            if (start_node_id == end_node_id) {
                throw new Exception("The length of segment is zero.");
            }

            var two_node = Pair<ushort, ushort>.MakeSortedPair(start_node_id, end_node_id);
            if (two_node_to_segment.ContainsKey(two_node)) {
                return two_node_to_segment[two_node];
            }

            if (
                Singleton<NetManager>.instance.CreateSegment(
                    out ushort segment_id,
                    ref Singleton<SimulationManager>.instance.m_randomizer,
                    PrefabCollection<NetInfo>.GetPrefab(prefab_id),
                    start_node_id,
                    end_node_id,
                    direction,
                    -direction,
                    SimulationManager.instance.m_currentBuildIndex,
                    SimulationManager.instance.m_currentBuildIndex,
                    false
                )
            ) {
                ++SimulationManager.instance.m_currentBuildIndex;
                this.segments[segment_id] = two_node;
                this.two_node_to_segment[two_node] = segment_id;
                if (IS_DEBUG_MODE) Debug.Log("Made a segment in " + start_pos + " to " + end_pos + " at " + two_node.First + ", " + two_node.Second);
            } else {
                throw new Exception("Error creating segment");
            }

            return segment_id;
        }

        // Release Segment == Remove Segment, 使用Release的原因是与Skyline代码统一
        private void Release_Segment(ushort segment_id) {
            var two_node = Pair<ushort, ushort>.MakeSortedPair(segments[segment_id].First, segments[segment_id].Second);
            Singleton<NetManager>.instance.ReleaseSegment(segment_id, true); // 这里的true表示保持segment对应的node
            this.segments.Remove(segment_id);
            this.two_node_to_segment.Remove(two_node);
        }

        private ushort Get_or_make_node(ref Vector3 node_pos, uint prefab_id) {
            node_pos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(new Vector3(node_pos.x, 0, node_pos.z)) + 1f;

            node_pos = Process_vector3(node_pos);

            if (this.position_to_node.ContainsKey(node_pos)) {
                return this.position_to_node[node_pos];
            }

            if (
                Singleton<NetManager>.instance.CreateNode(
                    out ushort node_id,
                    ref Singleton<SimulationManager>.instance.m_randomizer,
                    PrefabCollection<NetInfo>.GetPrefab(prefab_id),
                    node_pos,
                    SimulationManager.instance.m_currentBuildIndex
                )
            ) {
                ++SimulationManager.instance.m_currentBuildIndex;
                this.nodes[node_id] = node_pos;
                this.position_to_node[node_pos] = node_id;
                if (IS_DEBUG_MODE) Debug.Log("Made a node in " + node_pos);
                return node_id;
            } else {
                throw new Exception("Error creating node " + node_pos.x + ", " + node_pos.y + "at" + node_pos);
            }
        }

        private Vector3 Process_vector3(Vector3 input) {
            return new Vector3(
                (int)(input.x * 100) / 100.0f,
                (int)(input.y * 100) / 100.0f,
                (int)(input.z * 100) / 100.0f
            );
        }
    }

}