using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecompileAndCompare
{
    public class DecompileHelper
    {
        private static readonly object _syncObject = new object();

        //Prerequisite : Install JustDecompile from Telric and add the install path to PATH variable
        private string DECOMPILE_EXE_NAME = "";

        public DecompileHelper(string decompileEXEFullPath)
        {
            DECOMPILE_EXE_NAME = decompileEXEFullPath;
        }

        public void Decompile(string dllNameWithFullPath, string outDirectory)
        {
            try
            {
                Log("-->Decompile: " + dllNameWithFullPath + ": " + outDirectory);

                ProcessStartInfo decompileInfo = new ProcessStartInfo();
                decompileInfo.CreateNoWindow = true;
                decompileInfo.RedirectStandardError = true;
                decompileInfo.RedirectStandardOutput = true;
                decompileInfo.FileName = DECOMPILE_EXE_NAME;

                string command = string.Format(@"/lang:csharp /target:" + "\"" + "{0}" + "\"" +  " /out:" + "\"" + "{1}" + "\"" + " /vs:2012 /net4.0 /nodoc /nohex", dllNameWithFullPath, outDirectory);
                Log("Command :" + DECOMPILE_EXE_NAME + " " + command);

                Process decompileProcess = new Process();
                decompileInfo.Arguments = command;
                decompileInfo.WorkingDirectory = Path.GetDirectoryName(DECOMPILE_EXE_NAME); ;
                decompileInfo.UseShellExecute = false;  //To use streams
                decompileProcess.StartInfo = decompileInfo;

                //////Synchronous - during clone it hangs for ever , async fixed and also that logs progress to file
                ////gitProcess.Start();
                ////string stderr_str = gitProcess.StandardError.ReadToEnd();  // pick up STDERR
                ////if (string.IsNullOrEmpty(stderr_str))
                ////    stderr_str = "NIL";
                ////string stdout_str = gitProcess.StandardOutput.ReadToEnd(); // pick up STDOUT
                ////if (string.IsNullOrEmpty(stdout_str))
                ////    stdout_str = "NIL";
                ////Log(
                ////"Command : " + command
                ////+ Environment.NewLine + "Error: " + stderr_str
                ////+ Environment.NewLine + "Output message : " + stdout_str);

                //Async
                decompileProcess.StartInfo.RedirectStandardOutput = true;
                decompileProcess.StartInfo.RedirectStandardError = true;
                //* Set your output and error (asynchronous) handlers
                decompileProcess.OutputDataReceived += new DataReceivedEventHandler(OutputDataHandler);
                decompileProcess.ErrorDataReceived += new DataReceivedEventHandler(OutputErrorHandler);
                //* Start process and handlers
                decompileProcess.Start();
                decompileProcess.BeginOutputReadLine();
                decompileProcess.BeginErrorReadLine();


                decompileProcess.WaitForExit();
                decompileProcess.Close();
                Log("-->DONE - " + command);
            }
            catch (Exception ex)
            {
                Log("Exception in Decompile() : " + ex.Message);
                throw ex;
            }
        }


        private void OutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Log("Output : " + outLine.Data);
        }

        private void OutputErrorHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Log("Error : " + outLine.Data);
        }

        private static void Log(string message)
        {
            lock (_syncObject)
            {
                File.AppendAllText("DecompilerDebug.txt", message + Environment.NewLine);
            }
        }

    }
}
