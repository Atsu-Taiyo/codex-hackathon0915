using Hackathon.Core;

namespace Hackathon.RoomOne
{
    public sealed partial class RoomOneController
    {
        public bool ReturnToMap()
        {
            if (playing || voiceBusy) return false;
            WorldNavigation.OpenMap();
            return true;
        }
    }
}
