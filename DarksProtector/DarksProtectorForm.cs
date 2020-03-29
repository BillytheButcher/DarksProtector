using Confuser.Core;
using Confuser.Core.Project;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using Rule = Confuser.Core.Project.Rule;

namespace DarksProtector
{
    public partial class DarksProtectorForm : Form, ILogger
    {
        private readonly DateTime begin;
        private string charset;
        private string rename;

        public DarksProtectorForm()
        {
            InitializeComponent();
            this.Text = "DarksProtector v" + ConfuserEngine.Version;
            lentghtext.Text = "Length: " + lentgh.Value;
        }

        private void Label2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private void DarksProtectorForm_MouseDown(object sender, MouseEventArgs e)
        {
            Capture = false;
            Message msg = Message.Create(Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
            base.WndProc(ref msg);
        }

        private void Label1_MouseDown(object sender, MouseEventArgs e)
        {
            Capture = false;
            Message msg = Message.Create(Handle, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, IntPtr.Zero);
            base.WndProc(ref msg);
        }
        private void thirteenButton1_Click(object sender, EventArgs e) => Environment.Exit(0);

        private void thirteenButton2_Click(object sender, EventArgs e) => WindowState = FormWindowState.Minimized;

        private void thirteenTextBox1_DragDrop(object sender, DragEventArgs e)
        {
            try
            {
                Array array = (Array)e.Data.GetData(DataFormats.FileDrop);
                if (array != null)
                {
                    string path = array.GetValue(0).ToString();
                    int num = path.LastIndexOf(".");
                    if (num != -1)
                    {
                        string extension = path.Substring(num).ToLower();
                        if (extension == ".exe" || extension == ".dll")
                        {
                            Activate();
                            thirteenTextBox1.Text = path;
                        }
                    }
                }
            }
            catch { }
        }

        private void thirteenButton6_Click(object sender, EventArgs e)
        {

            if (thirteenTextBox1.Text == "")
            {
                MessageBox.Show("You need to provide a file !");
            }
            else
            {

                ConfuserProject proj = new ConfuserProject();
                proj.BaseDirectory = Path.GetDirectoryName(thirteenTextBox1.Text);
                proj.OutputDirectory = Path.Combine(Path.GetDirectoryName(thirteenTextBox1.Text) + @"\Protected");


                ProjectModule module = new ProjectModule();
                module.Path = Path.GetFileName(thirteenTextBox1.Text);
                proj.Add(module);
                Rule rule = new Rule("true", ProtectionPreset.None, false);
                if (groupBox1.Enabled == true)
                {
                    if (antiTamper.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("anti tamper", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (antiDebug.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("anti debug", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (antiDump.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("anti dump", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (antiILDasm.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("anti ildasm", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (calli.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Calli Protection", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (constants.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("constants", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (controlFlow.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("ctrl flow", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (invalidMetadat.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("invalid metadata", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (renamer.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("rename", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (refProxy.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("ref proxy", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (cleanRefProxy.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Clean ref proxy", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (moduleFlood.Checked)
                    {
                        SettingItem<Protection> moduleflood = new SettingItem<Protection>("module flood", SettingItemAction.Add);
                        rule.Add(moduleflood);
                    }

                    if (fakeNative.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Fake Native", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    SettingItem<Protection> rename1 = new SettingItem<Protection>("Rename Module", SettingItemAction.Add);
                    rule.Add(rename1);

                    if (mutateConst.Checked == true)
                    {
                        SettingItem<Protection> mutateconst = new SettingItem<Protection>("Mutate Constants", SettingItemAction.Add);
                        rule.Add(mutateconst);
                    }

                    if (mutations.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Mutations", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (localtofield.Checked)
                    {
                        SettingItem<Protection> field2field = new SettingItem<Protection>("lcltofield", SettingItemAction.Add);
                        rule.Add(field2field);
                    }

                    if (hideMethods.Checked)
                    {
                        SettingItem<Protection> entrypoint = new SettingItem<Protection>("Hide Methods", SettingItemAction.Add);
                        rule.Add(entrypoint);
                    }

                    if (md5Checksum.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("checksum", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (disConst.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Const disint", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (junk.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Junk", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (stackUn.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("stack underflow", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (virtualization.Checked)
                    {
                        proj.PluginPaths.Add(Directory.GetCurrentDirectory() + "\\KoiVM.Confuser.exe");
                        SettingItem<Protection> virtualization = new SettingItem<Protection>("virt", SettingItemAction.Add);
                        rule.Add(virtualization);
                    }

                    if (antidefordot.Checked)
                    {
                        SettingItem<Protection> headers = new SettingItem<Protection>("anti de4dot", SettingItemAction.Add);
                        rule.Add(headers);
                    }

                }
                else
                {
                    if (virt.Checked)
                    {
                        string extension = Path.GetExtension(thirteenTextBox1.Text);
                        if (extension == ".dll")
                        {
                            proj.PluginPaths.Add(Directory.GetCurrentDirectory() + "\\KoiVM.Confuser.exe");
                            SettingItem<Protection> virtualization = new SettingItem<Protection>("virt", SettingItemAction.Add);
                            rule.Add(virtualization);
                            SettingItem<Protection> md5 = new SettingItem<Protection>("checksum", SettingItemAction.Add);
                            rule.Add(md5);
                        }
                        else
                        {
                            proj.PluginPaths.Add(Directory.GetCurrentDirectory() + "\\KoiVM.Confuser.exe");
                            SettingItem<Protection> virtualization = new SettingItem<Protection>("virt", SettingItemAction.Add);
                            rule.Add(virtualization);
                            SettingItem<Protection> rename = new SettingItem<Protection>("rename", SettingItemAction.Add);
                            rule.Add(rename);
                            SettingItem<Protection> md5 = new SettingItem<Protection>("checksum", SettingItemAction.Add);
                            rule.Add(md5);
                            SettingItem<Protection> modulere = new SettingItem<Protection>("Rename Module", SettingItemAction.Add);
                            rule.Add(modulere);
                            SettingItem<Protection> debug = new SettingItem<Protection>("anti debug", SettingItemAction.Add);
                            rule.Add(debug);
                        }
                    }
                    else if (virtStrong.Checked)
                    {
                        proj.PluginPaths.Add(Directory.GetCurrentDirectory() + "\\KoiVM.Confuser.exe");
                        SettingItem<Protection> virtualization = new SettingItem<Protection>("virt", SettingItemAction.Add);
                        rule.Add(virtualization);
                        SettingItem<Protection> rename = new SettingItem<Protection>("rename", SettingItemAction.Add);
                        rule.Add(rename);
                        SettingItem<Protection> md5 = new SettingItem<Protection>("checksum", SettingItemAction.Add);
                        rule.Add(md5);
                        SettingItem<Protection> refproxy = new SettingItem<Protection>("Clean ref proxy", SettingItemAction.Add);
                        rule.Add(refproxy);
                        SettingItem<Protection> modulere = new SettingItem<Protection>("Rename Module", SettingItemAction.Add);
                        rule.Add(modulere);
                        SettingItem<Protection> antitamper = new SettingItem<Protection>("anti tamper", SettingItemAction.Add);
                        rule.Add(antitamper);
                    }
                }

                proj.Rules.Add(rule);

                XmlDocument tempdarpr = proj.Save();
                tempdarpr.Save("temp.darkpr");


                Process.Start("Confuser.CLI.exe", "temp.darkpr").WaitForExit();
                File.Delete("temp.darkpr");

            }
        }

        private void thirteenButton3_Click(object sender, EventArgs e)
        {
            OpenFileDialog k = new OpenFileDialog();
            DialogResult result = k.ShowDialog();
            if (result == DialogResult.OK)
            {
                string file = k.FileName;
                thirteenTextBox1.Text = file;
            }
        }

        private void thirteenTextBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Label3_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.gg/wHBexMW");
        }

        #region ConfuserEx Shit

        public static void Input(string text)
        {
            Colorful.Console.Write(string.Format("                         [{0}] ", DateTime.Now), Color.DarkRed);
            Colorful.Console.Write(text + "\n", Color.White);
        }

        public void Log(string msg) => Input(msg);

        public void LogFormat(string format, params object[] args) => Input(string.Format(format, args));

        public void Error(string msg) => Input(msg);

        public void ErrorException(string msg, Exception ex)
        {
            Input("ERROR: " + msg);
            Input("Exception: " + ex);
        }

        public void ErrorFormat(string format, params object[] args) => Input("ERROR: " + string.Format(format, args));

        public void Finish(bool successful)
        {
            DateTime now = DateTime.Now;
            string timeString = string.Format(
                "at {0}, {1}:{2:d2} elapsed.",
                now.ToShortTimeString(),
                (int)now.Subtract(begin).TotalMinutes,
                now.Subtract(begin).Seconds);

            if (successful)
            {
                Input("Finished " + timeString);
            }
            else
            {
                Input("Failed " + timeString);
            }
        }

        public void EndProgress() { }

        public void Progress(int progress, int overall) { }

        #endregion

        #region Checkboxes Changes

        private void best_CheckedChanged(object sender, EventArgs e)
        {
            if (best.Checked == true)
            {
                junk.Checked = true;
                antidefordot.Checked = false;
                antiDebug.Checked = true;
                antiDump.Checked = true;
                antiILDasm.Checked = true;
                antiTamper.Checked = true;
                calli.Checked = true;
                virtualization.Checked = false;
                disConst.Checked = true;
                constants.Checked = true;
                controlFlow.Checked = true;
                fakeNative.Checked = false;
                hideMethods.Checked = true;
                invalidMetadat.Checked = true;
                md5Checksum.Checked = true;
                cleanRefProxy.Checked = false;
                mutations.Checked = false;
                moduleFlood.Checked = true;
                localtofield.Checked = false;
                refProxy.Checked = false;
                renamer.Checked = true;
                resources1.Checked = false;
                mutateConst.Checked = true;
                stackUn.Checked = true;
                virt.Checked = false;
                all.Checked = false;
                minimum.Checked = false;
            }
            else
            {
                clearProtections();
            }

        }

        private void minimum_CheckedChanged(object sender, EventArgs e)
        {
            if (minimum.Checked == true)
            {
                junk.Checked = false;
                antiDebug.Checked = true;
                antidefordot.Checked = false;
                antiDump.Checked = false;
                antiILDasm.Checked = false;
                antiTamper.Checked = true;
                calli.Checked = false;
                virtualization.Checked = false;
                disConst.Checked = false;
                constants.Checked = true;
                controlFlow.Checked = true;
                fakeNative.Checked = false;
                hideMethods.Checked = true;
                invalidMetadat.Checked = true;
                md5Checksum.Checked = false;
                cleanRefProxy.Checked = false;
                mutations.Checked = false;
                moduleFlood.Checked = false;
                localtofield.Checked = false;
                refProxy.Checked = false;
                renamer.Checked = true;
                resources1.Checked = false;
                mutateConst.Checked = false;
                stackUn.Checked = false;
                best.Checked = false;
                all.Checked = false;
                virt.Checked = false;
            }
            else
            {
                clearProtections();
            }
        }

        private void all_CheckedChanged(object sender, EventArgs e)
        {
            if (all.Checked == true)
            {
                junk.Checked = true;
                antiDebug.Checked = true;
                antiDump.Checked = true;
                antiILDasm.Checked = true;
                antidefordot.Checked = true;
                antiTamper.Checked = true;
                calli.Checked = true;
                virtualization.Checked = true;
                disConst.Checked = true;
                constants.Checked = true;
                controlFlow.Checked = true;
                fakeNative.Checked = true;
                hideMethods.Checked = true;
                invalidMetadat.Checked = true;
                md5Checksum.Checked = true;
                cleanRefProxy.Checked = true;
                mutations.Checked = true;
                moduleFlood.Checked = true;
                localtofield.Checked = true;
                refProxy.Checked = true;
                mutateConst.Checked = true;
                renamer.Checked = true;
                resources1.Checked = true;
                stackUn.Checked = true;
                best.Checked = false;
                minimum.Checked = false;
            }
            else
            {
                clearProtections();
            }
        }
        public void clearProtections()
        {
            junk.Checked = false;
            antiDebug.Checked = false;
            antiDump.Checked = false;
            antiILDasm.Checked = false;
            antiTamper.Checked = false;
            calli.Checked = false;
            virtualization.Checked = false;
            disConst.Checked = false;
            constants.Checked = false;
            controlFlow.Checked = false;
            fakeNative.Checked = false;
            hideMethods.Checked = false;
            invalidMetadat.Checked = false;
            md5Checksum.Checked = false;
            cleanRefProxy.Checked = false;
            mutations.Checked = false;
            moduleFlood.Checked = false;
            localtofield.Checked = false;
            refProxy.Checked = false;
            renamer.Checked = false;
            resources1.Checked = false;
            mutateConst.Checked = false;
            stackUn.Checked = false;
            virt.Checked = false;
            virtStrong.Checked = false;
            antidefordot.Checked = false;
        }

        private void Virt_CheckedChanged(object sender, EventArgs e)
        {
            if (virt.Checked == true)
            {
                groupBox1.Enabled = false;
                minimum.Enabled = false;
                best.Enabled = false;
                all.Enabled = false;
                minimum.Checked = false;
                best.Checked = false;
                all.Checked = false;
                virtStrong.Checked = false;
            }
            else if (virt.Checked == false)
            {
                if (virtStrong.Checked != true)
                {
                    groupBox1.Enabled = true;
                    minimum.Enabled = true;
                    best.Enabled = true;
                    all.Enabled = true;
                }
            }
        }

        private void VirtStrong_CheckedChanged(object sender, EventArgs e)
        {
            if (virtStrong.Checked == true)
            {
                groupBox1.Enabled = false;
                minimum.Enabled = false;
                best.Enabled = false;
                all.Enabled = false;
                virt.Checked = false;
                minimum.Checked = false;
                best.Checked = false;
                all.Checked = false;
            }
            else if (virtStrong.Checked == false)
            {
                if (virt.Checked != true)
                {
                    groupBox1.Enabled = true;
                    minimum.Enabled = true;
                    best.Enabled = true;
                    all.Enabled = true;
                }
            }
        }

        private void CheckBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (renameAll.Checked == true)
            {
                renChi.Checked = true;
                renGre.Checked = true;
                renInv.Checked = true;
                renLet.Checked = true;
                renNum.Checked = true;
                renRus.Checked = true;
            }
            else
            {
                renChi.Checked = false;
                renGre.Checked = false;
                renInv.Checked = false;
                renLet.Checked = false;
                renNum.Checked = false;
                renRus.Checked = false;
            }
        }

        #endregion

        #region Refresh Project

        private void MetroButton1_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Projects")))
            {
                listView1.Clear();
                string[] files = Directory.GetFiles(Path.Combine(Environment.CurrentDirectory, "Projects"), "*.darkpr");
                foreach (string file in files)
                {
                    listView1.Items.Add(Path.GetFileName(file));
                }
            }
            else
            {
                MessageBox.Show("Projects folder doesn't exist, creating it...", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Information);
                try
                {
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Projects"));
                }
                catch
                {
                    MessageBox.Show("Error while trying to create projects folder, contact dark#5000 if this isn't normal", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Create Project

        private void MetroButton5_Click(object sender, EventArgs e)
        {
            if (projName.Text == "")
            {
                return;
            }

            if (projName.Text.Length > 30)
            {
                return;
            }

            if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Projects")))
            {
                ConfuserProject proj = new ConfuserProject();


                ProjectModule module = new ProjectModule();
                module.Path = Path.GetFileName(thirteenTextBox1.Text);
                proj.Add(module);
                Rule rule = new Rule("true", ProtectionPreset.None, false);
                if (groupBox1.Enabled == true)
                {
                    if (antiTamper.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("anti tamper", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (antiDebug.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("anti debug", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (antiDump.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("anti dump", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (antiILDasm.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("anti ildasm", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (calli.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Calli Protection", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (constants.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("constants", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (controlFlow.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("ctrl flow", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (invalidMetadat.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("invalid metadata", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (renamer.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("rename", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (refProxy.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("ref proxy", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (cleanRefProxy.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Clean ref proxy", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (moduleFlood.Checked)
                    {
                        SettingItem<Protection> moduleflood = new SettingItem<Protection>("module flood", SettingItemAction.Add);
                        rule.Add(moduleflood);
                    }

                    if (fakeNative.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Fake Native", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    SettingItem<Protection> rename = new SettingItem<Protection>("Rename Module", SettingItemAction.Add);
                    rule.Add(rename);

                    if (mutations.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Mutations", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (mutateConst.Checked == true)
                    {
                        SettingItem<Protection> mutateconst = new SettingItem<Protection>("Mutate Constants", SettingItemAction.Add);
                        rule.Add(mutateconst);
                    }

                    if (localtofield.Checked)
                    {
                        SettingItem<Protection> field2field = new SettingItem<Protection>("lcltofield", SettingItemAction.Add);
                        rule.Add(field2field);
                    }

                    if (hideMethods.Checked)
                    {
                        SettingItem<Protection> entrypoint = new SettingItem<Protection>("Hide Methods", SettingItemAction.Add);
                        rule.Add(entrypoint);
                    }

                    if (md5Checksum.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("checksum", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (disConst.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Const disint", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (junk.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Junk", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (stackUn.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("stack underflow", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (antidefordot.Checked)
                    {
                        SettingItem<Protection> headers = new SettingItem<Protection>("anti de4dot", SettingItemAction.Add);
                        rule.Add(headers);
                    }

                    if (virtualization.Checked)
                    {
                        proj.PluginPaths.Add(Directory.GetCurrentDirectory() + "\\KoiVM.Confuser.exe");
                        SettingItem<Protection> virtualization = new SettingItem<Protection>("virt", SettingItemAction.Add);
                        rule.Add(virtualization);
                    }

                }
                else
                {
                    if (virt.Checked)
                    {
                        proj.PluginPaths.Add(Directory.GetCurrentDirectory() + "\\KoiVM.Confuser.exe");
                        SettingItem<Protection> virtualization = new SettingItem<Protection>("virt", SettingItemAction.Add);
                        rule.Add(virtualization);
                        SettingItem<Protection> rename = new SettingItem<Protection>("rename", SettingItemAction.Add);
                        rule.Add(rename);
                        SettingItem<Protection> md5 = new SettingItem<Protection>("checksum", SettingItemAction.Add);
                        rule.Add(md5);
                        SettingItem<Protection> modulere = new SettingItem<Protection>("Rename Module", SettingItemAction.Add);
                        rule.Add(modulere);
                    }
                    else if (virtStrong.Checked)
                    {
                        proj.PluginPaths.Add(Directory.GetCurrentDirectory() + "\\KoiVM.Confuser.exe");
                        SettingItem<Protection> virtualization = new SettingItem<Protection>("virt", SettingItemAction.Add);
                        rule.Add(virtualization);
                        SettingItem<Protection> rename = new SettingItem<Protection>("rename", SettingItemAction.Add);
                        rule.Add(rename);
                        SettingItem<Protection> md5 = new SettingItem<Protection>("checksum", SettingItemAction.Add);
                        rule.Add(md5);
                        SettingItem<Protection> refproxy = new SettingItem<Protection>("Clean ref proxy", SettingItemAction.Add);
                        rule.Add(refproxy);
                        SettingItem<Protection> modulere = new SettingItem<Protection>("Rename Module", SettingItemAction.Add);
                        rule.Add(modulere);
                        SettingItem<Protection> antitamper = new SettingItem<Protection>("anti tamper", SettingItemAction.Add);
                        rule.Add(antitamper);
                    }
                }

                proj.Rules.Add(rule);

                XmlDocument tempdarpr = proj.Save();
                tempdarpr.Save(Path.Combine(Environment.CurrentDirectory, "Projects", projName.Text + ".darkpr"));
            }
            else
            {
                MessageBox.Show("Projects folder doesn't exist, creating it...", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Information);
                try
                {
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Projects"));
                }
                catch
                {
                    MessageBox.Show("Error while trying to create projects folder, contact dark#5000 if this isn't normal", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Overwrite Project

        private void MetroButton3_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "Projects", listView1.SelectedItems[0].Text)))
            {
                return;
            }

            if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Projects")))
            {
                ConfuserProject proj = new ConfuserProject();


                ProjectModule module = new ProjectModule();
                module.Path = Path.GetFileName(thirteenTextBox1.Text);
                proj.Add(module);
                Rule rule = new Rule("true", ProtectionPreset.None, false);
                if (groupBox1.Enabled == true)
                {
                    if (antiTamper.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("anti tamper", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (antiDebug.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("anti debug", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (antiDump.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("anti dump", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (antiILDasm.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("anti ildasm", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (calli.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Calli Protection", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (constants.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("constants", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (controlFlow.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("ctrl flow", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (invalidMetadat.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("invalid metadata", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (renamer.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("rename", SettingItemAction.Add);
                        rule.Add(protection);
                    }
                    if (refProxy.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("ref proxy", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (cleanRefProxy.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Clean ref proxy", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (moduleFlood.Checked)
                    {
                        SettingItem<Protection> moduleflood = new SettingItem<Protection>("module flood", SettingItemAction.Add);
                        rule.Add(moduleflood);
                    }

                    if (fakeNative.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Fake Native", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    SettingItem<Protection> rename = new SettingItem<Protection>("Rename Module", SettingItemAction.Add);
                    rule.Add(rename);

                    if (mutations.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Mutations", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (mutateConst.Checked == true)
                    {
                        SettingItem<Protection> mutateconst = new SettingItem<Protection>("Mutate Constants", SettingItemAction.Add);
                        rule.Add(mutateconst);
                    }

                    if (localtofield.Checked)
                    {
                        SettingItem<Protection> field2field = new SettingItem<Protection>("lcltofield", SettingItemAction.Add);
                        rule.Add(field2field);
                    }

                    if (hideMethods.Checked)
                    {
                        SettingItem<Protection> entrypoint = new SettingItem<Protection>("Hide Methods", SettingItemAction.Add);
                        rule.Add(entrypoint);
                    }

                    if (md5Checksum.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("checksum", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (disConst.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Const disint", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (junk.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("Junk", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (stackUn.Checked)
                    {
                        SettingItem<Protection> protection = new SettingItem<Protection>("stack underflow", SettingItemAction.Add);
                        rule.Add(protection);
                    }

                    if (virtualization.Checked)
                    {
                        proj.PluginPaths.Add(Directory.GetCurrentDirectory() + "\\KoiVM.Confuser.exe");
                        SettingItem<Protection> virtualization = new SettingItem<Protection>("virt", SettingItemAction.Add);
                        rule.Add(virtualization);
                    }

                    if (antidefordot.Checked)
                    {
                        SettingItem<Protection> headers = new SettingItem<Protection>("anti de4dot", SettingItemAction.Add);
                        rule.Add(headers);
                    }

                }
                else
                {
                    if (virt.Checked)
                    {
                        proj.PluginPaths.Add(Directory.GetCurrentDirectory() + "\\KoiVM.Confuser.exe");
                        SettingItem<Protection> virtualization = new SettingItem<Protection>("virt", SettingItemAction.Add);
                        rule.Add(virtualization);
                        SettingItem<Protection> rename = new SettingItem<Protection>("rename", SettingItemAction.Add);
                        rule.Add(rename);
                        SettingItem<Protection> md5 = new SettingItem<Protection>("checksum", SettingItemAction.Add);
                        rule.Add(md5);
                        SettingItem<Protection> modulere = new SettingItem<Protection>("Rename Module", SettingItemAction.Add);
                        rule.Add(modulere);
                    }
                    else if (virtStrong.Checked)
                    {
                        proj.PluginPaths.Add(Directory.GetCurrentDirectory() + "\\KoiVM.Confuser.exe");
                        SettingItem<Protection> virtualization = new SettingItem<Protection>("virt", SettingItemAction.Add);
                        rule.Add(virtualization);
                        SettingItem<Protection> rename = new SettingItem<Protection>("rename", SettingItemAction.Add);
                        rule.Add(rename);
                        SettingItem<Protection> md5 = new SettingItem<Protection>("checksum", SettingItemAction.Add);
                        rule.Add(md5);
                        SettingItem<Protection> refproxy = new SettingItem<Protection>("Clean ref proxy", SettingItemAction.Add);
                        rule.Add(refproxy);
                        SettingItem<Protection> modulere = new SettingItem<Protection>("Rename Module", SettingItemAction.Add);
                        rule.Add(modulere);
                        SettingItem<Protection> antitamper = new SettingItem<Protection>("anti tamper", SettingItemAction.Add);
                        rule.Add(antitamper);
                    }
                }

                proj.Rules.Add(rule);

                File.Delete(Path.Combine(Environment.CurrentDirectory, "Projects", listView1.SelectedItems[0].Text));
                XmlDocument tempdarpr = proj.Save();
                tempdarpr.Save(Path.Combine(Environment.CurrentDirectory, "Projects", listView1.SelectedItems[0].Text));
            }
            else
            {
                MessageBox.Show("Projects folder doesn't exist, creating it...", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Information);
                try
                {
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Projects"));
                }
                catch
                {
                    MessageBox.Show("Error while trying to create projects folder, contact dark#5000 if this isn't normal", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Load Project

        private void MetroButton2_Click(object sender, EventArgs e)
        {
            if (!File.Exists(Path.Combine(Environment.CurrentDirectory, "Projects", listView1.SelectedItems[0].Text)))
            {
                return;
            }

            string path = Path.Combine(Environment.CurrentDirectory, "Projects", listView1.SelectedItems[0].Text);

            clearProtections();
            ConfuserProject proj = new ConfuserProject();
            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(path);
                proj.Load(xmlDoc);
                ProjectVM project = new ProjectVM(proj, path);

                foreach (ProjectRuleVM r in project.Rules)
                {
                    foreach (ProjectSettingVM<Protection> s in r.Protections)
                    {
                        string name = s.Id;

                        if (name == "anti tamper")
                        {
                            antiTamper.Checked = true;
                        }

                        if (name == "anti debug")
                        {
                            antiDebug.Checked = true;
                        }

                        if (name == "anti dump")
                        {
                            antiDump.Checked = true;
                        }

                        if (name == "anti ildasm")
                        {
                            antiILDasm.Checked = true;
                        }

                        if (name == "Calli Protection")
                        {
                            calli.Checked = true;
                        }

                        if (name == "constants")
                        {
                            constants.Checked = true;
                        }

                        if (name == "ctrl flow")
                        {
                            controlFlow.Checked = true;
                        }

                        if (name == "invalid metadata")
                        {
                            invalidMetadat.Checked = true;
                        }

                        if (name == "rename")
                        {
                            renamer.Checked = true;
                        }

                        if (name == "ref proxy")
                        {
                            refProxy.Checked = true;
                        }

                        if (name == "Clean ref proxy")
                        {
                            cleanRefProxy.Checked = true;
                        }

                        if (name == "module flood")
                        {
                            moduleFlood.Checked = true;
                        }

                        if (name == "Fake Native")
                        {
                            fakeNative.Checked = true;
                        }

                        if (name == "Mutations")
                        {
                            mutations.Checked = true;
                        }

                        if (name == "lcltofield")
                        {
                            localtofield.Checked = true;
                        }

                        if (name == "Hide Methods")
                        {
                            hideMethods.Checked = true;
                        }

                        if (name == "checksum")
                        {
                            md5Checksum.Checked = true;
                        }

                        if (name == "Const disint")
                        {
                            disConst.Checked = true;
                        }

                        if (name == "Junk")
                        {
                            junk.Checked = true;

                        }

                        if (name == "Mutate Constants")
                        {
                            mutateConst.Checked = true;
                        }

                        if (name == "stack underflow")
                        {
                            stackUn.Checked = true;
                        }

                        if (name == "stack underflow")
                        {
                            stackUn.Checked = true;
                        }

                        if (name == "anti de4dot")
                        {
                            antidefordot.Checked = true;
                        }

                        if (name == "virt")
                        {
                            virtualization.Checked = true;
                        }
                    }
                }
            }
            catch { }
        }

        #endregion

        #region Open Projects Folder

        private void MetroButton4_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Projects")))
            {
                Process.Start("explorer.exe", Path.Combine(Environment.CurrentDirectory, "Projects"));
            }
            else
            {
                MessageBox.Show("Projects folder doesn't exist, creating it...", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Information);
                try
                {
                    Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Projects"));
                }
                catch
                {
                    MessageBox.Show("Error while trying to create projects folder, contact dark#5000 if this isn't normal", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        #endregion

        #region Custom Renamer

        private void MetroButton6_Click(object sender, EventArgs e)
        {
            if (prefix.Text == "" && suffix.Text == "" && moduleName.Text == "" && assemblyName.Text == "" && lentgh.Value == 0 && renRus.Checked == false && renNum.Checked == false && renLet.Checked == false && renInv.Checked == false && renGre.Checked == false && renChi.Checked == false)
            {
                return;
            }

            if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Config")))
            {
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Config", "Renamer.txt")))
                {
                    File.Delete(Path.Combine(Environment.CurrentDirectory, "Config", "Renamer.txt"));
                }
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Config", "ModulenAssembly.txt")))
                {
                    File.Delete(Path.Combine(Environment.CurrentDirectory, "Config", "ModulenAssembly.txt"));
                }
                if (prefix.Text == "" && suffix.Text == "" && moduleName.Text != "" && assemblyName.Text != "" && lentgh.Value == 0 && renRus.Checked == false && renNum.Checked == false && renLet.Checked == false && renInv.Checked == false && renGre.Checked == false && renChi.Checked == false)
                {
                    File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "Config", "ModulenAssembly.txt"), string.Concat(moduleName.Text, ":", assemblyName.Text));
                    return;
                }
                if (lentgh.Value == 0)
                {
                    MessageBox.Show("You can't use 0 for the trackbar", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (prefix.Text.Contains(" "))
                {
                    MessageBox.Show("You can't use space in prefix", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                if (suffix.Text.Contains(" "))
                {
                    MessageBox.Show("You can't use space in suffix", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                string chinese = "他是说汉语的ｱ尺乇你他是说汉语的ｱ尺乇你他是说汉语的ｱ尺乇你";
                string letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
                string numbers = "1234567891234567891234567891234567891234567891234567";
                string invisible = "ᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠ";
                string russian = "αβγδεζηθικλμνξοπρστυφχψωαβγδεζηθικλμνξοπρστυφχψωαβγδε";

                charset = "";

                if (renChi.Checked)
                {
                    charset += chinese;
                }

                if (renRus.Checked)
                {
                    charset += russian;
                }

                if (renInv.Checked)
                {
                    charset += invisible;
                }

                if (renNum.Checked)
                {
                    charset += numbers;
                }

                if (renLet.Checked)
                {
                    charset += letters;
                }

                if (renameAll.Checked)
                {
                    charset += chinese + russian + invisible + numbers + letters;
                }

                if (charset == "")
                {
                    MessageBox.Show("You didn't select anything as preset!", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                rename = "";

                if (moduleName.Text != "" && assemblyName.Text != "")
                {
                    File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "Config", "ModulenAssembly.txt"), string.Concat(moduleName.Text, ":", assemblyName.Text));
                }

                StreamWriter streamWriter = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "Config", "Renamer.txt"));
                try
                {
                    for (int i = 0; i < 40000; i++)
                    {
                        streamWriter.WriteLine(prefix.Text + smethod_2(lentgh.Value, charset) + suffix.Text);
                    }
                    return;
                }
                finally
                {
                    ((IDisposable)streamWriter).Dispose();
                }
            }
            else
            {
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Config"));
                MessageBox.Show("Config directory has been created, please reclick on set!", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static readonly Random random = new Random();

        public static string smethod_2(int int_0, string string_10)
        {
            return new string(Enumerable.Repeat<string>(string_10, int_0).Select(new Func<string, char>(method_0)).ToArray<char>());
        }

        private static char method_0(string string_0)
        {
            return string_0[random.Next(string_0.Length)];
        }

        private void Lentgh_ValueChanged(object sender, EventArgs e)
        {
            lentghtext.Text = "Length: " + lentgh.Value;
        }

        private void MetroButton7_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Config")))
            {
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Config", "Renamer.txt")))
                {
                    File.Delete(Path.Combine(Environment.CurrentDirectory, "Config", "Renamer.txt"));
                }
                if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Config", "ModulenAssembly.txt")))
                {
                    File.Delete(Path.Combine(Environment.CurrentDirectory, "Config", "ModulenAssembly.txt"));
                }

                charset = "他是说汉语的ｱ尺乇你他是说汉语的ｱ尺乇你他是说汉语的ｱ尺乇你abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ123456789123456789123456789123456789123456789123456789ᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠαβγδεζηθικλμνξοπρστυφχψωαβγδεζηθικλμνξοπρστυφχψωαβγδεᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠᅠ";

                rename = "";

                File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "Config", "ModulenAssembly.txt"), string.Concat("∂αякsρяσтεcтσя", ":", "∂αякsρяσтεcтσя"));

                StreamWriter streamWriter = new StreamWriter(Path.Combine(Environment.CurrentDirectory, "Config", "Renamer.txt"));
                try
                {
                    for (int i = 0; i < 40000; i++)
                    {
                        streamWriter.WriteLine(prefix.Text + smethod_2(30, charset) + suffix.Text);
                    }
                    return;
                }
                finally
                {
                    ((IDisposable)streamWriter).Dispose();
                }
            }
            else
            {
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Config"));
                MessageBox.Show("Config directory has been created, please reclick on set!", "DarksProtector", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        #endregion

        #region Credits

        private void Label13_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/Lekysha");
            }
            catch { }
        }

        private void PictureBox8_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/Lekysha");
            }
            catch { }
        }

        private void Label12_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/BedTheGod");
            }
            catch { }
        }

        private void PictureBox7_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/BedTheGod");
            }
            catch { }
        }

        private void Label4_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/yck1509");
            }
            catch { }
        }

        private void PictureBox6_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://github.com/yck1509");
            }
            catch { }
        }

        private void Label14_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://cracked.to/");
            }
            catch { }
        }

        private void PictureBox9_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://cracked.to/");
            }
            catch { }
        }

        private void Label10_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://dev/d4rk.fr/");
            }
            catch { }
        }

        private void Label11_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://cto.d4rk.fr/");
            }
            catch { }
        }

        private void Label1_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://cto.d4rk.fr/");
            }
            catch { }
        }

        private void PictureBox3_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://cto.d4rk.fr/");
            }
            catch { }
        }

        private void PictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://cto.d4rk.fr/");
            }
            catch { }
        }

        private void PictureBox4_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start("https://cto.d4rk.fr/");
            }
            catch { }
        }

        private void Label15_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(new WebClient().DownloadString("https://d4rk.fr/projects/darksprotector.cto"));
            }
            catch { }
        }

        #endregion
    }
}
