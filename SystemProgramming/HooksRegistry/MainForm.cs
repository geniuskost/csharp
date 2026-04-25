using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SystemProgramming.HooksRegistry;

public class MainForm : Form
{
    private TextBox textBox;
    private Button toggleLoggingButton;
    private Button escapingButton;
    private HookManager hookManager;
    private bool isLogging = false;

    public MainForm()
    {
        Text = "Hooks Registry Tasks";
        Size = new Size(600, 400);

        textBox = new TextBox
        {
            Location = new Point(20, 20),
            Size = new Size(300, 200),
            Multiline = true
        };

        toggleLoggingButton = new Button
        {
            Text = "Start Logging",
            Location = new Point(20, 240),
            Size = new Size(150, 30)
        };
        toggleLoggingButton.Click += ToggleLoggingButton_Click;

        escapingButton = new Button
        {
            Text = "Catch me!",
            Location = new Point(400, 150),
            Size = new Size(100, 30)
        };

        Controls.Add(textBox);
        Controls.Add(toggleLoggingButton);
        Controls.Add(escapingButton);

        hookManager = new HookManager(this, escapingButton);

        FormClosing += (s, e) => hookManager.Dispose();
    }

    private void ToggleLoggingButton_Click(object? sender, EventArgs e)
    {
        isLogging = !isLogging;
        toggleLoggingButton.Text = isLogging ? "Stop Logging" : "Start Logging";
        hookManager.SetLogging(isLogging);
    }
}
