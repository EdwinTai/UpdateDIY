using LibGit2Sharp;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;


namespace WpfGitUpdater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string LocalRepoPath = "UpdateRepo"; // 本地暫存目錄
        private const string RemoteRepoUrl = "https://github.com/EdwinTai/UpdateDIY.git"; // 遠端 Git 儲存庫
        private const string AppExecutable = "WpfGitUpdater.exe"; // 主要執行檔
        private const string UpdateBranch = "main"; // 用於更新的分支或標籤

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CheckForUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 初始化或更新本地 Git 儲存庫
                if (!Directory.Exists(LocalRepoPath))
                {
                    Repository.Clone(RemoteRepoUrl, LocalRepoPath);
                }
                else
                {
                    using var repo = new Repository(LocalRepoPath);
                    Commands.Fetch(repo, "origin", new[] { UpdateBranch }, null, null);
                }

                // 獲取遠端版本與本地版本
                using var localRepo = new Repository(LocalRepoPath);
                var localCommit = localRepo.Head.Tip.Sha;
                var remoteCommit = localRepo.Branches[$"origin/{UpdateBranch}"].Tip.Sha;

                if (localCommit == remoteCommit)
                {
                    MessageBox.Show("目前已是最新版本！");
                }
                else
                {
                    MessageBox.Show("發現新版本，開始更新...");
                    UpdateApplication();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"檢查更新時出錯：{ex.Message}");
            }
        }

        private void UpdateApplication()
        {
            try
            {
                // 拉取最新版本
                using var repo = new Repository(LocalRepoPath);
                Commands.Checkout(repo, repo.Branches[$"origin/{UpdateBranch}"]);

                // 替換應用程式文件
                var updateDir = Path.Combine(LocalRepoPath, "release");
                if (!Directory.Exists(updateDir))
                {
                    Directory.CreateDirectory(updateDir);
                    //throw new DirectoryNotFoundException("更新檔案資料夾不存在！");
                }

                // 複製文件到應用程式目錄
                var appDir = AppDomain.CurrentDomain.BaseDirectory;
                foreach (var file in Directory.GetFiles(updateDir))
                {
                    var fileName = Path.GetFileName(file);
                    var destFile = Path.Combine(appDir, fileName);

                    // 確保暫存更新
                    File.Copy(file, destFile + ".new", true);
                }

                // 設置更新計劃
                MessageBox.Show("更新完成，應用程式將重新啟動以應用更新！");
                RestartApplication();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更新時出錯：{ex.Message}");
            }
        }

        private void RestartApplication()
        {
            var appDir = AppDomain.CurrentDomain.BaseDirectory;
            var updater = Path.Combine(appDir, "Updater.exe");

            // 啟動 Updater 來完成替換和重啟
            Process.Start(new ProcessStartInfo
            {
                FileName = updater,
                Arguments = $"\"{AppExecutable}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            });

            Application.Current.Shutdown(); // 結束應用程式
        }

    }
}