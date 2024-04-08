using System;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;



namespace Emulator_Backend {

    public class Build_Segment_Base : Action_Base {
        const float SEGMENT_PITCH = 80;
        const float SEGMENT_PITCH_LOAD_MAX = 110; // 好像系统自带的一些奇怪segment长度过长，把这些在On_enable()加载时也筛掉

        private readonly bool IS_DEBUG_MODE = false;

        // 注意事项：
        // 1. 因为游戏内有很多空的Node和Segment，所以这里用Dictionary来存储。同时，我们不会将position==(0,0,0)和startNode==0的Node和Segment存储进去
        // 2. 这三个数组尽可能保证只在这三个函数中写入：Make_segment_not_considering_intersection(), Release_segment(), Get_or_make_node()
        private readonly Dictionary<ushort, Vector3> node_dict                              = new Dictionary<ushort, Vector3>();
        private readonly Dictionary<ushort, Pair<ushort, ushort>> segment_dict              = new Dictionary<ushort, Pair<ushort, ushort>>();
        private readonly Dictionary<Vector3, ushort> position_to_node_dict                  = new Dictionary<Vector3, ushort>();
        private readonly Dictionary<Pair<ushort, ushort>, ushort> node_pair_to_segment_dict = new Dictionary<Pair<ushort, ushort>, ushort>(); // 注意！Key.First <= Key.Second

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

        public override void On_enable() {
            // Load nodes
            for (ushort node_index = 0; node_index < Singleton<NetManager>.instance.m_nodeCount; node_index++) {
                if (Singleton<NetManager>.instance.m_nodes.m_buffer[node_index].m_position.x == 0){
                    continue;
                }

                var position = Singleton<NetManager>.instance.m_nodes.m_buffer[node_index].m_position;
                position = this.Process_vector3(position);
                this.node_dict.Add(node_index, position);
                this.position_to_node_dict[position] = node_index;
            }

            // Load segments
            for (ushort segments_index = 0; segments_index < Singleton<NetManager>.instance.m_segmentCount; segments_index++) {
                if (
                    Singleton<NetManager>.instance.m_segments.m_buffer[segments_index].m_startNode == 0 ||
                    Singleton<NetManager>.instance.m_segments.m_buffer[segments_index].m_endNode   == 0
                ){
                    continue;
                }

                var start = Singleton<NetManager>.instance.m_segments.m_buffer[segments_index].m_startNode;
                var end   = Singleton<NetManager>.instance.m_segments.m_buffer[segments_index].m_endNode;
                if ((node_dict[start] - node_dict[end]).magnitude > Build_Segment_Base.SEGMENT_PITCH_LOAD_MAX){
                    continue;
                }

                var node_pair = Pair<ushort, ushort>.Make_sorted_pair(start, end);
                this.segment_dict.Add(segments_index, node_pair);
                this.node_pair_to_segment_dict[node_pair] = segments_index;
            }

            if(IS_DEBUG_MODE) Debug.Log("Build_Segment_Base enabled");
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_param_dict) {
            if (!this.Check_parameter_validity(action_param_dict, out string parameter_validity_message)) {
                return new Dictionary<string, object> {
                    {"status", "error"},
                    {"message", parameter_validity_message}
                };
            }

            var start_x   = Convert.ToSingle(action_param_dict["start_x"]);
            var start_z   = Convert.ToSingle(action_param_dict["start_z"]);
            var end_x     = Convert.ToSingle(action_param_dict["end_x"]);
            var end_z     = Convert.ToSingle(action_param_dict["end_z"]);
            var prefab_id = Convert.ToUInt32(action_param_dict["prefab_id"]);

            this.Build_straight_road_perform(start_x, start_z, end_x, end_z, prefab_id);

            return new Dictionary<string, object> {
                {"status",  "ok"},
                {"message", "success"}
            };
        }

        private void Build_straight_road_perform(float start_x, float start_z, float end_x, float end_z, uint prefab_id) {
            var start_pos = new Vector3(start_x, 0, start_z);
            var end_pos   = new Vector3(end_x, 0, end_z);
            var delta     = end_pos - start_pos;
            var direction = delta.normalized;
            var length    = delta.magnitude;

            for (float delta_pos = 0; delta_pos + Build_Segment_Base.SEGMENT_PITCH <= length; delta_pos += Build_Segment_Base.SEGMENT_PITCH) {
                this.Make_segment(
                    start_pos + direction * delta_pos,
                    start_pos + direction * Math.Min(delta_pos + Build_Segment_Base.SEGMENT_PITCH, length),
                    prefab_id
                );
            }
        }



