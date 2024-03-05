using ICities;



namespace Emulator_Backend{

    public class Emulator_Backend_Mod: IUserMod{
        public string Name{
            get { return "Emulator Backend"; }
        }

        public string Description{
            get { return "C# side for emulator"; }
        }
    }

}