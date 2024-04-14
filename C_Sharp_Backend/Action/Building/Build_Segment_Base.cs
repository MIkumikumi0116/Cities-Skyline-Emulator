using System;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;



namespace Emulator_Backend {

    public class Build_Segment_Base: Action_Base {
        const float SEGMENT_PITCH     = 80;
        // const float MIN_NODE_DISTANCE = 24;  // 两个node之间的最小距离，新添加node离已有node的距离小于这个值时，合并为一个node
        const float MIN_SEGMENT_ANGLE = 30;  // 两个segment之间的最小夹角，新添加segment与已有segment的夹角小于这个值时，报错

        private NetManager        net_manager        = null;
        private TerrainManager    terrain_manager    = null;
        private SimulationManager simulation_manager = null;

        private readonly List<ushort>             new_node_id_list                 = new List<ushort>(); // 一次调用中新建的node，失败时回滚删除
        private readonly List<ushort>             new_segment_id_list              = new List<ushort>();
        private readonly List<Pair<Point, Point>> released_segment_node_point_list = new List<Pair<Point, Point>>(); // 一次调用中删除的segment，失败时回滚恢复

        private readonly Dictionary<ushort, Point> node_dict                                = new Dictionary<ushort, Point>();
        private readonly Dictionary<ushort, Pair<ushort, ushort>> segment_dict              = new Dictionary<ushort, Pair<ushort, ushort>>();
        private readonly Dictionary<Point, ushort> point_to_node_dict                       = new Dictionary<Point, ushort>();
        private readonly Dictionary<Pair<ushort, ushort>, ushort> node_pair_to_segment_dict = new Dictionary<Pair<ushort, ushort>, ushort>(); // 注意！Key.First <= Key.Second

        public Build_Segment_Base(){
            this.parameter_type_dict = new Dictionary<string, string>{
                {"action",    "string"},
                {"start_x",   "float"},
                {"start_z",   "float"},
                {"end_x",     "float"},
                {"end_z",     "float"},
                {"prefab_id", "uint"}
            };
        }

        public override void On_level_loaded(){
            this.net_manager        = Singleton<NetManager>.instance;
            this.terrain_manager    = Singleton<TerrainManager>.instance;
            this.simulation_manager = Singleton<SimulationManager>.instance;

            this.On_level_loaded_load_node();
            this.On_level_loaded_load_segment();
        }

        private void On_level_loaded_load_node(){
            ushort node_index = 0;
            var    node_list  = this.net_manager.m_nodes.m_buffer;
            foreach (var node_info in node_list){
                node_index++;

                if ((node_info.m_flags & NetNode.Flags.Created) == 0){
                    continue;
                }

                var pos       = node_info.m_position;
                var pos_point = new Point(pos.x, pos.z);
                this.Standardize_point(ref pos_point);

                var real_index = (ushort)(node_index - 1);
                this.node_dict.Add(real_index, pos_point);
                this.point_to_node_dict[pos_point] = real_index;
            }
        }

        private void On_level_loaded_load_segment(){
            ushort segment_index = 0;
            var    segment_list  = this.net_manager.m_segments.m_buffer;
            foreach(var segment_info in segment_list){
                segment_index++;

                if ((segment_info.m_flags & NetSegment.Flags.Created) == 0){
                    continue;
                }

                var start_node_index = segment_info.m_startNode;
                var end_node_index   = segment_info.m_endNode;

                var real_index = (ushort)(segment_index - 1);
                var node_pair = Pair<ushort, ushort>.Make_sorted_pair(start_node_index, end_node_index);
                this.segment_dict.Add(real_index, node_pair);
                this.node_pair_to_segment_dict[node_pair] = real_index;
            }
        }

        public override void On_released(){
            this.node_dict.Clear();
            this.segment_dict.Clear();
            this.point_to_node_dict.Clear();
            this.node_pair_to_segment_dict.Clear();
        }

        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_param_dict){
            if (
                this.net_manager        == null ||
                this.terrain_manager    == null ||
                this.simulation_manager == null
            ){
                this.net_manager        = Singleton<NetManager>.instance;
                this.terrain_manager    = Singleton<TerrainManager>.instance;
                this.simulation_manager = Singleton<SimulationManager>.instance;
            }

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

            var succeed_flag = this.Build_straight_road_perform(start_x, start_z, end_x, end_z, prefab_id, out string error_message);

            if (succeed_flag){
                return new Dictionary<string, object> {
                    {"status",  "ok"},
                    {"message", "success"}
                };
            }
            else{
                return new Dictionary<string, object> {
                    {"status",  "error"},
                    {"message", error_message}
                };
            }
        }

