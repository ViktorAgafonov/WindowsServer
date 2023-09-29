using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsServer
{
    public partial class Form1 : Form
    {
        private readonly TcpListener server;

        public Form1()
        {
            InitializeComponent();

            server = new TcpListener(IPAddress.Any, 1230);
            server.Start();

            Task.Run(AcceptTcpClientsAsync);
        }

        private async Task AcceptTcpClientsAsync()
        {
            while (true)
            {
                TcpClient client = await server.AcceptTcpClientAsync();
                _ = ProcessClientAsync(client);
            }
        }

        private async Task ProcessClientAsync(TcpClient client)
        {
            try
            {
                byte[] sizeBuffer = new byte[4];
                await client.GetStream().ReadAsync(sizeBuffer, 0, sizeBuffer.Length);

                int fileSize = BitConverter.ToInt32(sizeBuffer, 0);
                byte[] dataBuffer = new byte[fileSize];

                int totalBytesRead = 0;
                while (totalBytesRead < fileSize)
                {
                    int bytesRead = await client.GetStream().ReadAsync(dataBuffer, totalBytesRead, fileSize - totalBytesRead);

                    if (bytesRead == 0)
                    {
                        throw new ApplicationException("Connection closed prematurely");
                    }

                    totalBytesRead += bytesRead;
                }

                using (MemoryStream ms = new MemoryStream(dataBuffer))
                {
                    Image image = Image.FromStream(ms);
                    pictureBox1.Image = image;

                    string imageInfo = $"Received Image, Size: {image.Width} x {image.Height}, File Size: {fileSize} bytes";
                    ShowImageInformation(imageInfo);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                client.Dispose();
            }
        }

        private void ShowImageInformation(string info)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowImageInformation(info)));
            }
            else
            {
                lblImageInfo.Text = info;
            }
        }
    }
}