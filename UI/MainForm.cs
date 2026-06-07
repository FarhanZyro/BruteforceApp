using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using BruteforceApp1.Core;
using BruteforceApp1.Logging;

namespace BruteforceApp1.UI
{
    // The main window - only handles UI, no brute force logic here
    public partial class MainForm : Form
    {
        private string _currentPlainPassword = null;
        private string _currentHash = null;

        private BruteForceEngine _engine;
        private PerformanceLogger _logger;

        private BruteForceResult _singleResult = null;
        private BruteForceResult _multiResult = null;

        private bool _attackRunning = false;
        private DateTime _attackStartTime;

        // Timer updates the elapsed time label every 50ms
        private System.Windows.Forms.Timer _uiRefreshTimer;

        private Button _btnGenerate;
        private Button _btnStartStop;
        private RadioButton _rdoMulti;
        private RadioButton _rdoSingle;
        private Label _lblPassword;
        private Label _lblHash;
        private Label _lblThreadInfo;
        private Label _lblElapsed;
        private Label _lblProgress;
        private ProgressBar _progressBar;
        private TextBox _txtResult;
        private RichTextBox _rtbLog;

        public MainForm()
        {
            _logger = new PerformanceLogger();
            InitializeComponent();
            BuildUI();
            SetupRefreshTimer();
            UpdateThreadInfoLabel();
        }

