using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using PacketAnalyzer.Packets;
using PacketAnalyzer.Packets.Outgoing;

namespace PacketAnalyzer
{
    public partial class MainForm : Form
    {
        string DLL_PATH;
        uint DLL_PATH_LENGTH;
        Thread pipeListener;

        List<OutgoingPacket> outgoingPackets = new List<OutgoingPacket>();

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

        private void ProcessPacket(byte[] buffer)
        {
            byte type = buffer[0];
            uint timeStamp = BitConverter.ToUInt32(buffer, 1);
            uint packetSize = BitConverter.ToUInt32(buffer, 5);
            byte[] padding = new byte[8];
            Array.Copy(buffer, 9, padding, 0, 8);
            byte packetId = buffer[17];
            OutgoingPacket packet;
            if (type == 1)
            {
                switch (packetId)
                {
                    case 0x65:
                    case 0x66:
                    case 0x67:
                    case 0x68:
                    case 0x6A:
                    case 0x6B:
                    case 0x6C:
                    case 0x6D:
                        packet = new PlayerMove(packetId, buffer);
                        break;
                    case 0x96:
                        packet = new PlayerSpeech(packetId, buffer);
                        break;
                    default:
                        packet = new Unknown(packetId, buffer);
                        break;
                }

                outgoingPackets.Add(packet);
                AddList(timeStamp, packetId, packetSize, packet.Description);
            }
        }

        public void AddList(uint timeStamp, byte id, uint size, string name)
        {
            listView1.Invoke((MethodInvoker)delegate
            {
                ListViewItem lvi = new ListViewItem();
                lvi.Text = timeStamp.ToString();
                lvi.SubItems.Add(id.ToString("X"));
                lvi.SubItems.Add(size.ToString());
                lvi.SubItems.Add(name);


                listView1.Items.Add(lvi);
                if (!listView1.Focused)
                {
                    listView1.EnsureVisible(listView1.Items.Count - 1);
                }
            });
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
            dataGridView1.Rows.Clear();
            OutgoingPacket packet = outgoingPackets[e.ItemIndex];

            foreach (PacketData d in packet.ProcessedData)
            {
                dataGridView1.Rows.Add(d.Description, d.Type, d.Size, d.Data);
            }
            
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
