using Microsoft.Extensions.Configuration;
using NETCore.Encrypt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace IltscheduleMailReminderSchedule.Helper
{
    public static class Security
    {
        public static IConfigurationRoot Configuration { get; set; }

        static Security()
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json");

            Configuration = builder.Build();
        }
        public static string Encrypt(this string encrypt)
        {
            try
            {
                var aesKey = EncryptProvider.CreateAesKey();
                var key = Configuration["EncryptionKey"];
                string decrypted= EncryptProvider.AESEncrypt(encrypt, key);
                return decrypted;
            }
            catch (Exception)
            {
                return encrypt;
            }
        }
        public static string Decrypt(this string decrypt)
        {
            try
            {
                var aesKey = EncryptProvider.CreateAesKey();
                var key = Configuration["EncryptionKey"];
                return EncryptProvider.AESDecrypt(decrypt, key);
            }
            catch (Exception)
            {
                return decrypt;
            }
        }
        public static string EncryptSHA512(this string encrypt)
        {
            try
            {
                return EncryptProvider.Sha512(encrypt);
            }
            catch (Exception)
            {
                return encrypt;
            }
        }



    }
}