        private bool Build_straight_road_perform(float start_x, float start_z, float end_x, float end_z, uint prefab_id, out string error_message){
            var start_pos = new Vector3(start_x, 0, start_z);
            var end_pos   = new Vector3(end_x,   0, end_z);
            var delta     = end_pos - start_pos;
            var direction = delta.normalized;
            var length    = delta.magnitude;

            var succeed_flag = true;
            error_message    = "";
            for (float delta_pos = 0; delta_pos <= length; delta_pos += Build_Segment_Base.SEGMENT_PITCH) {
                var segment_start_pos   = start_pos + direction * delta_pos;
                var segment_end_pos     = start_pos + direction * Math.Min(delta_pos + Build_Segment_Base.SEGMENT_PITCH, length);

                var segment_start_point = new Point(segment_start_pos.x, segment_start_pos.z);
                var segment_end_point   = new Point(segment_end_pos.x,   segment_end_pos.z);

                this.Standardize_point(ref segment_start_point);
                this.Standardize_point(ref segment_end_point);

                succeed_flag = this.Make_segment(segment_start_point, segment_end_point, prefab_id, out error_message);

                if (!succeed_flag) {
                    foreach(var segment_id in this.new_segment_id_list) {
                        this.Release_segment(segment_id);
                    }
                    foreach(var node_id    in this.new_node_id_list) {
                        this.Release_node(node_id);
                    }
                    foreach(var segment_node_point_pair in this.released_segment_node_point_list) {
                        this.Make_segment_no_intersection(segment_node_point_pair.First, segment_node_point_pair.Second, prefab_id);
                    }

                    break;
                }
            }

            this.new_node_id_list.Clear();
            this.new_segment_id_list.Clear();
            this.released_segment_node_point_list.Clear();

            return succeed_flag;
        }

        private bool Make_segment(Point start_point, Point end_point, uint prefab_id, out string error_message){
            error_message = "";

            // 1. 遍历所有segment求交点，得到 intersection_point_list[..]，记录交点本身和对应边
            this.Get_intersection_point_and_segment_list(out List<Pair<Point, ushort>> intersection_point_and_segment_list, start_point, end_point, out error_message);

            // 2 将所有交点按照距离stx和stz的距离排序
            intersection_point_and_segment_list.Sort(
                (point_a, point_b) => {
                    float dist_a = (point_a.First - start_point).Abs();
                    float dist_b = (point_b.First - start_point).Abs();

                    return dist_a.CompareTo(dist_b);
                }
            );

            /*
            3. 遍历每个交点，假设当前遍历到交点i
               - 删除对应边
               - 在交点处新增node
               - 连接node与上个点和原边的两个点（不可递归）
               - 将上个点替换为当前node
            */
            var current_point = start_point;
            foreach(var intersection_point_and_segment in intersection_point_and_segment_list){
                var intersection_point       = intersection_point_and_segment.First;
                var intersection_segment_id  = intersection_point_and_segment.Second;
                var intersection_point_point = this.Process_intersection_point(intersection_point, intersection_segment_id, prefab_id, current_point);

                current_point = intersection_point_point;
            }

            // 4. 将末尾node连向(edx, edz)（不可递归）
            this.Make_segment_no_intersection(current_point, end_point, prefab_id);

            return true;
        }

        private bool Get_intersection_point_and_segment_list(out List<Pair<Point, ushort>> intersection_point_and_segment_list, Point start_point, Point end_point, out string error_message){
            error_message = "";
            intersection_point_and_segment_list = new List<Pair<Point, ushort>>();
            foreach (var segment_id_and_node in this.segment_dict) {
                var node_1_point = this.node_dict[segment_id_and_node.Value.First];
                var node_2_point = this.node_dict[segment_id_and_node.Value.Second];

                var overlap_flag      = false;
                var intersection_flag = false;

                if (
                    node_1_point == start_point ||
                    node_2_point == start_point ||
                    node_1_point == end_point   ||
                    node_2_point == end_point
                ){
                    overlap_flag = true;
                }
                else if (Point.Intersect(start_point, end_point, node_1_point, node_2_point)){
                    intersection_flag = true;
                }

                // if (overlap_flag || intersection_flag){
                //     if (Point.Get_intersection_angel(start_point, end_point, node_1_point, node_2_point) < Build_Segment_Base.MIN_SEGMENT_ANGLE){
                //         error_message = "The angle with the existing road is too small.";
                //         return false;
                //     }
                // }

                if (intersection_flag){
                    var intersection_point = Point.Get_intersection_point(start_point, end_point, node_1_point, node_2_point); // 相交，求交点
                    this.Standardize_point(ref intersection_point);
                    intersection_point_and_segment_list.Add(new Pair<Point, ushort>(intersection_point, segment_id_and_node.Key));
                }
            }

            return true;
        }