        private void BuildUI()
        {
            this.Text = "Brute-Force Password Cracker";
            this.Size = new Size(800, 680);
            this.MinimumSize = new Size(700, 600);
            this.BackColor = Color.FromArgb(18, 18, 30);
            this.ForeColor = Color.FromArgb(210, 210, 230);
            this.Font = new Font("Segoe UI", 9.5f);
            this.StartPosition = FormStartPosition.CenterScreen;

            var lblTitle = new Label
            {
                Text = "BRUTE-FORCE PASSWORD CRACKER",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Dock = DockStyle.Top,
                Height = 48,
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.FromArgb(24, 24, 44)
            };

            // GROUP 1: Password setup
            var grpPassword = MakeGroupBox("PASSWORD SETUP", 55, 10, 760, 115);

            var labelPwd = MakeLabel("Plain text:", 15, 32);
            _lblPassword = new Label
            {
                Location = new Point(105, 30),
                Size = new Size(220, 24),
                ForeColor = Color.LightGreen,
                Font = new Font("Consolas", 10.5f, FontStyle.Bold),
                Text = "(not generated yet)"
            };

            _btnGenerate = MakeButton("Generate New Password", 340, 26, 210, 36);
            _btnGenerate.Click += OnGenerateClicked;

            var labelHash = MakeLabel("SHA256 Hash:", 15, 72);
            _lblHash = new Label
            {
                Location = new Point(105, 70),
                Size = new Size(620, 20),
                ForeColor = Color.FromArgb(170, 170, 190),
                Font = new Font("Consolas", 8f),
                Text = "(hash will appear here)"
            };

            grpPassword.Controls.AddRange(new Control[] {
                labelPwd, _lblPassword, _btnGenerate, labelHash, _lblHash });

            // GROUP 2: Attack settings
            var grpAttack = MakeGroupBox("ATTACK SETTINGS", 178, 10, 760, 165);

            _rdoMulti = new RadioButton
            {
                Text = "Multi-Thread (uses multiple CPU cores at the same time)",
                Location = new Point(15, 28),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.FromArgb(100, 220, 130)
            };
            _rdoSingle = new RadioButton
            {
                Text = "Single-Thread (one core only - for benchmark comparison)",
                Location = new Point(15, 56),
                AutoSize = true,
                ForeColor = Color.FromArgb(220, 180, 100)
            };
            _rdoMulti.CheckedChanged += (s, e) => UpdateThreadInfoLabel();
            _rdoSingle.CheckedChanged += (s, e) => UpdateThreadInfoLabel();

            _lblThreadInfo = new Label
            {
                Location = new Point(15, 86),
                Size = new Size(500, 20),
                ForeColor = Color.FromArgb(140, 140, 170),
                Font = new Font("Segoe UI", 8.5f)
            };

            _btnStartStop = MakeButton("START ATTACK", 580, 20, 160, 100);
            _btnStartStop.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            _btnStartStop.BackColor = Color.FromArgb(35, 140, 70);
            _btnStartStop.Click += OnStartStopClicked;
            _btnStartStop.Enabled = false;

            var labelElapsedTitle = MakeLabel("Elapsed time:", 15, 112);
            _lblElapsed = new Label
            {
                Location = new Point(105, 109),
                AutoSize = true,
                Font = new Font("Consolas", 11f, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 215, 70),
                Text = "00:00.000"
            };

            _lblProgress = new Label
            {
                Location = new Point(15, 134),
                Size = new Size(540, 18),
                Font = new Font("Consolas", 8.5f),
                ForeColor = Color.FromArgb(180, 200, 255),
                Text = "0.0%  |  0 attempts"
            };

            _progressBar = new ProgressBar
            {
                Location = new Point(15, 150),
                Size = new Size(720, 6),
                Style = ProgressBarStyle.Continuous,
                ForeColor = Color.FromArgb(80, 180, 255)
            };

            grpAttack.Controls.AddRange(new Control[] {
                _rdoMulti, _rdoSingle, _lblThreadInfo,
                _btnStartStop, labelElapsedTitle, _lblElapsed,
                _lblProgress, _progressBar });

            // GROUP 3: Result
            var grpResult = MakeGroupBox("RESULT", 351, 10, 760, 68);
            _txtResult = new TextBox
            {
                Location = new Point(15, 26),
                Size = new Size(720, 32),
                Font = new Font("Consolas", 12f, FontStyle.Bold),
                BackColor = Color.FromArgb(14, 35, 14),
                ForeColor = Color.LightGreen,
                ReadOnly = true,
                Text = "Waiting for attack to start..."
            };
            grpResult.Controls.Add(_txtResult);

            // GROUP 4: Performance log
            var grpLog = MakeGroupBox("PERFORMANCE LOG", 427, 10, 760, 210);
            _rtbLog = new RichTextBox
            {
                Location = new Point(15, 26),
                Size = new Size(720, 172),
                BackColor = Color.FromArgb(10, 10, 18),
                ForeColor = Color.FromArgb(150, 215, 150),
                Font = new Font("Consolas", 8.5f),
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            grpLog.Controls.Add(_rtbLog);

            this.Controls.AddRange(new Control[] {
                lblTitle, grpPassword, grpAttack, grpResult, grpLog });
        }

        private void OnGenerateClicked(object sender, EventArgs e)
        {
            var generator = new PasswordGenerator();
            _currentPlainPassword = generator.GeneratePassword();
            _currentHash = PasswordHasher.Hash(_currentPlainPassword);

            _lblPassword.Text = _currentPlainPassword;
            _lblHash.Text = _currentHash;
            _progressBar.Value = 0;
            _lblProgress.Text = "0.0%  |  0 attempts";
            _lblElapsed.Text = "00:00.000";
            _txtResult.Text = "Password ready - press START ATTACK";
            _txtResult.ForeColor = Color.FromArgb(180, 200, 255);
            _btnStartStop.Enabled = true;

            Log($"[{Now()}] New password generated (length: {_currentPlainPassword.Length})");
        }

        private async void OnStartStopClicked(object sender, EventArgs e)
        {
            if (_attackRunning)
            {
                _engine?.Stop();
                SetUiStopped();
                Log($"[{Now()}] Attack stopped by user.");
                return;
            }

            SetUiRunning();

            var validator = new PasswordValidator(_currentHash);
            _engine = new BruteForceEngine(validator);

            // These run on background threads - Invoke makes them safe to use on UI
            _engine.OnProgressUpdate += (attempts, total) =>
            {
                UpdateUI(() =>
                {
                    double percent = total > 0 ? (double)attempts / total * 100.0 : 0;
                    _progressBar.Value = (int)Math.Min(100, percent);
                    _lblProgress.Text = $"{percent:F1}%  |  {attempts:N0} / {total:N0} attempts";
                });
            };

            _engine.OnPasswordFound += (password, elapsed) =>
            {
                UpdateUI(() =>
                {
                    _txtResult.Text = $"FOUND: \"{password}\"  (took {elapsed.TotalSeconds:F3} seconds)";
                    _txtResult.ForeColor = Color.LightGreen;
                    _progressBar.Value = 100;
                    SetUiStopped();
                });
            };

            _engine.OnAttackFinished += (elapsed) =>
            {
                UpdateUI(() =>
                {
                    _txtResult.Text = $"Not found after {elapsed.TotalSeconds:F3} seconds";
                    _txtResult.ForeColor = Color.Tomato;
                    SetUiStopped();
                });
            };

            BruteForceResult result;
            if (_rdoMulti.Checked)
            {
                result = await _engine.StartMultiThreadedAsync();
                _multiResult = result;
            }
            else
            {
                result = await _engine.StartSingleThreadedAsync();
                _singleResult = result;
            }

            _logger.LogRun(result);
            Log($"[{Now()}] {(result.IsMultiThreaded ? "Multi" : "Single")}-thread done: " +
                $"{result.ElapsedTime.TotalSeconds:F3}s | {result.TotalAttempts:N0} attempts | " +
                $"Found: {result.Success}");

            if (_singleResult != null && _multiResult != null)
                Log(_logger.BuildComparisonReport(_singleResult, _multiResult));
        }

        private void SetUiRunning()
        {
            _attackRunning = true;
            _attackStartTime = DateTime.Now;
            _btnStartStop.Text = "STOP";
            _btnStartStop.BackColor = Color.FromArgb(150, 35, 35);
            _btnGenerate.Enabled = false;
            _rdoMulti.Enabled = false;
            _rdoSingle.Enabled = false;
            _txtResult.Text = "Attack in progress...";
            _txtResult.ForeColor = Color.FromArgb(255, 215, 70);
        }

        private void SetUiStopped()
        {
            _attackRunning = false;
            _btnStartStop.Text = "START ATTACK";
            _btnStartStop.BackColor = Color.FromArgb(35, 140, 70);
            _btnGenerate.Enabled = true;
            _rdoMulti.Enabled = true;
            _rdoSingle.Enabled = true;
        }

        private void SetupRefreshTimer()
        {
            _uiRefreshTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _uiRefreshTimer.Tick += (s, e) =>
            {
                if (_attackRunning)
                {
                    TimeSpan elapsed = DateTime.Now - _attackStartTime;
                    _lblElapsed.Text = $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}.{elapsed.Milliseconds:D3}";
                }
            };
            _uiRefreshTimer.Start();
        }

