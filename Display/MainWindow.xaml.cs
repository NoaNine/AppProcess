﻿using System;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Display
{
    public partial class ProcessManager : Window, INotifyPropertyChanged
    {
        private List<Process> _processList = new List<Process>(); //список всех процессов
        ProcessInfo _selectedProcess = new ProcessInfo(); // процесс, который выбран в списке по клику
        public List<Process> ProcessList { get => _processList; } // проперти, дает доступ к _processList
        public ProcessInfo SelectedProcess { get => _selectedProcess; } // проперти, дает доступ к _selectedProcess
        public string SelectedProcessName // проперти, дает доступ имени выбранного процесса, нужен для биндинга к интерфейсу
        {
            get
            {
                string result = "Process Name: " + _selectedProcess.Name;
                return result;
            }
        }
        public string SelectedProcessPID // проперти, нужен для биндинга к интерфейсу
        {
            get
            {
                string result = _selectedProcess.PID;
                return result;
            }
        }
        public string SelectedMainModule
        {
            get
            {
                string result = "Path:" + _selectedProcess.MainModule;
                return result;
            }
        }
        /*public string MainWindowTitle
        {
            get
            {

                string result = _selectedProcess.Window;
                return result;
            }
        }*/
        //public string SelectedProcessWindow { get => _selectedProcess.Window; } // проперти, нужен для биндинга к интерфейсу
        public List<string> SelectedProcessModules { get => _selectedProcess.ModulesNames; } // проперти, нужен для биндинга к интерфейсу
        public List<string> SelectedProcessThread { get => _selectedProcess.ThreadNames; }
        public ProcessManager()
        {
            InitializeComponent(); // инициализация интерфейса, сгенерировано вижуал студией
            Task.Run(() => RefreshProcessList()); // запуск отдельного потока, в котором будет работать метод RefreshProcessList()
            ListMainTitle();
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
                if (_processList.Count == 0) // если список _processList был пуст - просто заполняем его
                {
                    _processList = processes;
                    OnPropertyChanged("ProcessList"); // вызываем обновление полей, забинженых на проперти ProcessList
                }
                // если пересечение списков processes и _processList не равно количесву элементов в обоих этих списках, то значит список
                // _processList просрочен и его надо обновит
                else if (_processList.Intersect(processes).Count() != _processList.Count && _processList.Count != processes.Count)
                {
                    _processList = processes;
                    OnPropertyChanged("ProcessList"); // вызываем обновление полей, забинженых на проперти ProcessList
                }
                Thread.Sleep(1000); // останавливаем поток на одну секунду
            }
        }
        private void TextBlock_GotFocus(object sender, RoutedEventArgs e)// событие, вызываемое при клике на текст
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
                        OnPropertyChanged("SelectedProcessThread");
                        OnPropertyChanged("SelectedMainModule");
                    }
                    catch(Exception ex)
                    {
                        Trace.WriteLine(ex.Message);
                    }
                }
            }
        }
        private void ListMainTitle()
        {
            List<Process> processes = Process.GetProcesses().ToList();
            foreach (Process p in processes)
            {
                if (!String.IsNullOrEmpty(p.MainWindowTitle))
                {
                    list.Items.Add(p.MainWindowTitle);
                }
            }
        }
    }
    public class ProcessInfoAccessor 
    // класс, созданный для удобства - выдает объект с удобно-отформатированной инфой о конкретном процессе
    {
        public static ProcessInfo GetInfo (Process process)
        {
            return new ProcessInfo(process.ProcessName, process.Id.ToString(), process.MainWindowTitle, process.Modules, process.Threads, process.MainModule.FileName);
        }
    }
    public class ProcessInfo 
    //класс, созданный для удобства - содержит удобно-отформатированную инфу о конкретном процессе
    {
        public ProcessInfo() // конструктор без параметров, нужен при старте проги
        {
            Name = string.Empty;
            PID = string.Empty;
            //Window = string.Empty;
            MainModule = string.Empty;
            ProcessModuleCollection = null;
            ProcessThreadCollection = null;
        }
        public ProcessInfo(string name, string pid, string window, ProcessModuleCollection processModuleCollection, ProcessThreadCollection processThreadCollection, string mainModule)
        // конструктор с параметрами - должен пополняться по мере увеличения кол-ва необходимых полей
        {
            Name = name;
            PID = pid;
            //Window = window;
            MainModule = mainModule;
            ProcessModuleCollection = processModuleCollection;
            ProcessThreadCollection = processThreadCollection;
            foreach (ProcessModule module in processModuleCollection) // создаем список модулей, которые используются процессом
            {
                _modulesNames.Add(module.FileName);
            }
            foreach (ProcessThread thread in processThreadCollection)
            {
                _threadNames.Add(thread.Id.ToString());
            }
        }
        List<string> _modulesNames = new List<string>();
        List<string> _threadNames = new List<string>();
        public string Name { get; set; }
        public string PID { get; set; }
        //public string Window { get; set; }
        public string MainModule { get; set; }
        ProcessModuleCollection ProcessModuleCollection { get; set; }
        ProcessThreadCollection ProcessThreadCollection { get; set; }
        public List<string> ModulesNames { get => _modulesNames; }
        public List<string> ThreadNames { get => _threadNames; }
    }
}
