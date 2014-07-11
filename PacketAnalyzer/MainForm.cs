using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;

namespace PacketAnalyzer
{
    public partial class MainForm : Form
    {
        string DLL_PATH;
        uint DLL_PATH_LENGTH;
        Thread pipeListener;
        

        public MainForm()
        {
            InitializeComponent();
            DLL_PATH = @"C:\Users\Martin\Desktop\Program\PacketAnalyzer\Debug\PacketAnalyzerDLL.dll";
            DLL_PATH_LENGTH = (uint)DLL_PATH.Length;
            pipeListener = new Thread(PipeListener);
            InjectDLL(Process.GetProcessesByName("Tibia")[0].Id);
            pipeListener.Start();
        }

        private void PipeListener(object sender)
        {
            NamedPipeClientStream pipeClient = new NamedPipeClientStream("packetanalyzer");

            byte[] buffer = new byte[2048];
            while (pipeClient.IsConnected)
            {
                if (pipeClient.CanRead)
                    pipeClient.Read(buffer, 0, 2048);
                Thread.Sleep(10);
            }

        }

        private bool InjectDLL(int processId)
        {
            IntPtr kernel = WinAPI.GetModuleHandle("Kernel32");
            IntPtr loadLibrary = WinAPI.GetProcAddress(kernel, "LoadLibraryA");

            if (loadLibrary == IntPtr.Zero)
                return false;

            IntPtr process = WinAPI.OpenProcess(WinAPI.PROCESS_ALL_ACCESS, false, processId);

            if (process == IntPtr.Zero)
                return false;

            IntPtr remoteMemory = WinAPI.VirtualAllocEx(process, IntPtr.Zero, DLL_PATH_LENGTH + 1, WinAPI.AllocationType.Commit, WinAPI.MemoryProtection.ReadWrite);

            if (remoteMemory == IntPtr.Zero)
            {
                WinAPI.CloseHandle(process);
                return false;
            }

            //byte[] bytes = Encoding.ASCII.GetBytes(DLL_PATH);
            UIntPtr lpNumberOfBytesWritten;
            WinAPI.WriteProcessMemory(process, remoteMemory, Encoding.ASCII.GetBytes(DLL_PATH), DLL_PATH_LENGTH, out lpNumberOfBytesWritten);
            IntPtr remoteThread = WinAPI.CreateRemoteThread(process, IntPtr.Zero, 0, loadLibrary, remoteMemory, 0, IntPtr.Zero);

            WinAPI.VirtualFreeEx(process, remoteMemory, DLL_PATH_LENGTH, WinAPI.AllocationType.Release);
            WinAPI.CloseHandle(process);

            return true;
        }
    }
}
