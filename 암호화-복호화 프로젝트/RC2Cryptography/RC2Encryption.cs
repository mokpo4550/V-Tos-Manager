﻿using System;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace RC2Cryptography
{
    public class RC2Encryption
    {
        public delegate void ProgressEvent(long num);
        public static event ProgressEvent runEvent = null;

        public static String Encrypt(String message, String key, String IV)
        {
            byte[] pbyteKey = Encoding.UTF8.GetBytes(key);
            byte[] pbyteIV = Encoding.UTF8.GetBytes(IV);
            String strReturn;

            if (pbyteKey.Length != 8)
            {
                throw (new Exception("Invalid key. Key length must be 8 byte."));
            }

            using (RC2CryptoServiceProvider rc2CSP = new RC2CryptoServiceProvider())
            {
                rc2CSP.Mode = CipherMode.CBC;
                rc2CSP.Padding = PaddingMode.PKCS7;
                rc2CSP.Key = pbyteKey;
                rc2CSP.IV = pbyteIV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cryptStream = new CryptoStream(ms, rc2CSP.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] data = Encoding.UTF8.GetBytes(message.ToCharArray());
                        cryptStream.Write(data, 0, data.Length);
                    }
                    strReturn = Convert.ToBase64String(ms.ToArray());
                }
            }

            return strReturn;
        }

        // RC2 알고리즘으로 암호화된 문자열을 받아서 복호화 한 후 암호화 이전의 원래 문자열을 Return 함
        public static String Decrypt(String message, String key, String IV)
        {
            byte[] pbyteKey = Encoding.UTF8.GetBytes(key);
            byte[] pbyteIV = Encoding.UTF8.GetBytes(IV);
            String strReturn;

            if (pbyteKey.Length != 8)
            {
                throw (new Exception("Invalid key. Key length must be 8 byte."));
            }

            using (RC2CryptoServiceProvider rc2CSP = new RC2CryptoServiceProvider())
            {
                rc2CSP.Mode = CipherMode.CBC;
                rc2CSP.Padding = PaddingMode.PKCS7;
                rc2CSP.Key = pbyteKey;
                rc2CSP.IV = pbyteIV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cryptStream = new CryptoStream(ms, rc2CSP.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        message = message.Replace(" ", "+");
                        byte[] data = Convert.FromBase64String(message);
                        cryptStream.Write(data, 0, data.Length);
                    }

                    strReturn = Encoding.UTF8.GetString(ms.GetBuffer()).Trim('\0');
                }
            }
            return strReturn;
        }

        public static void Encrypt(FileInfo readFileName, FileInfo writeFileName, String key, String IV)
        {
            long progress = 0;

            byte[] pbyteKey = Encoding.UTF8.GetBytes(key);
            byte[] pbyteIV = Encoding.UTF8.GetBytes(IV);

            if (pbyteKey.Length != 8)
            {
                throw (new Exception("Invalid key. Key length must be 8 byte."));
            }

            using (RC2CryptoServiceProvider rc2CSP = new RC2CryptoServiceProvider())
            {
                rc2CSP.Mode = CipherMode.CBC;
                rc2CSP.Padding = PaddingMode.PKCS7;
                rc2CSP.Key = pbyteKey;
                rc2CSP.IV = pbyteIV;

                using (FileStream fromFile = new FileStream(readFileName.FullName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    using (FileStream toFile = new FileStream(writeFileName.FullName, FileMode.Create, FileAccess.Write))
                    {
                        using (CryptoStream cryptScream = new CryptoStream(toFile, rc2CSP.CreateEncryptor(), CryptoStreamMode.Write))
                        {
                            byte[] buffer = new byte[1048576]; //1MB
                            int read;

                            while ((read = fromFile.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                cryptScream.Write(buffer, 0, read);
                                progress += read;
                                runEvent?.Invoke(progress);
                            }
                        }
                    }
                }
            }
        }

        public static void Decrypt(FileInfo readFileName, FileInfo writeFileName, String key, String IV)
        {
            long progress = 0;

            byte[] pbyteKey = Encoding.UTF8.GetBytes(key);
            byte[] pbyteIV = Encoding.UTF8.GetBytes(IV);

            if (pbyteKey.Length != 8)
            {
                throw (new Exception("Invalid key. Key length must be 8 byte."));
            }

            using (RC2CryptoServiceProvider rc2CSP = new RC2CryptoServiceProvider())
            {
                rc2CSP.Mode = CipherMode.CBC;
                rc2CSP.Padding = PaddingMode.PKCS7;
                rc2CSP.Key = pbyteKey;
                rc2CSP.IV = pbyteIV;

                using (FileStream fromFile = new FileStream(readFileName.FullName, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    using (FileStream toFile = new FileStream(writeFileName.FullName, FileMode.Create, FileAccess.Write))
                    {
                        using (CryptoStream cryptScream = new CryptoStream(fromFile, rc2CSP.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            byte[] buffer = new byte[1048576]; //1MB
                            int read;

                            while ((read = cryptScream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                toFile.Write(buffer, 0, read);
                                progress += read;
                                runEvent?.Invoke(progress);
                            }
                        }
                    }
                }
            }
        }
    }
}