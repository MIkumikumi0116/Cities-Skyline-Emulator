using System;
using System.Collections.Generic;
using UnityEngine;
using ColossalFramework;



namespace Emulator_Backend {

    public class Build_Segment_Base : Action_Base {
        const float SEGMENT_PITCH     = 80;
        const float MIN_NODE_DISTANCE = 8;   // 两个node之间的最小距离，新添加node离已有node的距离小于这个值时，合并为一个node
        const float MIN_SEGMENT_ANGLE = 30;  // 两个segment之间的最小夹角，新添加segment与已有segment的夹角小于这个值时，报错

        private readonly bool IS_DEBUG_MODE = true;
        private NetManager net_manager;

        private List<ushort> new_node_id_list    = new List<ushort>(); // 一次调用中新建的node，失败时回滚删除
        private List<ushort> new_segment_id_list = new List<ushort>();

        // 注意事项：
        // 1. 因为游戏内有很多空的Node和Segment，所以这里用Dictionary来存储。同时，我们不会将position==(0,0,0)和startNode==0的Node和Segment存储进去
        // 2. 这三个数组尽可能保证只在这三个函数中写入：Make_segment_not_considering_intersection(), Release_segment(), Get_or_make_node()
        private readonly Dictionary<ushort, Vector3> node_dict                              = new Dictionary<ushort, Vector3>();
        private readonly Dictionary<ushort, Pair<ushort, ushort>> segment_dict              = new Dictionary<ushort, Pair<ushort, ushort>>();
        private readonly Dictionary<Vector3, ushort> position_to_node_dict                  = new Dictionary<Vector3, ushort>();
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
            this.net_manager = Singleton<NetManager>.instance;

            ushort node_index = 0;
            var    node_list  = this.net_manager.m_nodes.m_buffer;
            foreach (var node_info in node_list){
                node_index++;

                if ((node_info.m_flags & NetNode.Flags.Created) == 0){
                    continue;
                }

                var position = node_info.m_position;
                position = this.Process_vector3(position);

                this.node_dict.Add((ushort)(node_index - 1), position);
                this.position_to_node_dict[position] = (ushort)(node_index - 1u);
            }

            ushort segment_index = 0;
            var    segment_list  = this.net_manager.m_segments.m_buffer;
            foreach(var segment_info in segment_list){
                segment_index++;

                if ((segment_info.m_flags & NetSegment.Flags.Created) == 0){
                    continue;
                }

                var start_node_index = segment_info.m_startNode;
                var end_node_index   = segment_info.m_endNode;

                var node_pair = Pair<ushort, ushort>.Make_sorted_pair(start_node_index, end_node_index);
                this.segment_dict.Add((ushort)(segment_index - 1), node_pair);
                this.node_pair_to_segment_dict[node_pair] = (ushort)(segment_index - 1);
            }
        }

        public override void On_released(){
            this.node_dict.Clear();
            this.segment_dict.Clear();
            this.position_to_node_dict.Clear();
            this.node_pair_to_segment_dict.Clear();
        }



        public override Dictionary<string, object> Perform_action(Dictionary<string, object> action_param_dict){
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
            for (float delta_pos = 0; delta_pos + Build_Segment_Base.SEGMENT_PITCH <= length; delta_pos += Build_Segment_Base.SEGMENT_PITCH) {
                succeed_flag = this.Make_segment(
                    start_pos + direction * delta_pos,
                    start_pos + direction * Math.Min(delta_pos + Build_Segment_Base.SEGMENT_PITCH, length),
                    prefab_id,
                    out error_message
                );

                if (!succeed_flag) {
                    foreach(var segment_index in this.new_segment_id_list) {
                        this.Release_segment(segment_index);
                    }
                    foreach (var node_index   in this.new_node_id_list) {
                        this.Release_node(node_index);
                    }

                    break;
                }
            }

            this.new_node_id_list.Clear();
            this.new_segment_id_list.Clear();

            if (succeed_flag){
                return true;
            }
            else{
                return false;
            }
        }



