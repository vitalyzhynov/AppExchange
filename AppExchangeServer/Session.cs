using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppExchangeServer
{

    [Serializable]
    public class Session
    {
        private int clientId;
        private byte[] serverPublicKey;
        private byte[] serverKey;
        private byte[] clientPublicKey;

        public Session()
        {

        }

        public Session(int clientId, byte[] serverKey)
        {
            this.clientId = clientId;
            this.serverKey = serverKey;
        }

        public int ClientId { get => clientId; set => clientId = value; }
        public byte[] ServerPublicKey { get => serverPublicKey; set => serverPublicKey = value; }
        public byte[] ClientPublicKey { get => clientPublicKey; set => clientPublicKey = value; }
        public byte[] ServerKey { get => serverKey; set => serverKey = value; }
    }
}
