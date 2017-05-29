using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Security.Cryptography;
using IDEAChipher;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using ClientModel;

namespace ClientWPF
{
    public class Client
    {
        private byte[] aliceKey;
        private int id; //int from file on the client machine
        private byte[] alicePublicKey;
        private Uri address = new Uri("http://localhost:4000/Service");
        IAppExchange proxy;
        private int defaultClientId = 0;

        public Client()
        {
            this.id = defaultClientId;
        }

        public Client(int id)
        {
            this.id = id;
        }

        public int Id { get { return id; } }

        public byte[] AlicePublicKey { get => alicePublicKey; }

        public bool SyncKeys()
        {
            bool result = false;

            if (proxy == null)
            {
                ChannelFactory<IAppExchange> channelFactory =
                        new ChannelFactory<IAppExchange>(new BasicHttpBinding(), new EndpointAddress(address));

                proxy = channelFactory.CreateChannel();
            }

            using (ECDiffieHellmanCng alice = new ECDiffieHellmanCng())
            {
                alice.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                alice.HashAlgorithm = CngAlgorithm.Sha256;
                alicePublicKey = alice.PublicKey.ToByteArray();
                byte[] bobPubKey = proxy.SyncKey(alicePublicKey, Id);
                aliceKey = alice.DeriveKeyMaterial(CngKey.Import(bobPubKey, CngKeyBlobFormat.EccPublicBlob));
                result = true;
            }

            return result;
        }

        public bool Send(int recipientId, string projectName)
        {
            bool result = false;

            if (aliceKey == null)
            {
                SyncKeys();
            }

            ChannelFactory<IAppExchange> channelFactory =
                    new ChannelFactory<IAppExchange>(new BasicHttpBinding(), new EndpointAddress(address));
            IAppExchange proxy = channelFactory.CreateChannel();

            String[] allFileNames = Directory.GetFiles(projectName);
            int indexOfProjNameStart = projectName.LastIndexOf('\\') + 1;
            string pureProjectName = projectName.Substring(indexOfProjNameStart, projectName.Length - indexOfProjNameStart);

            foreach (string fullFileName in allFileNames)
            {
                //pure name
                string fileName = fullFileName.Substring(projectName.Length + 1, fullFileName.Length - projectName.Length - 1 );
                string message = File.ReadAllText(fullFileName);
                //string actualKey = "ҕ潃謼䌀㿹处쾻⥑놠㯠☐䓻䵕욒";//ExtensionClass.ByteArrayToString(aliceKey);
                string actualKey = ExtensionClass.ByteArrayToString(aliceKey);
                string subActualKey = actualKey.Substring(0, 8);
                IdeaChipher idea = new IdeaChipher(subActualKey);
                string buffer = idea.Encrypt(message);
                string encryptedId = idea.Encrypt(recipientId.ToString());
                buffer += encryptedId;

                result = proxy.SendToServer(buffer, Id, fileName, pureProjectName);
            }

            return result;
        }

        public void WriteToFile(string whToWrite, string fileName, string userPath)
        {
            string pathString = System.IO.Path.Combine(userPath, fileName);
            FileInfo fi = new FileInfo(pathString);
            fi.Directory.Create();

            using (FileStream fs = fi.Create())//new FileStream(pathString, FileMode.Create))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(whToWrite);
                sw.Close();
            }

        }

        public bool Get(int senderId, string newDir)
        {
            bool result = false;

            if (aliceKey == null)
            {
                SyncKeys();
            }
            ChannelFactory<IAppExchange> channelFactory =
                  new ChannelFactory<IAppExchange>(new BasicHttpBinding(), new EndpointAddress(address));
            IAppExchange proxy = channelFactory.CreateChannel();

            string[] allFileNamesE, allFileNames, fileContents;
            string projectName;
            int count = proxy.GetCountAndNamesOfFiles(out projectName, out allFileNamesE, senderId, Id);

            //encrypting the two variables below:
            string actualKey = ExtensionClass.ByteArrayToString(aliceKey);
            string subActualKey = actualKey.Substring(0, 8);
            IdeaChipher idea = new IdeaChipher(subActualKey);
            projectName = idea.Decrypt(projectName);

            fileContents = proxy.GetContents(allFileNamesE, Id);

            allFileNames = new string[allFileNamesE.Length];
            for (int i = 0; i < allFileNamesE.Length; i++)
            {
                allFileNames[i] = idea.Decrypt(allFileNamesE[i]);
                fileContents[i] = idea.Decrypt(fileContents[i]);
            }

            //puring the names:
            int indexOfProjNameStart = projectName.LastIndexOf('\\') + 1;
            projectName = projectName.Substring(indexOfProjNameStart, projectName.Length - indexOfProjNameStart);

            for (int i = 0; i < allFileNames.Length; i++)
            {
                allFileNames[i] = allFileNames[i].Substring(indexOfProjNameStart, allFileNames[i].Length - indexOfProjNameStart);
            }

            //writing files to client directory
            for (int i = 0; i < allFileNames.Length; i++)
            {
                WriteToFile(fileContents[i], allFileNames[i], newDir);
            }

            return result;
        }

        public int SignIn(string log, string pas)
        {
            int res = -1;

            if (aliceKey == null)
            {
                SyncKeys();
            }

            ChannelFactory<IAppExchange> channelFactory =
                    new ChannelFactory<IAppExchange>(new BasicHttpBinding(), new EndpointAddress(address));
            IAppExchange proxy = channelFactory.CreateChannel();

            string actualKey = ExtensionClass.ByteArrayToString(aliceKey);
            string subActualKey = actualKey.Substring(0, 8);
            IdeaChipher idea = new IdeaChipher(subActualKey);
            log = idea.Encrypt(log);
            pas = idea.Encrypt(pas);

            string bufRes = proxy.SignIn(log, pas);
            bufRes = idea.Decrypt(bufRes);
            res = Convert.ToInt32(bufRes);

            return res;
        }

        public List<string> GetSenders()
        {
            ChannelFactory<IAppExchange> channelFactory =
                   new ChannelFactory<IAppExchange>(new BasicHttpBinding(), new EndpointAddress(address));
            IAppExchange proxy = channelFactory.CreateChannel();
            return proxy.GetSenders(Id.ToString());
        }

        public List<IdLoginClient> GetAllClients()
        {
            //Firstly is neccessary to sync keys and than to request the data
            if (aliceKey == null)
            {
                SyncKeys();
            }

            ChannelFactory<IAppExchange> channelFactory =
                   new ChannelFactory<IAppExchange>(new BasicHttpBinding(), new EndpointAddress(address));
            IAppExchange proxy = channelFactory.CreateChannel();
            List<IdLoginClient> result = proxy.GetAllClients(Id);

            //decryption
            string actualKey = ExtensionClass.ByteArrayToString(aliceKey);
            string subActualKey = actualKey.Substring(0, 8);
            IdeaChipher idea = new IdeaChipher(subActualKey);

            for(int i = 0; i < result.Count; i++)
            {
                //should be changed to custom deleting the spaces only at the right side
                result[i].Id = (idea.Decrypt(result[i].Id)).Trim();
                result[i].Login = (idea.Decrypt(result[i].Login)).Trim();
            }

            return result;
        }
    }
}
