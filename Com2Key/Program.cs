using CameraCapture.tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Com2Key {
    static class Program {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main() {
            bool isCreateOK;
            System.Threading.Mutex run = new System.Threading.Mutex(true,"Com2Key",out isCreateOK);
            if(!isCreateOK) {
                MessageBox.Show("程序已经打开","错误");
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            Application.Run(new FormSerialPortSet());
        }
    }
}
