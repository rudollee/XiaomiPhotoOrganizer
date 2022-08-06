using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace XiaomiPhotoOrganizerTest
{
    public partial class Form1 : Form
    {
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private readonly string _folderToWatch;
        private FileSystemWatcher _xiaomiWatcher = new FileSystemWatcher();
        private List<(string, string)> _filesToMove = new List<(string, string)>();
        private List<string> _filesX = new List<string>();
        public Form1()
        {
            InitializeComponent();

            _folderToWatch = Environment.GetEnvironmentVariable("Xiaomi");
            _xiaomiWatcher.Path = _folderToWatch;
            _xiaomiWatcher.IncludeSubdirectories = false;
            _xiaomiWatcher.Created += _xiaomiWatcher_Created;
            _xiaomiWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Security;
            _xiaomiWatcher.EnableRaisingEvents = true;
        }

        private void _xiaomiWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory)) return;

            var directory = $@"{_folderToWatch}\{e.Name.Substring(4, 8)}";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            //_filesToMove.Add((e.FullPath, $@"{directory}\{e.Name}"));
            //_filesX.Add(e.Name);
            while (this.Locked(e.FullPath))
            {
                Thread.Sleep(500);
            }
            File.Move(e.FullPath, $@"{directory}\{e.Name}");
        }

        private bool Locked(string filename)
        {
            FileStream stream = null;
            try
            {
                stream = File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                {
                    stream.Close();
                }
            }

            return false;
        }

    }
}
