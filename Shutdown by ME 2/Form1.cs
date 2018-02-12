using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Shutdown_by_ME_2
{
    public partial class Form1 : Form
    {
        private enum ScreenType
        {
            Disable,
            Running,
            ShutdownInProcess
        }

        const int OFFSET_SECONDS = 1;
        const string DEFAULT_DATETIME_LABEL = "0.00:00:00";
        DateTime _dt = DateTime.Now;
        Timer _timer;
        bool _enabled;
        bool _shutdownInProcess;
        public Form1()
        {
            InitializeComponent();
            
            _timer = new Timer();
            _timer.Interval = 200;
            _timer.Tick += _timer_Tick;
            _timer.Start();

            Screen(ScreenType.Disable);
        }        

        private void Debug(string message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        private bool IsOffsetValid(DateTime next)
        {
            DateTime offset = DateTime.Now.AddSeconds(OFFSET_SECONDS);

            bool isValid = (offset <= next);

            Debug($"Warning: offset not valid: {offset} <= {next}");

            return isValid;
        }


        private void Screen(ScreenType screenType)
        {
            switch (screenType)
            {
                case ScreenType.Disable:
                    _lblCountdown.Text = DEFAULT_DATETIME_LABEL;
                    _lblShutdownTime.Text = DEFAULT_DATETIME_LABEL;

                    _lblCountdown.Enabled = false;
                    _lblShutdownTime.Enabled = false;
                    _btnSet.Enabled = true;
                    _btnStop.Enabled = false;
                    break;
                case ScreenType.Running:
                    _lblCountdown.Enabled = true;
                    _lblShutdownTime.Enabled = true;
                    _btnSet.Enabled = true;
                    _btnStop.Enabled = true;
                    break;
                case ScreenType.ShutdownInProcess:
                    _lblCountdown.Text = "Starting shutdown...";
                    _lblShutdownTime.Text = "Starting shutdown...";
                    _lblCountdown.Enabled = false ;
                    _lblShutdownTime.Enabled = false;
                    _btnSet.Enabled = false;
                    _btnStop.Enabled = false;
                    _btnExit.Enabled = false;
                    break;
            }                        
        }

        private void AddTimeSpan(TimeSpan timeSpan)
        {
            if (_shutdownInProcess)
                return;

            if (!_enabled)
            {
                _dt = DateTime.Now;
                Debug($"Time reset to now: {_dt}");
            }

            DateTime next = _dt.Add(timeSpan);
         
            if (IsOffsetValid(next))
            {
                _dt = next;
                dateTimePicker1.Value = _dt;
                monthCalendar1.SetDate(_dt);
                dateTimePicker1.Value = _dt;

                if (!_enabled)
                {
                    _enabled = true;
                    Screen(ScreenType.Running);
                    Debug($"Enabled by AddTime. Add: {timeSpan} Shutdown time: " + next.ToString());
                }
            }
            else
            {                
                _enabled = false;
                Screen(ScreenType.Disable);
                Debug($"Disabled by AddTime. Reason: offset not valid.");
            }            
        }

        private void SetTime(DateTime nextTimeOnly)
        {
            if (_shutdownInProcess)
                return;

            if (_enabled)
                return;

            DateTime nextDateTime = new DateTime(_dt.Year, _dt.Month, _dt.Day, nextTimeOnly.Hour, nextTimeOnly.Minute, nextTimeOnly.Second);

            if (IsOffsetValid(nextDateTime))
            {
                _dt = nextDateTime;
                dateTimePicker1.Value = _dt;
                monthCalendar1.SetDate(_dt);
            }
        }

        private void SetDate(DateTime nextDateOnly)
        {
            if (_shutdownInProcess)
                return;

            if (_enabled)
                return;

            DateTime nextDateTime = new DateTime(nextDateOnly.Year, nextDateOnly.Month, nextDateOnly.Day, _dt.Hour, _dt.Minute, _dt.Second);

            if (IsOffsetValid(nextDateOnly))
            {
                _dt = nextDateTime;
                dateTimePicker1.Value = _dt;
                monthCalendar1.SetDate(_dt);
            }
        }

        private void SetDateTime(DateTime next)
        {
            if (_shutdownInProcess)
                return;

            if (IsOffsetValid(next))
            {
                _enabled = true;                
                _dt = next;
                Screen(ScreenType.Running);
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            if (_enabled)
            {
                TimeSpan remainingTime = DateTime.Now.Subtract(_dt);
                _lblCountdown.Text = FormatCountdown(remainingTime);                
                _lblShutdownTime.Text = FormatShutdownTime(_dt);
                
                if (!_shutdownInProcess && DateTime.Now >= _dt)
                {
                    _enabled = false;
                    _shutdownInProcess = true;
                    Screen(ScreenType.ShutdownInProcess);
                    Debug("The end " + (int)remainingTime.TotalSeconds);


                    ProcessStartInfo processStartInfo = new ProcessStartInfo("cmd.exe");
                    processStartInfo.RedirectStandardInput = true;
                    processStartInfo.RedirectStandardOutput = true;
                    processStartInfo.RedirectStandardError = true;
                    processStartInfo.UseShellExecute = false;
                    processStartInfo.CreateNoWindow = true;
                    processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    ProcessStartInfo startInfo = processStartInfo;
                    Process process = Process.Start(startInfo);
                    process.StandardInput.WriteLine("shutdown -s -f -t 1");
                }
            }

            _lblCurrentTime.Text = DateTime.Now.ToString("HH:mm:ss");
        }

        private string FormatCountdown(TimeSpan timespan)
        {
            if (timespan.Days == 0)
                return timespan.ToString(@"hh\:mm\:ss");

            return timespan.ToString(@"d\.hh\:mm\:ss");
        }

        private string FormatShutdownTime(DateTime dt)
        {
            if (DateTime.Now.Date == _dt.Date)
                return dt.ToString("HH:mm:ss");

            if (DateTime.Now.Date.AddDays(1) == _dt.Date)
                return $"Tomorrow at {dt.ToString("HH:mm:ss")}";
            
            return _lblShutdownTime.Text = dt.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void _btn1minP_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(0, 0, 5));
        }        

        private void _btn1minM_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(0, -1, 0));
        }

        private void _btn5minP_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(0, 5, 0));
        }

        private void _btn5minM_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(0, -5, 0));
        }

        private void _btn15minP_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(0, 15, 0));
        }

        private void _btn15minM_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(0, -15, 0));
        }

        private void _btn30minP_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(0, 30, 0));
        }

        private void _btn30minM_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(0, -30, 0));
        }        

        private void _btn1hourP_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(1, 0, 0));
        }

        private void _btn1hourM_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(-1, 0, 0));
        }

        private void _btn6hourP_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(6, 0, 0));
        }

        private void _btn6hourM_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(-6, 0, 0));
        }

        private void _btn12hourP_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(12, 0, 0));
        }

        private void _btn12hourM_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(-12, 0, 0));
        }

        private void _btn1dayP_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(1, 0, 0, 0));
        }

        private void _btn1dayM_Click(object sender, EventArgs e)
        {
            AddTimeSpan(new TimeSpan(-1, 0, 0, 0));
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            SetTime(dateTimePicker1.Value);
        }

        private void monthCalendar1_DateChanged(object sender, DateRangeEventArgs e)
        {
            SetDate(monthCalendar1.SelectionStart);
        }

        private void _btnSet_Click(object sender, EventArgs e)
        {
            DateTime sd = monthCalendar1.SelectionStart;
            DateTime st = dateTimePicker1.Value;
            DateTime next = new DateTime(sd.Year, sd.Month, sd.Day, st.Hour, st.Minute, st.Second);

            SetDateTime(next);
        }

        private void _btnStop_Click(object sender, EventArgs e)
        {
            if (_enabled)
            {
                _enabled = false;
                Screen(ScreenType.Disable);
                Debug($"Timer stopped");
            }            
        }

        private void _btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }      
    }
}
