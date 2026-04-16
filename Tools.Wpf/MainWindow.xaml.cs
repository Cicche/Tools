using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml;
using Forms = System.Windows.Forms;
using Tools;
using Tools.Core.Abstractions;
using Tools.Core.Models;
using Tools.Core.Services;

namespace Tools.Wpf
{
    public partial class MainWindow : Window
    {
        private enum XmlSchemaKind
        {
            Modern,
            Legacy,
            Unknown
        }

        private enum LogSeverity
        {
            Info,
            Warning,
            Error
        }

        private enum BatchOperation
        {
            Ping,
            Reboot,
            Shutdown,
            Kill,
            Copy
        }

        private static readonly string[] CategoryOrder = { "CMP", "TRD", "SERVER", "GW", "DOK", "MFC" };
        private const int MaxBatchParallelism = 8;
        private const int DefaultTimeoutMs = 8000;
        private const int DefaultRetryCount = 3;
        private const int MaxLogLines = 2000;
        private const int ScannerPanelWidth = 280;
        private const int ScannerToggleWidth = 26;
        private static readonly Brush SuccessBrush = Brushes.LimeGreen;  //new SolidColorBrush((Color)ColorConverter.ConvertFromString("#74E08A"));
        private static readonly Brush WarningBrush = Brushes.Yellow;  //new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C58B2A"));
        private static readonly Brush ErrorBrush = Brushes.IndianRed;

        private readonly IToolsCoreFacade coreFacade = new ToolsCoreFacade();
        private readonly IAppLogger appLogger = new FileLogger();

        private readonly MainViewModel viewModel = new MainViewModel();
        private readonly ObservableCollection<ScanResultRow> scanResults = new ObservableCollection<ScanResultRow>();
        private List<PC> loadedMachines = new List<PC>();
        private CancellationTokenSource batchCts;
        private CancellationTokenSource scanCts;
        private bool isBatchRunning;
        private bool isScannerExpanded;
        private bool isScannerAnimating;
        private double widthBeforeScanner = double.NaN;

        private sealed class HostnameValidationResult
        {
            public bool Checked { get; set; }
            public bool IsMatch { get; set; }
            public string Expected { get; set; }
            public string Resolved { get; set; }
            public string Error { get; set; }
        }

        private sealed class ScanResultRow
        {
            public string Ip { get; set; }
            public string Host { get; set; }
            public uint IpOrder { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.XmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tool_List.xml");
            DgScanResults.ItemsSource = scanResults;
            InitializeDefaultScanRange();
            LoadMachines();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleMaximizeRestore();
                return;
            }

            DragMove();
        }

        private void BtnWindowMin_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void BtnWindowMax_Click(object sender, RoutedEventArgs e)
        {
            ToggleMaximizeRestore();
        }

        private void BtnWindowClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void BtnPingSelected_OnClick(object sender, RoutedEventArgs e)
        {
            await RunBatchAsync(BatchOperation.Ping);
        }

        private async void BtnRebootSelected_OnClick(object sender, RoutedEventArgs e)
        {
            await RunBatchAsync(BatchOperation.Reboot);
        }

        private async void BtnShutdownSelected_OnClick(object sender, RoutedEventArgs e)
        {
            await RunBatchAsync(BatchOperation.Shutdown);
        }

        private async void BtnKillSelected_OnClick(object sender, RoutedEventArgs e)
        {
            await RunBatchAsync(BatchOperation.Kill);
        }

        private async void BtnCopySelected_OnClick(object sender, RoutedEventArgs e)
        {
            await RunBatchAsync(BatchOperation.Copy);
        }

        private void BtnRefreshView_OnClick(object sender, RoutedEventArgs e)
        {
            RebuildCategoriesFromMachines();
        }

        private void CategoryPanel_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ShouldIgnoreCategoryBulkToggle(e.OriginalSource as DependencyObject)) return;
            if (!(sender is Border border) || !(border.DataContext is CategoryPanelModel category)) return;

