using C_Sharp_Backend.Action.Zone;
using ICities;
using UnityEngine;



namespace Emulator_Backend
{
    public class Mod_Loader : LoadingExtensionBase
    {
        private readonly GameObject action_distributor_object = new GameObject("Action_Distributor_Object");

        public override void OnCreated(ILoading loading)
        {
            base.OnCreated(loading);

            this.action_distributor_object.AddComponent<Action_Distributor>();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            base.OnLevelLoaded(mode);

            if (mode == LoadMode.NewGame)
            {
                ZoneTool zoneTool = GameObject.FindObjectOfType<ZoneTool>();
                GameObject zoneToolParent = zoneTool.gameObject;

                if (zoneToolParent != null)
                {
                    Debug.Log("ModLoader >> Ready to append OZoneTool");
                    // GameObject.Destroy(zoneTool);
                    GameObject gameObject = new GameObject("OZoneTool");
                    gameObject.transform.SetParent(zoneToolParent.transform);
                    gameObject.AddComponent<OZoneTool>();
                }

            }
        }

        public override void OnReleased() { }
    }
}