using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace WinTAVRYNEXO.Services
{
    public class FileShredderService
    {
        public async Task<bool> ShredFileAsync(string filePath, int passes = 3)
        {
            return await Task.Run(() =>
            {
                if (!File.Exists(filePath)) return false;

                try
                {
                    var fileInfo = new FileInfo(filePath);
                    long length = fileInfo.Length;
                    File.SetAttributes(filePath, FileAttributes.Normal);

                    using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Write, FileShare.None))
                    {
                        byte[] buffer = new byte[4096];
                        for (int pass = 1; pass <= passes; pass++)
                        {
                            stream.Position = 0;
                            long sectors = length / buffer.Length;
                            int remainder = (int)(length % buffer.Length);

                            for (int i = 0; i < sectors; i++)
                            {
                                FillBuffer(buffer, pass);
                                stream.Write(buffer, 0, buffer.Length);
                            }

                            if (remainder > 0)
                            {
                                byte[] remBuffer = new byte[remainder];
                                FillBuffer(remBuffer, pass);
                                stream.Write(remBuffer, 0, remainder);
                            }
                            stream.Flush();
                        }
                    }

                    File.SetCreationTime(filePath, new DateTime(2000, 1, 1));
                    File.SetLastWriteTime(filePath, new DateTime(2000, 1, 1));
                    File.SetLastAccessTime(filePath, new DateTime(2000, 1, 1));
                    File.Delete(filePath);
                    return true;
                }
                catch { return false; }
            });
        }

        private void FillBuffer(byte[] buffer, int pass)
        {
            if (pass == 1) Array.Clear(buffer, 0, buffer.Length);
            else if (pass == 2) for (int i = 0; i < buffer.Length; i++) buffer[i] = 0xFF;
            else RandomNumberGenerator.Fill(buffer);
        }
    }
}
