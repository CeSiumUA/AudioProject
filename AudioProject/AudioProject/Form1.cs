using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;





namespace AudioProject
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region global Variables
        SerialPort _SerialPort = new SerialPort();

        List<byte> sound = new List<byte>();

        DateTime dt;

        string SelectedPort = "";

        string AudioFilePath = "";

        byte[] Buff = null;

        WaveOut _waveOut = new WaveOut();

        bool started = false;

        #endregion



        private void Form1_Load(object sender, EventArgs e)
        {
            #region OLD
            /* try
             {
                 #region Trash
                 sound.Add(0x52);
                 sound.Add(0x49);
                 sound.Add(0x46);
                 sound.Add(0x46);

                 sound.Add(0x24);  //sound.Add(0x00);
                 sound.Add(0x80); //sound.Add(0x00);
                 sound.Add(0xb8); //sound.Add(0x00);
                 sound.Add(0x05); //sound.Add(0x00);

                 sound.Add(0x57);
                 sound.Add(0x41);
                 sound.Add(0x56);
                 sound.Add(0x45);

                 sound.Add(0x66);
                 sound.Add(0x6d);
                 sound.Add(0x74);
                 sound.Add(0x20);

                 sound.Add(0x10);
                 sound.Add(0x00);
                 sound.Add(0x00);
                 sound.Add(0x00);

                 sound.Add(0x01);
                 sound.Add(0x00);
                 sound.Add(0x04); //4
                 sound.Add(0x00);

                 sound.Add(0x40);
                 sound.Add(0x1f);
                 sound.Add(0x00);
                 sound.Add(0x00);

                 sound.Add(0x00);
                 sound.Add(0x7d);
                 sound.Add(0x00);
                 sound.Add(0x00);

                 sound.Add(0x04);
                 sound.Add(0x00);
                 sound.Add(0x08);//8
                 sound.Add(0x00);

                 sound.Add(0x64);
                 sound.Add(0x61);
                 sound.Add(0x74);
                 sound.Add(0x61);

                 sound.Add(0x00);
                 sound.Add(0x80);
                 sound.Add(0xb8);
                 sound.Add(0x05);

                 sound.Add(0x00);
                 sound.Add(0x80);
                 sound.Add(0x00);
                 sound.Add(0x00);



                 #endregion

                 byte[] ByteArray = File.ReadAllBytes("1Prot.dat");


                 for (int x = 0; x < ByteArray.Length; x++)
                 {
                     sound.Add(ByteArray[x]);
                 }

                 File.WriteAllBytes("audiofile.wav", sound.ToArray());
             }
             catch (Exception r)
             {
                 MessageBox.Show(r.ToString());
             }
             */
            #endregion
        }


        public byte[] ByteHeader(int frequency, int byteRate)
        {
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);

                #region OLDHEADER
                /*writer.Write(0x46464952);
                writer.Write(0x059265db);
                writer.Write(0x45564157);
                writer.Write(0x20746D66);
                writer.Write(0x00000010);
                writer.Write(0x00020001); //4

                writer.Write(16000);
                writer.Write(0x00007d00);
                writer.Write(0x00080002); //08
                writer.Write(0x61746164);
                writer.Write(0x0592646c);
               writer.Write(0x00000000);
               */
                #endregion

                writer.Write(0x46464952);
                writer.Write(0x05b88030);
                writer.Write(0x45564157);
                writer.Write(0x20746D66);
                writer.Write(0x00000010);
                writer.Write(0x00020001);

                writer.Write(frequency);  // -- frequency/ //writer.Write(0x00003e80);
                writer.Write(byteRate);  // -- byteRate
                writer.Write(0x00080002);
                writer.Write(0x61746164);
                writer.Write(0x0592646c);
                

                return stream.ToArray();
            }
        }



        private void button1_Click(object sender, EventArgs e)
        {
            Stopwatch sp = Stopwatch.StartNew();
            comboBox1.Text ="";

            

            button1.Text = "Scanning for ports...";

            string[] ports =  SerialPort.GetPortNames();


          
            comboBox1.DataSource = ports;

            LabelStatus.Text = "Scanned Successfully!";
            button1.Text = "Scanned Successfully!";

            sp.Stop();
            labelLatency.Text = sp.ElapsedMilliseconds.ToString() + " ms";

        }
    
        private void Timer()
        {
    
            dt = DateTime.Now;
            Timer tm = new Timer();
            tm.Interval = 10;
            tm.Tick += new EventHandler(tickTimer);
            tm.Start();



        }

        private void tickTimer(object sender, EventArgs e)
        {
            long tick = DateTime.Now.Ticks - dt.Ticks;
            DateTime STOP = new DateTime();
            STOP = STOP.AddTicks(tick);
            label1.Text = String.Format("{0:HH:mm:ss:ff}",STOP);
            
        }

        private void button2_Click(object sender, EventArgs e)
        {


            if (!started)
            {
                started = true;
                button2.Text = "Stop!";
                Stopwatch sp = Stopwatch.StartNew();
                SelectedPort = comboBox1.Text;
                AudioFilePath = Fname.Text;
                AudioFilePath = "1Prot.dat";

                if (radioButton2.Checked)
                {
                    if (SelectedPort == "")
                    {
                        MessageBox.Show("Select COM-port!");
                    }
                    else
                    {
                        PlayCOM();
                    }
                }
                if (radioButton1.Checked)
                {
                    if (AudioFilePath == "")
                    {
                        MessageBox.Show("Select audiofile!");

                    }
                    else
                    {
                        //PlayFile();
                        PlayFileN();
                    }
                }
                if (!radioButton1.Checked & !radioButton2.Checked)
                {
                    MessageBox.Show("Select Playing method!");
                }



                sp.Stop();
                labelLatency.Text = sp.ElapsedMilliseconds.ToString() + " ms";
            }
            else
            {
                _waveOut.Stop();
                started = false;
                LabelStatus.Text = AudioFilePath + " stopped!";
                button2.Text = "Start!";
            }

        }

        private void PlayFile()
        {
            List<byte> sound1 = new List<byte>(ByteHeader(16000, 16000));
 
            byte[] ByteArray = File.ReadAllBytes(AudioFilePath);

            for (int x = 0; x < ByteArray.Length; x++)
            {
                sound1.Add(ByteArray[x]);
            }

            LabelStatus.Text = "File " + AudioFilePath + " loaded successfylly! Playing...";

            File.WriteAllBytes("test.wav", sound1.ToArray());

            Timer();

            using (MemoryStream ms1 = new MemoryStream(sound1.ToArray()))
            {

                SoundPlayer sp = new SoundPlayer(ms1);
                
                sp.Play();
            }

        }
        private void PlayFileN()
        {

            List<byte> soundList = new List<byte>(ByteHeader(8000, 32000));

            byte[] audioBytes = File.ReadAllBytes(AudioFilePath);
            for(int i = 0; i < audioBytes.Length; i++)
            {
                soundList.Add(audioBytes[i]);
            }

            LabelStatus.Text = "File " + AudioFilePath + " loaded successfylly! Playing...";

            File.WriteAllBytes("testN.wav" ,soundList.ToArray());

            Timer();

            IWaveProvider _BufferedWaveProvider = new RawSourceWaveStream(new MemoryStream(soundList.ToArray()), new WaveFormat(16000, 8, 2));
            
            _waveOut.Init(_BufferedWaveProvider);
            _waveOut.Play();

        }

        private void PlayCOM()
        {
            _SerialPort.PortName = Pnumber.Text;
            _SerialPort.Open();
            LabelStatus.Text = Pnumber.Text + " opened successfully! Playing...";
            BufferedWaveProvider _bufferedWaveProvider = new BufferedWaveProvider(new WaveFormat(16000 , 8 , 2));

            int bytes = _SerialPort.BytesToRead;
            Buff = new byte[bytes];
            _SerialPort.Read(Buff, 0, bytes );
            _bufferedWaveProvider.AddSamples(Buff, 0, bytes);

            _waveOut.Init(_bufferedWaveProvider);
            _waveOut.Play();
            

            
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            Fname.Visible = !Fname.Visible;
            Fpath.Visible = !Fpath.Visible;
            Pname.Visible = !Pname.Visible;
            Pnumber.Visible = !Pnumber.Visible;
            Pnumber.Text = comboBox1.Text;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Pnumber.Text = comboBox1.Text;
            
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void drawPlotToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            customWaveViewer1.WaveStream = new WaveFileReader("dot.wav");
            customWaveViewer1.FitToScreen();
        }

        private void fitToScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
         
        }
    }
}

      
    

