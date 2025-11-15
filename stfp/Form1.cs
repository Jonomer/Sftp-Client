using Renci.SshNet;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace stfp
{
    public partial class Form1 : Form
    {
        SshClient sshClient;
        SftpClient sftpClient;

        public Form1()
        {
            InitializeComponent();
            SetupFormProperties();
        }

        private void SetupFormProperties()
        {
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = true;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "SFTP Client";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label7.Text = "Ready - SFTP Client is ready";
            label7.BackColor = Color.Black;
            label7.ForeColor = Color.White;
            label7.AutoSize = false;
            label7.Size = new Size(380, 60);
            label7.TextAlign = ContentAlignment.MiddleLeft;
            label7.BorderStyle = BorderStyle.FixedSingle;
        }

        private void btnSelectFile_Click(object sender, EventArgs e)
        {
            
            ContextMenuStrip contextMenu = new ContextMenuStrip();

           
            ToolStripMenuItem fileItem = new ToolStripMenuItem("Select Files");
            fileItem.Click += (s, args) => SelectFiles();

          
            ToolStripMenuItem folderItem = new ToolStripMenuItem("Select Folder");
            folderItem.Click += (s, args) => SelectFolder();

            contextMenu.Items.Add(fileItem);
            contextMenu.Items.Add(folderItem);

            
            contextMenu.Show(btnSelectFile, new Point(0, btnSelectFile.Height));
        }

        private void SelectFiles()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select Files";
                openFileDialog.Filter = "All Files (*.*)|*.*";
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox7.Text = string.Join("; ", openFileDialog.FileNames);
                    label7.Text = $"{openFileDialog.FileNames.Length} file(s) selected";
                }
            }
        }

        private void SelectFolder()
        {
            using (FolderBrowserDialog folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select a folder to upload";
                folderDialog.ShowNewFolderButton = true;
                folderDialog.RootFolder = Environment.SpecialFolder.Desktop;

                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    textBox7.Text = folderDialog.SelectedPath;
                    label7.Text = $"Folder selected: {Path.GetFileName(folderDialog.SelectedPath)}";
                }
            }
        }

        private void button3_Click(object sender, EventArgs e) 
        {
            try
            {
                string host = textBox1.Text;
                string user = textBox2.Text;
                string pass = textBox3.Text;
                int port = int.Parse(textBox6.Text);
                string localPath = textBox7.Text;
                string remotePath = textBox8.Text;

                if (string.IsNullOrEmpty(localPath))
                {
                    label7.Text = "Error: Please select files or folder!";
                    MessageBox.Show("Please select files or folder!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                label7.Text = "Connecting to SFTP server...";

                using (var sftp = new Renci.SshNet.SftpClient(host, port, user, pass))
                {
                    sftp.Connect();

                   
                    if (File.Exists(localPath))
                    {
                        // Tek dosya
                        UploadFile(sftp, localPath, remotePath);
                    }
                    else if (Directory.Exists(localPath))
                    {
                       
                        UploadFolder(sftp, localPath, remotePath);
                    }
                    else if (localPath.Contains(";"))
                    {
                        
                        UploadMultipleFiles(sftp, localPath, remotePath);
                    }
                    else
                    {
                        throw new Exception("Selected path is not valid");
                    }

                    sftp.Disconnect();
                }

                label7.Text = "Success! Upload completed!";
                MessageBox.Show("Upload completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                label7.Text = $"Error: {ex.Message}";
                MessageBox.Show("SFTP Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UploadFile(SftpClient sftp, string localFile, string remotePath)
        {
            using (FileStream fs = new FileStream(localFile, FileMode.Open))
            {
                string target = remotePath.TrimEnd('/') + "/" + Path.GetFileName(localFile);
                label7.Text = $"Uploading: {Path.GetFileName(localFile)}";
                sftp.UploadFile(fs, target, true);
            }
        }

        private void UploadMultipleFiles(SftpClient sftp, string localFiles, string remotePath)
        {
            string[] filePaths = localFiles.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < filePaths.Length; i++)
            {
                string filePath = filePaths[i].Trim();
                if (File.Exists(filePath))
                {
                    label7.Text = $"Uploading: {Path.GetFileName(filePath)} ({i + 1}/{filePaths.Length})";
                    UploadFile(sftp, filePath, remotePath);
                }
            }
        }

        private void UploadFolder(SftpClient sftp, string localFolder, string remoteFolder)
        {
            
            try
            {
                sftp.CreateDirectory(remoteFolder);
            }
            catch
            {
               
            }

          
            string[] files = Directory.GetFiles(localFolder);
            foreach (string file in files)
            {
                UploadFile(sftp, file, remoteFolder);
            }

            
            string[] subDirectories = Directory.GetDirectories(localFolder);
            foreach (string subDir in subDirectories)
            {
                string dirName = Path.GetFileName(subDir);
                string remoteSubDir = remoteFolder.TrimEnd('/') + "/" + dirName;
                UploadFolder(sftp, subDir, remoteSubDir);
            }
        }

        private void label8_Click(object sender, EventArgs e)
        {
            
        }

        private void label7_Click(object sender, EventArgs e)
        {
            
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://sabenvome.com/");
        }
    }
}