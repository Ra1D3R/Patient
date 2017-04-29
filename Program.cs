﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using OpenTK;

namespace Patient
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            System.Threading.Thread.CurrentThread.Name = "Patient (main)";
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