        private void UpdateThreadInfoLabel()
        {
            int cores = Environment.ProcessorCount;
            int threads = _rdoMulti.Checked ? Math.Max(1, cores - 1) : 1;
            _lblThreadInfo.Text =
                $"Your CPU has {cores} cores  ->  this attack will use {threads} thread(s)  " +
                $"|  Charset: a-z + 0-9 (36 chars)  |  Max length: {PasswordGenerator.MaxLength}";
        }

        // Thread-safe UI update
        // Background threads cannot touch UI controls directly in WinForms
        // Invoke sends the action to the UI thread where it is safe to run
        private void UpdateUI(Action action)
        {
            if (IsDisposed) return;
            if (InvokeRequired) Invoke(action);
            else action();
        }

        private void Log(string text)
        {
            UpdateUI(() => { _rtbLog.AppendText(text + "\n"); _rtbLog.ScrollToCaret(); });
        }

        private static string Now() => DateTime.Now.ToString("HH:mm:ss");

        private GroupBox MakeGroupBox(string title, int top, int left, int width, int height)
        {
            return new GroupBox
            {
                Text = title,
                Top = top,
                Left = left,
                Width = width,
                Height = height,
                BackColor = Color.FromArgb(22, 22, 40),
                ForeColor = Color.FromArgb(100, 180, 255),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Parent = this
            };
        }

        private static Label MakeLabel(string text, int x, int y) =>
            new Label { Text = text, Location = new Point(x, y), AutoSize = true };

        private static Button MakeButton(string text, int x, int y, int w, int h) =>
            new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(w, h),
                BackColor = Color.FromArgb(45, 75, 130),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

        protected override void Dispose(bool disposing)
        {
            if (disposing) _uiRefreshTimer?.Dispose();
            base.Dispose(disposing);
        }
    }
}