/*
 using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;
using System.Windows.Forms;
using NAudio.Wave;

namespace structtestapp
{




    class Program
    {

        static void Main(string[] args)
        {




            List<byte> sound = new List<byte>();


            #region Trash
            sound.Add(0x52);
            sound.Add(0x49);
            sound.Add(0x46);
            sound.Add(0x46);
            sound.Add(0x24);
            sound.Add(0x80);
            sound.Add(0xb8);
            sound.Add(0x05);
            sound.Add(0x57);
            sound.Add(0x41);
            sound.Add(0x56);
            sound.Add(0x45);
            sound.Add(0x66);
            sound.Add(0x6d);
            sound.Add(0x74);
            sound.Add(0x20);
            sound.Add(0x10);
            sound.Add(0x00);
            sound.Add(0x00);
            sound.Add(0x00);
            sound.Add(0x01);
            sound.Add(0x00);
            sound.Add(0x02);
                 sound.Add(0x00);
            sound.Add(0x40);
            sound.Add(0x1f);
            sound.Add(0x00);
            sound.Add(0x00);
            sound.Add(0x00);
            sound.Add(0x7d);
            sound.Add(0x00);
            sound.Add(0x00);
            sound.Add(0x04);
            sound.Add(0x00);
            sound.Add(0x10);
            sound.Add(0x00);
            sound.Add(0x64);
            sound.Add(0x61);
            sound.Add(0x74);
            sound.Add(0x61);
            sound.Add(0x00);
            sound.Add(0x80);
            sound.Add(0xb8);
            sound.Add(0x05);
            sound.Add(0x00);
            sound.Add(0x80);
            sound.Add(0x00);
            sound.Add(0x00);
            sound.Add(0x00);
            sound.Add(0x80);
            sound.Add(0x00);
            sound.Add(0x80);
            sound.Add(0x00);
            sound.Add(0x80);
            sound.Add(0x00);
            sound.Add(0x80);
            sound.Add(0x00);
            sound.Add(0x80);
            sound.Add(0x00);
            sound.Add(0x80);
            sound.Add(0x00);
            sound.Add(0x80);
            sound.Add(0x00);
            sound.Add(0x00);


            #endregion


            byte[] bt = File.ReadAllBytes("WH.dat");

            for( int i = 0; i < bt.Length; i++)
            {
                sound.Add(bt[i]);
            }


           // foreach(byte x in sound)
           // {
            //    Console.WriteLine(x);
           // }

             using (Stream stream = new MemoryStream(sound.ToArray()))
             {





                Console.WriteLine("Done");


                SoundPlayer myPlayer = new SoundPlayer(stream);
                
               myPlayer.Play();
             }
            








            Console.ReadKey();






        }

       



    }
}
 
  */
