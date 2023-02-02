using System;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Sockets;
using System.IO;
using System.Linq;

namespace WindowsServer
{
    public partial class Form1 : Form
    {
        readonly TcpListener server;
        public Form1()
        {
            InitializeComponent();
            server = new TcpListener(System.Net.IPAddress.Any, 1230);
            server.Start();
            server.BeginAcceptTcpClient(AcceptTcpClient, server);
        }
        private void AcceptTcpClient(IAsyncResult result)
        {
            TcpClient client = server.EndAcceptTcpClient(result);
            server.BeginAcceptTcpClient(AcceptTcpClient, server);

            byte[] buffer = new byte[client.ReceiveBufferSize*5000];
            int bytesRead = client.GetStream().Read(buffer, 0,  buffer.Length);

            using (MemoryStream ms = new MemoryStream(buffer, 0, bytesRead))
            {
                Image image = Image.FromStream(ms);
                pictureBox1.Image = image;
                ms.Dispose();
            }

        }
    }
}