            foreach (var row in category.Machines)
            {
                row.IsSelected = true;
            }
        }

        private void CategoryPanel_MouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ShouldIgnoreCategoryBulkToggle(e.OriginalSource as DependencyObject)) return;
            if (!(sender is Border border) || !(border.DataContext is CategoryPanelModel category)) return;

            foreach (var row in category.Machines)
            {
                row.IsSelected = false;
            }
        }

        private void BtnCancelBatch_OnClick(object sender, RoutedEventArgs e)
        {
            batchCts?.Cancel();
            AppendLog("Richiesta annullamento batch inviata.");
        }

        private void BtnScannerToggle_OnClick(object sender, RoutedEventArgs e)
        {
            if (isScannerAnimating) return;
            isScannerAnimating = true;

            isScannerExpanded = !isScannerExpanded;
            bool opening = isScannerExpanded;

            if (opening)
            {
                if (WindowState == WindowState.Normal)
                {
                    widthBeforeScanner = Width;
                    Width = Width + ScannerPanelWidth;
                }

                ScannerColumn.Width = new GridLength(ScannerPanelWidth);
                ScannerPanel.Visibility = Visibility.Visible;
                BtnScannerToggle.Content = "<";
            }
            else
            {
                BtnScannerToggle.Content = ">";
            }

            var opacityAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(220),
                To = opening ? 1 : 0,
                EasingFunction = new CubicEase
                {
                    EasingMode = opening ? EasingMode.EaseOut : EasingMode.EaseIn
                }
            };

            var slideAnimation = new DoubleAnimation
            {
                Duration = TimeSpan.FromMilliseconds(220),
                To = opening ? 0 : 24,
                EasingFunction = new CubicEase
                {
                    EasingMode = opening ? EasingMode.EaseOut : EasingMode.EaseIn
                }
            };

            slideAnimation.Completed += (_, __) =>
            {
                if (!opening)
                {
                    ScannerPanel.Visibility = Visibility.Collapsed;
                    ScannerColumn.Width = new GridLength(0);

                    if (WindowState == WindowState.Normal && !double.IsNaN(widthBeforeScanner))
                    {
                        Width = Math.Max(MinWidth, widthBeforeScanner);
                    }

                    widthBeforeScanner = double.NaN;
                }
                isScannerAnimating = false;
            };

            ScannerPanel.BeginAnimation(UIElement.OpacityProperty, opacityAnimation);
            ScannerPanelTranslate.BeginAnimation(System.Windows.Media.TranslateTransform.XProperty, slideAnimation);
        }

        private async void BtnScanStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (scanCts != null)
            {
                AppendLog("Scanner già in esecuzione.", LogSeverity.Warning);
                return;
            }

            if (!TryParseIpRange(TxtScanStart.Text, TxtScanEnd.Text, out uint start, out uint end, out string parseError))
            {
                AppendLog("Scanner IP - " + parseError, LogSeverity.Warning);
                return;
            }

            int timeoutMs = 600;
            int.TryParse(TxtScanTimeout.Text, out timeoutMs);
            if (timeoutMs < 100) timeoutMs = 100;
            if (timeoutMs > 5000) timeoutMs = 5000;

            scanResults.Clear();
            scanCts = new CancellationTokenSource();
            BtnScanStart.IsEnabled = false;
            BtnScanStop.IsEnabled = true;

            try
            {
                AppendLog($"Scanner IP avviato: {UIntToIp(start)} -> {UIntToIp(end)}");
                await ScanRangeAsync(start, end, timeoutMs, scanCts.Token);
                AppendLog($"Scanner IP completato. Host trovati: {scanResults.Count}");
            }
            catch (OperationCanceledException)
            {
                AppendLog("Scanner IP annullato.", LogSeverity.Warning);
            }
            finally
            {
                scanCts.Dispose();
                scanCts = null;
                BtnScanStart.IsEnabled = true;
                BtnScanStop.IsEnabled = false;
            }
        }

        private void BtnScanStop_OnClick(object sender, RoutedEventArgs e)
        {
            scanCts?.Cancel();
        }

        private void BtnResetCredentials_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBoxResult confirm = MessageBox.Show(
                "Confermi il reset del file credenziali cifrato?",
                "Reset credenziali",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            if (confirm != MessageBoxResult.Yes) return;

            coreFacade.ResetCredentialStore();
            foreach (PC pc in loadedMachines)
            {
                pc.User = string.Empty;
                pc.Password = string.Empty;
            }

            RebuildCategoriesFromMachines();
            AppendLog("Store credenziali resettato.");
        }

        private void BtnBrowseCopyOrigin_OnClick(object sender, RoutedEventArgs e)
        {
            string selected = SelectFolderPath(viewModel.CopyOrigin);
            if (string.IsNullOrWhiteSpace(selected)) return;
            viewModel.CopyOrigin = selected;
        }

        private async void MachineButton_OnClick(object sender, RoutedEventArgs e)
        {
            var row = ResolveMachineRowFromButton(sender);
            if (row == null) return;
            await ExecuteSingleAsync(row, BatchOperation.Ping, CancellationToken.None);
        }

        private async void MachineOpenExplorer_OnClick(object sender, RoutedEventArgs e)
        {
            var row = ResolveMachineRowFromContextMenu(sender);
            if (row == null) return;
            if (!HasResolvedCredentials(row))
            {
                var denied = OperationResult.Fail("Credenziali non disponibili.", OperationErrorCode.CredentialsMissing, row.Source);
                ApplyResultVisual(row, denied);
                AppendOperationResult("OPEN_FOLDER", denied);
                return;
            }
            await ExecuteSingleAsync(row, BatchOperation.Copy, CancellationToken.None, openFolderOnly: true, operationLabel: "OPEN_FOLDER");
        }

        private async void MachineOpenRdp_OnClick(object sender, RoutedEventArgs e)
        {
            var row = ResolveMachineRowFromContextMenu(sender);
            if (row == null) return;

            row.IsBusy = true;
            try
            {
                OperationResult result = await ExecuteWithPolicyAsync(
                    () => coreFacade.OpenRemoteDesktopAsync(row.Source),
                    row.Source,
                    "RDP",
                    CancellationToken.None);
                ApplyResultVisual(row, result);
                AppendOperationResult("RDP", result);
            }
            finally
            {
                row.IsBusy = false;
            }
        }

        private void MachineEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var row = ResolveMachineRowFromContextMenu(sender);
            if (row == null) return;

            PC machine = row.Source;
            if (machine == null) return;

            string originalName = machine.Nome ?? string.Empty;
            string originalIp = machine.Ip ?? string.Empty;

            var dialog = new EditMachineWindow(machine, CategoryOrder) { Owner = this };
            if (dialog.ShowDialog() != true) return;

            if (dialog.DeleteRequested)
            {
                loadedMachines = loadedMachines
                    .Where(pc => !string.Equals(pc.Nome, originalName, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                coreFacade.RemoveCredentialsForMachine(new PC { Nome = originalName, Ip = originalIp });
                coreFacade.SaveSanitized(viewModel.XmlPath, loadedMachines);
                RebuildCategoriesFromMachines();
                AppendLog($"DELETE_MACHINE - {originalName} rimossa.");
                return;
            }

            bool duplicate = loadedMachines.Any(x =>
                !ReferenceEquals(x, machine) &&
                (string.Equals(x.Nome, dialog.MachineName, StringComparison.OrdinalIgnoreCase) ||
                 string.Equals(x.Ip, dialog.Ip, StringComparison.OrdinalIgnoreCase)));
            if (duplicate)
            {
                AppendLog("EDIT_MACHINE - Nome o IP gia presenti. Modifica annullata.", LogSeverity.Warning);
                return;
            }

            if (!IPAddress.TryParse(dialog.Ip, out _))
            {
                AppendLog($"EDIT_MACHINE - IP non valido: {dialog.Ip}", LogSeverity.Warning);
                return;
            }

            coreFacade.RemoveCredentialsForMachine(new PC { Nome = originalName, Ip = originalIp });

            machine.Type = dialog.Category;
            machine.Nome = dialog.MachineName;
            machine.Ip = dialog.Ip;
            machine.User = string.Empty;
            machine.Password = string.Empty;

            if (!string.IsNullOrWhiteSpace(dialog.User) && !string.IsNullOrWhiteSpace(dialog.Password))
            {
                coreFacade.SetMachineCredentials(machine, dialog.User, dialog.Password);
            }

            coreFacade.SaveSanitized(viewModel.XmlPath, loadedMachines);
            coreFacade.ApplyCredentials(loadedMachines);
            RebuildCategoriesFromMachines();
            AppendLog($"EDIT_MACHINE - {originalName} aggiornato in {machine.Nome} ({machine.Ip}) categoria {machine.Type}.");
        }

        private void BtnCategoryAction_OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null) return;
            string category = (button.Tag as string ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(category)) return;

            var menu = new ContextMenu();
            var categoryCredentials = new MenuItem { Header = "Aggiungi credenziali categoria", Tag = category };
            categoryCredentials.Click += CategoryCredentials_OnClick;
            var addMachine = new MenuItem { Header = "Aggiungi PC in categoria", Tag = category };
            addMachine.Click += CategoryAddMachine_OnClick;

            menu.Items.Add(categoryCredentials);
            menu.Items.Add(addMachine);
            menu.PlacementTarget = button;
            menu.IsOpen = true;
        }

        private void CategoryCredentials_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menuItem)) return;
            string category = (menuItem.Tag as string ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(category)) return;

            var dialog = new CategoryCredentialsWindow(category) { Owner = this };
            if (dialog.ShowDialog() != true) return;

            coreFacade.SetCategoryCredentials(category, dialog.User, dialog.Password);
            coreFacade.ApplyCredentials(loadedMachines);
            RebuildCategoriesFromMachines();
            AppendLog($"CATEGORY_CREDENTIALS - {category} aggiornate.");
        }

        private void CategoryAddMachine_OnClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is MenuItem menuItem)) return;
            string category = (menuItem.Tag as string ?? string.Empty).Trim().ToUpperInvariant();
            if (string.IsNullOrWhiteSpace(category)) return;

            var dialog = new AddMachineWindow(category) { Owner = this };
            if (dialog.ShowDialog() != true) return;

            bool duplicate = loadedMachines.Any(x =>
                string.Equals(x.Nome, dialog.MachineName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Ip, dialog.Ip, StringComparison.OrdinalIgnoreCase));
            if (duplicate)
            {
                AppendLog("ADD_MACHINE - Nome o IP gia presenti. Inserimento annullato.", LogSeverity.Warning);
                return;
            }

            if (!IPAddress.TryParse(dialog.Ip, out _))
            {
                AppendLog($"ADD_MACHINE - IP non valido: {dialog.Ip}", LogSeverity.Warning);
                return;
            }

            var newPc = new PC
            {
                Type = category,
                Nome = dialog.MachineName,
                Ip = dialog.Ip,
                User = string.Empty,
                Password = string.Empty
            };

            loadedMachines.Add(newPc);
            coreFacade.SaveSanitized(viewModel.XmlPath, loadedMachines);
            coreFacade.SetMachineCredentials(newPc, dialog.User, dialog.Password);
            coreFacade.ApplyCredentials(new[] { newPc });
            RebuildCategoriesFromMachines();
            AppendLog($"ADD_MACHINE - {newPc.Nome} ({newPc.Ip}) categoria {category}.");
        }

        private async Task RunBatchAsync(BatchOperation operation)
        {
            if (isBatchRunning)
            {
                AppendLog("Batch già in esecuzione.", LogSeverity.Warning);
                return;
            }

            var allRows = viewModel.Categories.SelectMany(c => c.Machines).ToList();
            var targetRows = allRows.Where(r => r.IsSelected).ToList();
            if (targetRows.Count == 0)
            {
                AppendLog("Nessuna macchina selezionata.");
                return;
            }

            if (operation == BatchOperation.Copy &&
                (string.IsNullOrWhiteSpace(viewModel.CopyOrigin) || string.IsNullOrWhiteSpace(viewModel.CopyDestination)))
            {
                AppendLog("COPY - Origine o destinazione mancanti.", LogSeverity.Warning);
                return;
            }

            if (RequiresCredentials(operation))
            {
                var missingCredentials = targetRows.Where(r => !HasResolvedCredentials(r)).ToList();
                if (missingCredentials.Count > 0)
                {
                    foreach (var row in missingCredentials)
                    {
                        var denied = OperationResult.Fail("Credenziali non disponibili.", OperationErrorCode.CredentialsMissing, row.Source);
                        ApplyResultVisual(row, denied);
                        AppendOperationResult(operation.ToString().ToUpperInvariant(), denied);
                    }
                }

                targetRows = targetRows.Where(HasResolvedCredentials).ToList();
                if (targetRows.Count == 0)
                {
                    AppendLog($"Batch {operation}: nessuna macchina con credenziali disponibili.", LogSeverity.Warning);
                    return;
                }
            }

            isBatchRunning = true;
            batchCts = new CancellationTokenSource();
            BtnCancelBatch.IsEnabled = true;
            SetToolbarEnabled(false);
            appLogger.Info("BATCH", $"{operation} START selected={targetRows.Count} retry={GetRetryCount()} timeoutMs={GetTimeoutMs()}");

            int total = targetRows.Count;
            int completed = 0;
            int success = 0;
            int failed = 0;
            bool isProgressVisible = BatchProgressBar.Visibility == Visibility.Visible && TxtBatchStatus.Visibility == Visibility.Visible;
            if (isProgressVisible)
            {
                BatchProgressBar.Value = 0;
                TxtBatchStatus.Text = $"Batch {operation}: 0/{total} (retry {GetRetryCount()}, timeout {GetTimeoutMs()}ms)";
            }

            var gate = new SemaphoreSlim(MaxBatchParallelism);
            var tasks = targetRows.Select(async row =>
            {
                await gate.WaitAsync(batchCts.Token);
                try
                {
                    await ExecuteSingleAsync(row, operation, batchCts.Token);
                    if (!batchCts.IsCancellationRequested)
                    {
                        if (IsSuccessfulLikeStatus(row.Status)) Interlocked.Increment(ref success);
                        else Interlocked.Increment(ref failed);
                    }
                }
                catch (OperationCanceledException)
                {
                    // no-op
                }
                finally
                {
                    int done = Interlocked.Increment(ref completed);
                    if (isProgressVisible)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            BatchProgressBar.Value = (done * 100.0) / total;
                            TxtBatchStatus.Text = $"Batch {operation}: {done}/{total} - OK:{success} KO:{failed}";
                        });
                    }
                    gate.Release();
                }
            }).ToList();

            try
            {
                await Task.WhenAll(tasks);
                if (batchCts.IsCancellationRequested)
                {
                    AppendLog($"Batch {operation} annullato.", LogSeverity.Warning);
                    appLogger.Warn("BATCH", $"{operation} CANCELLED done={completed} ok={success} ko={failed}");
                }
                else
                {
                    AppendLog($"Batch {operation} completato. OK:{success} KO:{failed} Tot:{total}");
                    appLogger.Info("BATCH", $"{operation} DONE ok={success} ko={failed} total={total}");
                }
            }
            catch (OperationCanceledException)
            {
                AppendLog($"Batch {operation} annullato.", LogSeverity.Warning);
                appLogger.Warn("BATCH", $"{operation} CANCELLED via exception done={completed} ok={success} ko={failed}");
            }
            finally
            {
                isBatchRunning = false;
                BtnCancelBatch.IsEnabled = false;
                SetToolbarEnabled(true);
                gate.Dispose();
                batchCts.Dispose();
                batchCts = null;
            }
        }

        private async Task ExecuteSingleAsync(
            MachineRow row,
            BatchOperation operation,
            CancellationToken token,
            bool openFolderOnly = false,
            string operationLabel = null)
        {
            if (row == null) return;
            token.ThrowIfCancellationRequested();

            await Dispatcher.InvokeAsync(() =>
            {
                row.IsBusy = true;
                row.Status = operation.ToString().ToUpperInvariant();
                row.Background = Brushes.Khaki;
            });

            try
            {
                OperationResult result;
                if (openFolderOnly)
                {
                    result = await ExecuteWithPolicyAsync(
                        () => coreFacade.OpenFolderAsync(row.Source),
                        row.Source,
                        operationLabel ?? "OPEN_FOLDER",
                        token);
                }
                else
                {
                    if (RequiresPrePing(operation))
                    {
                        OperationResult prePing = await coreFacade.PingAsync(row.Source);
                        if (prePing == null || !prePing.Success)
                        {
                            result = OperationResult.Fail(
                                "Host non raggiungibile (pre-check). Comando non inviato.",
                                OperationErrorCode.NetworkUnreachable,
                                row.Source);

                            token.ThrowIfCancellationRequested();
                            await Dispatcher.InvokeAsync(() =>
                            {
                                ApplyResultVisual(row, result);
                                AppendOperationResult(operation.ToString().ToUpperInvariant(), result);
                            });
                            return;
                        }
                    }

                    result = await ExecuteOperationAsync(row.Source, operation, token);
                }

                token.ThrowIfCancellationRequested();
                await Dispatcher.InvokeAsync(() =>
                {
                    ApplyResultVisual(row, result);
                    string label = string.IsNullOrWhiteSpace(operationLabel)
                        ? operation.ToString().ToUpperInvariant()
                        : operationLabel;
                    AppendOperationResult(label, result);
                });

                if (!openFolderOnly && operation == BatchOperation.Ping && result != null && result.Success)
                {
                    HostnameValidationResult validation = await ValidateHostnameByIpAsync(row.Source);
                    if (validation.Checked && !validation.IsMatch)
                    {
                        await Dispatcher.InvokeAsync(() =>
                        {
                            row.Background = WarningBrush;
                            row.Status = "WARN NAME";
                            AppendLog(
                                $"[PING] {row.Name} ({row.Ip}) - WARNING [HOSTNAME_MISMATCH] atteso={validation.Expected} risolto={validation.Resolved}",
                                LogSeverity.Warning);
                        });
                    }
                    else if (!validation.Checked && !string.IsNullOrWhiteSpace(validation.Error))
                    {
                        await Dispatcher.InvokeAsync(() =>
                            AppendLog($"[PING] {row.Name} ({row.Ip}) - warning verifica hostname: {validation.Error}", LogSeverity.Warning));
                    }
                }
            }
            finally
            {
                await Dispatcher.InvokeAsync(() => row.IsBusy = false);
            }
        }

        private Task<OperationResult> ExecuteOperationAsync(PC machine, BatchOperation operation, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            switch (operation)
            {
                case BatchOperation.Ping:
                    return ExecuteWithPolicyAsync(() => coreFacade.PingAsync(machine), machine, "PING", token);
                case BatchOperation.Reboot:
                    return ExecuteWithPolicyAsync(() => coreFacade.RebootAsync(machine), machine, "REBOOT", token);
                case BatchOperation.Shutdown:
                    return ExecuteWithPolicyAsync(() => coreFacade.ShutdownAsync(machine), machine, "SHUTDOWN", token);
                case BatchOperation.Kill:
                    return ExecuteWithPolicyAsync(() => coreFacade.KillProcessesAsync(machine), machine, "KILL", token);
                case BatchOperation.Copy:
                    return ExecuteWithPolicyAsync(
                        () => coreFacade.CopyFolderAsync(machine, viewModel.CopyOrigin, viewModel.CopyDestination),
                        machine,
                        "COPY",
                        token);
                default:
                    return Task.FromResult(OperationResult.Fail("Operazione non supportata.", OperationErrorCode.InvalidInput, machine));
            }
        }

        private void LoadMachines()
        {
            try
            {
                XmlSchemaKind schemaKind = DetectXmlSchemaKind(viewModel.XmlPath);
                switch (schemaKind)
                {
                    case XmlSchemaKind.Modern:
                        AppendLog($"XML moderno rilevato: {Path.GetFileName(viewModel.XmlPath)}");
                        break;
                    case XmlSchemaKind.Legacy:
                        AppendLog($"XML legacy rilevato: {Path.GetFileName(viewModel.XmlPath)}. Conversione automatica in formato moderno.", LogSeverity.Warning);
                        break;
                    default:
                        AppendLog($"Formato XML non ancora rilevabile: {Path.GetFileName(viewModel.XmlPath)}. Procedo con bootstrap.", LogSeverity.Warning);
                        break;
                }

                bool strictMode = IsStrictCredentialModeEnabled();
                if (schemaKind == XmlSchemaKind.Legacy && strictMode)
                {
                    strictMode = false;
                    AppendLog("STRICT mode bypass temporaneo per migrazione automatica legacy -> moderno.", LogSeverity.Warning);
                }

                BootstrapResult bootstrap = coreFacade.Bootstrap(viewModel.XmlPath, strictMode);

                foreach (string message in bootstrap.Messages)
                {
                    AppendLog(message);
                }

                if (bootstrap.BlockedByStrictMode)
                {
                    loadedMachines = new List<PC>();
                    RebuildCategoriesFromMachines();
                    AppendLog("Caricamento bloccato da STRICT mode.", LogSeverity.Error);
                    return;
                }

                if (!bootstrap.Loaded)
                {
                    if (bootstrap.TemplateCreated)
                    {
                        AppendLog("Template XML creato. Ricarico automaticamente...");
                        BootstrapResult secondPass = coreFacade.Bootstrap(viewModel.XmlPath, strictMode);
                        foreach (string message in secondPass.Messages)
                        {
                            AppendLog(message);
                        }

                        if (secondPass.Loaded)
                        {
                            loadedMachines = secondPass.Machines.ToList();
                            RebuildCategoriesFromMachines();
                            AppendLog($"Caricamento completato. Macchine: {loadedMachines.Count}");
                            if (secondPass.CredentialStoreMissing)
                            {
                                AppendLog("Store credenziali assente: imposta credenziali categoria/macchina dall'app.", LogSeverity.Warning);
                            }
                            return;
                        }
                    }

                    loadedMachines = new List<PC>();
                    RebuildCategoriesFromMachines();
                    AppendLog("Caricamento non completato: controllare XML/log.", LogSeverity.Warning);
                    return;
                }

                loadedMachines = bootstrap.Machines.ToList();
                RebuildCategoriesFromMachines();
                AppendLog($"Caricamento completato. Macchine: {loadedMachines.Count}");
                if (bootstrap.CredentialStoreMissing)
                {
                    AppendLog("Store credenziali assente: imposta credenziali categoria/macchina dall'app.", LogSeverity.Warning);
                }
            }
            catch (Exception ex)
            {
                AppendLog("Errore caricamento: " + ex.Message);
            }
        }

        private void RebuildCategoriesFromMachines()
        {
            var selectedNames = new HashSet<string>(
                viewModel.Categories.SelectMany(c => c.Machines).Where(m => m.IsSelected).Select(m => m.Name),
                StringComparer.OrdinalIgnoreCase);

            viewModel.Categories.Clear();
            foreach (string category in CategoryOrder)
            {
                var panel = new CategoryPanelModel
                {
                    Name = category,
                    RowsPerColumn = category == "SERVER" || category == "GW" ? 2 : 5
                };

                var categoryMachines = loadedMachines
                    .Where(m => m != null && string.Equals((m.Type ?? string.Empty).Trim(), category, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(m => m.Nome)
                    .ToList();

                foreach (PC machine in categoryMachines)
                {
                    panel.Machines.Add(new MachineRow
                    {
                        Source = machine,
                        Type = category,
                        Name = (machine.Nome ?? string.Empty).ToUpperInvariant(),
                        Ip = machine.Ip ?? string.Empty,
                        CredentialState = string.IsNullOrWhiteSpace(machine.User) || string.IsNullOrWhiteSpace(machine.Password) ? "Mancanti" : "Presenti",
                        Background = Brushes.LightGray,
                        Status = string.Empty,
                        IsSelected = selectedNames.Contains(machine.Nome ?? string.Empty)
                    });
                }

                panel.ColumnCount = Math.Max(1, (int)Math.Ceiling(panel.Machines.Count / (double)panel.RowsPerColumn));
                panel.VisibleRowCount = Math.Max(1, Math.Min(panel.RowsPerColumn, panel.Machines.Count == 0 ? 1 : panel.Machines.Count));
                panel.ItemsHostHeight = CalculateItemsHostHeight(panel.RowsPerColumn);
                panel.PanelMinHeight = CalculatePanelMinHeight(panel.VisibleRowCount);
                panel.PanelMinWidth = CalculatePanelMinWidth(panel.ColumnCount);
                viewModel.Categories.Add(panel);
            }

            viewModel.CategoryRows.Clear();
            AddCategoryRow("CMP", "TRD");
            AddCategoryRow("SERVER", "GW");
            AddCategoryRow("DOK", "MFC");

            RecalculateWindowConstraints();
        }

        private static double CalculatePanelMinHeight(int visibleRows)
        {
            const double headerHeight = 34;
            const double panelPadding = 16;
            const double buttonHeight = 28;
            const double rowGap = 4;

            int rows = Math.Max(1, visibleRows);
            return headerHeight + panelPadding + (rows * buttonHeight) + ((rows - 1) * rowGap);
        }

        private static double CalculateItemsHostHeight(int rowsPerColumn)
        {
            const double rowHeight = 32; // button(28) + spacing
            const double rowGap = 4;
            int rows = Math.Max(1, rowsPerColumn);
            return (rows * rowHeight) + ((rows - 1) * rowGap);
        }

        private static double CalculatePanelMinWidth(int columnCount)
        {
            const double panelPadding = 16;
            const double itemWidth = 122; // 102 button + 4 gap + 16 checkbox
            const double columnGap = 4;
            int cols = Math.Max(1, columnCount);
            return panelPadding + (cols * itemWidth) + ((cols - 1) * columnGap);
        }

        private void RecalculateWindowConstraints()
        {
            double leftWidth = 0;
            double rightWidth = 0;
            double categoriesMinHeight = 0;

            foreach (CategoryRowModel row in viewModel.CategoryRows)
            {
                if (row == null) continue;
                leftWidth = Math.Max(leftWidth, row.Left?.PanelMinWidth ?? 0);
                rightWidth = Math.Max(rightWidth, row.Right?.PanelMinWidth ?? 0);
                categoriesMinHeight += Math.Max(row.Left?.PanelMinHeight ?? 0, row.Right?.PanelMinHeight ?? 0) + 2;
            }

            viewModel.CategoriesContainerMinWidth = Math.Max(760, leftWidth + rightWidth + 8);
            viewModel.CategoriesContainerMinHeight = Math.Max(260, categoriesMinHeight);

            const double actionsColumnWidth = 214;
            const double scannerToggleWidth = ScannerToggleWidth;
            const double chromeAndMarginsWidth = 90;
            const double topSectionsHeight = 168;
            const double logMinHeight = 10;
            const double chromeAndMarginsHeight = 70;

            MinWidth = Math.Max(1000, viewModel.CategoriesContainerMinWidth + actionsColumnWidth + scannerToggleWidth + chromeAndMarginsWidth);
            MinHeight = Math.Max(700, topSectionsHeight + viewModel.CategoriesContainerMinHeight + logMinHeight + chromeAndMarginsHeight);
        }

        private void AddCategoryRow(string leftCategory, string rightCategory)
        {
            CategoryPanelModel left = viewModel.Categories.FirstOrDefault(c =>
                string.Equals(c.Name, leftCategory, StringComparison.OrdinalIgnoreCase));
            CategoryPanelModel right = viewModel.Categories.FirstOrDefault(c =>
                string.Equals(c.Name, rightCategory, StringComparison.OrdinalIgnoreCase));

            if (left == null) left = new CategoryPanelModel { Name = leftCategory };
            if (right == null) right = new CategoryPanelModel { Name = rightCategory };

            viewModel.CategoryRows.Add(new CategoryRowModel
            {
                Left = left,
                Right = right
            });
        }

        private static MachineRow ResolveMachineRowFromButton(object sender)
        {
            var btn = sender as Button;
            return btn?.Tag as MachineRow;
        }

        private static MachineRow ResolveMachineRowFromContextMenu(object sender)
        {
            var menuItem = sender as MenuItem;
            if (menuItem?.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is Button btn)
            {
                return btn.Tag as MachineRow;
            }

            return null;
        }

        private void ApplyResultVisual(MachineRow row, OperationResult result)
        {
            if (row == null) return;

            if (result != null && result.Success)
            {
                row.Background = SuccessBrush;
                row.Status = "OK";
            }
            else
            {
                row.Background = ErrorBrush;
                row.Status = result == null ? "KO" : "KO " + result.Code;
                row.IsSelected = false;
            }
        }

        private void AppendOperationResult(string operation, OperationResult result)
        {
            if (result == null)
            {
                AppendLog($"[{operation}] risultato nullo.", LogSeverity.Error);
                appLogger.Error(operation, "result=null");
                return;
            }

            string machine = string.IsNullOrWhiteSpace(result.MachineName)
                ? "(sconosciuta)"
                : $"{result.MachineName} ({result.MachineIp})";

            string status = result.Success ? "OK" : $"KO [{result.Code}]";
            LogSeverity severity = ResolveSeverity(result);
            AppendLog($"[{operation}] {machine} - {status} - {result.Message}", severity);
            string logLine = $"{operation} machine={result.MachineName} ip={result.MachineIp} status={status} message={result.Message}";
            if (result.Success)
            {
                appLogger.Info("OPERATION", logLine);
            }
            else if (severity == LogSeverity.Warning)
            {
                appLogger.Warn("OPERATION", logLine);
            }
            else
            {
                appLogger.Error("OPERATION", logLine);
            }
        }

        private void AppendLog(string text, LogSeverity severity = LogSeverity.Info)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => AppendLog(text, severity));
                return;
            }

            var paragraph = new Paragraph { Margin = new Thickness(0) };
            var run = new Run($"[{DateTime.Now:HH:mm:ss}] {text}");
            run.Foreground = GetLogBrush(severity);
            paragraph.Inlines.Add(run);
            TxtLog.Document.Blocks.Add(paragraph);
            while (TxtLog.Document.Blocks.Count > MaxLogLines)
            {
                TxtLog.Document.Blocks.Remove(TxtLog.Document.Blocks.FirstBlock);
            }
            TxtLog.ScrollToEnd();

            if (severity == LogSeverity.Error)
            {
                appLogger.Error("UI", text);
            }
            else if (severity == LogSeverity.Warning)
            {
                appLogger.Warn("UI", text);
            }
            else
            {
                appLogger.Info("UI", text);
            }
        }

        private void SetToolbarEnabled(bool enabled)
        {
            BtnPingSelected.IsEnabled = enabled;
            BtnRebootSelected.IsEnabled = enabled;
            BtnShutdownSelected.IsEnabled = enabled;
            BtnKillSelected.IsEnabled = enabled;
            BtnCopySelected.IsEnabled = enabled;
            BtnRefreshView.IsEnabled = enabled;
            BtnResetCredentials.IsEnabled = enabled;
            TxtTimeoutMs.IsEnabled = enabled;
            TxtRetryCount.IsEnabled = enabled;
            TxtCopyOrigin.IsEnabled = enabled;
            TxtCopyDestination.IsEnabled = enabled;
            BtnBrowseCopyOrigin.IsEnabled = enabled;
        }

        private static string SelectFolderPath(string initialPath)
        {
            using (var dialog = new Forms.FolderBrowserDialog())
            {
                dialog.Description = "Seleziona cartella";
                dialog.ShowNewFolderButton = false;

                if (!string.IsNullOrWhiteSpace(initialPath) && Directory.Exists(initialPath))
                {
                    dialog.SelectedPath = initialPath;
                }

                Forms.DialogResult result = dialog.ShowDialog();
                if (result != Forms.DialogResult.OK) return string.Empty;
                return dialog.SelectedPath ?? string.Empty;
            }
        }

        private static XmlSchemaKind DetectXmlSchemaKind(string xmlPath)
        {
            if (string.IsNullOrWhiteSpace(xmlPath) || !File.Exists(xmlPath))
            {
                return XmlSchemaKind.Unknown;
            }

            try
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null
                };

                var doc = new XmlDocument { XmlResolver = null };
                using (var stream = new FileStream(xmlPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var reader = XmlReader.Create(stream, settings))
                {
                    doc.Load(reader);
                }

                if (doc.SelectSingleNode("/ToolsConfig/ArrayOfPC") != null)
                {
                    return XmlSchemaKind.Modern;
                }

                if (doc.SelectSingleNode("/ArrayOfPC") != null)
                {
                    return XmlSchemaKind.Legacy;
                }

                return XmlSchemaKind.Unknown;
            }
            catch
            {
                return XmlSchemaKind.Unknown;
            }
        }

        private async Task<OperationResult> ExecuteWithPolicyAsync(
            Func<Task<OperationResult>> operation,
            PC machine,
            string operationName,
            CancellationToken token)
        {
            int retryCount = GetRetryCount();
            int timeoutMs = GetTimeoutMs();
            timeoutMs = ResolveTimeoutForOperation(operationName, timeoutMs);
            OperationResult last = null;

            for (int attempt = 0; attempt <= retryCount; attempt++)
            {
                token.ThrowIfCancellationRequested();

                Task<OperationResult> runTask = operation();
                Task finished = await Task.WhenAny(runTask, Task.Delay(timeoutMs, token));
                if (finished != runTask)
                {
                    last = OperationResult.Fail($"Timeout dopo {timeoutMs}ms.", OperationErrorCode.Timeout, machine);
                }
                else
                {
                    last = await runTask;
                }

                if (last.Success) return last;

                bool canRetry = attempt < retryCount &&
                                IsRetryable(last) &&
                                !string.Equals(operationName, "PING", StringComparison.OrdinalIgnoreCase) &&
                                !string.Equals(operationName, "COPY", StringComparison.OrdinalIgnoreCase) &&
                                last.Code != OperationErrorCode.Timeout;
                if (!canRetry) return last;

                AppendLog($"[{operationName}] retry {attempt + 1}/{retryCount} - {machine.Nome} ({machine.Ip})");
                await Task.Delay(300 * (attempt + 1), token);
            }

            return last ?? OperationResult.Fail("Operazione fallita.", OperationErrorCode.Unexpected, machine);
        }

        private static int ResolveTimeoutForOperation(string operationName, int configuredTimeoutMs)
        {
            if (string.Equals(operationName, "REBOOT", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(operationName, "SHUTDOWN", StringComparison.OrdinalIgnoreCase))
            {
                // Reboot/Shutdown includono verifica offline post-comando; 12s è spesso troppo basso.
                return Math.Max(configuredTimeoutMs, 25000);
            }

            return configuredTimeoutMs;
        }

        private static bool IsRetryable(OperationResult result)
        {
            if (result == null) return false;
            return result.Code == OperationErrorCode.NetworkUnreachable ||
                   result.Code == OperationErrorCode.Timeout ||
                   result.Code == OperationErrorCode.ExternalProcessError;
        }

        private static bool IsSuccessfulLikeStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;
            if (string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase)) return true;
            return status.StartsWith("WARN", StringComparison.OrdinalIgnoreCase);
        }

        private static bool RequiresCredentials(BatchOperation operation)
        {
            return operation != BatchOperation.Ping;
        }

        private static bool RequiresPrePing(BatchOperation operation)
        {
            return operation == BatchOperation.Reboot ||
                   operation == BatchOperation.Shutdown ||
                   operation == BatchOperation.Kill;
        }

        private static bool HasResolvedCredentials(MachineRow row)
        {
            if (row?.Source == null) return false;
            return !string.IsNullOrWhiteSpace(row.Source.User) &&
                   !string.IsNullOrWhiteSpace(row.Source.Password);
        }

        private void InitializeDefaultScanRange()
        {
            if (TryGetPrimaryNetworkRange(out string start, out string end))
            {
                TxtScanStart.Text = start;
                TxtScanEnd.Text = end;
                return;
            }

            TxtScanStart.Text = "192.168.1.1";
            TxtScanEnd.Text = "192.168.1.254";
        }

        private static bool TryGetPrimaryNetworkRange(out string start, out string end)
        {
            start = null;
            end = null;

            try
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n =>
                        n.OperationalStatus == OperationalStatus.Up &&
                        n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                        n.GetIPProperties().GatewayAddresses.Any(g =>
                            g?.Address != null &&
                            g.Address.AddressFamily == AddressFamily.InterNetwork))
                    .ToList();

                foreach (NetworkInterface adapter in adapters)
                {
                    var unicast = adapter.GetIPProperties().UnicastAddresses
                        .FirstOrDefault(u =>
                            u?.Address != null &&
                            u.Address.AddressFamily == AddressFamily.InterNetwork &&
                            u.IPv4Mask != null);
                    if (unicast == null) continue;

                    uint ip = IpToUInt(unicast.Address);
                    uint mask = IpToUInt(unicast.IPv4Mask);
                    uint network = ip & mask;
                    uint broadcast = network | ~mask;
                    if (broadcast <= network + 1) continue;

                    uint first = network + 1;
                    uint last = broadcast - 1;
                    start = UIntToIp(first);
                    end = UIntToIp(last);
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static bool TryParseIpRange(string startIp, string endIp, out uint start, out uint end, out string error)
        {
            start = 0;
            end = 0;
            error = null;

            if (!IPAddress.TryParse(startIp, out IPAddress startAddress) ||
                startAddress.AddressFamily != AddressFamily.InterNetwork)
            {
                error = "IP Start non valido.";
                return false;
            }

            if (!IPAddress.TryParse(endIp, out IPAddress endAddress) ||
                endAddress.AddressFamily != AddressFamily.InterNetwork)
            {
                error = "IP End non valido.";
                return false;
            }

            start = IpToUInt(startAddress);
            end = IpToUInt(endAddress);
            if (start > end)
            {
                error = "Range non valido: Start deve essere <= End.";
                return false;
            }

            uint size = end - start + 1;
            if (size > 4096)
            {
                error = "Range troppo ampio (max 4096 IP).";
                return false;
            }

            return true;
        }

        private async Task ScanRangeAsync(uint start, uint end, int timeoutMs, CancellationToken token)
        {
            const int parallelism = 64;
            var gate = new SemaphoreSlim(parallelism);
            var tasks = new List<Task>();

            for (uint ip = start; ip <= end; ip++)
            {
                token.ThrowIfCancellationRequested();
                uint ipOrder = ip;
                string ipString = UIntToIp(ipOrder);
                await gate.WaitAsync(token);
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        using (var ping = new Ping())
                        {
                            PingReply reply = await ping.SendPingAsync(ipString, timeoutMs);
                            if (reply.Status != IPStatus.Success) return;
                        }

                        string hostName = string.Empty;
                        try
                        {
                            IPHostEntry host = await Dns.GetHostEntryAsync(ipString);
                            hostName = NormalizeHostName(host?.HostName ?? string.Empty);
                        }
                        catch
                        {
                            hostName = string.Empty;
                        }

                        await Dispatcher.InvokeAsync(() =>
                        {
                            InsertScanResultOrdered(new ScanResultRow
                            {
                                Ip = ipString,
                                Host = string.IsNullOrWhiteSpace(hostName) ? "-" : hostName,
                                IpOrder = ipOrder
                            });
                        });
                    }
                    finally
                    {
                        gate.Release();
                    }
                }, token));

                if (ip == uint.MaxValue) break;
            }

            await Task.WhenAll(tasks);
            gate.Dispose();
        }

        private static uint IpToUInt(IPAddress ipAddress)
        {
            byte[] bytes = ipAddress.GetAddressBytes();
            return ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];
        }

        private static string UIntToIp(uint value)
        {
            uint b1 = (value >> 24) & 255;
            uint b2 = (value >> 16) & 255;
            uint b3 = (value >> 8) & 255;
            uint b4 = value & 255;
            return $"{b1}.{b2}.{b3}.{b4}";
        }

        private void InsertScanResultOrdered(ScanResultRow row)
        {
            if (row == null) return;

            int low = 0;
            int high = scanResults.Count;
            while (low < high)
            {
                int mid = low + ((high - low) / 2);
                if (scanResults[mid].IpOrder <= row.IpOrder)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid;
                }
            }

            scanResults.Insert(low, row);
        }

        private void ScanResultRow_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(sender is DataGridRow row) || !(row.Item is ScanResultRow scanRow)) return;
            AddMachineFromScanResult(scanRow);
        }

        private void DgScanResults_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(e.OriginalSource is DependencyObject source)) return;
            DataGridRow row = FindAncestor<DataGridRow>(source);
            if (row == null) return;

            row.IsSelected = true;
            DgScanResults.SelectedItem = row.Item;
            row.Focus();
        }

        private void ScanResultAddMachine_OnClick(object sender, RoutedEventArgs e)
        {
            ScanResultRow scanRow = null;
            if (sender is MenuItem menuItem &&
                menuItem.Parent is ContextMenu contextMenu &&
                contextMenu.PlacementTarget is DataGridRow dataGridRow)
            {
                scanRow = dataGridRow.Item as ScanResultRow;
            }

            if (scanRow == null)
            {
                scanRow = DgScanResults.SelectedItem as ScanResultRow;
            }

            if (scanRow == null) return;
            AddMachineFromScanResult(scanRow);
        }

        private void AddMachineFromScanResult(ScanResultRow scanRow)
        {
            if (scanRow == null) return;

            string category = PromptCategorySelection();
            if (string.IsNullOrWhiteSpace(category)) return;

            string proposedName = string.IsNullOrWhiteSpace(scanRow.Host) || scanRow.Host == "-"
                ? ("PC-" + scanRow.Ip.Replace(".", "-"))
                : scanRow.Host.ToUpperInvariant();

            var dialog = new AddMachineWindow(category, proposedName, scanRow.Ip) { Owner = this };
            if (dialog.ShowDialog() != true) return;

            bool duplicate = loadedMachines.Any(x =>
                string.Equals(x.Nome, dialog.MachineName, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(x.Ip, dialog.Ip, StringComparison.OrdinalIgnoreCase));
            if (duplicate)
            {
                AppendLog("SCAN_ADD_MACHINE - Nome o IP gia presenti. Inserimento annullato.", LogSeverity.Warning);
                return;
            }

            if (!IPAddress.TryParse(dialog.Ip, out _))
            {
                AppendLog($"SCAN_ADD_MACHINE - IP non valido: {dialog.Ip}", LogSeverity.Warning);
                return;
            }

            var newPc = new PC
            {
                Type = category,
                Nome = dialog.MachineName,
                Ip = dialog.Ip,
                User = string.Empty,
                Password = string.Empty
            };

            loadedMachines.Add(newPc);
            coreFacade.SaveSanitized(viewModel.XmlPath, loadedMachines);
            coreFacade.SetMachineCredentials(newPc, dialog.User, dialog.Password);
            coreFacade.ApplyCredentials(new[] { newPc });
            RebuildCategoriesFromMachines();
            AppendLog($"SCAN_ADD_MACHINE - {newPc.Nome} ({newPc.Ip}) categoria {category}.");
        }

        private string PromptCategorySelection()
        {
            var win = new Window
            {
                Title = "Seleziona categoria",
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Width = 300,
                Height = 150,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2A343E")),
                Foreground = Brushes.White
            };

            var root = new Grid { Margin = new Thickness(12) };
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var txt = new TextBlock
            {
                Text = "Categoria macchina",
                Margin = new Thickness(0, 0, 0, 8)
            };
            Grid.SetRow(txt, 0);
            root.Children.Add(txt);

            var combo = new ComboBox
            {
                ItemsSource = CategoryOrder,
                SelectedIndex = 0,
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(combo, 1);
            root.Children.Add(combo);

            var panelButtons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            var btnCancel = new Button { Content = "Annulla", Width = 80, Margin = new Thickness(0, 0, 8, 0) };
            var btnOk = new Button { Content = "OK", Width = 80 };
            btnCancel.Click += (_, __) => { win.DialogResult = false; win.Close(); };
            btnOk.Click += (_, __) => { win.DialogResult = true; win.Close(); };
            panelButtons.Children.Add(btnCancel);
            panelButtons.Children.Add(btnOk);
            Grid.SetRow(panelButtons, 2);
            root.Children.Add(panelButtons);

            win.Content = root;
            bool? result = win.ShowDialog();
            if (result != true) return string.Empty;
            return (combo.SelectedItem as string ?? string.Empty).Trim().ToUpperInvariant();
        }

        private int GetTimeoutMs()
        {
            if (!int.TryParse(viewModel.TimeoutMs, out int timeoutMs)) return DefaultTimeoutMs;
            if (timeoutMs < 1000) return 1000;
            if (timeoutMs > 120000) return 120000;
            return timeoutMs;
        }

        private int GetRetryCount()
        {
            if (!int.TryParse(viewModel.RetryCount, out int retryCount)) return DefaultRetryCount;
            if (retryCount < 0) return 0;
            if (retryCount > 5) return 5;
            return retryCount;
        }

        private static async Task<HostnameValidationResult> ValidateHostnameByIpAsync(PC machine)
        {
            var result = new HostnameValidationResult();
            if (machine == null || string.IsNullOrWhiteSpace(machine.Ip) || string.IsNullOrWhiteSpace(machine.Nome))
            {
                result.Checked = false;
                result.Error = "dati macchina incompleti";
                return result;
            }

            try
            {
                IPHostEntry hostEntry = await Dns.GetHostEntryAsync(machine.Ip);
                string expected = NormalizeHostName(machine.Nome);
                string resolved = NormalizeHostName(hostEntry?.HostName ?? string.Empty);

                result.Checked = true;
                result.Expected = expected;
                result.Resolved = resolved;
                result.IsMatch = !string.IsNullOrWhiteSpace(expected) &&
                                 !string.IsNullOrWhiteSpace(resolved) &&
                                 string.Equals(expected, resolved, StringComparison.OrdinalIgnoreCase);
                return result;
            }
            catch (Exception ex)
            {
                result.Checked = false;
                result.Error = ex.Message;
                return result;
            }
        }

        private static string NormalizeHostName(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;
            string normalized = value.Trim();
            int dotIndex = normalized.IndexOf('.');
            if (dotIndex >= 0)
            {
                normalized = normalized.Substring(0, dotIndex);
            }

            if (normalized.EndsWith("$", StringComparison.Ordinal))
            {
                normalized = normalized.Substring(0, normalized.Length - 1);
            }

            return normalized.Trim().ToUpperInvariant();
        }

        private static string ExtractCategoryTag(object sender)
        {
            if (sender is Button button)
            {
                return (button.Tag as string ?? string.Empty).Trim().ToUpperInvariant();
            }

            return string.Empty;
        }

        private static bool IsStrictCredentialModeEnabled()
        {
            bool strict = false;
            string raw = ConfigurationManager.AppSettings["Credentials.StrictMode"];
            if (!string.IsNullOrWhiteSpace(raw))
            {
                bool.TryParse(raw, out strict);
            }

            return strict;
        }

        private void ToggleMaximizeRestore()
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private static bool ShouldIgnoreCategoryBulkToggle(DependencyObject origin)
        {
            return FindAncestor<Button>(origin) != null ||
                   FindAncestor<CheckBox>(origin) != null ||
                   FindAncestor<MenuItem>(origin) != null;
        }

        private static T FindAncestor<T>(DependencyObject start) where T : DependencyObject
        {
            DependencyObject current = start;
            while (current != null)
            {
                if (current is T found) return found;
                current = System.Windows.Media.VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        private static LogSeverity ResolveSeverity(OperationResult result)
        {
            if (result == null) return LogSeverity.Error;
            if (result.Success) return LogSeverity.Info;

            switch (result.Code)
            {
                case OperationErrorCode.NetworkUnreachable:
                case OperationErrorCode.Timeout:
                case OperationErrorCode.ExternalProcessError:
                    return LogSeverity.Warning;
                default:
                    return LogSeverity.Error;
            }
        }

        private static Brush GetLogBrush(LogSeverity severity)
        {
            switch (severity)
            {
                case LogSeverity.Warning:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C58B2A"));
                case LogSeverity.Error:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF5A5A"));
                default:
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7CFC00"));
            }
        }
    }
}