        private bool Make_segment(Vector3 start_pos, Vector3 end_pos, uint prefab_id, out string error_message){
            error_message   = "";

            var start_point = new Point(start_pos.x, start_pos.z);
            var end_point   = new Point(end_pos.x,   end_pos.z);

            // 1. 遍历所有segment求交点，得到 intersection_point_list[..]，记录交点本身和对应边
            var intersection_point_and_segment_list = new List<Pair<Point, ushort>>();
            foreach (var segment_id_and_node_pair in this.segment_dict) {
                var node_1 = this.node_dict[segment_id_and_node_pair.Value.First];
                var node_2 = this.node_dict[segment_id_and_node_pair.Value.Second];

                var node_1_point = new Point(node_1.x, node_1.z);
                var node_2_point = new Point(node_2.x, node_2.z);

                if (
                    node_1_point == start_point ||
                    node_2_point == start_point ||
                    node_1_point == end_point   ||
                    node_2_point == end_point
                ){
                    continue;
                }

                // 先确定相交才可以求交点，因为第一个函数是“两线段是否相交”，第二个函数是“两直线求交点”
                if (!Point.Intersect(start_point, end_point, node_1_point, node_2_point)){
                    continue; // 不相交，则直接continue
                }

                var intersection_point_and_segment = Point.Get_intersection_point(start_point, end_point, node_1_point, node_2_point); // 相交，求交点
                intersection_point_and_segment_list.Add(new Pair<Point, ushort>(intersection_point_and_segment, segment_id_and_node_pair.Key));

                if (IS_DEBUG_MODE){
                    Debug.Log(intersection_point_and_segment + " between " + start_point + ", " + end_point + "; " + node_1_point + ", " + node_2_point);
                }
            }

            // 2.1 去重！
            //intersections = intersections.Distinct().ToList();

            // 2.2 将所有交点按照距离stx和stz的距离排序
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
            Vector3 last_pos = start_pos;
            foreach(var intersection_point_and_segment in intersection_point_and_segment_list){
                var intersection_segment_id = intersection_point_and_segment.Second;

                var intersection_point_pos          = new Vector3(intersection_point_and_segment.First.X_pos, 0, intersection_point_and_segment.First.Z_pos);
                var intersection_segment_node_pos_1 = this.node_dict[this.segment_dict[intersection_segment_id].First];
                var intersection_segment_node_pos_2 = this.node_dict[this.segment_dict[intersection_segment_id].Second];

                Debug.Log((intersection_point_pos - intersection_segment_node_pos_1).magnitude);
                Debug.Log((intersection_point_pos - intersection_segment_node_pos_2).magnitude);

                var net_manager = Singleton<NetManager>.instance;
                if ((intersection_point_pos - intersection_segment_node_pos_1).magnitude < Build_Segment_Base.MIN_NODE_DISTANCE){
                    net_manager.MoveNode(this.segment_dict[intersection_segment_id].First, intersection_point_pos);

                    this.Make_segment_not_considering_intersection(ref last_pos, ref intersection_point_pos, prefab_id, ref new_node_id_list, ref new_segment_id_list);

                    if (IS_DEBUG_MODE){
                        Debug.Log("Move node " + this.segment_dict[intersection_segment_id].First.ToString() + " to " + intersection_point_pos.ToString());
                    }
                }
                else if((intersection_point_pos - intersection_segment_node_pos_2).magnitude < Build_Segment_Base.MIN_NODE_DISTANCE){
                    net_manager.MoveNode(this.segment_dict[intersection_segment_id].Second, intersection_point_pos);

                    this.Make_segment_not_considering_intersection(ref last_pos, ref intersection_point_pos, prefab_id, ref new_node_id_list, ref new_segment_id_list);

                    if (IS_DEBUG_MODE){
                        Debug.Log("Move node " + this.segment_dict[intersection_segment_id].Second.ToString() + " to " + intersection_point_pos.ToString());
                    }
                }
                else{
                    this.Release_segment(intersection_segment_id);
                    this.Get_or_make_node(ref intersection_point_pos, prefab_id, ref new_node_id_list);

                    this.Make_segment_not_considering_intersection(ref last_pos,                        ref intersection_point_pos, prefab_id, ref new_node_id_list, ref new_segment_id_list);
                    this.Make_segment_not_considering_intersection(ref intersection_segment_node_pos_1, ref intersection_point_pos, prefab_id, ref new_node_id_list, ref new_segment_id_list);
                    this.Make_segment_not_considering_intersection(ref intersection_segment_node_pos_2, ref intersection_point_pos, prefab_id, ref new_node_id_list, ref new_segment_id_list);
                }

                last_pos = intersection_point_pos;
            }

            // 4. 将末尾node连向(edx, edz)（不可递归）
            this.Make_segment_not_considering_intersection(ref last_pos, ref end_pos, prefab_id, ref new_node_id_list, ref new_segment_id_list);

            return true;
        }

