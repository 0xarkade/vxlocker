using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;

    public class vxMutex
    {
        Mutex _Mutex = null;
        const int MAX_MUTEX_LENGTH = 260;

        [DllImport("user32.dll")]
        static extern int MessageBox(int hWnd, string message, string title, UInt32 type);
        

        ///<summary>
        ///Creates unique program identificator
        ///</summary>
        ///<param name="createmut">Set boolean whether you want to create unique mutex or not.</param>
        ///<param name="MutexName">Set your unique Name for Program Mutex.</param>
        public vxMutex(bool createmut, string MutexName)
        {
            if (createmut && MutexName.Length <= MAX_MUTEX_LENGTH) CheckMutex(MutexName);
        }


        /// <summary>
        /// Procedure to release unique mutex wich is identificator for program
        /// </summary>
        public void ReleaseMutex()
        {
            try
            {
                _Mutex.ReleaseMutex();
            }
            catch { }
        }


       // Procedure checks if our unique ID exists(active) currently or not, if not create's unique ID.
       protected void CheckMutex(string Mname)
       {
             try { 
                 Mutex.OpenExisting(@Mname);
                 Environment.Exit(201);
             }
             catch(WaitHandleCannotBeOpenedException)//Named Mutex doesn't exist, creating new.
             {
                  _Mutex = new Mutex(true, @Mname);
             }
        }

    /*
    * [ Error Codes ]
    * 
    * 201 = Mutex Already Exists.
    * */
    }
