using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Display
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ProcessManager : Window, INotifyPropertyChanged
    {
        private List<Process> _processList = new List<Process>();
        ProcessInfo _selectedProcess = new ProcessInfo();
        public List<Process> ProcessList { get => _processList; }
        public ProcessInfo SelectedProcess { get => _selectedProcess; }
        public string SelectedProcessName
        {
            get
            {
                string result = "Process Name: " + _selectedProcess.Name;
                return result;
            }
        }
        public string SelectedProcessPID
        {
            get
            {
                string result = "Process PID: " + _selectedProcess.PID;
                return result;
            }
        }
        public string SelectedProcessWindow { get => _selectedProcess.Window; }
        public List<string> SelectedProcessModules { get => _selectedProcess.ModulesNames; }
        public ProcessManager()
        {
            InitializeComponent();
            Task.Run(() => RefreshProcessList());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private void RefreshProcessList()
        {
            while (true)
            {
                List<Process> processes = Process.GetProcesses().ToList();
                if (_processList.Count == 0)
                {
                    _processList = processes;
                }
                else if (_processList.Intersect(processes).Count() != _processList.Count)
                {
                    _processList = processes;
                    OnPropertyChanged("ProcessList");
                }
                Thread.Sleep(1000);
            }
        }

        private void TextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender;
            if(textBlock != null)
            {
                string processName = textBlock.Text;
                Process selectedProcess = _processList.FirstOrDefault(process => process.ProcessName == processName);
                if(selectedProcess != null)
                {
                    try
                    {
                        _selectedProcess = ProcessInfoAccessor.GetInfo(selectedProcess);
                        OnPropertyChanged("SelectedProcess");
                        OnPropertyChanged("SelectedProcessName");
                        OnPropertyChanged("SelectedProcessPID");
                        OnPropertyChanged("SelectedProcessWindow");
                        OnPropertyChanged("SelectedProcessModules");
                    }
                    catch(Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
            }
        }
    }

    public class ProcessInfoAccessor
    {
        public static ProcessInfo GetInfo (Process process)
        {
            return new ProcessInfo(process.ProcessName, process.Id.ToString(), process.MainWindowTitle, process.Modules);
        }

    }

    public class ProcessInfo
    {
        public ProcessInfo()
        {
            Name = string.Empty;
            PID = string.Empty;
            Window = string.Empty;
            ProcessModuleCollection = null;
        }
        public ProcessInfo(string name, string pid, string window, ProcessModuleCollection processModuleCollection)
        {
            Name = name;
            PID = pid;
            Window = window;
            ProcessModuleCollection = processModuleCollection;
            foreach(ProcessModule module in processModuleCollection)
            {
                _modulesNames.Add(module.FileName);
            }
        }
        List<string> _modulesNames = new List<string>();
        public string Name { get; set; }
        public string PID { get; set; }
        public string Window { get; set; }
        ProcessModuleCollection ProcessModuleCollection { get; set; }
        public List<string> ModulesNames { get => _modulesNames; }
    }
}
