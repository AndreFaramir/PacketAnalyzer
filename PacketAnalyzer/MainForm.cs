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

        List<Packet> outgoingPackets = new List<Packet>();

        public MainForm()
        {
            InitializeComponent();
            this.TopMost = true;
            DLL_PATH = @"C:\Users\Martin\Desktop\Program\PacketAnalyzer\Debug\PacketAnalyzerDLL.dll";
            DLL_PATH_LENGTH = (uint)DLL_PATH.Length;
            pipeListener = new Thread(PipeListener);
            pipeListener.Start();
        }

        private void PipeListener(object sender)
        {
            NamedPipeClientStream pipeClient = new NamedPipeClientStream("packetanalyzer");

            pipeClient.Connect();
            byte[] buffer = new byte[2048];
            while (pipeClient.IsConnected)
            {
                if (pipeClient.CanRead)
                {
                    pipeClient.Read(buffer, 0, 2048);
                    ProcessPacket(buffer);
                }

                Thread.Sleep(10);
            }

        }

        private void ProcessPacket(byte[] packet)
        {
            if (packet[0] == 1) //Is outgoing packet
            {
                uint timeStamp = BitConverter.ToUInt32(packet, 1);
                uint packetSize = BitConverter.ToUInt32(packet, 5);
                byte packetId = packet[17];
                string textData = Encoding.ASCII.GetString(packet, 18, (int)packetSize);
                listView1.Invoke((MethodInvoker)delegate
                {
                    ListViewItem lvi = new ListViewItem();
                    lvi.Text = timeStamp.ToString();
                    lvi.SubItems.Add("0x" + packetId.ToString("X"));
                    lvi.SubItems.Add(packetSize.ToString());
                    Packet p;
                    switch (packetId)
                    {
                        case 0x65:
                            p = new Packet(packetId, "PLAYER_MOVE_NORTH", packet);
                            break;
                        case 0x66:
                            p = new Packet(packetId, "PLAYER_MOVE_EAST", packet);
                            break;
                        case 0x67:
                            p = new Packet(packetId, "PLAYER_MOVE_SOUTH", packet);
                            break;
                        case 0x68:
                            p = new Packet(packetId, "PLAYER_MOVE_WEST", packet);
                            break;
                        case 0x6A:
                            p = new Packet(packetId, "PLAYER_MOVE_NORTH_EAST", packet);
                            break;
                        case 0x6B:
                            p = new Packet(packetId, "PLAYER_MOVE_SOUTH_EAST", packet);
                            break;
                        case 0x6C:
                            p = new Packet(packetId, "PLAYER_MOVE_SOUTH_WEST", packet);
                            break;
                        case 0x6D:
                            p = new Packet(packetId, "PLAYER_MOVE_NORTH_WEST", packet);
                            break;
                        case 0x96:
                            p = new Packet(packetId, "PLAYER_SPEECH", packet);
                            break;
                        default:
                            p = new Packet(packetId, "UNKNOWN_PACKET", packet);
                            break;
                    }

                    lvi.SubItems.Add(p.PacketName);
                    listView1.Items.Add(lvi);
                    outgoingPackets.Add(p);
                });
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

        private void listView1_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {

        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            InjectForm injectForm = new InjectForm();
            if (injectForm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                InjectDLL(injectForm.SelectedClient);
            }
        }
    }
}
