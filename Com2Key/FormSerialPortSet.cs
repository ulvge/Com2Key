using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CameraCapture.tools {
    public partial class FormSerialPortSet : Form {
        private System.IO.Ports.SerialPort serialPort;
        static string SectorSerialOptions = "SerialOptions";
        const string configName = "com2KeyConfig.ini";
        string CurrentRootPath = "";
        INIClassHelper config_ini;
        private bool isPLCLinkOK = false;
        private bool isFristInitOver = false;
        public FormSerialPortSet() {
            InitializeComponent();
            serialPort = serialPort1;
            CurrentRootPath = Directory.GetCurrentDirectory();//获取当前根目录
            config_ini = new INIClassHelper(CurrentRootPath + "\\" + configName);
        }
        private void FormSerialPortSet_Load(object sender,EventArgs e) {
            displayPara();
            bt_switch_Click(null,null);
            //setCOMMStat(isPLCLinkOK);
            isFristInitOver = true;
            //注意，这里很重要，使用BeginInvoke调用里面的代码将会在Load执行完毕后调用否则没办法实现因为Load的时候窗体还不显示，当然你可以用Shown事件
            this.BeginInvoke(new Action(() => {
                this.notifyIcon1.Visible = true;//显示托盘图标
                this.Hide();//隐藏窗口
                //this.Opacity = 1;
            }));
            startThreadMonitor_sendKeys();
        }
        /// <summary>
        /// 从ini 中后台读取参数
        /// </summary>
        public void readSerialPortPara(){
            string CurrentRootPath = Directory.GetCurrentDirectory();//获取当前根目录
            if(!config_ini.ExistINIFile()) {
                WriteLog.WriteLogFile("串口配置文件丢失,重新生成默认参数_init");
                setAndSave_DefaultSerialPortPara();
            } else {
                try {
                    SeriaPortPara.StopBits = config_ini.IniReadValue(SectorSerialOptions,"StopBits");
                    if(string.IsNullOrEmpty(SeriaPortPara.StopBits)) {//随便拿一个参数作为判断依据 
                        setAndSave_DefaultSerialPortPara();
                    } else {
                        SeriaPortPara.SerialPort = config_ini.IniReadValue(SectorSerialOptions,"SerialPort");
                        SeriaPortPara.BaudRate = config_ini.IniReadValue(SectorSerialOptions,"BaudRate");
                        SeriaPortPara.DataBits = config_ini.IniReadValue(SectorSerialOptions,"DataBits");
                        SeriaPortPara.Parity = config_ini.IniReadValue(SectorSerialOptions,"Parity");
                        SeriaPortPara.StopBits = config_ini.IniReadValue(SectorSerialOptions,"StopBits");
                    }
                } catch(Exception) {
                    setAndSave_DefaultSerialPortPara();
                }
            }
        }

        public void setAndSave_DefaultSerialPortPara() {
            SeriaPortPara.SerialPort = "";
            SeriaPortPara.BaudRate = "9600";
            SeriaPortPara.DataBits = "8";
            SeriaPortPara.Parity = "None";
            SeriaPortPara.StopBits = "1";

            config_ini.IniWriteValue(SectorSerialOptions,"SerialPort",SeriaPortPara.SerialPort);
            config_ini.IniWriteValue(SectorSerialOptions,"BaudRate",SeriaPortPara.BaudRate);
            config_ini.IniWriteValue(SectorSerialOptions,"DataBits",SeriaPortPara.DataBits);
            config_ini.IniWriteValue(SectorSerialOptions,"Parity",SeriaPortPara.Parity);
            config_ini.IniWriteValue(SectorSerialOptions,"StopBits",SeriaPortPara.StopBits);
        }

        /// <summary>
        /// 读取参数，并将参数 显示到控件上
        /// </summary>
        private void displayPara() {
            string[] com_full = SerialPort.GetPortNames();//{"COM1"};//
            
            this.comboBox1.Items.Clear();                 //清空combobox


            if(config_ini.ExistINIFile()) {
                readSerialPortPara();
            }

            this.comboBox1.DataSource = com_full;           //将com_full列表绑定到combobx
            this.comboBox2.Text = SeriaPortPara.BaudRate;
            this.comboBox3.Text = SeriaPortPara.DataBits;
            this.comboBox4.Text = SeriaPortPara.Parity;
            this.comboBox5.Text = SeriaPortPara.StopBits;

            string serialPortName = SeriaPortPara.SerialPort;
            if(string.IsNullOrEmpty(serialPortName)){
                this.comboBox1.SelectedIndex = 0;
            }else{
                for(int index = 0; index < com_full.Length; index++) {
                    if(com_full[index].Equals(serialPortName)) {
                        this.comboBox1.SelectedIndex = index;
                    }
                }
            }
        }
        /// <summary>
        /// UI界面上修改参数后，更新ini，并保存到新的结构体变量之中
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonSave_Click(object sender,EventArgs e) {
            string Current;
            Current = Directory.GetCurrentDirectory();//获取当前根目录
            if(!config_ini.ExistINIFile()) {
                MessageBox.Show("配置文件丢失");
                WriteLog.WriteLogFile("串口配置文件丢失,重新生成默认参数_2");
                return;
            }

            config_ini.IniWriteValue(SectorSerialOptions,"SerialPort",this.comboBox1.Text);
            config_ini.IniWriteValue(SectorSerialOptions,"BaudRate",this.comboBox2.Text);
            config_ini.IniWriteValue(SectorSerialOptions,"DataBits",this.comboBox3.Text);
            config_ini.IniWriteValue(SectorSerialOptions,"Parity",this.comboBox4.Text);
            config_ini.IniWriteValue(SectorSerialOptions,"StopBits",this.comboBox5.Text);
            SeriaPortPara.SerialPort    = this.comboBox1.Text;
            SeriaPortPara.BaudRate      = this.comboBox2.Text;
            SeriaPortPara.DataBits      = this.comboBox3.Text;
            SeriaPortPara.Parity        = this.comboBox4.Text;
            SeriaPortPara.StopBits      = this.comboBox5.Text;
            if( e != EventArgs.Empty)
            MessageBox.Show("参数保存成功");
        }
        public static class SeriaPortPara {
            public static string SerialPort { get; set; }
            public static string BaudRate { get; set; }
            public static string DataBits { get; set; }
            public static string Parity { get; set; }
            public static string StopBits { get; set; }
        }
        /// <summary>
        /// 用参数，并且 打开串口
        /// </summary>
        /// <returns></returns>
        public bool setAndOpen_SerialPortPara(bool isNeedLink,bool isMsgShow) {
            try {
                if(!isNeedLink) {
                    return this.PLCLinkToggle(isNeedLink,isMsgShow);
                }

                readSerialPortPara();
                
                serialPort.PortName = SeriaPortPara.SerialPort;
                serialPort.BaudRate = Convert.ToInt32(SeriaPortPara.BaudRate);//波特率
                serialPort.DataBits = Convert.ToInt32(SeriaPortPara.DataBits);//数据位
                 
                switch(SeriaPortPara.Parity) {
                    case "None":
                        serialPort.Parity = Parity.None;
                        break;
                    case "Odd":
                        serialPort.Parity = Parity.Odd;
                        break;
                    case "Even":
                        serialPort.Parity = Parity.Even;
                        break;
                    default:
                        serialPort.Parity = Parity.Space;
                        break;
                }
                switch(SeriaPortPara.StopBits) {
                    case "1":
                        serialPort.StopBits = StopBits.One;
                        break;
                    case "2":
                        serialPort.StopBits = StopBits.Two;
                        break;
                    default:
                        serialPort.StopBits = StopBits.OnePointFive;
                        break;
                }
                serialPort.DtrEnable = true;
                serialPort.RtsEnable = true;
                serialPort.ReadTimeout = 1000;
                return PLCLinkToggle(isNeedLink,isMsgShow);
            } catch(Exception ex) {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        private void FormSerialPortSet_FormClosing(object sender,FormClosingEventArgs e) {
            e.Cancel = true;    //取消"关闭窗口"事件
            this.WindowState = FormWindowState.Minimized;    //使关闭时窗口向右下角缩小的效果
            notifyIcon1.Visible = true;
            this.Hide();
            return;
        }
       /// <summary>
       /// 打开或关闭串口
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        private void bt_switch_Click(object sender,EventArgs e) {
            //有可能参数已经发生改变 ，先保存一遍
            buttonSave_Click(null,EventArgs.Empty);
            Thread.Sleep(100);
            isPLCLinkOK = !isPLCLinkOK;
            
            //SeriaPortPara.SerialPort

            for(int i = 0; i < comboBox1.Items.Count; i++) {
                if(!setAndOpen_SerialPortPara(isPLCLinkOK,true)) {
                    if(comboBox1.Items[i].Equals("COM1")) { //现场的电脑配置，主板自带一个串口1，故丢弃。
                        continue;
                    }
                    SeriaPortPara.SerialPort = comboBox1.Items[i].ToString();
                    config_ini.IniWriteValue(SectorSerialOptions,"SerialPort",SeriaPortPara.SerialPort);//用默认的参数，一次性打开成功
                }
                else {
                    break;
                }
            }
        }
        /// <summary>
        /// 当参数改变的时候，先关闭串口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void comboBox1_SelectedIndexChanged(object sender,EventArgs e) {
            if(isFristInitOver)
            PLCLinkToggle(false,false);
        }
       
        /// <summary>
        /// 打开或关闭串口，返回当前状态
        /// true,已经打开。flase，关闭
        /// </summary>
        /// <param name="isNeedLink"></param>
        /// <param name="isShowMsg"></param>
        /// <returns></returns>
        public bool PLCLinkToggle(bool isNeedLink,bool isShowMsg) {
            if(isNeedLink) {
                try {
                    if(!serialPort.IsOpen) {
                        serialPort.Open();
                        serialPort.RtsEnable = true;
                        Thread.Sleep(100);
                        serialPort.RtsEnable = false;
                    }
                    isPLCLinkOK = true;
                    if(isShowMsg && isFristInitOver) {
                        //MessageBox.Show("PLC联接成功","成功");
                    }
                    setCOMMStat(isPLCLinkOK);
                } catch(Exception) {
                    if(isShowMsg) {
                        MessageBox.Show("串口打开失败","错误");
                    }
                }
            } else {
                try {
                    if(serialPort.IsOpen) {
                        this.serialPort.Close();
                    }
                    isPLCLinkOK = false;
                    setCOMMStat(isPLCLinkOK);
                } catch(Exception) {
                    if(isShowMsg) {
                        MessageBox.Show("串口: " + serialPort.PortName + " 打开失败");
                    }
                }
            }
            #region
            //if(isPLCLinkOK) {
            //    bt_switch.Text = "关闭";
            //    imgState.BeginInvoke(new MethodInvoker(
            //            delegate {
            //                imgState.Image = global::CameraCapture.Properties.Resources.green;
            //            }
            //        )
            //    );
            //} else {
            //    bt_switch.Text = "打开";
            //    imgState.BeginInvoke(new MethodInvoker(
            //        delegate {
            //            imgState.Image = global::CameraCapture.Properties.Resources.gray;
            //        }
            //    )
            //        );
            //}
            #endregion

            return isPLCLinkOK;
        }
        private void setCOMMStat(bool isOpened) {
            bt_switch.BeginInvoke(new MethodInvoker(
                   delegate {
                       if(isOpened) {
                           bt_switch.Text = "关闭";
                           bt_switch.BackColor = Color.Green;
                       } else {
                           bt_switch.Text = "打开";
                           bt_switch.BackColor = System.Drawing.SystemColors.Control;
                       }
                   }
               )
           );
        }
        public void releaseSerialPort() {
            try {
                if(serialPort.IsOpen) {
                    this.serialPort.Close();
                }
            } catch(Exception) {
            }
        }

        private StringBuilder RecBuff = new StringBuilder();
        private void serialPort1_DataReceived(object sender,SerialDataReceivedEventArgs e) {
            int nReviceBytesNum = serialPort1.BytesToRead; //收到的字节数。
            byte[] ReadBuf = new byte[nReviceBytesNum]; //定义接收字节数组
            serialPort1.Read(ReadBuf,0,nReviceBytesNum);  //接收数据
            string tmp = System.Text.Encoding.ASCII.GetString(ReadBuf);
            //tmp = tmp.TrimEnd('\r');  // \r\n 截取不成功
            lock(RecBuff) {
                RecBuff.Append(tmp);
            }
            if(tmp.Length <= 1) {
                WriteLog.WriteLogFile("收到了错误数据 一个字节:" + tmp);
                return;
            }
            //sendKeys(tmp+Environment.NewLine);
            //sendKeys(tmp);
        }
        Thread ts_Monitor_sendKeys;
        private void startThreadMonitor_sendKeys() {
            ts_Monitor_sendKeys = new Thread(new ThreadStart(CheckPacket_OK));
            ts_Monitor_sendKeys.IsBackground = true;
            ts_Monitor_sendKeys.Priority = ThreadPriority.AboveNormal;
            ts_Monitor_sendKeys.Start();
        }
        
        private static int Last_RecBuff = 0;
        private void CheckPacket_OK() {
            string getStr = string.Empty;
            int len = 0;
            while(true) {
                try {
                    lock(RecBuff){
                        len = RecBuff.Length;
                        if((len != 0) && (Last_RecBuff == len)) {
                            getStr = RecBuff.ToString();
                            RecBuff.Clear();
                        }else {
                            if(len > 4000) {//Max:4096
                                RecBuff.Clear();
                                len = 0;
                            } 
                            Last_RecBuff = len;//更新最新长度
                        }
                    }
                    if(getStr.Length > 0){
                        sendKeys(getStr + '\r');
                        getStr = string.Empty;
                    }
                    if(len == 0) {
                        Thread.Sleep(300);
                    } else {
                        Thread.Sleep(100);
                    }
                } catch(Exception e) {
                    Console.WriteLine(e.Message);
                }
            }
        }




        private void sendKeys(string str) {
            SendKeys.SendWait("(" + str+")");
            
            //模拟按下"ABCDEFG"
            //SendKeys.SendWait("(ABCDEFG)");
            //SendKeys.SendWait("{left 5}");
            //SendKeys.SendWait("{h 10}");

            /*
            更多举例:
            SendKeys.SendWait("^C");  //Ctrl+C 组合键
            SendKeys.SendWait("+C");  //Shift+C 组合键
            SendKeys.SendWait("%C");  //Alt+C 组合键
            SendKeys.SendWait("+(AX)");  //Shift+A+X 组合键
            SendKeys.SendWait("+AX");  //Shift+A 组合键,之后按X键
            SendKeys.SendWait("{left 5}");  //按←键 5次
            SendKeys.SendWait("{h 10}");   //按h键 10次
            SendKeys.Send("汉字");  //模拟输入"汉字"2个字
            */
        }

        private void notifyIcon1_MouseDoubleClick(object sender,MouseEventArgs e) {
            this.comboBox2.Text = SeriaPortPara.BaudRate;
            this.comboBox3.Text = SeriaPortPara.DataBits;
            this.comboBox4.Text = SeriaPortPara.Parity;
            this.comboBox5.Text = SeriaPortPara.StopBits;
            setCOMMStat(isPLCLinkOK);

            notifyIcon1.Visible = true;//显示托盘图标
            this.Show();
            WindowState = FormWindowState.Normal;
            this.Focus();
        }

        private void 退出ToolStripMenuItem_Click(object sender,EventArgs e) {
            // 关闭所有的线程
            ts_Monitor_sendKeys.Abort();
            Thread.Sleep(100);
            this.Dispose();
            this.Close();
        }
    }
}
