using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Security.Cryptography;
using IDEAChipher;
using System.Xml.Serialization;
using System.Reflection;
using System.Text.RegularExpressions;
using ClientModel;

namespace AppExchangeServer
{
    public class Service : IAppExchange
    {

        private List<Session> clientSessions;
        private string sessionsFile = "sessions.xml";
        private int defaultClientId = 0;

        public Service()
        {
            //Deserialize(fromId);
            //if(clientSessions == null)
            //    clientSessions = new List<Session>();
        }

        //public MainWindow ServiceWindow
        //{
        //    get; set;
        //}

        public bool SendToServer(string encryptedMessage, int idFrom, string fileName, string containerName)
        {
            bool res = false;



            try
            {
                Session sender = FindSessionByClientId(idFrom);
                int idTo;

                string actualKey = ExtensionClass.ByteArrayToString(sender.ServerKey);
                string subActualKey = actualKey.Substring(0, 8);
                IdeaChipher idea = new IdeaChipher(subActualKey);


                //string encryptedMessageOld = encryptedMessage;

                if (idea.SurrogatePairsDetected(encryptedMessage))
                {
                    encryptedMessage = idea.DeleteAdditionalPartsFromSurrogetePairs(encryptedMessage);
                }

                string idRecipientStr = encryptedMessage.Substring(encryptedMessage.Length - 4, 4);
                encryptedMessage = encryptedMessage.Substring(0, encryptedMessage.Length - 4);
                idRecipientStr = idea.Decrypt(idRecipientStr);

                idTo = Convert.ToInt32(idRecipientStr);
                string buffer = idea.Decrypt(encryptedMessage);

                WriteToFile(buffer, fileName, containerName, idFrom, idTo);
                //WriteToFile(encryptedMessageOld, fileName + ".enc", containerName, idFrom, idTo);
                res = true;
            }
            catch (Exception ex)
            {
                res = false;
                throw ex;
            }

            return res;
        }

        public byte[] SyncKey(byte[] alicePublicKey, int idFrom)
        {
            byte[] bobPublicKey;
            byte[] bobKey;
            
            using (ECDiffieHellmanCng bob = new ECDiffieHellmanCng())
            {
                bob.KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash;
                bob.HashAlgorithm = CngAlgorithm.Sha256;
                bobPublicKey = bob.PublicKey.ToByteArray();
                CngKey alicePubKey = CngKey.Import(alicePublicKey, CngKeyBlobFormat.EccPublicBlob);
                bobKey = bob.DeriveKeyMaterial(alicePubKey);
            }

            Session currentClientSession = FindSessionByClientId(idFrom);
            if (currentClientSession != null)
            {
                //to change the private key for this client
                clientSessions.Remove(currentClientSession);
            }
            else
            {
            }

            clientSessions.Add(new Session(idFrom, bobKey));
            Serialize();

            return bobPublicKey;
        }

        private Session FindSessionByClientId(int id)
        {
            Deserialize(id);
            if (clientSessions == null) clientSessions = new List<Session>();

            var query = from a in clientSessions
                        where a.ClientId == id
                        select a;

            Session sender = null;
            try
            {
                sender = query.FirstOrDefault();
            }
            catch
            {
                //Just leave the null value 
            }
            return sender;
        }

        private void Serialize()
        {
            XmlSerializer ser = new XmlSerializer(typeof(List<Session>));
            FileStream fs = new FileStream(sessionsFile, FileMode.OpenOrCreate);
            ser.Serialize(fs, clientSessions);
            fs.Close();
        }

