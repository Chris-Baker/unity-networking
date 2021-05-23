namespace Framework.Networking
{
    public class NetworkIdManager
    {
        private uint _nextId;

        public NetworkIdManager()
        {
            _nextId = 0;
        }

        public uint NextId()
        {
            uint id = _nextId;
            _nextId += 1;
            return id;
        }
    }
}