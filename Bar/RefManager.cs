using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace lab1_compiler.Bar
{
    internal class RefManager
    {
        private readonly string _helpFilePath;
        private readonly string _aboutFilePath;

        public RefManager(string helpPath, string aboutPath)
        {
            _helpFilePath = helpPath;
            _aboutFilePath = aboutPath;
        }

        public void ShowHelp()
        {
            OpenHtmlFile(_helpFilePath, "Справка");
        }

        public void ShowAbout()
        {
            OpenHtmlFile(_aboutFilePath, "О программе");
        }

        private void OpenHtmlFile(string filePath, string title)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    MessageBox.Show($"Файл {Path.GetFileName(filePath)} не найден!",
                                  "Ошибка",
                                  MessageBoxButtons.OK,
                                  MessageBoxIcon.Error);
                    return;
                }

                var processStartInfo = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                Process.Start(processStartInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}",
                              title,
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
            }
        }
    }
}