        private void Deserialize(int fromId)
        {
            try
            {
                sessionsFile = fromId + sessionsFile;
                using (FileStream fs = new FileStream(sessionsFile, FileMode.Open))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(List<Session>));
                    clientSessions = (List<Session>)ser.Deserialize(fs);
                }
            }
            catch (Exception ex)
            {

            }
        }

        public static Session DeserializeAndGetSession(int fromId, string path)
        {
            Session s = null;

            try
            {
                string sessionsFile1 = fromId + "sessions.xml";
                sessionsFile1 = System.IO.Path.Combine(path, sessionsFile1);

                using (FileStream fs = new FileStream(sessionsFile1, FileMode.Open))
                {
                    XmlSerializer ser = new XmlSerializer(typeof(List<Session>));
                    s = ((List<Session>)ser.Deserialize(fs))[0];
                }
            }
            catch (Exception ex)
            {

            }

            return s;
        }

        public void WriteToFile(string whToWrite, string fileName, string container, int id, int forId)
        {
            string pathString = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            pathString = System.IO.Path.Combine(pathString, forId + "-" + id);
            System.IO.Directory.CreateDirectory(pathString);
            pathString = System.IO.Path.Combine(pathString, container);
            System.IO.Directory.CreateDirectory(pathString);
            pathString = System.IO.Path.Combine(pathString, fileName);


            if (File.Exists(pathString))
            {
                File.Delete(pathString);
            }

            using (FileStream fs = new FileStream(pathString, FileMode.CreateNew))
            {
                StreamWriter sw = new StreamWriter(fs);
                sw.Write(whToWrite);
                sw.Close();
            }
        }

        public int GetCountAndNamesOfFiles(out string projectName, out string[] fileNames, int senderId, int forId)
        {
            int count;

            string pathString = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            pathString = System.IO.Path.Combine(pathString, forId + "-" + senderId);
            string[] directories = Directory.GetDirectories(pathString);
            string projectName_ = directories[0];
            string[] fileNames_ = Directory.GetFiles(projectName_);

            Session sender = FindSessionByClientId( forId);
            string actualKey = ExtensionClass.ByteArrayToString(sender.ServerKey);
            string subActualKey = actualKey.Substring(0, 8);
            IdeaChipher idea = new IdeaChipher(subActualKey);

            count = fileNames_.Length;
            projectName = idea.Encrypt(projectName_);
            fileNames = new string[fileNames_.Length];

            for (int i = 0; i < fileNames_.Length; i++)
            {
                fileNames[i] = idea.Encrypt(fileNames_[i]);
            }

            return count;
        }

        public string[] GetContents(string[] encryptedNames, int idFor)
        {
            string[] result = new string[encryptedNames.Length];

            Session sender = FindSessionByClientId(idFor);
            string actualKey = ExtensionClass.ByteArrayToString(sender.ServerKey);
            string subActualKey = actualKey.Substring(0, 8);
            IdeaChipher idea = new IdeaChipher(subActualKey);

            string[] decryptedNames = new string[encryptedNames.Length];
            for (int i = 0; i < encryptedNames.Length; i++)
            {
                decryptedNames[i] = idea.Decrypt(encryptedNames[i]);
            }

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = File.ReadAllText(decryptedNames[i]);
                result[i] = idea.Encrypt(result[i]);
            }

            return result;
        }

        public string SignIn(string login, string password)
        {
            string res = string.Empty;

            if (login.Equals(string.Empty) || password.Equals(string.Empty))
            {
                throw new ArgumentException("SignIn method. Empty parameters!");
            }
            Session sender = FindSessionByClientId(defaultClientId);

            string actualKey = ExtensionClass.ByteArrayToString(sender.ServerKey);
            string subActualKey = actualKey.Substring(0, 8);
            IdeaChipher idea = new IdeaChipher(subActualKey);

            if (idea.SurrogatePairsDetected(login))
            {
                login = idea.DeleteAdditionalPartsFromSurrogetePairs(login);
            }

            if (idea.SurrogatePairsDetected(password))
            {
                password = idea.DeleteAdditionalPartsFromSurrogetePairs(password);
            }

            login = idea.Decrypt(login);
            password = idea.Decrypt(password);
            //should be changed to custom deleting the spaces only at the right side
            login = login.Trim();
            password = password.Trim();

            int id = GetIdAndVerifyPas(login, password);

            res = id.ToString();
            res = idea.Encrypt(res);

            //ServiceWindow.LogTextBlock.Text += string.Format("Sign in id: {0} ", res);

            return res;
        }

        private int GetIdAndVerifyPas(string log, string pas)
        {
            int res = -1;

            try
            {
                UsersContext usersContext = new UsersContext();
                user user = usersContext.users.First(i => i.Login.Equals(log));

                if (user.UserPassword.Equals(pas))
                {
                    res = user.Id;
                }

            }
            catch (Exception ex)
            {
                //
            }

            return res;
        }

        public List<string> GetSenders(string recipientId)
        {
            List<string> result = new List<string>();

            string path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            string[] directories = Directory.GetDirectories(path);

            Regex regex = new Regex("\\D" + recipientId + "-");

            foreach (string str in directories)
            {
                if (regex.IsMatch(str))
                {
                    string buf = str.Substring(path.Length + 1);
                    int deficeIndex = buf.LastIndexOf('-');
                    buf = buf.Substring(deficeIndex + 1);
                    result.Add(buf);
                }
            }

            return result;
        }


        public List<IdLoginClient> GetAllClients(int id)
        {
            List<IdLoginClient> res = new List<IdLoginClient>();

            UsersContext usersContext = new UsersContext();
            //string strId = Convert.ToString(id.Id);
            var buffer = usersContext.users.Select(i => new { Id = i.Id, Login = i.Login }).ToList();

            foreach (var v in buffer)
            {
                res.Add(new IdLoginClient() { Id = Convert.ToString(v.Id), Login = v.Login });
            }

            //Encrypt the users data
            Session sender = FindSessionByClientId(id);
            string actualKey = ExtensionClass.ByteArrayToString(sender.ServerKey);
            string subActualKey = actualKey.Substring(0, 8);
            IdeaChipher idea = new IdeaChipher(subActualKey);

            for (int i = 0; i < res.Count; i++)
            {
                res[i].Id = idea.Encrypt(res[i].Id);
                res[i].Login = idea.Encrypt(res[i].Login);
            }

            return res;
        }
    }
}
