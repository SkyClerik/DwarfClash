using System;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ButtonAttribute : Attribute
{
    public string Text { get; private set; }

    public ButtonAttribute(string text = null)
    {
        Text = text;
    }
}