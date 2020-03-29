using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Confuser.Runtime
{
    internal static class MD5
    {
        static void Initialize()
        {
            var bas = new StreamReader(typeof(MD5).Assembly.Location).BaseStream;
            var file = new BinaryReader(bas);
            var file2 = File.ReadAllBytes(typeof(MD5).Assembly.Location);
            byte[] byt = file.ReadBytes(file2.Length - 32);
            var a = Hash(byt);
            file.BaseStream.Position = file.BaseStream.Length - 32;
            string b = Encoding.ASCII.GetString(file.ReadBytes(32));

            if (a != b)
            {
                MessageBox.Show("You probably know what this mean, if not, i'll tell u, you modified the app so DarksProtector is mad :)", "DarksProtector - dark#5000", MessageBoxButtons.OK, MessageBoxIcon.Error);

                Process.GetCurrentProcess().Kill();
            }
        }

        static string Hash(byte[] hash)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] btr = hash;
            btr = md5.ComputeHash(btr);
            StringBuilder sb = new StringBuilder();

            foreach (byte ba in btr)
            {
                sb.Append(ba.ToString("x2").ToLower());
            }
            return sb.ToString();
        }
    }
}
