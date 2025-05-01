using System.Windows.Forms;

namespace lab1_compiler.Bar
{
    internal class CorManager
    {
        private readonly RichTextBox _editor;
        private bool _isTyping = false;
        private bool _undoTrackingPaused;

        public void PauseUndoTracking() => _undoTrackingPaused = true;
        public void ResumeUndoTracking() => _undoTrackingPaused = false;
        public CorManager(RichTextBox editor)
        {
            _editor = editor;
            _editor.KeyPress += Editor_KeyPress;
        }

        private void Editor_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar))
            {
                // Сохраняем текущую позицию курсора и выделение
                int selectionStart = _editor.SelectionStart;
                int selectionLength = _editor.SelectionLength;

                // Принудительно завершаем текущую операцию ввода
                _isTyping = true;
                _editor.Undo();
                _editor.Redo();
                _isTyping = false;

                // Восстанавливаем позицию курсора
                _editor.SelectionStart = selectionStart;
                _editor.SelectionLength = selectionLength;

                // Прокрутка к курсору
                _editor.ScrollToCaret();
            }
        }

        public void Undo()
        {
            if (_undoTrackingPaused) return;
            if (_editor.CanUndo)
            {
                _editor.Undo();
                _editor.Redo(); // Добавляем Redo для компенсации автоматических изменений
                _editor.Undo(); // Корректная отмена
                _editor.ScrollToCaret();
            }
        }

        public void Redo()
        {
            if (_editor.CanRedo)
            {
                _editor.Redo();
                _editor.ScrollToCaret();
            }
        }

        public void Cut()
        {
            if (_editor.SelectionLength > 0)
                _editor.Cut();
        }

        public void Copy()
        {
            if (_editor.SelectionLength > 0)
                _editor.Copy();
        }

        public void Paste() => _editor.Paste();

        public void Delete()
        {
            if (_editor.SelectionLength > 0)
                _editor.SelectedText = string.Empty;
        }

        public void SelectAll() => _editor.SelectAll();
    }
}