        private void Make_segment_not_considering_intersection(ref Vector3 start_pos, ref Vector3 end_pos, uint prefab_id, ref List<ushort> new_node_id_list, ref List<ushort> new_segment_id_list){
            var start_node_id = this.Get_or_make_node(ref start_pos, prefab_id, ref new_node_id_list);
            var end_node_id   = this.Get_or_make_node(ref end_pos,   prefab_id, ref new_node_id_list);

            if (start_node_id == end_node_id) {
                return;
            }

            var node_pair = Pair<ushort, ushort>.Make_sorted_pair(start_node_id, end_node_id);
            if (this.node_pair_to_segment_dict.ContainsKey(node_pair)) {
                return;
            }

            Vector3 direction = (end_pos - start_pos).normalized;
            var net_manager = Singleton<NetManager>.instance;
            if (
                net_manager.CreateSegment(
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

                new_segment_id_list.Add(segment_id);

            } else {
                throw new Exception("Error creating segment");
            }

            return;
        }

        private void Release_segment(ushort segment_id){ // Release Segment == Remove Segment, 使用Release的原因是与Skyline代码统一
        var net_manager = Singleton<NetManager>.instance;
            net_manager.ReleaseSegment(segment_id, true); // keep node = true，只删segment，不删node

            var node_pair = Pair<ushort, ushort>.Make_sorted_pair(this.segment_dict[segment_id].First, this.segment_dict[segment_id].Second);
            this.segment_dict.Remove(segment_id);
            this.node_pair_to_segment_dict.Remove(node_pair);
        }

        private void Release_node(ushort node_id){
            var net_manager = Singleton<NetManager>.instance;
            net_manager.ReleaseNode(node_id);

            this.position_to_node_dict.Remove(this.node_dict[node_id]);
            this.node_dict.Remove(node_id);
        }

        private ushort Get_or_make_node(ref Vector3 node_pos, uint prefab_id, ref List<ushort> new_node_id_list) {
            node_pos.y = Singleton<TerrainManager>.instance.SampleRawHeightSmooth(new Vector3(node_pos.x, 0, node_pos.z)) + 1f;

            node_pos = this.Process_vector3(node_pos);

            if (this.position_to_node_dict.ContainsKey(node_pos)) {
                return this.position_to_node_dict[node_pos];
            }

            var net_manager = Singleton<NetManager>.instance;
            if (
                net_manager.CreateNode(
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

                new_node_id_list.Add(node_id);

                return node_id;
            } else {
                throw new Exception("Error creating node " + node_pos.x + ", " + node_pos.y + "at" + node_pos);
            }
        }

        private Vector3 Process_vector3(Vector3 input){
            return new Vector3(
                (int)(input.x * 100) / 100.0f,
                (int)(input.y * 100) / 100.0f,
                (int)(input.z * 100) / 100.0f
            );
        }
    }

}