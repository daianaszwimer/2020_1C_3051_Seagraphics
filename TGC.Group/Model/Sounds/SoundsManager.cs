
using Microsoft.DirectX.DirectSound;

namespace TGC.Group.Model.Sounds
{
    class SoundsManager
    {
        public Device sound { get; set; }
        public string mediaDir { get; set; }
        private static SoundsManager _instance;

        protected SoundsManager()
        {
        }

        public static SoundsManager Instance()
        {
            if (_instance == null)
            {
                _instance = new SoundsManager();
            }

            return _instance;
        }
    }
}
