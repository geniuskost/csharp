using System.Drawing;

namespace Practice;

internal sealed class MainForm : Form
{
    private readonly TextBox extensionTextBox;
    private readonly TextBox directoryTextBox;
    private readonly CheckBox subdirectoriesCheckBox;
    private readonly Button browseButton;
    private readonly Button searchButton;
    private readonly DataGridView resultsGrid;
    private readonly Label statusLabel;

    public MainForm()
    {
        Text = "Пошук файлів за розширенням";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(900, 620);
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        var titleLabel = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 8),
            Text = "Послідовний і паралельний пошук файлів за розширенням",
            Font = new Font(Font, FontStyle.Bold)
        };

        var instructionLabel = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(0, 0, 0, 12),
            ForeColor = Color.DimGray,
            Text = "Вкажіть розширення, шлях до каталогу та запустіть порівняння двох алгоритмів."
        };

        extensionTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Наприклад: txt або .txt"
        };

        directoryTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Шлях до каталогу"
        };

        browseButton = new Button
        {
            Text = "Огляд...",
            AutoSize = true,
            Padding = new Padding(12, 4, 12, 4)
        };

        subdirectoriesCheckBox = new CheckBox
        {
            Text = "Шукати у вкладених каталогах",
            AutoSize = true,
            Checked = true,
            Dock = DockStyle.Fill
        };

        searchButton = new Button
        {
            Text = "Порахувати",
            AutoSize = true,
            Padding = new Padding(12, 6, 12, 6),
            Anchor = AnchorStyles.Right
        };

        statusLabel = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ForeColor = Color.DimGray,
            Text = "Введіть параметри пошуку та натисніть кнопку \"Порахувати\"."
        };

        resultsGrid = new DataGridView
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.FixedSingle
        };

        resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "MethodColumn",
            HeaderText = "Метод",
            FillWeight = 35
        });

        resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "CountColumn",
            HeaderText = "Кількість файлів",
            FillWeight = 35
        });

        resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "TimeColumn",
            HeaderText = "Час, мс",
            FillWeight = 30
        });

        var inputsLayout = BuildInputsLayout();
        var resultsPanel = BuildResultsPanel();

        var mainLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16),
            ColumnCount = 1,
            RowCount = 4
        };

        mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        mainLayout.Controls.Add(titleLabel, 0, 0);
        mainLayout.Controls.Add(instructionLabel, 0, 1);
        mainLayout.Controls.Add(inputsLayout, 0, 2);
        mainLayout.Controls.Add(resultsPanel, 0, 3);

        Controls.Add(mainLayout);

        browseButton.Click += BrowseButton_Click;
        searchButton.Click += SearchButton_Click;
        directoryTextBox.KeyDown += OnInputKeyDown;
        extensionTextBox.KeyDown += OnInputKeyDown;
    }

    private TableLayoutPanel BuildInputsLayout()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 3,
            Padding = new Padding(12),
            Margin = new Padding(0, 0, 0, 12),
            BackColor = Color.WhiteSmoke
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 170));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(new Label
        {
            Text = "Розширення файлу",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 8, 0, 0)
        }, 0, 0);
        layout.Controls.Add(extensionTextBox, 1, 0);
        layout.SetColumnSpan(extensionTextBox, 2);

        layout.Controls.Add(new Label
        {
            Text = "Каталог",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 8, 0, 0)
        }, 0, 1);

        var directoryPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 36,
            Padding = new Padding(0, 0, 8, 0)
        };

        directoryTextBox.Dock = DockStyle.Fill;
        browseButton.Dock = DockStyle.Right;
        directoryPanel.Controls.Add(directoryTextBox);
        directoryPanel.Controls.Add(browseButton);

        layout.Controls.Add(directoryPanel, 1, 1);
        layout.SetColumnSpan(directoryPanel, 2);

        layout.Controls.Add(subdirectoriesCheckBox, 1, 2);
        layout.SetColumnSpan(subdirectoriesCheckBox, 2);

        return layout;
    }

    private Control BuildResultsPanel()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(0)
        };

        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        panel.Controls.Add(BuildActionsLayout(), 0, 0);
        panel.Controls.Add(resultsGrid, 0, 1);
        panel.Controls.Add(statusLabel, 0, 2);

        return panel;
    }

    private Control BuildActionsLayout()
    {
        var layout = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            Padding = new Padding(0, 0, 0, 8)
        };

        searchButton.Margin = new Padding(0);
        layout.Controls.Add(searchButton);
        return layout;
    }

    private void BrowseButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Виберіть каталог для пошуку файлів",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = false
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            directoryTextBox.Text = dialog.SelectedPath;
        }
    }

    private async void SearchButton_Click(object? sender, EventArgs e)
    {
        var extension = extensionTextBox.Text;
        var directory = directoryTextBox.Text.Trim();
        var includeSubdirectories = subdirectoriesCheckBox.Checked;

        if (string.IsNullOrWhiteSpace(extension))
        {
            SetStatus("Вкажіть розширення файлу.");
            return;
        }

        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            SetStatus("Вкажіть існуючий каталог.");
            return;
        }

        ToggleControls(false);
        SetStatus("Виконується пошук...");

        try
        {
            var sequentialResult = await Task.Run(() => FileSearchService.SearchSequential(directory, extension, includeSubdirectories));
            var parallelResult = await Task.Run(() => FileSearchService.SearchParallel(directory, extension, includeSubdirectories));

            ShowResults(sequentialResult, parallelResult);

            if (sequentialResult.Count == parallelResult.Count)
            {
                SetStatus($"Пошук завершено. Знайдено {sequentialResult.Count} файлів.");
            }
            else
            {
                SetStatus("Пошук завершено, але результати методів відрізняються.");
            }
        }
        catch (ArgumentException ex)
        {
            SetStatus(ex.Message);
        }
        catch (Exception ex)
        {
            SetStatus($"Помилка: {ex.Message}");
        }
        finally
        {
            ToggleControls(true);
        }
    }

    private void ShowResults(params SearchResult[] results)
    {
        resultsGrid.Rows.Clear();

        foreach (var result in results)
        {
            resultsGrid.Rows.Add(
                result.Mode,
                result.Count,
                result.Elapsed.TotalMilliseconds.ToString("N0"));
        }
    }

    private void ToggleControls(bool enabled)
    {
        extensionTextBox.Enabled = enabled;
        directoryTextBox.Enabled = enabled;
        browseButton.Enabled = enabled;
        subdirectoriesCheckBox.Enabled = enabled;
        searchButton.Enabled = enabled;
        UseWaitCursor = !enabled;
    }

    private void SetStatus(string message)
    {
        statusLabel.Text = message;
    }

    private void OnInputKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter)
        {
            e.SuppressKeyPress = true;
            searchButton.PerformClick();
        }
    }
}