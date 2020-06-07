using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Display
{
    public class AutoStart
    {
        public AutoStart(string exePath, string exeParams, bool start)
        {
            ExePath = exePath;
            ExeParams = exeParams;
            Start = start;
        }

        public string ExePath { get; set; }
        public string ExeParams { get; set; }
        public bool Start { get; set; }

    }
    public partial class ProcessManager : Window, INotifyPropertyChanged
    {
        private List<Process> _processList = new List<Process>(); //список всех процессов
        private Dictionary<Process, List<Process>> _processTree = new Dictionary<Process, List<Process>>();
        private List<Process> _programList = new List<Process>();
        private ProcessInfo _selectedProcess = new ProcessInfo(); // процесс, который выбран в списке по клику
        private Process _selectedProgram = new Process();
        private List<AutoStart> _autoStart = new List<AutoStart> {  new AutoStart(@"C:\Users\админ\Downloads\Telegram\Telegram\Telegram.exe", "", true ),
                                                            new AutoStart(@"C:\Program Files\AVAST Software\Avast\AvastUI.exe", "", true ),
                                                            new AutoStart(@"C:\Program Files (x86)\Google\Picasa3\Picasa3.exe", "", true ),
                                                            new AutoStart(@"C:\Users\админ\AppData\Local\Viber\Viber.exe", "", true ),
                                                            new AutoStart(@"C:\Program Files (x86)\FinalWire\AIDA64 Extreme\aida64.exe", "/SILENCE", true )};
        public List<AutoStart> AutoStartList { get => _autoStart; set { _autoStart = value; } }

        public List<Process> ProcessList { get => _processList; } // проперти, дает доступ к _processList
        public List<Process> SelectedProgramProcessList
        {
            get
            {
                List<Process> res = new List<Process>();
                _processTree.TryGetValue(_selectedProgram, out res);
                return res;
            }
        }// проперти, дает доступ к _processList
        public List<Process> ProgramList { get => _programList; } // проперти, дает доступ к _processList
        public ProcessInfo SelectedProcess { get => _selectedProcess; } // проперти, дает доступ к _selectedProcess
        public string SelectedProcessName // проперти, дает доступ имени выбранного процесса, нужен для биндинга к интерфейсу
        {
            get
            {
                string result = "Process Name: " + _selectedProcess.Name;
                return result;
            }
        }
        public string SelectedMainModule // проперти, дает доступ имени выбранного процесса, нужен для биндинга к интерфейсу
        {
            get
            {
                string result = "Main Module: " + _selectedProcess.PathToExe;
                return result;
            }
        }
        public string SelectedProcessPID // проперти, нужен для биндинга к интерфейсу
        {
            get
            {
                string result = /*"Process PID: "*/ _selectedProcess.PID;
                return result;
            }
        }

        

        public string SelectedProcessWindow { get => _selectedProcess.Window; } // проперти, нужен для биндинга к интерфейсу
        public List<string> SelectedProcessModules { get => _selectedProcess.ModulesNames; } // проперти, нужен для биндинга к интерфейсу
        public Dictionary<Process, List<Process>> ProcessTree { get => _processTree; set => _processTree = value; }

        public ProcessManager()
        {
            InitializeComponent(); // инициализация интерфейса, сгенерировано вижуал студией
            Task.Run(() => RefreshProcessList()); // запуск отдельного потока, в котором будет работать метод RefreshProcessList()
        }

        public event PropertyChangedEventHandler PropertyChanged; // событие, сигнализирующее интерфейсу об изменениях в отображаемых данных

        protected void OnPropertyChanged(string name) // вызов события PropertyChanged
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string pid = SelectedProcessPID;
            Process myProc = null;
            try
            {
                int i = int.Parse(pid);
                myProc = Process.GetProcessById(i);
                myProc.Kill();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void RefreshProcessList() // метод, в котором происходит обновления списка процессов
        {
            while (true) // вечный цикл
            {
                List<Process> processes = Process.GetProcesses().ToList(); // в новую переменную записываем все текущии процессы
                OnPropertyChanged("ProcessList");
                if (Refresh(processes))
                    RefreshProgramList();
            }
        }
        private void RefreshProgramList() // метод, в котором происходит обновления списка процессов
        {
            _programList = new List<Process>();
            List<Process> processes = Process.GetProcesses().ToList(); // в новую переменную записываем все текущии процессы
            List<Process> programs = new List<Process>();
            foreach (var process in processes)
            {
                if (!String.IsNullOrEmpty(process.MainWindowTitle))
                {
                    programs.Add(process);
                    _programList.Add(process);
                }
            }
            foreach (var process in _programList)
            {
                List<Process> list = new List<Process>();
                try
                {
                    var modName = process.MainModule.ModuleName;
                    foreach (var p in processes)
                    {
                        if (p.MainModule.ModuleName == modName)
                            list.Add(p);
                    }
                    //var ap = processes.FindAll(p => p.MainModule.ModuleName == modName);
                    _processTree.Add(process, list);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex.Message);
                    _processTree.Add(process, list);
                }
            }
            //if(Refresh(processes))
            OnPropertyChanged("ProgramList");
        }

        private bool Refresh(List<Process> processes)
        {
            bool res = false;
            if (_processList.Count == 0) // если список _processList был пуст - просто заполняем его
            {
                _processList = processes;
                //OnPropertyChanged("ProcessList"); // вызываем обновление полей, забинженых на проперти ProcessList
                res = true;
            }
            // если пересечение списков processes и _processList не равно количесву элементов в обоих этих списках, то значит список
            // _processList просрочен и его надо обновит
            else if (_processList.Intersect(processes).Count() != _processList.Count && _processList.Count != processes.Count)
            {
                _processList = processes;
                //OnPropertyChanged("ProcessList"); // вызываем обновление полей, забинженых на проперти ProcessList
                res = true;
            }
            Thread.Sleep(1000); // останавливаем поток на одну секунду
            return res;
        }

        // событие, вызываемое при клике на текст
        private void TextBlock_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender; // sender - объект, в котором произошел клик, т.е. TextBlock. 
                                                    //По-этому sender мы можен привести к типу TextBlock 
            if(textBlock != null) // если приведение типов прошло успешно
            {
                string processName = textBlock.Text; //берем имя вібранного процесса
                Process selectedProcess = _processList.FirstOrDefault(process => process.ProcessName == processName); // находим в списке _processList
                                                                                                                        // процесс с именем processName
                if(selectedProcess != null) // если искомій процесс существует
                {
                    try
                    {
                        _selectedProcess = ProcessInfoAccessor.GetInfo(selectedProcess); // достаем инфу о процессе
                        // уведомляем интерфейс о том, что даннsе измененs
                        OnPropertyChanged("SelectedProcess");
                        OnPropertyChanged("SelectedProcessName");
                        OnPropertyChanged("SelectedProcessPID");
                        OnPropertyChanged("SelectedProcessWindow");
                        OnPropertyChanged("SelectedProcessModules");
                        OnPropertyChanged("SelectedMainModule");
                    }
                    catch(Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void Grid_ProgramList_Focus(object sender, RoutedEventArgs e)
        {
            TextBlock textBlock = (TextBlock)sender; // sender - объект, в котором произошел клик, т.е. TextBlock. 
                                                     //По-этому sender мы можен привести к типу TextBlock 
            if (textBlock != null) // если приведение типов прошло успешно
            {
                string processName = textBlock.Text; //берем имя вібранного процесса
                Process selectedProgram = _programList.FirstOrDefault(process => process.ProcessName == processName); // находим в списке _processList
                                                                                                                      // процесс с именем processName
                if (selectedProgram != null) // если искомій процесс существует
                {
                    try
                    {
                        _selectedProgram = selectedProgram; // достаем инфу о процессе
                        // уведомляем интерфейс о том, что даннsе измененs
                        OnPropertyChanged("SelectedProgram");
                        OnPropertyChanged("SelectedProgramName");
                        OnPropertyChanged("SelectedProgramProcessList");
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
    // класс, созданный для удобства - выдает объект с удобно-отформатированной инфой о конкретном процессе
    public class ProcessInfoAccessor 
    {
        public static ProcessInfo GetInfo (Process process)
        {
            ProcessStartInfo processStartInfo = process.StartInfo;
            return new ProcessInfo(process.ProcessName, process.Id.ToString(), process.MainWindowTitle, process.Modules, process.MainModule.FileName);
        }

    }
    
    //класс, созданный для удобства - содержит удобно-отформатированную инфу о конкретном процессе
    public class ProcessInfo
    {
        public ProcessInfo() // конструктор без параметров, нужен при старте проги
        {
            Name = string.Empty;
            PID = string.Empty;
            Window = string.Empty;
            PathToExe = string.Empty;
            ProcessModuleCollection = null;
        }
        // конструктор с параметрами - должен пополняться по мере увеличения кол-ва необходимых полей
        public ProcessInfo(string name, string pid, string window, ProcessModuleCollection processModuleCollection, string pathToExe)
        {
            Name = name;
            PID = pid;
            Window = window;
            ProcessModuleCollection = processModuleCollection;
            PathToExe = pathToExe;
            foreach (ProcessModule module in processModuleCollection) // создаем список модулей, которые используются процессом
            {
                _modulesNames.Add(module.FileName);
            }
        }
        List<string> _modulesNames = new List<string>();
        public string Name { get; set; }
        public string PID { get; set; }
        public string Window { get; set; }
        public string PathToExe { get; set; }
        ProcessModuleCollection ProcessModuleCollection { get; set; }
        public List<string> ModulesNames { get => _modulesNames; }
    }
}
