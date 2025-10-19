using System;
using System.Windows.Automation;

namespace Aura
{
    public class TextCapture
    {
        public event Action<string> TextCaptured;

        public void TryCaptureSelectedText()
        {
            try
            {
                var focusedElement = AutomationElement.FocusedElement;
                if (focusedElement == null) return;

                if (focusedElement.TryGetCurrentPattern(TextPattern.Pattern, out var patternObject))
                {
                    var textPattern = (TextPattern)patternObject;
                    var selection = textPattern.GetSelection();
                    if (selection.Length > 0)
                    {
                        var selectedText = selection[0].GetText(-1).Trim();
                        if (!string.IsNullOrEmpty(selectedText))
                        {
                            TextCaptured?.Invoke(selectedText);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore exceptions, as UIA can be unreliable across different applications.
            }
        }
    }
}
