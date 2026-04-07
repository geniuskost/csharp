using System.Drawing;
using System.Windows.Forms;

namespace Hooks;

internal sealed class MainForm : Form
{
    private readonly Label visibilityValueLabel;
    private readonly Label hookStatusValueLabel;
    private readonly Label altStatusValueLabel;
    private readonly Button toggleButton;
    private readonly Button exitButton;
    private HookManager? hookManager;

    public MainForm()
    {
        Text = "Hooks";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(860, 520);
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        BackColor = Color.FromArgb(18, 23, 34);
        ForeColor = Color.Gainsboro;

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
            ColumnCount = 1,
            RowCount = 5,
            BackColor = BackColor
        };

        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var titleLabel = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            Text = "Global Hooks Demo",
            Font = new Font("Segoe UI Semibold", 24F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Color.White
        };

        var subtitleLabel = new Label
        {
            AutoSize = true,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 8, 0, 18),
            MaximumSize = new Size(760, 0),
            Text = "Ctrl + Shift + Q миттєво ховає або показує вікно. Якщо утримувати Alt, курсор не зможе вийти за межі невидимого квадрата 500x500 по центру екрана.",
            ForeColor = Color.FromArgb(191, 198, 212)
        };

        var cardsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = BackColor,
            Margin = new Padding(0)
        };

        cardsPanel.Controls.Add(CreateCard(
            "Task 1",
            "Секретна комбінація клавіш",
            "Натисніть Ctrl + Shift + Q у будь-якому вікні. Форма зникає через Visible = false і повертається повторним натисканням тієї ж комбінації."));

        cardsPanel.Controls.Add(CreateCard(
            "Task 2",
            "Обмеження курсора",
            "Утримуйте Alt, щоб курсор залишався всередині невидимого квадрата 500x500 по центру екрана. Коли Alt відпущено, миша працює у звичайному режимі."));

        var statusPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 3,
            Margin = new Padding(0),
            Padding = new Padding(0, 18, 0, 0),
            BackColor = BackColor
        };

        statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        statusPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        statusPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statusPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        statusPanel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        visibilityValueLabel = CreateStatusValueLabel();
        hookStatusValueLabel = CreateStatusValueLabel();
        altStatusValueLabel = CreateStatusValueLabel();

        statusPanel.Controls.Add(CreateStatusBlock("Вікно", visibilityValueLabel), 0, 0);
        statusPanel.Controls.Add(CreateStatusBlock("Хуки", hookStatusValueLabel), 1, 0);
        statusPanel.Controls.Add(CreateStatusBlock("Alt lock", altStatusValueLabel), 0, 1);
        statusPanel.SetColumnSpan(statusPanel.Controls[2], 2);

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Margin = new Padding(0, 20, 0, 0),
            BackColor = BackColor
        };

        toggleButton = CreateButton("Hide / Show");
        toggleButton.Click += (_, _) => ToggleVisibilityFromHook();

        exitButton = CreateButton("Exit");
        exitButton.Click += (_, _) => Close();

        buttonsPanel.Controls.Add(exitButton);
        buttonsPanel.Controls.Add(toggleButton);

        root.Controls.Add(titleLabel, 0, 0);
        root.Controls.Add(subtitleLabel, 0, 1);
        root.Controls.Add(cardsPanel, 0, 2);
        root.Controls.Add(statusPanel, 0, 3);
        root.Controls.Add(buttonsPanel, 0, 4);

        Controls.Add(root);

        Shown += MainForm_Shown;
        FormClosing += MainForm_FormClosing;

        UpdateVisibilityStatus();
        UpdateHookStatus("Hooks are not installed yet.");
        UpdateAltStatus("Release Alt to use the mouse normally.");
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        if (hookManager is not null)
        {
            return;
        }

        try
        {
            hookManager = new HookManager(ToggleVisibilityFromHook, BuildLockArea());
            UpdateHookStatus("Hooks installed. Ctrl + Shift + Q is active.");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Hook installation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Close();
        }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        hookManager?.Dispose();
        hookManager = null;
    }

    private Panel CreateCard(string badge, string title, string description)
    {
        var card = new Panel
        {
            Width = 780,
            Height = 155,
            Margin = new Padding(0, 0, 0, 16),
            Padding = new Padding(18),
            BackColor = Color.FromArgb(26, 32, 46)
        };

        var badgeLabel = new Label
        {
            AutoSize = true,
            Text = badge.ToUpperInvariant(),
            ForeColor = Color.FromArgb(124, 200, 255),
            Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point)
        };

        var titleLabel = new Label
        {
            AutoSize = true,
            Text = title,
            Top = 26,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold, GraphicsUnit.Point)
        };

        var descriptionLabel = new Label
        {
            AutoSize = false,
            Width = 740,
            Height = 70,
            Top = 62,
            Text = description,
            ForeColor = Color.FromArgb(200, 208, 221),
            MaximumSize = new Size(740, 0)
        };

        card.Controls.Add(badgeLabel);
        card.Controls.Add(titleLabel);
        card.Controls.Add(descriptionLabel);

        return card;
    }

    private Label CreateStatusValueLabel()
    {
        return new Label
        {
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold, GraphicsUnit.Point),
            Margin = new Padding(0, 6, 0, 0)
        };
    }

    private Panel CreateStatusBlock(string title, Label valueLabel)
    {
        var panel = new Panel
        {
            Dock = DockStyle.Top,
            Height = 68,
            Padding = new Padding(0, 0, 16, 0),
            Margin = new Padding(0),
            BackColor = BackColor
        };

        var titleLabel = new Label
        {
            AutoSize = true,
            Text = title,
            ForeColor = Color.FromArgb(124, 133, 155),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point)
        };

        valueLabel.Top = 22;

        panel.Controls.Add(titleLabel);
        panel.Controls.Add(valueLabel);

        return panel;
    }

    private Button CreateButton(string text)
    {
        return new Button
        {
            Text = text,
            AutoSize = true,
            Height = 38,
            MinimumSize = new Size(120, 38),
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(41, 130, 255),
            ForeColor = Color.White,
            Margin = new Padding(0, 0, 0, 0)
        };
    }

    private Rectangle BuildLockArea()
    {
        var screenBounds = Screen.PrimaryScreen?.Bounds ?? SystemInformation.VirtualScreen;
        const int areaWidth = 500;
        const int areaHeight = 500;

        var left = screenBounds.Left + (screenBounds.Width - areaWidth) / 2;
        var top = screenBounds.Top + (screenBounds.Height - areaHeight) / 2;

        return new Rectangle(left, top, areaWidth, areaHeight);
    }

    private void ToggleVisibilityFromHook()
    {
        if (InvokeRequired)
        {
            BeginInvoke(new Action(ToggleVisibilityFromHook));
            return;
        }

        if (Visible)
        {
            HideWindow();
        }
        else
        {
            ShowWindow();
        }
    }

    private void HideWindow()
    {
        ShowInTaskbar = false;
        Visible = false;
        UpdateVisibilityStatus();
    }

    private void ShowWindow()
    {
        ShowInTaskbar = true;
        if (!Visible)
        {
            Visible = true;
        }

        WindowState = FormWindowState.Normal;
        Activate();
        BringToFront();
        UpdateVisibilityStatus();
    }

    private void UpdateVisibilityStatus()
    {
        visibilityValueLabel.Text = Visible ? "Visible" : "Hidden";
        visibilityValueLabel.ForeColor = Visible ? Color.FromArgb(110, 231, 183) : Color.FromArgb(255, 180, 110);
        toggleButton.Text = Visible ? "Hide" : "Show";
    }

    private void UpdateHookStatus(string text)
    {
        hookStatusValueLabel.Text = text;
    }

    private void UpdateAltStatus(string text)
    {
        altStatusValueLabel.Text = text;
    }
}