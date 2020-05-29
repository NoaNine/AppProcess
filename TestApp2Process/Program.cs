using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TestApp2Process
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("****** ПРОЦЕССЫ ******\n");
            string comm = Command();
            UseMyCommand(comm);
            if (comm != "x")
                Main();
        }
        static string Command()
        {
            Console.WriteLine("Какую информацию нужно получить? \n" +
                " 1 - Список всех процессов\n 2 - Выбрать процесс по PID\n" +
                " 3 - Информация о потоках\n 4 - Информация о подключаемых модулях\n" +
                " 5 - Запуск процесса\n 6 - Останов процесса\n 7 - Информация о домене приложения\n x - выход\n");
            Console.Write("Введите команду: ");
            string comm = Console.ReadLine();
            return comm;
        }
        static void UseMyCommand(string str)
        {
            switch (str)
            {
                case "x":
                    break;
                case "1":
                    AllInfoProcess();
                    break;
                case "2":
                    ProcInMyPid();
                    break;
                case "3":
                    Threads();
                    break;
                case "4":
                    InfoByModuleProc();
                    break;
                case "5":
                    StartProcess();
                    break;
                case "6":
                    StopProcess();
                    break;
                case "7":
                    InfoDomein();
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Команда не распознана!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    break;
            }
            Console.WriteLine();
        }
        static void AllInfoProcess()
        {
            var myProcess = from proc in Process.GetProcesses(".")
                            orderby proc.Id
                            select proc;
            Console.WriteLine("\n*** Текущие процессы ***\n");
            foreach (var p in myProcess)
                Console.WriteLine("-> PID: {0}\tName: {1}", p.Id, p.ProcessName);
        }
        static void ProcInMyPid()
        {
            Console.Write("Введите PID-идентификатор: ");
            string pid = Console.ReadLine();
            Process myProc = null;
            try
            {
                int i = int.Parse(pid);
                myProc = Process.GetProcessById(i);
                Console.WriteLine("\n-> PID: {0}\tName: {1}\n", myProc.Id, myProc.ProcessName);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static void Threads()
        {
            Console.Write("Введите PID-идентификатор: ");
            string pid = Console.ReadLine();
            Process myProc = null;
            try
            {
                int i = int.Parse(pid);
                myProc = Process.GetProcessById(i);
                ProcessThreadCollection treads = myProc.Threads;
                Console.WriteLine("Потоки процесса {0}:\n", myProc.ProcessName);
                foreach (ProcessThread pt in treads)
                    Console.WriteLine("-> Thread ID: {0}\tВремя: {1}\tПриоритет: {2}"
                        , pt.Id, pt.StartTime.ToShortTimeString(), pt.PriorityLevel);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine();
            }
        }
        static void InfoByModuleProc()
        {
            Console.Write("Введите PID-идентификатор: ");
            string pid = Console.ReadLine();
            Process myProc = null;
            try
            {
                int i = int.Parse(pid);
                myProc = Process.GetProcessById(i);
                Console.WriteLine("Подключаемые модули процесса {0}:\n", myProc.ProcessName);
                ProcessModuleCollection mods = myProc.Modules;
                foreach(ProcessModule pm in mods)
                    Console.WriteLine("-> Имя модуля: " + pm.ModuleName);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Console.WriteLine();
            }
        }
        static void StartProcess()
        {
            Process myProc = null;
            try
            {
                myProc = Process.Start("notepad.exe");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        static void StopProcess()
        {
            Console.Write("Введите PID-идентификатор процесса, который нужно остановить: ");
            string pid = Console.ReadLine();
            Process myProc = null;
            try
            {
                int i = int.Parse(pid);
                myProc = Process.GetProcessById(i);
                myProc.Kill();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        static void InfoDomein()
        {
            AppDomain defaultD = AppDomain.CurrentDomain;

            Console.WriteLine("-> Имя: {0}\n-> ID: {1}\n-> По умолчанию? {2}\n-> Путь: {3}\n",
                defaultD.FriendlyName, defaultD.Id, defaultD.IsDefaultAppDomain(), defaultD.BaseDirectory);

            Console.WriteLine("Загружаемые сборки: \n");
            var infAsm = from asm in defaultD.GetAssemblies()
                         orderby asm.GetName().Name
                         select asm;
            foreach (var a in infAsm)
                Console.WriteLine("-> Имя: {0}", a.GetName().Name);
        }
    }
}