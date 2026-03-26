using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Automation;

public class ChromeWrapper
{
    public string GetChromeUrl(Process chrome)
    {
        if (chrome.MainWindowHandle == IntPtr.Zero) return null;

        AutomationElement element = AutomationElement.FromHandle(chrome.MainWindowHandle);
        Condition conditions = new AndCondition(
            new PropertyCondition(AutomationElement.ProcessIdProperty, chrome.Id),
            new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit));

        AutomationElement addressBar = element.FindFirst(TreeScope.Descendants, conditions);

        return ((ValuePattern)addressBar.GetCurrentPattern(ValuePattern.Pattern)).Current.Value as string;
    }
}