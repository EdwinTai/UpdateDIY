using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Updater
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("必須指定要更新的應用程式名稱。");
                return;
            }

            string appExecutable = args[0];
            string appDir = AppDomain.CurrentDomain.BaseDirectory;

            try
            {
                // 確保原應用程式關閉
                Thread.Sleep(2000);

                // 替換更新文件
                var newFile = Path.Combine(appDir, appExecutable + ".new");
                var targetFile = Path.Combine(appDir, appExecutable);

                if (File.Exists(newFile))
                {
                    if (File.Exists(targetFile))
                    {
                        File.Delete(targetFile);
                    }

                    File.Move(newFile, targetFile);
                }

                // 重新啟動應用程式
                Process.Start(targetFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"更新時出錯：{ex.Message}");
            }
        }
    }
}
