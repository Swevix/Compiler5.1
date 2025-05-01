using System;
using System.IO;
using System.Windows.Forms;

namespace lab1_compiler.Bar
{
    internal class FileManager
    {
        private string? _currentFilePath;
        private string _fileContent = string.Empty;
        private readonly Compiler _mainForm;


        public string? CurrentFilePath
        {
            get => _currentFilePath;
            private set
            {
                _currentFilePath = value;
                UpdateWindowTitle();
            }
        }

        public bool IsFileModified { get; private set; }

        public FileManager(Compiler mainForm)
        {
            _mainForm = mainForm;
        }

        public void UpdateFileContent(string content)
        {
            _fileContent = content;
            IsFileModified = true;
        }

        public void CreateNewFile()
        {
            if (CheckUnsavedChanges()) return;

            _fileContent = string.Empty;
            CurrentFilePath = null;
            IsFileModified = true;
            UpdateEditorContent();
        }

        public void OpenFile()
        {
            if (CheckUnsavedChanges()) return;

            using var openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog() == DialogResult.OK)
            {
                CurrentFilePath = openDialog.FileName;
                _fileContent = System.IO.File.ReadAllText(CurrentFilePath);
                IsFileModified = false;
                UpdateEditorContent();
            }
        }

        public void SaveFile()
        {
            if (string.IsNullOrEmpty(CurrentFilePath))
            {
                SaveAsFile();
                return;
            }

            _fileContent = _mainForm.GetCurrentContent();
            System.IO.File.WriteAllText(CurrentFilePath, _fileContent);
            IsFileModified = false;
            UpdateWindowTitle();
        }

        public void SaveAsFile()
        {
            using var saveDialog = new SaveFileDialog();
            saveDialog.Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                CurrentFilePath = saveDialog.FileName;
                System.IO.File.WriteAllText(CurrentFilePath, _fileContent);
                IsFileModified = false;
                UpdateWindowTitle();
            }
        }

        public void Exit()
        {
            if (!CheckUnsavedChanges())
                Application.Exit();
        }

        private bool CheckUnsavedChanges()
        {
            if (!IsFileModified) return false;

            var result = MessageBox.Show(
                "Сохранить изменения в текущем файле?",
                "Несохраненные изменения",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning
            );

            switch (result)
            {
                case DialogResult.Yes:
                    SaveFile();
                    return false;
                case DialogResult.Cancel:
                    return true;
                default:
                    return false;
            }
        }

        private void UpdateEditorContent()
        {
            _mainForm?.UpdateRichTextBox(_fileContent);
            UpdateWindowTitle();
        }

        public void UpdateWindowTitle()
        {
            _mainForm?.UpdateWindowTitle();
        }

        public void SetFileModified(bool modified)
        {
            IsFileModified = modified;
            UpdateWindowTitle();
        }

        public void DragFile(string filePath)
        {
            if (CheckUnsavedChanges()) return;

            CurrentFilePath = filePath;
            _fileContent = File.ReadAllText(filePath); // Читаем как обычный текст
            IsFileModified = false;
            UpdateEditorContent();
        }
    }
}