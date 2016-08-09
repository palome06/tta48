using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace Trench.Log
{
    public class LogES
    {
        /// <summary>
        /// Encrypt string with DES
        /// </summary>
        /// <param name="encryptedStr">plaintext</param>
        /// <param name="key">key(length 8 at most)</param>
        /// <param name="IV">initial vector(length 8 at most)</param>
        /// <returns>ciphertext</returns>
        public static string DESEncrypt(string encryptStr, string key, string IV)
        {
            // Set it to length of 8
            key = (key + "12345678").Substring(0, 8);
            IV = (IV + "12345678").Substring(0, 8);

            SymmetricAlgorithm sa = new DESCryptoServiceProvider()
            {
                Key = Encoding.UTF8.GetBytes(key),
                IV = Encoding.UTF8.GetBytes(IV)
            };
            ICryptoTransform ict = sa.CreateEncryptor();
            byte[] byt = Encoding.UTF8.GetBytes(encryptStr);

            string retVal = "";
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, ict, CryptoStreamMode.Write))
                {
                    cs.Write(byt, 0, byt.Length);
                    cs.FlushFinalBlock();        
                }
                retVal = Convert.ToBase64String(ms.ToArray());
            }

            // do some confusion
            System.Random ra = new Random();
            for (int i = 0; i < 8; i++)
            {
                int radNum = ra.Next(36);
                char radChr = Convert.ToChar(radNum + 65);// get a random character

                retVal = retVal.Substring(0, 2 * i + 1) + radChr.ToString() + retVal.Substring(2 * i + 1);
            }

            return retVal;
        }

        /// <summary>
        /// Decrypt string with DES
        /// </summary>
        /// <param name="encryptedValue">ciphertext</param>
        /// <param name="key">key(length 8 at most)</param>
        /// <param name="IV">initial vector(length 8 at most)</param>
        /// <returns>plaintext</returns>
        public static string DESDecrypt(string encryptedValue, string key, string IV)
        {
            // remove disturbs
            string tmp = encryptedValue;
            if (tmp.Length < 16)
            {
                return "";
            }

            for (int i = 0; i < 8; i++)
            {
                tmp = tmp.Substring(0, i + 1) + tmp.Substring(i + 2);
            }
            encryptedValue = tmp;

            // Set it to length of 8
            key = (key + "12345678").Substring(0, 8);
            IV = (IV + "12345678").Substring(0, 8);

            try
            {
                SymmetricAlgorithm sa = new DESCryptoServiceProvider()
                {
                    Key = Encoding.UTF8.GetBytes(key),
                    IV = Encoding.UTF8.GetBytes(IV)
                };
                ICryptoTransform ict = sa.CreateDecryptor();

                byte[] byt = Convert.FromBase64String(encryptedValue);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, ict, CryptoStreamMode.Write))
                    {
                        cs.Write(byt, 0, byt.Length);
                        cs.FlushFinalBlock();
                    }
                    return Encoding.UTF8.GetString(ms.ToArray());
                }
            }
            catch (Exception) { return ""; }
        }
    }
}