        private void Make_segment(Vector3 start, Vector3 end, uint prefab_id) {
            var intersection_point_list = new List<Pair<Point, ushort>>();

            var start_point = new Point(start.x, start.z);
            var end_point   = new Point(end.x,   end.z);

            if (IS_DEBUG_MODE) Debug.Log(" ============== Start Make Segments =========== ");

            // 1. 遍历所有segment求交点，得到 intersection_point_list[..]，记录交点本身和对应边
            foreach (var segment_info in this.segment_dict) {
                if (this.node_dict[segment_info.Value.First] == null || this.node_dict[segment_info.Value.Second] == null) {
                    Debug.LogError("nodes is null in Make_segments!");
                    continue;
                }

                var segment_point_1 = new Point(this.node_dict[segment_info.Value.First].x,  this.node_dict[segment_info.Value.First].z);
                var segment_point_2 = new Point(this.node_dict[segment_info.Value.Second].x, this.node_dict[segment_info.Value.Second].z);

                if (
                    segment_point_1 == segment_point_2 ||
                    segment_point_1 == start_point     ||
                    segment_point_2 == start_point     ||
                    segment_point_1 == end_point       ||
                    segment_point_2 == end_point
                ){
                    continue;
                }

                // 先确定相交才可以求交点，因为第一个函数是“两线段是否相交”，第二个函数是“两直线求交点”
                if (!Point.Intersect(start_point, end_point, segment_point_1, segment_point_2)){
                    continue; // 不相交，则直接continue
                }

                var intersection_point = Point.Get_intersection_point(start_point, end_point, segment_point_1, segment_point_2); // 相交，求交点

                if (IS_DEBUG_MODE) Debug.Log(intersection_point + " between " + start_point + ", " + end_point + "; " + segment_point_1 + ", " + segment_point_2);

                intersection_point_list.Add(new Pair<Point, ushort>(intersection_point, segment_info.Key));
            }

            // 2.1 去重！
            //intersections = intersections.Distinct().ToList();

            // 2.2 将所有交点按照距离stx和stz的距离排序
            intersection_point_list.Sort(
                (point_a, point_b) => {
                    float dist_a = (point_a.First - start_point).Abs();
                    float dist_b = (point_b.First - start_point).Abs();

                    return dist_a.CompareTo(dist_b);
                }
            );

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
            Vector3 last_point = start;
            foreach(var intersection_point_info in intersection_point_list){
                Point intersect_point  = intersection_point_info.First;
                ushort segment_id      = intersection_point_info.Second;

                Vector3 segment_node_1 = this.node_dict[segment_dict[segment_id].First];
                Vector3 segment_node_2 = this.node_dict[segment_dict[segment_id].Second];

                if (IS_DEBUG_MODE) Debug.Log("An intersection in " + intersect_point + " with segment from " + segment_node_1 + " to " + segment_node_2);

                // 删除对应边
                this.Release_segment(segment_id);

                // 在交点处新增node
                Vector3 intersection_point = new Vector3(intersect_point.X_pos, 0, intersect_point.Y_pos);
                this.Get_or_make_node(ref intersection_point, prefab_id);

                // 连接 node 与 上个点和原边的两个点（不可递归）
                if ((last_point     - intersection_point).sqrMagnitude >= 0.1) { // sqrMagnitude复杂度低，反正这里都是经验值，不如来点小优化
                    this.Make_segment_not_considering_intersection(ref last_point,     ref intersection_point, prefab_id);
                }
                if ((segment_node_1 - intersection_point).sqrMagnitude >= 0.1) {
                    this.Make_segment_not_considering_intersection(ref segment_node_1, ref intersection_point, prefab_id);
                }
                if ((segment_node_2 - intersection_point).sqrMagnitude >= 0.1) {
                    this.Make_segment_not_considering_intersection(ref segment_node_2, ref intersection_point, prefab_id);
                }

                // 将上个点(last_p)替换为当前node
                last_point = intersection_point;
            }

            // 4. 将末尾node连向(edx, edz)（不可递归）
            this.Make_segment_not_considering_intersection(ref last_point, ref end, prefab_id);
        }

        private ushort Make_segment_not_considering_intersection(ref Vector3 start_pos, ref Vector3 end_pos, uint prefab_id) {
            var start_node_id = this.Get_or_make_node(ref start_pos, prefab_id);
            var end_node_id   = this.Get_or_make_node(ref end_pos, prefab_id);

            Vector3 direction = (end_pos - start_pos).normalized;

            if (start_node_id == end_node_id) {
                throw new Exception("The length of segment is zero.");
            }

            var node_pair = Pair<ushort, ushort>.Make_sorted_pair(start_node_id, end_node_id);
            if (this.node_pair_to_segment_dict.ContainsKey(node_pair)) {
                return this.node_pair_to_segment_dict[node_pair];
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
                SimulationManager.instance.m_currentBuildIndex++;

                this.segment_dict[segment_id] = node_pair;
                this.node_pair_to_segment_dict[node_pair] = segment_id;

                if (IS_DEBUG_MODE) Debug.Log("Made a segment in " + start_pos + " to " + end_pos + " at " + node_pair.First + ", " + node_pair.Second);
            } else {
                throw new Exception("Error creating segment");
            }

            return segment_id;
        }

        private void Release_segment(ushort segment_id) { // Release Segment == Remove Segment, 使用Release的原因是与Skyline代码统一
            Singleton<NetManager>.instance.ReleaseSegment(segment_id, true); // 这里的true表示保持segment对应的node

            var node_pair = Pair<ushort, ushort>.Make_sorted_pair(segment_dict[segment_id].First, segment_dict[segment_id].Second);
            this.segment_dict.Remove(segment_id);
            this.node_pair_to_segment_dict.Remove(node_pair);
        }

        private ushort Get_or_make_node(ref Vector3 node_pos, uint prefab_id) {
            node_pos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(new Vector3(node_pos.x, 0, node_pos.z)) + 1f;

            node_pos = this.Process_vector3(node_pos);

            if (this.position_to_node_dict.ContainsKey(node_pos)) {
                return this.position_to_node_dict[node_pos];
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
                SimulationManager.instance.m_currentBuildIndex++;

                this.node_dict[node_id] = node_pos;
                this.position_to_node_dict[node_pos] = node_id;

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