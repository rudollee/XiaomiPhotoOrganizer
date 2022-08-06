using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XiaomiPhotoOrganizer
{
    public partial class ServiceDefault : ServiceBase
    {
        private readonly string _folderToWatch;
        private FileSystemWatcher _xiaomiWatcher = new FileSystemWatcher();
        public ServiceDefault()
        {
            InitializeComponent();

            _folderToWatch = Environment.GetEnvironmentVariable("Xiaomi");
            _xiaomiWatcher.Path = _folderToWatch;
            _xiaomiWatcher.IncludeSubdirectories = false;
            _xiaomiWatcher.Created += _xiaomiWatcher_Created;
            _xiaomiWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Security;

        }

        private void _xiaomiWatcher_Created(object sender, FileSystemEventArgs e)
        {
            if (File.GetAttributes(e.FullPath).HasFlag(FileAttributes.Directory)) return;

            var directory = $@"{_folderToWatch}\{e.Name.Substring(4, 8)}";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
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

        protected override void OnStart(string[] args)
        {
            _xiaomiWatcher.EnableRaisingEvents = true;
        }

        protected override void OnStop()
        {
            _xiaomiWatcher.EnableRaisingEvents = false;
        }
    }
}
