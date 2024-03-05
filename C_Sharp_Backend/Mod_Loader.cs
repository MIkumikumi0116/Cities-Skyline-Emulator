using ICities;
using UnityEngine;



namespace Emulator_Backend{

    public class Mod_Loader: LoadingExtensionBase{
        private readonly GameObject action_distributor_object = new GameObject("Action_Distributor_Object");

        public override void OnCreated(ILoading loading) {
            this.action_distributor_object.AddComponent<Action_Distributor>();
        }

        public override void OnLevelLoaded(LoadMode mode) {}

        public override void OnReleased() {
            var action_distributor = this.action_distributor_object.GetComponent<Action_Distributor>();
            action_distributor.Stop();

            GameObject.Destroy(this.action_distributor_object);
        }
    }

}