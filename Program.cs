using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
/*
 * The following will forcefully shutdown a workstation in the domain.
 * Must be executed by an account with admin rights.
 * Just add the workstations you want to shutdown to the workstations file.
 * 
 * To be used as a scheduled task or from the command line.
 * 
 */
namespace RebootWorkstations
{
    class Program
    {
        StreamWriter file;

        public Program()
        {
            file = File.AppendText(@"error.txt");
        }

        static void Main(string[] args)
        {
            Program theBoot = new Program();
            
            //check error log file
            if (!checkErrorFile())
                    createErrorFile();

            if (args.Length > 0)
            {
                if (args[0].ToLower() == "reboot")
                {
                    if (checkRebootFile())
                        theBoot.rebootWorkstations();
                    else
                        logFailure("RebootWorkstations.txt");
                }
                else if (args[0].ToLower() == "shutdown")
                {
                    if (checkShutdownFile())
                        theBoot.shutdownWorkstations();
                    else
                        logFailure("ShutdownWorkstations.txt");
               
                }
                else
                    Console.WriteLine(args[0].ToLower() + " is not a supported switch.");
            }
            else
            {
                Console.WriteLine("Must supply the switch shutdown or reboot.");

                //Log the error in Event Log
                EventLog m_EventLog = new EventLog("");
                m_EventLog.Source = "TheBoot";
                m_EventLog.WriteEntry("Must supply the switch shutdown or reboot.",EventLogEntryType.Warning);
            }
          
        }

        /// <summary>
        /// Reads in the list of workstations from RebootWorkstations.txt and preforms a remote reboot process on each.
        /// </summary>
        public void rebootWorkstations()
        {

            StreamReader fileStream = new StreamReader(@"RebootWorkstations.txt");
            string line = "";
            string error = "";
            ArrayList workstations = new ArrayList();

            while (line != null)
            {
                line = fileStream.ReadLine();
                if (line != null)
                    workstations.Add(line);
            }
            fileStream.Close();

            foreach (string workstation in workstations)
            {
                if (workstation.Length > 0)
                {
                    ProcessStartInfo start = new ProcessStartInfo();
                    start.FileName = @"shutdown.exe"; // Specify exe name.
                    start.UseShellExecute = false;    // do not show command shell
                    start.RedirectStandardOutput = true;
                    start.RedirectStandardError = true;
                    /*
                     *  /r reboot
                     *  /f force
                     *  /t 0 immediately
                     */
                    start.Arguments = "/r /f /t 0 /m \\\\" + workstation;
                    start.WindowStyle = ProcessWindowStyle.Hidden;
                    Process process = Process.Start(start);
                    error = process.StandardError.ReadToEnd();
                    
                    if(error != "")
                        this.logError("Reboot," + error);
                    
                    process.WaitForExit();
                    process.Close();
                }
            }

            file.Close();
        }

        /// <summary>
        /// Writes an error message to the log file 
        /// </summary>
        /// <param name="msg"></param>
        public void logError(string msg)
        {
            try
            {
                file.WriteLine(System.DateTime.Now + "," + msg);
            }
            catch (FileLoadException e)
            {
            }
            
        }

        /// <summary>
        /// Reads in the list of workstations from ShutdownWorkstations.txt and preforms a remote shutdown process on each.
        /// </summary>
        public void shutdownWorkstations()
        {
            StreamReader fileStream = new StreamReader(@"ShutdownWorkstations.txt");
            string line = "";
            string error = "";
            ArrayList workstations = new ArrayList();

            while (line != null)
            {
                line = fileStream.ReadLine();
                if (line != null)
                    workstations.Add(line);
            }
            fileStream.Close();

            foreach (string workstation in workstations)
            {
                if (workstation.Length > 0)
                {
                ProcessStartInfo start = new ProcessStartInfo();
                start.FileName = @"shutdown.exe"; // Specify exe name.
                start.UseShellExecute = false;
                start.RedirectStandardOutput = true;
                start.RedirectStandardError = true;
                start.Arguments = "/s /f /t 0 /m \\\\" + workstation;
                start.WindowStyle = ProcessWindowStyle.Hidden;
                Process process = Process.Start(start);
                error = process.StandardError.ReadToEnd();

                if (error != "")
                    this.logError("Shutdown," + error);

                process.WaitForExit();
                process.Close();
                }
            }

            file.Close();

        }

        /// <summary>
        /// tests to see if the RebootWorkstations.txt file exists.
        /// </summary>
        /// <returns>true if the file exists or false if it does not.</returns>
        static public bool checkRebootFile()
        {
            if (File.Exists("RebootWorkstations.txt"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// tests to see if the ShutdownWorkstations.txt file exists.
        /// </summary>
        /// <returns>true if the file exists or false if it does not.</returns>
        static public bool checkShutdownFile()
        {
            if (File.Exists("ShutdownWorkstations.txt"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// tests to see if the error.txt file exists.
        /// </summary>
        /// <returns>true if the file exists or false if it does not.</returns>
        static public bool checkErrorFile()
        {
            if (File.Exists("error.txt"))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Creates the error log file.
        /// </summary>
        /// <returns>true if the newly created file exists or false if it was not successfully created.</returns>
        static bool createErrorFile()
        {
            StreamWriter errorFile = new StreamWriter(@"error.txt", true);
            if (checkErrorFile())
                return true;
            else
                return false;
        }

        /// <summary>
        /// Creates an entry in the Windows Event Log under Application.
        /// </summary>
        /// <param name="msg">The message that appears in the Windows Event Log.</param>
        static void logFailure(string msg)
        {
            EventLog m_EventLog = new EventLog("");
            m_EventLog.Source = "TheBoot";
            m_EventLog.WriteEntry(msg + " does not exist or is corrupted.",
                EventLogEntryType.Warning);
        }

    }
}