        private Point Process_intersection_point(Point intersection_point, ushort intersection_segment_id, uint prefab_id, Point current_point){
            var intersection_segment_node_id_1    = this.segment_dict[intersection_segment_id].First;
            var intersection_segment_node_id_2    = this.segment_dict[intersection_segment_id].Second;
            var intersection_segment_node_point_1 = this.node_dict[intersection_segment_node_id_1];
            var intersection_segment_node_point_2 = this.node_dict[intersection_segment_node_id_2];

            this.released_segment_node_point_list.Add(new Pair<Point, Point>(intersection_segment_node_point_1, intersection_segment_node_point_2));
            this.Release_segment(intersection_segment_id);

            var intersection_node_id = this.Get_or_make_node(intersection_point, prefab_id);

            this.Make_segment_no_intersection(intersection_point, intersection_segment_node_point_1, prefab_id);
            this.Make_segment_no_intersection(intersection_point, intersection_segment_node_point_2, prefab_id);
            this.Make_segment_no_intersection(intersection_point, current_point, prefab_id);

            return this.node_dict[intersection_node_id];
        }

        private void Make_segment_no_intersection(Point start_point, Point end_point, uint prefab_id){
            var start_id = this.Get_or_make_node(start_point, prefab_id);
            var end_id   = this.Get_or_make_node(end_point,   prefab_id);

            var node_pair = Pair<ushort, ushort>.Make_sorted_pair(start_id, end_id);
            if (this.node_pair_to_segment_dict.ContainsKey(node_pair)) {
                return;
            }

            var start_pos = new Vector3(
                start_point.X_pos,
                this.terrain_manager.SampleRawHeightSmooth(
                    new Vector3(start_point.X_pos, 0, start_point.Z_pos)
                ) + 1f,
                start_point.Z_pos
            );

            var end_pos = new Vector3(
                end_point.X_pos,
                this.terrain_manager.SampleRawHeightSmooth(
                    new Vector3(end_point.X_pos, 0, end_point.Z_pos)
                ) + 1f,
                end_point.Z_pos
            );

            var direction = (end_pos - start_pos).normalized;
            this.net_manager.CreateSegment(
                out ushort segment_id,
                ref this.simulation_manager.m_randomizer,
                PrefabCollection<NetInfo>.GetPrefab(prefab_id),
                start_id,
                end_id,
                direction,
                -direction,
                this.simulation_manager.m_currentBuildIndex,
                this.simulation_manager.m_currentBuildIndex,
                false
            );
            this.simulation_manager.m_currentBuildIndex++;

            this.segment_dict[segment_id]             = node_pair;
            this.node_pair_to_segment_dict[node_pair] = segment_id;

            this.new_segment_id_list.Add(segment_id);

            return;
        }

        private void Release_segment(ushort segment_id){ // Release Segment == Remove Segment, 使用Release的原因是与Skyline代码统一
            this.net_manager.ReleaseSegment(segment_id, true); // keep node = true，只删segment，不删node

            var node_pair = Pair<ushort, ushort>.Make_sorted_pair(this.segment_dict[segment_id].First, this.segment_dict[segment_id].Second);
            this.node_pair_to_segment_dict.Remove(node_pair);

            this.segment_dict.Remove(segment_id);
        }

        private void Release_node(ushort node_id){
            this.net_manager.ReleaseNode(node_id);

            this.point_to_node_dict.Remove(this.node_dict[node_id]);
            this.node_dict.Remove(node_id);
        }

        private ushort Get_or_make_node(Point point, uint prefab_id) {
            if (this.point_to_node_dict.ContainsKey(point)) {
                return this.point_to_node_dict[point];
            }

            var pos = new Vector3(
                point.X_pos,
                this.terrain_manager.SampleRawHeightSmooth(
                    new Vector3(point.X_pos, 0, point.Z_pos)
                ) + 1f,
                point.Z_pos
            );

            this.net_manager.CreateNode(
                out ushort node_id,
                ref this.simulation_manager.m_randomizer,
                PrefabCollection<NetInfo>.GetPrefab(prefab_id),
                pos,
                this.simulation_manager.m_currentBuildIndex
            );
            this.simulation_manager.m_currentBuildIndex++;

            this.node_dict[node_id]        = point;
            this.point_to_node_dict[point] = node_id;

            this.new_node_id_list.Add(node_id);

            return node_id;
        }

        private void Standardize_point(ref Point point){
            point.X_pos = (int)(point.X_pos * 100) / 100.0f;
            point.Z_pos = (int)(point.Z_pos * 100) / 100.0f;
        }
    }

}