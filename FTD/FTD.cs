using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace FTD
{
    public static class FTD
    {
        private static Exception exception;
        private static string lasthash = String.Empty;
        static FTD()
        {

        }
        public static void SendFile(string file, Socket destination, string name, string directory)
        {
            byte[] filebyte = null;
            string extension = null;
            try
            {
                filebyte = File.ReadAllBytes(file);
                extension = Path.GetExtension(file);
            }
            catch(Exception ex)
            {
                exception = ex;
                return;
            }
            if ((filebyte == null) || (extension == null)) return;
            //byte bname = Convert.ToByte(name);
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            writer.Write(7848);
            writer.Write(name);
            writer.Write(extension);
            writer.Write(directory);
            writer.Write(filebyte.Length);
            writer.Write(filebyte);
            try { destination.Send(stream.GetBuffer()); }
            catch (SocketException e)
            {
                exception = e;
            }
            lasthash = MD5HASH.ComputeMD5Checksum(file);
            
        }

        public static void SetupReciveFile()
        {

        }


        public static void CheckHash(byte[] buffer, Socket sock)
        {
            MemoryStream stream = new MemoryStream(buffer);
            BinaryReader reader = new BinaryReader(stream);
            int IRC = reader.ReadInt32();
            string hash = reader.ReadString();
            if (!MD5HASH.CompareMD5Hash(lasthash, hash))
            {
                Console.WriteLine("Compare MD5 failed!");
            }
        }

        public static void AcceptFile(byte[] buffer, Socket sock)
        {
            byte[] buff;
            List <byte[]> list = new List<byte[]> { };
            list.Add(buffer);
            while (sock.Available > 0)
            {
                byte[] buffers = new byte[1024];
                sock.Receive(buffers);
                list.Add(buffers);
            }
            if (list[0].Length < 1) return;
            int size = 0;
            foreach (byte[] ln in list)
            {
                size += ln.Length;
            }
            buff = new byte[size];
            int lnlastindex = 0;
            foreach (byte[] ln in list)
            {
                Array.Copy(ln, 0, buff, lnlastindex, ln.Length);
                lnlastindex = ln.Length;
            }
            MemoryStream stream = new MemoryStream(buff);
            BinaryReader reader = new BinaryReader(stream);
            int IRC = reader.ReadInt32();
            string name = reader.ReadString();
            string extens = reader.ReadString();
            string dir = reader.ReadString();
            int fblen = reader.ReadInt32();
            byte[] app = reader.ReadBytes(fblen);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllBytes(dir + name + extens, app);
            }
            catch (Exception e)
            {
                exception = e;
            }
            MemoryStream strm = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(strm);
            writer.Write(7848);
            writer.Write(MD5HASH.ComputeMD5Checksum(dir + name + extens));
            sock.Send(strm.GetBuffer());
        }
        
        public static Exception GetException()
        {
            return exception;
        }

    }

    public static class MD5HASH
    {
        public static string ComputeMD5Checksum(string path)
        {
            FileStream fs = File.OpenRead(path);
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                byte[] checkSum = md5.ComputeHash(fileData);
                string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                fs.Close();
                return result;
            }
        }
        
        public static bool CompareMD5Hash(string hash1, string hash2)
        {
            if (String.Compare(hash1.ToLower(), hash2.ToLower()) == 0) return true;
            else return false;
        }


    }

}
