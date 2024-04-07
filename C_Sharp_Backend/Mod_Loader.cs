using ColossalFramework.Plugins;
using ICities;
using System;
using UnityEngine;



namespace Emulator_Backend{

    public class Mod_Loader: LoadingExtensionBase{
        private readonly GameObject action_distributor_object = new GameObject("Action_Distributor_Object");

        public override void OnCreated(ILoading loading) {
            base.OnCreated(loading);

            this.action_distributor_object.AddComponent<Action_Distributor>();

            //{
            //    String str = "";
            //    for (uint i = 0; i < PrefabCollection<NetInfo>.PrefabCount(); ++i) {
            //        string name = PrefabCollection<NetInfo>.PrefabName(i);
            //        str += i + ":" + name + '\n';
            //    }
            //    Debug.Log(str);
            //}
            //{
            //    String str = "";
            //    for (uint i = 0; i < PrefabCollection<BuildingInfo>.PrefabCount(); ++i) {
            //        string name = PrefabCollection<BuildingInfo>.PrefabName(i);
            //        str += i + ":" + name + '\n';
            //    }
            //    Debug.Log(str);
            //}
            //{
            //    String str = "";
            //    for (uint i = 0; i < PrefabCollection<PropInfo>.PrefabCount(); ++i) {
            //        string name = PrefabCollection<PropInfo>.PrefabName(i);
            //        str += i + ":" + name + '\n';
            //    }
            //    Debug.Log(str);
            //}
        }

        public override void OnLevelLoaded(LoadMode mode) {}

        public override void OnReleased() {}
    }

}