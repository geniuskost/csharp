using System.Drawing;

namespace ForbiddenWordsFinder;

internal sealed class MainForm : Form
{
    private readonly ForbiddenWordScanner scanner = new();
    private readonly PauseController pauseController = new();
    private readonly DataGridView resultsGrid;
    private readonly TextBox wordsTextBox;
    private readonly TextBox outputTextBox;
    private readonly TextBox logTextBox;
    private readonly Label statusLabel;
    private readonly ProgressBar driveProgressBar;
    private readonly ProgressBar fileProgressBar;
    private readonly Button startButton;
    private readonly Button pauseButton;
    private readonly Button resumeButton;
    private readonly Button stopButton;
    private readonly Button loadWordsButton;
    private readonly Button browseOutputButton;
    private readonly Button clearButton;
    private CancellationTokenSource? cancellationTokenSource;
    private bool scanRunning;

    public MainForm() : this(null)
    {
    }

    public MainForm(ApplicationOptions? initialOptions)
    {
        Text = "Пошук заборонених слів у файлах";
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(1200, 820);
        Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);

        var titleLabel = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Font = new Font(Font, FontStyle.Bold),
            Padding = new Padding(0, 0, 0, 4),
            Text = "Пошук, копіювання та маскування заборонених слів"
        };

        var descriptionLabel = new Label
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ForeColor = Color.DimGray,
            Padding = new Padding(0, 0, 0, 12),
            Text = "Введіть слова вручну або завантажте їх з файлу, виберіть папку для результатів та запустіть пошук по всіх доступних дисках."
        };

        wordsTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ScrollBars = ScrollBars.Vertical,
            Height = 96,
            PlaceholderText = "Слова через кому, крапку з комою або з нового рядка"
        };

        loadWordsButton = new Button
        {
            Text = "Завантажити з файлу...",
            AutoSize = true,
            Padding = new Padding(12, 6, 12, 6)
        };

        outputTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            PlaceholderText = "Папка для копій та звіту"
        };

        browseOutputButton = new Button
        {
            Text = "Огляд...",
            AutoSize = true,
            Padding = new Padding(12, 6, 12, 6)
        };

        startButton = new Button
        {
            Text = "Старт",
            AutoSize = true,
            Padding = new Padding(16, 8, 16, 8)
        };

        pauseButton = new Button
        {
            Text = "Пауза",
            AutoSize = true,
            Padding = new Padding(16, 8, 16, 8),
            Enabled = false
        };

        resumeButton = new Button
        {
            Text = "Продовжити",
            AutoSize = true,
            Padding = new Padding(16, 8, 16, 8),
            Enabled = false
        };

        stopButton = new Button
        {
            Text = "Зупинити",
            AutoSize = true,
            Padding = new Padding(16, 8, 16, 8),
            Enabled = false
        };

        clearButton = new Button
        {
            Text = "Очистити результати",
            AutoSize = true,
            Padding = new Padding(12, 6, 12, 6)
        };

        driveProgressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = 1
        };

        fileProgressBar = new ProgressBar
        {
            Dock = DockStyle.Fill,
            Minimum = 0,
            Maximum = 1
        };

        statusLabel = new Label
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ForeColor = Color.DimGray,
            Text = "Готово до запуску."
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
            Name = "SourceColumn",
            HeaderText = "Файл-оригінал",
            FillWeight = 45
        });

        resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "SizeColumn",
            HeaderText = "Розмір, байт",
            FillWeight = 12
        });

        resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "ReplacementsColumn",
            HeaderText = "Замін",
            FillWeight = 10
        });

        resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "OriginalCopyColumn",
            HeaderText = "Копія оригіналу",
            FillWeight = 16
        });

        resultsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "RedactedCopyColumn",
            HeaderText = "Маскована копія",
            FillWeight = 17
        });

        logTextBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Vertical,
            BackColor = SystemColors.Window,
            PlaceholderText = "Тут буде журнал роботи програми"
        };

        var settingsPanel = BuildSettingsPanel();
        var progressPanel = BuildProgressPanel();
        var contentPanel = BuildContentPanel();

        var rootLayout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(16)
        };

        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        rootLayout.Controls.Add(titleLabel, 0, 0);
        rootLayout.Controls.Add(descriptionLabel, 0, 1);
        rootLayout.Controls.Add(settingsPanel, 0, 2);
        rootLayout.Controls.Add(progressPanel, 0, 3);
        rootLayout.Controls.Add(contentPanel, 0, 4);

        Controls.Add(rootLayout);

        loadWordsButton.Click += LoadWordsButton_Click;
        browseOutputButton.Click += BrowseOutputButton_Click;
        startButton.Click += async (_, _) => await StartScanAsync();
        pauseButton.Click += (_, _) => PauseScan();
        resumeButton.Click += (_, _) => ResumeScan();
        stopButton.Click += (_, _) => StopScan();
        clearButton.Click += (_, _) => ClearResults();
        FormClosing += MainForm_FormClosing;

        outputTextBox.Text = initialOptions?.OutputDirectory ?? Path.Combine(Environment.CurrentDirectory, "ForbiddenWordsOutput");
        if (initialOptions?.ForbiddenWords.Count > 0)
        {
            wordsTextBox.Text = string.Join(Environment.NewLine, initialOptions.ForbiddenWords);
        }
    }

    private Control BuildSettingsPanel()
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

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(new Label
        {
            Text = "Заборонені слова",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 8, 0, 0)
        }, 0, 0);
        layout.Controls.Add(wordsTextBox, 1, 0);
        layout.SetColumnSpan(wordsTextBox, 2);

        layout.Controls.Add(new Label
        {
            Text = "Папка результатів",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 8, 0, 0)
        }, 0, 1);

        var outputPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 36,
            Padding = new Padding(0, 0, 8, 0)
        };

        outputTextBox.Dock = DockStyle.Fill;
        browseOutputButton.Dock = DockStyle.Right;
        outputPanel.Controls.Add(outputTextBox);
        outputPanel.Controls.Add(browseOutputButton);

        layout.Controls.Add(outputPanel, 1, 1);
        layout.SetColumnSpan(outputPanel, 2);

        layout.Controls.Add(new Label
        {
            Text = "Керування",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 8, 0, 0)
        }, 0, 2);

        var buttonsPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };

        buttonsPanel.Controls.Add(startButton);
        buttonsPanel.Controls.Add(pauseButton);
        buttonsPanel.Controls.Add(resumeButton);
        buttonsPanel.Controls.Add(stopButton);
        buttonsPanel.Controls.Add(loadWordsButton);
        buttonsPanel.Controls.Add(clearButton);

        layout.Controls.Add(buttonsPanel, 1, 2);
        layout.SetColumnSpan(buttonsPanel, 2);

        return layout;
    }

    private Control BuildProgressPanel()
    {
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            RowCount = 3,
            Padding = new Padding(0, 0, 0, 10)
        };

        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        layout.Controls.Add(new Label
        {
            Text = "Прогрес по дисках",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 4, 0, 0)
        }, 0, 0);
        layout.Controls.Add(driveProgressBar, 1, 0);

        layout.Controls.Add(new Label
        {
            Text = "Прогрес у поточному диску",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Padding = new Padding(0, 4, 0, 0)
        }, 0, 1);
        layout.Controls.Add(fileProgressBar, 1, 1);

        layout.Controls.Add(statusLabel, 1, 2);
        layout.SetColumnSpan(statusLabel, 2);

        return layout;
    }

    private Control BuildContentPanel()
    {
        var splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 360,
            Panel1MinSize = 240,
            Panel2MinSize = 180
        };

        splitContainer.Panel1.Controls.Add(resultsGrid);
        splitContainer.Panel2.Controls.Add(logTextBox);

        return splitContainer;
    }

    private void LoadWordsButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Виберіть файл зі списком заборонених слів",
            Filter = "Текстові файли (*.txt)|*.txt|Усі файли (*.*)|*.*"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            wordsTextBox.Text = string.Join(Environment.NewLine, File.ReadAllLines(dialog.FileName));
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BrowseOutputButton_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Виберіть папку для результатів",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            outputTextBox.Text = dialog.SelectedPath;
        }
    }

    private async Task StartScanAsync()
    {
        if (scanRunning)
        {
            return;
        }

        var forbiddenWords = ParseWords(wordsTextBox.Text);
        if (forbiddenWords.Count == 0)
        {
            MessageBox.Show(this, "Вкажіть хоча б одне заборонене слово.", "Потрібні дані", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var outputDirectory = outputTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            MessageBox.Show(this, "Вкажіть папку для результатів.", "Потрібні дані", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        Directory.CreateDirectory(outputDirectory);

        ClearResults();
        SetScanState(true);
        pauseController.Resume();
        cancellationTokenSource = new CancellationTokenSource();

        var options = new ApplicationOptions(forbiddenWords, outputDirectory, false);
        var progress = new Progress<ScanProgress>(UpdateProgress);

        AppendLog("Старт сканування...");

        try
        {
            var summary = await Task.Run(
                () => scanner.Scan(
                    options,
                    progress,
                    pauseController,
                    cancellationTokenSource.Token,
                    AddMatchToGrid),
                cancellationTokenSource.Token);

            var reportPath = ReportWriter.Write(summary);
            summary = summary with { ReportPath = reportPath };

            AppendLog($"Сканування завершено. Звіт: {summary.ReportPath}");
            statusLabel.Text = $"Готово. Знайдено файлів: {summary.MatchedFiles}, замін: {summary.TotalReplacementCount}";
        }
        catch (OperationCanceledException)
        {
            AppendLog("Сканування зупинено користувачем.");
            statusLabel.Text = "Зупинено.";
        }
        catch (Exception exception)
        {
            AppendLog($"Помилка: {exception.Message}");
            MessageBox.Show(this, exception.Message, "Помилка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
            SetScanState(false);
        }
    }

    private void PauseScan()
    {
        if (!scanRunning)
        {
            return;
        }

        pauseController.Pause();
        pauseButton.Enabled = false;
        resumeButton.Enabled = true;
        AppendLog("Пауза.");
        statusLabel.Text = "Пауза.";
    }

    private void ResumeScan()
    {
        if (!scanRunning)
        {
            return;
        }

        pauseController.Resume();
        pauseButton.Enabled = true;
        resumeButton.Enabled = false;
        AppendLog("Відновлено.");
        statusLabel.Text = "Відновлено.";
    }

    private void StopScan()
    {
        if (!scanRunning)
        {
            return;
        }

        cancellationTokenSource?.Cancel();
        pauseController.Resume();
        AppendLog("Запит на зупинку надіслано.");
    }

    private void ClearResults()
    {
        resultsGrid.Rows.Clear();
        logTextBox.Clear();
        driveProgressBar.Value = 0;
        fileProgressBar.Value = 0;
        driveProgressBar.Maximum = 1;
        fileProgressBar.Maximum = 1;
        statusLabel.Text = "Готово до запуску.";
    }

    private void UpdateProgress(ScanProgress progress)
    {
        driveProgressBar.Maximum = Math.Max(1, progress.DriveCount);
        driveProgressBar.Value = Math.Min(Math.Max(progress.DriveIndex, 0), driveProgressBar.Maximum);
        fileProgressBar.Maximum = Math.Max(1, progress.FilesInDrive);
        fileProgressBar.Value = Math.Min(Math.Max(progress.FilesProcessedInDrive, 0), fileProgressBar.Maximum);

        statusLabel.Text = $"{progress.CurrentDrive}: {progress.FilesProcessedInDrive}/{progress.FilesInDrive} файлів, знайдено {progress.MatchedFiles}";
        if (!string.IsNullOrWhiteSpace(progress.CurrentFilePath) && progress.Stage is "match")
        {
            AppendLog($"Знайдено: {progress.CurrentFilePath}");
        }
    }

    private void AddMatchToGrid(FoundFileInfo info)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AddMatchToGrid(info));
            return;
        }

        resultsGrid.Rows.Add(
            info.SourcePath,
            info.SizeBytes,
            info.ReplacementCount,
            info.OriginalCopyPath,
            info.RedactedCopyPath);
    }

    private static List<string> ParseWords(string text)
    {
        return text
            .Split(new[] { ',', ';', '|', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(word => !string.IsNullOrWhiteSpace(word))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void AppendLog(string message)
    {
        if (InvokeRequired)
        {
            BeginInvoke(() => AppendLog(message));
            return;
        }

        logTextBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private void SetScanState(bool running)
    {
        scanRunning = running;
        startButton.Enabled = !running;
        pauseButton.Enabled = running && !pauseController.IsPaused;
        resumeButton.Enabled = running && pauseController.IsPaused;
        stopButton.Enabled = running;
        loadWordsButton.Enabled = !running;
        browseOutputButton.Enabled = !running;
        wordsTextBox.Enabled = !running;
        outputTextBox.Enabled = !running;
        clearButton.Enabled = !running;

        if (!running)
        {
            pauseController.Resume();
        }
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!scanRunning)
        {
            return;
        }

        cancellationTokenSource?.Cancel();
        pauseController.Resume();
    }
}
