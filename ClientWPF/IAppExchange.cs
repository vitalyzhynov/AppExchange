using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace ClientWPF
{
    [ServiceContract]
    public interface IAppExchange
    {
        [OperationContract]
        byte[] SyncKey(byte[] alicePublicKey, int idFrom);
        [OperationContract]
        bool SendToServer(string encryptedMessage, int senderId, string fileName, string container);
        [OperationContract]
        int GetCountAndNamesOfFiles(out string pureProjectName, out string[] fileNames, int senderId, int forId);
        [OperationContract]
        string[] GetContents(string[] allFileNamesE, int Id);
        [OperationContract]
        string SignIn(string login, string password);
        [OperationContract]
        List<string> GetSenders(string recipientId);
        [OperationContract]
        List<ClientModel.IdLoginClient> GetAllClients(int id);
    }
}
