using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Configuration;

namespace DecompileAndCompare
{
    public partial class Decompile : Form
    {
        
        DecompileHelper _decompileHelper = null;
        string _baseOutputDir = null;
        string _beyondComapreEXEPath = "";
        string _decompileEXEFullPath = "";
        string _archiveRelativePath = "";
        
        public Decompile()
        {
            InitializeComponent();
            _baseOutputDir = AppDomain.CurrentDomain.BaseDirectory;
            _beyondComapreEXEPath = ConfigurationManager.AppSettings["CompareToolPath"];
            _decompileEXEFullPath = ConfigurationManager.AppSettings["DecompilerExeFullPath"];
            _decompileHelper = new DecompileHelper(_decompileEXEFullPath);
            _archiveRelativePath = ConfigurationManager.AppSettings["ArchiveLocation"];
        }

        //GET
        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = this.openFileDialog1.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    if (openFileDialog1.FileNames.Count() > 0)
                    {
                        txtLocation1.Text = Path.GetDirectoryName(openFileDialog1.FileNames.First());
                    }
                    else
                    {
                        MessageBox.Show("Please choose Dlls to decompile for Location1");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                updateStatus("Exception : " + ex.Message);
            }

        }

        //PUSH
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult dr = this.openFileDialog2.ShowDialog();
                if (dr == System.Windows.Forms.DialogResult.OK)
                {
                    if (openFileDialog2.FileNames.Count() > 0)
                    {
                        txtLocation2.Text = Path.GetDirectoryName(openFileDialog2.FileNames.First());
                    }
                    else
                    {
                        MessageBox.Show("Please choose Dlls to decompile for Location2");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                updateStatus("Exception : " + ex.Message);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            clearStatus();
            Cursor.Current = Cursors.WaitCursor;
            try
            {

                //archive, if the o/p directory has got some content
                string archiveDir1 = string.Empty;
                string archiveDir2 = string.Empty;

                if (chkArchive.Checked)
                {
                    updateStatus("Starting archive process..");
                    archiveDir1 = Path.Combine(_baseOutputDir, Path.Combine(_archiveRelativePath, "Location1"));
                    archiveDir2 = Path.Combine(_baseOutputDir, Path.Combine(_archiveRelativePath, "Location2"));
                }

                string outputDir1 = Path.Combine(_baseOutputDir, "Location1");
                if (Directory.Exists(outputDir1))
                {
                    //archive 
                    foreach (string sourceDir in Directory.GetDirectories(outputDir1))
                    {
                        updateStatus("Archiving Source : " + sourceDir + " to Destination : " + archiveDir1);
                        var source = new DirectoryInfo(sourceDir);
                        source.CopyTo(archiveDir1, true);
                    }

                    //clean up
                    Directory.Delete(outputDir1, true);
                    Directory.CreateDirectory(outputDir1);
                }

                string outputDir2 = Path.Combine(_baseOutputDir, "Location2");
                if (Directory.Exists(outputDir2))
                {
                    //archive 
                    foreach (string sourceDir in Directory.GetDirectories(outputDir2))
                    {
                        updateStatus("Archiving Source : " + sourceDir + " to Destination : " + archiveDir1);

                        var source = new DirectoryInfo(sourceDir);
                        source.CopyTo(archiveDir2, true);
                    }

                    //clean up
                    Directory.Delete(outputDir2, true);
                    Directory.CreateDirectory(outputDir2);
                }

                updateStatus("Starting Decompile with Location1 :" + Path.GetDirectoryName(openFileDialog1.FileNames.First()));
                // Read the files and decompile
                foreach (String file in openFileDialog1.FileNames)
                {
                    string outputDir = Path.Combine(outputDir1, Path.GetFileNameWithoutExtension(file));
                    updateStatus("Decompiling: " + Path.GetFileName(file));
                    _decompileHelper.Decompile(file, outputDir);

                    //Delete the folder under Location1 which cotains the reference dlls as we don't need them : <Dll>References
                    string refereencesFolder = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(file) + "References");
                    if (Directory.Exists(refereencesFolder))
                    {
                        Directory.Delete(refereencesFolder, true);
                    }
                }

                updateStatus("Done Decompile with Location1. Output at :" + outputDir1);
                updateStatus("Starting Decompile with Location2 :" + Path.GetDirectoryName(openFileDialog2.FileNames.First()));
                // Read the files and decompile
                foreach (String file in openFileDialog2.FileNames)
                {
                    string outputDir = Path.Combine(outputDir2, Path.GetFileNameWithoutExtension(file));
                    updateStatus("Decompiling: " + Path.GetFileName(file));
                    _decompileHelper.Decompile(file, outputDir);

                    //Delete the folder under Location1 which cotains the reference dlls as we don't need them : <Dll>References
                    string refereencesFolder = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(file) + "References");
                    if (Directory.Exists(refereencesFolder))
                    {
                        Directory.Delete(refereencesFolder, true);
                    }
                }
                updateStatus("Done Decompile with Location2. Output at :" + outputDir2);

                //Open Beyond compare to comapre
                string arguments = outputDir1 + " " + outputDir2;
                Process.Start(_beyondComapreEXEPath, arguments);
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an exception while processing. Message : " + ex.Message);
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }

        private void updateStatus(string message)
        {
            textBox1.Text = textBox1.Text + Environment.NewLine + "->" + message;
            textBox1.Refresh();
        }

        private void clearStatus()
        {
            textBox1.Text = "";
            textBox1.Refresh();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitializeOpenFileDialog();
        }

        private void InitializeOpenFileDialog()
        {
            // Set the file dialog to filter for graphics files.
            this.openFileDialog1.Filter =
                "Dlls (*.dll)|*.dll|EXEs (*.exe)|*.exe";

            // Allow the user to select multiple images.
            this.openFileDialog1.Multiselect = true;
            this.openFileDialog1.Title = "Source CVR Binaries, can hoose multiple files";

            // Set the file dialog to filter for graphics files.
            this.openFileDialog2.Filter =
                "Dlls (*.dll)|*.dll|EXEs (*.exe)|*.exe";

            // Allow the user to select multiple images.
            this.openFileDialog2.Multiselect = true;
            this.openFileDialog2.Title = "Target Binaries, can hoose multiple files";
        }

    }
}
