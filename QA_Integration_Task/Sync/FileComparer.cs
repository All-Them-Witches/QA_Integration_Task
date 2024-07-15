using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Sync
{
    public static class FileComparer
    {
        public static bool FilesAreEqual(string file1, string file2)
        {
            using (var hashAlgorithm = MD5.Create())
            {
                var hash1 = GetFileHash(file1, hashAlgorithm);
                var hash2 = GetFileHash(file2, hashAlgorithm);

                return hash1.SequenceEqual(hash2);
            }
        }

        private static byte[] GetFileHash(string filePath, HashAlgorithm hashAlgorithm)
        {
            using (var stream = File.OpenRead(filePath))
            {
                return hashAlgorithm.ComputeHash(stream);
            }
        }
    }
}
