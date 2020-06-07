﻿using LibNDSFormats.NSBMD;
using NarcAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Tao.OpenGl;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        #region RM
        ResourceManager rm = new ResourceManager("WindowsFormsApplication1.WinFormStrings", Assembly.GetExecutingAssembly());
        ResourceManager getChar = new ResourceManager("WindowsFormsApplication1.Resources.ReadText", Assembly.GetExecutingAssembly());
        ResourceManager getByte = new ResourceManager("WindowsFormsApplication1.Resources.WriteText", Assembly.GetExecutingAssembly());
        ResourceManager scriptData;
        ResourceManager scriptName;
        ResourceManager scriptNameW;
        ResourceManager MovementName = new ResourceManager("WindowsFormsApplication1.Resources.MovementNames", Assembly.GetExecutingAssembly());
        ResourceManager MovementNameW = new ResourceManager("WindowsFormsApplication1.Resources.MovementNamesW", Assembly.GetExecutingAssembly());
        #endregion

        #region Variables
        public static string ndsFileName;
        public static string workingFolder;
        public static int gameID;
        public long headerCount = 0;
        public long newHeaderCount;
        public int matrixCount;
        public int buildingsCount;
        public int mapCount;
        public static int texturesCount;
        public static int bldTexturesCount;
        public static int bld2TexturesCount;
        public string matrixPath;
        public static string matrixEditorPath;
        public int matrixLayers;
        public int unknownLayer;
        public static int permissionSize;
        public static int buildingsSize;
        public static int modelSize;
        public static int terrainSize;
        public static int unknownSize;
        public static string mapFileName;
        public static int mapIndex;
        public bool saveModeON = false;
        public bool radioModeON = false;
        public bool iconON = false;
        public bool threedON = true;
        public static int mapFlags;
        public static int eventIndex;
        public static int wildIndex;
        public static string textPath;
        public static string scriptPath;
        public static string eventPath;
        public int textCount;
        public int initialKey;
        public int stringCount;
        public int textSections;
        public string modelTileset;
        public string editorTileset;
        public int scriptCount;
        public NSBTX_File nsbtx;
        public MemoryStream mapRAM = new MemoryStream();
        public string file_2 = "";
        public List<List<int>> MovementOffset = new List<List<int>>();
        public List<List<int>> FunctionOffset = new List<List<int>>();
        public List<int> scriptOffset = new List<int>();
        public List<int> useIndex = new List<int>();
        public List<bool> fixMovOffset = new List<bool>();
        Dictionary<int, GameInfo> dictOfGameInfo;
        public static bool soundON = false;
        public static bool isBW = false;
        public static bool isB2W2 = false;
        public static int mapType = 0;
        public static int vmodelOffset;
        public static int vpermOffset;
        public static int vbldOffset;
        public static int vunknownOffset;
        public static int vendOffset;
        public static int voffsetPos;
        public static int offset2 = 0;
        public static int offset3 = 0;
        public List<string> nameText = new List<string>();
        public int currentLine = 0;
        public string searchTerm = "";

        public List<int> useScriptList = new List<int>();
        public List<MemoryStream> scriptList = new List<MemoryStream>();
        public List<List<MemoryStream>> functionList = new List<List<MemoryStream>>();
        public List<List<MemoryStream>> movementList = new List<List<MemoryStream>>();
        public List<Tuple<string, string, string>> scriptsToSearch = new List<Tuple<string, string, string>>();
        #endregion

        public Form1()
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en");
            InitializeComponent();
            //Form10 model = new Form10();
            //model.ShowDialog(this);
            simpleOpenGlControl1.InitializeContexts();
            simpleOpenGlControl2.InitializeContexts();
            simpleOpenGlControl1.MouseWheel += new MouseEventHandler(simpleOpenGlControl1_MouseWheel);
            simpleOpenGlControl2.MouseWheel += new MouseEventHandler(simpleOpenGlControl1_MouseWheel);
            Render();
            tabControl1.TabPages.Remove(tabPage1);
            tabControl1.TabPages.Remove(tabPage22);
            tabControl1.TabPages.Remove(tabPage2);
            tabControl1.TabPages.Remove(tabPage6);
            tabControl1.TabPages.Remove(tabPage7);
            tabControl1.TabPages.Remove(tabPage11);
            tabControl1.TabPages.Remove(tabPage23);
            tabControl1.TabPages.Remove(genVheaderTab);
            tabControl1.TabPages.Remove(tabPage15);
            tabControl1.TabPages.Remove(tabPage17);
            ((TextBox)numericUpDown3.Controls[1]).MaxLength = 2;
            ((TextBox)numericUpDown3.Controls[1]).CharacterCasing = CharacterCasing.Upper;
            //simpleOpenGlControl1.Refresh();
            //simpleOpenGlControl2.Refresh();
        }

        #region Basic Options

        private void quitToolStripMenuItem_Click(object sender, EventArgs e) // Quit
        {
            if (MessageBox.Show(rm.GetString("sureQuit"), rm.GetString("warning"), MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e) // About
        {
            MessageBox.Show("Spiky's DS Map Editor\nVersion 1.8.1\n\nMade by Spiky-Eared Pichu/Markitus95\nSpecial thanks to Arc, who made the NARC API and helped me a lot with C#, and Zark, who helped me with the wild Pokémon editor and lots of data structures\n\n3D renderer and OBJ exporter based on MKDS Course Modifier by Florian", rm.GetString("about"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e) // Delete Extracted Narcs
        {
            Program.gameID = gameID;
            Program.workingFolder = workingFolder;
            Program.isBW = isBW;
            Program.isB2W2 = isB2W2;
        }

        private void editMapViewerColoursToolStripMenuItem_Click(object sender, EventArgs e) // Edit Map Colours
        {
            Process.Start("notepad.exe", @"Data\ColorTable.txt");
        }

        private void editMapViewerColoursToolStripMenuItem_ClickBW(object sender, EventArgs e) // Edit Map Colours
        {
            Process.Start("notepad.exe", @"Data\ColorTableBW.txt");
        }

        #endregion

        private void openToolStripMenuItem_Click(object sender, EventArgs e) // Open ROM
        {
            OpenFileDialog openNDS = new OpenFileDialog();
            openNDS.Title = rm.GetString("selectROM");
            openNDS.Filter = rm.GetString("ndsROM");
            if (openNDS.ShowDialog() == DialogResult.OK)
            {
                ndsFileName = openNDS.FileName;
                tabControl1.TabPages.Remove(tabPage22);
                tabControl1.TabPages.Remove(tabPage2);
                tabControl1.TabPages.Remove(tabPage6);
                tabControl1.TabPages.Remove(tabPage7);
                tabControl1.TabPages.Remove(tabPage11);
                tabControl1.TabPages.Remove(tabPage23);
                tabControl1.TabPages.Remove(genVheaderTab);
                tabControl1.TabPages.Remove(tabPage15);
                tabControl1.TabPages.Remove(tabPage17);
                Form1_FormClosed(null, null);
                System.IO.BinaryReader read = new System.IO.BinaryReader(File.OpenRead(openNDS.FileName));
                read.BaseStream.Position = 0xc;
                gameID = (int)read.ReadUInt32();
                read.Close();
                button3.Enabled = true;
                saveROMToolStripMenuItem.Enabled = true;
                btnSearchScript.Enabled = true;
                button19.Enabled = true;
                button14.Enabled = true;
                button21.Enabled = true;
                radioButton14.Visible = false;
                radioButton15.Visible = false;
                numericUpDown7.Visible = false;
                Column11.MaxInputLength = 3;
                Column12.MaxInputLength = 32767;
                Column11.ReadOnly = false;
                Column12.ReadOnly = true;
                dictOfGameInfo = new Dictionary<int, GameInfo>()
                {
                    { 0x45414441, new GameInfo("usa", "diamond", 0xEEDBC, 0x45414441) },
                    { 0x45415041, new GameInfo("usa", "pearl", 0xEEDBC, 0x45415041) },
                    { 0x45555043, new GameInfo("usa", "platinum", 0xE601C, 0x45555043) },
                    { 0x454B5049, new GameInfo("usa", "heartgold", 0xF6BE0, 0x454B5049) },
                    { 0x45475049, new GameInfo("usa", "soulsilver", 0xF6BE0, 0x45475049) },
                    { 0x4F425249, new GameInfo("usa", "black", 0, 0x4F425249) },
                    { 0x4F415249, new GameInfo("usa", "white", 0, 0x4F415249) },
                    { 0x4F455249, new GameInfo("usa", "black2", 0, 0x4F455249) },
                    { 0x4F445249, new GameInfo("usa", "white2", 0, 0x4F445249) },

                    { 0x53414441, new GameInfo("spa", "diamond", 0xEEE08, 0x53414441) },
                    { 0x53415041, new GameInfo("spa", "pearl", 0xEEE08, 0x53415041) },
                    { 0x53555043, new GameInfo("spa", "platinum", 0xE60B0, 0x53555043) },
                    { 0x534B5049, new GameInfo("spa", "heartgold", 0xF6BC8, 0x534B5049) },
                    { 0x53475049, new GameInfo("spa", "soulsilver", 0xF6BC8, 0x53475049) },
                    { 0x53425249, new GameInfo("spa", "black", 0, 0x53425249) },
                    { 0x53415249, new GameInfo("spa", "white", 0, 0x53415249) },
                    { 0x53455249, new GameInfo("spa", "black2", 0, 0x53455249) },
                    { 0x53445249, new GameInfo("spa", "white2", 0, 0x53445249) },

                    { 0x46414441, new GameInfo("fra", "diamond", 0xEEDFC, 0x46414441) },
                    { 0x46415041, new GameInfo("fra", "pearl", 0xEEDFC, 0x46415041) },
                    { 0x46555043, new GameInfo("fra", "platinum", 0xE60A4, 0x46555043) },
                    { 0x464B5049, new GameInfo("fra", "heartgold", 0xF6BC4, 0x464B5049) },
                    { 0x46475049, new GameInfo("fra", "soulsilver", 0xF6BC4, 0x46475049) },
                    { 0x46425249, new GameInfo("fra", "black", 0, 0x46425249) },
                    { 0x46415249, new GameInfo("fra", "white", 0, 0x46415249) },
                    { 0x46455249, new GameInfo("fra", "black2", 0, 0x46455249) },
                    { 0x46445249, new GameInfo("fra", "white2", 0, 0x46445249) },

                    { 0x49414441, new GameInfo("ita", "diamond", 0xEED70, 0x49414441) },
                    { 0x49415041, new GameInfo("ita", "pearl", 0xEED70, 0x49415041) },
                    { 0x49555043, new GameInfo("ita", "platinum", 0xE6038, 0x49555043) },
                    { 0x494B5049, new GameInfo("ita", "heartgold", 0xF6B58, 0x494B5049) },
                    { 0x49475049, new GameInfo("ita", "soulsilver", 0xF6B58, 0x49475049) },
                    { 0x49425249, new GameInfo("ita", "black", 0, 0x49425249) },
                    { 0x49415249, new GameInfo("ita", "white", 0, 0x49415249) },
                    { 0x49455249, new GameInfo("ita", "black2", 0, 0x49455249) },
                    { 0x49445249, new GameInfo("ita", "white2", 0, 0x49445249) },

                    { 0x44414441, new GameInfo("ger", "diamond", 0xEEDCC, 0x44414441) },
                    { 0x44415041, new GameInfo("ger", "pearl", 0xEEDCC, 0x44415041) },
                    { 0x44555043, new GameInfo("ger", "platinum", 0xE6074, 0x44555043) },
                    { 0x444B5049, new GameInfo("ger", "heartgold", 0xF6B94, 0x444B5049) },
                    { 0x44475049, new GameInfo("ger", "soulsilver", 0xF6B94, 0x44475049) },
                    { 0x44425249, new GameInfo("ger", "black", 0, 0x44425249) },
                    { 0x44415249, new GameInfo("ger", "white", 0, 0x44415249) },
                    { 0x44455249, new GameInfo("ger", "black2", 0, 0x44455249) },
                    { 0x44445249, new GameInfo("ger", "white2", 0, 0x44445249) },

                    { 0x4A414441, new GameInfo("jap", "diamond", 0xF0C28, 0x4A414441) },
                    { 0x4A415041, new GameInfo("jap", "pearl", 0xF0C28, 0x4A415041) },
                    { 0x4A555043, new GameInfo("jap", "platinum", 0xE56F0, 0x4A555043) },
                    { 0x4A4B5049, new GameInfo("jap", "heartgold", 0xF6390, 0x4A4B5049) },
                    { 0x4A475049, new GameInfo("jap", "soulsilver", 0xF6390, 0x4A475049) },
                    { 0x4A425249, new GameInfo("jap", "black", 0, 0x4A425249) },
                    { 0x4A415249, new GameInfo("jap", "white", 0, 0x4A415249) },
                    { 0x4A455249, new GameInfo("jap", "black2", 0, 0x4A455249) },
                    { 0x4A445249, new GameInfo("jap", "white2", 0, 0x4A445249) },

                    { 0x4B414441, new GameInfo("kor", "diamond", 0xEA408, 0x4B414441) },
                    { 0x4B415041, new GameInfo("kor", "pearl", 0xEA408, 0x4B415041) },
                    { 0x4B555043, new GameInfo("kor", "platinum", 0xE6AA4, 0x4B555043) },
                    { 0x4B4B5049, new GameInfo("kor", "heartgold", 0xF728C, 0x4B4B5049) },
                    { 0x4B475049, new GameInfo("kor", "soulsilver", 0xF728C, 0x4B475049) },
                    { 0x4B425249, new GameInfo("kor", "black", 0, 0x4B425249) },
                    { 0x4B415249, new GameInfo("kor", "white", 0, 0x4B415249) },
                    { 0x4B455249, new GameInfo("kor", "black2", 0, 0x4B455249) },
                    { 0x4B445249, new GameInfo("kor", "white2", 0, 0x4B445249) }
                };

                #region DP Support
                if (dictOfGameInfo[gameID].Title.Equals("diamond") || dictOfGameInfo[gameID].Title.Equals("pearl"))
                {
                    loadDP();
                    return;
                }

                #endregion

                #region Platinum Support

                if (dictOfGameInfo[gameID].Title.Equals("platinum"))
                {
                    loadP();
                    return;
                }

                #endregion

                #region HGSS Support

                tabControl1.TabPages.Remove(tabPage1);

                if (dictOfGameInfo[gameID].Title.Equals("heartgold") || dictOfGameInfo[gameID].Title.Equals("soulsilver"))
                {
                    loadHGSS();
                    return;
                }

                #endregion

                #region BW Support
                if (gameID == 0x4F425249) // Black USA
                {
                    label1.Text = rm.GetString("black") + rm.GetString("usa");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                if (gameID == 0x4F415249) // White USA
                {
                    label1.Text = rm.GetString("white") + rm.GetString("usa");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                if (gameID == 0x53425249) // Black SPA
                {
                    label1.Text = rm.GetString("black") + rm.GetString("spa");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                if (gameID == 0x53415249) // White SPA
                {
                    label1.Text = rm.GetString("white") + rm.GetString("spa");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                if (gameID == 0x46425249) // Black FRA
                {
                    label1.Text = rm.GetString("black") + rm.GetString("fra");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                if (gameID == 0x46415249) // White FRA
                {
                    label1.Text = rm.GetString("white") + rm.GetString("fra");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                if (gameID == 0x49425249) // Black ITA
                {
                    label1.Text = rm.GetString("black") + rm.GetString("ita");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                if (gameID == 0x49415249) // White ITA
                {
                    label1.Text = rm.GetString("white") + rm.GetString("ita");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                if (gameID == 0x44425249) // Black GER
                {
                    label1.Text = rm.GetString("black") + rm.GetString("ger");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                if (gameID == 0x44415249) // White GER
                {
                    label1.Text = rm.GetString("white") + rm.GetString("ger");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                if (gameID == 0x4A425249) // Black JAP
                {
                    label1.Text = rm.GetString("black") + rm.GetString("jap");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                if (gameID == 0x4A415249) // White JAP
                {
                    label1.Text = rm.GetString("white") + rm.GetString("jap");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                if (gameID == 0x4B425249) // Black KOR
                {
                    label1.Text = rm.GetString("black") + rm.GetString("kor");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                if (gameID == 0x4B415249) // White KOR
                {
                    label1.Text = rm.GetString("white") + rm.GetString("kor");
                    ndsFileName = openNDS.FileName;
                    loadBW();
                    return;
                }
                #endregion

                #region B2W2 Support
                if (gameID == 0x4F455249) // Black 2 USA
                {
                    label1.Text = rm.GetString("black2") + rm.GetString("usa");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                if (gameID == 0x4F445249) // White 2 USA
                {
                    label1.Text = rm.GetString("white2") + rm.GetString("usa");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                if (gameID == 0x53455249) // Black 2 SPA
                {
                    label1.Text = rm.GetString("black2") + rm.GetString("spa");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                if (gameID == 0x53445249) // White 2 SPA
                {
                    label1.Text = rm.GetString("white2") + rm.GetString("spa");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                if (gameID == 0x46455249) // Black 2 FRA
                {
                    label1.Text = rm.GetString("black2") + rm.GetString("fra");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                if (gameID == 0x46445249) // White 2 FRA
                {
                    label1.Text = rm.GetString("white2") + rm.GetString("fra");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                if (gameID == 0x49455249) // Black 2 ITA
                {
                    label1.Text = rm.GetString("black2") + rm.GetString("ita");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                if (gameID == 0x49445249) // White 2 ITA
                {
                    label1.Text = rm.GetString("white2") + rm.GetString("ita");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                if (gameID == 0x44455249) // Black 2 GER
                {
                    label1.Text = rm.GetString("black2") + rm.GetString("ger");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                if (gameID == 0x44445249) // White 2 GER
                {
                    label1.Text = rm.GetString("white2") + rm.GetString("ger");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                if (gameID == 0x4A455249) // Black 2 JAP
                {
                    label1.Text = rm.GetString("black2") + rm.GetString("jap");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                if (gameID == 0x4A445249) // White 2 JAP
                {
                    label1.Text = rm.GetString("white2") + rm.GetString("jap");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                if (gameID == 0x4B455249) // Black 2 KOR
                {
                    label1.Text = rm.GetString("black2") + rm.GetString("kor");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                if (gameID == 0x4B445249) // White 2 KOR
                {
                    label1.Text = rm.GetString("white2") + rm.GetString("kor");
                    ndsFileName = openNDS.FileName;
                    loadB2W2();
                    return;
                }
                #endregion

                MessageBox.Show(rm.GetString("unsupported"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                saveROMToolStripMenuItem.Enabled = false;
                btnSearchScript.Enabled = false;
                button19.Enabled = false;
                button14.Enabled = false;
                button21.Enabled = false;
                if (isBW || isB2W2)
                {
                    tabControl1.TabPages.Add(genVheaderTab);
                    tabControl1.TabPages.Add(tabPage15);
                    tabControl1.TabPages.Add(tabPage17);
                    tabControl1.TabPages.Add(tabPage7);
                    tabControl1.TabPages.Add(tabPage11);
                    //tabControl1.TabPages.Add(tabPage23);
                    radioButton14.Visible = true;
                    radioButton15.Visible = true;
                    numericUpDown7.Visible = true;
                    button21.Enabled = true;
                }
                if (comboBox2.SelectedIndex != -1)
                {
                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043) tabControl1.TabPages.Add(tabPage22);
                    else tabControl1.TabPages.Add(tabPage1);
                    tabControl1.TabPages.Add(tabPage2);
                    tabControl1.TabPages.Add(tabPage6);
                    tabControl1.TabPages.Add(tabPage7);
                    tabControl1.TabPages.Add(tabPage11);
                    tabControl1.TabPages.Add(tabPage23);
                    saveROMToolStripMenuItem.Enabled = true;
                    btnSearchScript.Enabled = true;
                    button19.Enabled = true;
                    button14.Enabled = true;
                    button21.Enabled = true;
                }
                dataGridView1.Columns[13].HeaderText = rm.GetString("weather");
                dataGridView1.Columns[14].HeaderText = rm.GetString("camera");
                dataGridView1.Columns[15].HeaderText = rm.GetString("nameStyle");
                dataGridView1.Columns[16].HeaderText = rm.GetString("flags");
                Column11.MaxInputLength = 3;
                Column12.MaxInputLength = 32767;
                Column11.ReadOnly = false;
                Column12.ReadOnly = true;
                if (dataGridView1.Columns[17].Visible == true && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                {
                    dataGridView1.Columns[13].HeaderText = rm.GetString("nameFrame");
                    dataGridView1.Columns[14].HeaderText = rm.GetString("weather");
                    dataGridView1.Columns[15].HeaderText = rm.GetString("camera");
                    dataGridView1.Columns[16].HeaderText = rm.GetString("nameStyle");
                    dataGridView1.Columns[17].HeaderText = rm.GetString("flags");
                }
                return;
            }
        }

        private void saveROMToolStripMenuItem_Click(object sender, EventArgs e) // Save ROM
        {
            SaveFileDialog saveNDS = new SaveFileDialog();
            saveNDS.Title = rm.GetString("romSaveTitle");
            saveNDS.Filter = rm.GetString("ndsROM");
            saveNDS.FileName = Path.GetFileNameWithoutExtension(ndsFileName);
            if (saveNDS.ShowDialog() == DialogResult.OK)
            {
                if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4B414441 || gameID == 0x4B415041)
                {
                    toolStripStatusLabel1.Text = rm.GetString("savingRom");
                    Narc.FromFolder(workingFolder + @"data\fielddata\mapmatrix\map_matrix\").Save(workingFolder + @"data\fielddata\mapmatrix\map_matrix.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\land_data\land_data_release").Save(workingFolder + @"data\fielddata\land_data\land_data_release.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\build_model\build_model").Save(workingFolder + @"data\fielddata\build_model\build_model.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set").Save(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset").Save(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset.narc");
                    Narc.FromFolder(workingFolder + @"data\msgdata\msg\").Save(workingFolder + @"data\msgdata\msg.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\script\scr_seq_release").Save(workingFolder + @"data\fielddata\script\scr_seq_release.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\eventdata\zone_event_release").Save(workingFolder + @"data\fielddata\eventdata\zone_event_release.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\areadata\area_data").Save(workingFolder + @"data\fielddata\areadata\area_data.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\areadata\area_build_model\area_build").Save(workingFolder + @"data\fielddata\areadata\area_build_model\area_build.narc");
                    Directory.Delete(workingFolder + @"data\fielddata\mapmatrix\map_matrix", true);
                    Directory.Delete(workingFolder + @"data\fielddata\land_data\land_data_release", true);
                    Directory.Delete(workingFolder + @"data\fielddata\build_model\build_model", true);
                    Directory.Delete(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set", true);
                    Directory.Delete(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset", true);
                    Directory.Delete(workingFolder + @"data\msgdata\msg", true);
                    Directory.Delete(workingFolder + @"data\fielddata\script\scr_seq_release", true);
                    Directory.Delete(workingFolder + @"data\fielddata\eventdata\zone_event_release", true);
                    Directory.Delete(workingFolder + @"data\fielddata\areadata\area_data", true);
                    Directory.Delete(workingFolder + @"data\fielddata\areadata\area_build_model\area_build", true);
                    Process repack = new Process();
                    repack.StartInfo.FileName = @"Data\ndstool.exe";
                    repack.StartInfo.Arguments = "-c " + '"' + saveNDS.FileName + '"' + " -9 " + '"' + workingFolder + "arm9.bin" + '"' + " -7 " + '"' + workingFolder + "arm7.bin" + '"' + " -y9 " + '"' + workingFolder + "y9.bin" + '"' + " -y7 " + '"' + workingFolder + "y7.bin" + '"' + " -d " + '"' + workingFolder + "data" + '"' + " -y " + '"' + workingFolder + "overlay" + '"' + " -t " + '"' + workingFolder + "banner.bin" + '"' + " -h " + '"' + workingFolder + "header.bin" + '"';
                    Application.DoEvents();
                    repack.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    repack.StartInfo.CreateNoWindow = true;
                    repack.Start();
                    repack.WaitForExit();
                    Narc.Open(workingFolder + @"data\fielddata\mapmatrix\map_matrix.narc").ExtractToFolder(workingFolder + @"data\fielddata\mapmatrix\map_matrix");
                    Narc.Open(workingFolder + @"data\fielddata\land_data\land_data_release.narc").ExtractToFolder(workingFolder + @"data\fielddata\land_data\land_data_release");
                    Narc.Open(workingFolder + @"data\fielddata\build_model\build_model.narc").ExtractToFolder(workingFolder + @"data\fielddata\build_model\build_model");
                    Narc.Open(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set");
                    Narc.Open(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset");
                    Narc.Open(workingFolder + @"data\msgdata\msg.narc").ExtractToFolder(workingFolder + @"data\msgdata\msg\");
                    Narc.Open(workingFolder + @"data\fielddata\script\scr_seq_release.narc").ExtractToFolder(workingFolder + @"data\fielddata\script\scr_seq_release");
                    Narc.Open(workingFolder + @"data\fielddata\eventdata\zone_event_release.narc").ExtractToFolder(workingFolder + @"data\fielddata\eventdata\zone_event_release");
                    Narc.Open(workingFolder + @"data\fielddata\areadata\area_data.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_data");
                    Narc.Open(workingFolder + @"data\fielddata\areadata\area_build_model\area_build.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_build_model\area_build");
                    toolStripStatusLabel1.Text = rm.GetString("ready");
                }
                if (gameID == 0x4A414441 || gameID == 0x4A415041)
                {
                    toolStripStatusLabel1.Text = rm.GetString("savingRom");
                    Narc.FromFolder(workingFolder + @"data\fielddata\mapmatrix\map_matrix\").Save(workingFolder + @"data\fielddata\mapmatrix\map_matrix.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\build_model\build_model").Save(workingFolder + @"data\fielddata\build_model\build_model.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set").Save(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset").Save(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset.narc");
                    Narc.FromFolder(workingFolder + @"data\msgdata\msg\").Save(workingFolder + @"data\msgdata\msg.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\script\scr_seq").Save(workingFolder + @"data\fielddata\scr_seq.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\eventdata\zone_event").Save(workingFolder + @"data\fielddata\eventdata\zone_event.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\areadata\area_data").Save(workingFolder + @"data\fielddata\areadata\area_data.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\areadata\area_build_model\area_build").Save(workingFolder + @"data\fielddata\areadata\area_build_model\area_build.narc");
                    Directory.Delete(workingFolder + @"data\fielddata\mapmatrix\map_matrix\", true);
                    Directory.Delete(workingFolder + @"data\fielddata\build_model\build_model", true);
                    Directory.Delete(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set", true);
                    Directory.Delete(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset", true);
                    Directory.Delete(workingFolder + @"data\msgdata\msg", true);
                    Directory.Delete(workingFolder + @"data\fielddata\eventdata\zone_event", true);
                    Directory.Delete(workingFolder + @"data\fielddata\land_data\land_data", true);
                    Directory.Delete(workingFolder + @"data\fielddata\script\scr_seq", true);
                    Directory.Delete(workingFolder + @"data\fielddata\areadata\area_data", true);
                    Directory.Delete(workingFolder + @"data\fielddata\areadata\area_build_model\area_build", true);
                    Process repack = new Process();
                    repack.StartInfo.FileName = @"Data\ndstool.exe";
                    repack.StartInfo.Arguments = "-c " + '"' + saveNDS.FileName + '"' + " -9 " + '"' + workingFolder + "arm9.bin" + '"' + " -7 " + '"' + workingFolder + "arm7.bin" + '"' + " -y9 " + '"' + workingFolder + "y9.bin" + '"' + " -y7 " + '"' + workingFolder + "y7.bin" + '"' + " -d " + '"' + workingFolder + "data" + '"' + " -y " + '"' + workingFolder + "overlay" + '"' + " -t " + '"' + workingFolder + "banner.bin" + '"' + " -h " + '"' + workingFolder + "header.bin" + '"';
                    Application.DoEvents();
                    repack.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    repack.StartInfo.CreateNoWindow = true;
                    repack.Start();
                    repack.WaitForExit();
                    Narc.Open(workingFolder + @"data\fielddata\mapmatrix\map_matrix.narc").ExtractToFolder(workingFolder + @"data\fielddata\mapmatrix\map_matrix");
                    Narc.Open(workingFolder + @"data\fielddata\build_model\build_model.narc").ExtractToFolder(workingFolder + @"data\fielddata\build_model\build_model");
                    Narc.Open(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set");
                    Narc.Open(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset");
                    Narc.Open(workingFolder + @"data\msgdata\msg.narc").ExtractToFolder(workingFolder + @"data\msgdata\msg\");
                    Narc.Open(workingFolder + @"data\fielddata\land_data\land_data.narc").ExtractToFolder(workingFolder + @"data\fielddata\land_data\land_data");
                    Narc.Open(workingFolder + @"data\fielddata\script\scr_seq.narc").ExtractToFolder(workingFolder + @"data\fielddata\script\scr_seq");
                    Narc.Open(workingFolder + @"data\fielddata\eventdata\zone_event.narc").ExtractToFolder(workingFolder + @"data\fielddata\eventdata\zone_event");
                    Narc.Open(workingFolder + @"data\fielddata\areadata\area_data.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_data");
                    Narc.Open(workingFolder + @"data\fielddata\areadata\area_build_model\area_build.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_build_model\area_build");
                    toolStripStatusLabel1.Text = rm.GetString("ready");
                }

                if (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                {
                    toolStripStatusLabel1.Text = rm.GetString("savingRom");
                    Narc.FromFolder(workingFolder + @"data\fielddata\mapmatrix\map_matrix\").Save(workingFolder + @"data\fielddata\mapmatrix\map_matrix.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\land_data\land_data").Save(workingFolder + @"data\fielddata\land_data\land_data.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\build_model\build_model").Save(workingFolder + @"data\fielddata\build_model\build_model.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset").Save(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set").Save(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set.narc");
                    Narc.FromFolder(workingFolder + @"data\msgdata\pl_msg\").Save(workingFolder + @"data\msgdata\pl_msg.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\script\scr_seq").Save(workingFolder + @"data\fielddata\scr_seq.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\eventdata\zone_event").Save(workingFolder + @"data\fielddata\eventdata\zone_event.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\areadata\area_data").Save(workingFolder + @"data\fielddata\areadata\area_data.narc");
                    Narc.FromFolder(workingFolder + @"data\fielddata\areadata\area_build_model\area_build").Save(workingFolder + @"data\fielddata\areadata\area_build_model\area_build.narc");
                    Directory.Delete(workingFolder + @"data\fielddata\mapmatrix\map_matrix\", true);
                    Directory.Delete(workingFolder + @"data\fielddata\land_data\land_data", true);
                    Directory.Delete(workingFolder + @"data\fielddata\build_model\build_model", true);
                    Directory.Delete(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set", true);
                    Directory.Delete(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset", true);
                    Directory.Delete(workingFolder + @"data\msgdata\pl_msg", true);
                    Directory.Delete(workingFolder + @"data\fielddata\script\scr_seq", true);
                    Directory.Delete(workingFolder + @"data\fielddata\eventdata\zone_event", true);
                    Directory.Delete(workingFolder + @"data\fielddata\areadata\area_data", true);
                    Directory.Delete(workingFolder + @"data\fielddata\areadata\area_build_model\area_build", true);
                    Process repack = new Process();
                    repack.StartInfo.FileName = @"Data\ndstool.exe";
                    repack.StartInfo.Arguments = "-c " + '"' + saveNDS.FileName + '"' + " -9 " + '"' + workingFolder + "arm9.bin" + '"' + " -7 " + '"' + workingFolder + "arm7.bin" + '"' + " -y9 " + '"' + workingFolder + "y9.bin" + '"' + " -y7 " + '"' + workingFolder + "y7.bin" + '"' + " -d " + '"' + workingFolder + "data" + '"' + " -y " + '"' + workingFolder + "overlay" + '"' + " -t " + '"' + workingFolder + "banner.bin" + '"' + " -h " + '"' + workingFolder + "header.bin" + '"';
                    Application.DoEvents();
                    repack.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    repack.StartInfo.CreateNoWindow = true;
                    repack.Start();
                    repack.WaitForExit();
                    Narc.Open(workingFolder + @"data\fielddata\mapmatrix\map_matrix.narc").ExtractToFolder(workingFolder + @"data\fielddata\mapmatrix\map_matrix");
                    Narc.Open(workingFolder + @"data\fielddata\land_data\land_data.narc").ExtractToFolder(workingFolder + @"data\fielddata\land_data\land_data");
                    Narc.Open(workingFolder + @"data\fielddata\build_model\build_model.narc").ExtractToFolder(workingFolder + @"data\fielddata\build_model\build_model");
                    Narc.Open(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set");
                    Narc.Open(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset");
                    Narc.Open(workingFolder + @"data\msgdata\pl_msg.narc").ExtractToFolder(workingFolder + @"data\msgdata\pl_msg\");
                    Narc.Open(workingFolder + @"data\fielddata\script\scr_seq.narc").ExtractToFolder(workingFolder + @"data\fielddata\script\scr_seq");
                    Narc.Open(workingFolder + @"data\fielddata\eventdata\zone_event.narc").ExtractToFolder(workingFolder + @"data\fielddata\eventdata\zone_event");
                    Narc.Open(workingFolder + @"data\fielddata\areadata\area_data.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_data");
                    Narc.Open(workingFolder + @"data\fielddata\areadata\area_build_model\area_build.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_build_model\area_build");
                    toolStripStatusLabel1.Text = rm.GetString("ready");
                }
                if (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049)
                {
                    toolStripStatusLabel1.Text = rm.GetString("savingRom");
                    Narc.FromFolder(workingFolder + @"data\a\0\4\matrix\").Save(workingFolder + @"data\a\0\4\1");
                    Narc.FromFolder(workingFolder + @"data\a\0\6\map\").Save(workingFolder + @"data\a\0\6\5");
                    Narc.FromFolder(workingFolder + @"data\a\0\4\building\").Save(workingFolder + @"data\a\0\4\0");
                    Narc.FromFolder(workingFolder + @"data\a\0\4\texture\").Save(workingFolder + @"data\a\0\4\4");
                    Narc.FromFolder(workingFolder + @"data\a\0\7\textureBld\").Save(workingFolder + @"data\a\0\7\0");
                    Narc.FromFolder(workingFolder + @"data\a\0\2\text\").Save(workingFolder + @"data\a\0\2\7");
                    Narc.FromFolder(workingFolder + @"data\a\0\1\script\").Save(workingFolder + @"data\a\0\1\2");
                    Narc.FromFolder(workingFolder + @"data\a\0\3\event\").Save(workingFolder + @"data\a\0\3\2");
                    Directory.Delete(workingFolder + @"data\a\0\4\matrix\", true);
                    Directory.Delete(workingFolder + @"data\a\0\6\map\", true);
                    Directory.Delete(workingFolder + @"data\a\0\4\building\", true);
                    Directory.Delete(workingFolder + @"data\a\0\4\texture\", true);
                    Directory.Delete(workingFolder + @"data\a\0\7\textureBld\", true);
                    Directory.Delete(workingFolder + @"data\a\0\2\text\", true);
                    Directory.Delete(workingFolder + @"data\a\0\1\script\", true);
                    Directory.Delete(workingFolder + @"data\a\0\3\event\", true);
                    Process repack = new Process();
                    repack.StartInfo.FileName = @"Data\ndstool.exe";
                    repack.StartInfo.Arguments = "-c " + '"' + saveNDS.FileName + '"' + " -9 " + '"' + workingFolder + "arm9.bin" + '"' + " -7 " + '"' + workingFolder + "arm7.bin" + '"' + " -y9 " + '"' + workingFolder + "y9.bin" + '"' + " -y7 " + '"' + workingFolder + "y7.bin" + '"' + " -d " + '"' + workingFolder + "data" + '"' + " -y " + '"' + workingFolder + "overlay" + '"' + " -t " + '"' + workingFolder + "banner.bin" + '"' + " -h " + '"' + workingFolder + "header.bin" + '"';
                    Application.DoEvents();
                    repack.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    repack.StartInfo.CreateNoWindow = true;
                    repack.Start();
                    repack.WaitForExit();
                    Narc.Open(workingFolder + @"data\a\0\4\1").ExtractToFolder(workingFolder + @"data\a\0\4\matrix");
                    Narc.Open(workingFolder + @"data\a\0\6\5").ExtractToFolder(workingFolder + @"data\a\0\6\map");
                    Narc.Open(workingFolder + @"data\a\0\4\0").ExtractToFolder(workingFolder + @"data\a\0\4\building");
                    Narc.Open(workingFolder + @"data\a\0\4\4").ExtractToFolder(workingFolder + @"data\a\0\4\texture");
                    Narc.Open(workingFolder + @"data\a\0\7\0").ExtractToFolder(workingFolder + @"data\a\0\7\textureBld");
                    Narc.Open(workingFolder + @"data\a\0\2\7").ExtractToFolder(workingFolder + @"data\a\0\2\text");
                    Narc.Open(workingFolder + @"data\a\0\1\2").ExtractToFolder(workingFolder + @"data\a\0\1\script");
                    Narc.Open(workingFolder + @"data\a\0\3\2").ExtractToFolder(workingFolder + @"data\a\0\3\event");
                    Process decompress = new Process();
                    decompress.StartInfo.FileName = @"Data\blz.exe";
                    decompress.StartInfo.Arguments = @" -d " + '"' + workingFolder + "arm9.bin" + '"';
                    decompress.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    decompress.StartInfo.CreateNoWindow = true;
                    decompress.Start();
                    decompress.WaitForExit();
                    toolStripStatusLabel1.Text = rm.GetString("ready");
                }
                if (isBW == true)
                {
                    toolStripStatusLabel1.Text = rm.GetString("savingRom");
                    Narc.FromFolder(workingFolder + @"data\a\0\0\maps\").Save(workingFolder + @"data\a\0\0\8");
                    Narc.FromFolder(workingFolder + @"data\a\0\1\headers\").Save(workingFolder + @"data\a\0\1\2");
                    Narc.FromFolder(workingFolder + @"data\a\0\0\matrix\").Save(workingFolder + @"data\a\0\0\9");
                    Narc.FromFolder(workingFolder + @"data\a\0\1\tilesets\").Save(workingFolder + @"data\a\0\1\4");
                    Narc.FromFolder(workingFolder + @"data\a\1\7\bldtilesets").Save(workingFolder + @"data\a\1\7\6");
                    Narc.FromFolder(workingFolder + @"data\a\1\7\bld2tilesets").Save(workingFolder + @"data\a\1\7\7");
                    Narc.FromFolder(workingFolder + @"data\a\0\0\texts\").Save(workingFolder + @"data\a\0\0\2");
                    Narc.FromFolder(workingFolder + @"data\a\0\0\texts2\").Save(workingFolder + @"data\a\0\0\3");
                    Narc.FromFolder(workingFolder + @"data\a\0\5\scripts\").Save(workingFolder + @"data\a\0\5\7");
                    Directory.Delete(workingFolder + @"data\a\0\0\maps\", true);
                    Directory.Delete(workingFolder + @"data\a\0\1\headers\", true);
                    Directory.Delete(workingFolder + @"data\a\0\0\matrix\", true);
                    Directory.Delete(workingFolder + @"data\a\0\1\tilesets\", true);
                    Directory.Delete(workingFolder + @"data\a\1\7\bldtilesets", true);
                    Directory.Delete(workingFolder + @"data\a\1\7\bld2tilesets", true);
                    Directory.Delete(workingFolder + @"data\a\0\0\texts\", true);
                    Directory.Delete(workingFolder + @"data\a\0\0\texts2\", true);
                    Directory.Delete(workingFolder + @"data\a\0\5\scripts\", true);
                    Process repack = new Process();
                    repack.StartInfo.FileName = @"Data\ndstool.exe";
                    repack.StartInfo.Arguments = "-c " + '"' + saveNDS.FileName + '"' + " -9 " + '"' + workingFolder + "arm9.bin" + '"' + " -7 " + '"' + workingFolder + "arm7.bin" + '"' + " -y9 " + '"' + workingFolder + "y9.bin" + '"' + " -y7 " + '"' + workingFolder + "y7.bin" + '"' + " -d " + '"' + workingFolder + "data" + '"' + " -y " + '"' + workingFolder + "overlay" + '"' + " -t " + '"' + workingFolder + "banner.bin" + '"' + " -h " + '"' + workingFolder + "header.bin" + '"';
                    Application.DoEvents();
                    repack.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    repack.StartInfo.CreateNoWindow = true;
                    repack.Start();
                    repack.WaitForExit();
                    Narc.Open(workingFolder + @"data\a\0\0\8").ExtractToFolder(workingFolder + @"data\a\0\0\maps");
                    Narc.Open(workingFolder + @"data\a\0\1\2").ExtractToFolder(workingFolder + @"data\a\0\1\headers");
                    Narc.Open(workingFolder + @"data\a\0\0\9").ExtractToFolder(workingFolder + @"data\a\0\0\matrix");
                    Narc.Open(workingFolder + @"data\a\0\1\4").ExtractToFolder(workingFolder + @"data\a\0\1\tilesets");
                    Narc.Open(workingFolder + @"data\a\1\7\6").ExtractToFolder(workingFolder + @"data\a\1\7\bldtilesets");
                    Narc.Open(workingFolder + @"data\a\1\7\7").ExtractToFolder(workingFolder + @"data\a\1\7\bld2tilesets");
                    Narc.Open(workingFolder + @"data\a\0\0\2").ExtractToFolder(workingFolder + @"data\a\0\0\texts");
                    Narc.Open(workingFolder + @"data\a\0\0\3").ExtractToFolder(workingFolder + @"data\a\0\0\texts2");
                    Narc.Open(workingFolder + @"data\a\0\5\7").ExtractToFolder(workingFolder + @"data\a\0\5\scripts");
                    toolStripStatusLabel1.Text = rm.GetString("ready");
                }
                if (isB2W2 == true)
                {
                    toolStripStatusLabel1.Text = rm.GetString("savingRom");
                    Narc.FromFolder(workingFolder + @"data\a\0\0\maps\").Save(workingFolder + @"data\a\0\0\8");
                    Narc.FromFolder(workingFolder + @"data\a\0\1\headers\").Save(workingFolder + @"data\a\0\1\2");
                    Narc.FromFolder(workingFolder + @"data\a\0\0\matrix\").Save(workingFolder + @"data\a\0\0\9");
                    Narc.FromFolder(workingFolder + @"data\a\0\1\tilesets\").Save(workingFolder + @"data\a\0\1\4");
                    Narc.FromFolder(workingFolder + @"data\a\1\7\bldtilesets").Save(workingFolder + @"data\a\1\7\4");
                    Narc.FromFolder(workingFolder + @"data\a\1\7\bld2tilesets").Save(workingFolder + @"data\a\1\7\5");
                    Narc.FromFolder(workingFolder + @"data\a\0\0\texts\").Save(workingFolder + @"data\a\0\0\2");
                    Narc.FromFolder(workingFolder + @"data\a\0\0\texts2\").Save(workingFolder + @"data\a\0\0\3");
                    Narc.FromFolder(workingFolder + @"data\a\0\5\scripts\").Save(workingFolder + @"data\a\0\5\6");
                    Directory.Delete(workingFolder + @"data\a\0\0\maps\", true);
                    Directory.Delete(workingFolder + @"data\a\0\1\headers\", true);
                    Directory.Delete(workingFolder + @"data\a\0\0\matrix\", true);
                    Directory.Delete(workingFolder + @"data\a\0\1\tilesets\", true);
                    Directory.Delete(workingFolder + @"data\a\1\7\bldtilesets", true);
                    Directory.Delete(workingFolder + @"data\a\1\7\bld2tilesets", true);
                    Directory.Delete(workingFolder + @"data\a\0\0\texts\", true);
                    Directory.Delete(workingFolder + @"data\a\0\0\texts2\", true);
                    Directory.Delete(workingFolder + @"data\a\0\5\scripts\", true);
                    Process repack = new Process();
                    repack.StartInfo.FileName = @"Data\ndstool.exe";
                    repack.StartInfo.Arguments = "-c " + '"' + saveNDS.FileName + '"' + " -9 " + '"' + workingFolder + "arm9.bin" + '"' + " -7 " + '"' + workingFolder + "arm7.bin" + '"' + " -y9 " + '"' + workingFolder + "y9.bin" + '"' + " -y7 " + '"' + workingFolder + "y7.bin" + '"' + " -d " + '"' + workingFolder + "data" + '"' + " -y " + '"' + workingFolder + "overlay" + '"' + " -t " + '"' + workingFolder + "banner.bin" + '"' + " -h " + '"' + workingFolder + "header.bin" + '"';
                    Application.DoEvents();
                    repack.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    repack.StartInfo.CreateNoWindow = true;
                    repack.Start();
                    repack.WaitForExit();
                    Narc.Open(workingFolder + @"data\a\0\0\8").ExtractToFolder(workingFolder + @"data\a\0\0\maps");
                    Narc.Open(workingFolder + @"data\a\0\1\2").ExtractToFolder(workingFolder + @"data\a\0\1\headers");
                    Narc.Open(workingFolder + @"data\a\0\0\9").ExtractToFolder(workingFolder + @"data\a\0\0\matrix");
                    Narc.Open(workingFolder + @"data\a\0\1\4").ExtractToFolder(workingFolder + @"data\a\0\1\tilesets");
                    Narc.Open(workingFolder + @"data\a\1\7\4").ExtractToFolder(workingFolder + @"data\a\1\7\bldtilesets");
                    Narc.Open(workingFolder + @"data\a\1\7\5").ExtractToFolder(workingFolder + @"data\a\1\7\bld2tilesets");
                    Narc.Open(workingFolder + @"data\a\0\0\2").ExtractToFolder(workingFolder + @"data\a\0\0\texts");
                    Narc.Open(workingFolder + @"data\a\0\0\3").ExtractToFolder(workingFolder + @"data\a\0\0\texts2");
                    Narc.Open(workingFolder + @"data\a\0\5\6").ExtractToFolder(workingFolder + @"data\a\0\5\scripts");
                    toolStripStatusLabel1.Text = rm.GetString("ready");
                }
            }
        }

        private void loadDP() // Initialize DP
        {
            Program.ApplicationExit(null, null);
            isBW = false;
            isB2W2 = false;
            dataGridView1.Columns[17].Visible = false;
            dataGridView1.Columns[18].Visible = false;
            sPKPackagesToolStripMenuItem.Enabled = true;
            dataGridView1.Rows.Clear();
            label1.Text = rm.GetString(dictOfGameInfo[gameID].Title) + rm.GetString(dictOfGameInfo[gameID].Region);
            workingFolder = Path.GetDirectoryName(ndsFileName) + "\\" + Path.GetFileNameWithoutExtension(ndsFileName) + "_SDSME" + "\\";
            loadLastRom();
            iconON = true; pictureBox1.Refresh();
            toolStripStatusLabel1.Text = rm.GetString("extractPackage");
            if (dictOfGameInfo[gameID].Region.Equals("jap"))
            {
                Narc.Open(workingFolder + @"data\fielddata\land_data\land_data.narc").ExtractToFolder(workingFolder + @"data\fielddata\land_data\land_data");
                Narc.Open(workingFolder + @"data\fielddata\script\scr_seq.narc").ExtractToFolder(workingFolder + @"data\fielddata\script\scr_seq");
                Narc.Open(workingFolder + @"data\fielddata\eventdata\zone_event.narc").ExtractToFolder(workingFolder + @"data\fielddata\eventdata\zone_event");
            }
            else
            {
                Narc.Open(workingFolder + @"data\fielddata\land_data\land_data_release.narc").ExtractToFolder(workingFolder + @"data\fielddata\land_data\land_data_release");
                Narc.Open(workingFolder + @"data\fielddata\script\scr_seq_release.narc").ExtractToFolder(workingFolder + @"data\fielddata\script\scr_seq_release");
                Narc.Open(workingFolder + @"data\fielddata\eventdata\zone_event_release.narc").ExtractToFolder(workingFolder + @"data\fielddata\eventdata\zone_event_release");
            }
            Narc.Open(workingFolder + @"data\fielddata\mapmatrix\map_matrix.narc").ExtractToFolder(workingFolder + @"data\fielddata\mapmatrix\map_matrix");
            Narc.Open(workingFolder + @"data\fielddata\build_model\build_model.narc").ExtractToFolder(workingFolder + @"data\fielddata\build_model\build_model");
            Narc.Open(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set");
            Narc.Open(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset");
            Narc.Open(workingFolder + @"data\msgdata\msg.narc").ExtractToFolder(workingFolder + @"data\msgdata\msg");
            Narc.Open(workingFolder + @"data\fielddata\areadata\area_data.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_data");
            Narc.Open(workingFolder + @"data\fielddata\areadata\area_build_model\area_build.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_build_model\area_build");
            if (dictOfGameInfo[gameID].Region.Equals("jap"))
            {
                eventPath = Form1.workingFolder + @"data\fielddata\eventdata\zone_event";
                scriptCount = Directory.GetFiles(workingFolder + @"data\fielddata\script\scr_seq").Length;
                mapCount = Directory.GetFiles(workingFolder + @"data\fielddata\land_data\land_data").Length;
            }
            else
            {
                eventPath = Form1.workingFolder + @"data\fielddata\eventdata\zone_event_release";
                scriptCount = Directory.GetFiles(workingFolder + @"data\fielddata\script\scr_seq_release").Length;
                mapCount = Directory.GetFiles(workingFolder + @"data\fielddata\land_data\land_data_release").Length;
            }
            matrixCount = Directory.GetFiles(workingFolder + @"data\fielddata\mapmatrix\map_matrix").Length;
            buildingsCount = Directory.GetFiles(workingFolder + @"data\fielddata\build_model\build_model").Length;
            texturesCount = Directory.GetFiles(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set").Length;
            bldTexturesCount = Directory.GetFiles(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset").Length;
            textCount = Directory.GetFiles(workingFolder + @"data\msgdata\msg").Length;
            toolStripStatusLabel1.Text = rm.GetString("ready");
            headerCount = 0;
            headerCount = new System.IO.FileInfo(workingFolder + @"data\fielddata\maptable\mapname.bin").Length / 16;
            MessageBox.Show(rm.GetString("headersFound") + headerCount, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            toolStripStatusLabel1.Text = rm.GetString("loadingHeaders");
            System.IO.BinaryReader readArm9 = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"arm9.bin"));
            dataGridView1.Enabled = true;
            readArm9.BaseStream.Position = dictOfGameInfo[gameID].ReadOffset;
            System.IO.BinaryReader readMapTable = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\fielddata\maptable\mapname.bin"));
            int rowNumber = 0;
            nameText.Clear();
            #region Names
            BinaryReader readText;
            if (dictOfGameInfo[gameID].Region.Equals("jap"))
                readText = new System.IO.BinaryReader(File.OpenRead(Form1.workingFolder + @"data\msgdata\msg\0374"));
            else if (dictOfGameInfo[gameID].Region.Equals("kor"))
                readText = new System.IO.BinaryReader(File.OpenRead(Form1.workingFolder + @"data\msgdata\msg\0376"));
            else
                readText = new System.IO.BinaryReader(File.OpenRead(Form1.workingFolder + @"data\msgdata\msg\0382"));
            int stringCount = (int)readText.ReadUInt16();
            int initialKey = (int)readText.ReadUInt16();
            int key1 = (initialKey * 0x2FD) & 0xFFFF;
            int key2 = 0;
            int realKey = 0;
            bool specialCharON = false;
            int[] currentOffset = new int[stringCount];
            int[] currentSize = new int[stringCount];
            string[] currentPokemon = new string[stringCount];
            int car = 0;
            for (int i = 0; i < stringCount; i++) // Reads and stores string offsets and sizes
            {
                key2 = (key1 * (i + 1) & 0xFFFF);
                realKey = key2 | (key2 << 16);
                currentOffset[i] = ((int)readText.ReadUInt32()) ^ realKey;
                currentSize[i] = ((int)readText.ReadUInt32()) ^ realKey;
            }
            for (int i = 0; i < stringCount; i++) // Adds new string
            {
                key1 = (0x91BD3 * (i + 1)) & 0xFFFF;
                readText.BaseStream.Position = currentOffset[i];
                string pokemonText = "";
                for (int j = 0; j < currentSize[i]; j++) // Adds new characters to string
                {
                    car = ((int)readText.ReadUInt16()) ^ key1;
                    #region Special Characters
                    if (car == 0xE000 || car == 0x25BC || car == 0x25BD || car == 0xFFFE || car == 0xFFFF)
                    {
                        if (car == 0xE000)
                        {
                            pokemonText += @"\n";
                        }
                        if (car == 0x25BC)
                        {
                            pokemonText += @"\r";
                        }
                        if (car == 0x25BD)
                        {
                            pokemonText += @"\f";
                        }
                        if (car == 0xFFFE)
                        {
                            pokemonText += @"\v";
                            specialCharON = true;
                        }
                        if (car == 0xFFFF)
                        {
                            pokemonText += "";
                        }
                    }
                    #endregion
                    else
                    {
                        if (specialCharON == true)
                        {
                            pokemonText += car.ToString("X4");
                            specialCharON = false;
                        }
                        else
                        {
                            string character = getChar.GetString(car.ToString("X4"));
                            pokemonText += character;
                            if (character == null)
                            {
                                pokemonText += @"\x" + car.ToString("X4");
                            }
                        }
                    }
                    key1 += 0x493D;
                    key1 &= 0xFFFF;
                }
                nameText.Add(pokemonText);
            }
            readText.Close();
            #endregion
            string mapNames;
            for (int i = 0; i < headerCount; i++)
            {
                mapNames = "";
                for (int nameLength = 0; nameLength < 16; nameLength++)
                {
                    int currentByte = readMapTable.ReadByte();
                    byte[] mapBytes = new Byte[] { Convert.ToByte(currentByte) }; // Reads map name
                    if (currentByte != 0) mapNames = mapNames + Encoding.UTF8.GetString(mapBytes);
                }
                dataGridView1.Rows.Add(rowNumber, mapNames, readArm9.ReadByte(), readArm9.ReadByte(), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), nameText[readArm9.ReadUInt16()], readArm9.ReadByte(), readArm9.ReadByte(), readArm9.ReadByte(), readArm9.ReadByte()); // Adds header data to grid
                dataGridView1.Rows[rowNumber].HeaderCell.Value = i.ToString();
                dataGridView1.Rows[rowNumber].ReadOnly = false;
                rowNumber++;
            }
            matrixPath = workingFolder + @"data\fielddata\mapmatrix\map_matrix\";
            if (dictOfGameInfo[gameID].Region.Equals("jap"))
                mapFileName = workingFolder + @"data\fielddata\land_data\land_data\";
            else
                mapFileName = workingFolder + @"data\fielddata\land_data\land_data_release\";
            button1.Enabled = true;
            readArm9.Close();
            readMapTable.Close();
            newHeaderCount = headerCount;
            comboBox1.Items.Clear();
            for (int i = 0; i < matrixCount; i++)
            {
                comboBox1.Items.Add(rm.GetString("matrix") + i);
            }
            comboBox1.SelectedIndex = 0;
            comboBox2.Items.Clear();

            comboBox4.Items.Clear();
            listBox1.Items.Clear();
            comboBox4.Items.Add(rm.GetString("untextured"));
            for (int i = 0; i < texturesCount; i++)
            {
                comboBox4.Items.Add(rm.GetString("tileset") + i);
                listBox1.Items.Add(rm.GetString("tileset") + i);
            }
            listBox1.SelectedIndex = 0;
            #region Read Map Names
            for (int i = 0; i < mapCount; i++)
            {
                string nsbmdName = "";
                BinaryReader readNames;
                if (dictOfGameInfo[gameID].Region.Equals("jap"))
                    readNames = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\fielddata\land_data\land_data" + "\\" + i.ToString("D4")));
                else
                    readNames = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\fielddata\land_data\land_data_release" + "\\" + i.ToString("D4")));
                permissionSize = (int)readNames.ReadUInt32();
                buildingsSize = (int)readNames.ReadUInt32();
                readNames.BaseStream.Position = 0x10 + permissionSize + buildingsSize + 0x34;
                for (int nameLength = 0; nameLength < 16; nameLength++)
                {
                    int currentByte = readNames.ReadByte();
                    byte[] mapBytes = new Byte[] { Convert.ToByte(currentByte) }; // Reads map name
                    if (currentByte != 0) nsbmdName = nsbmdName + Encoding.UTF8.GetString(mapBytes);
                }
                comboBox2.Items.Add(i + ": " + nsbmdName);
                readNames.Close();
            }
            #endregion

            comboBox2.SelectedIndex = 0;
            if (tabControl1.TabPages.Count == 0) tabControl1.TabPages.Add(tabPage1);
            tabControl1.TabPages.Add(tabPage2);
            tabControl1.TabPages.Add(tabPage6);
            tabControl1.TabPages.Add(tabPage7);
            tabControl1.TabPages.Add(tabPage11);
            tabControl1.TabPages.Add(tabPage23);
            comboBox1_SelectedIndexChanged(null, null);
            comboBox2_SelectedIndexChanged(null, null);
            comboBox3.Items.Clear();
            for (int i = 0; i < textCount; i++)
            {
                comboBox3.Items.Add(rm.GetString("text") + i);
            }
            comboBox3.SelectedIndex = 0;
            comboBox5.Items.Clear();
            comboBox9.Items.Clear();
            comboBox10.Items.Clear();
            comboBox12.Items.Clear();
            comboBox13.Items.Clear();
            for (int i = 0; i < scriptCount; i++)
            {
                comboBox5.Items.Add(rm.GetString("script") + i);
            }
            comboBox5.SelectedIndex = 0;
            for (int i = 0; i < Directory.GetFiles(eventPath).Length; i++)
            {
                comboBox10.Items.Add(rm.GetString("eventList") + i);
            }
            comboBox10.SelectedIndex = 0;
            for (int i = 0; i < Directory.GetFiles(workingFolder + @"data\fielddata\areadata\area_data").Length; i++)
            {
                comboBox12.Items.Add(rm.GetString("areaDataList") + i);
            }
            comboBox12.SelectedIndex = 0;
            for (int i = 0; i < Directory.GetFiles(workingFolder + @"data\fielddata\areadata\area_build_model\area_build").Length; i++)
            {
                comboBox13.Items.Add(rm.GetString("buildingPackList") + i);
            }
            readBldNames(workingFolder + @"data\fielddata\build_model\build_model.narc");
            comboBox13.SelectedIndex = 0;
        }

        private void loadP() // Initialize Platinum
        {
            Program.ApplicationExit(null, null);
            isBW = false;
            isB2W2 = false;
            dataGridView1.Columns[17].Visible = true;
            dataGridView1.Columns[18].Visible = false;
            dataGridView1.Columns[13].HeaderText = rm.GetString("nameFrame");
            dataGridView1.Columns[14].HeaderText = rm.GetString("weather");
            dataGridView1.Columns[15].HeaderText = rm.GetString("camera");
            dataGridView1.Columns[16].HeaderText = rm.GetString("nameStyle");
            dataGridView1.Columns[17].HeaderText = rm.GetString("flags");
            sPKPackagesToolStripMenuItem.Enabled = true;
            dataGridView1.Rows.Clear();
            label1.Text = rm.GetString(dictOfGameInfo[gameID].Title) + rm.GetString(dictOfGameInfo[gameID].Region);
            workingFolder = Path.GetDirectoryName(ndsFileName) + "\\" + Path.GetFileNameWithoutExtension(ndsFileName) + "_SDSME" + "\\";
            loadLastRom();
            iconON = true; pictureBox1.Refresh();
            toolStripStatusLabel1.Text = rm.GetString("extractPackage");
            Narc.Open(workingFolder + @"data\fielddata\mapmatrix\map_matrix.narc").ExtractToFolder(workingFolder + @"data\fielddata\mapmatrix\map_matrix");
            Narc.Open(workingFolder + @"data\fielddata\land_data\land_data.narc").ExtractToFolder(workingFolder + @"data\fielddata\land_data\land_data");
            Narc.Open(workingFolder + @"data\fielddata\build_model\build_model.narc").ExtractToFolder(workingFolder + @"data\fielddata\build_model\build_model");
            Narc.Open(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set");
            Narc.Open(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset");
            Narc.Open(workingFolder + @"data\msgdata\pl_msg.narc").ExtractToFolder(workingFolder + @"data\msgdata\pl_msg");
            Narc.Open(workingFolder + @"data\fielddata\areadata\area_data.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_data");
            Narc.Open(workingFolder + @"data\fielddata\areadata\area_build_model\area_build.narc").ExtractToFolder(workingFolder + @"data\fielddata\areadata\area_build_model\area_build");
            Narc.Open(workingFolder + @"data\fielddata\script\scr_seq.narc").ExtractToFolder(workingFolder + @"data\fielddata\script\scr_seq");
            Narc.Open(Form1.workingFolder + @"data\fielddata\eventdata\zone_event.narc").ExtractToFolder(Form1.workingFolder + @"data\fielddata\eventdata\zone_event");
            eventPath = Form1.workingFolder + @"data\fielddata\eventdata\zone_event";
            scriptCount = Directory.GetFiles(workingFolder + @"data\fielddata\script\scr_seq").Length;
            matrixCount = Directory.GetFiles(workingFolder + @"data\fielddata\mapmatrix\map_matrix").Length;
            mapCount = Directory.GetFiles(workingFolder + @"data\fielddata\land_data\land_data").Length;
            buildingsCount = Directory.GetFiles(workingFolder + @"data\fielddata\build_model\build_model").Length;
            texturesCount = Directory.GetFiles(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set").Length;
            bldTexturesCount = Directory.GetFiles(workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset").Length;
            textCount = Directory.GetFiles(workingFolder + @"data\msgdata\pl_msg").Length;
            toolStripStatusLabel1.Text = rm.GetString("ready");
            headerCount = 0;
            headerCount = new System.IO.FileInfo(workingFolder + @"data\fielddata\maptable\mapname.bin").Length / 16;
            MessageBox.Show(rm.GetString("headersFound") + headerCount, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            System.IO.BinaryReader readArm9 = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"arm9.bin"));
            dataGridView1.Enabled = true;
            readArm9.BaseStream.Position = dictOfGameInfo[gameID].ReadOffset;
            System.IO.BinaryReader readMapTable = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\fielddata\maptable\mapname.bin"));
            int rowNumber = 0;
            nameText.Clear();
            #region Names
            BinaryReader readText;
            if (dictOfGameInfo[gameID].Region.Equals("jap"))
            {
                readText = new System.IO.BinaryReader(File.OpenRead(Form1.workingFolder + @"data\msgdata\pl_msg\0427"));
            }
            else if (dictOfGameInfo[gameID].Region.Equals("kor"))
            {
                readText = new System.IO.BinaryReader(File.OpenRead(Form1.workingFolder + @"data\msgdata\pl_msg\0428"));
            }
            else
            {
                readText = new System.IO.BinaryReader(File.OpenRead(Form1.workingFolder + @"data\msgdata\pl_msg\0433"));
            }
            int stringCount = (int)readText.ReadUInt16();
            int initialKey = (int)readText.ReadUInt16();
            int key1 = (initialKey * 0x2FD) & 0xFFFF;
            int key2 = 0;
            int realKey = 0;
            bool specialCharON = false;
            int[] currentOffset = new int[stringCount];
            int[] currentSize = new int[stringCount];
            string[] currentPokemon = new string[stringCount];
            int car = 0;
            for (int i = 0; i < stringCount; i++) // Reads and stores string offsets and sizes
            {
                key2 = (key1 * (i + 1) & 0xFFFF);
                realKey = key2 | (key2 << 16);
                currentOffset[i] = ((int)readText.ReadUInt32()) ^ realKey;
                currentSize[i] = ((int)readText.ReadUInt32()) ^ realKey;
            }
            for (int i = 0; i < stringCount; i++) // Adds new string
            {
                key1 = (0x91BD3 * (i + 1)) & 0xFFFF;
                readText.BaseStream.Position = currentOffset[i];
                string pokemonText = "";
                for (int j = 0; j < currentSize[i]; j++) // Adds new characters to string
                {
                    car = ((int)readText.ReadUInt16()) ^ key1;
                    #region Special Characters
                    if (car == 0xE000 || car == 0x25BC || car == 0x25BD || car == 0xFFFE || car == 0xFFFF)
                    {
                        if (car == 0xE000)
                        {
                            pokemonText += @"\n";
                        }
                        if (car == 0x25BC)
                        {
                            pokemonText += @"\r";
                        }
                        if (car == 0x25BD)
                        {
                            pokemonText += @"\f";
                        }
                        if (car == 0xFFFE)
                        {
                            pokemonText += @"\v";
                            specialCharON = true;
                        }
                        if (car == 0xFFFF)
                        {
                            pokemonText += "";
                        }
                    }
                    #endregion
                    else
                    {
                        if (specialCharON == true)
                        {
                            pokemonText += car.ToString("X4");
                            specialCharON = false;
                        }
                        else
                        {
                            string character = getChar.GetString(car.ToString("X4"));
                            pokemonText += character;
                            if (character == null)
                            {
                                pokemonText += @"\x" + car.ToString("X4");
                            }
                        }
                    }
                    key1 += 0x493D;
                    key1 &= 0xFFFF;
                }
                nameText.Add(pokemonText);
            }
            readText.Close();
            #endregion
            string mapNames;
            for (int i = 0; i < headerCount; i++)
            {
                mapNames = "";
                for (int nameLength = 0; nameLength < 16; nameLength++)
                {
                    int currentByte = readMapTable.ReadByte();
                    byte[] mapBytes = new Byte[] { Convert.ToByte(currentByte) }; // Reads map name
                    if (currentByte != 0) mapNames = mapNames + Encoding.UTF8.GetString(mapBytes);
                }
                dataGridView1.Rows.Add(rowNumber, mapNames, readArm9.ReadByte(), readArm9.ReadByte(), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), nameText[readArm9.ReadByte()], readArm9.ReadByte(), readArm9.ReadByte(), readArm9.ReadByte(), readArm9.ReadByte(), readArm9.ReadByte()); // Adds header data to grid
                dataGridView1.Rows[rowNumber].HeaderCell.Value = i.ToString();
                dataGridView1.Rows[rowNumber].ReadOnly = false;
                rowNumber++;
            }
            matrixPath = workingFolder + @"data\fielddata\mapmatrix\map_matrix\";
            mapFileName = workingFolder + @"data\fielddata\land_data\land_data\";
            button1.Enabled = true;
            readArm9.Close();
            readMapTable.Close();
            newHeaderCount = headerCount;
            comboBox1.Items.Clear();
            for (int i = 0; i < matrixCount; i++)
            {
                comboBox1.Items.Add(rm.GetString("matrix") + i);
            }
            comboBox1.SelectedIndex = 0;
            comboBox2.Items.Clear();

            comboBox4.Items.Clear();
            listBox1.Items.Clear();
            comboBox4.Items.Add(rm.GetString("untextured"));
            for (int i = 0; i < texturesCount; i++)
            {
                comboBox4.Items.Add(rm.GetString("tileset") + i);
                listBox1.Items.Add(rm.GetString("tileset") + i);
            }
            listBox1.SelectedIndex = 0;
            #region Read Map Names
            for (int i = 0; i < mapCount; i++)
            {
                string nsbmdName = "";
                System.IO.BinaryReader readNames = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\fielddata\land_data\land_data" + "\\" + i.ToString("D4")));
                permissionSize = (int)readNames.ReadUInt32();
                buildingsSize = (int)readNames.ReadUInt32();
                readNames.BaseStream.Position = 0x10 + permissionSize + buildingsSize + 0x34;
                for (int nameLength = 0; nameLength < 16; nameLength++)
                {
                    int currentByte = readNames.ReadByte();
                    byte[] mapBytes = new Byte[] { Convert.ToByte(currentByte) }; // Reads map name
                    if (currentByte != 0) nsbmdName = nsbmdName + Encoding.UTF8.GetString(mapBytes);
                }
                comboBox2.Items.Add(i + ": " + nsbmdName);
                readNames.Close();
            }
            #endregion

            comboBox2.SelectedIndex = 0;
            if (tabControl1.TabPages.Count == 0) tabControl1.TabPages.Add(tabPage1);
            tabControl1.TabPages.Add(tabPage2);
            tabControl1.TabPages.Add(tabPage6);
            tabControl1.TabPages.Add(tabPage7);
            tabControl1.TabPages.Add(tabPage11);
            tabControl1.TabPages.Add(tabPage23);
            comboBox1_SelectedIndexChanged(null, null);
            comboBox2_SelectedIndexChanged(null, null);
            comboBox3.Items.Clear();
            for (int i = 0; i < textCount; i++)
            {
                comboBox3.Items.Add(rm.GetString("text") + i);
            }
            comboBox3.SelectedIndex = 0;
            comboBox5.Items.Clear();
            comboBox9.Items.Clear();
            comboBox10.Items.Clear();
            comboBox12.Items.Clear();
            for (int i = 0; i < scriptCount; i++)
            {
                comboBox5.Items.Add(rm.GetString("script") + i);
            }
            comboBox5.SelectedIndex = 0;
            for (int i = 0; i < Directory.GetFiles(eventPath).Length; i++)
            {
                comboBox10.Items.Add(rm.GetString("eventList") + i);
            }
            comboBox10.SelectedIndex = 0;
            for (int i = 0; i < Directory.GetFiles(workingFolder + @"data\fielddata\areadata\area_data").Length; i++)
            {
                comboBox12.Items.Add(rm.GetString("areaDataList") + i);
            }
            comboBox12.SelectedIndex = 0;
            for (int i = 0; i < Directory.GetFiles(workingFolder + @"data\fielddata\areadata\area_build_model\area_build").Length; i++)
            {
                comboBox13.Items.Add(rm.GetString("buildingPackList") + i);
            }
            readBldNames(workingFolder + @"data\fielddata\build_model\build_model.narc");
            comboBox13.SelectedIndex = 0;
        }

        private void loadHGSS() // Initialize HGSS
        {
            Program.ApplicationExit(null, null);
            isBW = false;
            isB2W2 = false;
            sPKPackagesToolStripMenuItem.Enabled = true;
            dataGridView13.Rows.Clear();
            label1.Text = rm.GetString(dictOfGameInfo[gameID].Title) + rm.GetString(dictOfGameInfo[gameID].Region);
            workingFolder = Path.GetDirectoryName(ndsFileName) + "\\" + Path.GetFileNameWithoutExtension(ndsFileName) + "_SDSME" + "\\";
            loadLastRom();
            iconON = true; pictureBox1.Refresh();
            toolStripStatusLabel1.Text = rm.GetString("extractPackage");
            Narc.Open(workingFolder + @"data\a\0\4\1").ExtractToFolder(workingFolder + @"data\a\0\4\matrix");
            Narc.Open(workingFolder + @"data\a\0\6\5").ExtractToFolder(workingFolder + @"data\a\0\6\map");
            Narc.Open(workingFolder + @"data\a\0\4\0").ExtractToFolder(workingFolder + @"data\a\0\4\building");
            Narc.Open(workingFolder + @"data\a\0\4\4").ExtractToFolder(workingFolder + @"data\a\0\4\texture");
            Narc.Open(workingFolder + @"data\a\0\7\0").ExtractToFolder(workingFolder + @"data\a\0\7\textureBld");
            Narc.Open(workingFolder + @"data\a\0\2\7").ExtractToFolder(workingFolder + @"data\a\0\2\text");
            Narc.Open(workingFolder + @"data\a\0\1\2").ExtractToFolder(workingFolder + @"data\a\0\1\script");
            Narc.Open(workingFolder + @"data\a\0\3\2").ExtractToFolder(workingFolder + @"data\a\0\3\event");
            eventPath = Form1.workingFolder + @"data\a\0\3\event";
            scriptCount = Directory.GetFiles(workingFolder + @"data\a\0\1\script").Length;
            matrixCount = Directory.GetFiles(workingFolder + @"data\a\0\4\matrix").Length;
            mapCount = Directory.GetFiles(workingFolder + @"data\a\0\6\map").Length;
            buildingsCount = Directory.GetFiles(workingFolder + @"data\a\0\4\building").Length;
            texturesCount = Directory.GetFiles(workingFolder + @"data\a\0\4\texture").Length;
            bldTexturesCount = Directory.GetFiles(workingFolder + @"data\a\0\7\textureBld").Length;
            textCount = Directory.GetFiles(workingFolder + @"data\a\0\2\text").Length;
            if (new FileInfo(workingFolder + @"arm9.bin").Length < 0xC0000)
            {
                System.IO.BinaryWriter arm9Truncate = new System.IO.BinaryWriter(File.OpenWrite(workingFolder + @"arm9.bin"));
                long arm9Length = new FileInfo(workingFolder + @"arm9.bin").Length;
                arm9Truncate.BaseStream.SetLength(arm9Length - 0xc);
                arm9Truncate.Close();
            }
            Process decompress = new Process();
            decompress.StartInfo.FileName = @"Data\blz.exe";
            decompress.StartInfo.Arguments = @" -d " + '"' + workingFolder + "arm9.bin" + '"';
            decompress.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            decompress.StartInfo.CreateNoWindow = true;
            decompress.Start();
            decompress.WaitForExit();
            toolStripStatusLabel1.Text = rm.GetString("ready");
            headerCount = 0;
            headerCount = new System.IO.FileInfo(workingFolder + @"data\fielddata\maptable\mapname.bin").Length / 16;
            MessageBox.Show(rm.GetString("headersFound") + headerCount, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            System.IO.BinaryReader readArm9 = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"arm9.bin"));
            dataGridView1.Enabled = true;
            readArm9.BaseStream.Position = dictOfGameInfo[gameID].ReadOffset;
            System.IO.BinaryReader readMapTable = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\fielddata\maptable\mapname.bin"));
            int rowNumber = 0;
            nameText.Clear();
            #region Names
            BinaryReader readText;
            if (dictOfGameInfo[gameID].Region.Equals("jap"))
            {
                readText = new System.IO.BinaryReader(File.OpenRead(Form1.workingFolder + @"data\a\0\2\text\0272"));
            }
            else if (dictOfGameInfo[gameID].Region.Equals("kor"))
            {
                readText = new System.IO.BinaryReader(File.OpenRead(Form1.workingFolder + @"data\a\0\2\text\0274"));
            }
            else
            {
                readText = new System.IO.BinaryReader(File.OpenRead(Form1.workingFolder + @"data\a\0\2\text\0279"));
            }
            int stringCount = (int)readText.ReadUInt16();
            int initialKey = (int)readText.ReadUInt16();
            int key1 = (initialKey * 0x2FD) & 0xFFFF;
            int key2 = 0;
            int realKey = 0;
            bool specialCharON = false;
            int[] currentOffset = new int[stringCount];
            int[] currentSize = new int[stringCount];
            string[] currentPokemon = new string[stringCount];
            int car = 0;
            for (int i = 0; i < stringCount; i++) // Reads and stores string offsets and sizes
            {
                key2 = (key1 * (i + 1) & 0xFFFF);
                realKey = key2 | (key2 << 16);
                currentOffset[i] = ((int)readText.ReadUInt32()) ^ realKey;
                currentSize[i] = ((int)readText.ReadUInt32()) ^ realKey;
            }
            for (int i = 0; i < stringCount; i++) // Adds new string
            {
                key1 = (0x91BD3 * (i + 1)) & 0xFFFF;
                readText.BaseStream.Position = currentOffset[i];
                string pokemonText = "";
                for (int j = 0; j < currentSize[i]; j++) // Adds new characters to string
                {
                    car = ((int)readText.ReadUInt16()) ^ key1;
                    #region Special Characters
                    if (car == 0xE000 || car == 0x25BC || car == 0x25BD || car == 0xFFFE || car == 0xFFFF)
                    {
                        if (car == 0xE000)
                        {
                            pokemonText += @"\n";
                        }
                        if (car == 0x25BC)
                        {
                            pokemonText += @"\r";
                        }
                        if (car == 0x25BD)
                        {
                            pokemonText += @"\f";
                        }
                        if (car == 0xFFFE)
                        {
                            pokemonText += @"\v";
                            specialCharON = true;
                        }
                        if (car == 0xFFFF)
                        {
                            pokemonText += "";
                        }
                    }
                    #endregion
                    else
                    {
                        if (specialCharON == true)
                        {
                            pokemonText += car.ToString("X4");
                            specialCharON = false;
                        }
                        else
                        {
                            string character = getChar.GetString(car.ToString("X4"));
                            pokemonText += character;
                            if (character == null)
                            {
                                pokemonText += @"\x" + car.ToString("X4");
                            }
                        }
                    }
                    key1 += 0x493D;
                    key1 &= 0xFFFF;
                }
                nameText.Add(pokemonText);
            }
            readText.Close();
            #endregion
            string mapNames;
            for (int i = 0; i < headerCount; i++)
            {
                mapNames = "";
                for (int nameLength = 0; nameLength < 16; nameLength++)
                {
                    int currentByte = readMapTable.ReadByte();
                    byte[] mapBytes = new Byte[] { Convert.ToByte(currentByte) }; // Reads map name
                    if (currentByte != 0) mapNames = mapNames + Encoding.UTF8.GetString(mapBytes);
                }
                dataGridView13.Rows.Add(rowNumber, mapNames, readArm9.ReadByte(), readArm9.ReadByte(), readArm9.ReadByte(), readArm9.ReadByte(), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), readArm9.ReadByte() + (readArm9.ReadByte() << 8), nameText[readArm9.ReadByte()], readArm9.ReadByte(), readArm9.ReadByte(), readArm9.ReadByte(), readArm9.ReadByte(), readArm9.ReadByte()); // Adds header data to grid
                dataGridView13.Rows[rowNumber].HeaderCell.Value = i.ToString();
                dataGridView13.Rows[rowNumber].ReadOnly = false;
                rowNumber++;
            }
            matrixPath = workingFolder + @"data\a\0\4\matrix\";
            mapFileName = workingFolder + @"data\a\0\6\map\";
            button1.Enabled = true;
            readArm9.Close();
            readMapTable.Close();
            newHeaderCount = headerCount;
            comboBox1.Items.Clear();
            for (int i = 0; i < matrixCount; i++)
            {
                comboBox1.Items.Add(rm.GetString("matrix") + i);
            }
            comboBox1.SelectedIndex = 0;
            comboBox2.Items.Clear();

            comboBox4.Items.Clear();
            listBox1.Items.Clear();
            comboBox4.Items.Add(rm.GetString("untextured"));
            for (int i = 0; i < texturesCount; i++)
            {
                comboBox4.Items.Add(rm.GetString("tileset") + i);
                listBox1.Items.Add(rm.GetString("tileset") + i);
            }
            listBox1.SelectedIndex = 0;
            #region Read Map Names
            for (int i = 0; i < mapCount; i++)
            {
                string nsbmdName = "";
                System.IO.BinaryReader readNames = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\6\map" + "\\" + i.ToString("D4")));
                permissionSize = (int)readNames.ReadUInt32();
                buildingsSize = (int)readNames.ReadUInt32();
                readNames.BaseStream.Position += 0xa;
                unknownSize = (int)readNames.ReadUInt16();
                readNames.BaseStream.Position = 0x14 + unknownSize + permissionSize + buildingsSize + 0x34;
                for (int nameLength = 0; nameLength < 16; nameLength++)
                {
                    int currentByte = readNames.ReadByte();
                    byte[] mapBytes = new Byte[] { Convert.ToByte(currentByte) }; // Reads map name
                    if (currentByte != 0) nsbmdName = nsbmdName + Encoding.UTF8.GetString(mapBytes);
                }
                comboBox2.Items.Add(i + ": " + nsbmdName);
                readNames.Close();
            }
            #endregion

            comboBox2.SelectedIndex = 0;
            tabControl1.TabPages.Add(tabPage22);
            tabControl1.TabPages.Add(tabPage2);
            tabControl1.TabPages.Add(tabPage6);
            tabControl1.TabPages.Add(tabPage7);
            tabControl1.TabPages.Add(tabPage11);
            tabControl1.TabPages.Add(tabPage23);
            comboBox1_SelectedIndexChanged(null, null);
            comboBox2_SelectedIndexChanged(null, null);
            comboBox3.Items.Clear();
            for (int i = 0; i < textCount; i++)
            {
                comboBox3.Items.Add(rm.GetString("text") + i);
            }
            comboBox5.Items.Clear();
            comboBox9.Items.Clear();
            for (int i = 0; i < scriptCount; i++)
            {
                comboBox5.Items.Add(rm.GetString("script") + i);
            }
            return;
        }

        private void loadBW() // Initialize BW
        {
            Program.ApplicationExit(null, null);
            isBW = true;
            isB2W2 = false;
            tabControl1.TabPages.Remove(tabPage1);
            tabControl1.TabPages.Remove(tabPage22);
            tabControl1.TabPages.Remove(tabPage2);
            tabControl1.TabPages.Remove(tabPage6);
            tabControl1.TabPages.Remove(tabPage7);
            tabControl1.TabPages.Remove(tabPage11);
            tabControl1.TabPages.Add(genVheaderTab);
            tabControl1.TabPages.Add(tabPage15);
            tabControl1.TabPages.Add(tabPage17);
            tabControl1.TabPages.Add(tabPage7);
            tabControl1.TabPages.Add(tabPage11);
            button14.Enabled = false;
            radioButton14.Visible = true;
            radioButton15.Visible = true;
            numericUpDown7.Visible = true;
            sPKPackagesToolStripMenuItem.Enabled = true;
            dataGridView7.Rows.Clear();
            Column40.MaxInputLength = 5;
            Column41.Visible = false;
            dataGridViewTextBoxColumn30.MaxInputLength = 5;
            Column47.Visible = false;
            workingFolder = Path.GetDirectoryName(ndsFileName) + "\\" + Path.GetFileNameWithoutExtension(ndsFileName) + "_SDSME" + "\\";
            loadLastRom();
            iconON = true; pictureBox1.Refresh();
            toolStripStatusLabel1.Text = rm.GetString("extractPackage");
            Narc.Open(workingFolder + @"data\a\0\0\8").ExtractToFolder(workingFolder + @"data\a\0\0\maps");
            Narc.Open(workingFolder + @"data\a\0\1\2").ExtractToFolder(workingFolder + @"data\a\0\1\headers");
            Narc.Open(workingFolder + @"data\a\0\0\9").ExtractToFolder(workingFolder + @"data\a\0\0\matrix");
            Narc.Open(workingFolder + @"data\a\0\1\4").ExtractToFolder(workingFolder + @"data\a\0\1\tilesets");
            Narc.Open(workingFolder + @"data\a\1\7\6").ExtractToFolder(workingFolder + @"data\a\1\7\bldtilesets");
            Narc.Open(workingFolder + @"data\a\1\7\7").ExtractToFolder(workingFolder + @"data\a\1\7\bld2tilesets");
            Narc.Open(workingFolder + @"data\a\0\0\2").ExtractToFolder(workingFolder + @"data\a\0\0\texts");
            Narc.Open(workingFolder + @"data\a\0\0\3").ExtractToFolder(workingFolder + @"data\a\0\0\texts2");
            Narc.Open(workingFolder + @"data\a\0\5\7").ExtractToFolder(workingFolder + @"data\a\0\5\scripts");
            if (new FileInfo(workingFolder + @"arm9.bin").Length < 0xA0000)
            {
                System.IO.BinaryWriter arm9Truncate = new System.IO.BinaryWriter(File.OpenWrite(workingFolder + @"arm9.bin"));
                long arm9Length = new FileInfo(workingFolder + @"arm9.bin").Length;
                arm9Truncate.BaseStream.SetLength(arm9Length - 0xc);
                arm9Truncate.Close();
            }
            Process decompress = new Process();
            decompress.StartInfo.FileName = @"Data\blz.exe";
            decompress.StartInfo.Arguments = @" -d " + '"' + workingFolder + "arm9.bin" + '"';
            decompress.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            decompress.StartInfo.CreateNoWindow = true;
            decompress.Start();
            decompress.WaitForExit();
            mapCount = Directory.GetFiles(workingFolder + @"data\a\0\0\maps").Length;
            headerCount = new FileInfo(workingFolder + @"data\a\0\1\headers\0000").Length / 48;
            matrixCount = Directory.GetFiles(workingFolder + @"data\a\0\0\matrix").Length;
            texturesCount = Directory.GetFiles(workingFolder + @"data\a\0\1\tilesets").Length;
            bldTexturesCount = Directory.GetFiles(workingFolder + @"data\a\1\7\bldtilesets").Length;
            bld2TexturesCount = Directory.GetFiles(workingFolder + @"data\a\1\7\bld2tilesets").Length;
            textCount = Directory.GetFiles(workingFolder + @"data\a\0\0\texts").Length;
            scriptCount = Directory.GetFiles(workingFolder + @"data\a\0\5\scripts").Length;
            MessageBox.Show(rm.GetString("headersFound") + headerCount, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            toolStripStatusLabel1.Text = rm.GetString("ready");
            nameText.Clear();
            #region Map Names
            int mainKey = 31881;
            System.IO.BinaryReader readText = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\texts\0089"));
            int nameSections = readText.ReadUInt16();
            uint[] sectionOffset = new uint[3];
            uint[] sectionSize = new uint[3];
            int nameCount = readText.ReadUInt16();
            int stringOffset;
            int stringSize;
            int[] stringUnknown = new int[3];
            sectionSize[0] = readText.ReadUInt32();
            readText.ReadUInt32();
            int key;
            for (int i = 0; i < nameSections; i++)
            {
                sectionOffset[i] = readText.ReadUInt32();
            }
            for (int j = 0; j < nameCount; j++)
            {
                #region Layer 1
                readText.BaseStream.Position = sectionOffset[0];
                sectionSize[0] = readText.ReadUInt32();
                readText.BaseStream.Position += j * 8;
                stringOffset = (int)readText.ReadUInt32();
                stringSize = readText.ReadUInt16();
                stringUnknown[0] = readText.ReadUInt16();
                readText.BaseStream.Position = sectionOffset[0] + stringOffset;
                string pokemonText = "";
                key = mainKey;
                for (int k = 0; k < stringSize; k++)
                {
                    int car = Convert.ToUInt16(readText.ReadUInt16() ^ key);
                    if (car == 0xFFFF)
                    {
                    }
                    else if (car == 0xF100)
                    {
                        pokemonText += @"\xF100";
                    }
                    else if (car == 0xFFFE)
                    {
                        pokemonText += @"\n";
                    }
                    else if (car > 20 && car <= 0xFFF0 && car != 0xF000 && Char.GetUnicodeCategory(Convert.ToChar(car)) != UnicodeCategory.OtherNotAssigned)
                    {
                        pokemonText += Convert.ToChar(car);
                    }
                    else
                    {
                        pokemonText += @"\x" + car.ToString("X4");
                    }
                    key = ((key << 3) | (key >> 13)) & 0xFFFF;
                }
                mainKey += 0x2983;
                if (mainKey > 0xFFFF) mainKey -= 0x10000;
                #endregion
                nameText.Add(pokemonText);
            }
            readText.Close();
            #endregion
            System.IO.BinaryReader readHeader = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\1\headers\0000"));
            for (int i = 0; i < headerCount; i++)
            {
                dataGridView7.Rows.Add("", readHeader.ReadByte(), readHeader.ReadByte(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), 0,  readHeader.ReadUInt16(), readHeader.ReadUInt16(), nameText[readHeader.ReadByte()], readHeader.ReadByte(), readHeader.ReadByte(), readHeader.ReadByte(), readHeader.ReadByte(), readHeader.ReadByte(), readHeader.ReadUInt16(), readHeader.ReadByte(), readHeader.ReadByte(), readHeader.ReadUInt32(), readHeader.ReadUInt32(), readHeader.ReadUInt32()); // Adds header data to grid
                dataGridView7.Rows[i].HeaderCell.Value = i.ToString();
                dataGridView7.Rows[i].ReadOnly = false;
            }
            readHeader.Close();
            comboBox6.Items.Clear();
            for (int i = 0; i < matrixCount; i++)
            {
                comboBox6.Items.Add(rm.GetString("matrix") + i);
            }
            comboBox6.SelectedIndex = 0;

            comboBox7.Items.Clear();
            listBox6.Items.Clear();
            comboBox7.Items.Add(rm.GetString("untextured"));
            for (int i = 0; i < texturesCount; i++)
            {
                comboBox7.Items.Add(rm.GetString("tileset") + i);
                listBox6.Items.Add(rm.GetString("tileset") + i);
            }
            listBox6.SelectedIndex = 0;
            comboBox8.Items.Clear();
            #region Read Map Names
            for (int i = 0; i < mapCount; i++)
            {
                string nsbmdName = "";
                System.IO.BinaryReader readNames = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\maps" + "\\" + i.ToString("D4")));
                readNames.BaseStream.Position = 0x4;
                int offset = (int)readNames.ReadUInt32();
                readNames.BaseStream.Position = offset + 0x34;
                for (int nameLength = 0; nameLength < 16; nameLength++)
                {
                    int currentByte = readNames.ReadByte();
                    byte[] mapBytes = new Byte[] { Convert.ToByte(currentByte) }; // Reads map name
                    if (currentByte != 0) nsbmdName = nsbmdName + Encoding.UTF8.GetString(mapBytes);
                }
                comboBox8.Items.Add(i + ": " + nsbmdName);
                readNames.Close();
            }
            #endregion
            comboBox8.SelectedIndex = 0;

            comboBox5.Items.Clear();
            comboBox9.Items.Clear();
            for (int i = 0; i < scriptCount; i++)
            {
                comboBox5.Items.Add(rm.GetString("script") + i);
            }
            comboBox3.Items.Clear();
            for (int i = 0; i < textCount; i++)
            {
                comboBox3.Items.Add(rm.GetString("text") + i);
            }
        }

        private void loadB2W2() // Initialize B2W2
        {
            Program.ApplicationExit(null, null);
            isBW = false;
            isB2W2 = true;
            tabControl1.TabPages.Remove(tabPage1);
            tabControl1.TabPages.Remove(tabPage22);
            tabControl1.TabPages.Remove(tabPage2);
            tabControl1.TabPages.Remove(tabPage6);
            tabControl1.TabPages.Remove(tabPage7);
            tabControl1.TabPages.Remove(tabPage11);
            tabControl1.TabPages.Add(genVheaderTab);
            tabControl1.TabPages.Add(tabPage15);
            tabControl1.TabPages.Add(tabPage17);
            tabControl1.TabPages.Add(tabPage7);
            tabControl1.TabPages.Add(tabPage11);
            button14.Enabled = false;
            radioButton14.Visible = true;
            radioButton15.Visible = true;
            numericUpDown7.Visible = true;
            sPKPackagesToolStripMenuItem.Enabled = true;
            dataGridView7.Rows.Clear();
            Column40.MaxInputLength = 3;
            Column41.Visible = true;
            dataGridViewTextBoxColumn30.MaxInputLength = 3;
            Column47.Visible = true;
            workingFolder = Path.GetDirectoryName(ndsFileName) + "\\" + Path.GetFileNameWithoutExtension(ndsFileName) + "_SDSME" + "\\";
            loadLastRom();
            iconON = true; pictureBox1.Refresh();
            toolStripStatusLabel1.Text = rm.GetString("extractPackage");
            Narc.Open(workingFolder + @"data\a\0\0\8").ExtractToFolder(workingFolder + @"data\a\0\0\maps");
            Narc.Open(workingFolder + @"data\a\0\1\2").ExtractToFolder(workingFolder + @"data\a\0\1\headers");
            Narc.Open(workingFolder + @"data\a\0\0\9").ExtractToFolder(workingFolder + @"data\a\0\0\matrix");
            Narc.Open(workingFolder + @"data\a\0\1\4").ExtractToFolder(workingFolder + @"data\a\0\1\tilesets");
            Narc.Open(workingFolder + @"data\a\1\7\4").ExtractToFolder(workingFolder + @"data\a\1\7\bldtilesets");
            Narc.Open(workingFolder + @"data\a\1\7\5").ExtractToFolder(workingFolder + @"data\a\1\7\bld2tilesets");
            Narc.Open(workingFolder + @"data\a\0\0\2").ExtractToFolder(workingFolder + @"data\a\0\0\texts");
            Narc.Open(workingFolder + @"data\a\0\0\3").ExtractToFolder(workingFolder + @"data\a\0\0\texts2");
            Narc.Open(workingFolder + @"data\a\0\5\6").ExtractToFolder(workingFolder + @"data\a\0\5\scripts");
            if (new FileInfo(workingFolder + @"arm9.bin").Length < 0xA0000)
            {
                System.IO.BinaryWriter arm9Truncate = new System.IO.BinaryWriter(File.OpenWrite(workingFolder + @"arm9.bin"));
                long arm9Length = new FileInfo(workingFolder + @"arm9.bin").Length;
                arm9Truncate.BaseStream.SetLength(arm9Length - 0xc);
                arm9Truncate.Close();
            }
            Process decompress = new Process();
            decompress.StartInfo.FileName = @"Data\blz.exe";
            decompress.StartInfo.Arguments = @" -d " + '"' + workingFolder + "arm9.bin" + '"';
            decompress.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            decompress.StartInfo.CreateNoWindow = true;
            decompress.Start();
            decompress.WaitForExit();
            mapCount = Directory.GetFiles(workingFolder + @"data\a\0\0\maps").Length;
            headerCount = new FileInfo(workingFolder + @"data\a\0\1\headers\0000").Length / 48;
            matrixCount = Directory.GetFiles(workingFolder + @"data\a\0\0\matrix").Length;
            texturesCount = Directory.GetFiles(workingFolder + @"data\a\0\1\tilesets").Length;
            bldTexturesCount = Directory.GetFiles(workingFolder + @"data\a\1\7\bldtilesets").Length;
            bld2TexturesCount = Directory.GetFiles(workingFolder + @"data\a\1\7\bld2tilesets").Length;
            textCount = Directory.GetFiles(workingFolder + @"data\a\0\0\texts").Length;
            scriptCount = Directory.GetFiles(workingFolder + @"data\a\0\5\scripts").Length;
            MessageBox.Show(rm.GetString("headersFound") + headerCount, "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            toolStripStatusLabel1.Text = rm.GetString("ready");
            nameText.Clear();
            #region Map Names
            int mainKey = 31881;
            System.IO.BinaryReader readText = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\texts\0109"));
            int nameSections = readText.ReadUInt16();
            uint[] sectionOffset = new uint[3];
            uint[] sectionSize = new uint[3];
            int nameCount = readText.ReadUInt16();
            int stringOffset;
            int stringSize;
            int[] stringUnknown = new int[3];
            sectionSize[0] = readText.ReadUInt32();
            readText.ReadUInt32();
            int key;
            for (int i = 0; i < nameSections; i++)
            {
                sectionOffset[i] = readText.ReadUInt32();
            }
            for (int j = 0; j < nameCount; j++)
            {
                #region Layer 1
                readText.BaseStream.Position = sectionOffset[0];
                sectionSize[0] = readText.ReadUInt32();
                readText.BaseStream.Position += j * 8;
                stringOffset = (int)readText.ReadUInt32();
                stringSize = readText.ReadUInt16();
                stringUnknown[0] = readText.ReadUInt16();
                readText.BaseStream.Position = sectionOffset[0] + stringOffset;
                string pokemonText = "";
                key = mainKey;
                for (int k = 0; k < stringSize; k++)
                {
                    int car = Convert.ToUInt16(readText.ReadUInt16() ^ key);
                    if (car == 0xFFFF)
                    {
                    }
                    else if (car == 0xF100)
                    {
                        pokemonText += @"\xF100";
                    }
                    else if (car == 0xFFFE)
                    {
                        pokemonText += @"\n";
                    }
                    else if (car > 20 && car <= 0xFFF0 && car != 0xF000 && Char.GetUnicodeCategory(Convert.ToChar(car)) != UnicodeCategory.OtherNotAssigned)
                    {
                        pokemonText += Convert.ToChar(car);
                    }
                    else
                    {
                        pokemonText += @"\x" + car.ToString("X4");
                    }
                    key = ((key << 3) | (key >> 13)) & 0xFFFF;
                }
                mainKey += 0x2983;
                if (mainKey > 0xFFFF) mainKey -= 0x10000;
                #endregion
                nameText.Add(pokemonText);
            }
            readText.Close();
            #endregion
            System.IO.BinaryReader readHeader = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\1\headers\0000"));
            for (int i = 0; i < headerCount; i++)
            {
                dataGridView7.Rows.Add("", readHeader.ReadByte(), readHeader.ReadByte(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), readHeader.ReadByte(), readHeader.ReadByte(), readHeader.ReadUInt16(), readHeader.ReadUInt16(), nameText[readHeader.ReadByte()], readHeader.ReadByte(), readHeader.ReadByte(), readHeader.ReadByte(), readHeader.ReadByte(), readHeader.ReadByte(), readHeader.ReadUInt16(), readHeader.ReadByte(), readHeader.ReadByte(), readHeader.ReadUInt32(), readHeader.ReadUInt32(), readHeader.ReadUInt32()); // Adds header data to grid
                dataGridView7.Rows[i].HeaderCell.Value = i.ToString();
                dataGridView7.Rows[i].ReadOnly = false;
            }
            readHeader.Close();
            comboBox6.Items.Clear();
            for (int i = 0; i < matrixCount; i++)
            {
                comboBox6.Items.Add(rm.GetString("matrix") + i);
            }
            comboBox6.SelectedIndex = 0;

            comboBox7.Items.Clear();
            listBox6.Items.Clear();
            comboBox7.Items.Add(rm.GetString("untextured"));
            for (int i = 0; i < texturesCount; i++)
            {
                comboBox7.Items.Add(rm.GetString("tileset") + i);
                listBox6.Items.Add(rm.GetString("tileset") + i);
            }
            listBox6.SelectedIndex = 0;
            comboBox8.Items.Clear();
            #region Read Map Names
            for (int i = 0; i < mapCount; i++)
            {
                string nsbmdName = "";
                System.IO.BinaryReader readNames = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\maps" + "\\" + i.ToString("D4")));
                readNames.BaseStream.Position = 0x4;
                int offset = (int)readNames.ReadUInt32();
                readNames.BaseStream.Position = offset + 0x34;
                for (int nameLength = 0; nameLength < 16; nameLength++)
                {
                    int currentByte = readNames.ReadByte();
                    byte[] mapBytes = new Byte[] { Convert.ToByte(currentByte) }; // Reads map name
                    if (currentByte != 0) nsbmdName = nsbmdName + Encoding.UTF8.GetString(mapBytes);
                }
                comboBox8.Items.Add(i + ": " + nsbmdName);
                readNames.Close();
            }
            #endregion
            comboBox8.SelectedIndex = 0;

            comboBox5.Items.Clear();
            comboBox9.Items.Clear();
            for (int i = 0; i < scriptCount; i++)
            {
                comboBox5.Items.Add(rm.GetString("script") + i);
            }
            comboBox3.Items.Clear();
            for (int i = 0; i < textCount; i++)
            {
                comboBox3.Items.Add(rm.GetString("text") + i);
            }
        }

        private void loadLastRom() // Load Last ROM
        {
            if (Directory.Exists(workingFolder))
            {
                if (MessageBox.Show(rm.GetString("lastROM"), "", MessageBoxButtons.YesNo, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1) == DialogResult.Yes)
                {
                    return;
                }
                else
                {
                    Application.DoEvents();
                    Directory.Delete(workingFolder, true);
                }
            }
            Directory.CreateDirectory(workingFolder);
            Process unpack = new Process();
            unpack.StartInfo.FileName = @"Data\ndstool.exe";
            unpack.StartInfo.Arguments = "-v -x " + '"' + ndsFileName + '"' + " -9 " + '"' + workingFolder + "arm9.bin" + '"' + " -7 " + '"' + workingFolder + "arm7.bin" + '"' + " -y9 " + '"' + workingFolder + "y9.bin" + '"' + " -y7 " + '"' + workingFolder + "y7.bin" + '"' + " -d " + '"' + workingFolder + "data" + '"' + " -y " + '"' + workingFolder + "overlay" + '"' + " -t " + '"' + workingFolder + "banner.bin" + '"' + " -h " + '"' + workingFolder + "header.bin" + '"';
            Application.DoEvents();
            unpack.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            unpack.StartInfo.CreateNoWindow = true;
            unpack.Start();
            toolStripStatusLabel1.Text = rm.GetString("loadingROM");
            unpack.WaitForExit();
        }
        
        #region Headers

        private void button24_Click(object sender, EventArgs e) // Add V Header
        {
            dataGridView7.Rows.Add(dataGridView7.Rows.Count, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, nameText[0], 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            dataGridView7.Rows[dataGridView7.Rows.Count - 1].ReadOnly = false;
            dataGridView7.Rows[dataGridView7.Rows.Count - 1].HeaderCell.Value = (dataGridView7.Rows.Count - 1).ToString();
            headerCount++;
            if (dataGridView7.Rows.Count > 0)
            {
                button23.Enabled = true;
            }
        }

        private void button23_Click(object sender, EventArgs e) // Remove Last V Header
        {
            dataGridView7.Rows.RemoveAt(dataGridView7.Rows.Count - 1);
            headerCount--;
            if (dataGridView7.Rows.Count == 1)
            {
                button23.Enabled = false;
            }
        }

        private void button1_Click(object sender, EventArgs e) // Save Headers
        {
            #region BW Support
            if (isBW == true)
            {
                toolStripStatusLabel1.Text = rm.GetString("writingHeaders");
                BinaryWriter writeHeaders = new BinaryWriter(File.Create(workingFolder + @"data\a\0\1\headers\0000"));
                for (int i = 0; i < headerCount; i++)
                {
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[1].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[2].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[3].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[4].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[5].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[6].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[7].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[8].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[9].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[10].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[11].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[12].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[14].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[15].Value));
                    writeHeaders.Write(Convert.ToByte(nameText.IndexOf(dataGridView7.Rows[i].Cells[16].Value.ToString())));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[17].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[18].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[19].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[20].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[21].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[22].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[23].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[24].Value));
                    writeHeaders.Write(Convert.ToUInt32(dataGridView7.Rows[i].Cells[25].Value));
                    writeHeaders.Write(Convert.ToUInt32(dataGridView7.Rows[i].Cells[26].Value));
                    writeHeaders.Write(Convert.ToUInt32(dataGridView7.Rows[i].Cells[27].Value));
                }
                writeHeaders.Close();
                toolStripStatusLabel1.Text = rm.GetString("headersSaved");
                return;
            }
            #endregion

            #region B2W2 Support
            if (isB2W2 == true)
            {
                toolStripStatusLabel1.Text = rm.GetString("writingHeaders");
                BinaryWriter writeHeaders = new BinaryWriter(File.Create(workingFolder + @"data\a\0\1\headers\0000"));
                for (int i = 0; i < headerCount; i++)
                {
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[1].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[2].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[3].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[4].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[5].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[6].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[7].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[8].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[9].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[10].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[11].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[12].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[13].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[14].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[15].Value));
                    writeHeaders.Write(Convert.ToByte(nameText.IndexOf(dataGridView7.Rows[i].Cells[16].Value.ToString())));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[17].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[18].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[19].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[20].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[21].Value));
                    writeHeaders.Write(Convert.ToUInt16(dataGridView7.Rows[i].Cells[22].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[23].Value));
                    writeHeaders.Write(Convert.ToByte(dataGridView7.Rows[i].Cells[24].Value));
                    writeHeaders.Write(Convert.ToUInt32(dataGridView7.Rows[i].Cells[25].Value));
                    writeHeaders.Write(Convert.ToUInt32(dataGridView7.Rows[i].Cells[26].Value));
                    writeHeaders.Write(Convert.ToUInt32(dataGridView7.Rows[i].Cells[27].Value));
                }
                writeHeaders.Close();
                toolStripStatusLabel1.Text = rm.GetString("headersSaved");
                return;
            }
            #endregion

            toolStripStatusLabel1.Text = rm.GetString("writingHeaders");
            File.Create(workingFolder + @"arm9_new.bin").Close();
            BinaryWriter writeArm9 = new BinaryWriter(File.OpenWrite(workingFolder + @"arm9_new.bin"));
            BinaryReader readArm9 = new BinaryReader(File.OpenRead(workingFolder + @"arm9.bin"));
            int rowNumber = 0;

            #region DP Support
            if (gameID == 0x45414441 || gameID == 0x45415041) // DP USA
            {
                for (int i = 0; i < 0xEEDBC; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToUInt16(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xEEDBC); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x53414441 || gameID == 0x53415041) // DP ESP
            {
                for (int i = 0; i < 0xEEE08; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToUInt16(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xEEE08); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x46414441 || gameID == 0x46415041) // DP FRA
            {
                for (int i = 0; i < 0xEEDFC; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToUInt16(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xEEDFC); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x49414441 || gameID == 0x49415041) // DP ITA
            {
                for (int i = 0; i < 0xEED70; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToUInt16(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xEED70); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x44414441 || gameID == 0x44415041) // DP GER
            {
                for (int i = 0; i < 0xEEDCC; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToUInt16(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xEEDCC); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x4A414441 || gameID == 0x4A415041) // DP JAP
            {
                for (int i = 0; i < 0xF0C28; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToUInt16(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xF0C28); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x4B414441 || gameID == 0x4B415041) // DP KOR
            {
                for (int i = 0; i < 0xEA408; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToUInt16(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xEA408); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            #endregion

            #region Platinum Support

            if (gameID == 0x45555043) // Pt USA
            {
                for (int i = 0; i < 0xE601C; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column16
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xE601C); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x53555043) // Pt ESP
            {
                for (int i = 0; i < 0xE60B0; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column16
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xE60B0); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x46555043) // Pt FRA
            {
                for (int i = 0; i < 0xE60A4; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column16
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xE60A4); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x49555043) // Pt ITA
            {
                for (int i = 0; i < 0xE6038; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column16
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xE6038); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x44555043) // Pt GER
            {
                for (int i = 0; i < 0xE6074; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column16
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xE6074); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x4A555043) // Pt JAP
            {
                for (int i = 0; i < 0xE56F0; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column16
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xE56F0); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x4B555043) // Pt KOR
            {
                for (int i = 0; i < 0xE6AA4; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToUInt16(dataGridView1.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column10
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString()))); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[13].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column15
                    writeArm9.Write(Convert.ToByte(dataGridView1.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column16
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xE6AA4); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            #endregion

            #region HGSS Support

            if (gameID == 0x454B5049 || gameID == 0x45475049) // HGSS USA
            {
                for (int i = 0; i < 0xF6BE0; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column16
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column17
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[12].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView13.Rows[i].Cells[13].Value.ToString()))); // Writes Column10
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[18].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xF6BE0); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x534B5049 || gameID == 0x53475049) // HGSS ESP
            {
                for (int i = 0; i < 0xF6BC8; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column16
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column17
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[12].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView13.Rows[i].Cells[13].Value.ToString()))); // Writes Column10
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[18].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xF6BC8); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x464B5049 || gameID == 0x46475049) // HGSS FRA
            {
                for (int i = 0; i < 0xF6BC4; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column16
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column17
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[12].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView13.Rows[i].Cells[13].Value.ToString()))); // Writes Column10
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[18].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xF6BC4); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x494B5049 || gameID == 0x49475049) // HGSS ITA
            {
                for (int i = 0; i < 0xF6B58; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column16
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column17
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[12].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView13.Rows[i].Cells[13].Value.ToString()))); // Writes Column10
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[18].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xF6B58); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x444B5049 || gameID == 0x44475049) // HGSS GER
            {
                for (int i = 0; i < 0xF6B94; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column16
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column17
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[12].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView13.Rows[i].Cells[13].Value.ToString()))); // Writes Column10
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[18].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xF6B94); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x4A4B5049 || gameID == 0x4A475049) // HGSS JAP
            {
                for (int i = 0; i < 0xF6390; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column16
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column17
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[12].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView13.Rows[i].Cells[13].Value.ToString()))); // Writes Column10
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[18].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xF6390); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }
            if (gameID == 0x4B4B5049 || gameID == 0x4B475049) // HGSS KOR
            {
                for (int i = 0; i < 0xF728C; i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data before headers
                }
                for (int i = 0; i < newHeaderCount; i++)
                {
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[2].Value.ToString())); // Writes Column16
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[3].Value.ToString())); // Writes Column17
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[4].Value.ToString())); // Writes Column1
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[5].Value.ToString())); // Writes Column2
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[6].Value.ToString())); // Writes Column3
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[7].Value.ToString())); // Writes Column4
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[8].Value.ToString())); // Writes Column5
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[9].Value.ToString())); // Writes Column6
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[10].Value.ToString())); // Writes Column7
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[11].Value.ToString())); // Writes Column8
                    writeArm9.Write(Convert.ToUInt16(dataGridView13.Rows[rowNumber].Cells[12].Value.ToString())); // Writes Column9
                    writeArm9.Write(Convert.ToByte(nameText.IndexOf(dataGridView13.Rows[i].Cells[13].Value.ToString()))); // Writes Column10
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[14].Value.ToString())); // Writes Column11
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[15].Value.ToString())); // Writes Column12
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[16].Value.ToString())); // Writes Column13
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[17].Value.ToString())); // Writes Column14
                    writeArm9.Write(Convert.ToByte(dataGridView13.Rows[rowNumber].Cells[18].Value.ToString())); // Writes Column15
                    rowNumber++;
                }
                readArm9.BaseStream.Position += headerCount * 24;
                long arm9Size = new System.IO.FileInfo(workingFolder + @"arm9.bin").Length;
                for (int i = 0; i < (arm9Size - (headerCount * 24) - 0xF728C); i++)
                {
                    writeArm9.Write(readArm9.ReadByte()); // Writes data after headers
                }
            }

            #endregion

            readArm9.Close();
            writeArm9.Close();
            File.Delete(workingFolder + @"arm9.bin");
            File.Move(workingFolder + @"arm9_new.bin", workingFolder + @"arm9.bin");
            BinaryWriter writeMapNames = new BinaryWriter(File.OpenWrite(workingFolder + @"data\fielddata\maptable\mapname.bin"));
            int zCoord = 0;
            for (int z = 0; z < newHeaderCount; z++)
            {
                if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                {
                    writeMapNames.Write(Encoding.UTF8.GetBytes(Convert.ToString(dataGridView1.Rows[zCoord].Cells[1].Value)));
                    for (int i = 0; i < 16 - (dataGridView1.Rows[zCoord].Cells[1].Value.ToString().Length); i++)
                    {
                        writeMapNames.Write((byte)0x0);
                    }
                    zCoord++;
                }
                else
                {
                    writeMapNames.Write(Encoding.UTF8.GetBytes(Convert.ToString(dataGridView13.Rows[zCoord].Cells[1].Value)));
                    for (int i = 0; i < 16 - (dataGridView13.Rows[zCoord].Cells[1].Value.ToString().Length); i++)
                    {
                        writeMapNames.Write((byte)0x0);
                    }
                    zCoord++;
                }
            }
            zCoord = 0;
            writeMapNames.Close();
            toolStripStatusLabel1.Text = rm.GetString("headersSaved");
        }

        #endregion

        #region Area Data

        private void comboBox12_SelectedIndexChanged(object sender, EventArgs e) // Select DPPt Area Data
        {
            System.IO.BinaryReader readArea = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\fielddata\areadata\area_data" + "\\" + comboBox12.SelectedIndex.ToString("D4")));
            numericUpDown27.Value = readArea.ReadUInt16();
            numericUpDown30.Value = readArea.ReadUInt16();
            numericUpDown62.Value = readArea.ReadUInt16();
            if (readArea.ReadUInt16() == 1) checkBox7.Checked = true;
            else checkBox7.Checked = false;
            readArea.Close();
        }

        private void button63_Click(object sender, EventArgs e) // Save Current Area Data
        {
            System.IO.BinaryWriter writeArea = new System.IO.BinaryWriter(File.Create(workingFolder + @"data\fielddata\areadata\area_data" + "\\" + comboBox12.SelectedIndex.ToString("D4")));
            writeArea.Write((UInt16)numericUpDown27.Value);
            writeArea.Write((UInt16)numericUpDown30.Value);
            writeArea.Write((UInt16)numericUpDown62.Value);
            if (checkBox7.Checked) writeArea.Write((UInt16)1);
            else writeArea.Write((UInt16)0);
            writeArea.Close();
        }

        private void button61_Click(object sender, EventArgs e) // Add new Area Data
        {
            System.IO.BinaryWriter writeArea = new System.IO.BinaryWriter(File.Create(workingFolder + @"data\fielddata\areadata\area_data" + "\\" + comboBox12.Items.Count.ToString("D4")));
            writeArea.Write((UInt16)0);
            writeArea.Write((UInt16)0);
            writeArea.Write((UInt16)0);
            writeArea.Write((UInt16)0);
            writeArea.Close();
            comboBox12.Items.Add(rm.GetString("areaDataList") + comboBox12.Items.Count);
            comboBox12.SelectedIndex = comboBox12.Items.Count - 1;
        }

        #endregion

        #region Building Packs

        private void comboBox13_SelectedIndexChanged(object sender, EventArgs e) // Select Building Pack
        {
            System.IO.BinaryReader readPack = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\fielddata\areadata\area_build_model\area_build" + "\\" + comboBox13.SelectedIndex.ToString("D4")));
            int count = readPack.ReadUInt16();
            if (count == 0) button64.Enabled = false;
            else button64.Enabled = true;
            listBox11.Items.Clear();
            for (int i = 0; i < count; i++)
            {
                int index = readPack.ReadUInt16();
                listBox11.Items.Add(listBox12.Items[index]);
            }
            readPack.Close();
            if (listBox11.Items.Count > 0) listBox11.SelectedIndex = 0;
            listBox12.SelectedIndex = 0;
        }

        private void listBox12_SelectedIndexChanged(object sender, EventArgs e) // Select Building from Main List
        {
            if (listBox11.Items.Contains(listBox12.SelectedItem)) button65.Enabled = false;
            else button65.Enabled = true;
        }

        private void button65_Click(object sender, EventArgs e) // Add building to Pack
        {
            listBox11.Items.Add(listBox12.SelectedItem);
            if (listBox11.Items.Contains(listBox12.SelectedItem)) button65.Enabled = false;
            if (listBox11.Items.Count == 1) listBox11.SelectedIndex = 0;
        }

        private void button64_Click(object sender, EventArgs e) // Remove Building
        {
            listBox11.Items.RemoveAt(listBox11.SelectedIndex);
            if (listBox11.Items.Count == 0) button64.Enabled = false;
            else listBox11.SelectedIndex = 0;
        }

        private void button66_Click(object sender, EventArgs e) // Save Building Pack
        {
            System.IO.BinaryWriter writePack = new System.IO.BinaryWriter(File.Create(workingFolder + @"data\fielddata\areadata\area_build_model\area_build" + "\\" + comboBox13.SelectedIndex.ToString("D4")));
            int count = listBox11.Items.Count;
            writePack.Write(Convert.ToUInt16(count));
            for (int i = 0; i < count; i++)
            {
                string[] number = listBox11.Items[i].ToString().Split(':');
                int index = Convert.ToUInt16(number[0]);
                writePack.Write(Convert.ToUInt16(index));
            }
            writePack.Close();
        }
        
        private void readBldNames(string path)
        {
            listBox12.Items.Clear();
            System.IO.BinaryReader readNarc = new System.IO.BinaryReader(File.OpenRead(path));
            readNarc.BaseStream.Position = 0x14;
            int offsetLength = (int)readNarc.ReadUInt32();
            int count = (int)readNarc.ReadUInt32();
            readNarc.BaseStream.Position += offsetLength - 0x8;
            int nameLength = (int)readNarc.ReadUInt32();
            for (int i = 0; i < count; i++)
            {
                string bldName = "";
                readNarc.BaseStream.Position = 0x1C + (i * 8);
                int offset = (int)readNarc.ReadUInt32();
                readNarc.BaseStream.Position = 0x18 + offsetLength + nameLength + offset + 0x14;
                if (readNarc.ReadUInt32() == 0x304C444D)
                {
                    readNarc.BaseStream.Position += 0x1C;
                }
                else readNarc.BaseStream.Position += 0x20;
                for (int nameL = 0; nameL < 16; nameL++)
                {
                    int currentByte = readNarc.ReadByte();
                    byte[] mapBytes = new Byte[] { Convert.ToByte(currentByte) }; // Reads map name
                    if (currentByte != 0) bldName += Encoding.UTF8.GetString(mapBytes);
                }
                listBox12.Items.Add(i.ToString("D3") + ": " + bldName);
            }
            readNarc.Close();
        }

        private void button62_Click(object sender, EventArgs e) // Add new Pack
        {
            OpenFileDialog ef = new OpenFileDialog();
            ef.Title = rm.GetString("importTileset");
            ef.Filter = rm.GetString("tilesetFile");
            if (ef.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader textureStream = new System.IO.BinaryReader(File.OpenRead(ef.FileName));
                int header;
                header = (int)textureStream.ReadUInt32();
                textureStream.Close();
                if (header == 811095106)
                {
                    bldTexturesCount++;
                    string packPath;
                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                    {
                        File.Copy(ef.FileName, workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset" + "\\" + comboBox13.Items.Count.ToString("D4"), true);
                        packPath = workingFolder + @"data\fielddata\areadata\area_build_model\area_build";
                    }
                    else
                    {
                        File.Copy(ef.FileName, workingFolder + @"data\a\0\7\textureBld" + "\\" + comboBox13.Items.Count.ToString("D4"), true);
                        packPath = "";
                    }
                    System.IO.BinaryWriter writePack = new System.IO.BinaryWriter(File.Create(packPath + "\\" + comboBox13.Items.Count.ToString("D4")));
                    writePack.Write((UInt16)0x0);
                    writePack.Close();
                    comboBox13.Items.Add(rm.GetString("buildingPackList") + comboBox13.Items.Count);
                    comboBox13.SelectedIndex = comboBox13.Items.Count - 1;
                }
                else
                {
                    MessageBox.Show(rm.GetString("invalidFile"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
            return;
        }

        #endregion

        #region Matrix Editor

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) // Select Matrix
        {
            toolStripStatusLabel1.Text = rm.GetString("readingMatrix");
            dataGridView2.Rows.Clear();
            dataGridView3.Rows.Clear();
            dataGridView4.Rows.Clear();
            dataGridView2.Columns.Clear();
            dataGridView3.Columns.Clear();
            dataGridView4.Columns.Clear();
            System.IO.BinaryReader readMatrix = new System.IO.BinaryReader(File.OpenRead(matrixPath + "\\" + comboBox1.SelectedIndex.ToString("D4")));
            numericUpDown1.Value = readMatrix.ReadByte(); // Reads Height
            numericUpDown2.Value = readMatrix.ReadByte(); // Reads Width
            unknownLayer = readMatrix.ReadByte();
            matrixLayers = readMatrix.ReadByte() + 1; // Reads Layers
            int nameLengthByte = readMatrix.ReadByte();  // Name Length
            string matrixName = "";
            for (int nameLength = 0; nameLength < nameLengthByte; nameLength++)
            {
                int currentByte = readMatrix.ReadByte();
                byte[] mapBytes = new Byte[] { Convert.ToByte(currentByte) }; // Reads map name
                if (currentByte != 0) matrixName = matrixName + Encoding.UTF8.GetString(mapBytes);
            }
            textBox1.Text = matrixName;
            DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
            for (int i = 0; i < numericUpDown1.Value; i++)
            {
                dataGridView2.Columns.Add(new DataGridViewColumn { CellTemplate = cell }); // Adds columns
                dataGridView3.Columns.Add(new DataGridViewColumn { CellTemplate = cell }); // Adds columns
                dataGridView4.Columns.Add(new DataGridViewColumn { CellTemplate = cell }); // Adds columns
                dataGridView2.Columns[i].Width = 34;
                dataGridView3.Columns[i].Width = 22;
                dataGridView4.Columns[i].Width = 34;
                dataGridView2.Columns[i].HeaderText = (i + 1).ToString();
                dataGridView3.Columns[i].HeaderText = (i + 1).ToString();
                dataGridView4.Columns[i].HeaderText = (i + 1).ToString();
            }
            for (int i = 0; i < numericUpDown2.Value; i++)
            {
                dataGridView2.Rows.Add(); // Adds rows
                dataGridView3.Rows.Add(); // Adds rows
                dataGridView4.Rows.Add(); // Adds rows
                dataGridView2.Rows[i].HeaderCell.Value = (i + 1).ToString();
                dataGridView3.Rows[i].HeaderCell.Value = (i + 1).ToString();
                dataGridView4.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
            int xCoord = 0;
            int zCoord = 0;
            tabPage4.Text = rm.GetString("mapFiles");
            for (int z = 0; z < numericUpDown2.Value; z++) // Main Layer
            {
                for (int i = 0; i < numericUpDown1.Value; i++)
                {
                    int matrixIndex = readMatrix.ReadByte() + (readMatrix.ReadByte() << 8); // Reads Entry
                    dataGridView2.Rows[zCoord].Cells[xCoord].Value = matrixIndex;
                    if (unknownLayer == 1 && matrixIndex == 0) // Alternate colour for 0 value
                    {
                        dataGridView2.Rows[zCoord].Cells[xCoord].Style.BackColor = Color.Black;
                        dataGridView2.Rows[zCoord].Cells[xCoord].Style.ForeColor = Color.White;
                    }
                    xCoord++;
                }
                zCoord++;
                xCoord = 0;
            }
            zCoord = 0;
            tabControl2.TabPages.Remove(tabPage3);
            tabControl2.TabPages.Remove(tabPage5);
            if (unknownLayer == 1) // Hidden Layer
            {
                tabControl2.TabPages.Add(tabPage3);
                for (int z = 0; z < numericUpDown2.Value; z++)
                {
                    for (int i = 0; i < numericUpDown1.Value; i++)
                    {
                        int matrixIndex = readMatrix.ReadByte(); // Reads Entry
                        dataGridView3.Rows[zCoord].Cells[xCoord].Value = matrixIndex;
                        dataGridView3.Rows[zCoord].Cells[xCoord].Style.BackColor = Color.Black;
                        dataGridView3.Rows[zCoord].Cells[xCoord].Style.ForeColor = Color.White;
                        xCoord++;
                    }
                    zCoord++;
                    xCoord = 0;
                }
            }
            zCoord = 0;
            if (matrixLayers == 2)
            {
                tabControl2.TabPages.Add(tabPage5);
                tabPage4.Text = rm.GetString("mapHeaders");
                for (int z = 0; z < numericUpDown2.Value; z++)
                {
                    for (int i = 0; i < numericUpDown1.Value; i++)
                    {
                        int matrixIndex = readMatrix.ReadByte() + (readMatrix.ReadByte() << 8); // Reads Entry
                        dataGridView4.Rows[zCoord].Cells[xCoord].Value = matrixIndex;
                        if (matrixIndex == 0xFFFF) // Alternate colour for FFFF value
                        {
                            dataGridView4.Rows[zCoord].Cells[xCoord].Value = "NULL";
                            dataGridView4.Rows[zCoord].Cells[xCoord].Style.BackColor = Color.Black;
                            dataGridView4.Rows[zCoord].Cells[xCoord].Style.ForeColor = Color.White;
                        }
                        else if (Convert.ToInt32(dataGridView2.Rows[zCoord].Cells[xCoord].Value) == 0) // Alternate colour for FFFF value
                        {
                            dataGridView3.Rows[zCoord].Cells[xCoord].Style.BackColor = Color.Purple;
                            dataGridView3.Rows[zCoord].Cells[xCoord].Style.ForeColor = Color.White;
                            dataGridView4.Rows[zCoord].Cells[xCoord].Style.BackColor = Color.Purple;
                            dataGridView4.Rows[zCoord].Cells[xCoord].Style.ForeColor = Color.White;
                        }
                        else
                        {
                            dataGridView3.Rows[zCoord].Cells[xCoord].Style.BackColor = Color.White;
                            dataGridView3.Rows[zCoord].Cells[xCoord].Style.ForeColor = Color.Black;
                        }
                        xCoord++;
                    }
                    zCoord++;
                    xCoord = 0;
                }
            }
            readMatrix.Close();
            toolStripStatusLabel1.Text = rm.GetString("ready");
        }

        private void comboBox6_SelectedIndexChanged(object sender, EventArgs e) // Gen V Select Matrix
        {
            toolStripStatusLabel1.Text = rm.GetString("readingMatrix");
            dataGridView10.Rows.Clear();
            dataGridView8.Rows.Clear();
            dataGridView10.Columns.Clear();
            dataGridView8.Columns.Clear();
            System.IO.BinaryReader readMatrix = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\matrix" + "\\" + comboBox6.SelectedIndex.ToString("D4")));
            int flag = readMatrix.ReadInt32();
            numericUpDown5.Value = readMatrix.ReadUInt16();
            numericUpDown4.Value = readMatrix.ReadUInt16();
            DataGridViewTextBoxCell cell = new DataGridViewTextBoxCell();
            for (int i = 0; i < numericUpDown5.Value; i++)
            {
                dataGridView10.Columns.Add(new DataGridViewColumn { CellTemplate = cell }); // Adds columns
                dataGridView8.Columns.Add(new DataGridViewColumn { CellTemplate = cell }); // Adds columns
                dataGridView10.Columns[i].Width = 34;
                dataGridView8.Columns[i].Width = 34;
                dataGridView10.Columns[i].HeaderText = (i + 1).ToString();
                dataGridView8.Columns[i].HeaderText = (i + 1).ToString();
            }
            for (int i = 0; i < numericUpDown4.Value; i++)
            {
                dataGridView10.Rows.Add(); // Adds rows
                dataGridView8.Rows.Add(); // Adds rows
                dataGridView10.Rows[i].HeaderCell.Value = (i + 1).ToString();
                dataGridView8.Rows[i].HeaderCell.Value = (i + 1).ToString();
            }
            tabControl5.TabPages.Remove(tabPage16);
            if (flag == 1)
            {
                readMatrix.BaseStream.Position = (int)numericUpDown5.Value * (int)numericUpDown4.Value * 4 + 8;
                tabControl5.TabPages.Add(tabPage16);
                for (int z = 0; z < numericUpDown4.Value; z++) // Main Layer
                {
                    for (int i = 0; i < numericUpDown5.Value; i++)
                    {
                        uint matrixIndex = readMatrix.ReadUInt32(); // Reads Entry
                        dataGridView8.Rows[z].Cells[i].Value = matrixIndex;
                        if (matrixIndex == 0xFFFFFFFF) // Alternate colour for 0 value
                        {
                            dataGridView8.Rows[z].Cells[i].Value = "NULL";
                            dataGridView8.Rows[z].Cells[i].Style.BackColor = Color.Black;
                            dataGridView8.Rows[z].Cells[i].Style.ForeColor = Color.White;
                        }
                    }
                }
            }
            readMatrix.BaseStream.Position = 0x8;
            for (int z = 0; z < numericUpDown4.Value; z++) // Main Layer
            {
                for (int i = 0; i < numericUpDown5.Value; i++)
                {
                    uint matrixIndex = readMatrix.ReadUInt32(); // Reads Entry
                    dataGridView10.Rows[z].Cells[i].Value = matrixIndex;
                    if (matrixIndex == 0xFFFFFFFF) // Alternate colour for 0 value
                    {
                        dataGridView10.Rows[z].Cells[i].Value = "NULL";
                        dataGridView10.Rows[z].Cells[i].Style.BackColor = Color.Black;
                        dataGridView10.Rows[z].Cells[i].Style.ForeColor = Color.White;
                    }
                    if (dataGridView10.Rows[z].Cells[i].Value.ToString() != "NULL" && tabControl5.TabCount == 2 && dataGridView8.Rows[z].Cells[i].Value.ToString() == "NULL") // Alternate colour for 0 value
                    {
                        dataGridView10.Rows[z].Cells[i].Style.BackColor = Color.Purple;
                        dataGridView10.Rows[z].Cells[i].Style.ForeColor = Color.White;
                    }
                }
            }
            readMatrix.Close();
            toolStripStatusLabel1.Text = rm.GetString("ready");
        }

        private void dataGridView2_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e) // Redirect matrix to header
        {
            if (tabControl2.TabCount > 1)
            {
                if (Convert.ToUInt32(dataGridView2.CurrentCell.Value) < dataGridView1.RowCount)
                {
                    dataGridView1.CurrentCell = dataGridView1.Rows[Convert.ToInt32(dataGridView2.CurrentCell.Value)].Cells[1];
                    tabControl1.SelectedIndex = 0;
                    return;
                }
            }
            else
            {
                if (Convert.ToUInt32(dataGridView2.CurrentCell.Value) < mapCount)
                {
                    comboBox2.SelectedIndex = Convert.ToInt32(dataGridView2.CurrentCell.Value);
                    tabControl1.SelectedIndex = 2;
                    return;
                }
            }
        }

        private void dataGridView8_CellDoubleClick(object sender, DataGridViewCellEventArgs e) // Gen V Redirect matrix to header
        {
            if (dataGridView8.CurrentCell.Value.ToString().ToUpper() != "NULL" && Convert.ToUInt32(dataGridView8.CurrentCell.Value) < dataGridView7.RowCount)
            {
                dataGridView7.CurrentCell = dataGridView7.Rows[Convert.ToInt32(dataGridView8.CurrentCell.Value)].Cells[1];
                tabControl1.SelectedIndex = 0;
                return;
            }
        }

        private void dataGridView4_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e) // Redirect matrix to map
        {
            comboBox2.SelectedIndex = Convert.ToInt32(dataGridView4.CurrentCell.Value);
            if (dataGridView4.CurrentCell.Value.ToString().ToUpper() != "NULL" && Convert.ToUInt32(dataGridView2.Rows[dataGridView4.CurrentCell.RowIndex].Cells[dataGridView4.CurrentCell.ColumnIndex].Value) < dataGridView1.RowCount)
            {
                comboBox4.SelectedIndex = ((Convert.ToInt32(dataGridView1.Rows[Convert.ToInt32(dataGridView2.Rows[dataGridView4.CurrentCell.RowIndex].Cells[dataGridView4.CurrentCell.ColumnIndex].Value)].Cells[2].Value)) + 1);
            }
            tabControl1.SelectedIndex = 2;
            return;
        }

        private void dataGridView10_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e) // Gen V Redirect matrix to map
        {
            if (dataGridView10.CurrentCell.Value.ToString().ToUpper() != "NULL" && Convert.ToUInt32(dataGridView10.CurrentCell.Value) < mapCount)
            {
                comboBox8.SelectedIndex = Convert.ToInt32(dataGridView10.CurrentCell.Value);
                if (tabControl5.TabCount > 1 && dataGridView8.Rows[dataGridView10.CurrentCell.RowIndex].Cells[dataGridView10.CurrentCell.ColumnIndex].Value.ToString().ToUpper() != "NULL" && Convert.ToUInt32(dataGridView8.Rows[dataGridView10.CurrentCell.RowIndex].Cells[dataGridView10.CurrentCell.ColumnIndex].Value) < dataGridView7.RowCount)
                {
                    comboBox7.SelectedIndex = ((Convert.ToInt32(dataGridView7.Rows[Convert.ToInt32(dataGridView8.Rows[dataGridView10.CurrentCell.RowIndex].Cells[dataGridView10.CurrentCell.ColumnIndex].Value)].Cells[3].Value)) + 1);
                }
                tabControl1.SelectedIndex = 2;
                return;
            }
        }

        private void button4_Click(object sender, EventArgs e) // Save Matrix
        {
            toolStripStatusLabel1.Text = rm.GetString("writingMatrix");
            System.IO.BinaryWriter writeMatrix = new System.IO.BinaryWriter(File.Create(matrixPath + "\\" + comboBox1.SelectedIndex.ToString("D4")));
            writeMatrix.Write((byte)numericUpDown1.Value);
            writeMatrix.Write((byte)numericUpDown2.Value);
            writeMatrix.Write((byte)unknownLayer);
            writeMatrix.Write((byte)(matrixLayers - 1));
            writeMatrix.Write((byte)textBox1.Text.Length);
            writeMatrix.Write(Encoding.UTF8.GetBytes(textBox1.Text));
            int xCoord = 0;
            int zCoord = 0;
            for (int z = 0; z < numericUpDown2.Value; z++) // Main Layer
            {
                for (int i = 0; i < numericUpDown1.Value; i++)
                {
                    if (xCoord < (dataGridView2.ColumnCount) && zCoord < (dataGridView2.RowCount))
                    {
                       writeMatrix.Write(Convert.ToUInt16(dataGridView2.Rows[zCoord].Cells[xCoord].Value)); // Writes Entry
                    }
                    else
                    {
                        writeMatrix.Write(Convert.ToUInt16(0x0)); // Writes Null Entry
                    }
                    xCoord++;
                }
                zCoord++;
                xCoord = 0;
            }
            zCoord = 0;
            if (unknownLayer == 1)
            {
                for (int z = 0; z < numericUpDown2.Value; z++) // Hidden Layer
                {
                    for (int i = 0; i < numericUpDown1.Value; i++)
                    {
                        if ((xCoord < dataGridView3.ColumnCount) && zCoord < (dataGridView3.RowCount))
                        {
                            writeMatrix.Write(Convert.ToByte(dataGridView3.Rows[zCoord].Cells[xCoord].Value)); // Writes Entry
                        }
                        else
                        {
                            writeMatrix.Write(Convert.ToByte(0x0)); // Writes Null Entry
                        }
                        xCoord++;
                    }
                    zCoord++;
                    xCoord = 0;
                }
                zCoord = 0;
            }
            if (matrixLayers == 2)
            {
                for (int z = 0; z < numericUpDown2.Value; z++) // Hidden Layer
                {
                    for (int i = 0; i < numericUpDown1.Value; i++)
                    {
                        if (xCoord < (dataGridView4.ColumnCount) && zCoord < (dataGridView4.RowCount))
                        {
                            if (dataGridView4.Rows[zCoord].Cells[xCoord].Value.ToString().ToUpper() == "NULL")
                            {
                                writeMatrix.Write(Convert.ToUInt16(0xFFFF)); // Writes Entry
                            }
                            else
                            {
                                writeMatrix.Write(Convert.ToUInt16(dataGridView4.Rows[zCoord].Cells[xCoord].Value)); // Writes Entry
                            }
                        }
                        else
                        {
                            writeMatrix.Write(Convert.ToUInt16(0xFFFF)); // Writes Null Entry
                        }
                        xCoord++;
                    }
                    zCoord++;
                    xCoord = 0;
                }
                zCoord = 0;
            }
            xCoord = 0;
            writeMatrix.Close();
            comboBox1_SelectedIndexChanged(null, null);
            toolStripStatusLabel1.Text = rm.GetString("matrixSaved");
        }

        private void button26_Click(object sender, EventArgs e) // Gen V Save Matrix
        {
            toolStripStatusLabel1.Text = rm.GetString("writingMatrix");
            System.IO.BinaryWriter writeMatrix = new System.IO.BinaryWriter(File.Create(workingFolder + @"data\a\0\0\matrix" + "\\" + comboBox6.SelectedIndex.ToString("D4")));
            if (tabControl5.TabCount == 2) writeMatrix.Write(1);
            else writeMatrix.Write(0);
            writeMatrix.Write((UInt16)numericUpDown5.Value);
            writeMatrix.Write((UInt16)numericUpDown4.Value);
            for (int z = 0; z < numericUpDown4.Value; z++) // Main Layer
            {
                for (int i = 0; i < numericUpDown5.Value; i++)
                {
                    uint matrixIndex;
                    if ((i < (dataGridView10.ColumnCount)) && (z < (dataGridView10.RowCount)))
                    {
                        if (dataGridView10.Rows[z].Cells[i].Value.ToString().ToUpper() == "NULL")
                        {
                            matrixIndex = 0xFFFFFFFF;
                        }
                        else
                        {
                            matrixIndex = Convert.ToUInt32(dataGridView10.Rows[z].Cells[i].Value);
                        }
                    }
                    else
                    {
                        matrixIndex = 0xFFFFFFFF;
                    }
                    writeMatrix.Write((uint)matrixIndex);
                }
            }
            if (tabControl5.TabCount == 2)
            {
                for (int z = 0; z < numericUpDown4.Value; z++) // Main Layer
                {
                    for (int i = 0; i < numericUpDown5.Value; i++)
                    {
                        uint matrixIndex;
                        if ((i < (dataGridView8.ColumnCount)) && (z < (dataGridView8.RowCount)))
                        {
                            if (dataGridView8.Rows[z].Cells[i].Value.ToString().ToUpper() == "NULL")
                            {
                                matrixIndex = 0xFFFFFFFF;
                            }
                            else
                            {
                                matrixIndex = Convert.ToUInt32(dataGridView8.Rows[z].Cells[i].Value);
                            }
                        }
                        else
                        {
                            matrixIndex = 0xFFFFFFFF;
                        }
                        writeMatrix.Write((uint)matrixIndex);
                    }
                }
            }
            writeMatrix.Close();
            comboBox6_SelectedIndexChanged(null, null);
            toolStripStatusLabel1.Text = rm.GetString("matrixSaved");
        }

        private void button17_Click(object sender, EventArgs e) // View Map Matrix
        {
            matrixEditorPath = matrixPath + "\\" + comboBox1.SelectedIndex.ToString("D4");
            Form7 matrixViewer = new Form7();
            matrixViewer.ShowDialog(this);
        }

        #endregion

        #region Grid Restrictions

        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041)
            {
                if (e.ColumnIndex == dataGridView1.Columns[4].Index || e.ColumnIndex == dataGridView1.Columns[5].Index || e.ColumnIndex == dataGridView1.Columns[6].Index || e.ColumnIndex == dataGridView1.Columns[7].Index || e.ColumnIndex == dataGridView1.Columns[8].Index || e.ColumnIndex == dataGridView1.Columns[9].Index || e.ColumnIndex == dataGridView1.Columns[10].Index || e.ColumnIndex == dataGridView1.Columns[11].Index)
                {
                    uint number;
                    if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 65535)
                    {
                        MessageBox.Show(rm.GetString("value65535"));
                        dataGridView1.CancelEdit();
                        return;
                    }
                }
                if (e.ColumnIndex == dataGridView1.Columns[2].Index || e.ColumnIndex == dataGridView1.Columns[3].Index || e.ColumnIndex == dataGridView1.Columns[13].Index || e.ColumnIndex == dataGridView1.Columns[14].Index || e.ColumnIndex == dataGridView1.Columns[15].Index || e.ColumnIndex == dataGridView1.Columns[16].Index)
                {
                    uint number;
                    if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 255)
                    {
                        MessageBox.Show(rm.GetString("value255"));
                        dataGridView1.CancelEdit();
                        return;
                    }
                }
            }
            if (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
            {
                if (e.ColumnIndex == dataGridView1.Columns[4].Index || e.ColumnIndex == dataGridView1.Columns[5].Index || e.ColumnIndex == dataGridView1.Columns[6].Index || e.ColumnIndex == dataGridView1.Columns[7].Index || e.ColumnIndex == dataGridView1.Columns[8].Index || e.ColumnIndex == dataGridView1.Columns[9].Index || e.ColumnIndex == dataGridView1.Columns[10].Index || e.ColumnIndex == dataGridView1.Columns[11].Index)
                {
                    uint number;
                    if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 65535)
                    {
                        MessageBox.Show(rm.GetString("value65535"));
                        dataGridView1.CancelEdit();
                        return;
                    }
                }
                if (e.ColumnIndex == dataGridView1.Columns[2].Index || e.ColumnIndex == dataGridView1.Columns[3].Index || e.ColumnIndex == dataGridView1.Columns[13].Index || e.ColumnIndex == dataGridView1.Columns[14].Index || e.ColumnIndex == dataGridView1.Columns[15].Index || e.ColumnIndex == dataGridView1.Columns[16].Index || e.ColumnIndex == dataGridView1.Columns[17].Index)
                {
                    uint number;
                    if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 255)
                    {
                        MessageBox.Show(rm.GetString("value255"));
                        dataGridView1.CancelEdit();
                        return;
                    }
                }
            }
            if (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049)
            {
                if (e.ColumnIndex == dataGridView1.Columns[4].Index || e.ColumnIndex == dataGridView1.Columns[5].Index || e.ColumnIndex == dataGridView1.Columns[6].Index || e.ColumnIndex == dataGridView1.Columns[7].Index || e.ColumnIndex == dataGridView1.Columns[8].Index || e.ColumnIndex == dataGridView1.Columns[9].Index || e.ColumnIndex == dataGridView1.Columns[10].Index)
                {
                    uint number;
                    if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 65535)
                    {
                        MessageBox.Show(rm.GetString("value65535"));
                        dataGridView1.CancelEdit();
                        return;
                    }
                }
                if (e.ColumnIndex == dataGridView1.Columns[2].Index || e.ColumnIndex == dataGridView1.Columns[3].Index || e.ColumnIndex == dataGridView1.Columns[12].Index || e.ColumnIndex == dataGridView1.Columns[13].Index || e.ColumnIndex == dataGridView1.Columns[14].Index || e.ColumnIndex == dataGridView1.Columns[15].Index || e.ColumnIndex == dataGridView1.Columns[16].Index)
                {
                    uint number;
                    if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 255)
                    {
                        MessageBox.Show(rm.GetString("value255"));
                        dataGridView1.CancelEdit();
                        return;
                    }
                }
            }
        }

        private void dataGridView7_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (isBW == true)
            {
                if (e.ColumnIndex == dataGridView7.Columns[3].Index || e.ColumnIndex == dataGridView7.Columns[4].Index || e.ColumnIndex == dataGridView7.Columns[5].Index || e.ColumnIndex == dataGridView7.Columns[6].Index || e.ColumnIndex == dataGridView7.Columns[7].Index || e.ColumnIndex == dataGridView7.Columns[8].Index || e.ColumnIndex == dataGridView7.Columns[9].Index || e.ColumnIndex == dataGridView7.Columns[10].Index || e.ColumnIndex == dataGridView7.Columns[11].Index || e.ColumnIndex == dataGridView7.Columns[12].Index || e.ColumnIndex == dataGridView7.Columns[14].Index || e.ColumnIndex == dataGridView7.Columns[15].Index || e.ColumnIndex == dataGridView7.Columns[22].Index || e.ColumnIndex == dataGridView7.Columns[25].Index || e.ColumnIndex == dataGridView7.Columns[26].Index || e.ColumnIndex == dataGridView7.Columns[27].Index)
                {
                    uint number;
                    if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 65535)
                    {
                        MessageBox.Show(rm.GetString("value65535"));
                        dataGridView7.CancelEdit();
                        return;
                    }
                }
                if (e.ColumnIndex == dataGridView7.Columns[1].Index || e.ColumnIndex == dataGridView7.Columns[2].Index || e.ColumnIndex == dataGridView7.Columns[17].Index || e.ColumnIndex == dataGridView7.Columns[18].Index || e.ColumnIndex == dataGridView7.Columns[19].Index || e.ColumnIndex == dataGridView7.Columns[20].Index || e.ColumnIndex == dataGridView7.Columns[22].Index || e.ColumnIndex == dataGridView7.Columns[23].Index)
                {
                    uint number;
                    if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 255)
                    {
                        MessageBox.Show(rm.GetString("value255"));
                        dataGridView7.CancelEdit();
                        return;
                    }
                }
            }
            if (isB2W2 == true)
            {
                if (e.ColumnIndex == dataGridView7.Columns[3].Index || e.ColumnIndex == dataGridView7.Columns[4].Index || e.ColumnIndex == dataGridView7.Columns[5].Index || e.ColumnIndex == dataGridView7.Columns[6].Index || e.ColumnIndex == dataGridView7.Columns[7].Index || e.ColumnIndex == dataGridView7.Columns[8].Index || e.ColumnIndex == dataGridView7.Columns[9].Index || e.ColumnIndex == dataGridView7.Columns[10].Index || e.ColumnIndex == dataGridView7.Columns[11].Index || e.ColumnIndex == dataGridView7.Columns[12].Index || e.ColumnIndex == dataGridView7.Columns[14].Index || e.ColumnIndex == dataGridView7.Columns[15].Index || e.ColumnIndex == dataGridView7.Columns[22].Index || e.ColumnIndex == dataGridView7.Columns[23].Index || e.ColumnIndex == dataGridView7.Columns[24].Index || e.ColumnIndex == dataGridView7.Columns[25].Index || e.ColumnIndex == dataGridView7.Columns[26].Index || e.ColumnIndex == dataGridView7.Columns[27].Index)
                {
                    uint number;
                    if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 65535)
                    {
                        MessageBox.Show(rm.GetString("value65535"));
                        dataGridView7.CancelEdit();
                        return;
                    }
                }
                if (e.ColumnIndex == dataGridView7.Columns[1].Index || e.ColumnIndex == dataGridView7.Columns[2].Index || e.ColumnIndex == dataGridView7.Columns[17].Index || e.ColumnIndex == dataGridView7.Columns[18].Index || e.ColumnIndex == dataGridView7.Columns[19].Index || e.ColumnIndex == dataGridView7.Columns[20].Index || e.ColumnIndex == dataGridView7.Columns[21].Index)
                {
                    uint number;
                    if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 255)
                    {
                        MessageBox.Show(rm.GetString("value255"));
                        dataGridView7.CancelEdit();
                        return;
                    }
                }
            }
        }

        private void dataGridView2_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (tabControl2.SelectedIndex == 0)
            {
                uint number;
                if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 65535)
                {
                    MessageBox.Show(rm.GetString("value65535"));
                    dataGridView2.CancelEdit();
                    return;
                }
            }
        }

        private void dataGridView4_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (tabControl2.SelectedIndex == 2)
            {
                if (e.FormattedValue.ToString().ToUpper() == "NULL")
                {
                    dataGridView4.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "NULL";
                    return;
                }
                uint number;
                if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 65535)
                {
                    MessageBox.Show(rm.GetString("value65535"));
                    dataGridView4.CancelEdit();
                    return;
                }
            }
        }

        private void dataGridView3_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (tabControl2.SelectedIndex == 1)
            {
                uint number;
                if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 256)
                {
                    MessageBox.Show("Value must be a number between 0 and 256");
                    dataGridView3.CancelEdit();
                    return;
                }
            }
        }

        private void dataGridView10_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) // Check Gen V Matrix
        {
            if (e.FormattedValue.ToString().ToUpper() == "NULL")
            {
                dataGridView10.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "NULL";
                return;
            }
            uint number;
            if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 0xFFFFFFFF)
            {
                MessageBox.Show(rm.GetString("valueFFFFFFFF"));
                dataGridView10.CancelEdit();
                return;
            }
        }

        private void dataGridView8_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) // Check Gen V Matrix
        {
            if (tabControl5.TabCount == 2)
            {
                if (e.FormattedValue.ToString().ToUpper() == "NULL")
                {
                    dataGridView8.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = "NULL";
                    return;
                }
                uint number;
                if (!uint.TryParse(e.FormattedValue.ToString(), out number) || number < 0 || number > 0xFFFFFFFF)
                {
                    MessageBox.Show(rm.GetString("valueFFFFFFFF"));
                    dataGridView8.CancelEdit();
                    return;
                }
            }
        }

        private void dataGridView2_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            (e.Control as TextBox).MaxLength = 5;
        }

        private void dataGridView3_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            (e.Control as TextBox).MaxLength = 3;
        }

        private void dataGridView4_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            (e.Control as TextBox).MaxLength = 5;
        }

        #endregion

        #region Map Editor

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) // Select Map
        {
            dataGridView5.Rows.Clear();
            saveModeON = false;
            mapIndex = comboBox2.SelectedIndex;
            toolStripStatusLabel1.Text = rm.GetString("readingMap");
            System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4")));
            permissionSize = (readMap.ReadByte() + (readMap.ReadByte() << 8) + (readMap.ReadByte() << 16) + (readMap.ReadByte() << 24)); // Read Move Permissions Section Size
            buildingsSize = (readMap.ReadByte() + (readMap.ReadByte() << 8) + (readMap.ReadByte() << 16) + (readMap.ReadByte() << 24)); // Read Buildings Section Size
            modelSize = (readMap.ReadByte() + (readMap.ReadByte() << 8) + (readMap.ReadByte() << 16) + (readMap.ReadByte() << 24)); // Read BMD0 Section Size
            terrainSize = (readMap.ReadByte() + (readMap.ReadByte() << 8) + (readMap.ReadByte() << 16) + (readMap.ReadByte() << 24)); // Read Terrain Size
            if (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049)
            {
                readMap.BaseStream.Position += 0x2;
                unknownSize = (readMap.ReadByte() + (readMap.ReadByte() << 8));
                readMap.BaseStream.Position += unknownSize;
            }
            int xCoord = 0;
            int zCoord = 0;
            for (int i = 0; i < 32; i++)
            {
                dataGridView5.Rows.Add(); // Creates 32x32 grid
                dataGridView5.Rows[i].ReadOnly = false;
            }
            for (int i = 0; i < 32; i++)
            {
                for (int counter = 0; counter < 32; counter++)
                {
                    int firstByte = readMap.ReadByte();
                    dataGridView5.Rows[zCoord].Cells[xCoord].Value = firstByte.ToString("X2");
                    StreamReader colors = new StreamReader(@"Data\ColorTable.txt");
                    for (int lineCounter = 0; lineCounter < firstByte; lineCounter++) // Chooses line
                    {
                        colors.ReadLine();
                    }
                    string colorString = colors.ReadLine();
                    dataGridView5.Rows[zCoord].Cells[xCoord].Style.BackColor = System.Drawing.ColorTranslator.FromHtml(colorString.Substring(5, 7)); // Shows backcolor
                    dataGridView5.Rows[zCoord].Cells[xCoord].Style.ForeColor = System.Drawing.ColorTranslator.FromHtml(colorString.Substring(13, 7)); // Shows forecolor
                    int secondByte = readMap.ReadByte();
                    if (secondByte == 128 && colorString.Substring(5, 7) == "#FFFFFF" && colorString.Substring(13, 7) == "#000000") // "No Movements"
                    {
                        dataGridView5.Rows[zCoord].Cells[xCoord].Style.BackColor = System.Drawing.ColorTranslator.FromHtml("#FF0000"); // Red
                        dataGridView5.Rows[zCoord].Cells[xCoord].Style.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF"); // White
                    }
                    if (secondByte == 4 && colorString.Substring(5, 7) == "#FFFFFF" && colorString.Substring(13, 7) == "#000000") // HGSS "No Special Permissions"
                    {
                        dataGridView5.Rows[zCoord].Cells[xCoord].Style.BackColor = System.Drawing.ColorTranslator.FromHtml("#99FF66"); // Light Green
                        dataGridView5.Rows[zCoord].Cells[xCoord].Style.ForeColor = System.Drawing.ColorTranslator.FromHtml("#000000"); // Black
                    }
                    xCoord++;
                    colors.Close();
                }
                zCoord++;
                xCoord = 0;
            }
            if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
            {
                readMap.BaseStream.Position = 0x10; // Jumps to start of permission section
                modelTileset = workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set";
            }
            else
            {
                readMap.BaseStream.Position = 0x14 + unknownSize; // Jumps to start of permission section
                modelTileset = workingFolder + @"data\a\0\4\texture";
            }
            byte[] bytes = new byte[2048];
            readMap.Read(bytes, 0, 2048);
            mapRAM.Position = 0x0;
            mapRAM.Write(bytes, 0, 2048);
            toolStripStatusLabel1.Text = rm.GetString("ready");
            saveModeON = true;

            #region Create 3D Model for Viewer
            if (Form1.gameID == 0x45414441 || Form1.gameID == 0x45415041 || Form1.gameID == 0x53414441 || Form1.gameID == 0x53415041 || Form1.gameID == 0x46414441 || Form1.gameID == 0x46415041 || Form1.gameID == 0x49414441 || Form1.gameID == 0x49415041 || Form1.gameID == 0x44414441 || Form1.gameID == 0x44415041 || Form1.gameID == 0x4A414441 || Form1.gameID == 0x4A415041 || Form1.gameID == 0x4B414441 || Form1.gameID == 0x4B415041 || Form1.gameID == 0x45555043 || Form1.gameID == 0x53555043 || Form1.gameID == 0x46555043 || Form1.gameID == 0x49555043 || Form1.gameID == 0x44555043 || Form1.gameID == 0x4A555043 || Form1.gameID == 0x4B555043)
            {
                readMap.BaseStream.Position = 0x10 + Form1.permissionSize + Form1.buildingsSize;
            }
            else
            {
                readMap.BaseStream.Position = 0x14 + Form1.unknownSize + Form1.permissionSize + Form1.buildingsSize;
            }
            System.IO.BinaryWriter export = new System.IO.BinaryWriter(File.OpenWrite(Path.GetTempPath() + "map.nsbmd"));
            export.BaseStream.Position = 0x00;
            export.BaseStream.SetLength(Form1.modelSize);
            for (int i = 0; i < Form1.modelSize; i++)
            {
                export.Write(readMap.ReadByte()); // Reads byte and writes it to file
            }
            export.Close();
            readMap.Close();
            #endregion

            comboBox4.SelectedIndex = 0;
            comboBox4_SelectedIndexChanged(null, null);
            return;
        }

        private void comboBox8_SelectedIndexChanged(object sender, EventArgs e) // Gen V Select Map
        {
            toolStripStatusLabel1.Text = rm.GetString("readingMap");
            button40.Enabled = true;
            button39.Enabled = true;
            checkBox5.Enabled = false;
            checkBox5.Checked = false;
            mapIndex = comboBox8.SelectedIndex;
            System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4")));
            mapType = (int)readMap.ReadUInt32();
            vmodelOffset = (int)readMap.ReadUInt32();
            if (mapType == 0x0002474E) // NG Map
            {
                vbldOffset = (int)readMap.ReadUInt32();
                voffsetPos = (int)readMap.BaseStream.Position;
                vendOffset = (int)readMap.ReadUInt32();
                offset2 = vbldOffset;
                button40.Enabled = false;
                button39.Enabled = false;
                tabControl6.TabPages.Remove(tabPage20);
            }
            else if (mapType == 0x00034452 || mapType == 0x00034257)
            {
                vpermOffset = (int)readMap.ReadUInt32();
                vbldOffset = (int)readMap.ReadUInt32();
                voffsetPos = (int)readMap.BaseStream.Position;
                vendOffset = (int)readMap.ReadUInt32();
                offset2 = vpermOffset;
                offset3 = vbldOffset;
                if (tabControl6.TabPages.Count != 3)
                {
                    tabControl6.TabPages.Remove(tabPage21);
                    tabControl6.TabPages.Add(tabPage20);
                    tabControl6.TabPages.Add(tabPage21);
                }
            }
            else
            {
                vpermOffset = (int)readMap.ReadUInt32();
                vunknownOffset = (int)readMap.ReadUInt32();
                vbldOffset = (int)readMap.ReadUInt32();
                voffsetPos = (int)readMap.BaseStream.Position;
                vendOffset = (int)readMap.ReadUInt32();
                offset2 = vpermOffset;
                offset3 = vunknownOffset;
                checkBox5.Enabled = true;
                if (tabControl6.TabPages.Count != 3)
                {
                    tabControl6.TabPages.Remove(tabPage21);
                    tabControl6.TabPages.Add(tabPage20);
                    tabControl6.TabPages.Add(tabPage21);
                }
            }
            if (mapType != 0x0002474E)
            {
                readMap.BaseStream.Position = vpermOffset + 0x4;
                byte[] bytes = new byte[8192];
                readMap.Read(bytes, 0, 8192);
                mapRAM.Position = 0x0;
                mapRAM.Write(bytes, 0, 8192);
                mapRAM.Position = 0x0;
                saveModeON = true;

                for (int i = 0; i < 32; i++)
                {
                    dataGridView9.Rows.Add(); // Creates 32x32 grid
                    dataGridView9.Rows[i].ReadOnly = false;
                }
                changeLayer(null, null);
            }
            toolStripStatusLabel1.Text = rm.GetString("ready");

            System.IO.BinaryWriter export = new System.IO.BinaryWriter(File.Create(Path.GetTempPath() + "map.nsbmd"));
            export.BaseStream.SetLength(offset2 - vmodelOffset);
            readMap.BaseStream.Position = vmodelOffset;
            for (int i = 0; i < offset2 - vmodelOffset; i++)
            {
                export.Write(readMap.ReadByte()); // Reads byte and writes it to file
            }
            export.Close();
            readMap.Close();

            comboBox7.SelectedIndex = 0;
            comboBox7_SelectedIndexChanged(null, null);
        }

        private void changeLayer(object sender, EventArgs e) // Gen V Change Layer
        {
            saveModeON = false;
            mapRAM.Position = 0x0;
            for (int i = 0; i < 32; i++)
            {
                for (int j = 0; j < 32; j++)
                {
                    if (radioButton9.Checked) dataGridView9.Rows[i].Cells[j].Value = mapRAM.ReadByte().ToString("X2");
                    else mapRAM.Position++;
                    if (radioButton16.Checked) dataGridView9.Rows[i].Cells[j].Value = mapRAM.ReadByte().ToString("X2");
                    else mapRAM.Position++;
                    if (radioButton19.Checked) dataGridView9.Rows[i].Cells[j].Value = mapRAM.ReadByte().ToString("X2");
                    else mapRAM.Position++;
                    if (radioButton20.Checked) dataGridView9.Rows[i].Cells[j].Value = mapRAM.ReadByte().ToString("X2");
                    else mapRAM.Position++;
                    int firstByte = mapRAM.ReadByte();
                    if (radioButton21.Checked) dataGridView9.Rows[i].Cells[j].Value = firstByte.ToString("X2");
                    if (radioButton22.Checked) dataGridView9.Rows[i].Cells[j].Value = mapRAM.ReadByte().ToString("X2");
                    else mapRAM.Position++;
                    StreamReader colors = new StreamReader(@"Data\ColorTableBW.txt");
                    for (int lineCounter = 0; lineCounter < firstByte; lineCounter++) // Chooses line
                    {
                        colors.ReadLine();
                    }
                    string colorString = colors.ReadLine();
                    dataGridView9.Rows[i].Cells[j].Style.BackColor = System.Drawing.ColorTranslator.FromHtml(colorString.Substring(5, 7)); // Shows backcolor
                    dataGridView9.Rows[i].Cells[j].Style.ForeColor = System.Drawing.ColorTranslator.FromHtml(colorString.Substring(13, 7)); // Shows forecolor
                    int secondByte = mapRAM.ReadByte();
                    if (radioButton10.Checked) dataGridView9.Rows[i].Cells[j].Value = secondByte.ToString("X2");
                    if (secondByte == 129 && colorString.Substring(5, 7) == "#FFFFFF" && colorString.Substring(13, 7) == "#000000") // "No Movements"
                    {
                        dataGridView9.Rows[i].Cells[j].Style.BackColor = System.Drawing.ColorTranslator.FromHtml("#FF0000"); // Red
                        dataGridView9.Rows[i].Cells[j].Style.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF"); // White
                    }
                    colors.Close();
                    if (radioButton11.Checked) dataGridView9.Rows[i].Cells[j].Value = mapRAM.ReadByte().ToString("X2");
                    else mapRAM.Position++;
                }
            }
            saveModeON = true;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e) // Gen V Change to Secondary
        {
            if (checkBox5.Enabled)
            {
                if (checkBox5.Checked)
                {
                    System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4")));
                    readMap.BaseStream.Position = vunknownOffset + 0x4;
                    byte[] bytes = new byte[8192];
                    readMap.Read(bytes, 0, 8192);
                    mapRAM.Position = 0x0;
                    mapRAM.Write(bytes, 0, 8192);
                    mapRAM.Position = 0x0;
                    readMap.Close();
                    changeLayer(null, null);
                }
                else
                {
                    System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4")));
                    readMap.BaseStream.Position = vpermOffset + 0x4;
                    byte[] bytes = new byte[8192];
                    readMap.Read(bytes, 0, 8192);
                    mapRAM.Position = 0x0;
                    mapRAM.Write(bytes, 0, 8192);
                    mapRAM.Position = 0x0;
                    readMap.Close();
                    changeLayer(null, null);
                }
            }
        }

        private void dataGridView5_SelectionChanged(object sender, EventArgs e)
        {
            radioModeON = false;
            label5.Text = "X: " + Convert.ToString(dataGridView5.CurrentCellAddress.X + 1);
            label7.Text = "Y: " + Convert.ToString(dataGridView5.CurrentCellAddress.Y + 1);
            mapRAM.Position = dataGridView5.CurrentCellAddress.X * 2 + dataGridView5.CurrentCellAddress.Y * 64 + 0x1; // Jumps to current tile
            int secondByte = mapRAM.ReadByte();
            radioButton2.Enabled = true;
            radioButton6.Enabled = true;
            if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
            {
                radioButton2.Enabled = false; // Disable "Grass" for DPPt
                radioButton6.Enabled = false;
            }
            if (secondByte == 128) radioButton3.Checked = true; // No Passage
            else radioButton1.Checked = true; // Free Passage
            if (secondByte == 4)
            {
                if (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049)
                {
                    radioButton2.Checked = true; // HGSS Grass
                }
                else radioButton1.Checked = true;
            }
            radioModeON = true;
        }

        private void dataGridView9_SelectionChanged(object sender, EventArgs e)
        {
            label15.Text = "X: " + Convert.ToString(dataGridView9.CurrentCellAddress.X + 1);
            label14.Text = "Y: " + Convert.ToString(dataGridView9.CurrentCellAddress.Y + 1);
        }

        private void button5_Click(object sender, EventArgs e) // Export Move Permissions
        {
            SaveFileDialog ef = new SaveFileDialog();
            ef.Title = rm.GetString("exportMove");
            ef.Filter = rm.GetString("moveFile");
            if (ef.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4")));
                if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                {
                    readMap.BaseStream.Position = 0x10;
                }
                else
                {
                    readMap.BaseStream.Position = 0x14 + unknownSize;
                }
                System.IO.BinaryWriter export = new System.IO.BinaryWriter(File.OpenWrite(ef.FileName));
                string exportfilename;
                exportfilename = ef.FileName;
                export.BaseStream.Position = 0x00;
                export.BaseStream.SetLength(permissionSize);

                for (int i = 0; i < permissionSize; i++)
                {
                    export.Write(readMap.ReadByte()); // Reads byte and writes it to file
                }
                export.Close();
                readMap.Close();
            }
        }

        private void button40_Click(object sender, EventArgs e) // Gen V Export Move Permissions
        {
            SaveFileDialog ef = new SaveFileDialog();
            ef.Title = rm.GetString("exportMove");
            ef.Filter = rm.GetString("moveFile");
            if (ef.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4")));
                if (!checkBox5.Checked) readMap.BaseStream.Position = vpermOffset;
                else readMap.BaseStream.Position = vunknownOffset;
                System.IO.BinaryWriter export = new System.IO.BinaryWriter(File.Create(ef.FileName));
                string exportfilename;
                exportfilename = ef.FileName;
                if (!checkBox5.Checked)
                {
                    for (int i = 0; i < 8228; i++)
                    {
                        export.Write(readMap.ReadByte()); // Reads byte and writes it to file
                    }
                }
                else
                {
                    for (int i = 0; i < 8228; i++)
                    {
                        export.Write(readMap.ReadByte()); // Reads byte and writes it to file
                    }
                }
                export.Close();
                readMap.Close();
            }
        }

        private void button8_Click(object sender, EventArgs e) // Export Buildings
        {
            SaveFileDialog ef = new SaveFileDialog();
            ef.Title = rm.GetString("exportBuildings");
            ef.Filter = rm.GetString("buildingsFile");
            if (ef.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4")));
                if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                {
                    readMap.BaseStream.Position = 0x10 + permissionSize;
                }
                else
                {
                    readMap.BaseStream.Position = 0x14 + unknownSize + permissionSize;
                }
                System.IO.BinaryWriter export = new System.IO.BinaryWriter(File.OpenWrite(ef.FileName));
                string exportfilename;
                exportfilename = ef.FileName;
                export.BaseStream.Position = 0x00;
                export.BaseStream.SetLength(buildingsSize);

                for (int i = 0; i < buildingsSize; i++)
                {
                    export.Write(readMap.ReadByte()); // Reads byte and writes it to file
                }
                export.Close();
                readMap.Close();
            }
        }

        private void button38_Click(object sender, EventArgs e) // Gen V Export Buildings
        {
            SaveFileDialog ef = new SaveFileDialog();
            ef.Title = rm.GetString("exportBuildings");
            ef.Filter = rm.GetString("buildingsFile");
            if (ef.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4")));
                readMap.BaseStream.Position = vbldOffset;
                System.IO.BinaryWriter export = new System.IO.BinaryWriter(File.Create(ef.FileName));
                string exportfilename;
                exportfilename = ef.FileName;
                for (int i = 0; i < vendOffset - vbldOffset; i++)
                {
                    export.Write(readMap.ReadByte()); // Reads byte and writes it to file
                }
                export.Close();
                readMap.Close();
            }
        }

        private void button10_Click(object sender, EventArgs e) // Export NSBMD / OBJ
        {
            if (comboBox4.SelectedIndex == 0)
            {
                SaveFileDialog ef = new SaveFileDialog();
                ef.Title = rm.GetString("exportModel");
                ef.Filter = rm.GetString("modelFile");
                if (ef.ShowDialog() == DialogResult.OK)
                {
                    if (ef.FileName.EndsWith(".obj"))
                    {
                        try
                        {
                            NsbmdGlRenderer rendererOBJ = new NsbmdGlRenderer
                            {
                                Model = _nsbmd.models[0]
                            };
                            rendererOBJ.RipModel(ef.FileName);
                        }
                        catch
                        {
                            MessageBox.Show("There was a problem when exporting. Operation aborted.", null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        }
                        return;
                    }

                    System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4")));
                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                    {
                        readMap.BaseStream.Position = 0x10 + permissionSize + buildingsSize;
                    }
                    else
                    {
                        readMap.BaseStream.Position = 0x14 + unknownSize + permissionSize + buildingsSize;
                    }
                    System.IO.BinaryWriter export = new System.IO.BinaryWriter(File.OpenWrite(ef.FileName));
                    string exportfilename;
                    exportfilename = ef.FileName;
                    export.BaseStream.Position = 0x00;
                    export.BaseStream.SetLength(modelSize);

                    for (int i = 0; i < modelSize; i++)
                    {
                        export.Write(readMap.ReadByte()); // Reads byte and writes it to file
                    }
                    export.Close();
                    readMap.Close();
                }
                return;
            }
            else
            {
                SaveFileDialog ef = new SaveFileDialog();
                ef.Title = "Export Model Section with Textures as...";
                ef.Filter = rm.GetString("modelFile");
                if (ef.ShowDialog() == DialogResult.OK)
                {
                    if (ef.FileName.EndsWith(".obj"))
                    {
                        try
                        {
                            NsbmdGlRenderer rendererOBJ = new NsbmdGlRenderer
                            {
                                Model = _nsbmd.models[0]
                            };
                            rendererOBJ.RipModel(ef.FileName);
                        }
                        catch
                        {
                            MessageBox.Show("There was a problem when exporting. Operation aborted.", null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        }
                        return;
                    }

                    System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(Form1.mapFileName + "\\" + Form1.mapIndex.ToString("D4")));
                    if (Form1.gameID == 0x45414441 || Form1.gameID == 0x45415041 || Form1.gameID == 0x53414441 || Form1.gameID == 0x53415041 || Form1.gameID == 0x46414441 || Form1.gameID == 0x46415041 || Form1.gameID == 0x49414441 || Form1.gameID == 0x49415041 || Form1.gameID == 0x44414441 || Form1.gameID == 0x44415041 || Form1.gameID == 0x4A414441 || Form1.gameID == 0x4A415041 || Form1.gameID == 0x4B414441 || Form1.gameID == 0x4B415041 || Form1.gameID == 0x45555043 || Form1.gameID == 0x53555043 || Form1.gameID == 0x46555043 || Form1.gameID == 0x49555043 || Form1.gameID == 0x44555043 || Form1.gameID == 0x4A555043 || Form1.gameID == 0x4B555043)
                    {
                        readMap.BaseStream.Position = 0x10 + Form1.permissionSize + Form1.buildingsSize;
                    }
                    else
                    {
                        readMap.BaseStream.Position = 0x14 + Form1.unknownSize + Form1.permissionSize + Form1.buildingsSize;
                    }
                    System.IO.BinaryWriter export = new System.IO.BinaryWriter(File.OpenWrite(ef.FileName));
                    export.BaseStream.Position = 0x00;
                    export.BaseStream.SetLength(Form1.modelSize);

                    for (int i = 0; i < 0xe; i++)
                    {
                        export.Write(readMap.ReadByte()); // Reads header bytes and writes them to file
                    }
                    export.Write((Int16)0x0002); // Writes texture flag
                    export.Write((Int16)0x0018);
                    export.Write((Int16)0x0);
                    export.Write(0x0); // Writes blank BTX offset
                    readMap.BaseStream.Position += 0x6;
                    for (int i = 0; i < Form1.modelSize - 0x14; i++)
                    {
                        export.Write(readMap.ReadByte()); // Writes model section
                    }
                    readMap.Close();
                    if (Form1.gameID == 0x45414441 || Form1.gameID == 0x45415041 || Form1.gameID == 0x53414441 || Form1.gameID == 0x53415041 || Form1.gameID == 0x46414441 || Form1.gameID == 0x46415041 || Form1.gameID == 0x49414441 || Form1.gameID == 0x49415041 || Form1.gameID == 0x44414441 || Form1.gameID == 0x44415041 || Form1.gameID == 0x4A414441 || Form1.gameID == 0x4A415041 || Form1.gameID == 0x4B414441 || Form1.gameID == 0x4B415041 || Form1.gameID == 0x45555043 || Form1.gameID == 0x53555043 || Form1.gameID == 0x46555043 || Form1.gameID == 0x49555043 || Form1.gameID == 0x44555043 || Form1.gameID == 0x4A555043 || Form1.gameID == 0x4B555043)
                    {
                        File.Copy(workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set\" + (comboBox4.SelectedIndex - 1).ToString("D4"), ef.FileName + "tex");
                    }
                    else
                    {
                        File.Copy(workingFolder + @"data\a\0\4\texture\" + (comboBox4.SelectedIndex - 1).ToString("D4"), ef.FileName + "tex");
                    }
                    System.IO.BinaryReader readTex = new System.IO.BinaryReader(File.OpenRead(ef.FileName + "tex"));
                    long texLength = readTex.BaseStream.Length - 0x14;
                    readTex.BaseStream.Position = 0x14;
                    long texOffset = export.BaseStream.Position;
                    for (int i = 0; i < texLength; i++)
                    {
                        export.Write(readTex.ReadByte()); // Writes BTX section
                    }
                    export.BaseStream.Position = 0x8;
                    export.Write((int)((Form1.modelSize + 0x4) + texLength));
                    export.BaseStream.Position = 0x14;
                    export.Write((int)texOffset);
                    export.Close();
                    readTex.Close();
                    File.Delete(ef.FileName + "tex");
                }
                return;
            }

        }

        private void exportNSBMD(object sender, EventArgs e) // Gen V Export NSBMD / OBJ
        {
            if (comboBox4.SelectedIndex == 0)
            {
                SaveFileDialog ef = new SaveFileDialog();
                ef.Title = rm.GetString("exportModel");
                ef.Filter = rm.GetString("modelFile");
                if (ef.ShowDialog() == DialogResult.OK)
                {
                    if (ef.FileName.EndsWith(".obj"))
                    {
                        try
                        {
                            NsbmdGlRenderer rendererOBJ = new NsbmdGlRenderer
                            {
                                Model = _nsbmd.models[0]
                            };
                            rendererOBJ.RipModel(ef.FileName);
                        }
                        catch
                        {
                            MessageBox.Show("There was a problem when exporting. Operation aborted.", null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        }
                        return;
                    }

                    System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4")));
                    readMap.BaseStream.Position = vmodelOffset;
                    System.IO.BinaryWriter export = new System.IO.BinaryWriter(File.Create(ef.FileName));
                    string exportfilename;
                    exportfilename = ef.FileName;
                    export.BaseStream.Position = 0x00;

                    for (int i = 0; i < offset2 - vmodelOffset; i++)
                    {
                        export.Write(readMap.ReadByte()); // Reads byte and writes it to file
                    }
                    export.Close();
                    readMap.Close();
                }
                return;
            }
            else
            {
                SaveFileDialog ef = new SaveFileDialog();
                ef.Title = "Export Model Section with Textures as...";
                ef.Filter = rm.GetString("modelFile");
                if (ef.ShowDialog() == DialogResult.OK)
                {
                    if (ef.FileName.EndsWith(".obj"))
                    {
                        try
                        {
                            NsbmdGlRenderer rendererOBJ = new NsbmdGlRenderer
                            {
                                Model = _nsbmd.models[0]
                            };
                            rendererOBJ.RipModel(ef.FileName);
                        }
                        catch
                        {
                            MessageBox.Show("There was a problem when exporting. Operation aborted.", null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        }
                        return;
                    }

                    System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4")));
                    readMap.BaseStream.Position = vmodelOffset;
                    System.IO.BinaryWriter export = new System.IO.BinaryWriter(File.Create(ef.FileName));
                    export.BaseStream.Position = 0x00;

                    for (int i = 0; i < 0xe; i++)
                    {
                        export.Write(readMap.ReadByte()); // Reads header bytes and writes them to file
                    }
                    export.Write((Int16)0x0002); // Writes texture flag
                    export.Write((Int16)0x0018);
                    export.Write((Int16)0x0);
                    export.Write(0x0); // Writes blank BTX offset
                    readMap.BaseStream.Position += 0x6;
                    for (int i = 0; i < (offset2 - vmodelOffset) - 0x14; i++)
                    {
                        export.Write(readMap.ReadByte()); // Writes model section
                    }
                    readMap.Close();
                    File.Copy(workingFolder + @"data\a\0\1\tilesets" + "\\" + (comboBox7.SelectedIndex - 1).ToString("D4"), ef.FileName + "tex");
                    System.IO.BinaryReader readTex = new System.IO.BinaryReader(File.OpenRead(ef.FileName + "tex"));
                    long texLength = readTex.BaseStream.Length - 0x14;
                    readTex.BaseStream.Position = 0x14;
                    long texOffset = export.BaseStream.Position;
                    for (int i = 0; i < texLength; i++)
                    {
                        export.Write(readTex.ReadByte()); // Writes BTX section
                    }
                    export.BaseStream.Position = 0x8;
                    export.Write((int)((offset2 - vmodelOffset + 0x4) + texLength));
                    export.BaseStream.Position = 0x14;
                    export.Write((int)texOffset);
                    export.Close();
                    readTex.Close();
                    File.Delete(ef.FileName + "tex");
                }
                return;
            }

        }

        private void button12_Click(object sender, EventArgs e) // Export Terrain
        {
            SaveFileDialog ef = new SaveFileDialog();
            ef.Title = rm.GetString("exportTerrain");
            ef.Filter = rm.GetString("terrainFile");
            if (ef.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4")));
                if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                {
                    readMap.BaseStream.Position = 0x10 + permissionSize + buildingsSize + modelSize;
                }
                else
                {
                    readMap.BaseStream.Position = 0x14 + unknownSize + permissionSize + buildingsSize + modelSize;
                }
                System.IO.BinaryWriter export = new System.IO.BinaryWriter(File.OpenWrite(ef.FileName));
                string exportfilename;
                exportfilename = ef.FileName;
                export.BaseStream.Position = 0x00;
                export.BaseStream.SetLength(terrainSize);

                for (int i = 0; i < terrainSize; i++)
                {
                    export.Write(readMap.ReadByte()); // Reads byte and writes it to file
                }
                export.Close();
                readMap.Close();
            }
        }

        private void button6_Click(object sender, EventArgs e) // Import Move Permissions
        {
            OpenFileDialog ifpermission = new OpenFileDialog();
            ifpermission.Title = rm.GetString("importMove");
            ifpermission.Filter = rm.GetString("moveFile");
            if (ifpermission.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader permissionstream = new System.IO.BinaryReader(File.OpenRead(ifpermission.FileName));
                string importpermission = ifpermission.FileName;
                long importpersize = new System.IO.FileInfo(ifpermission.FileName).Length;
                if (importpersize == 2048)
                {
                    permissionstream.BaseStream.Position = 0x0;
                    System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4")));
                    System.IO.File.Create(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4") + "_edited").Close();
                    System.IO.BinaryWriter write = new System.IO.BinaryWriter(File.OpenWrite(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4") + "_edited"));
                    readMap.BaseStream.Position = 0x0;

                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                    {
                        for (int i = 0; i < (0x10); i++)
                        {
                            write.Write(readMap.ReadByte()); // Reads unmodified bytes and writes them to the main file
                        }
                    }
                    else // HGSS
                    {
                        for (int i = 0; i < (0x14 + unknownSize); i++)
                        {
                            write.Write(readMap.ReadByte()); // Reads unmodified bytes and writes them to the main file
                        }
                    }
                    for (int i = 0; i < importpersize; i++)
                    {
                        write.Write(permissionstream.ReadByte()); // Reads import file bytes and writes them to the main file
                    }
                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                    {
                        readMap.BaseStream.Position = 0x10 + permissionSize;
                    }
                    else // HGSS
                    {
                        readMap.BaseStream.Position = 0x14 + unknownSize + permissionSize;
                    }
                    for (int i = 0; i < (buildingsSize + modelSize + terrainSize); i++)
                    {
                        write.Write(readMap.ReadByte()); // Reads unmodified bytes following and writes them to the main file
                    }
                    readMap.Close();
                    write.Close();
                    permissionstream.Close();
                    File.Delete(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4"));
                    File.Move(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4") + "_edited", mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4"));
                    comboBox2_SelectedIndexChanged(null, null);
                }
                else
                {
                    MessageBox.Show(rm.GetString("invalidFile"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    permissionstream.Close();
                }
            }
        }

        private void button39_Click(object sender, EventArgs e) // Gen V Import Move Permissions
        {
            OpenFileDialog ifpermission = new OpenFileDialog();
            ifpermission.Title = rm.GetString("importMove");
            ifpermission.Filter = rm.GetString("moveFile");
            if (ifpermission.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader permissionstream = new System.IO.BinaryReader(File.OpenRead(ifpermission.FileName));
                string importpermission = ifpermission.FileName;
                long importpersize = new System.IO.FileInfo(ifpermission.FileName).Length;
                if (importpersize == 8228)
                {
                    System.IO.BinaryWriter write = new System.IO.BinaryWriter(File.OpenWrite(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4")));
                    if (!checkBox5.Checked) write.BaseStream.Position = vpermOffset;
                    else write.BaseStream.Position = vunknownOffset;
                    for (int i = 0; i < 8228; i++)
                    {
                        write.Write(permissionstream.ReadByte()); // Reads import file bytes and writes them to the main file
                    }
                    write.Close();
                    permissionstream.Close();
                    comboBox8_SelectedIndexChanged(null, null);
                }
                else
                {
                    MessageBox.Show(rm.GetString("invalidFile"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    permissionstream.Close();
                }
            }
        }

        private void button7_Click(object sender, EventArgs e) // Import Buildings
        {
            OpenFileDialog ifBuilding = new OpenFileDialog();
            ifBuilding.Title = rm.GetString("importBuildings");
            ifBuilding.Filter = rm.GetString("buildingsFile");
            if (ifBuilding.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader buildingStream = new System.IO.BinaryReader(File.OpenRead(ifBuilding.FileName));
                string importBuilding = ifBuilding.FileName;
                long importbldSize = new System.IO.FileInfo(ifBuilding.FileName).Length;

                if (importbldSize % 0x30 == 0)
                {
                    buildingStream.BaseStream.Position = 0x0;
                    System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4")));
                    System.IO.File.Create(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4") + "_edited").Close();
                    System.IO.BinaryWriter write = new System.IO.BinaryWriter(File.OpenWrite(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4") + "_edited"));
                    readMap.BaseStream.Position = 0x0;

                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                    {
                        for (int i = 0; i < (0x10) + permissionSize; i++)
                        {
                            write.Write(readMap.ReadByte()); // Reads unmodified bytes and writes them to the main file
                        }
                    }
                    else // HGSS
                    {
                        for (int i = 0; i < (0x14 + unknownSize + permissionSize); i++)
                        {
                            write.Write(readMap.ReadByte()); // Reads unmodified bytes and writes them to the main file
                        }
                    }
                    for (int i = 0; i < importbldSize; i++)
                    {
                        write.Write(buildingStream.ReadByte()); // Reads import file bytes and writes them to the main file
                    }
                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                    {
                        readMap.BaseStream.Position = 0x10 + permissionSize + buildingsSize;
                    }
                    else // HGSS
                    {
                        readMap.BaseStream.Position = 0x14 + unknownSize + permissionSize + buildingsSize;
                    }
                    for (int i = 0; i < (modelSize + terrainSize); i++)
                    {
                        write.Write(readMap.ReadByte()); // Reads unmodified bytes following and writes them to the main file
                    }
                    write.BaseStream.Position = 0x4;
                    write.Write((int)importbldSize); // Writes new section size to header
                    buildingsSize = Convert.ToInt32(importbldSize);
                    readMap.Close();
                    write.Close();
                    buildingStream.Close();
                    File.Delete(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4"));
                    File.Move(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4") + "_edited", mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4"));
                }
                else
                {
                    MessageBox.Show(rm.GetString("invalidFile"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    buildingStream.Close();
                }
            }
        }

        private void importBLD(object sender, EventArgs e) // Gen V Import Buildings
        {
            OpenFileDialog ifBuilding = new OpenFileDialog();
            ifBuilding.Title = rm.GetString("importBuildings");
            ifBuilding.Filter = rm.GetString("buildingsFile");
            if (ifBuilding.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader buildingStream = new System.IO.BinaryReader(File.OpenRead(ifBuilding.FileName));
                string importBuilding = ifBuilding.FileName;
                long importbldSize = new System.IO.FileInfo(ifBuilding.FileName).Length;

                if ((importbldSize - 4) % 0x10 == 0)
                {
                    buildingStream.BaseStream.Position = 0x0;
                    System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4")));
                    System.IO.File.Create(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4") + "_edited").Close();
                    System.IO.BinaryWriter write = new System.IO.BinaryWriter(File.OpenWrite(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4") + "_edited"));
                    readMap.BaseStream.Position = 0x0;

                    for (int i = 0; i < (vbldOffset); i++)
                    {
                        write.Write(readMap.ReadByte()); // Reads unmodified bytes and writes them to the main file
                    }
                    for (int i = 0; i < importbldSize; i++)
                    {
                        write.Write(buildingStream.ReadByte()); // Reads import file bytes and writes them to the main file
                    }
                    vendOffset = (int)write.BaseStream.Position;
                    readMap.Close();

                    if (mapType == 0x0002474E) // NG Map
                    {
                        write.BaseStream.Position = 0xC;
                    }
                    else if (mapType == 0x00034452 || mapType == 0x00034257)
                    {
                        write.BaseStream.Position = 0x10;
                    }
                    else
                    {
                        write.BaseStream.Position = 0x14;
                    }
                    write.Write((uint)vendOffset);
                    write.Close();
                    buildingStream.Close();
                    File.Delete(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4"));
                    File.Move(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4") + "_edited", workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4"));
                }
                else
                {
                    MessageBox.Show(rm.GetString("invalidFile"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    buildingStream.Close();
                }
            }
        }

        private void button9_Click(object sender, EventArgs e) // Import NSBMD
        {
            OpenFileDialog ifModel = new OpenFileDialog();
            ifModel.Title = rm.GetString("importModel");
            ifModel.Filter = rm.GetString("importModelFile");
            if (ifModel.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader modelStream = new System.IO.BinaryReader(File.OpenRead(ifModel.FileName));
                string importModel = ifModel.FileName;
                long importnsbmdSize = new System.IO.FileInfo(ifModel.FileName).Length;
                int header;
                header = (int)modelStream.ReadUInt32();
                if (header == 809782594)
                {
                    System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4")));
                    System.IO.File.Create(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4") + "_edited").Close();
                    System.IO.BinaryWriter write = new System.IO.BinaryWriter(File.OpenWrite(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4") + "_edited"));
                    readMap.BaseStream.Position = 0x0;

                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                    {
                        for (int i = 0; i < (0x10) + permissionSize + buildingsSize; i++)
                        {
                            write.Write(readMap.ReadByte()); // Reads unmodified bytes and writes them to the main file
                        }
                    }
                    else // HGSS
                    {
                        for (int i = 0; i < (0x14 + unknownSize + permissionSize + buildingsSize); i++)
                        {
                            write.Write(readMap.ReadByte()); // Reads unmodified bytes and writes them to the main file
                        }
                    }

                    modelStream.BaseStream.Position = 0x18;
                    header = (int)modelStream.ReadUInt32();
                    if (header == 810304589)
                    {
                        MessageBox.Show(rm.GetString("embeddedTextures"));
                        modelStream.BaseStream.Position = 0x14;
                        importnsbmdSize = (int)(modelStream.ReadUInt32() - 0x4);
                        write.Write((int)809782594);
                        write.Write((int)0x02FEFF);
                        write.Write((int)importnsbmdSize);
                        write.Write((int)0x010010);
                        write.Write((int)0x14);
                        for (int i = 0; i < importnsbmdSize - 0x14; i++)
                        {
                            write.Write(modelStream.ReadByte()); // Reads import file bytes and writes them to the main file
                        }
                    }
                    else
                    {
                        modelStream.BaseStream.Position = 0x0;
                        for (int i = 0; i < importnsbmdSize; i++)
                        {
                            write.Write(modelStream.ReadByte()); // Reads import file bytes and writes them to the main file
                        }
                    }

                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                    {
                        readMap.BaseStream.Position = 0x10 + permissionSize + buildingsSize + modelSize;
                    }
                    else // HGSS
                    {
                        readMap.BaseStream.Position = 0x14 + unknownSize + permissionSize + buildingsSize + modelSize;
                    }
                    for (int i = 0; i < (terrainSize); i++)
                    {
                        write.Write(readMap.ReadByte()); // Reads unmodified bytes following and writes them to the main file
                    }
                    write.BaseStream.Position = 0x8;
                    write.Write((int)importnsbmdSize); // Writes new section size to header
                    modelSize = Convert.ToInt32(importnsbmdSize);
                    readMap.Close();
                    write.Close();
                    modelStream.Close();
                    File.Delete(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4"));
                    File.Move(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4") + "_edited", mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4"));
                    comboBox2_SelectedIndexChanged(null, null);
                }
                else
                {
                    MessageBox.Show(rm.GetString("invalidFile"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    modelStream.Close();
                }
            }
        }

        private void button35_Click(object sender, EventArgs e) // Gen V Import NSBMD
        {
            OpenFileDialog ifModel = new OpenFileDialog();
            ifModel.Title = rm.GetString("importModel");
            ifModel.Filter = rm.GetString("importModelFile");
            if (ifModel.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader modelStream = new System.IO.BinaryReader(File.OpenRead(ifModel.FileName));
                string importModel = ifModel.FileName;
                long importnsbmdSize = new System.IO.FileInfo(ifModel.FileName).Length;
                int header;
                header = (int)modelStream.ReadUInt32();
                if (header == 809782594)
                {
                    System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4")));
                    System.IO.File.Create(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4") + "_edited").Close();
                    System.IO.BinaryWriter write = new System.IO.BinaryWriter(File.OpenWrite(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4") + "_edited"));
                    readMap.BaseStream.Position = 0x0;

                    for (int i = 0; i < vmodelOffset; i++)
                    {
                        write.Write(readMap.ReadByte()); // Reads unmodified bytes and writes them to the main file
                    }

                    modelStream.BaseStream.Position = 0x18;
                    header = (int)modelStream.ReadUInt32();
                    if (header == 810304589)
                    {
                        MessageBox.Show(rm.GetString("embeddedTextures"));
                        modelStream.BaseStream.Position = 0x14;
                        importnsbmdSize = (int)(modelStream.ReadUInt32() - 0x4);
                        write.Write((int)809782594);
                        write.Write((int)0x02FEFF);
                        write.Write((int)importnsbmdSize);
                        write.Write((int)0x010010);
                        write.Write((int)0x14);
                        for (int i = 0; i < importnsbmdSize - 0x14; i++)
                        {
                            write.Write(modelStream.ReadByte()); // Reads import file bytes and writes them to the main file
                        }
                    }
                    else
                    {
                        modelStream.BaseStream.Position = 0x0;
                        for (int i = 0; i < importnsbmdSize; i++)
                        {
                            write.Write(modelStream.ReadByte()); // Reads import file bytes and writes them to the main file
                        }
                    }
                    readMap.BaseStream.Position = 0x8;
                    int offset = (int)readMap.ReadUInt32();
                    readMap.BaseStream.Position = offset;
                    int size2 = 0;
                    if (mapType == 0x0002474E) // NG Map
                    {
                        size2 = vendOffset - vbldOffset;
                        vbldOffset = (int)write.BaseStream.Position;
                    }
                    else if (mapType == 0x00034452 || mapType == 0x00034257)
                    {
                        size2 = vbldOffset - vpermOffset;
                        vpermOffset = (int)write.BaseStream.Position;
                    }
                    else
                    {
                        size2 = vunknownOffset - vpermOffset;
                        vpermOffset = (int)write.BaseStream.Position;
                    }
                    for (int i = 0; i < (size2); i++)
                    {
                        write.Write(readMap.ReadByte()); // Reads unmodified bytes following and writes them to the main file
                    }

                    int size3 = 0;
                    if (mapType == 0x0002474E) // NG Map
                    {
                        vendOffset = (int)write.BaseStream.Position;
                    }
                    else if (mapType == 0x00034452 || mapType == 0x00034257)
                    {
                        size3 = vendOffset - vbldOffset;
                        vbldOffset = (int)write.BaseStream.Position;
                    }
                    else
                    {
                        size3 = vbldOffset - vunknownOffset;
                        vunknownOffset = (int)write.BaseStream.Position;
                    }
                    for (int i = 0; i < (size3); i++)
                    {
                        write.Write(readMap.ReadByte()); // Reads unmodified bytes following and writes them to the main file
                    }

                    int size4 = 0;
                    if (mapType == 0x00034452 || mapType == 0x00034257)
                    {
                        vendOffset = (int)write.BaseStream.Position;
                    }
                    else
                    {
                        size4 = vendOffset - vbldOffset;
                        vbldOffset = (int)write.BaseStream.Position;
                    }
                    for (int i = 0; i < (size4); i++)
                    {
                        write.Write(readMap.ReadByte()); // Reads unmodified bytes following and writes them to the main file
                    }
                    if (mapType == 0x00044347)
                    {
                        vendOffset = (int)write.BaseStream.Position;
                    }

                    write.BaseStream.Position = 0x8;
                    if (mapType == 0x0002474E) // NG Map
                    {
                        write.Write((uint)vbldOffset);
                        write.Write((uint)vendOffset);
                    }
                    else if (mapType == 0x00034452 || mapType == 0x00034257)
                    {
                        write.Write((uint)vpermOffset);
                        write.Write((uint)vbldOffset);
                        write.Write((uint)vendOffset);
                    }
                    else
                    {
                        write.Write((uint)vpermOffset);
                        write.Write((uint)vunknownOffset);
                        write.Write((uint)vbldOffset);
                        write.Write((uint)vendOffset);
                    }
                    readMap.Close();
                    write.Close();
                    modelStream.Close();
                    File.Delete(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4"));
                    File.Move(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4") + "_edited", workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4"));
                    comboBox8_SelectedIndexChanged(null, null);
                }
                else
                {
                    MessageBox.Show(rm.GetString("invalidFile"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    modelStream.Close();
                }
            }
        }

        private void button11_Click(object sender, EventArgs e) // Import Terrain
        {
            OpenFileDialog ifTerrain = new OpenFileDialog();
            ifTerrain.Title = rm.GetString("importTerrain");
            ifTerrain.Filter = rm.GetString("terrainFile");
            if (ifTerrain.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader terrainStream = new System.IO.BinaryReader(File.OpenRead(ifTerrain.FileName));
                string importTerrain = ifTerrain.FileName;
                long importterSize = new System.IO.FileInfo(ifTerrain.FileName).Length;
                int header;
                header = (terrainStream.ReadByte() + (terrainStream.ReadByte() << 8) + (terrainStream.ReadByte() << 16) + (terrainStream.ReadByte() << 24));
                if (header == 1128809538)
                {
                    terrainStream.BaseStream.Position = 0x0;
                    System.IO.BinaryReader readMap = new System.IO.BinaryReader(File.OpenRead(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4")));
                    System.IO.File.Create(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4") + "_edited").Close();
                    System.IO.BinaryWriter write = new System.IO.BinaryWriter(File.OpenWrite(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4") + "_edited"));
                    readMap.BaseStream.Position = 0x0;

                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                    {
                        for (int i = 0; i < (0x10) + permissionSize + buildingsSize + modelSize; i++)
                        {
                            write.Write(readMap.ReadByte()); // Reads unmodified bytes and writes them to the main file
                        }
                    }
                    else // HGSS
                    {
                        for (int i = 0; i < (0x14 + unknownSize + permissionSize + buildingsSize + modelSize); i++)
                        {
                            write.Write(readMap.ReadByte()); // Reads unmodified bytes and writes them to the main file
                        }
                    }
                    for (int i = 0; i < importterSize; i++)
                    {
                        write.Write(terrainStream.ReadByte()); // Reads import file bytes and writes them to the main file
                    }
                    write.BaseStream.Position = 0xc;
                    write.Write((int)importterSize); // Writes new section size to header
                    terrainSize = Convert.ToInt32(importterSize);
                    readMap.Close();
                    write.Close();
                    terrainStream.Close();
                    File.Delete(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4"));
                    File.Move(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4") + "_edited", mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4"));
                }
                else
                {
                    MessageBox.Show(rm.GetString("invalidFile"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    terrainStream.Close();
                }
            }
        }

        private void button13_Click(object sender, EventArgs e) // Start Buildings Editor
        {
            Form6_Building_List bldList = new Form6_Building_List();
            bldList.ShowDialog(this);
        }

        private void button32_Click(object sender, EventArgs e) // Start V Buildings Editor
        {
            Form6_2_Building_List bldList = new Form6_2_Building_List();
            bldList.ShowDialog(this);
        }

        private void dataGridView5_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) // Permission Check
        {
            uint number;
            if (!uint.TryParse(e.FormattedValue.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out number))
            {
                MessageBox.Show(rm.GetString("valueFF"));
                dataGridView5.CancelEdit();
                return;
            }
        }

        private void dataGridView9_CellValidating(object sender, DataGridViewCellValidatingEventArgs e) // Gen V Permission Check
        {
            uint number;
            if (!uint.TryParse(e.FormattedValue.ToString(), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out number))
            {
                MessageBox.Show(rm.GetString("valueFF"));
                dataGridView9.CancelEdit();
                return;
            }
        }

        private void dataGridView5_CellValueChanged(object sender, DataGridViewCellEventArgs e) // Permission Saving
        {
            dataGridView5.CurrentCell.Value = dataGridView5.CurrentCell.Value.ToString().ToUpper();
            if (dataGridView5.CurrentCell.Value.ToString().Length == 1)
            {
                dataGridView5.CurrentCell.Value = "0" + dataGridView5.CurrentCell.Value.ToString();
            }
            if (saveModeON == true)
            {
                mapRAM.Position = dataGridView5.CurrentCellAddress.X * 2 + dataGridView5.CurrentCellAddress.Y * 64; // Jumps to current tile
                mapRAM.WriteByte(Convert.ToByte(dataGridView5.CurrentCell.Value.ToString(), 16)); // Writes first byte to map
                StreamReader colors = new StreamReader(@"Data\ColorTable.txt");
                for (int lineCounter = 0; lineCounter < Convert.ToByte(dataGridView5.CurrentCell.Value.ToString(), 16); lineCounter++) // Chooses line
                {
                    colors.ReadLine();
                }
                string colorString = colors.ReadLine();
                dataGridView5.CurrentCell.Style.BackColor = System.Drawing.ColorTranslator.FromHtml(colorString.Substring(5, 7)); // Shows backcolor
                dataGridView5.CurrentCell.Style.ForeColor = System.Drawing.ColorTranslator.FromHtml(colorString.Substring(13, 7)); // Shows forecolor
                if (radioButton3.Checked == true && colorString.Substring(5, 7) == "#FFFFFF" && colorString.Substring(13, 7) == "#000000") // "No Movements"
                {
                    dataGridView5.CurrentCell.Style.BackColor = System.Drawing.ColorTranslator.FromHtml("#FF0000"); // Red
                    dataGridView5.CurrentCell.Style.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF"); // White
                }
                else
                    if (radioButton2.Checked == true) // HGSS "No Special Permissions"
                    {
                        dataGridView5.CurrentCell.Style.BackColor = System.Drawing.ColorTranslator.FromHtml("#99FF66"); // Light Green
                        dataGridView5.CurrentCell.Style.ForeColor = System.Drawing.ColorTranslator.FromHtml("#000000"); // Black
                    }
                colors.Close();
            }
        }

        private void dataGridView9_CellValueChanged(object sender, DataGridViewCellEventArgs e) // Gen V Permission Saving
        {
            dataGridView9.CurrentCell.Value = dataGridView9.CurrentCell.Value.ToString().ToUpper();
            if (dataGridView9.CurrentCell.Value.ToString().Length == 1)
            {
                dataGridView9.CurrentCell.Value = "0" + dataGridView9.CurrentCell.Value.ToString();
            }
            if (saveModeON)
            {
                mapRAM.Position = dataGridView9.CurrentCellAddress.X * 8 + dataGridView9.CurrentCellAddress.Y * 256; // Jumps to current tile
                if (radioButton9.Checked) mapRAM.WriteByte(Convert.ToByte(dataGridView9.CurrentCell.Value.ToString(), 16)); // Writes byte to map
                else mapRAM.Position++;
                if (radioButton16.Checked) mapRAM.WriteByte(Convert.ToByte(dataGridView9.CurrentCell.Value.ToString(), 16)); // Writes byte to map
                else mapRAM.Position++;
                if (radioButton19.Checked) mapRAM.WriteByte(Convert.ToByte(dataGridView9.CurrentCell.Value.ToString(), 16)); // Writes byte to map
                else mapRAM.Position++;
                if (radioButton20.Checked) mapRAM.WriteByte(Convert.ToByte(dataGridView9.CurrentCell.Value.ToString(), 16)); // Writes byte to map
                else mapRAM.Position++;
                if (radioButton21.Checked) mapRAM.WriteByte(Convert.ToByte(dataGridView9.CurrentCell.Value.ToString(), 16)); // Writes byte to map
                else mapRAM.Position++;
                if (radioButton22.Checked) mapRAM.WriteByte(Convert.ToByte(dataGridView9.CurrentCell.Value.ToString(), 16)); // Writes byte to map
                else mapRAM.Position++;
                if (radioButton10.Checked) mapRAM.WriteByte(Convert.ToByte(dataGridView9.CurrentCell.Value.ToString(), 16)); // Writes byte to map
                else mapRAM.Position++;
                if (radioButton11.Checked) mapRAM.WriteByte(Convert.ToByte(dataGridView9.CurrentCell.Value.ToString(), 16)); // Writes byte to map
                changeLayer(null, null);
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioModeON == true)
            {
                mapRAM.Position = dataGridView5.CurrentCellAddress.X * 2 + dataGridView5.CurrentCellAddress.Y * 64 + 0x1; // Jumps to current tile
                if (radioButton1.Checked == true) mapRAM.WriteByte(0x0); // Writes second byte to map
                if (radioButton2.Checked == true) mapRAM.WriteByte(0x04); // Writes second byte to map
                if (radioButton3.Checked == true) mapRAM.WriteByte(0x80); // Writes second byte to map
                StreamReader colors = new StreamReader(@"Data\ColorTable.txt");
                for (int lineCounter = 0; lineCounter < Convert.ToByte(dataGridView5.CurrentCell.Value.ToString(), 16); lineCounter++) // Chooses line
                {
                    colors.ReadLine();
                }
                string colorString = colors.ReadLine();
                dataGridView5.CurrentCell.Style.BackColor = System.Drawing.ColorTranslator.FromHtml(colorString.Substring(5, 7)); // Shows backcolor
                dataGridView5.CurrentCell.Style.ForeColor = System.Drawing.ColorTranslator.FromHtml(colorString.Substring(13, 7)); // Shows forecolor
                if (radioButton3.Checked == true && colorString.Substring(5, 7) == "#FFFFFF" && colorString.Substring(13, 7) == "#000000") // "No Movements"
                {
                    dataGridView5.CurrentCell.Style.BackColor = System.Drawing.ColorTranslator.FromHtml("#FF0000"); // Red
                    dataGridView5.CurrentCell.Style.ForeColor = System.Drawing.ColorTranslator.FromHtml("#FFFFFF"); // White
                }
                if (radioButton2.Checked == true) // HGSS "No Special Permissions"
                {
                    dataGridView5.CurrentCell.Style.BackColor = System.Drawing.ColorTranslator.FromHtml("#99FF66"); // Light Green
                    dataGridView5.CurrentCell.Style.ForeColor = System.Drawing.ColorTranslator.FromHtml("#000000"); // Black
                }
                colors.Close();
            }
        }

        private void button16_Click(object sender, EventArgs e) // Writes Permission stream to disk
        {
            if (isBW == true || isB2W2 == true)
            {
                System.IO.BinaryWriter writePerm = new System.IO.BinaryWriter(File.OpenWrite(workingFolder + @"data\a\0\0\maps" + "\\" + comboBox8.SelectedIndex.ToString("D4")));
                if (!checkBox5.Checked) writePerm.BaseStream.Position = vpermOffset + 4;
                else writePerm.BaseStream.Position = vunknownOffset + 4;
                mapRAM.WriteTo(writePerm.BaseStream);
                writePerm.Close();
                statusStrip1.Text = rm.GetString("mapSaved");
                return;
            }
            System.IO.BinaryWriter write = new System.IO.BinaryWriter(File.OpenWrite(mapFileName + "\\" + comboBox2.SelectedIndex.ToString("D4")));
            if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
            {
                write.BaseStream.Position = 0x10; // Jumps to start of permission section
            }
            else
            {
                write.BaseStream.Position = 0x14 + unknownSize; // Jumps to start of permission section
            }
            mapRAM.WriteTo(write.BaseStream);
            write.Close();
            statusStrip1.Text = rm.GetString("mapSaved");
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e) // Paint Permissions
        {
            if (checkBox2.Checked == true)
            {
                radioButton1.Visible = false;
                radioButton2.Visible = false;
                radioButton3.Visible = false;
            }
            if (checkBox2.Checked == false)
            {
                radioButton1.Visible = true;
                radioButton2.Visible = true;
                radioButton3.Visible = true;
            }
        }

        private void dataGridView5_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            if (checkBox2.Checked == true)
            {
                dataGridView5.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ((UpDownBase)numericUpDown3).Text;
                if (radioButton4.Checked == true) radioButton1.Checked = true;
                if (radioButton5.Checked == true) radioButton3.Checked = true;
                if (radioButton6.Checked == true) radioButton2.Checked = true;
            }
        }

        private void dataGridView9_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (checkBox3.Checked == true)
            {
                dataGridView9.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = ((UpDownBase)numericUpDown6).Text;
            }
        }

        private void tabControl3_SelectedIndexChanged(object sender, EventArgs e) // Map Mode Change
        {
            simpleOpenGlControl1_Resize(null, null);
            if (tabControl3.SelectedTab == tabPage8)
            {
                groupBox1.Visible = true;
            }
            else
            {
                groupBox1.Visible = false;
            }
        }

        private void tabControl6_SelectedIndexChanged(object sender, EventArgs e)
        {
            simpleOpenGlControl1_Resize(null, null);
            if (tabControl6.SelectedTab == tabPage20)
            {
                groupBox2.Visible = true;
            }
            else
            {
                groupBox2.Visible = false;
            }
        }

        #endregion

        private void dataGridView1_CellContentDoubleClick(object sender, DataGridViewCellEventArgs e) // Header Links
        {
            // DPPt Links

            if (dataGridView1.CurrentCellAddress.X == 2 && Convert.ToInt32(dataGridView1.CurrentCell.Value) < comboBox12.Items.Count)
            {
                comboBox12.SelectedIndex = Convert.ToInt32(dataGridView1.CurrentCell.Value);
                return;
            }
            if (dataGridView1.CurrentCellAddress.X == 4)
            {
                tabControl1.SelectedIndex = 1;
                comboBox1.SelectedIndex = Convert.ToInt32(dataGridView1.CurrentCell.Value);
                return;
            }
            if (dataGridView1.CurrentCellAddress.X == 5 || dataGridView1.CurrentCellAddress.X == 6)
            {
                tabControl1.SelectedIndex = 4;
                comboBox5.SelectedIndex = Convert.ToInt32(dataGridView1.CurrentCell.Value);
                return;
            }
            if (dataGridView1.CurrentCellAddress.X == 7)
            {
                tabControl1.SelectedIndex = 3;
                comboBox3.SelectedIndex = Convert.ToInt32(dataGridView1.CurrentCell.Value);
                return;
            }
            if (dataGridView1.CurrentCellAddress.X == 10 && (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
            {
                wildIndex = (int)Convert.ToUInt32(dataGridView1.CurrentCell.Value);
                Form9 wildEditor = new Form9();
                wildEditor.ShowDialog(this);
                return;
            }
            if (dataGridView1.CurrentCellAddress.X == 11 && (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
            {
                tabControl1.SelectedIndex = 5;
                comboBox10.SelectedIndex = Convert.ToInt32(dataGridView1.CurrentCell.Value);
                return;
            }
            if (dataGridView1.CurrentCellAddress.X == 12 && (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
            {
                wildIndex = nameText.IndexOf(dataGridView1.CurrentCell.Value.ToString());
                Form4_2 nameEditor = new Form4_2();
                nameEditor.ShowDialog(this);
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = nameText[wildIndex];
                return;
            }
            if (dataGridView1.CurrentCellAddress.X == 16 && (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041))
            {
                mapFlags = Convert.ToUInt16(dataGridView1.CurrentCell.Value);
                Form4 flagEditor = new Form4();
                flagEditor.ShowDialog(this);
                dataGridView1.CurrentCell.Value = mapFlags;
                return;
            }

            // Platinum Links

            if (dataGridView1.CurrentCellAddress.X == 17 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
            {
                mapFlags = Convert.ToUInt16(dataGridView1.CurrentCell.Value);
                Form4 flagEditor = new Form4();
                flagEditor.ShowDialog(this);
                dataGridView1.CurrentCell.Value = mapFlags;
                return;
            }

            // HGSS Links

            if (dataGridView13.CurrentCellAddress.X == 6)
            {
                tabControl1.SelectedIndex = 1;
                comboBox1.SelectedIndex = Convert.ToInt32(dataGridView13.CurrentCell.Value);
                return;
            }
            if (dataGridView13.CurrentCellAddress.X == 7 || dataGridView13.CurrentCellAddress.X == 8)
            {
                tabControl1.SelectedIndex = 4;
                comboBox5.SelectedIndex = Convert.ToInt32(dataGridView13.CurrentCell.Value);
                return;
            }
            if (dataGridView13.CurrentCellAddress.X == 9)
            {
                tabControl1.SelectedIndex = 3;
                comboBox3.SelectedIndex = Convert.ToInt32(dataGridView13.CurrentCell.Value);
                return;
            }
            if (dataGridView13.CurrentCellAddress.X == 12 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
            {
                tabControl1.SelectedIndex = 5;
                comboBox10.SelectedIndex = Convert.ToInt32(dataGridView13.CurrentCell.Value);
                return;
            }
            if (dataGridView13.CurrentCellAddress.X == 13 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
            {
                wildIndex = nameText.IndexOf(dataGridView13.CurrentCell.Value.ToString());
                Form4_2 nameEditor = new Form4_2();
                nameEditor.ShowDialog(this);
                dataGridView13.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = nameText[wildIndex];
                return;
            }
            if (dataGridView13.CurrentCellAddress.X == 2 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
            {
                wildIndex = (int)Convert.ToUInt32(dataGridView13.CurrentCell.Value);
                Form9 wildEditor = new Form9();
                wildEditor.ShowDialog(this);
                return;
            }
            if (dataGridView13.CurrentCellAddress.X == 18 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
            {
                mapFlags = Convert.ToUInt16(dataGridView13.CurrentCell.Value);
                Form4 flagEditor = new Form4();
                flagEditor.ShowDialog(this);
                dataGridView13.CurrentCell.Value = mapFlags;
                return;
            }
        }

        private void dataGridView7_CellContentClick(object sender, DataGridViewCellEventArgs e) // V Header Links
        {
            if (dataGridView7.CurrentCellAddress.X == 4)
            {
                tabControl1.SelectedIndex = 1;
                comboBox6.SelectedIndex = Convert.ToInt32(dataGridView7.CurrentCell.Value);
                return;
            }
            if (dataGridView7.CurrentCellAddress.X == 5 || dataGridView7.CurrentCellAddress.X == 6)
            {
                tabControl1.SelectedIndex = 4;
                comboBox5.SelectedIndex = Convert.ToInt32(dataGridView7.CurrentCell.Value);
                return;
            }
            if (dataGridView7.CurrentCellAddress.X == 7)
            {
                tabControl1.SelectedIndex = 3;
                radioButton15.Checked = true;
                comboBox3.SelectedIndex = Convert.ToInt32(dataGridView7.CurrentCell.Value);
                return;
            }

            if (dataGridView7.CurrentCellAddress.X == 16)
            {
                wildIndex = nameText.IndexOf(dataGridView7.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString());
                Form4_2 nameEditor = new Form4_2();
                nameEditor.ShowDialog(this);
                dataGridView7.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = nameText[wildIndex];
                return;
            }
            if (dataGridView7.CurrentCellAddress.X == 21)
            {
                mapFlags = Convert.ToInt32(dataGridView7.CurrentCell.Value);
                Form4 flagEditor = new Form4();
                flagEditor.ShowDialog(this);
                dataGridView7.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = mapFlags;
                return;
            }
            if (dataGridView7.CurrentCellAddress.X == 12)
            {
                wildIndex = Convert.ToInt32(dataGridView7.CurrentCell.Value);
                Form9 wildEditor = new Form9();
                wildEditor.ShowDialog(this);
            }
        }

        private void button15_Click(object sender, EventArgs e) // Load Event Editor
        {
            eventIndex = 0;
            Form5 eventEditor = new Form5();
            eventEditor.ShowDialog(this);
        }

        private void button19_Click(object sender, EventArgs e) // Load Wild Editor
        {
            wildIndex = 0;
            Form9 wildEditor = new Form9();
            wildEditor.ShowDialog(this);
        }

        private void button14_Click(object sender, EventArgs e) // Load Trainer Editor
        {
            Form3 trainerEditor = new Form3();
            trainerEditor.ShowDialog(this);
        }

        private void button21_Click(object sender, EventArgs e) // Load Music File List
        {
            if (soundON == false)
            {
                Form8 soundList = new Form8();
                soundList.Show(this);
                soundON = true;
            }
        }
        
        #region Patches

        private void sPKPackagesToolStripMenuItem_Click(object sender, EventArgs e) // SPK Package Management
        {
            SPK spk = new SPK();
            spk.ShowDialog();
        }

        #endregion

        private void pictureBox1_Paint(object sender, PaintEventArgs e) // Draw Game Icon
        {
            if (iconON == true)
            {
                System.IO.BinaryReader readIcon = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"banner.bin"));
                #region Read Icon Palette
                readIcon.BaseStream.Position = 0x220;
                byte firstByte;
                byte secondByte;
                int palR;
                int palG;
                int palB;
                int palCounter = 0;
                int[] paletteArray = new int[48];
                for (int i = 0; i < 16; i++)
                {
                    palR = 0;
                    palG = 0;
                    palB = 0;
                    secondByte = readIcon.ReadByte();
                    firstByte = readIcon.ReadByte();
                    if ((firstByte & (1 << 6)) != 0) palB = palB | (1 << 4);
                    if ((firstByte & (1 << 5)) != 0) palB = palB | (1 << 3);
                    if ((firstByte & (1 << 4)) != 0) palB = palB | (1 << 2);
                    if ((firstByte & (1 << 3)) != 0) palB = palB | (1 << 1);
                    if ((firstByte & (1 << 2)) != 0) palB = palB | (1 << 0);
                    if ((firstByte & (1 << 1)) != 0) palG = palG | (1 << 4);
                    if ((firstByte & (1 << 0)) != 0) palG = palG | (1 << 3);
                    if ((secondByte & (1 << 7)) != 0) palG = palG | (1 << 2);
                    if ((secondByte & (1 << 6)) != 0) palG = palG | (1 << 1);
                    if ((secondByte & (1 << 5)) != 0) palG = palG | (1 << 0);
                    if ((secondByte & (1 << 4)) != 0) palR = palR | (1 << 4);
                    if ((secondByte & (1 << 3)) != 0) palR = palR | (1 << 3);
                    if ((secondByte & (1 << 2)) != 0) palR = palR | (1 << 2);
                    if ((secondByte & (1 << 1)) != 0) palR = palR | (1 << 1);
                    if ((secondByte & (1 << 0)) != 0) palR = palR | (1 << 0);
                    paletteArray[palCounter] = palR * 8;
                    palCounter++;
                    paletteArray[palCounter] = palG * 8;
                    palCounter++;
                    paletteArray[palCounter] = palB * 8;
                    palCounter++;
                }
                #endregion
                #region Read Icon Image
                readIcon.BaseStream.Position = 0x20;
                byte pixelByte;
                int pixelPalId;
                int iconX;
                int iconY = 0;
                int xTile = 0;
                int yTile = 0;
                for (int o = 0; o < 4; o++)
                {
                    for (int a = 0; a < 4; a++)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            iconX = xTile;
                            for (int counter = 0; counter < 4; counter++)
                            {
                                pixelByte = readIcon.ReadByte();
                                pixelPalId = pixelByte & 0x0F;
                                Brush icon = new SolidBrush(Color.FromArgb(255, paletteArray[pixelPalId * 3], paletteArray[pixelPalId * 3 + 1], paletteArray[pixelPalId * 3 + 2]));
                                e.Graphics.FillRectangle(icon, iconX, i + yTile, 1, 1);
                                iconX++;
                                pixelPalId = (pixelByte & 0xF0) >> 4;
                                icon = new SolidBrush(Color.FromArgb(255, paletteArray[pixelPalId * 3], paletteArray[pixelPalId * 3 + 1], paletteArray[pixelPalId * 3 + 2]));
                                e.Graphics.FillRectangle(icon, iconX, i + yTile, 1, 1);
                                iconX++;
                            }
                            iconY++;
                        }
                        iconY = 0;
                        xTile += 8;
                    }
                    xTile = 0;
                    yTile += 8;
                }
                #endregion
                readIcon.Close();
            }
            else return;
        }

        #region 3D Viewer

        #region Properties (1)

        public Tao.Platform.Windows.SimpleOpenGlControl OpenGLControl
        {
            get
            {
                if (isBW || isB2W2)
                {
                    return this.simpleOpenGlControl2;
                }
                else
                {
                    return this.simpleOpenGlControl1;
                }
            }
        }

        #endregion Properties

        #region Delegates and Events (1)

        // Events (1) 

        public event Action RenderScene;

        #endregion Delegates and Events

        public static float ang = 0.0f;
        public static float dist = 12.0f;
        public static float elev = 50.0f;
        public static float tempAng = 0.0f;
        public static float tempDist = 0.0f;
        public static float tempElev = 0.0f;
        public float perspective = 45f;
        public int mouseX;
        public int mouseY;
        public int screenX = 0;
        public int screenY = 0;
        private static NsbmdGlRenderer renderer = new NsbmdGlRenderer();
        private static Nsbmd _nsbmd;
        MKDS_Course_Editor.NSBTA.NSBTA.NSBTA_File ani;
        MKDS_Course_Editor.NSBTP.NSBTP.NSBTP_File tp;
        MKDS_Course_Editor.NSBCA.NSBCA.NSBCA_File ca;

        private void button20_Click(object sender, EventArgs e) // Render 3D Model
        {
            if (isBW || isB2W2)
            {
                simpleOpenGlControl2.Invalidate();
                simpleOpenGlControl2.MakeCurrent();
            }
            else
            {
                simpleOpenGlControl1.Invalidate();
                simpleOpenGlControl1.MakeCurrent();
            }
            Gl.glEnable(Gl.GL_RESCALE_NORMAL);
            Gl.glEnable(Gl.GL_COLOR_MATERIAL);
            Gl.glEnable(Gl.GL_DEPTH_TEST);
            Gl.glEnable(Gl.GL_NORMALIZE);
            Gl.glDisable(Gl.GL_CULL_FACE);
            Gl.glFrontFace(Gl.GL_CCW);
            Gl.glClearDepth(1);
            Gl.glEnable(Gl.GL_ALPHA_TEST);
            Gl.glBlendFunc(Gl.GL_SRC_ALPHA, Gl.GL_ONE_MINUS_SRC_ALPHA);
            Gl.glEnable(Gl.GL_BLEND);
            Gl.glAlphaFunc(Gl.GL_GREATER, 0f);
            Gl.glClearColor(51f / 255f, 51f / 255f, 51f / 255f, 1f);
            float aspect;
            if (isBW || isB2W2)
            {
                Gl.glViewport(0, 0, simpleOpenGlControl2.Width, simpleOpenGlControl2.Height);
                aspect = (float)simpleOpenGlControl2.Width / (float)simpleOpenGlControl2.Height;//(vp[2] - vp[0]) / (vp[3] - vp[1]);
            }
            else
            {
                Gl.glViewport(0, 0, simpleOpenGlControl1.Width, simpleOpenGlControl1.Height);
                aspect = (float)simpleOpenGlControl1.Width / (float)simpleOpenGlControl1.Height;//(vp[2] - vp[0]) / (vp[3] - vp[1]);
            }
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluPerspective(perspective, aspect, 0.02f, 1000000.0f);//0.02f, 32.0f);
            Gl.glTranslatef(0, 0, -dist);
            Gl.glRotatef(elev, 1, 0, 0);
            Gl.glRotatef(ang, 0, 1, 0);
            Gl.glMatrixMode(Gl.GL_MODELVIEW);
            Gl.glLoadIdentity();
            Gl.glTranslatef(0, 0, -dist);
            Gl.glRotatef(elev, 1, 0, 0);
            Gl.glRotatef(-ang, 0, 1, 0);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, new float[] { 1, 1, 1, 0 });
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_POSITION, new float[] { 1, 1, 1, 0 });
            Gl.glLightfv(Gl.GL_LIGHT2, Gl.GL_POSITION, new float[] { 1, 1, 1, 0 });
            Gl.glLightfv(Gl.GL_LIGHT3, Gl.GL_POSITION, new float[] { 1, 1, 1, 0 });
            Gl.glLoadIdentity();
            Gl.glBindTexture(Gl.GL_TEXTURE_2D, 0);
            Gl.glColor3f(1.0f, 1.0f, 1.0f);
            Gl.glDepthMask(Gl.GL_TRUE);
            Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
            int[] aniframeS = new int[0];

            try
            {

                var mod = _nsbmd.models[0];
                if (mod == null)
                    return;
                renderer.Model = _nsbmd.models[0];
                renderer.RenderModel(file_2, ani, aniframeS, aniframeS, aniframeS, aniframeS, aniframeS, ca, false, -1, 0.0f, 0.0f, dist, elev, ang, true, tp, _nsbmd);
            }

            catch { }
        }

        private void Render()
        {
            Gl.glViewport(0, 0, Width, Height);
            var vp = new[] { 0f, 0f, 0f, 0f };
            Gl.glGetFloatv(Gl.GL_VIEWPORT, vp);
            float aspect = (vp[2] - vp[0]) / (vp[3] - vp[1]);
            Gl.glMatrixMode(Gl.GL_PROJECTION);
            Gl.glLoadIdentity();
            Glu.gluPerspective(90.0f, aspect, 0.02f, 32.0f);
            Gl.glTranslatef(0, 0, -dist);
            Gl.glRotatef(elev, 1, 0, 0);
            Gl.glRotatef(ang, 0, 1, 0);

            if (RenderScene != null)
                RenderScene.Invoke();
        }

        private void simpleOpenGlControl1_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }

        private void simpleOpenGlControl1_Resize(object sender, EventArgs e) // Refresh 3D
        {
            if (isBW || isB2W2)
            {
                simpleOpenGlControl2.Invalidate();
            }
            else
            {
                simpleOpenGlControl1.Invalidate();
            }
            button20_Click(null, null);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e) // Wireframe On/Off
        {
            if (checkBox1.Checked == true || checkBox4.Checked == true)
            {
                Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_LINE);
                //simpleOpenGlControl1.Refresh();//.Invalidate();
                button20_Click(null, null);
                simpleOpenGlControl1.Refresh();//.Invalidate();
                simpleOpenGlControl2.Refresh();//.Invalidate();
            }
            else
            {
                Gl.glPolygonMode(Gl.GL_FRONT_AND_BACK, Gl.GL_FILL);
                //simpleOpenGlControl1.Refresh();//.Invalidate();
                button20_Click(null, null);
                simpleOpenGlControl1.Refresh();//.Invalidate();
                simpleOpenGlControl2.Refresh();//.Invalidate();
            }
            return;
        }

        void simpleOpenGlControl1_MouseWheel(object sender, MouseEventArgs e) // Zoom In/Out
        {
            if ((Control.ModifierKeys & Keys.Shift) == Keys.Shift)
            {
                dist += (float)e.Delta / 200;
            }
            else
            {
                dist -= (float)e.Delta / 200;
            }
            simpleOpenGlControl1.Invalidate();
            simpleOpenGlControl2.Invalidate();
            button20_Click(null, null);
        }

        private void timer1_Tick(object sender, EventArgs e) // Mouse Control
        {
            ang -= mouseX - Cursor.Position.X;
            elev -= mouseY - Cursor.Position.Y;
            simpleOpenGlControl1.Invalidate();
            simpleOpenGlControl2.Invalidate();
            button20_Click(null, null);
            Cursor.Position = new Point(mouseX, mouseY);
        }

        private void simpleOpenGlControl1_MouseDown(object sender, MouseEventArgs e) // Begin Mouse Control
        {
            timer1.Enabled = true;
            Cursor.Hide();
            screenX = Screen.PrimaryScreen.WorkingArea.Width;
            screenY = Screen.PrimaryScreen.WorkingArea.Height;
            mouseX = Cursor.Position.X;
            mouseY = Cursor.Position.Y;
            timer1.Start();
        }

        private void simpleOpenGlControl1_MouseUp(object sender, MouseEventArgs e) // End Mouse Control
        {
            timer1.Stop();
            Cursor.Show();
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e) // Select Tileset
        {
            renderer.ClearOBJ();
            if (comboBox4.SelectedIndex == 0)
            {
                Gl.glDisable(Gl.GL_TEXTURE_2D);
                var fileStream = new FileStream(Path.GetTempPath() + "map.nsbmd", FileMode.Open);
                file_2 = Path.GetTempPath() + "map.nsbmd";
                _nsbmd = NsbmdLoader.LoadNsbmd(fileStream);
                fileStream.Close();
                simpleOpenGlControl1.Invalidate();
                button20_Click(null, null);
            }
            else
            {
                _nsbmd.models[0].Palettes.Clear();
                _nsbmd.models[0].Textures.Clear();
                Gl.glEnable(Gl.GL_TEXTURE_2D);
                var fileStream = new FileStream(Path.GetTempPath() + "map.nsbmd", FileMode.Open);
                file_2 = Path.GetTempPath() + "map.nsbmd";
                _nsbmd = NsbmdLoader.LoadNsbmd(fileStream);
                fileStream.Close();
                _nsbmd.materials = LibNDSFormats.NSBTX.NsbtxLoader.LoadNsbtx(new MemoryStream(File.ReadAllBytes(modelTileset + "\\" + (comboBox4.SelectedIndex - 1).ToString("D4"))), out _nsbmd.Textures, out _nsbmd.Palettes);
                try
                {
                    _nsbmd.MatchTextures();
                }
                catch { }
                simpleOpenGlControl1.Invalidate();
                button20_Click(null, null);
            }
        }

        private void comboBox7_SelectedIndexChanged(object sender, EventArgs e) // Select Tileset
        {
            renderer.ClearOBJ();
            if (comboBox7.SelectedIndex == 0)
            {
                Gl.glDisable(Gl.GL_TEXTURE_2D);
                var fileStream = new FileStream(Path.GetTempPath() + "map.nsbmd", FileMode.Open);
                file_2 = Path.GetTempPath() + "map.nsbmd";
                _nsbmd = NsbmdLoader.LoadNsbmd(fileStream);
                fileStream.Close();
                simpleOpenGlControl2.Invalidate();
                button20_Click(null, null);
            }
            else
            {
                _nsbmd.models[0].Palettes.Clear();
                _nsbmd.models[0].Textures.Clear();
                Gl.glEnable(Gl.GL_TEXTURE_2D);
                var fileStream = new FileStream(Path.GetTempPath() + "map.nsbmd", FileMode.Open);
                file_2 = Path.GetTempPath() + "map.nsbmd";
                _nsbmd = NsbmdLoader.LoadNsbmd(fileStream);
                fileStream.Close();
                _nsbmd.materials = LibNDSFormats.NSBTX.NsbtxLoader.LoadNsbtx(new MemoryStream(File.ReadAllBytes(workingFolder + @"data\a\0\1\tilesets" + "\\" + (comboBox7.SelectedIndex - 1).ToString("D4"))), out _nsbmd.Textures, out _nsbmd.Palettes);
                try
                {
                    _nsbmd.MatchTextures();
                }
                catch { }
                simpleOpenGlControl2.Invalidate();
                button20_Click(null, null);
            }
        }

        private void simpleOpenGlControl1_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e) // 3D Navigation
        {
            switch (e.KeyCode)
            {
                case Keys.Add:
                    dist -= 10.0f / 100;
                    simpleOpenGlControl1.Invalidate();
                    simpleOpenGlControl2.Invalidate();
                    button20_Click(null, null);
                    break;
                case Keys.Subtract:
                    dist += 10.0f / 100;
                    simpleOpenGlControl1.Invalidate();
                    simpleOpenGlControl2.Invalidate();
                    button20_Click(null, null);
                    break;
            }
            if (threedON == true)
            {
                switch (e.KeyCode)
                {
                    case Keys.D:
                        ang += 1;
                        simpleOpenGlControl1.Invalidate();
                        simpleOpenGlControl2.Invalidate();
                        button20_Click(null, null);
                        break;
                    case Keys.A:
                        ang -= 1;
                        simpleOpenGlControl1.Invalidate();
                        simpleOpenGlControl2.Invalidate();
                        button20_Click(null, null);
                        break;
                    case Keys.W:
                        elev += 1;
                        simpleOpenGlControl1.Invalidate();
                        simpleOpenGlControl2.Invalidate();
                        button20_Click(null, null);
                        break;
                    case Keys.S:
                        elev -= 1;
                        simpleOpenGlControl1.Invalidate();
                        simpleOpenGlControl2.Invalidate();
                        button20_Click(null, null);
                        break;
                }
            }
        }

        private void radio2D_CheckedChanged(object sender, EventArgs e)
        {
            if (radio2D.Checked == true || radioButton13.Checked == true)
            {
                threedON = false;
                tempAng = ang;
                tempDist = dist;
                tempElev = elev;
                perspective = 4f;
                ang = 0f;
                dist = 173f;
                elev = 90f;
                button20_Click(null, null);
            }
            else
            {
                threedON = true;
                ang = tempAng;
                dist = tempDist;
                elev = tempElev;
                perspective = 45f;
                button20_Click(null, null);
            }
        }

        #endregion

        #region Tileset Editor

        #region NSBTX
        public struct NSBTX_File
        {
            public byte[] Before;
            public byte[] Beafter;
            public header Header;
            public struct header
            {
                public string ID;
                public byte[] Magic;
                public Int32 file_size;
                public Int16 header_size;
                public Int16 nSection;
                public Int32[] Section_Offset;
            }
            public tex0 TEX0;
            public struct tex0
            {
                public string ID;
                public Int32 Section_size;
                public Int32 Padding1;
                public Int32 Texture_Data_Size; //shift << 3
                public Int16 Texture_Info_Offset;
                public Int32 Padding2;
                public Int32 Texture_Data_Offset;
                public Int32 Padding3;
                public Int32 Compressed_Texture_Data_Size; //shift << 3
                public Int16 Compressed_Texture_Info_Offset;
                public Int32 Padding4;
                public Int32 Compressed_Texture_Data_Offset;
                public Int32 Compressed_Texture_Info_Data_Offset;
                public Int32 Padding5;
                public Int32 Palette_Data_Size; //shift << 3
                public Int32 Palette_Info_Offset;
                public Int32 Palette_Data_Offset;
            }
            public texInfo TexInfo;
            public struct texInfo
            {
                public byte dummy;
                public byte num_objs;
                public short section_size;
                public UnknownBlock unknownBlock;
                public Info infoBlock;
                public List<string> names;

                public struct UnknownBlock
                {
                    public short header_size;
                    public short section_size;
                    public int constant; // 0x017F

                    public List<short> unknown1;
                    public List<short> unknown2;
                }
                public struct Info
                {
                    public short header_size;
                    public short data_size;

                    public texInfo[] TexInfo;

                    public struct texInfo
                    {
                        public Int32 Texture_Offset; //shift << 3, relative to start of Texture Data
                        public Int16 Parameters;
                        public byte Width;
                        public byte Unknown1;
                        public byte Height;
                        public byte Unknown2;

                        public byte[] Image;
                        public byte[] spData;
                        // Parameters
                        public byte repeat_X;   // 0 = freeze; 1 = repeat
                        public byte repeat_Y;   // 0 = freeze; 1 = repeat
                        public byte flip_X;     // 0 = no; 1 = flip each 2nd texture (requires repeat)
                        public byte flip_Y;     // 0 = no; 1 = flip each 2nd texture (requires repeat)
                        public ushort width;      // 8 << width
                        public ushort height;     // 8 << height
                        public byte format;     // Texture format
                        public byte color0; // 0 = displayed; 1 = transparent
                        public byte coord_transf; // Texture coordination transformation mode

                        public byte depth;
                        public uint compressedDataStart;
                    }
                }
            }
            public palInfo PalInfo;
            public struct palInfo
            {
                public byte dummy;
                public byte num_objs;
                public short section_size;
                public UnknownBlock unknownBlock;
                public Info infoBlock;
                public List<string> names;

                public struct UnknownBlock
                {
                    public short header_size;
                    public short section_size;
                    public int constant; // 0x017F

                    public List<short> unknown1;
                    public List<short> unknown2;
                }
                public struct Info
                {
                    public short header_size;
                    public short data_size;

                    public palInfo[] PalInfo;

                    public struct palInfo
                    {
                        public Int32 Palette_Offset; //shift << 3, relative to start of Palette Data
                        public Int16 Color0;
                        public Color[] pal;
                    }
                }
            }
        }
        int[] bpp = { 0, 8, 2, 4, 8, 2, 8, 16 };
        private bool convert_4x4texel(uint[] tex, int width, int height, UInt16[] data, Color[] pal, NSMBe4.NSBMD.ImageTexeler.LockBitmap rgbaOut)
        {
            int w = width / 4;
            int h = height / 4;

            // traverse 'w x h blocks' of 4x4-texel
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int index = y * w + x;
                    UInt32 t = tex[index];
                    UInt16 d = data[index];
                    UInt16 addr = (ushort)(d & 0x3fff);
                    UInt16 mode = (ushort)((d >> 14) & 3);

                    // traverse every texel in the 4x4 texels
                    for (int r = 0; r < 4; r++)
                        for (int c = 0; c < 4; c++)
                        {
                            int texel = (int)((t >> ((r * 4 + c) * 2)) & 3);
                            Color pixel = rgbaOut.GetPixel((x * 4 + c), (y * 4 + r));

                            switch (mode)
                            {
                                case 0:
                                    pixel = pal[(addr << 1) + texel];
                                    if (texel == 3) pixel = Color.Transparent; // make it transparent, alpha = 0
                                    break;
                                case 2:
                                    pixel = pal[(addr << 1) + texel];
                                    break;
                                case 1:
                                    switch (texel)
                                    {
                                        case 0:
                                        case 1:
                                            pixel = pal[(addr << 1) + texel];
                                            break;
                                        case 2:
                                            byte R = (byte)((pal[(addr << 1)].R + pal[(addr << 1) + 1].R) / 2L);
                                            byte G = (byte)((pal[(addr << 1)].G + pal[(addr << 1) + 1].G) / 2L);
                                            byte B = (byte)((pal[(addr << 1)].B + pal[(addr << 1) + 1].B) / 2L);
                                            byte A = 0xff;
                                            pixel = Color.FromArgb(A, R, G, B);
                                            break;
                                        case 3:
                                            pixel = Color.Transparent; // make it transparent, alpha = 0
                                            break;
                                    }
                                    break;
                                case 3:
                                    switch (texel)
                                    {
                                        case 0:
                                        case 1:
                                            pixel = pal[(addr << 1) + texel];
                                            break;
                                        case 2:
                                            {
                                                byte R = (byte)((pal[(addr << 1)].R * 5L + pal[(addr << 1) + 1].R * 3L) / 8);
                                                byte G = (byte)((pal[(addr << 1)].G * 5L + pal[(addr << 1) + 1].G * 3L) / 8);
                                                byte B = (byte)((pal[(addr << 1)].B * 5L + pal[(addr << 1) + 1].B * 3L) / 8);
                                                byte A = 0xff;
                                                pixel = Color.FromArgb(A, R, G, B);
                                                break;
                                            }
                                        case 3:
                                            {
                                                byte R = (byte)((pal[(addr << 1)].R * 3L + pal[(addr << 1) + 1].R * 5L) / 8);
                                                byte G = (byte)((pal[(addr << 1)].G * 3L + pal[(addr << 1) + 1].G * 5L) / 8);
                                                byte B = (byte)((pal[(addr << 1)].B * 3L + pal[(addr << 1) + 1].B * 5L) / 8);
                                                byte A = 0xff;
                                                pixel = Color.FromArgb(A, R, G, B);
                                                break;
                                            }
                                    }
                                    break;
                            }
                            rgbaOut.SetPixel((x * 4 + c), (y * 4 + r), pixel);
                            //rgbaOut[(y * 4 + r) * width + (x * 4 + c)] = pixel;
                        }
                }
            return true;
        }
        private void convert_4x4texel_b(byte[] tex, int width, int height, byte[] data, Color[] pal, NSMBe4.NSBMD.ImageTexeler.LockBitmap rgbaOut)
        {
            var list1 = new List<uint>();
            for (int i = 0; i < (tex.Length + 1) / 4; ++i)
                list1.Add(LibNDSFormats.Utils.Read4BytesAsUInt32(tex, i * 4));

            var list2 = new List<UInt16>();
            for (int i = 0; i < (data.Length + 1) / 2; ++i)
                list2.Add(LibNDSFormats.Utils.Read2BytesAsUInt16(data, i * 2));
            var b = convert_4x4texel(list1.ToArray(), width, height, list2.ToArray(), pal, rgbaOut);
        }
        #endregion

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            listBox3.Items.Clear();
            if (radioButton8.Checked == false)
            {
                if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                {
                    editorTileset = workingFolder + @"data\fielddata\areadata\area_map_tex\map_tex_set";
                }
                else
                {
                    editorTileset = workingFolder + @"data\a\0\4\texture";
                }
            }
            else
            {
                if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                {
                    editorTileset = workingFolder + @"data\fielddata\areadata\area_build_model\areabm_texset";
                }
                else
                {
                    editorTileset = workingFolder + @"data\a\0\7\textureBld";
                }
            }
            EndianBinaryReader er = new EndianBinaryReader(File.OpenRead(editorTileset + "\\" + listBox1.SelectedIndex.ToString("D4")), Endianness.LittleEndian);
            nsbtx = new NSBTX_File();
            if (er.ReadString(Encoding.ASCII, 4) != "BTX0")
            {
                MessageBox.Show(rm.GetString("tilesetError"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                er.Close();
                return;
            }
            else
            {
                nsbtx.Header.ID = "BTX0";
                nsbtx.Header.Magic = er.ReadBytes(4);
                nsbtx.Header.file_size = er.ReadInt32();
                nsbtx.Header.header_size = er.ReadInt16();
                nsbtx.Header.nSection = er.ReadInt16();
                nsbtx.Header.Section_Offset = new Int32[nsbtx.Header.nSection];
                for (int i = 0; i < nsbtx.Header.nSection; i++)
                {
                    nsbtx.Header.Section_Offset[i] = er.ReadInt32();
                }
                nsbtx.TEX0.ID = er.ReadString(Encoding.ASCII, 4);
                if (nsbtx.TEX0.ID != "TEX0")
                {
                    MessageBox.Show(rm.GetString("tilesetError"));
                    er.Close();
                    return;
                }
            }
            nsbtx.TEX0.Section_size = er.ReadInt32();
            nsbtx.TEX0.Padding1 = er.ReadInt32();
            nsbtx.TEX0.Texture_Data_Size = (er.ReadInt16() << 3);
            nsbtx.TEX0.Texture_Info_Offset = er.ReadInt16();
            nsbtx.TEX0.Padding2 = er.ReadInt32();
            nsbtx.TEX0.Texture_Data_Offset = er.ReadInt32();
            nsbtx.TEX0.Padding3 = er.ReadInt32();
            nsbtx.TEX0.Compressed_Texture_Data_Size = (er.ReadInt16() << 3);
            nsbtx.TEX0.Compressed_Texture_Info_Offset = er.ReadInt16();
            nsbtx.TEX0.Padding4 = er.ReadInt32();
            nsbtx.TEX0.Compressed_Texture_Data_Offset = er.ReadInt32();
            nsbtx.TEX0.Compressed_Texture_Info_Data_Offset = er.ReadInt32();
            nsbtx.TEX0.Padding5 = er.ReadInt32();
            nsbtx.TEX0.Palette_Data_Size = (er.ReadInt32() << 3);
            nsbtx.TEX0.Palette_Info_Offset = er.ReadInt32();
            nsbtx.TEX0.Palette_Data_Offset = er.ReadInt32();

            nsbtx.TexInfo.dummy = er.ReadByte();
            nsbtx.TexInfo.num_objs = er.ReadByte();
            nsbtx.TexInfo.section_size = er.ReadInt16();

            nsbtx.TexInfo.unknownBlock.header_size = er.ReadInt16();
            nsbtx.TexInfo.unknownBlock.section_size = er.ReadInt16();
            nsbtx.TexInfo.unknownBlock.constant = er.ReadInt32();
            nsbtx.TexInfo.unknownBlock.unknown1 = new List<short>();
            nsbtx.TexInfo.unknownBlock.unknown2 = new List<short>();
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                nsbtx.TexInfo.unknownBlock.unknown1.Add(er.ReadInt16());
                nsbtx.TexInfo.unknownBlock.unknown2.Add(er.ReadInt16());
            }

            nsbtx.TexInfo.infoBlock.header_size = er.ReadInt16();
            nsbtx.TexInfo.infoBlock.data_size = er.ReadInt16();
            nsbtx.TexInfo.infoBlock.TexInfo = new NSBTX_File.texInfo.Info.texInfo[nsbtx.TexInfo.num_objs];
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                nsbtx.TexInfo.infoBlock.TexInfo[i].Texture_Offset = (er.ReadInt16() << 3);
                nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters = er.ReadInt16();
                nsbtx.TexInfo.infoBlock.TexInfo[i].Width = er.ReadByte();
                nsbtx.TexInfo.infoBlock.TexInfo[i].Unknown1 = er.ReadByte();
                nsbtx.TexInfo.infoBlock.TexInfo[i].Height = er.ReadByte();
                nsbtx.TexInfo.infoBlock.TexInfo[i].Unknown2 = er.ReadByte();
                nsbtx.TexInfo.infoBlock.TexInfo[i].coord_transf = (byte)(nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters & 14);
                nsbtx.TexInfo.infoBlock.TexInfo[i].color0 = (byte)((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 13) & 1);
                nsbtx.TexInfo.infoBlock.TexInfo[i].format = (byte)((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 10) & 7);
                nsbtx.TexInfo.infoBlock.TexInfo[i].height = (byte)(8 << ((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 7) & 7));
                nsbtx.TexInfo.infoBlock.TexInfo[i].width = (byte)(8 << ((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 4) & 7));
                nsbtx.TexInfo.infoBlock.TexInfo[i].flip_Y = (byte)((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 3) & 1);
                nsbtx.TexInfo.infoBlock.TexInfo[i].flip_X = (byte)((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 2) & 1);
                nsbtx.TexInfo.infoBlock.TexInfo[i].repeat_Y = (byte)((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 1) & 1);
                nsbtx.TexInfo.infoBlock.TexInfo[i].repeat_X = (byte)(nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters & 1);
                nsbtx.TexInfo.infoBlock.TexInfo[i].depth = (byte)bpp[nsbtx.TexInfo.infoBlock.TexInfo[i].format];
                if (nsbtx.TexInfo.infoBlock.TexInfo[i].width == 0x00)
                    switch (nsbtx.TexInfo.infoBlock.TexInfo[i].Unknown1 & 0x3)
                    {
                        case 2:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].width = 0x200;
                            break;
                        default:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].width = 0x100;
                            break;
                    }
                if (nsbtx.TexInfo.infoBlock.TexInfo[i].height == 0x00)
                    switch ((nsbtx.TexInfo.infoBlock.TexInfo[i].Height >> 3) & 0x3)
                    {
                        case 2:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].height = 0x200;
                            break;
                        default:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].height = 0x100;
                            break;
                    }
                int imgsize = (nsbtx.TexInfo.infoBlock.TexInfo[i].width * nsbtx.TexInfo.infoBlock.TexInfo[i].height * nsbtx.TexInfo.infoBlock.TexInfo[i].depth) / 8;
                long curpos = er.BaseStream.Position;
                if (nsbtx.TexInfo.infoBlock.TexInfo[i].format != 5)
                {
                    er.BaseStream.Seek(nsbtx.TexInfo.infoBlock.TexInfo[i].Texture_Offset + nsbtx.Header.Section_Offset[0] + nsbtx.TEX0.Texture_Data_Offset, SeekOrigin.Begin);
                }
                else
                {
                    er.BaseStream.Seek(nsbtx.Header.Section_Offset[0] + nsbtx.TEX0.Compressed_Texture_Data_Offset + nsbtx.TexInfo.infoBlock.TexInfo[i].Texture_Offset, SeekOrigin.Begin);
                }
                nsbtx.TexInfo.infoBlock.TexInfo[i].Image = er.ReadBytes(imgsize);
                er.BaseStream.Seek(curpos, SeekOrigin.Begin);

                if (nsbtx.TexInfo.infoBlock.TexInfo[i].format == 5)
                {
                    curpos = er.BaseStream.Position;
                    er.BaseStream.Seek(nsbtx.Header.Section_Offset[0] + nsbtx.TEX0.Compressed_Texture_Info_Data_Offset + nsbtx.TexInfo.infoBlock.TexInfo[i].Texture_Offset / 2, SeekOrigin.Begin);
                    nsbtx.TexInfo.infoBlock.TexInfo[i].spData = er.ReadBytes(imgsize / 2);
                    er.BaseStream.Seek(curpos, SeekOrigin.Begin);
                }
            }
            nsbtx.TexInfo.names = new List<string>();
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                nsbtx.TexInfo.names.Add(er.ReadString(Encoding.ASCII, 16).Replace("\0", ""));
                listBox2.Items.Add(nsbtx.TexInfo.names[i]);
            }

            nsbtx.PalInfo.dummy = er.ReadByte();
            nsbtx.PalInfo.num_objs = er.ReadByte();
            nsbtx.PalInfo.section_size = er.ReadInt16();

            nsbtx.PalInfo.unknownBlock.header_size = er.ReadInt16();
            nsbtx.PalInfo.unknownBlock.section_size = er.ReadInt16();
            nsbtx.PalInfo.unknownBlock.constant = er.ReadInt32();
            nsbtx.PalInfo.unknownBlock.unknown1 = new List<short>();
            nsbtx.PalInfo.unknownBlock.unknown2 = new List<short>();
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                nsbtx.PalInfo.unknownBlock.unknown1.Add(er.ReadInt16());
                nsbtx.PalInfo.unknownBlock.unknown2.Add(er.ReadInt16());
            }

            nsbtx.PalInfo.infoBlock.header_size = er.ReadInt16();
            nsbtx.PalInfo.infoBlock.data_size = er.ReadInt16();
            nsbtx.PalInfo.infoBlock.PalInfo = new NSBTX_File.palInfo.Info.palInfo[nsbtx.PalInfo.num_objs];
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset = (er.ReadInt16() << 3);
                nsbtx.PalInfo.infoBlock.PalInfo[i].Color0 = er.ReadInt16();
                er.BaseStream.Position -= 4;
                int palBase = er.ReadInt32();
                int palAddr = palBase & 0xfff;
                long curpos = er.BaseStream.Position;
                er.BaseStream.Seek(nsbtx.Header.Section_Offset[0] + nsbtx.TEX0.Palette_Data_Offset, SeekOrigin.Begin);
                er.BaseStream.Seek(nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset, SeekOrigin.Current);
                nsbtx.PalInfo.infoBlock.PalInfo[i].pal = Tinke.Convertir.BGR555(er.ReadBytes(nsbtx.TEX0.Palette_Data_Size - nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset));
                er.BaseStream.Seek(curpos, SeekOrigin.Begin);
            }
            nsbtx.PalInfo.names = new List<string>();
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                nsbtx.PalInfo.names.Add(er.ReadString(Encoding.ASCII, 16).Replace("\0", ""));
                listBox3.Items.Add(nsbtx.PalInfo.names[i]);
            }
            List<int> offsets = new List<int>();
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                offsets.Add(nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset);
            }
            offsets.Add((int)er.BaseStream.Length);
            offsets.Sort();
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                int pallength;
                int j = -1;
                do
                {
                    j++;
                }
                while (offsets[j] - nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset <= 0);
                pallength = offsets[j] - nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset;
                Color[] c_ = nsbtx.PalInfo.infoBlock.PalInfo[i].pal;
                List<Color> c = new List<Color>();
                c.AddRange(nsbtx.PalInfo.infoBlock.PalInfo[i].pal.Take(pallength / 2));
                nsbtx.PalInfo.infoBlock.PalInfo[i].pal = c.ToArray();
            }
            er.Close();
            listBox2.SelectedIndex = 0;
            if (nsbtx.PalInfo.num_objs != 0)
            {
                listBox3.SelectedIndex = 0;
            }
        }

        private void listBox6_SelectedIndexChanged(object sender, EventArgs e) // Gen V
        {
            listBox5.Items.Clear();
            listBox4.Items.Clear();
            if (radioButton18.Checked == true)
            {
                editorTileset = workingFolder + @"data\a\0\1\tilesets";
            }
            else if (radioButton17.Checked == true)
            {
                editorTileset = workingFolder + @"data\a\1\7\bldtilesets";
            }
            else
            {
                editorTileset = workingFolder + @"data\a\1\7\bld2tilesets";
            }
            EndianBinaryReader er = new EndianBinaryReader(File.OpenRead(editorTileset + "\\" + listBox6.SelectedIndex.ToString("D4")), Endianness.LittleEndian);
            nsbtx = new NSBTX_File();
            if (er.ReadString(Encoding.ASCII, 4) != "BTX0")
            {
                MessageBox.Show(rm.GetString("tilesetError"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                er.Close();
                return;
            }
            else
            {
                nsbtx.Header.ID = "BTX0";
                nsbtx.Header.Magic = er.ReadBytes(4);
                nsbtx.Header.file_size = er.ReadInt32();
                nsbtx.Header.header_size = er.ReadInt16();
                nsbtx.Header.nSection = er.ReadInt16();
                nsbtx.Header.Section_Offset = new Int32[nsbtx.Header.nSection];
                for (int i = 0; i < nsbtx.Header.nSection; i++)
                {
                    nsbtx.Header.Section_Offset[i] = er.ReadInt32();
                }
                nsbtx.TEX0.ID = er.ReadString(Encoding.ASCII, 4);
                if (nsbtx.TEX0.ID != "TEX0")
                {
                    MessageBox.Show(rm.GetString("tilesetError"));
                    er.Close();
                    return;
                }
            }
            nsbtx.TEX0.Section_size = er.ReadInt32();
            nsbtx.TEX0.Padding1 = er.ReadInt32();
            nsbtx.TEX0.Texture_Data_Size = (er.ReadInt16() << 3);
            nsbtx.TEX0.Texture_Info_Offset = er.ReadInt16();
            nsbtx.TEX0.Padding2 = er.ReadInt32();
            nsbtx.TEX0.Texture_Data_Offset = er.ReadInt32();
            nsbtx.TEX0.Padding3 = er.ReadInt32();
            nsbtx.TEX0.Compressed_Texture_Data_Size = (er.ReadInt16() << 3);
            nsbtx.TEX0.Compressed_Texture_Info_Offset = er.ReadInt16();
            nsbtx.TEX0.Padding4 = er.ReadInt32();
            nsbtx.TEX0.Compressed_Texture_Data_Offset = er.ReadInt32();
            nsbtx.TEX0.Compressed_Texture_Info_Data_Offset = er.ReadInt32();
            nsbtx.TEX0.Padding5 = er.ReadInt32();
            nsbtx.TEX0.Palette_Data_Size = (er.ReadInt32() << 3);
            nsbtx.TEX0.Palette_Info_Offset = er.ReadInt32();
            nsbtx.TEX0.Palette_Data_Offset = er.ReadInt32();

            nsbtx.TexInfo.dummy = er.ReadByte();
            nsbtx.TexInfo.num_objs = er.ReadByte();
            nsbtx.TexInfo.section_size = er.ReadInt16();

            nsbtx.TexInfo.unknownBlock.header_size = er.ReadInt16();
            nsbtx.TexInfo.unknownBlock.section_size = er.ReadInt16();
            nsbtx.TexInfo.unknownBlock.constant = er.ReadInt32();
            nsbtx.TexInfo.unknownBlock.unknown1 = new List<short>();
            nsbtx.TexInfo.unknownBlock.unknown2 = new List<short>();
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                nsbtx.TexInfo.unknownBlock.unknown1.Add(er.ReadInt16());
                nsbtx.TexInfo.unknownBlock.unknown2.Add(er.ReadInt16());
            }

            nsbtx.TexInfo.infoBlock.header_size = er.ReadInt16();
            nsbtx.TexInfo.infoBlock.data_size = er.ReadInt16();
            nsbtx.TexInfo.infoBlock.TexInfo = new NSBTX_File.texInfo.Info.texInfo[nsbtx.TexInfo.num_objs];
            //long compressedStartOffset = 0x00;
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                nsbtx.TexInfo.infoBlock.TexInfo[i].Texture_Offset = (er.ReadInt16() << 3);
                nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters = er.ReadInt16();
                nsbtx.TexInfo.infoBlock.TexInfo[i].Width = er.ReadByte();
                nsbtx.TexInfo.infoBlock.TexInfo[i].Unknown1 = er.ReadByte();
                nsbtx.TexInfo.infoBlock.TexInfo[i].Height = er.ReadByte();
                nsbtx.TexInfo.infoBlock.TexInfo[i].Unknown2 = er.ReadByte();
                nsbtx.TexInfo.infoBlock.TexInfo[i].coord_transf = (byte)(nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters & 14);
                nsbtx.TexInfo.infoBlock.TexInfo[i].color0 = (byte)((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 13) & 1);
                nsbtx.TexInfo.infoBlock.TexInfo[i].format = (byte)((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 10) & 7);
                nsbtx.TexInfo.infoBlock.TexInfo[i].height = (byte)(8 << ((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 7) & 7));
                nsbtx.TexInfo.infoBlock.TexInfo[i].width = (byte)(8 << ((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 4) & 7));
                nsbtx.TexInfo.infoBlock.TexInfo[i].flip_Y = (byte)((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 3) & 1);
                nsbtx.TexInfo.infoBlock.TexInfo[i].flip_X = (byte)((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 2) & 1);
                nsbtx.TexInfo.infoBlock.TexInfo[i].repeat_Y = (byte)((nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters >> 1) & 1);
                nsbtx.TexInfo.infoBlock.TexInfo[i].repeat_X = (byte)(nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters & 1);
                nsbtx.TexInfo.infoBlock.TexInfo[i].depth = (byte)bpp[nsbtx.TexInfo.infoBlock.TexInfo[i].format];
                if (nsbtx.TexInfo.infoBlock.TexInfo[i].width == 0x00)
                    switch (nsbtx.TexInfo.infoBlock.TexInfo[i].Unknown1 & 0x3)
                    {
                        case 2:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].width = 0x200;
                            break;
                        default:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].width = 0x100;
                            break;
                    }
                if (nsbtx.TexInfo.infoBlock.TexInfo[i].height == 0x00)
                    switch ((nsbtx.TexInfo.infoBlock.TexInfo[i].Height >> 3) & 0x3)
                    {
                        case 2:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].height = 0x200;
                            break;
                        default:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].height = 0x100;
                            break;
                    }
                int imgsize = (nsbtx.TexInfo.infoBlock.TexInfo[i].width * nsbtx.TexInfo.infoBlock.TexInfo[i].height * nsbtx.TexInfo.infoBlock.TexInfo[i].depth) / 8;
                long curpos = er.BaseStream.Position;
                if (nsbtx.TexInfo.infoBlock.TexInfo[i].format != 5)
                {
                    er.BaseStream.Seek(nsbtx.TexInfo.infoBlock.TexInfo[i].Texture_Offset + nsbtx.Header.Section_Offset[0] + nsbtx.TEX0.Texture_Data_Offset, SeekOrigin.Begin);
                }
                else
                {
                    er.BaseStream.Seek(nsbtx.Header.Section_Offset[0] + nsbtx.TEX0.Compressed_Texture_Data_Offset + nsbtx.TexInfo.infoBlock.TexInfo[i].Texture_Offset, SeekOrigin.Begin);
                }
                nsbtx.TexInfo.infoBlock.TexInfo[i].Image = er.ReadBytes(imgsize);
                er.BaseStream.Seek(curpos, SeekOrigin.Begin);

                if (nsbtx.TexInfo.infoBlock.TexInfo[i].format == 5)
                {
                    //nsbtx.TexInfo.infoBlock.TexInfo[i].compressedDataStart = (uint)compressedStartOffset;
                    //compressedStartOffset += imgsize / 2;
                    curpos = er.BaseStream.Position;
                    er.BaseStream.Seek(nsbtx.Header.Section_Offset[0] + nsbtx.TEX0.Compressed_Texture_Info_Data_Offset + nsbtx.TexInfo.infoBlock.TexInfo[i].Texture_Offset / 2, SeekOrigin.Begin);
                    nsbtx.TexInfo.infoBlock.TexInfo[i].spData = er.ReadBytes(imgsize / 2);
                    er.BaseStream.Seek(curpos, SeekOrigin.Begin);
                }
            }
            nsbtx.TexInfo.names = new List<string>();
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                nsbtx.TexInfo.names.Add(er.ReadString(Encoding.ASCII, 16).Replace("\0", ""));
                listBox5.Items.Add(nsbtx.TexInfo.names[i]);
            }

            nsbtx.PalInfo.dummy = er.ReadByte();
            nsbtx.PalInfo.num_objs = er.ReadByte();
            nsbtx.PalInfo.section_size = er.ReadInt16();

            nsbtx.PalInfo.unknownBlock.header_size = er.ReadInt16();
            nsbtx.PalInfo.unknownBlock.section_size = er.ReadInt16();
            nsbtx.PalInfo.unknownBlock.constant = er.ReadInt32();
            nsbtx.PalInfo.unknownBlock.unknown1 = new List<short>();
            nsbtx.PalInfo.unknownBlock.unknown2 = new List<short>();
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                nsbtx.PalInfo.unknownBlock.unknown1.Add(er.ReadInt16());
                nsbtx.PalInfo.unknownBlock.unknown2.Add(er.ReadInt16());
            }

            nsbtx.PalInfo.infoBlock.header_size = er.ReadInt16();
            nsbtx.PalInfo.infoBlock.data_size = er.ReadInt16();
            nsbtx.PalInfo.infoBlock.PalInfo = new NSBTX_File.palInfo.Info.palInfo[nsbtx.PalInfo.num_objs];
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset = (er.ReadInt16() << 3);
                nsbtx.PalInfo.infoBlock.PalInfo[i].Color0 = er.ReadInt16();
                er.BaseStream.Position -= 4;
                int palBase = er.ReadInt32();
                int palAddr = palBase & 0xfff;
                long curpos = er.BaseStream.Position;
                er.BaseStream.Seek(nsbtx.Header.Section_Offset[0] + nsbtx.TEX0.Palette_Data_Offset, SeekOrigin.Begin);
                er.BaseStream.Seek(nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset, SeekOrigin.Current);
                nsbtx.PalInfo.infoBlock.PalInfo[i].pal = Tinke.Convertir.BGR555(er.ReadBytes(nsbtx.TEX0.Palette_Data_Size - nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset));
                er.BaseStream.Seek(curpos, SeekOrigin.Begin);
            }
            nsbtx.PalInfo.names = new List<string>();
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                nsbtx.PalInfo.names.Add(er.ReadString(Encoding.ASCII, 16).Replace("\0", ""));
                listBox4.Items.Add(nsbtx.PalInfo.names[i]);
            }
            List<int> offsets = new List<int>();
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                //if(!offsets.Contains(nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset))
                //{
                offsets.Add(nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset);
                //}
            }
            offsets.Add((int)er.BaseStream.Length);
            offsets.Sort();
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                int pallength;
                int j = -1;
                do
                {
                    j++;
                }
                while (offsets[j] - nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset <= 0);//nsbtx.PalInfo.infoBlock.PalInfo[i + j].Palette_Offset - nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset == 0)
                pallength = offsets[j] - nsbtx.PalInfo.infoBlock.PalInfo[i].Palette_Offset;
                Color[] c_ = nsbtx.PalInfo.infoBlock.PalInfo[i].pal;
                List<Color> c = new List<Color>();
                c.AddRange(nsbtx.PalInfo.infoBlock.PalInfo[i].pal.Take(pallength / 2));
                nsbtx.PalInfo.infoBlock.PalInfo[i].pal = c.ToArray();
            }
            er.Close();
            listBox5.SelectedIndex = 0;
            if (nsbtx.PalInfo.num_objs != 0)
            {
                listBox4.SelectedIndex = 0;
            }
        }

        private void saveTilePNG_Click(object sender, EventArgs e)
        {
            if (isBW || isB2W2)
            {
                SaveFileDialog savePNG = new SaveFileDialog();
                savePNG.Filter = "PNG (*.png)|*.png";
                savePNG.FileName = nsbtx.TexInfo.names[listBox5.SelectedIndex];
                if (savePNG.ShowDialog() == DialogResult.OK)
                {
                    pictureBox3.Image.Save(savePNG.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
            else
            {
                SaveFileDialog savePNG = new SaveFileDialog();
                savePNG.Filter = "PNG (*.png)|*.png";
                savePNG.FileName = nsbtx.TexInfo.names[listBox2.SelectedIndex];
                if (savePNG.ShowDialog() == DialogResult.OK)
                {
                    pictureBox2.Image.Save(savePNG.FileName, System.Drawing.Imaging.ImageFormat.Png);
                }
            }
        }

        private void openTilePNG_Click(object sender, EventArgs e)
        {
            OpenFileDialog openPNG = new OpenFileDialog();
            openPNG.Filter = "PNG (*.png)|*.png";
            openPNG.FileName = listBox2.SelectedItem.ToString() + ".png";
            if (openPNG.ShowDialog() == DialogResult.OK)
            {
                int selectedTex = listBox2.SelectedIndex;
                int selectedPal = listBox3.SelectedIndex;
                Bitmap b = new Bitmap(openPNG.FileName);
                NSMBe4.DSFileSystem.ExternalFilesystemSource f = new NSMBe4.DSFileSystem.ExternalFilesystemSource(editorTileset + "\\" + listBox1.SelectedIndex.ToString("D4"));
                NSMBe4.DSFileSystem.Filesystem fs = new NSMBe4.DSFileSystem.Filesystem(f);
                NSMBe4.NSBMD.NSBTX nsbtx = new NSMBe4.NSBMD.NSBTX(new NSMBe4.DSFileSystem.File(fs, null, Path.GetFileName(editorTileset + "\\" + listBox1.SelectedIndex.ToString("D4"))));
                if (b.Size.Width != pictureBox2.Image.Width || b.Size.Height != pictureBox2.Image.Height)
                {
                    return;
                }
                nsbtx.textures[listBox2.SelectedIndex].replaceImgAndPal(b, nsbtx.pal[listBox3.SelectedIndex]);
                nsbtx.textures[listBox2.SelectedIndex].save();
                nsbtx.pal[listBox3.SelectedIndex].save();
                if (nsbtx.textures[listBox2.SelectedIndex].format == 5)
                {
                    nsbtx.str.seek((int)nsbtx.textures[listBox2.SelectedIndex].offset - nsbtx.startoffset);
                    nsbtx.str.write(nsbtx.textures[listBox2.SelectedIndex].getRawData());
                    nsbtx.str.seek((int)nsbtx.textures[listBox2.SelectedIndex].offset5 - nsbtx.startoffset);
                    nsbtx.str.write(nsbtx.textures[listBox2.SelectedIndex].getRawData5());
                    nsbtx.str.seek((int)nsbtx.palettes[listBox3.SelectedIndex].offs - nsbtx.startoffset);
                    nsbtx.str.write(nsbtx.pal[listBox3.SelectedIndex].getRawData());
                }
                else
                {
                    nsbtx.str.seek((int)nsbtx.textures[listBox2.SelectedIndex].offset - nsbtx.startoffset);
                    nsbtx.str.write(nsbtx.textures[listBox2.SelectedIndex].getRawData());
                    nsbtx.str.seek((int)nsbtx.palettes[listBox3.SelectedIndex].offs - nsbtx.startoffset);
                    nsbtx.str.write(nsbtx.pal[listBox3.SelectedIndex].getRawData());
                }
                File.WriteAllBytes(editorTileset + "\\" + listBox1.SelectedIndex.ToString("D4"), nsbtx.str.getData());
                b.Dispose();
                listBox1_SelectedIndexChanged(null, null);
                listBox2.SelectedIndex = selectedTex;
                listBox3.SelectedIndex = selectedPal;
            }
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e) // Select Texture
        {
            Bitmap b_ = new Bitmap(nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width, nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].height);
            NSMBe4.NSBMD.ImageTexeler.LockBitmap b = new NSMBe4.NSBMD.ImageTexeler.LockBitmap(b_);
            b.LockBits();
            int pixelnum = b.Height * b.Width;
            try
            {
                switch (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].format)
                {
                    case 1:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            int index = nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j] & 0x1f;
                            int alpha = (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j] >> 5);
                            alpha = ((alpha * 4) + (alpha / 2)) * 8;
                            Color c = Color.FromArgb(alpha, nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal[index]);
                            b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), c);
                        }
                        b.UnlockBits();
                        break;
                    case 2:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            uint index = nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j / 4];
                            index = (index >> ((j % 4) << 1)) & 3;
                            if (index == 0 && nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].color0 == 1)
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), Color.Transparent);
                            }
                            else
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal[index]);
                            }
                        }
                        b.UnlockBits();
                        //if (nsbtx.TexInfo.infoBlock.TexInfo[listBox1.SelectedIndex].color0 == 1) { b_.MakeTransparent(nsbtx.PalInfo.infoBlock.PalInfo[listBox2.SelectedIndex].pal[0]); }
                        break;
                    case 3:
                        //sp.PLTO[listBox1.SelectedIndex].Pal[sp.PLTO[listBox1.SelectedIndex].Unknown] = Color.Transparent; // made palette entry 0 transparent
                        for (int j = 0; j < pixelnum; j++)
                        {
                            uint index = nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j / 2];
                            index = (index >> ((j % 2) << 2)) & 0x0f;
                            if (index == 0 && nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].color0 == 1)
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), Color.Transparent);
                            }
                            else
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal[index]);
                            }
                        }
                        b.UnlockBits();
                        //if (nsbtx.TexInfo.infoBlock.TexInfo[listBox1.SelectedIndex].color0 == 1) { b_.MakeTransparent(nsbtx.PalInfo.infoBlock.PalInfo[listBox2.SelectedIndex].pal[0]); }
                        break;
                    case 4:
                        //if (mat.color0 != 0) mat.paldata[0] = RGBA.Transparent; // made palette entry 0 transparent
                        for (int j = 0; j < pixelnum; j++)
                        {
                            byte index = nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j];
                            if (index == 0 && nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].color0 == 1)
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), Color.Transparent);
                            }
                            else
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal[index]);
                            }
                        }
                        b.UnlockBits();
                        break;
                    case 5:
                        convert_4x4texel_b(nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image, nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width, nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].height, nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].spData, nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal, b);
                        b.UnlockBits();
                        break;
                    case 6:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            int index = nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j] & 0x7;
                            int alpha = (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j] >> 3);// & 0x1f;
                            alpha *= 8;
                            Color c = Color.FromArgb(alpha, nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal[index]);
                            b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), c);
                        }
                        b.UnlockBits();
                        break;
                    case 7:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            UInt16 p = (ushort)(nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j * 2] + (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j * 2 + 1] << 8));
                            Color c = Color.FromArgb((((p & 0x8000) != 0) ? 0xff : 0), (((p >> 0) & 0x1f) << 3), (((p >> 5) & 0x1f) << 3), (((p >> 10) & 0x1f) << 3));
                            b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), c);
                        }
                        b.UnlockBits();
                        break;
                }
            }
            catch
            {
                b.UnlockBits();
            }
            pictureBox2.Image = b_;
            textBox6.Text = nsbtx.TexInfo.names[listBox2.SelectedIndex];
            if (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].color0 == 1) checkBox6.Checked = true;
            else checkBox6.Checked = false;
        }

        private void listBox3_SelectedIndexChanged(object sender, EventArgs e) // Select Palette
        {
            Bitmap b_ = new Bitmap(nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width, nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].height);
            NSMBe4.NSBMD.ImageTexeler.LockBitmap b = new NSMBe4.NSBMD.ImageTexeler.LockBitmap(b_);
            b.LockBits();
            int pixelnum = b.Height * b.Width;
            try
            {
                switch (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].format)
                {
                    case 1:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            int index = nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j] & 0x1f;
                            int alpha = (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j] >> 5);
                            alpha = ((alpha * 4) + (alpha / 2)) * 8;
                            Color c = Color.FromArgb(alpha, nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal[index]);
                            b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), c);
                        }
                        b.UnlockBits();
                        break;
                    case 2:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            uint index = nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j / 4];
                            index = (index >> ((j % 4) << 1)) & 3;
                            if (index == 0 && nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].color0 == 1)
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), Color.Transparent);
                            }
                            else
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal[index]);
                            }
                        }
                        b.UnlockBits();
                        //if (nsbtx.TexInfo.infoBlock.TexInfo[listBox1.SelectedIndex].color0 == 1) { b_.MakeTransparent(nsbtx.PalInfo.infoBlock.PalInfo[listBox2.SelectedIndex].pal[0]); }
                        break;
                    case 3:
                        //sp.PLTO[listBox1.SelectedIndex].Pal[sp.PLTO[listBox1.SelectedIndex].Unknown] = Color.Transparent; // made palette entry 0 transparent
                        for (int j = 0; j < pixelnum; j++)
                        {
                            uint index = nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j / 2];
                            index = (index >> ((j % 2) << 2)) & 0x0f;
                            if (index == 0 && nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].color0 == 1)
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), Color.Transparent);
                            }
                            else
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal[index]);
                            }
                        }
                        b.UnlockBits();
                        //if (nsbtx.TexInfo.infoBlock.TexInfo[listBox1.SelectedIndex].color0 == 1) { b_.MakeTransparent(nsbtx.PalInfo.infoBlock.PalInfo[listBox2.SelectedIndex].pal[0]); }
                        break;
                    case 4:
                        //if (mat.color0 != 0) mat.paldata[0] = RGBA.Transparent; // made palette entry 0 transparent
                        for (int j = 0; j < pixelnum; j++)
                        {
                            byte index = nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j];
                            if (index == 0 && nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].color0 == 1)
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), Color.Transparent);
                            }
                            else
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal[index]);
                            }
                        }
                        b.UnlockBits();
                        break;
                    case 5:
                        convert_4x4texel_b(nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image, nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width, nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].height, nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].spData, nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal, b);
                        b.UnlockBits();
                        break;
                    case 6:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            int index = nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j] & 0x7;
                            int alpha = (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j] >> 3);// & 0x1f;
                            alpha *= 8;
                            Color c = Color.FromArgb(alpha, nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal[index]);
                            b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), c);
                        }
                        b.UnlockBits();
                        break;
                    case 7:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            UInt16 p = (ushort)(nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j * 2] + (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].Image[j * 2 + 1] << 8));
                            Color c = Color.FromArgb((((p & 0x8000) != 0) ? 0xff : 0), (((p >> 0) & 0x1f) << 3), (((p >> 5) & 0x1f) << 3), (((p >> 10) & 0x1f) << 3));
                            b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].width), c);
                        }
                        b.UnlockBits();
                        break;
                }
            }
            catch
            {
                b.UnlockBits();
            }
            if (listBox3.SelectedIndex != -1)
            {
                dataGridView11.Rows.Clear();
                for (int i = 0; i < 16; i++)
                {
                    dataGridView11.Rows.Add();
                }
                int cells = 0;
                int rows = 0;
                for (int i = 0; i < nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal.Count(); i++)
                {
                    dataGridView11.Rows[rows].Cells[cells].Style.BackColor = nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal[i];
                    cells++;
                    if (cells == 16)
                    {
                        cells = 0;
                        rows++;
                    }
                }
                dataGridView11.Rows[0].Cells[0].Selected = true;
                textBox5.Text = nsbtx.PalInfo.names[listBox3.SelectedIndex];
            }
            pictureBox2.Image = b_;
        }

        private void listBox5_SelectedIndexChanged(object sender, EventArgs e) // Gen V Select Texture
        {
            Bitmap b_ = new Bitmap(nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width, nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].height);
            NSMBe4.NSBMD.ImageTexeler.LockBitmap b = new NSMBe4.NSBMD.ImageTexeler.LockBitmap(b_);
            b.LockBits();
            int pixelnum = b.Height * b.Width;
            try
            {
                switch (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].format)
                {
                    case 1:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            int index = nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].Image[j] & 0x1f;
                            int alpha = (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].Image[j] >> 5);
                            alpha = ((alpha * 4) + (alpha / 2)) * 8;
                            Color c = Color.FromArgb(alpha, nsbtx.PalInfo.infoBlock.PalInfo[listBox4.SelectedIndex].pal[index]);
                            b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width), c);
                        }
                        b.UnlockBits();
                        break;
                    case 2:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            uint index = nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].Image[j / 4];
                            index = (index >> ((j % 4) << 1)) & 3;
                            if (index == 0 && nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].color0 == 1)
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width), Color.Transparent);
                            }
                            else
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width), nsbtx.PalInfo.infoBlock.PalInfo[listBox4.SelectedIndex].pal[index]);
                            }
                        }
                        b.UnlockBits();
                        //if (nsbtx.TexInfo.infoBlock.TexInfo[listBox1.SelectedIndex].color0 == 1) { b_.MakeTransparent(nsbtx.PalInfo.infoBlock.PalInfo[listBox2.SelectedIndex].pal[0]); }
                        break;
                    case 3:
                        //sp.PLTO[listBox1.SelectedIndex].Pal[sp.PLTO[listBox1.SelectedIndex].Unknown] = Color.Transparent; // made palette entry 0 transparent
                        for (int j = 0; j < pixelnum; j++)
                        {
                            uint index = nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].Image[j / 2];
                            index = (index >> ((j % 2) << 2)) & 0x0f;
                            if (index == 0 && nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].color0 == 1)
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width), Color.Transparent);
                            }
                            else
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width), nsbtx.PalInfo.infoBlock.PalInfo[listBox4.SelectedIndex].pal[index]);
                            }
                        }
                        b.UnlockBits();
                        //if (nsbtx.TexInfo.infoBlock.TexInfo[listBox1.SelectedIndex].color0 == 1) { b_.MakeTransparent(nsbtx.PalInfo.infoBlock.PalInfo[listBox2.SelectedIndex].pal[0]); }
                        break;
                    case 4:
                        //if (mat.color0 != 0) mat.paldata[0] = RGBA.Transparent; // made palette entry 0 transparent
                        for (int j = 0; j < pixelnum; j++)
                        {
                            byte index = nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].Image[j];
                            if (index == 0 && nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].color0 == 1)
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width), Color.Transparent);
                            }
                            else
                            {
                                b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width), nsbtx.PalInfo.infoBlock.PalInfo[listBox4.SelectedIndex].pal[index]);
                            }
                        }
                        b.UnlockBits();
                        break;
                    case 5:
                        convert_4x4texel_b(nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].Image, nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width, nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].height, nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].spData, nsbtx.PalInfo.infoBlock.PalInfo[listBox4.SelectedIndex].pal, b);
                        b.UnlockBits();
                        break;
                    case 6:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            int index = nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].Image[j] & 0x7;
                            int alpha = (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].Image[j] >> 3);// & 0x1f;
                            alpha *= 8;
                            Color c = Color.FromArgb(alpha, nsbtx.PalInfo.infoBlock.PalInfo[listBox4.SelectedIndex].pal[index]);
                            b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width), c);
                        }
                        b.UnlockBits();
                        break;
                    case 7:
                        for (int j = 0; j < pixelnum; j++)
                        {
                            UInt16 p = (ushort)(nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].Image[j * 2] + (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].Image[j * 2 + 1] << 8));
                            Color c = Color.FromArgb((((p & 0x8000) != 0) ? 0xff : 0), (((p >> 0) & 0x1f) << 3), (((p >> 5) & 0x1f) << 3), (((p >> 10) & 0x1f) << 3));
                            b.SetPixel(j - ((j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)) * (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width)), j / (nsbtx.TexInfo.infoBlock.TexInfo[listBox5.SelectedIndex].width), c);
                        }
                        b.UnlockBits();
                        break;
                }
            }
            catch
            {
                b.UnlockBits();
                goto end2;
            }
        end2:
            if (listBox4.SelectedIndex != -1)
            {
                dataGridView12.Rows.Clear();
                for (int i = 0; i < 16; i++)
                {
                    dataGridView12.Rows.Add();
                }
                int cells = 0;
                int rows = 0;
                for (int i = 0; i < nsbtx.PalInfo.infoBlock.PalInfo[listBox4.SelectedIndex].pal.Count(); i++)
                {
                    dataGridView12.Rows[rows].Cells[cells].Style.BackColor = nsbtx.PalInfo.infoBlock.PalInfo[listBox4.SelectedIndex].pal[i];
                    cells++;
                    if (cells == 16)
                    {
                        cells = 0;
                        rows++;
                    }
                }
                dataGridView12.Rows[0].Cells[0].Selected = true;
            }
            pictureBox3.Image = b_;
        }

        private void tilesetImport_Click(object sender, EventArgs e)
        {
            if (isBW || isB2W2)
            {
                OpenFileDialog ef = new OpenFileDialog();
                ef.Title = rm.GetString("importTileset");
                ef.Filter = rm.GetString("tilesetFile");
                if (ef.ShowDialog() == DialogResult.OK)
                {
                    System.IO.BinaryReader textureStream = new System.IO.BinaryReader(File.OpenRead(ef.FileName));
                    int header;
                    header = (int)textureStream.ReadUInt32();
                    textureStream.Close();
                    if (header == 811095106)
                    {
                        File.Copy(ef.FileName, editorTileset + "\\" + listBox6.SelectedIndex.ToString("D4"), true);
                        listBox6_SelectedIndexChanged(null, null);
                    }
                    else
                    {
                        MessageBox.Show(rm.GetString("invalidFile"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                }
                return;
            }
            else
            {
                OpenFileDialog ef = new OpenFileDialog();
                ef.Title = rm.GetString("importTileset");
                ef.Filter = rm.GetString("tilesetFile");
                if (ef.ShowDialog() == DialogResult.OK)
                {
                    System.IO.BinaryReader textureStream = new System.IO.BinaryReader(File.OpenRead(ef.FileName));
                    int header;
                    header = (int)textureStream.ReadUInt32();
                    textureStream.Close();
                    if (header == 811095106)
                    {
                        File.Copy(ef.FileName, editorTileset + "\\" + listBox1.SelectedIndex.ToString("D4"), true);
                        listBox1_SelectedIndexChanged(null, null);
                    }
                    else
                    {
                        MessageBox.Show(rm.GetString("invalidFile"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    }
                }
                return;
            }
        }

        private void tilesetExport_Click(object sender, EventArgs e)
        {
            if (isBW || isB2W2)
            {
                SaveFileDialog ef = new SaveFileDialog();
                ef.Title = rm.GetString("exportTileset");
                ef.Filter = rm.GetString("tilesetFile");
                if (ef.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(editorTileset + "\\" + listBox6.SelectedIndex.ToString("D4"), ef.FileName, true);
                }
                return;
            }
            else
            {
                SaveFileDialog ef = new SaveFileDialog();
                ef.Title = rm.GetString("exportTileset");
                ef.Filter = rm.GetString("tilesetFile");
                if (ef.ShowDialog() == DialogResult.OK)
                {
                    File.Copy(editorTileset + "\\" + listBox1.SelectedIndex.ToString("D4"), ef.FileName, true);
                }
                return;
            }
        }

        private void button15_Click_1(object sender, EventArgs e) // Add new tileset
        {
            OpenFileDialog ef = new OpenFileDialog();
            ef.Title = rm.GetString("importTileset");
            ef.Filter = rm.GetString("tilesetFile");
            if (ef.ShowDialog() == DialogResult.OK)
            {
                System.IO.BinaryReader textureStream = new System.IO.BinaryReader(File.OpenRead(ef.FileName));
                int header;
                header = (int)textureStream.ReadUInt32();
                textureStream.Close();
                if (header == 811095106)
                {
                    File.Copy(ef.FileName, editorTileset + "\\" + listBox1.Items.Count.ToString("D4"), true);
                    comboBox4.Items.Add(rm.GetString("tileset") + comboBox4.Items.Count);
                    listBox1.Items.Add(rm.GetString("tileset") + listBox1.Items.Count);
                    texturesCount++;
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;
                }
                else
                {
                    MessageBox.Show(rm.GetString("invalidFile"), null, MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
            return;
        }

        private void radioButton8_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton8.Checked == true)
            {
                listBox1.Items.Clear();
                for (int i = 0; i < bldTexturesCount; i++)
                {
                    listBox1.Items.Add(rm.GetString("buildingPackList") + i);
                }
                listBox1.SelectedIndex = 0;
                button15.Enabled = false;
            }
            else
            {
                listBox1.Items.Clear();
                for (int i = 0; i < texturesCount; i++)
                {
                    listBox1.Items.Add(rm.GetString("tileset") + i);
                }
                listBox1.SelectedIndex = 0;
                button15.Enabled = true;
            }
        }

        private void radioButton17_CheckedChanged(object sender, EventArgs e) // Gen V
        {
            if (radioButton17.Checked == true)
            {
                listBox6.Items.Clear();
                for (int i = 0; i < bldTexturesCount; i++)
                {
                    listBox6.Items.Add(rm.GetString("buildingPackList") + i);
                }
                listBox6.SelectedIndex = 0;
            }
            else if (radioButton23.Checked == true)
            {
                listBox6.Items.Clear();
                for (int i = 0; i < bld2TexturesCount; i++)
                {
                    listBox6.Items.Add(rm.GetString("buildingPackList") + i);
                }
                listBox6.SelectedIndex = 0;
            }
            else
            {
                listBox6.Items.Clear();
                for (int i = 0; i < texturesCount; i++)
                {
                    listBox6.Items.Add(rm.GetString("tileset") + i);
                }
                listBox6.SelectedIndex = 0;
            }
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e) // Palette colour change
        {
            if (saveModeON == true)
            {
                pictureBox4.BackColor = Color.FromArgb((int)numericUpDown8.Value, (int)numericUpDown9.Value, (int)numericUpDown10.Value);
                dataGridView11.DefaultCellStyle.SelectionBackColor = pictureBox4.BackColor;
                dataGridView11.CurrentCell.Style.BackColor = pictureBox4.BackColor;
            }
        }

        private void numericUpDown13_ValueChanged(object sender, EventArgs e) // V Palette colour change
        {
            pictureBox5.BackColor = Color.FromArgb((int)numericUpDown13.Value, (int)numericUpDown12.Value, (int)numericUpDown11.Value);
            dataGridView12.DefaultCellStyle.SelectionBackColor = pictureBox5.BackColor;
        }

        private void dataGridView11_SelectionChanged(object sender, EventArgs e) // Update Colour Box
        {
            saveModeON = false;
            numericUpDown8.Value = dataGridView11.CurrentCell.Style.BackColor.R;
            numericUpDown9.Value = dataGridView11.CurrentCell.Style.BackColor.G;
            numericUpDown10.Value = dataGridView11.CurrentCell.Style.BackColor.B;
            saveModeON = true;
            numericUpDown8_ValueChanged(null, null);
        }

        private void dataGridView12_SelectionChanged(object sender, EventArgs e) // V Update Colour Box
        {
            numericUpDown13.Value = dataGridView12.CurrentCell.Style.BackColor.R;
            numericUpDown12.Value = dataGridView12.CurrentCell.Style.BackColor.G;
            numericUpDown11.Value = dataGridView12.CurrentCell.Style.BackColor.B;
            dataGridView12.DefaultCellStyle.SelectionBackColor = pictureBox5.BackColor;
        }

        private void saveNSBTX2() // Save NSBTX
        {
            BinaryWriter write = new BinaryWriter(File.Create(editorTileset + "\\" + listBox1.SelectedIndex.ToString("D4")));
            write.Write(0x30585442); // BTX0
            write.Write(nsbtx.Header.Magic);
            write.Write(0x0);
            write.Write((short)nsbtx.Header.header_size);
            write.Write((short)nsbtx.Header.nSection);
            for (int i = 0; i < nsbtx.Header.nSection; i++)
            {
                write.Write(nsbtx.Header.Section_Offset[i]);
            }

            write.Write(0x30584554); // TEX0
            write.Write(nsbtx.TEX0.Section_size);
            write.Write(nsbtx.TEX0.Padding1);
            write.Write((short)(nsbtx.TEX0.Texture_Data_Size >> 3));
            write.Write((short)nsbtx.TEX0.Texture_Info_Offset);
            write.Write(nsbtx.TEX0.Padding2);
            write.Write(nsbtx.TEX0.Texture_Data_Offset);
            write.Write(nsbtx.TEX0.Padding3);
            write.Write((short)(nsbtx.TEX0.Compressed_Texture_Data_Size  >> 3));
            write.Write((short)nsbtx.TEX0.Compressed_Texture_Info_Offset);
            write.Write(nsbtx.TEX0.Padding4);
            write.Write(nsbtx.TEX0.Compressed_Texture_Data_Offset);
            write.Write(nsbtx.TEX0.Compressed_Texture_Info_Data_Offset);
            write.Write(nsbtx.TEX0.Padding5);
            int paletteSize = 0;
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                paletteSize += (nsbtx.PalInfo.infoBlock.PalInfo[i].pal.Length * 2);
            }
            write.Write(paletteSize >> 3);
            write.Write(nsbtx.TEX0.Palette_Info_Offset);
            write.Write(nsbtx.TEX0.Palette_Data_Offset);

            write.Write((byte)nsbtx.TexInfo.dummy);
            write.Write((byte)nsbtx.TexInfo.num_objs);
            write.Write((short)nsbtx.TexInfo.section_size);

            write.Write((short)nsbtx.TexInfo.unknownBlock.header_size);
            write.Write((short)nsbtx.TexInfo.unknownBlock.section_size);
            write.Write(nsbtx.TexInfo.unknownBlock.constant);
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                write.Write((short)nsbtx.TexInfo.unknownBlock.unknown1[i]);
                write.Write((short)nsbtx.TexInfo.unknownBlock.unknown2[i]);
            }

            write.Write((short)nsbtx.TexInfo.infoBlock.header_size);
            write.Write((short)nsbtx.TexInfo.infoBlock.data_size);

            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                write.Write((short)(nsbtx.TexInfo.infoBlock.TexInfo[i].Texture_Offset >> 3));
                write.Write((short)nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters);
                write.Write((byte)nsbtx.TexInfo.infoBlock.TexInfo[i].Width);
                write.Write((byte)nsbtx.TexInfo.infoBlock.TexInfo[i].Unknown1);
                write.Write((byte)nsbtx.TexInfo.infoBlock.TexInfo[i].Height);
                write.Write((byte)nsbtx.TexInfo.infoBlock.TexInfo[i].Unknown2);
            }
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                write.Write(Encoding.UTF8.GetBytes(nsbtx.TexInfo.names[i]));
                for (int j = 0; j < (16 - nsbtx.TexInfo.names[i].Length); j++)
                {
                    write.Write((byte)0x0);
                }
            }

            write.Write((byte)nsbtx.PalInfo.dummy);
            write.Write((byte)nsbtx.PalInfo.num_objs);
            write.Write((short)nsbtx.PalInfo.section_size);

            write.Write((short)nsbtx.PalInfo.unknownBlock.header_size);
            write.Write((short)nsbtx.PalInfo.unknownBlock.section_size);
            write.Write(nsbtx.PalInfo.unknownBlock.constant);
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                write.Write((short)nsbtx.PalInfo.unknownBlock.unknown1[i]);
                write.Write((short)nsbtx.PalInfo.unknownBlock.unknown2[i]);
            }

            write.Write((short)nsbtx.PalInfo.infoBlock.header_size);
            write.Write((short)nsbtx.PalInfo.infoBlock.data_size);
            int palOffset = nsbtx.PalInfo.infoBlock.PalInfo[0].Palette_Offset;
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                write.Write((short)(palOffset >> 3));
                palOffset += (nsbtx.PalInfo.infoBlock.PalInfo[i].pal.Length * 2);
                write.Write((short)nsbtx.PalInfo.infoBlock.PalInfo[i].Color0);
            }
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                write.Write(Encoding.UTF8.GetBytes(nsbtx.PalInfo.names[i]));
                for (int j = 0; j < (16 - nsbtx.PalInfo.names[i].Length); j++)
                {
                    write.Write((byte)0x0);
                }
            }

            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                write.Write(nsbtx.TexInfo.infoBlock.TexInfo[i].Image);
                if (nsbtx.TexInfo.infoBlock.TexInfo[i].format == 5)
                {
                    write.Write(nsbtx.TexInfo.infoBlock.TexInfo[i].spData);
                }
            }
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                for (int j = 0; j < nsbtx.PalInfo.infoBlock.PalInfo[i].pal.Length; j++)
                {
                    int colour = 0;
                    colour = (nsbtx.PalInfo.infoBlock.PalInfo[i].pal[j].R / 8) + ((nsbtx.PalInfo.infoBlock.PalInfo[i].pal[j].G / 8) << 5) + ((nsbtx.PalInfo.infoBlock.PalInfo[i].pal[j].B / 8) << 10);
                    write.Write((short)colour);
                }
            }
            write.BaseStream.Position = 8;
            write.Write((Int32)write.BaseStream.Length);
            write.Close();
        }

        private void saveNSBTX() // Save NSBTX
        {
            BinaryWriter write = new BinaryWriter(File.Create(editorTileset + "\\" + listBox1.SelectedIndex.ToString("D4")));
            write.Write(0x30585442); // BTX0
            write.Write(nsbtx.Header.Magic);
            write.Write(0x0);
            write.Write((short)nsbtx.Header.header_size);
            write.Write((short)nsbtx.Header.nSection);
            for (int i = 0; i < nsbtx.Header.nSection; i++)
            {
                write.Write(nsbtx.Header.Section_Offset[i]);
            }

            write.Write(0x30584554); // TEX0
            write.Write((Int32)0);
            write.Write((Int32)0);
            int tex_length = 0;
            int comp_tex_length = 0;
            int comp_inf_length = 0;
            int nrtex = 0;
            int nr5 = 0;
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                if (nsbtx.TexInfo.infoBlock.TexInfo[i].format == 5)
                {
                    comp_tex_length += nsbtx.TexInfo.infoBlock.TexInfo[i].Image.Length;
                    comp_inf_length += nsbtx.TexInfo.infoBlock.TexInfo[i].spData.Length;
                    nr5 += 1;
                }
                else
                {
                    tex_length += nsbtx.TexInfo.infoBlock.TexInfo[i].Image.Length;
                    nrtex += 1;
                }
            }
            write.Write((Int16)(tex_length >> 3));
            write.Write((Int16)nsbtx.TEX0.Texture_Info_Offset);
            write.Write((Int32)0);
            write.Write((Int32)nsbtx.TEX0.Texture_Data_Offset);
            write.Write((Int32)0);
            write.Write((Int16)(comp_tex_length >> 3));
            write.Write((Int16)nsbtx.TEX0.Compressed_Texture_Info_Offset);
            write.Write((Int32)0);
            int Compressed_Texture_Data_Offset = 0x3c + nsbtx.TexInfo.section_size + nsbtx.PalInfo.section_size + tex_length;
            write.Write((Int32)Compressed_Texture_Data_Offset);
            int Compressed_Texture_Info_Data_Offset = Compressed_Texture_Data_Offset + comp_tex_length;
            write.Write((Int32)Compressed_Texture_Info_Data_Offset);
            write.Write((Int32)0);
            int pallength = 0;
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                pallength += nsbtx.PalInfo.infoBlock.PalInfo[i].pal.Length * 2;
            }
            write.Write((Int32)(pallength >> 3));
            int Pal_Info_Offset = 0x3c + nsbtx.TexInfo.section_size;
            write.Write((Int32)Pal_Info_Offset);
            int Pal_Data_Offset = Compressed_Texture_Info_Data_Offset + comp_inf_length;
            write.Write((Int32)Pal_Data_Offset);

            write.Write((byte)0);
            write.Write((byte)nsbtx.TexInfo.num_objs);
            write.Write((Int16)nsbtx.TexInfo.section_size);

            write.Write((Int16)nsbtx.TexInfo.unknownBlock.header_size);
            write.Write((Int16)nsbtx.TexInfo.unknownBlock.section_size);
            write.Write((Int32)nsbtx.TexInfo.unknownBlock.constant);
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                write.Write((Int16)nsbtx.TexInfo.unknownBlock.unknown1[i]);
                write.Write((Int16)nsbtx.TexInfo.unknownBlock.unknown2[i]);
            }

            write.Write((Int16)nsbtx.TexInfo.infoBlock.header_size);
            write.Write((Int16)nsbtx.TexInfo.infoBlock.data_size);
            int texoff = 0;
            int comptexoff = 0;
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                if (nsbtx.TexInfo.infoBlock.TexInfo[i].format != 5)
                {
                    write.Write((Int16)(texoff >> 3));
                    texoff += nsbtx.TexInfo.infoBlock.TexInfo[i].Image.Length;
                }
                else
                {
                    write.Write((Int16)(comptexoff >> 3));
                    comptexoff += nsbtx.TexInfo.infoBlock.TexInfo[i].Image.Length;
                }

                int width = 8, height = 8;
                int dswidth = 0, dsheight = 0;
                while (width < nsbtx.TexInfo.infoBlock.TexInfo[i].width) { width *= 2; dswidth++; }
                while (height < nsbtx.TexInfo.infoBlock.TexInfo[i].height) { height *= 2; dsheight++; }

                nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters = (Int16)((dswidth << 4)
                    | (dsheight << 7)
                    | (nsbtx.TexInfo.infoBlock.TexInfo[i].format << 10)
                    | (nsbtx.TexInfo.infoBlock.TexInfo[i].color0 << 13)
                    | (nsbtx.TexInfo.infoBlock.TexInfo[i].flip_Y << 3)
                    | (nsbtx.TexInfo.infoBlock.TexInfo[i].flip_X << 2)
                    | (nsbtx.TexInfo.infoBlock.TexInfo[i].repeat_Y << 1)
                    | (nsbtx.TexInfo.infoBlock.TexInfo[i].repeat_X)
                    | (nsbtx.TexInfo.infoBlock.TexInfo[i].coord_transf << 14)
                    );

                if (nsbtx.TexInfo.infoBlock.TexInfo[i].width >= 256)
                    switch (nsbtx.TexInfo.infoBlock.TexInfo[i].width)
                    {
                        case 0x400:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].Unknown1 = 3;
                            break;
                        case 0x200:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].Unknown1 = 2;
                            break;
                        case 0x100:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].Unknown1 = 1;
                            break;
                    }

                if (nsbtx.TexInfo.infoBlock.TexInfo[i].height >= 256)
                    switch (nsbtx.TexInfo.infoBlock.TexInfo[i].height)
                    {
                        case 0x400:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].Height = 3 << 3;
                            break;
                        case 0x200:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].Height = 2 << 3;
                            break;
                        case 0x100:
                            nsbtx.TexInfo.infoBlock.TexInfo[i].Height = 1 << 3;
                            break;
                    }

                write.Write((Int16)nsbtx.TexInfo.infoBlock.TexInfo[i].Parameters);
                write.Write((byte)nsbtx.TexInfo.infoBlock.TexInfo[i].Width);
                write.Write((byte)nsbtx.TexInfo.infoBlock.TexInfo[i].Unknown1);
                write.Write((byte)nsbtx.TexInfo.infoBlock.TexInfo[i].Height);
                write.Write((byte)nsbtx.TexInfo.infoBlock.TexInfo[i].Unknown2);
            }
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                write.Write(Encoding.UTF8.GetBytes(nsbtx.TexInfo.names[i]));
                for (int j = 0; j < (16 - nsbtx.TexInfo.names[i].Length); j++)
                {
                    write.Write((byte)0x0);
                }
            }

            write.Write((byte)0);
            write.Write((byte)nsbtx.PalInfo.num_objs);
            write.Write((Int16)nsbtx.PalInfo.section_size);

            write.Write((Int16)nsbtx.PalInfo.unknownBlock.header_size);
            write.Write((Int16)nsbtx.PalInfo.unknownBlock.section_size);
            write.Write((Int32)nsbtx.PalInfo.unknownBlock.constant);
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                write.Write((Int16)nsbtx.PalInfo.unknownBlock.unknown1[i]);
                write.Write((Int16)nsbtx.PalInfo.unknownBlock.unknown2[i]);
            }

            write.Write((Int16)nsbtx.PalInfo.infoBlock.header_size);
            write.Write((Int16)nsbtx.PalInfo.infoBlock.data_size);
            int paloff = 0;
            List<int> paloffsets = new List<int>();
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                paloffsets.Add(paloff);
                paloff += nsbtx.PalInfo.infoBlock.PalInfo[i].pal.Length * 2;
            }
            int palOffset = 0;
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                write.Write((short)(palOffset >> 3));
                palOffset += (nsbtx.PalInfo.infoBlock.PalInfo[i].pal.Length * 2);
                write.Write((short)nsbtx.PalInfo.infoBlock.PalInfo[i].Color0);
            }
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                write.Write(Encoding.UTF8.GetBytes(nsbtx.PalInfo.names[i]));
                for (int j = 0; j < (16 - nsbtx.PalInfo.names[i].Length); j++)
                {
                    write.Write((byte)0x0);
                }
            }

            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                if (nsbtx.TexInfo.infoBlock.TexInfo[i].format != 5)
                {
                    write.Write(nsbtx.TexInfo.infoBlock.TexInfo[i].Image, 0, nsbtx.TexInfo.infoBlock.TexInfo[i].Image.Length);
                }
            }
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                if (nsbtx.TexInfo.infoBlock.TexInfo[i].format == 5)
                {
                    write.Write(nsbtx.TexInfo.infoBlock.TexInfo[i].Image, 0, nsbtx.TexInfo.infoBlock.TexInfo[i].Image.Length);
                }
            }
            for (int i = 0; i < nsbtx.TexInfo.num_objs; i++)
            {
                if (nsbtx.TexInfo.infoBlock.TexInfo[i].format == 5)
                {
                    write.Write(nsbtx.TexInfo.infoBlock.TexInfo[i].spData, 0, nsbtx.TexInfo.infoBlock.TexInfo[i].spData.Length);
                }
            }
            for (int i = 0; i < nsbtx.PalInfo.num_objs; i++)
            {
                for (int j = 0; j < nsbtx.PalInfo.infoBlock.PalInfo[i].pal.Length; j++)
                {
                    int colour = 0;
                    colour = (nsbtx.PalInfo.infoBlock.PalInfo[i].pal[j].R / 8) + ((nsbtx.PalInfo.infoBlock.PalInfo[i].pal[j].G / 8) << 5) + ((nsbtx.PalInfo.infoBlock.PalInfo[i].pal[j].B / 8) << 10);
                    write.Write((short)colour);
                }
            }
            write.BaseStream.Position = 0x8;
            write.Write((Int32)write.BaseStream.Length);
            write.BaseStream.Position = 0x18;
            write.Write((Int32)(write.BaseStream.Length - 0x14));
            write.Close();
        }

        private void button44_Click(object sender, EventArgs e) // Save Current Palette
        {
            nsbtx.PalInfo.names[listBox3.SelectedIndex] = textBox5.Text;
            if (checkBox6.Checked == true) nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].color0 = 0;
            else nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].color0 = 1;
            int columns = 0;
            for (int i = 0; i < nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal.Length; i++)
            {
                nsbtx.PalInfo.infoBlock.PalInfo[listBox3.SelectedIndex].pal[i] = dataGridView11.Rows[i / 16].Cells[columns].Style.BackColor;
                columns++;
                if (columns == 16) columns = 0;
            }
            int current = listBox3.SelectedIndex;
            //int y = dataGridView11.CurrentCell.RowIndex;
            //int x = dataGridView11.CurrentCell.ColumnIndex;
            listBox3.Items.RemoveAt(current);
            listBox3.Items.Insert(current, nsbtx.PalInfo.names[current]);
            listBox3.SelectedIndex = current;
            //dataGridView11.Rows[y].Cells[x].Selected = true;
            saveNSBTX();
        }

        private void button48_Click(object sender, EventArgs e) // Save Current Texture
        {
            nsbtx.TexInfo.names[listBox2.SelectedIndex] = textBox6.Text;
            if (checkBox6.Checked == true) nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].color0 = 1;
            else nsbtx.TexInfo.infoBlock.TexInfo[listBox2.SelectedIndex].color0 = 0;
            int current = listBox3.SelectedIndex;
            //int y = dataGridView11.CurrentCell.RowIndex;
            //int x = dataGridView11.CurrentCell.ColumnIndex;
            listBox3.Items.RemoveAt(current);
            listBox3.Items.Insert(current, nsbtx.PalInfo.names[current]);
            listBox3.SelectedIndex = current;
            //dataGridView11.Rows[y].Cells[x].Selected = true;
            saveNSBTX();
        }

        #endregion

        #region Text Editor

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e) // Read Text File
        {
            Column36.HeaderText = "";
            Column50.Visible = false;
            Column51.Visible = false;
            Column52.Visible = false;
            Column53.Visible = false;
            Column54.Visible = false;
            dataGridView6.Rows.Clear();
            saveStringBtn.Enabled = true;
            button18.Enabled = true;
            button20.Enabled = true;
            if (isBW || isB2W2)
            {
                Column36.HeaderText = "1";
                Column50.Visible = true;
                Column51.Visible = true;
                Column52.Visible = true;
                Column53.Visible = true;
                Column54.Visible = true;
                readTextV();
                return;
            }
            #region Paths
            if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041)
            {
                textPath = workingFolder + @"data\msgdata\msg" + "\\" + comboBox3.SelectedIndex.ToString("D4");
            }
            if (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
            {
                textPath = workingFolder + @"data\msgdata\pl_msg" + "\\" + comboBox3.SelectedIndex.ToString("D4");
            }
            if (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049)
            {
                textPath = workingFolder + @"data\a\0\2\text\" + "\\" + comboBox3.SelectedIndex.ToString("D4");
            }
            #endregion
            System.IO.BinaryReader readText = new System.IO.BinaryReader(File.OpenRead(textPath));
            stringCount = readText.ReadUInt16();
            progressBar1.Maximum = stringCount;
            initialKey = readText.ReadUInt16();
            int key1 = (initialKey * 0x2FD) & 0xFFFF;
            int key2 = 0;
            int realKey = 0;
            bool specialCharON = false;
            int[] currentOffset = new int[stringCount];
            int[] currentSize = new int[stringCount];
            int car = 0;
            bool compressed = false;
            for (int i = 0; i < stringCount; i++) // Reads and stores string offsets and sizes
            {
                key2 = (key1 * (i + 1) & 0xFFFF);
                realKey = key2 | (key2 << 16);
                currentOffset[i] = (int)readText.ReadUInt32() ^ realKey;
                currentSize[i] = (int)readText.ReadUInt32() ^ realKey;
            }
            for (int i = 0; i < stringCount; i++) // Adds new string
            {
                key1 = (0x91BD3 * (i + 1)) & 0xFFFF;
                readText.BaseStream.Position = currentOffset[i];
                string pokemonText = "";
                for (int j = 0; j < currentSize[i]; j++) // Adds new characters to string
                {
                    car = readText.ReadUInt16() ^ key1;
                    #region Special Characters
                    if (car == 0xE000 || car == 0x25BC || car == 0x25BD || car == 0xF100 || car == 0xFFFE || car == 0xFFFF)
                    {
                        if (car == 0xE000)
                        {
                            pokemonText += @"\n";
                        }
                        if (car == 0x25BC)
                        {
                            pokemonText += @"\r";
                        }
                        if (car == 0x25BD)
                        {
                            pokemonText += @"\f";
                        }
                        if (car == 0xF100)
                        {
                            compressed = true;
                        }
                        if (car == 0xFFFE)
                        {
                            pokemonText += @"\v";
                            specialCharON = true;
                        }
                    }
                    #endregion
                    else
                    {
                        if (specialCharON == true)
                        {
                            pokemonText += car.ToString("X4");
                            specialCharON = false;
                        }
                        else if (compressed)
                        {
                            #region Compressed String
                            int shift = 0;
                            int trans = 0;
                            string uncomp = "";
                            while (true)
                            {
                                int tmp = car >> shift;
                                int tmp1 = tmp;
                                if (shift >= 0xF)
                                {
                                    shift -= 0xF;
                                    if (shift > 0)
                                    {
                                        tmp1 = (trans | ((car << (9 - shift)) & 0x1FF));
                                        if ((tmp1 & 0xFF) == 0xFF)
                                        {
                                            break;
                                        }
                                        if (tmp1 != 0x0 && tmp1 != 0x1)
                                        {
                                            string character = getChar.GetString(tmp1.ToString("X4"));
                                            pokemonText += character;
                                            if (character == null)
                                            {
                                                pokemonText += @"\x" + tmp1.ToString("X4");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    tmp1 = ((car >> shift) & 0x1FF);
                                    if ((tmp1 & 0xFF) == 0xFF)
                                    {
                                        break;
                                    }
                                    if (tmp1 != 0x0 && tmp1 != 0x1)
                                    {
                                        string character = getChar.GetString(tmp1.ToString("X4"));
                                        pokemonText += character;
                                        if (character == null)
                                        {
                                            pokemonText += @"\x" + tmp1.ToString("X4");
                                        }
                                    }
                                    shift += 9;
                                    if (shift < 0xF)
                                    {
                                        trans = ((car >> shift) & 0x1FF);
                                        shift += 9;
                                    }
                                    key1 += 0x493D;
                                    key1 &= 0xFFFF;
                                    car = Convert.ToUInt16(readText.ReadUInt16() ^ key1);
                                    j++;
                                }
                            }
                            #endregion
                            pokemonText += uncomp;
                        }
                        else
                        {
                            string character = getChar.GetString(car.ToString("X4"));
                            pokemonText += character;
                            if (character == null)
                            {
                                pokemonText += @"\x" + car.ToString("X4");
                            }
                        }
                    }
                    key1 += 0x493D;
                    key1 &= 0xFFFF;
                }
                dataGridView6.Rows.Add("", pokemonText);
                dataGridView6.Rows[i].HeaderCell.Value = i.ToString();
                progressBar1.Value = i;
                compressed = false;
            }
            readText.Close();
            progressBar1.Value = progressBar1.Maximum;
        }

        private void readTextV() // Read V Text File
        {
            string path;
            int mainKey = 31881;
            bool compressed = false;
            if (radioButton14.Checked)
            {
                path = "texts";
            }
            else
            {
                path = "texts2";
            }
            System.IO.BinaryReader readText = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\0\" + path + "\\" + comboBox3.SelectedIndex.ToString("D4")));
            textSections = readText.ReadUInt16();
            numericUpDown7.Value = textSections;
            numericUpDown7_ValueChanged(null, null);
            uint[] sectionOffset = new uint[3];
            uint[] sectionSize = new uint[3];
            stringCount = readText.ReadUInt16();
            if (stringCount == 0) button20.Enabled = false;
            int stringOffset;
            int stringSize;
            int[] stringUnknown = new int[3];
            progressBar1.Maximum = stringCount;
            sectionSize[0] = readText.ReadUInt32();
            initialKey = (int)readText.ReadUInt32();
            int key;
            for (int i = 0; i < textSections; i++)
            {
                sectionOffset[i] = readText.ReadUInt32();
            }
            for (int j = 0; j < stringCount; j++)
            {
                #region Layer 1
                readText.BaseStream.Position = sectionOffset[0];
                sectionSize[0] = readText.ReadUInt32();
                readText.BaseStream.Position += j * 8;
                stringOffset = (int)readText.ReadUInt32();
                stringSize = readText.ReadUInt16();
                stringUnknown[0] = readText.ReadUInt16();
                string pokemonText = "";
                string pokemonText2 = "";
                string pokemonText3 = "";
                readText.BaseStream.Position = sectionOffset[0] + stringOffset;
                key = mainKey;
                for (int k = 0; k < stringSize; k++)
                {
                    int car = Convert.ToUInt16(readText.ReadUInt16() ^ key);
                    if (compressed)
                    {
                        #region Compressed String
                        int shift = 0;
                        int trans = 0;
                        string uncomp = "";
                        while (true)
                        {
                            int tmp = car >> shift;
                            int tmp1 = tmp;
                            if (shift >= 0x10)
                            {
                                shift -= 0x10;
                                if (shift > 0)
                                {
                                    tmp1 = (trans | ((car << (9 - shift)) & 0x1FF));
                                    if ((tmp1 & 0xFF) == 0xFF)
                                    {
                                        break;
                                    }
                                    if (tmp1 != 0x0 && tmp1 != 0x1)
                                    {
                                        uncomp += Convert.ToChar(tmp1);
                                    }
                                }
                            }
                            else
                            {
                                tmp1 = ((car >> shift) & 0x1FF);
                                if ((tmp1 & 0xFF) == 0xFF)
                                {
                                    break;
                                }
                                if (tmp1 != 0x0 && tmp1 != 0x1)
                                {
                                    uncomp += Convert.ToChar(tmp1);
                                }
                                shift += 9;
                                if (shift < 0x10)
                                {
                                    trans = ((car >> shift) & 0x1FF);
                                    shift += 9;
                                }
                                key = ((key << 3) | (key >> 13)) & 0xFFFF;
                                car = Convert.ToUInt16(readText.ReadUInt16() ^ key);
                                k++;
                            }
                        }
                        #endregion
                        pokemonText += uncomp;
                    }
                    else if (car == 0xFFFF)
                    {
                    }
                    else if (car == 0xF100)
                    {
                        compressed = true;
                    }
                    else if (car == 0xFFFE)
                    {
                        pokemonText += @"\n";
                    }
                    else if (car > 20 && car <= 0xFFF0 && car != 0xF000 && Char.GetUnicodeCategory(Convert.ToChar(car)) != UnicodeCategory.OtherNotAssigned)
                    {
                        pokemonText += Convert.ToChar(car);
                    }
                    else
                    {
                        pokemonText += @"\x" + car.ToString("X4");
                    }
                    key = ((key << 3) | (key >> 13)) & 0xFFFF;
                }
                compressed = false;
                #endregion
                #region Layer 2
                if (numericUpDown7.Value > 1)
                {
                    readText.BaseStream.Position = sectionOffset[1];
                    sectionSize[1] = readText.ReadUInt32();
                    readText.BaseStream.Position += j * 8;
                    stringOffset = (int)readText.ReadUInt32();
                    stringSize = readText.ReadUInt16();
                    stringUnknown[1] = readText.ReadUInt16();
                    readText.BaseStream.Position = sectionOffset[1] + stringOffset;
                    key = mainKey;
                    for (int k = 0; k < stringSize; k++)
                    {
                        int car = Convert.ToUInt16(readText.ReadUInt16() ^ key);
                        if (compressed)
                        {
                            #region Compressed String
                            int shift = 0;
                            int trans = 0;
                            string uncomp = "";
                            while (true)
                            {
                                int tmp = car >> shift;
                                int tmp1 = tmp;
                                if (shift >= 0x10)
                                {
                                    shift -= 0x10;
                                    if (shift > 0)
                                    {
                                        tmp1 = (trans | ((car << (9 - shift)) & 0x1FF));
                                        if ((tmp1 & 0xFF) == 0xFF)
                                        {
                                            break;
                                        }
                                        if (tmp1 != 0x0 && tmp1 != 0x1)
                                        {
                                            uncomp += Convert.ToChar(tmp1);
                                        }
                                    }
                                }
                                else
                                {
                                    tmp1 = ((car >> shift) & 0x1FF);
                                    if ((tmp1 & 0xFF) == 0xFF)
                                    {
                                        break;
                                    }
                                    if (tmp1 != 0x0 && tmp1 != 0x1)
                                    {
                                        uncomp += Convert.ToChar(tmp1);
                                    }
                                    shift += 9;
                                    if (shift < 0x10)
                                    {
                                        trans = ((car >> shift) & 0x1FF);
                                        shift += 9;
                                    }
                                    key = ((key << 3) | (key >> 13)) & 0xFFFF;
                                    car = Convert.ToUInt16(readText.ReadUInt16() ^ key);
                                    k++;
                                }
                            }
                            #endregion
                            pokemonText2 += uncomp;
                        }
                        else if (car == 0xFFFF)
                        {
                        }
                        else if (car == 0xF100)
                        {
                            compressed = true;
                        }
                        else if (car == 0xFFFE)
                        {
                            pokemonText2 += @"\n";
                        }
                        else if (car > 20 && car <= 0xFFF0 && car != 0xF000 && Char.GetUnicodeCategory(Convert.ToChar(car)) != UnicodeCategory.OtherNotAssigned)
                        {
                            pokemonText2 += Convert.ToChar(car);
                        }
                        else
                        {
                            pokemonText2 += @"\x" + car.ToString("X4");
                        }
                        key = ((key << 3) | (key >> 13)) & 0xFFFF;
                    }
                    compressed = false;
                }
                #endregion
                #region Layer 3
                if (numericUpDown7.Value > 2)
                {
                    readText.BaseStream.Position = sectionOffset[2];
                    sectionSize[2] = readText.ReadUInt32();
                    readText.BaseStream.Position += j * 8;
                    stringOffset = (int)readText.ReadUInt32();
                    stringSize = readText.ReadUInt16();
                    stringUnknown[2] = readText.ReadUInt16();
                    readText.BaseStream.Position = sectionOffset[2] + stringOffset;
                    key = mainKey;
                    for (int k = 0; k < stringSize; k++)
                    {
                        int car = Convert.ToUInt16(readText.ReadUInt16() ^ key);
                        if (compressed)
                        {
                            #region Compressed String
                            int shift = 0;
                            int trans = 0;
                            string uncomp = "";
                            while (true)
                            {
                                int tmp = car >> shift;
                                int tmp1 = tmp;
                                if (shift >= 0x10)
                                {
                                    shift -= 0x10;
                                    if (shift > 0)
                                    {
                                        tmp1 = (trans | ((car << (9 - shift)) & 0x1FF));
                                        if ((tmp1 & 0xFF) == 0xFF)
                                        {
                                            break;
                                        }
                                        if (tmp1 != 0x0 && tmp1 != 0x1)
                                        {
                                            uncomp += Convert.ToChar(tmp1);
                                        }
                                    }
                                }
                                else
                                {
                                    tmp1 = ((car >> shift) & 0x1FF);
                                    if ((tmp1 & 0xFF) == 0xFF)
                                    {
                                        break;
                                    }
                                    if (tmp1 != 0x0 && tmp1 != 0x1)
                                    {
                                        uncomp += Convert.ToChar(tmp1);
                                    }
                                    shift += 9;
                                    if (shift < 0x10)
                                    {
                                        trans = ((car >> shift) & 0x1FF);
                                        shift += 9;
                                    }
                                    key = ((key << 3) | (key >> 13)) & 0xFFFF;
                                    car = Convert.ToUInt16(readText.ReadUInt16() ^ key);
                                    k++;
                                }
                            }
                            #endregion
                            pokemonText3 += uncomp;
                        }
                        else if (car == 0xFFFF)
                        {
                        }
                        else if (car == 0xF100)
                        {
                            compressed = true;
                        }
                        else if (car == 0xFFFE)
                        {
                            pokemonText3 += @"\n";
                        }
                        else if (car > 20 && car <= 0xFFF0 && car != 0xF000 && Char.GetUnicodeCategory(Convert.ToChar(car)) != UnicodeCategory.OtherNotAssigned)
                        {
                            pokemonText3 += Convert.ToChar(car);
                        }
                        else
                        {
                            pokemonText3 += @"\x" + car.ToString("X4");
                        }
                        key = ((key << 3) | (key >> 13)) & 0xFFFF;
                    }
                    compressed = false;
                }
                #endregion
                dataGridView6.Rows.Add("", pokemonText, stringUnknown[0], pokemonText2, stringUnknown[1], pokemonText3, stringUnknown[2]);
                dataGridView6.Rows[j].HeaderCell.Value = j.ToString();
                progressBar1.Value = j;
                mainKey += 0x2983;
                if (mainKey > 0xFFFF) mainKey -= 0x10000;
            }
            readText.Close();
            progressBar1.Value = progressBar1.Maximum;
        }

        private void saveStringBtn_Click(object sender, EventArgs e) // Save Text File
        {
            if (isBW || isB2W2)
            {
                saveTextV();
                return;
            }
            progressBar1.Maximum = stringCount;
            BinaryWriter writeText = new BinaryWriter(File.Create(textPath));
            writeText.Write((UInt16)stringCount);
            writeText.Write((UInt16)initialKey);
            int key = (initialKey * 0x2FD) & 0xFFFF;
            int key2 = 0;
            int realKey = 0;
            int offset = 0x4 + (stringCount * 8);
            int[] stringSize = new int[stringCount];
            for (int i = 0; i < stringCount; i++) // Reads and stores string offsets and sizes
            {
                key2 = (key * (i + 1) & 0xFFFF);
                realKey = key2 | (key2 << 16);
                writeText.Write(offset ^ realKey);
                int length = getStringLength(i);
                stringSize[i] = length;
                writeText.Write(length ^ realKey);
                offset += length * 2;
            }
            for (int i = 0; i < stringCount; i++) // Encodes strings and writes them to file
            {
                key = (0x91BD3 * (i + 1)) & 0xFFFF;
                int[] currentString = EncodeString(i, stringSize[i]);
                for (int j = 0; j < stringSize[i] - 1; j++)
                {
                    writeText.Write((UInt16)(currentString[j] ^ key));
                    key += 0x493D;
                    key &= 0xFFFF;
                }
                writeText.Write((UInt16)(0xFFFF ^ key));
                progressBar1.Value = i;
            }
            writeText.Close();
            progressBar1.Value = progressBar1.Maximum;
            #region Name List Updates
            // DP Updates
            if (comboBox3.SelectedIndex == 382  && (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041))
            {
                int[] index = new int[dataGridView1.RowCount];
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    index[i] = nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString());
                }
                nameText.Clear();
                for (int i = 0; i < dataGridView6.RowCount; i++)
                {
                    nameText.Add(dataGridView6.Rows[i].Cells[1].Value.ToString());
                }
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    dataGridView1.Rows[i].Cells[12].Value = nameText[index[i]];
                }
            }
            if (comboBox3.SelectedIndex == 374  && (gameID == 0x4A414441 || gameID == 0x4A415041))
            {
                int[] index = new int[dataGridView1.RowCount];
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    index[i] = nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString());
                }
                nameText.Clear();
                for (int i = 0; i < dataGridView6.RowCount; i++)
                {
                    nameText.Add(dataGridView6.Rows[i].Cells[1].Value.ToString());
                }
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    dataGridView1.Rows[i].Cells[12].Value = nameText[index[i]];
                }
            }
            if (comboBox3.SelectedIndex == 376 && (gameID == 0x4B414441 || gameID == 0x4B415041))
            {
                int[] index = new int[dataGridView1.RowCount];
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    index[i] = nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString());
                }
                nameText.Clear();
                for (int i = 0; i < dataGridView6.RowCount; i++)
                {
                    nameText.Add(dataGridView6.Rows[i].Cells[1].Value.ToString());
                }
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    dataGridView1.Rows[i].Cells[12].Value = nameText[index[i]];
                }
            }
            // Platinum Update
            if (comboBox3.SelectedIndex == 433 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043))
            {
                int[] index = new int[dataGridView1.RowCount];
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    index[i] = nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString());
                }
                nameText.Clear();
                for (int i = 0; i < dataGridView6.RowCount; i++)
                {
                    nameText.Add(dataGridView6.Rows[i].Cells[1].Value.ToString());
                }
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    dataGridView1.Rows[i].Cells[12].Value = nameText[index[i]];
                }
            }
            if (comboBox3.SelectedIndex == 427 && gameID == 0x4A555043)
            {
                int[] index = new int[dataGridView1.RowCount];
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    index[i] = nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString());
                }
                nameText.Clear();
                for (int i = 0; i < dataGridView6.RowCount; i++)
                {
                    nameText.Add(dataGridView6.Rows[i].Cells[1].Value.ToString());
                }
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    dataGridView1.Rows[i].Cells[12].Value = nameText[index[i]];
                }
            }
            if (comboBox3.SelectedIndex == 428 && gameID == 0x4B555043)
            {
                int[] index = new int[dataGridView1.RowCount];
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    index[i] = nameText.IndexOf(dataGridView1.Rows[i].Cells[12].Value.ToString());
                }
                nameText.Clear();
                for (int i = 0; i < dataGridView6.RowCount; i++)
                {
                    nameText.Add(dataGridView6.Rows[i].Cells[1].Value.ToString());
                }
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    dataGridView1.Rows[i].Cells[12].Value = nameText[index[i]];
                }
            }
            // HGSS Update
            if (comboBox3.SelectedIndex == 279 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049))
            {
                int[] index = new int[dataGridView1.RowCount];
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    index[i] = nameText.IndexOf(dataGridView1.Rows[i].Cells[11].Value.ToString());
                }
                nameText.Clear();
                for (int i = 0; i < dataGridView6.RowCount; i++)
                {
                    nameText.Add(dataGridView6.Rows[i].Cells[1].Value.ToString());
                }
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    dataGridView1.Rows[i].Cells[11].Value = nameText[index[i]];
                }
            }
            if (comboBox3.SelectedIndex == 272 && (gameID == 0x4A4B5049 || gameID == 0x4A475049))
            {
                int[] index = new int[dataGridView1.RowCount];
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    index[i] = nameText.IndexOf(dataGridView1.Rows[i].Cells[11].Value.ToString());
                }
                nameText.Clear();
                for (int i = 0; i < dataGridView6.RowCount; i++)
                {
                    nameText.Add(dataGridView6.Rows[i].Cells[1].Value.ToString());
                }
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    dataGridView1.Rows[i].Cells[11].Value = nameText[index[i]];
                }
            }
            if (comboBox3.SelectedIndex == 274 && (gameID == 0x4B4B5049 || gameID == 0x4B475049))
            {
                int[] index = new int[dataGridView1.RowCount];
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    index[i] = nameText.IndexOf(dataGridView1.Rows[i].Cells[11].Value.ToString());
                }
                nameText.Clear();
                for (int i = 0; i < dataGridView6.RowCount; i++)
                {
                    nameText.Add(dataGridView6.Rows[i].Cells[1].Value.ToString());
                }
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    dataGridView1.Rows[i].Cells[11].Value = nameText[index[i]];
                }
            }
            #endregion
        }

        private void saveTextV() // Save V Text File
        {
            string path;
            int mainKey = 31881;
            if (radioButton14.Checked)
            {
                path = "texts";
            }
            else
            {
                path = "texts2";
            }
            BinaryWriter writeText = new BinaryWriter(File.Create(workingFolder + @"data\a\0\0\" + path + "\\" + comboBox3.SelectedIndex.ToString("D4")));
            writeText.Write(Convert.ToUInt16(numericUpDown7.Value));
            writeText.Write(Convert.ToUInt16(stringCount));
            int[] sectionSize = new int[3];
            int[] stringLength = new int[stringCount];
            int[] stringLength2 = new int[stringCount];
            int[] stringLength3 = new int[stringCount];
            int key;
            sectionSize[0] = 4 + (8 * stringCount);
            for (int i = 0; i < stringCount; i++)
            {
                stringLength[i] = getVStringLength(i, 1);
                sectionSize[0] += stringLength[i] * 2;
            }
            if (numericUpDown7.Value > 1)
            {
                sectionSize[1] = 4 + (8 * stringCount);
                for (int i = 0; i < stringCount; i++)
                {
                    stringLength2[i] = getVStringLength(i, 3);
                    sectionSize[1] += stringLength2[i] * 2;
                }
                if (numericUpDown7.Value > 2)
                {
                    sectionSize[2] = 4 + (8 * stringCount);
                    for (int i = 0; i < stringCount; i++)
                    {
                        stringLength3[i] = getVStringLength(i, 3);
                        sectionSize[2] += stringLength3[i] * 2;
                    }
                }
            }
            writeText.Write(sectionSize[0]);
            writeText.Write(initialKey);
            if (numericUpDown7.Value == 1)
            {
                writeText.Write(0x10);
            }
            else if (numericUpDown7.Value == 2)
            {
                writeText.Write(0x14);
                writeText.Write(0x14 + sectionSize[0]);
            }
            else
            {
                writeText.Write(0x18);
                writeText.Write(0x18 + sectionSize[0]);
                writeText.Write(0x18 + sectionSize[0] + sectionSize[1]);
            }

            #region Layer 1
            writeText.Write(sectionSize[0]);
            int offset = 4 + 8 * stringCount;
            for (int i = 0; i < stringCount; i++)
            {
                writeText.Write(offset);
                writeText.Write(Convert.ToUInt16(stringLength[i]));
                writeText.Write(Convert.ToUInt16(dataGridView6.Rows[i].Cells[2].Value));
                offset += stringLength[i] * 2;
            }
            for (int i = 0; i < stringCount; i++)
            {
                int[] currentString = EncodeVString(i, 1, stringLength[i]);
                key = mainKey;
                for (int j = 0; j < stringLength[i]; j++)
                {
                    if (j == stringLength[i] - 1)
                    {
                        writeText.Write(Convert.ToUInt16(key ^ 0xFFFF));
                        break;
                    }
                    writeText.Write(Convert.ToUInt16((currentString[j] ^ key) & 0xFFFF));
                    key = ((key << 3) | (key >> 13)) & 0xFFFF;
                }
                mainKey += 0x2983;
                if (mainKey > 0xFFFF) mainKey -= 0x10000;
            }
            #endregion
            #region Layer 2
            if (numericUpDown7.Value > 1)
            {
                mainKey = 31881;
                writeText.Write(sectionSize[1]);
                offset = 4 + 8 * stringCount;
                for (int i = 0; i < stringCount; i++)
                {
                    writeText.Write(offset);
                    writeText.Write(Convert.ToUInt16(stringLength2[i]));
                    writeText.Write(Convert.ToUInt16(dataGridView6.Rows[i].Cells[4].Value));
                    offset += stringLength2[i] * 2;
                }
                for (int i = 0; i < stringCount; i++)
                {
                    int[] currentString = EncodeVString(i, 3, stringLength2[i]);
                    key = mainKey;
                    for (int j = 0; j < stringLength2[i]; j++)
                    {
                        if (j == stringLength2[i] - 1)
                        {
                            writeText.Write(Convert.ToUInt16(key ^ 0xFFFF));
                            break;
                        }
                        writeText.Write(Convert.ToUInt16((currentString[j] ^ key) & 0xFFFF));
                        key = ((key << 3) | (key >> 13)) & 0xFFFF;
                    }
                    mainKey += 0x2983;
                    if (mainKey > 0xFFFF) mainKey -= 0x10000;
                }
                #region Layer 3
                if (numericUpDown7.Value > 2)
                {
                    mainKey = 31881;
                    writeText.Write(sectionSize[2]);
                    offset = 4 + 8 * stringCount;
                    for (int i = 0; i < stringCount; i++)
                    {
                        writeText.Write(offset);
                        writeText.Write(Convert.ToUInt16(stringLength3[i]));
                        writeText.Write(Convert.ToUInt16(dataGridView6.Rows[i].Cells[6].Value));
                        offset += stringLength3[i] * 2;
                    }
                    for (int i = 0; i < stringCount; i++)
                    {
                        int[] currentString = EncodeVString(i, 5, stringLength3[i]);
                        key = mainKey;
                        for (int j = 0; j < stringLength3[i]; j++)
                        {
                            if (j == stringLength3[i] - 1)
                            {
                                writeText.Write(Convert.ToUInt16(key ^ 0xFFFF));
                                break;
                            }
                            writeText.Write(Convert.ToUInt16((currentString[j] ^ key) & 0xFFFF));
                            key = ((key << 3) | (key >> 13)) & 0xFFFF;
                        }
                        mainKey += 0x2983;
                        if (mainKey > 0xFFFF) mainKey -= 0x10000;
                    }
                }
                #endregion
            }
            #endregion
            writeText.Close();
            #region Name List Updates
            if (comboBox3.SelectedIndex == 89 && radioButton14.Checked && isBW) // BW Place Names
            {
                int[] index = new int[dataGridView7.RowCount];
                for (int i = 0; i < dataGridView7.RowCount; i++)
                {
                    index[i] = nameText.IndexOf(dataGridView7.Rows[i].Cells[16].Value.ToString());
                }
                nameText.Clear();
                for (int i = 0; i < dataGridView6.RowCount; i++)
                {
                    nameText.Add(dataGridView6.Rows[i].Cells[1].Value.ToString());
                }
                for (int i = 0; i < dataGridView7.RowCount; i++)
                {
                    dataGridView7.Rows[i].Cells[16].Value = nameText[index[i]];
                }
            }
            if (comboBox3.SelectedIndex == 109 && radioButton14.Checked && isB2W2) // B2W2 Place Names
            {
                int[] index = new int[dataGridView7.RowCount];
                for (int i = 0; i < dataGridView7.RowCount; i++)
                {
                    index[i] = nameText.IndexOf(dataGridView7.Rows[i].Cells[16].Value.ToString());
                }
                nameText.Clear();
                for (int i = 0; i < dataGridView6.RowCount; i++)
                {
                    nameText.Add(dataGridView6.Rows[i].Cells[1].Value.ToString());
                }
                for (int i = 0; i < dataGridView7.RowCount; i++)
                {
                    dataGridView7.Rows[i].Cells[16].Value = nameText[index[i]];
                }
            }
            #endregion
        }

        private void button18_Click(object sender, EventArgs e) // Add String
        {
            int index = dataGridView6.Rows.Count;
            dataGridView6.Rows.Add("", "", 0, "", 0, "", 0);
            dataGridView6.Rows[index].HeaderCell.Value = index.ToString();
            dataGridView6.CurrentCell = dataGridView6.Rows[index].Cells[1];
            stringCount++;
        }

        private void button20_Click_1(object sender, EventArgs e) // Remove Last String
        {
            int index = dataGridView6.Rows.Count - 1;
            dataGridView6.Rows.RemoveAt(index);
            stringCount--;
            if (stringCount == 0)
            {
                button20.Enabled = false;
            }
        }

        private int getStringLength(int stringIndex) // Calculates string length
        {
            int count = 0;
            string currentMessage = "";
            try { currentMessage = dataGridView6[1, stringIndex].Value.ToString(); }
            catch { }
            var charArray = currentMessage.ToCharArray();
            for (int i = 0; i < currentMessage.Length; i++)
            {
                if (charArray[i] == '\\')
                {
                    if (charArray[i + 1] == 'r')
                    {
                        count++;
                        i++;
                    }
                    else
                    {
                        if (charArray[i + 1] == 'n')
                        {
                            count++;
                            i++;
                        }
                        else
                        {
                            if (charArray[i + 1] == 'f')
                            {
                                count++;
                                i++;
                            }
                            else
                            {
                                if (charArray[i + 1] == 'v')
                                {
                                    count += 2;
                                    i += 5;
                                }
                                else
                                {
                                    if (charArray[i + 1] == 'x' && charArray[i + 2] == '0' && charArray[i + 3] == '0' && charArray[i + 4] == '0' && charArray[i + 5] == '0')
                                    {
                                        count++;
                                        i += 5;
                                    }
                                    else
                                    {
                                        if (charArray[i + 1] == 'x' && charArray[i + 2] == '0' && charArray[i + 3] == '0' && charArray[i + 4] == '0' && charArray[i + 5] == '1')
                                        {
                                            count++;
                                            i += 5;
                                        }
                                        else
                                        {
                                            count++;
                                            i += 5;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (charArray[i] == '[')
                    {
                        if (charArray[i + 1] == 'P')
                        {
                            count++;
                            i += 3;
                        }
                        if (charArray[i + 1] == 'M')
                        {
                            count++;
                            i += 3;
                        }
                    }
                    else
                    {
                        count++;
                    }
                }
            }
            count++;
            return count;
        }

        private int getVStringLength(int stringIndex, int column) // Calculates V string length
        {
            int count = 0;
            string currentMessage = "";
            if (dataGridView6[column, stringIndex].Value == null)
            {
                return 1;
            }
            currentMessage = dataGridView6[column, stringIndex].Value.ToString();
            var charArray = currentMessage.ToCharArray();
            for (int i = 0; i < currentMessage.Length; i++)
            {
                if (charArray[i] == '\\')
                {
                    if (charArray[i + 1] == 'n')
                    {
                        count++;
                        i++;
                    }
                    else
                    {
                        if (charArray[i + 1] == 'x')
                        {
                            count++;
                            i += 5;
                        }
                        else
                        {
                            count++;
                        }
                    }
                }
                else
                {
                    count++;
                }
            }
            count++;
            return count;
        }

        private int[] EncodeString(int stringIndex, int stringSize) // Converts string to hex characters
        {
            int[] pokemonMessage = new int[stringSize - 1];
            string currentMessage = "";
            try { currentMessage = dataGridView6[1, stringIndex].Value.ToString(); }
            catch { }
            var charArray = currentMessage.ToCharArray();
            int count = 0;
            for (int i = 0; i < currentMessage.Length; i++)
            {
                if (charArray[i] == '\\')
                {
                    if (charArray[i + 1] == 'r')
                    {
                        pokemonMessage[count] = 0x25BC;
                        i++;
                    }
                    else
                    {
                        if (charArray[i + 1] == 'n')
                        {
                            pokemonMessage[count] = 0xE000;
                            i++;
                        }
                        else
                        {
                            if (charArray[i + 1] == 'f')
                            {
                                pokemonMessage[count] = 0x25BD;
                                i++;
                            }
                            else
                            {
                                if (charArray[i + 1] == 'v')
                                {
                                    pokemonMessage[count] = 0xFFFE;
                                    count++;
                                    string characterID = ((char)charArray[i + 2]).ToString() + ((char)charArray[i + 3]).ToString() + ((char)charArray[i + 4]).ToString() + ((char)charArray[i + 5]).ToString();
                                    pokemonMessage[count] = (int)Convert.ToUInt32(characterID, 16);
                                    i += 5;
                                }
                                else
                                {
                                    if (charArray[i + 1] == 'x' && charArray[i + 2] == '0' && charArray[i + 3] == '0' && charArray[i + 4] == '0' && charArray[i + 5] == '0')
                                    {
                                        pokemonMessage[count] = 0x0000;
                                        i += 5;
                                    }
                                    else
                                    {
                                        if (charArray[i + 1] == 'x' && charArray[i + 2] == '0' && charArray[i + 3] == '0' && charArray[i + 4] == '0' && charArray[i + 5] == '1')
                                        {
                                            pokemonMessage[count] = 0x0001;
                                            i += 5;
                                        }
                                        else
                                        {
                                            string characterID = ((char)charArray[i + 2]).ToString() + ((char)charArray[i + 3]).ToString() + ((char)charArray[i + 4]).ToString() + ((char)charArray[i + 5]).ToString();
                                            pokemonMessage[count] = (int)Convert.ToUInt32(characterID, 16);
                                            i += 5;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (charArray[i] == '[')
                    {
                        if (charArray[i + 1] == 'P')
                        {
                            pokemonMessage[count] = 0x01E0;
                            i += 3;
                        }
                        if (charArray[i + 1] == 'M')
                        {
                            pokemonMessage[count] = 0x01E1;
                            i += 3;
                        }
                    }
                    else
                    {
                        pokemonMessage[count] = (int)Convert.ToUInt32(getByte.GetString(((int)charArray[i]).ToString()), 16);
                    }
                }
                count++;
            }
            return pokemonMessage;
        }

        private int[] EncodeVString(int stringIndex, int column, int stringSize) // Converts V string to hex characters
        {
            int[] pokemonMessage = new int[stringSize - 1];
            string currentMessage = "";
            try { currentMessage = dataGridView6[column, stringIndex].Value.ToString(); }
            catch { }
            var charArray = currentMessage.ToCharArray();
            int count = 0;
            for (int i = 0; i < currentMessage.Length; i++)
            {
                if (charArray[i] == '\\')
                {
                    if (charArray[i + 1] == 'n')
                    {
                        pokemonMessage[count] = 0xFFFE;
                        i++;
                    }
                    else
                    {
                        if (charArray[i + 1] == 'x')
                        {
                            string characterID = ((char)charArray[i + 2]).ToString() + ((char)charArray[i + 3]).ToString() + ((char)charArray[i + 4]).ToString() + ((char)charArray[i + 5]).ToString();
                            pokemonMessage[count] = (int)Convert.ToUInt32(characterID, 16);
                            i += 5;
                        }
                        else
                        {
                            pokemonMessage[count] = (int)charArray[i];
                        }
                    }
                }
                else
                {
                    pokemonMessage[count] = (int)charArray[i];
                }
                count++;
            }
            return pokemonMessage;
        }

        private void numericUpDown7_ValueChanged(object sender, EventArgs e) // Layer Amount Change
        {
            if (numericUpDown7.Value == 1)
            {
                Column51.Visible = false;
                Column52.Visible = false;
                Column53.Visible = false;
                Column54.Visible = false;
            }
            else if (numericUpDown7.Value == 2)
            {
                Column51.Visible = true;
                Column52.Visible = true;
                Column53.Visible = false;
                Column54.Visible = false;
            }
            else
            {
                Column51.Visible = true;
                Column52.Visible = true;
                Column53.Visible = true;
                Column54.Visible = true;
            }
            saveStringBtn.Enabled = true;
            button18.Enabled = true;
            button20.Enabled = true;
        }

        private void radioButton14_CheckedChanged(object sender, EventArgs e) // Text Type Change
        {
            if (radioButton14.Checked)
            {
                textCount = Directory.GetFiles(workingFolder + @"data\a\0\0\texts").Length;
            }
            else
            {
                textCount = Directory.GetFiles(workingFolder + @"data\a\0\0\texts2").Length;
            }
            comboBox3.Items.Clear();
            for (int i = 0; i < textCount; i++)
            {
                comboBox3.Items.Add(rm.GetString("text") + i);
            }
            saveStringBtn.Enabled = false;
            button18.Enabled = false;
            button20.Enabled = false;
        }

        #endregion

        #region Event Editor

        private void comboBox10_SelectedIndexChanged(object sender, EventArgs e)
        {
            button57.Enabled = false;
            button58.Enabled = false;
            button59.Enabled = false;
            button60.Enabled = false;
            button29.Enabled = false;
            button51.Enabled = false;
            button53.Enabled = false;
            button55.Enabled = false;
            listBox7.Items.Clear();
            listBox8.Items.Clear();
            listBox9.Items.Clear();
            listBox10.Items.Clear();
            #region Reset
            numericUpDown14.Value = 0;
            numericUpDown15.Value = 0;
            numericUpDown54.Value = 1;
            numericUpDown55.Value = 1;
            numericUpDown17.Value = 0;
            numericUpDown16.Value = 1;
            numericUpDown18.Value = 1;
            numericUpDown19.Value = 0;
            numericUpDown20.Value = 0;
            numericUpDown21.Value = 0;
            numericUpDown22.Value = 0;
            numericUpDown23.Value = 0;

            numericUpDown33.Value = 0;
            numericUpDown32.Value = 0;
            numericUpDown31.Value = 0;
            radioButton25.Checked = true;
            numericUpDown29.Value = 0;
            numericUpDown28.Value = 0;
            comboBox11.SelectedIndex = 0;
            numericUpDown26.Value = 0;
            numericUpDown25.Value = 0;
            numericUpDown24.Value = 0;
            numericUpDown34.Value = 0;
            numericUpDown35.Value = 0;
            numericUpDown56.Value = 1;
            numericUpDown57.Value = 1;
            numericUpDown36.Value = 1;
            numericUpDown37.Value = 1;
            numericUpDown38.Value = 0;
            numericUpDown39.Value = 0;

            numericUpDown58.Value = 1;
            numericUpDown59.Value = 1;
            numericUpDown49.Value = 1;
            numericUpDown48.Value = 1;
            numericUpDown47.Value = 0;
            numericUpDown46.Value = 1;
            numericUpDown45.Value = 0;
            numericUpDown44.Value = 0;

            numericUpDown51.Value = 0;
            numericUpDown60.Value = 1;
            numericUpDown61.Value = 1;
            numericUpDown50.Value = 1;
            numericUpDown43.Value = 1;
            numericUpDown42.Value = 0;
            numericUpDown41.Value = 0;
            numericUpDown40.Value = 0;
            numericUpDown52.Value = 0;
            numericUpDown53.Value = 0;
            #endregion
            System.IO.BinaryReader readEvent = new System.IO.BinaryReader(File.OpenRead(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
            int furnitureCount = readEvent.ReadInt32();
            readEvent.BaseStream.Position += furnitureCount * 0x14;
            int overworldCount = readEvent.ReadInt32();
            readEvent.BaseStream.Position += overworldCount * 0x20;
            int warpCount = readEvent.ReadInt32();
            readEvent.BaseStream.Position += warpCount * 0xc;
            int triggerCount = readEvent.ReadInt32();
            readEvent.Close();
            for (int i = 0; i < furnitureCount; i++)
            {
                listBox7.Items.Add(rm.GetString("furniture") + (i + 1));
            }
            for (int i = 0; i < overworldCount; i++)
            {
                listBox8.Items.Add(rm.GetString("overworld") + (i + 1));
            }
            for (int i = 0; i < warpCount; i++)
            {
                listBox9.Items.Add(rm.GetString("warp") + (i + 1));
            }
            for (int i = 0; i < triggerCount; i++)
            {
                listBox10.Items.Add(rm.GetString("trigger") + (i + 1));
            }
            if (listBox7.Items.Count != 0)
            {
                listBox7.SelectedIndex = 0;
                button29.Enabled = true;
            }
            if (listBox8.Items.Count != 0)
            {
                listBox8.SelectedIndex = 0;
                button51.Enabled = true;
            }
            if (listBox9.Items.Count != 0)
            {
                listBox9.SelectedIndex = 0;
                button53.Enabled = true;
            }
            if (listBox10.Items.Count != 0)
            {
                listBox10.SelectedIndex = 0;
                button55.Enabled = true;
            }
        }

        private void listBox7_SelectedIndexChanged(object sender, EventArgs e) // Select Furniture
        {
            if (listBox7.SelectedIndex == -1)
            {
                button29.Enabled = false;
                return;
            }
            button29.Enabled = true;
            button57.Enabled = true;
            System.IO.BinaryReader readEvent = new System.IO.BinaryReader(File.OpenRead(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
            readEvent.BaseStream.Position = 0x4 + 0x14 * listBox7.SelectedIndex;
            numericUpDown14.Value = readEvent.ReadInt16();
            numericUpDown15.Value = readEvent.ReadInt16();
            int x = readEvent.ReadInt16();
            numericUpDown17.Value = readEvent.ReadInt16();
            int y = readEvent.ReadInt16();
            numericUpDown54.Value = (x / 32) + 1;
            numericUpDown55.Value = (y / 32) + 1;
            numericUpDown16.Value = (x % 32) + 1;
            numericUpDown18.Value = (y % 32) + 1;
            numericUpDown19.Value = readEvent.ReadInt16();
            numericUpDown20.Value = readEvent.ReadInt16();
            numericUpDown21.Value = readEvent.ReadInt16();
            numericUpDown22.Value = readEvent.ReadInt16();
            numericUpDown23.Value = readEvent.ReadInt16();
            readEvent.Close();
        }

        private void listBox8_SelectedIndexChanged(object sender, EventArgs e) // Select Overworld
        {
            if (listBox8.SelectedIndex == -1)
            {
                button51.Enabled = false;
                return;
            }
            button51.Enabled = true;
            button58.Enabled = true;
            System.IO.BinaryReader readEvent = new System.IO.BinaryReader(File.OpenRead(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
            readEvent.BaseStream.Position = 0x8 + 0x14 * listBox7.Items.Count + 0x20 * listBox8.SelectedIndex;
            numericUpDown33.Value = readEvent.ReadInt16();
            numericUpDown32.Value = readEvent.ReadInt16();
            numericUpDown31.Value = readEvent.ReadInt16();
            int isTrainer = readEvent.ReadInt16();
            if (isTrainer == 1) radioButton24.Checked = true;
            else radioButton25.Checked = true;
            numericUpDown29.Value = readEvent.ReadInt16();
            numericUpDown28.Value = readEvent.ReadInt16();
            comboBox11.SelectedIndex = readEvent.ReadInt16();
            numericUpDown26.Value = readEvent.ReadInt16();
            numericUpDown25.Value = readEvent.ReadInt16();
            numericUpDown24.Value = readEvent.ReadInt16();
            numericUpDown34.Value = readEvent.ReadInt16();
            numericUpDown35.Value = readEvent.ReadInt16();
            int x = readEvent.ReadInt16();
            int y = readEvent.ReadInt16();
            numericUpDown56.Value = (x / 32) + 1;
            numericUpDown57.Value = (y / 32) + 1;
            numericUpDown36.Value = (x % 32) + 1;
            numericUpDown37.Value = (y % 32) + 1;
            numericUpDown38.Value = readEvent.ReadInt16();
            numericUpDown39.Value = readEvent.ReadInt16();
            readEvent.Close();
        }

        private void listBox9_SelectedIndexChanged(object sender, EventArgs e) // Select Warp
        {
            if (listBox9.SelectedIndex == -1)
            {
                button53.Enabled = false;
                return;
            }
            button53.Enabled = true;
            button59.Enabled = true;
            System.IO.BinaryReader readEvent = new System.IO.BinaryReader(File.OpenRead(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
            readEvent.BaseStream.Position = 0xc + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count + 0xc * listBox9.SelectedIndex;
            int x = readEvent.ReadInt16();
            int y = readEvent.ReadInt16();
            numericUpDown58.Value = (x / 32) + 1;
            numericUpDown59.Value = (y / 32) + 1;
            numericUpDown49.Value = (x % 32) + 1;
            numericUpDown48.Value = (y % 32) + 1;
            numericUpDown47.Value = readEvent.ReadInt16();
            numericUpDown46.Value = readEvent.ReadInt16() + 1;
            numericUpDown45.Value = readEvent.ReadInt16();
            numericUpDown44.Value = readEvent.ReadInt16();
            readEvent.Close();
        }

        private void listBox10_SelectedIndexChanged(object sender, EventArgs e) // Select Trigger
        {
            if (listBox10.SelectedIndex == -1)
            {
                button55.Enabled = false;
                return;
            }
            button55.Enabled = true;
            button60.Enabled = true;
            System.IO.BinaryReader readEvent = new System.IO.BinaryReader(File.OpenRead(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
            readEvent.BaseStream.Position = 0x10 + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count + 0xc * listBox9.Items.Count + 0x10 * listBox10.SelectedIndex;
            numericUpDown51.Value = readEvent.ReadInt16();
            int x = readEvent.ReadInt16();
            int y = readEvent.ReadInt16();
            numericUpDown60.Value = (x / 32) + 1;
            numericUpDown61.Value = (y / 32) + 1;
            numericUpDown50.Value = (x % 32) + 1;
            numericUpDown43.Value = (y % 32) + 1;
            numericUpDown42.Value = readEvent.ReadInt16();
            numericUpDown41.Value = readEvent.ReadInt16();
            numericUpDown40.Value = readEvent.ReadInt16();
            numericUpDown52.Value = readEvent.ReadInt16();
            numericUpDown53.Value = readEvent.ReadInt16();
            readEvent.Close();
        }

        private void button57_Click(object sender, EventArgs e) // Save Furniture
        {
            if (listBox7.SelectedIndex != -1)
            {
                System.IO.BinaryWriter writeEvent = new System.IO.BinaryWriter(File.OpenWrite(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
                writeEvent.BaseStream.Position = 0x4 + 0x14 * listBox7.SelectedIndex;
                writeEvent.Write((Int16)numericUpDown14.Value);
                writeEvent.Write((Int16)numericUpDown15.Value);
                writeEvent.Write((Int16)((numericUpDown54.Value - 1) * 32 + (numericUpDown16.Value - 1)));
                writeEvent.Write((Int16)numericUpDown17.Value);
                writeEvent.Write((Int16)((numericUpDown55.Value - 1) * 32 + (numericUpDown18.Value - 1)));
                writeEvent.Write((Int16)numericUpDown19.Value);
                writeEvent.Write((Int16)numericUpDown20.Value);
                writeEvent.Write((Int16)numericUpDown21.Value);
                writeEvent.Write((Int16)numericUpDown22.Value);
                writeEvent.Write((Int16)numericUpDown23.Value);
                writeEvent.Close();
            }
        }

        private void button58_Click(object sender, EventArgs e) // Save Overworld
        {
            if (listBox8.SelectedIndex != -1)
            {
                System.IO.BinaryWriter writeEvent = new System.IO.BinaryWriter(File.OpenWrite(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
                writeEvent.BaseStream.Position = 0x8 + 0x14 * listBox7.Items.Count + 0x20 * listBox8.SelectedIndex;
                writeEvent.Write((Int16)numericUpDown33.Value);
                writeEvent.Write((Int16)numericUpDown32.Value);
                writeEvent.Write((Int16)numericUpDown31.Value);
                if (radioButton24.Checked == true) writeEvent.Write((Int16)1);
                else writeEvent.Write((Int16)0);
                writeEvent.Write((Int16)numericUpDown29.Value);
                writeEvent.Write((Int16)numericUpDown28.Value);
                writeEvent.Write((Int16)comboBox11.SelectedIndex);
                writeEvent.Write((Int16)numericUpDown26.Value);
                writeEvent.Write((Int16)numericUpDown25.Value);
                writeEvent.Write((Int16)numericUpDown24.Value);
                writeEvent.Write((Int16)numericUpDown34.Value);
                writeEvent.Write((Int16)numericUpDown35.Value);
                writeEvent.Write((Int16)((numericUpDown56.Value - 1) * 32 + (numericUpDown36.Value - 1)));
                writeEvent.Write((Int16)((numericUpDown57.Value - 1) * 32 + (numericUpDown37.Value - 1)));
                writeEvent.Write((Int16)numericUpDown38.Value);
                writeEvent.Write((Int16)numericUpDown39.Value);
                writeEvent.Close();
            }
        }

        private void button59_Click(object sender, EventArgs e) // Save Warp
        {
            if (listBox9.SelectedIndex != -1)
            {
                System.IO.BinaryWriter writeEvent = new System.IO.BinaryWriter(File.OpenWrite(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
                writeEvent.BaseStream.Position = 0xC + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count + 0xc * listBox9.SelectedIndex;
                writeEvent.Write((Int16)((numericUpDown58.Value - 1) * 32 + (numericUpDown49.Value - 1)));
                writeEvent.Write((Int16)((numericUpDown59.Value - 1) * 32 + (numericUpDown48.Value - 1)));
                writeEvent.Write((Int16)numericUpDown47.Value);
                writeEvent.Write((Int16)(numericUpDown46.Value - 1));
                writeEvent.Write((Int16)numericUpDown45.Value);
                writeEvent.Write((Int16)numericUpDown44.Value);
                writeEvent.Close();
            }
        }

        private void button60_Click(object sender, EventArgs e) // Save Trigger
        {
            if (listBox10.SelectedIndex != -1)
            {
                System.IO.BinaryWriter writeEvent = new System.IO.BinaryWriter(File.OpenWrite(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
                writeEvent.BaseStream.Position = 0x10 + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count + 0xc * listBox9.Items.Count + 0x10 * listBox10.SelectedIndex;
                writeEvent.Write((Int16)numericUpDown51.Value);
                writeEvent.Write((Int16)((numericUpDown60.Value - 1) * 32 + (numericUpDown50.Value - 1)));
                writeEvent.Write((Int16)((numericUpDown61.Value - 1) * 32 + (numericUpDown43.Value - 1)));
                writeEvent.Write((Int16)numericUpDown42.Value);
                writeEvent.Write((Int16)numericUpDown41.Value);
                writeEvent.Write((Int16)numericUpDown40.Value);
                writeEvent.Write((Int16)numericUpDown52.Value);
                writeEvent.Write((Int16)numericUpDown53.Value);
                writeEvent.Close();
            }
        }

        private void button30_Click(object sender, EventArgs e) // Add Furniture
        {
            System.IO.BinaryReader readEvent = new System.IO.BinaryReader(File.OpenRead(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
            File.Create(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new").Close();
            System.IO.BinaryWriter writeEvent = new System.IO.BinaryWriter(File.OpenWrite(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new"));
            for (int i = 0; i < (0x4 + 0x14 * listBox7.Items.Count); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            for (int i = 0; i < (0x14); i++)
            {
                writeEvent.Write((byte)0x0); // Writes new furniture
            }
            for (int i = 0; i < (readEvent.BaseStream.Length - (0x4 + 0x14 * listBox7.Items.Count)); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            writeEvent.BaseStream.Position = 0x0;
            writeEvent.Write(listBox7.Items.Count + 1);
            readEvent.Close();
            writeEvent.Close();
            File.Delete(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            File.Move(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new", eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            int count = listBox7.Items.Count;
            listBox7.Items.Add(rm.GetString("furniture") + (count + 1));
            listBox7.SelectedIndex = listBox7.Items.Count - 1;
            button57.Enabled = true;
            button29.Enabled = true;
        }

        private void button52_Click(object sender, EventArgs e) // Add Overworld
        {
            System.IO.BinaryReader readEvent = new System.IO.BinaryReader(File.OpenRead(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
            File.Create(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new").Close();
            System.IO.BinaryWriter writeEvent = new System.IO.BinaryWriter(File.OpenWrite(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new"));
            for (int i = 0; i < (0x8 + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            for (int i = 0; i < (0x20); i++)
            {
                writeEvent.Write((byte)0x0); // Writes new furniture
            }
            for (int i = 0; i < (readEvent.BaseStream.Length - (0x8 + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count)); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            writeEvent.BaseStream.Position = 0x4 + listBox7.Items.Count * 0x14;
            writeEvent.Write(listBox8.Items.Count + 1);
            readEvent.Close();
            writeEvent.Close();
            File.Delete(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            File.Move(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new", eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            int count = listBox8.Items.Count;
            listBox8.Items.Add(rm.GetString("overworld") + (count + 1));
            listBox8.SelectedIndex = listBox8.Items.Count - 1;
            button58.Enabled = true;
            button51.Enabled = true;
        }

        private void button54_Click(object sender, EventArgs e) // Add Warp
        {
            System.IO.BinaryReader readEvent = new System.IO.BinaryReader(File.OpenRead(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
            File.Create(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new").Close();
            System.IO.BinaryWriter writeEvent = new System.IO.BinaryWriter(File.OpenWrite(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new"));
            for (int i = 0; i < (0xC + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count + 0xc * listBox9.Items.Count); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            for (int i = 0; i < (0xc); i++)
            {
                writeEvent.Write((byte)0x0); // Writes new furniture
            }
            for (int i = 0; i < (readEvent.BaseStream.Length - (0xC + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count + 0xc * listBox9.Items.Count)); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            writeEvent.BaseStream.Position = 0x8 + listBox7.Items.Count * 0x14 + listBox8.Items.Count * 0x20;
            writeEvent.Write(listBox9.Items.Count + 1);
            readEvent.Close();
            writeEvent.Close();
            File.Delete(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            File.Move(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new", eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            int count = listBox9.Items.Count;
            listBox9.Items.Add(rm.GetString("warp") + (count + 1));
            listBox9.SelectedIndex = listBox9.Items.Count - 1;
            button59.Enabled = true;
            button53.Enabled = true;
        }

        private void button56_Click(object sender, EventArgs e) // Add Trigger
        {
            System.IO.BinaryReader readEvent = new System.IO.BinaryReader(File.OpenRead(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
            File.Create(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new").Close();
            System.IO.BinaryWriter writeEvent = new System.IO.BinaryWriter(File.OpenWrite(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new"));
            for (int i = 0; i < (0x10 + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count + 0xc * listBox9.Items.Count + 0x10 * listBox10.Items.Count); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            for (int i = 0; i < (0x10); i++)
            {
                writeEvent.Write((byte)0x0); // Writes new furniture
            }
            for (int i = 0; i < (readEvent.BaseStream.Length - (0x10 + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count + 0xc * listBox9.Items.Count + 0x10 * listBox10.Items.Count)); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            writeEvent.BaseStream.Position = 0xC + listBox7.Items.Count * 0x14 + listBox8.Items.Count * 0x20 + 0xc * listBox9.Items.Count;
            writeEvent.Write(listBox10.Items.Count + 1);
            readEvent.Close();
            writeEvent.Close();
            File.Delete(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            File.Move(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new", eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            int count = listBox10.Items.Count;
            listBox10.Items.Add(rm.GetString("trigger") + (count + 1));
            listBox10.SelectedIndex = listBox10.Items.Count - 1;
            button60.Enabled = true;
            button55.Enabled = true;
        }

        private void button29_Click(object sender, EventArgs e) // Remove Furniture
        {
            System.IO.BinaryReader readEvent = new System.IO.BinaryReader(File.OpenRead(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
            File.Create(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new").Close();
            System.IO.BinaryWriter writeEvent = new System.IO.BinaryWriter(File.OpenWrite(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new"));
            for (int i = 0; i < (0x4 + 0x14 * listBox7.SelectedIndex); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            readEvent.BaseStream.Position += 0x14;
            for (int i = 0; i < (readEvent.BaseStream.Length - (0x4 + 0x14 + 0x14 * listBox7.SelectedIndex)); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            writeEvent.BaseStream.Position = 0x0;
            int count = listBox7.Items.Count - 1;
            writeEvent.Write(count);
            readEvent.Close();
            writeEvent.Close();
            File.Delete(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            File.Move(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new", eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            listBox7.Items.Clear();
            for (int i = 0; i < count; i++)
            {
                listBox7.Items.Add(rm.GetString("furniture") + (i + 1));
            }
            if (listBox7.Items.Count != 0) listBox7.SelectedIndex = 0;
            else
            {
                button29.Enabled = false;
                button57.Enabled = false;
            }
        }

        private void button51_Click(object sender, EventArgs e) // Remove Overworld
        {
            System.IO.BinaryReader readEvent = new System.IO.BinaryReader(File.OpenRead(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
            File.Create(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new").Close();
            System.IO.BinaryWriter writeEvent = new System.IO.BinaryWriter(File.OpenWrite(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new"));
            for (int i = 0; i < (0x4 + 0x14 * listBox7.Items.Count + 0x20 * listBox8.SelectedIndex); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            readEvent.BaseStream.Position += 0x20;
            for (int i = 0; i < (readEvent.BaseStream.Length - (0x4 + 0x20 + 0x14 * listBox7.Items.Count + 0x20 * listBox8.SelectedIndex)); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            writeEvent.BaseStream.Position = 0x4 + listBox7.Items.Count * 0x14;
            int count = listBox8.Items.Count - 1;
            writeEvent.Write(count);
            readEvent.Close();
            writeEvent.Close();
            File.Delete(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            File.Move(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new", eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            listBox8.Items.Clear();
            for (int i = 0; i < count; i++)
            {
                listBox8.Items.Add(rm.GetString("overworld") + (i + 1));
            }
            if (listBox8.Items.Count != 0) listBox8.SelectedIndex = 0;
            else
            {
                button51.Enabled = false;
                button58.Enabled = false;
            }
        }

        private void button53_Click(object sender, EventArgs e) // Remove Warp
        {
            System.IO.BinaryReader readEvent = new System.IO.BinaryReader(File.OpenRead(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
            File.Create(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new").Close();
            System.IO.BinaryWriter writeEvent = new System.IO.BinaryWriter(File.OpenWrite(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new"));
            for (int i = 0; i < (0x8 + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count + 0xc * listBox9.SelectedIndex); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            readEvent.BaseStream.Position += 0xc;
            for (int i = 0; i < (readEvent.BaseStream.Length - (0x8 + 0xc + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count + 0xc * listBox9.SelectedIndex)); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            writeEvent.BaseStream.Position = 0x8 + listBox7.Items.Count * 0x14 + 0x20 * listBox8.Items.Count;
            int count = listBox9.Items.Count - 1;
            writeEvent.Write(count);
            readEvent.Close();
            writeEvent.Close();
            File.Delete(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            File.Move(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new", eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            listBox9.Items.Clear();
            for (int i = 0; i < count; i++)
            {
                listBox9.Items.Add(rm.GetString("warp") + (i + 1));
            }
            if (listBox9.Items.Count != 0) listBox9.SelectedIndex = 0;
            else
            {
                button53.Enabled = false;
                button59.Enabled = false;
            }
        }

        private void button55_Click(object sender, EventArgs e) // Remove Trigger
        {
            System.IO.BinaryReader readEvent = new System.IO.BinaryReader(File.OpenRead(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4")));
            File.Create(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new").Close();
            System.IO.BinaryWriter writeEvent = new System.IO.BinaryWriter(File.OpenWrite(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new"));
            for (int i = 0; i < (0xc + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count + 0xc * listBox9.Items.Count + 0x10 * listBox10.SelectedIndex); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            readEvent.BaseStream.Position += 0x10;
            for (int i = 0; i < (readEvent.BaseStream.Length - (0xc + 0x10 + 0x14 * listBox7.Items.Count + 0x20 * listBox8.Items.Count + 0xc * listBox9.Items.Count + 0x10 * listBox10.SelectedIndex)); i++)
            {
                writeEvent.Write(readEvent.ReadByte()); // Reads unmodified bytes and writes them to the main file
            }
            writeEvent.BaseStream.Position = 0xc + listBox7.Items.Count * 0x14 + 0x20 * listBox8.Items.Count + 0xc * listBox9.Items.Count;
            int count = listBox10.Items.Count - 1;
            writeEvent.Write(count);
            readEvent.Close();
            writeEvent.Close();
            File.Delete(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            File.Move(eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4") + "_new", eventPath + "\\" + comboBox10.SelectedIndex.ToString("D4"));
            listBox10.Items.Clear();
            for (int i = 0; i < count; i++)
            {
                listBox10.Items.Add(rm.GetString("trigger") + (i + 1));
            }
            if (listBox10.Items.Count != 0) listBox10.SelectedIndex = 0;
            else
            {
                button55.Enabled = false;
                button60.Enabled = false;
            }
        }

        #endregion

        #region Script Editor

        private void comboBox5_SelectedIndexChanged(object sender, EventArgs e) // Read Script File
        {
            if (isBW || isB2W2)
            {
                readScriptGenV();
                return;
            }

            #region Paths
            if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4B414441 || gameID == 0x4B415041)
            {
                scriptPath = workingFolder + @"data\fielddata\script\scr_seq_release" + "\\" + comboBox5.SelectedIndex.ToString("D4");
            }
            if (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043 || gameID == 0x4A414441 || gameID == 0x4A415041)
            {
                scriptPath = workingFolder + @"data\fielddata\script\scr_seq" + "\\" + comboBox5.SelectedIndex.ToString("D4");
            }
            if (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049)
            {
                scriptPath = workingFolder + @"data\a\0\1\script\" + "\\" + comboBox5.SelectedIndex.ToString("D4");
            }
            #endregion

            #region RM
            if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
            {
                scriptData = new ResourceManager("WindowsFormsApplication1.Resources.Scripts", Assembly.GetExecutingAssembly());
                scriptName = new ResourceManager("WindowsFormsApplication1.Resources.ScriptNames", Assembly.GetExecutingAssembly());
            }
            else
            {
                scriptData = new ResourceManager("WindowsFormsApplication1.Resources.ScriptsGS", Assembly.GetExecutingAssembly());
                scriptName = new ResourceManager("WindowsFormsApplication1.Resources.ScriptNamesGS", Assembly.GetExecutingAssembly());
            }
            #endregion
            scriptList.Clear();
            functionList.Clear();
            movementList.Clear();
            progressBar2.Value = progressBar2.Minimum;
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            button33.Enabled = false;
            int count = 0;
            scriptOffset.Clear();
            MovementOffset.Clear();
            FunctionOffset.Clear();
            useScriptList.Clear();
            comboBox9.Items.Clear();
            comboBox9.Enabled = false;
           
            System.IO.BinaryReader readScript = new System.IO.BinaryReader(File.OpenRead(scriptPath));
            int flag = (int)readScript.ReadUInt32();
            if (flag == 0)
            {
                textBox2.Text = "Level Scripts File";
                progressBar2.Value = progressBar2.Maximum;
                button33.Enabled = false;
                readScript.Close();
                return;
            }
            if ((flag & 0xFFFF) == 0xFD13)
            {
                textBox2.Text = "No scripts";
                progressBar2.Value = progressBar2.Maximum;
                readScript.Close();
                button33.Enabled = true;
                return;
            }
            while ((flag & 0xFFFF) != 0xFD13)
            {
                scriptOffset.Add(flag + (int)readScript.BaseStream.Position);
                FunctionOffset.Add(new List<int>());
                MovementOffset.Add(new List<int>());
                functionList.Add(new List<MemoryStream>());
                movementList.Add(new List<MemoryStream>());
                count++;
                if (scriptOffset.Contains((int)readScript.BaseStream.Position)) break;
                flag = (int)readScript.ReadUInt32();
                if ((flag & 0xFFFF) == 0)
                {
                    textBox2.Text = "Level Scripts File";
                    progressBar2.Value = progressBar2.Maximum;
                    button33.Enabled = false;
                    readScript.Close();
                    return;
                }
            }
            progressBar2.Maximum = count * 3;
            comboBox9.Enabled = true;
            try
            {
                #region Scripts
                for (int i = 0; i < count; i++)
                {
                    readScript.BaseStream.Position = scriptOffset[i];
                    comboBox9.Items.Add("Script #" + (i + 1).ToString());
                    scriptList.Add(new MemoryStream());
                    BinaryWriter load = new BinaryWriter(scriptList[i]);
                    bool exists = false;
                    if (scriptOffset.IndexOf(scriptOffset[i]) != i)
                    {
                        useScriptList.Add(scriptOffset.IndexOf(scriptOffset[i]));
                        exists = true;
                    }
                    else useScriptList.Add(0xFFFF);
                    while (!exists)
                    {
                        UInt16 currentCmd = readScript.ReadUInt16();
                        load.Write((UInt16)currentCmd);
                        string[] cmd = scriptData.GetString(currentCmd.ToString("X4")).Split(' ');
                        if (cmd[0] != "0")
                        {
                            if (currentCmd == 0x0016 || currentCmd == 0x001A) // Jump GoTo
                            {
                                int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                if (FunctionOffset[i].Contains(offset))
                                {
                                    load.Write((FunctionOffset[i].IndexOf(offset) + 1));
                                }
                                else if (scriptOffset.Contains(offset))
                                {
                                    load.Write((0xFFFF + (scriptOffset.IndexOf(offset) + 1)));
                                }
                                else
                                {
                                    FunctionOffset[i].Add(offset);
                                    load.Write((FunctionOffset[i].Count));
                                    ProcessFunction(offset, i);
                                }
                                if (currentCmd == 0x0016)
                                {
                                    break;
                                }
                            }
                            else if (currentCmd == 0x001C || currentCmd == 0x001D) // CompareLastResult
                            {
                                load.Write((byte)readScript.ReadByte());
                                int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                if (FunctionOffset[i].Contains(offset))
                                {
                                    load.Write((FunctionOffset[i].IndexOf(offset) + 1));
                                }
                                else if (scriptOffset.Contains(offset))
                                {
                                    load.Write((0xFFFF + (scriptOffset.IndexOf(offset) + 1)));
                                }
                                else
                                {
                                    FunctionOffset[i].Add(offset);
                                    load.Write((FunctionOffset[i].Count));
                                    ProcessFunction(offset, i);
                                }
                            }
                            else if (currentCmd == 0x005E)
                            {
                                load.Write((Int16)readScript.ReadInt16());
                                int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                MovementOffset[i].Add(offset);
                                load.Write((MovementOffset[i].Count));
                            }
                            else if (currentCmd == 0x011D && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                                load.Write((Int16)readScript.ReadInt16());
                                load.Write((Int16)readScript.ReadInt16());
                                load.Write((Int16)readScript.ReadInt16());
                            }
                            else if (currentCmd == 0x0190 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                            {
                                byte param = readScript.ReadByte();
                                load.Write((byte)param);
                                if (param == 0x2)
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                }
                            }
                            else if (currentCmd == 0x01CF)
                            {
                                byte param = readScript.ReadByte();
                                load.Write((byte)param);
                                if (param == 0x2)
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                }
                            }
                            else if (currentCmd == 0x01D1 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                            {
                                Int16 param = readScript.ReadInt16();
                                load.Write((Int16)param);
                                if (param == 0x0 || param == 0x1 || param == 0x2 || param == 0x3 || param == 0x4 || param == 0x5 || param == 0x7)
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                    if (param == 0x0 || param == 0x1 || param == 0x2 || param == 0x3)
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                    }
                                }
                            }
                            else if (currentCmd == 0x01E1 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                                load.Write((Int16)readScript.ReadInt16());
                                load.Write((Int16)readScript.ReadInt16());
                                load.Write((Int16)readScript.ReadInt16());
                            }
                            else if (currentCmd == 0x01E9 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                            {
                                Int16 param = readScript.ReadInt16();
                                load.Write((Int16)param);
                                if (param == 0x1 || param == 0x2 || param == 0x3 || param == 0x5 || param == 0x6 || param == 0x7)
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                    if (param == 0x5 || param == 0x6)
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                    }
                                }
                            }
                            else if (currentCmd == 0x021D)
                            {
                                if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                                {
                                    Int16 param = readScript.ReadInt16();
                                    load.Write((Int16)param);
                                    if (param != 0x6)
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                        if (param != 0x5)
                                        {
                                            load.Write((Int16)readScript.ReadInt16());
                                        }
                                    }
                                }
                                else
                                {
                                    byte param = readScript.ReadByte();
                                    load.Write((byte)param);
                                    if (param != 0x6)
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                        if (param != 0x5)
                                        {
                                            load.Write((Int16)readScript.ReadInt16());
                                        }
                                    }
                                }
                            }
                            else if (currentCmd == 0x0235)
                            {
                                Int16 param = readScript.ReadInt16();
                                load.Write((Int16)param);
                                if (param == 0 || param == 1 || param == 3 || param == 4 || param == 6)
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                    if (param == 1 || param == 3 || param == 4)
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                        if (param == 1 || param == 3)
                                        {
                                            load.Write((Int16)readScript.ReadInt16());
                                        }
                                    }
                                }
                            }
                            else if (currentCmd == 0x023E)
                            {
                                if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                                {
                                    Int16 param = readScript.ReadInt16();
                                    load.Write((Int16)param);
                                    if (param == 1 || param == 3 || param == 5 || param == 6)
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                        if (param == 5 || param == 6)
                                        {
                                            load.Write((Int16)readScript.ReadInt16());
                                        }
                                    }
                                }
                                else
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                    load.Write((Int16)readScript.ReadInt16());
                                }
                            }
                            else if (currentCmd == 0x02C4)
                            {
                                byte param = readScript.ReadByte();
                                load.Write((byte)param);
                                if (param == 0 || param == 1)
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                }
                            }
                            else if (currentCmd == 0x02C5 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                                load.Write((Int16)readScript.ReadInt16());
                                load.Write((Int16)readScript.ReadInt16());
                            }
                            else if (currentCmd == 0x02C6 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                            }
                            else if (currentCmd == 0x02C9 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                            }
                            else if (currentCmd == 0x02CA && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                            }
                            else if (currentCmd == 0x02CD && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                            }
                            else if (currentCmd == 0x02CF && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                                load.Write((Int16)readScript.ReadInt16());
                                load.Write((Int16)readScript.ReadInt16());
                            }
                            else
                            {
                                for (int j = 0; j < Convert.ToInt32(cmd[0]); j++)
                                {
                                    if (cmd[j + 1] == "1")
                                    {
                                        load.Write((byte)readScript.ReadByte());
                                    }
                                    if (cmd[j + 1] == "2")
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                    }
                                    if (cmd[j + 1] == "4")
                                    {
                                        load.Write(readScript.ReadInt32());
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (currentCmd == 0x0002 || currentCmd == 0x001B)
                            {
                                break;
                            }
                        }
                    }
                    if (load.BaseStream.Length % 2 == 1) load.Write((byte)0xFF);
                    load.Flush();
                    progressBar2.Value++;
                }
                #endregion
                #region Functions
                for (int j = 0; j < count; j++)
                {
                    for (int i = 0; i < FunctionOffset[j].Count; i++)
                    {
                        readScript.BaseStream.Position = FunctionOffset[j][i];
                        functionList[j].Add(new MemoryStream());
                        BinaryWriter load = new BinaryWriter(functionList[j][i]);
                        while (true)
                        {
                            int currentCmd = readScript.ReadUInt16();
                            load.Write((UInt16)currentCmd);
                            string[] cmd = scriptData.GetString(currentCmd.ToString("X4")).Split(' ');
                            if (cmd[0] != "0")
                            {
                                if (currentCmd == 0x0016 || currentCmd == 0x001A) // Jump GoTo
                                {
                                    int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                    if (FunctionOffset[j].Contains(offset))
                                    {
                                        load.Write((FunctionOffset[j].IndexOf(offset) + 1));
                                    }
                                    else
                                    {
                                        load.Write((0xFFFF + (scriptOffset.IndexOf(offset) + 1)));
                                    }
                                    if (currentCmd == 0x0016)
                                    {
                                        break;
                                    }
                                }
                                else if (currentCmd == 0x001C || currentCmd == 0x001D) // CompareLastResult
                                {
                                    load.Write((byte)readScript.ReadByte());
                                    int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                    if (FunctionOffset[j].Contains(offset))
                                    {
                                        load.Write((FunctionOffset[j].IndexOf(offset) + 1));
                                    }
                                    else
                                    {
                                        load.Write((0xFFFF + (scriptOffset.IndexOf(offset) + 1)));
                                    }
                                }
                                else if (currentCmd == 0x005E)
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                    int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                    MovementOffset[j].Add(offset);
                                    load.Write((MovementOffset[j].Count));
                                }
                                else if (currentCmd == 0x011D && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                    load.Write((Int16)readScript.ReadInt16());
                                    load.Write((Int16)readScript.ReadInt16());
                                }
                                else if (currentCmd == 0x0190 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                                {
                                    int param = readScript.ReadByte();
                                    load.Write((byte)param);
                                    if (param == 0x2)
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                    }
                                }
                                else if (currentCmd == 0x01CF)
                                {
                                    int param = readScript.ReadByte();
                                    load.Write((byte)param);
                                    if (param == 0x2)
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                    }
                                }
                                else if (currentCmd == 0x01D1 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                                {
                                    int param = readScript.ReadInt16();
                                    load.Write((Int16)param);
                                    if (param == 0x0 || param == 0x1 || param == 0x2 || param == 0x3 || param == 0x4 || param == 0x5 || param == 0x7)
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                        if (param == 0x0 || param == 0x1 || param == 0x2 || param == 0x3)
                                        {
                                            load.Write((Int16)readScript.ReadInt16());
                                        }
                                    }
                                }
                                else if (currentCmd == 0x01E1 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                    load.Write((Int16)readScript.ReadInt16());
                                    load.Write((Int16)readScript.ReadInt16());
                                }
                                else if (currentCmd == 0x01E9 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                                {
                                    int param = readScript.ReadInt16();
                                    load.Write((Int16)param);
                                    if (param == 0x1 || param == 0x2 || param == 0x3 || param == 0x5 || param == 0x6 || param == 0x7)
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                        if (param == 0x5 || param == 0x6)
                                        {
                                            load.Write((Int16)readScript.ReadInt16());
                                        }
                                    }
                                }
                                else if (currentCmd == 0x021D)
                                {
                                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                                    {
                                        int param = readScript.ReadInt16();
                                        load.Write((Int16)param);
                                        if (param != 0x6)
                                        {
                                            load.Write((Int16)readScript.ReadInt16());
                                            if (param != 0x5)
                                            {
                                                load.Write((Int16)readScript.ReadInt16());
                                            }
                                        }
                                    }
                                    else
                                    {
                                        int param = readScript.ReadByte();
                                        load.Write((byte)param);
                                        if (param != 0x6)
                                        {
                                            load.Write((Int16)readScript.ReadInt16());
                                            if (param != 0x5)
                                            {
                                                load.Write((Int16)readScript.ReadInt16());
                                            }
                                        }
                                    }
                                }
                                else if (currentCmd == 0x0235)
                                {
                                    int param = readScript.ReadInt16();
                                    load.Write((Int16)param);
                                    if (param == 0 || param == 1 || param == 3 || param == 4 || param == 6)
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                        if (param == 1 || param == 3 || param == 4)
                                        {
                                            load.Write((Int16)readScript.ReadInt16());
                                            if (param == 1 || param == 3)
                                            {
                                                load.Write((Int16)readScript.ReadInt16());
                                            }
                                        }
                                    }
                                }
                                else if (currentCmd == 0x023E)
                                {
                                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                                    {
                                        int param = readScript.ReadInt16();
                                        load.Write((Int16)param);
                                        if (param == 1 || param == 3 || param == 5 || param == 6)
                                        {
                                            load.Write((Int16)readScript.ReadInt16());
                                            if (param == 5 || param == 6)
                                            {
                                                load.Write((Int16)readScript.ReadInt16());
                                            }
                                        }
                                    }
                                    else
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                        load.Write((Int16)readScript.ReadInt16());
                                    }
                                }
                                else if (currentCmd == 0x02C4)
                                {
                                    int param = readScript.ReadByte();
                                    load.Write((byte)param);
                                    if (param == 0 || param == 1)
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                    }
                                }
                                else if (currentCmd == 0x02C5 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                    load.Write((Int16)readScript.ReadInt16());
                                }
                                else if (currentCmd == 0x02C6 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                }
                                else if (currentCmd == 0x02C9 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                }
                                else if (currentCmd == 0x02CA && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                }
                                else if (currentCmd == 0x02CD && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                }
                                else if (currentCmd == 0x02CF && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                    load.Write((Int16)readScript.ReadInt16());
                                }
                                else
                                {
                                    for (int k = 0; k < Convert.ToInt32(cmd[0]); k++)
                                    {
                                        if (cmd[k + 1] == "1")
                                        {
                                            load.Write((byte)readScript.ReadByte());
                                        }
                                        if (cmd[k + 1] == "2")
                                        {
                                            load.Write((Int16)readScript.ReadInt16());
                                        }
                                        if (cmd[k + 1] == "4")
                                        {
                                            load.Write(readScript.ReadInt32());
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (currentCmd == 0x0002 || currentCmd == 0x001B)
                                {
                                    break;
                                }
                            }
                        }
                        if (load.BaseStream.Length % 2 == 1) load.Write((byte)0xFF);
                        load.Flush();
                    }
                    progressBar2.Value++;
                }
                #endregion
                #region Movements
                for (int j = 0; j < count; j++)
                {
                    for (int i = 0; i < MovementOffset[j].Count; i++)
                    {
                        readScript.BaseStream.Position = MovementOffset[j][i];
                        movementList[j].Add(new MemoryStream());
                        BinaryWriter load = new BinaryWriter(movementList[j][i]);
                        while (true)
                        {
                            int currentMove = readScript.ReadUInt16();
                            load.Write((UInt16)currentMove);
                            if (currentMove != 0x00FE)
                            {
                                load.Write((UInt16)readScript.ReadUInt16());
                            }
                            else
                            {
                                break;
                            }
                        }
                        load.Flush();
                    }
                    progressBar2.Value++;
                }
                #endregion
            }
            catch
            {
                textBox2.Text = "Script Error";
                progressBar2.Value = progressBar2.Maximum;
                readScript.Close();
                return;
            }
            progressBar2.Value = progressBar2.Maximum;
            button33.Enabled = true;
            readScript.Close();
            //comboBox9.SelectedIndex = 0;
        }

        private string getOperator(byte op)
        {
            if (op == 0x0) return "LOWER";
            if (op == 0x1) return "EQUAL";
            if (op == 0x2) return "BIGGER";
            if (op == 0x3) return "LOWER/EQUAL";
            if (op == 0x4) return "BIGGER/EQUAL";
            if (op == 0x5) return "DIFFERENT";
            if (op == 0x6) return "OR";
            if (op == 0x7) return "AND";
            if (op == 0xFF) return "TRUEUP";
            return ("0x" + op.ToString("X"));
        }

        private bool checkOpByte(string op)
        {
            byte output8;
            NumberStyles hex = NumberStyles.HexNumber;
            CultureInfo invar = CultureInfo.InvariantCulture;
            if (op == "LOWER") return true;
            else if (op == "EQUAL") return true;
            else if (op == "BIGGER") return true;
            else if (op == "LOWER/EQUAL") return true;
            else if (op == "BIGGER/EQUAL") return true;
            else if (op == "DIFFERENT") return true;
            else if (op == "OR") return true;
            else if (op == "AND") return true;
            else if (op == "TRUEUP") return true;
            else if (byte.TryParse(op.Substring(2), hex, invar, out output8))
            {
                if (output8 >= 0x0 && output8 <= 0xFF) return true;
                else return false;
            }
            else return false;
            
        }

        private byte getOpByte(string op)
        {
            if (op == "LOWER") return 0x0;
            else if (op == "EQUAL") return 0x1;
            else if (op == "BIGGER") return 0x2;
            else if (op == "LOWER/EQUAL") return 0x3;
            else if (op == "BIGGER/EQUAL") return 0x4;
            else if (op == "DIFFERENT") return 0x5;
            else if (op == "OR") return 0x6;
            else if (op == "AND") return 0x7;
            else if (op == "TRUEUP") return 0xFF;
            else return (Convert.ToByte(op.Substring(2), 16));
        }

        private void ProcessFunction(int offset2, int script) // Read function
        {
            System.IO.BinaryReader readFunction = new System.IO.BinaryReader(File.OpenRead(scriptPath));
            readFunction.BaseStream.Position = offset2;
            while (true)
            {
                int currentCmd = readFunction.ReadUInt16();
                string[] cmd = scriptData.GetString(currentCmd.ToString("X4")).Split(' ');
                if (cmd[0] != "0")
                {
                    if (currentCmd == 0x0016 || currentCmd == 0x001A)
                    {
                        int offset = readFunction.ReadInt32() + (int)readFunction.BaseStream.Position;
                        if (!FunctionOffset[script].Contains(offset) && !scriptOffset.Contains(offset))
                        {
                            FunctionOffset[script].Add(offset);
                            ProcessFunction(offset, script);
                        }
                        if (currentCmd == 0x0016)
                        {
                            break;
                        }
                    }
                    else if (currentCmd == 0x001C || currentCmd == 0x001D) // CompareLastResult
                    {
                        readFunction.BaseStream.Position++;
                        int offset = readFunction.ReadInt32() + (int)readFunction.BaseStream.Position;
                        if (!FunctionOffset[script].Contains(offset) && !scriptOffset.Contains(offset))
                        {
                            FunctionOffset[script].Add(offset);
                            ProcessFunction(offset, script);
                        }
                    }
                    else if (currentCmd == 0x005E)
                    {
                        readFunction.BaseStream.Position += 6;
                    }
                    else if (currentCmd == 0x011D && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                    {
                        readFunction.BaseStream.Position += 6;
                    }
                    else if (currentCmd == 0x0190 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                    {
                        int param = readFunction.ReadByte();
                        if (param == 0x2)
                        {
                            readFunction.BaseStream.Position += 2;
                        }
                    }
                    else if (currentCmd == 0x01CF)
                    {
                        int param = readFunction.ReadByte();
                        if (param == 0x2)
                        {
                            readFunction.BaseStream.Position += 2;
                        }
                    }
                    else if (currentCmd == 0x01D1 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                    {
                        int param = readFunction.ReadInt16();
                        if (param == 0x0 || param == 0x1 || param == 0x2 || param == 0x3 || param == 0x4 || param == 0x5 || param == 0x7)
                        {
                            readFunction.BaseStream.Position += 2;
                            if (param == 0x0 || param == 0x1 || param == 0x2 || param == 0x3)
                            {
                                readFunction.BaseStream.Position += 2;
                            }
                        }
                    }
                    else if (currentCmd == 0x01E1 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                    {
                        readFunction.BaseStream.Position += 6;
                    }
                    else if (currentCmd == 0x01E9 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                    {
                        int param = readFunction.ReadInt16();
                        if (param == 0x1 || param == 0x2 || param == 0x3 || param == 0x5 || param == 0x6 || param == 0x7)
                        {
                            readFunction.BaseStream.Position += 2;
                            if (param == 0x5 || param == 0x6)
                            {
                                readFunction.BaseStream.Position += 2;
                            }
                        }
                    }
                    else if (currentCmd == 0x021D)
                    {
                        if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                        {
                            int param = readFunction.ReadInt16();
                            if (param != 0x6)
                            {
                                readFunction.BaseStream.Position += 2;
                                if (param != 0x5)
                                {
                                    readFunction.BaseStream.Position += 2;
                                }
                            }
                        }
                        else
                        {
                            int param = readFunction.ReadByte();
                            if (param != 0x6)
                            {
                                readFunction.BaseStream.Position += 2;
                                if (param != 0x5)
                                {
                                    readFunction.BaseStream.Position += 2;
                                }
                            }
                        }
                    }
                    else if (currentCmd == 0x0235)
                    {
                        int param = readFunction.ReadInt16();
                        if (param == 0 || param == 1 || param == 3 || param == 4 || param == 6)
                        {
                            readFunction.BaseStream.Position += 2;
                            if (param == 1 || param == 3 || param == 4)
                            {
                                readFunction.BaseStream.Position += 2;
                                if (param == 1 || param == 3)
                                {
                                    readFunction.BaseStream.Position += 2;
                                }
                            }
                        }
                    }
                    else if (currentCmd == 0x023E)
                    {
                        if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                        {
                            int param = readFunction.ReadInt16();
                            if (param == 1 || param == 3 || param == 5 || param == 6)
                            {
                                readFunction.BaseStream.Position += 2;
                                if (param == 5 || param == 6)
                                {
                                    readFunction.BaseStream.Position += 2;
                                }
                            }

                        }
                        else
                        {
                            readFunction.BaseStream.Position += 4;
                        }
                    }
                    else if (currentCmd == 0x02C4)
                    {
                        int param = readFunction.ReadByte();
                        if (param == 0 || param == 1)
                        {
                            readFunction.BaseStream.Position += 2;
                        }
                    }
                    else if (currentCmd == 0x02C5 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                    {
                        readFunction.BaseStream.Position += 4;
                    }
                    else if (currentCmd == 0x02C6 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                    {
                    }
                    else if (currentCmd == 0x02C9 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                    {
                    }
                    else if (currentCmd == 0x02CA && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                    {
                    }
                    else if (currentCmd == 0x02CD && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                    {
                    }
                    else if (currentCmd == 0x02CF && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                    {
                        readFunction.BaseStream.Position += 4;
                    }
                    else
                    {
                        for (int j = 0; j < Convert.ToInt32(cmd[0]); j++)
                        {
                            if (cmd[j + 1] == "1")
                            {
                                readFunction.BaseStream.Position++;
                            }
                            if (cmd[j + 1] == "2")
                            {
                                readFunction.BaseStream.Position += 2;
                            }
                            if (cmd[j + 1] == "4")
                            {
                                readFunction.BaseStream.Position += 4;
                            }
                        }
                    }
                }
                else
                {
                    if (currentCmd == 0x0002 || currentCmd == 0x001B)
                    {
                        break;
                    }
                }
            }
            readFunction.Close();
            return;
        }

        private void searchScripts ()
        {
            searchTerm = Microsoft.VisualBasic.Interaction.InputBox("Input a string to search for.", "Input Search Term");
            List<string> output = new List<string>();
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                for (int count = 0; count < comboBox5.Items.Count; count++)
                {
                    comboBox5.SelectedIndex = count;

                    for (int count2 = 0; count2 < comboBox9.Items.Count; count2++)
                    {
                        comboBox9.SelectedIndex = count2;

                        foreach (Tuple<string, string, string> tuple in scriptsToSearch)
                        {
                            if (tuple.Item1.Contains(searchTerm))
                            {
                                output.Add($"Found term in Script File {count}, Script #{count2+1}.");
                            }

                            if (tuple.Item2.Contains(searchTerm))
                            {
                                output.Add($"Found term in Script File {count}, Function #{count2+1}.");
                            }

                            if (tuple.Item3.Contains(searchTerm))
                            {
                                output.Add($"Found term in Script File {count}, Movement #{count2+1}.");
                            }
                            
                        }
                        scriptsToSearch.Clear();
                    }
                }
                textBox7.Text = string.Join(Environment.NewLine, output);
                tabControl1.SelectedTab = tabPage11;
                tabControl4.SelectedTab = tabPage25;
            }
            else  
            {
                MessageBox.Show("Can't search for nothing.");
            }
            
            searchTerm = "";
        }

        private void comboBox9_SelectedIndexChanged(object sender, EventArgs e) // Select Script
        {
            string scripts = "";
            string functions = "";
            string movements = "";
            int current = comboBox9.SelectedIndex;
            if (!isBW && !isB2W2)
            {
                #region Scripts
                bool exists = false;
                if (useScriptList[current] != 0xFFFF)
                {
                    scripts += "UseScript_#" + (useScriptList[current] + 1).ToString();
                    scripts += "\r\n\r\n";
                    exists = true;
                }
                BinaryReader readScript = new BinaryReader(scriptList[current]);
                readScript.BaseStream.Position = 0;
                while (!exists)
                {
                    int currentCmd = readScript.ReadUInt16();
                    if (scriptName.GetString(currentCmd.ToString("X4")) != null)
                    {
                        scripts += scriptName.GetString(currentCmd.ToString("X4"));
                    }
                    else
                    {
                        scripts += currentCmd.ToString("X4");
                    }
                    string[] cmd = scriptData.GetString(currentCmd.ToString("X4")).Split(' ');
                    if (cmd[0] != "0")
                    {
                        if (currentCmd == 0x0016 || currentCmd == 0x001A) // Jump GoTo
                        {
                            int offset = readScript.ReadInt32();
                            if (offset < 0xFFFF)
                            {
                                scripts += " Function_#" + offset;
                            }
                            else
                            {
                                scripts += " Script_#" + (offset - 0xFFFF);
                            }
                            if (currentCmd == 0x0016)
                            {
                                scripts += "\r\n";
                                break;
                            }
                        }
                        else if (currentCmd == 0x001C || currentCmd == 0x001D) // CompareLastResult
                        {
                            scripts += " " + getOperator(readScript.ReadByte());
                            int offset = readScript.ReadInt32();
                            if (offset < 0xFFFF)
                            {
                                scripts += " Function_#" + offset;
                            }
                            else
                            {
                                scripts += " Script_#" + (offset - 0xFFFF);
                            }
                        }
                        else if (currentCmd == 0x005E)
                        {
                            scripts += " 0x" + readScript.ReadInt16().ToString("X");
                            int offset = readScript.ReadInt32();
                            scripts += " Movement_#" + offset;
                        }
                        else if (currentCmd == 0x011D && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                        {
                            scripts += " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X");
                        }
                        else if (currentCmd == 0x0190 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                        {
                            int param = readScript.ReadByte();
                            scripts += " 0x" + param.ToString("X");
                            if (param == 0x2)
                            {
                                scripts += " 0x" + readScript.ReadInt16().ToString("X");
                            }
                        }
                        else if (currentCmd == 0x01CF)
                        {
                            int param = readScript.ReadByte();
                            scripts += " 0x" + param.ToString("X");
                            if (param == 0x2)
                            {
                                scripts += " 0x" + readScript.ReadInt16().ToString("X");
                            }
                        }
                        else if (currentCmd == 0x01D1 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                        {
                            int param = readScript.ReadInt16();
                            scripts += " 0x" + param.ToString("X");
                            if (param == 0x0 || param == 0x1 || param == 0x2 || param == 0x3 || param == 0x4 || param == 0x5 || param == 0x7)
                            {
                                scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                if (param == 0x0 || param == 0x1 || param == 0x2 || param == 0x3)
                                {
                                    scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                }
                            }
                        }
                        else if (currentCmd == 0x01E1 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                        {
                            scripts += " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X");
                        }
                        else if (currentCmd == 0x01E9 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                        {
                            int param = readScript.ReadInt16();
                            scripts += " 0x" + param.ToString("X");
                            if (param == 0x1 || param == 0x2 || param == 0x3 || param == 0x5 || param == 0x6 || param == 0x7)
                            {
                                scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                if (param == 0x5 || param == 0x6)
                                {
                                    scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                }
                            }
                        }
                        else if (currentCmd == 0x021D)
                        {
                            if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                            {
                                int param = readScript.ReadInt16();
                                scripts += " 0x" + param.ToString("X");
                                if (param != 0x6)
                                {
                                    scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                    if (param != 0x5)
                                    {
                                        scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                    }
                                }
                            }
                            else
                            {
                                int param = readScript.ReadByte();
                                scripts += " 0x" + param.ToString("X");
                                if (param != 0x6)
                                {
                                    scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                    if (param != 0x5)
                                    {
                                        scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                    }
                                }
                            }
                        }
                        else if (currentCmd == 0x0235)
                        {
                            int param = readScript.ReadInt16();
                            scripts += " 0x" + param.ToString("X");
                            if (param == 0 || param == 1 || param == 3 || param == 4 || param == 6)
                            {
                                scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                if (param == 1 || param == 3 || param == 4)
                                {
                                    scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                    if (param == 1 || param == 3)
                                    {
                                        scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                    }
                                }
                            }
                        }
                        else if (currentCmd == 0x023E)
                        {
                            if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                            {
                                int param = readScript.ReadInt16();
                                scripts += " 0x" + param.ToString("X");
                                if (param == 1 || param == 3 || param == 5 || param == 6)
                                {
                                    scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                    if (param == 5 || param == 6)
                                    {
                                        scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                    }
                                }
                            }
                            else
                            {
                                scripts += " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X");
                            }
                        }
                        else if (currentCmd == 0x02C4)
                        {
                            int param = readScript.ReadByte();
                            scripts += " 0x" + param.ToString("X");
                            if (param == 0 || param == 1)
                            {
                                scripts += " 0x" + readScript.ReadInt16().ToString("X");
                            }
                        }
                        else if (currentCmd == 0x02C5 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                        {
                            scripts += " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X");
                        }
                        else if (currentCmd == 0x02C6 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                        {
                        }
                        else if (currentCmd == 0x02C9 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                        {
                        }
                        else if (currentCmd == 0x02CA && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                        {
                        }
                        else if (currentCmd == 0x02CD && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                        {
                        }
                        else if (currentCmd == 0x02CF && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                        {
                            scripts += " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X");
                        }
                        else
                        {
                            for (int j = 0; j < Convert.ToInt32(cmd[0]); j++)
                            {
                                if (cmd[j + 1] == "1")
                                {
                                    scripts += " 0x" + readScript.ReadByte().ToString("X");
                                }
                                if (cmd[j + 1] == "2")
                                {
                                    scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                }
                                if (cmd[j + 1] == "4")
                                {
                                    scripts += " 0x" + readScript.ReadInt32().ToString("X");
                                }
                            }
                        }
                        scripts += "\r\n";
                    }
                    else
                    {
                        if (currentCmd == 0x0002 || currentCmd == 0x001B)
                        {
                            scripts += "\r\n";
                            break;
                        }
                        else
                        {
                            scripts += "\r\n";
                        }
                    }
                }
                readScript.BaseStream.Flush();
                #endregion
                #region Functions
                for (int i = 0; i < FunctionOffset[current].Count; i++)
                {
                    readScript = new BinaryReader(functionList[current][i]);
                    readScript.BaseStream.Position = 0;
                    functions += "Function #" + (i + 1).ToString();
                    functions += "\r\n\r\n";
                    while (true)
                    {
                        int currentCmd = readScript.ReadUInt16();
                        if (scriptName.GetString(currentCmd.ToString("X4")) != null)
                        {
                            functions += scriptName.GetString(currentCmd.ToString("X4"));
                        }
                        else
                        {
                            functions += currentCmd.ToString("X4");
                        }
                        string[] cmd = scriptData.GetString(currentCmd.ToString("X4")).Split(' ');
                        if (cmd[0] != "0")
                        {
                            if (currentCmd == 0x0016 || currentCmd == 0x001A) // Jump GoTo
                            {
                                int offset = readScript.ReadInt32();
                                if (offset < 0xFFFF)
                                {
                                    functions += " Function_#" + offset;
                                }
                                else
                                {
                                    functions += " Script_#" + (offset - 0xFFFF);
                                }
                                if (currentCmd == 0x0016)
                                {
                                    functions += "\r\n";
                                    break;
                                }
                            }
                            else if (currentCmd == 0x001C || currentCmd == 0x001D) // CompareLastResult
                            {
                                functions += " " + getOperator(readScript.ReadByte());
                                int offset = readScript.ReadInt32();
                                if (offset < 0xFFFF)
                                {
                                    functions += " Function_#" + offset;
                                }
                                else
                                {
                                    functions += " Script_#" + (offset - 0xFFFF);
                                }
                            }
                            else if (currentCmd == 0x005E)
                            {
                                functions += " 0x" + readScript.ReadInt16().ToString("X");
                                int offset = readScript.ReadInt32();
                                functions += " Movement_#" + offset;
                            }
                            else if (currentCmd == 0x011D && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                                functions += " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X");
                            }
                            else if (currentCmd == 0x0190 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                            {
                                int param = readScript.ReadByte();
                                functions += " 0x" + param.ToString("X");
                                if (param == 0x2)
                                {
                                    functions += " 0x" + readScript.ReadInt16().ToString("X");
                                }
                            }
                            else if (currentCmd == 0x01CF)
                            {
                                int param = readScript.ReadByte();
                                functions += " 0x" + param.ToString("X");
                                if (param == 0x2)
                                {
                                    functions += " 0x" + readScript.ReadInt16().ToString("X");
                                }
                            }
                            else if (currentCmd == 0x01D1 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                            {
                                int param = readScript.ReadInt16();
                                functions += " 0x" + param.ToString("X");
                                if (param == 0x0 || param == 0x1 || param == 0x2 || param == 0x3 || param == 0x4 || param == 0x5 || param == 0x7)
                                {
                                    functions += " 0x" + readScript.ReadInt16().ToString("X");
                                    if (param == 0x0 || param == 0x1 || param == 0x2 || param == 0x3)
                                    {
                                        functions += " 0x" + readScript.ReadInt16().ToString("X");
                                    }
                                }
                            }
                            else if (currentCmd == 0x01E1 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                                functions += " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X");
                            }
                            else if (currentCmd == 0x01E9 && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                            {
                                int param = readScript.ReadInt16();
                                functions += " 0x" + param.ToString("X");
                                if (param == 0x1 || param == 0x2 || param == 0x3 || param == 0x5 || param == 0x6 || param == 0x7)
                                {
                                    functions += " 0x" + readScript.ReadInt16().ToString("X");
                                    if (param == 0x5 || param == 0x6)
                                    {
                                        functions += " 0x" + readScript.ReadInt16().ToString("X");
                                    }
                                }
                            }
                            else if (currentCmd == 0x021D)
                            {
                                if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                                {
                                    int param = readScript.ReadInt16();
                                    functions += " 0x" + param.ToString("X");
                                    if (param != 0x6)
                                    {
                                        functions += " 0x" + readScript.ReadInt16().ToString("X");
                                        if (param != 0x5)
                                        {
                                            functions += " 0x" + readScript.ReadInt16().ToString("X");
                                        }
                                    }
                                }
                                else
                                {
                                    int param = readScript.ReadByte();
                                    functions += " 0x" + param.ToString("X");
                                    if (param != 0x6)
                                    {
                                        functions += " 0x" + readScript.ReadInt16().ToString("X");
                                        if (param != 0x5)
                                        {
                                            functions += " 0x" + readScript.ReadInt16().ToString("X");
                                        }
                                    }
                                }
                            }
                            else if (currentCmd == 0x0235)
                            {
                                int param = readScript.ReadInt16();
                                functions += " 0x" + param.ToString("X");
                                if (param == 0 || param == 1 || param == 3 || param == 4 || param == 6)
                                {
                                    functions += " 0x" + readScript.ReadInt16().ToString("X");
                                    if (param == 1 || param == 3 || param == 4)
                                    {
                                        functions += " 0x" + readScript.ReadInt16().ToString("X");
                                        if (param == 1 || param == 3)
                                        {
                                            functions += " 0x" + readScript.ReadInt16().ToString("X");
                                        }
                                    }
                                }
                            }
                            else if (currentCmd == 0x023E)
                            {
                                if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                                {
                                    int param = readScript.ReadInt16();
                                    functions += " 0x" + param.ToString("X");
                                    if (param == 1 || param == 3 || param == 5 || param == 6)
                                    {
                                        functions += " 0x" + readScript.ReadInt16().ToString("X");
                                        if (param == 5 || param == 6)
                                        {
                                            functions += " 0x" + readScript.ReadInt16().ToString("X");
                                        }
                                    }
                                }
                                else
                                {
                                    functions += " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X");
                                }
                            }
                            else if (currentCmd == 0x02C4)
                            {
                                int param = readScript.ReadByte();
                                functions += " 0x" + param.ToString("X");
                                if (param == 0 || param == 1)
                                {
                                    functions += " 0x" + readScript.ReadInt16().ToString("X");
                                }
                            }
                            else if (currentCmd == 0x02C5 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                                functions += " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X");
                            }
                            else if (currentCmd == 0x02C6 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                            }
                            else if (currentCmd == 0x02C9 && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                            }
                            else if (currentCmd == 0x02CA && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                            }
                            else if (currentCmd == 0x02CD && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                            }
                            else if (currentCmd == 0x02CF && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                                functions += " 0x" + readScript.ReadInt16().ToString("X") + " 0x" + readScript.ReadInt16().ToString("X");
                            }
                            else
                            {
                                for (int k = 0; k < Convert.ToInt32(cmd[0]); k++)
                                {
                                    if (cmd[k + 1] == "1")
                                    {
                                        functions += " 0x" + readScript.ReadByte().ToString("X");
                                    }
                                    if (cmd[k + 1] == "2")
                                    {
                                        functions += " 0x" + readScript.ReadInt16().ToString("X");
                                    }
                                    if (cmd[k + 1] == "4")
                                    {
                                        functions += " 0x" + readScript.ReadInt32().ToString("X");
                                    }
                                }
                            }
                            functions += "\r\n";
                        }
                        else
                        {
                            if (currentCmd == 0x0002 || currentCmd == 0x001B)
                            {
                                functions += "\r\n";
                                break;
                            }
                            else
                            {
                                functions += "\r\n";
                            }
                        }
                    }
                    readScript.BaseStream.Flush();
                    functions += "\r\n\r\n";
                }
                #endregion
                #region Movements
                for (int i = 0; i < MovementOffset[current].Count; i++)
                {
                    readScript = new BinaryReader(movementList[current][i]);
                    readScript.BaseStream.Position = 0;
                    movements += "Movement #" + (i + 1).ToString();
                    movements += "\r\n\r\n";
                    while (true)
                    {
                        int currentMove = readScript.ReadUInt16();
                        if (currentMove != 0x00FE)
                        {
                            if (MovementName.GetString(currentMove.ToString("X4")) != null)
                            {
                                movements += MovementName.GetString(currentMove.ToString("X4")) + " 0x" + readScript.ReadInt16().ToString("X");
                                movements += "\r\n";
                            }
                            else
                            {
                                movements += currentMove.ToString("X4") + " 0x" + readScript.ReadInt16().ToString("X");
                                movements += "\r\n";
                            }
                        }
                        else
                        {
                            movements += "End";
                            break;
                        }
                    }
                    readScript.BaseStream.Flush();
                    movements += "\r\n\r\n\r\n";
                }
                #endregion
            }
            else
            {
                #region Scripts
                bool exists = false;
                if (useScriptList[current] != 0xFFFF)
                {
                    scripts += "UseScript_#" + (useScriptList[current] + 1).ToString();
                    scripts += "\r\n\r\n";
                    exists = true;
                }
                BinaryReader readScript = new BinaryReader(scriptList[current]);
                readScript.BaseStream.Position = 0;
                while (!exists)
                {
                    int currentCmd = readScript.ReadUInt16();
                    if (scriptName.GetString(currentCmd.ToString("X4")) != null)
                    {
                        scripts += scriptName.GetString(currentCmd.ToString("X4"));
                    }
                    else
                    {
                        scripts += currentCmd.ToString("X4");
                    }
                    string[] cmd = scriptData.GetString(currentCmd.ToString("X4")).Split(' ');
                    if (cmd[0] != "0")
                    {
                        if (currentCmd == 0x001E || currentCmd == 0x0004) // Jump Call
                        {
                            int offset = readScript.ReadInt32();
                            if (offset < 0xFFFF)
                            {
                                scripts += " Function_#" + offset;
                            }
                            else
                            {
                                scripts += " Script_#" + (offset - 0xFFFF);
                            }
                            if (currentCmd == 0x001E)
                            {
                                scripts += "\r\n";
                                break;
                            }
                        }
                        else if (currentCmd == 0x001F) // If
                        {
                            scripts += " 0x" + readScript.ReadByte().ToString("X");
                            int offset = readScript.ReadInt32();
                            if (offset < 0xFFFF)
                            {
                                scripts += " Function_#" + offset;
                            }
                            else
                            {
                                scripts += " Script_#" + (offset - 0xFFFF);
                            }
                        }
                        else if (currentCmd == 0x0064 && gameID != 0x4A425249 && gameID != 0x4A415249 && gameID != 0x4A455249 && gameID != 0x4A445249)
                        {
                            scripts += " 0x" + readScript.ReadInt16().ToString("X");
                            int offset = readScript.ReadInt32();
                            scripts += " Movement_#" + offset;
                        }
                        else if (currentCmd == 0x0060 && (gameID == 0x4A425249 || gameID == 0x4A415249 || gameID == 0x4A455249 || gameID == 0x4A445249))
                        {
                            scripts += " 0x" + readScript.ReadInt16().ToString("X");
                            int offset = readScript.ReadInt32();
                            scripts += " Movement_#" + offset;
                        }
                        else
                        {
                            for (int j = 0; j < Convert.ToInt32(cmd[0]); j++)
                            {
                                if (cmd[j + 1] == "1")
                                {
                                    scripts += " 0x" + readScript.ReadByte().ToString("X");
                                }
                                if (cmd[j + 1] == "2")
                                {
                                    scripts += " 0x" + readScript.ReadInt16().ToString("X");
                                }
                                if (cmd[j + 1] == "4")
                                {
                                    scripts += " 0x" + readScript.ReadInt32().ToString("X");
                                }
                            }
                        }
                        scripts += "\r\n";
                    }
                    else
                    {
                        if (currentCmd == 0x0002 || currentCmd == 0x0005 || currentCmd == 0x001D)
                        {
                            scripts += "\r\n";
                            break;
                        }
                        else if (currentCmd == 0x0003)
                        {
                            scripts += " 0x" + readScript.ReadInt16().ToString("X");
                            scripts += "\r\n";
                            break;
                        }
                        else
                        {
                            scripts += "\r\n";
                        }
                    }
                }
                readScript.BaseStream.Flush();
                #endregion
                #region Functions
                for (int i = 0; i < FunctionOffset[current].Count; i++)
                {
                    readScript = new BinaryReader(functionList[current][i]);
                    readScript.BaseStream.Position = 0;
                    functions += "Function #" + (i + 1).ToString();
                    functions += "\r\n\r\n";
                    while (true)
                    {
                        int currentCmd = readScript.ReadUInt16();
                        if (scriptName.GetString(currentCmd.ToString("X4")) != null)
                        {
                            functions += scriptName.GetString(currentCmd.ToString("X4"));
                        }
                        else
                        {
                            functions += currentCmd.ToString("X4");
                        }
                        Debug.Print(currentCmd.ToString("X4"));
                        string[] cmd = scriptData.GetString(currentCmd.ToString("X4")).Split(' ');
                        if (cmd[0] != "0")
                        {
                            if (currentCmd == 0x001E || currentCmd == 0x0004) // Jump Call
                            {
                                int offset = readScript.ReadInt32();
                                if (offset < 0xFFFF)
                                {
                                    functions += " Function_#" + offset;
                                }
                                else
                                {
                                    functions += " Script_#" + (offset - 0xFFFF);
                                }
                                if (currentCmd == 0x001E)
                                {
                                    functions += "\r\n";
                                    break;
                                }
                            }
                            else if (currentCmd == 0x001F) // If
                            {
                                functions += " 0x" + readScript.ReadByte().ToString("X");
                                int offset = readScript.ReadInt32();
                                if (offset < 0xFFFF)
                                {
                                    functions += " Function_#" + offset;
                                }
                                else
                                {
                                    functions += " Script_#" + (offset - 0xFFFF);
                                }
                            }
                            else if (currentCmd == 0x0064 && gameID != 0x4A425249 && gameID != 0x4A415249 && gameID != 0x4A455249 && gameID != 0x4A445249)
                            {
                                functions += " 0x" + readScript.ReadInt16().ToString("X");
                                int offset = readScript.ReadInt32();
                                functions += " Movement_#" + offset;
                            }
                            else if (currentCmd == 0x0060 && (gameID == 0x4A425249 || gameID == 0x4A415249 || gameID == 0x4A455249 || gameID == 0x4A445249))
                            {
                                functions += " 0x" + readScript.ReadInt16().ToString("X");
                                int offset = readScript.ReadInt32();
                                functions += " Movement_#" + offset;
                            }
                            else
                            {
                                for (int k = 0; k < Convert.ToInt32(cmd[0]); k++)
                                {
                                    if (cmd[k + 1] == "1")
                                    {
                                        functions += " 0x" + readScript.ReadByte().ToString("X");
                                    }
                                    if (cmd[k + 1] == "2")
                                    {
                                        functions += " 0x" + readScript.ReadInt16().ToString("X");
                                    }
                                    if (cmd[k + 1] == "4")
                                    {
                                        functions += " 0x" + readScript.ReadInt32().ToString("X");
                                    }
                                }
                            }
                            functions += "\r\n";
                        }
                        else
                        {
                            if (currentCmd == 0x0002 || currentCmd == 0x0005 || currentCmd == 0x001D)
                            {
                                functions += "\r\n";
                                break;
                            }
                            else if (currentCmd == 0x0003)
                            {
                                functions += " 0x" + readScript.ReadInt16().ToString("X");
                                functions += "\r\n";
                                break;
                            }
                            else
                            {
                                functions += "\r\n";
                            }
                        }
                    }
                    readScript.BaseStream.Flush();
                    functions += "\r\n\r\n";
                }
                #endregion
                #region Movements
                for (int i = 0; i < MovementOffset[current].Count; i++)
                {
                    readScript = new BinaryReader(movementList[current][i]);
                    readScript.BaseStream.Position = 0;
                    movements += "Movement #" + (i + 1).ToString();
                    movements += "\r\n\r\n";
                    while (true)
                    {
                        int currentMove = readScript.ReadUInt16();
                        if (currentMove != 0x00FE)
                        {
                            if (MovementName.GetString(currentMove.ToString("X4")) != null)
                            {
                                movements += MovementName.GetString(currentMove.ToString("X4")) + " 0x" + readScript.ReadInt16().ToString("X");
                                movements += "\r\n";
                            }
                            else
                            {
                                movements += currentMove.ToString("X4") + " 0x" + readScript.ReadInt16().ToString("X");
                                movements += "\r\n";
                            }
                        }
                        else
                        {
                            movements += "End";
                            break;
                        }
                    }
                    readScript.BaseStream.Flush();
                    movements += "\r\n\r\n\r\n";
                }
                #endregion
            }
            textBox2.Text = scripts;
            textBox3.Text = functions;
            textBox4.Text = movements;

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                scriptsToSearch.Add(new Tuple<string, string, string>(scripts, functions, movements));
            }
        }

        private void button33_Click(object sender, EventArgs e) // Save Script File
        {
            useIndex.Clear();
            if (isBW || isB2W2)
            {
                saveScriptGenV();
                return;
            }
            #region RM
            if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
            {
                scriptNameW = new ResourceManager("WindowsFormsApplication1.Resources.ScriptNamesW", Assembly.GetExecutingAssembly());
            }
            else
            {
                scriptNameW = new ResourceManager("WindowsFormsApplication1.Resources.ScriptNamesWGS", Assembly.GetExecutingAssembly());
            }
            #endregion
            int scriptLines = 0;
            List<int> functionLines = new List<int>();
            List<int> movementLines = new List<int>();
            List<int> scriptOffsets = new List<int>();
            List<int> functionOffsets = new List<int>();
            List<int> movementOffsets = new List<int>();
            int scriptCount = 1;
            int functionCount = 0;
            int movementCount = 0;
            //for (int i = 0; i < textBox2.Lines.Count(); i++)
            //{
            //    try
            //    {
            //        if (textBox2.Lines[i].Substring(0, 8) == "Script #")
            //        {
            //            scriptCount++;
            //            scriptLines = i;
            //            break;
            //        }
            //    }
            //    catch { }
            //}
            functionList[comboBox9.SelectedIndex].Clear();
            functionList[comboBox9.SelectedIndex] = new List<MemoryStream>();
            for (int i = 0; i < textBox3.Lines.Count(); i++)
            {
                try
                {
                    if (textBox3.Lines[i].Substring(0, 10) == "Function #")
                    {
                        functionCount++;
                        functionLines.Add(i);
                    }
                }
                catch { }
            }
            movementList[comboBox9.SelectedIndex].Clear();
            movementList[comboBox9.SelectedIndex] = new List<MemoryStream>();
            for (int i = 0; i < textBox4.Lines.Count(); i++)
            {
                try
                {
                    if (textBox4.Lines[i].Substring(0, 10) == "Movement #")
                    {
                        movementCount++;
                        movementLines.Add(i);
                    }
                }
                catch { }
            }
            if (scriptCount == 0)
            {
                //System.IO.BinaryWriter writeZero = new System.IO.BinaryWriter(File.Create(scriptPath));
                //writeZero.Write((int)0xFD13);
                //writeZero.Close();
                //textBox2.Text = "End";
                //scriptLines = 0;
            }
            progressBar2.Maximum = scriptCount + functionCount + movementCount;
            progressBar2.Value = 0;
            int baseOffset = 2 + (scriptCount * 4);
            //scriptOffsets.Add(0);
            //functionOffsets.Add(0);
            //movementOffsets.Add(0);
            //for (int i = 0; i < scriptCount; i++)
            //{
            //    scriptOffsets.Add(baseOffset);
            //    baseOffset += (int)scriptList[i].Length;
            //}
            //for (int i = 0; i < functionCount; i++)
            //{
            //    functionOffsets.Add(baseOffset);
            //    baseOffset += (int)functionList[comboBox9.SelectedIndex][i].Length;
            //}
            //for (int i = 0; i < movementCount; i++)
            //{
            //    movementOffsets.Add(baseOffset);
            //    baseOffset += (int)movementList[comboBox9.SelectedIndex][i].Length;
            //}
            //System.IO.BinaryWriter write = new System.IO.BinaryWriter(File.Create(scriptPath));
            //for (int i = 1; i < scriptCount + 1; i++)
            //{
            //    write.Write((Int32)(scriptOffsets[i] - (write.BaseStream.Position + 4)));
            //}
            //write.Write((UInt16)0xFD13);
            #region Write Scripts
            int currentIndex = 0;
            for (int i = 0; i < scriptCount; i++)
            {
                scriptList[comboBox9.SelectedIndex] = new MemoryStream();
                BinaryWriter write = new BinaryWriter(scriptList[comboBox9.SelectedIndex]);
                currentLine = scriptLines;
                while (true)
                {
                    try
                    {
                        string[] cmd = textBox2.Lines[currentLine].Split(' ');
                        //if ((int)scriptList[i].Length == 0)
                        //{
                        //    long position = write.BaseStream.Position;
                        //    write.BaseStream.Position = 4 * i;
                        //    write.Write((Int32)(scriptOffsets[useIndex[currentIndex]] - (write.BaseStream.Position + 4)));
                        //    write.BaseStream.Position = position;
                        //    currentIndex++;
                        //    break;
                        //}
                        if (scriptData.GetString(cmd[0]) == null && scriptNameW.GetString(cmd[0]) == null)
                        {
                        }
                        else
                        {
                            byte output8;
                            UInt16 output16;
                            UInt32 output32;
                            NumberStyles hex = NumberStyles.HexNumber;
                            CultureInfo invar = CultureInfo.InvariantCulture;
                            if (scriptData.GetString(cmd[0]) == null)
                            {
                                cmd[0] = scriptNameW.GetString(cmd[0]);
                            }
                            write.Write((UInt16)Convert.ToUInt16(cmd[0], 16));
                            if (scriptData.GetString(cmd[0]) != "0")
                            {
                                if (cmd[0] == "0016" || cmd[0] == "001A") // Jump GoTo
                                {
                                    try
                                    {
                                        if (cmd.Count() >= 2 && cmd[1].Substring(0, 8) == "Script_#" && UInt32.TryParse(cmd[1].Substring(8), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[1].Substring(8)) < comboBox9.Items.Count + 1 && Convert.ToUInt32(cmd[1].Substring(8)) > 0)
                                        {
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToUInt32(0xFFFF + Convert.ToInt32(cmd[1].Substring(8))));
                                            if (cmd[0] == "0016")
                                            {
                                                currentLine++;
                                                break;
                                            }
                                        }
                                        else if (cmd.Count() >= 2 && cmd[1].Substring(0, 10) == "Function_#" && UInt32.TryParse(cmd[1].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[1].Substring(10)) < functionCount + 1 && Convert.ToUInt32(cmd[1].Substring(10)) > 0)
                                        {
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToUInt32(cmd[1].Substring(10)));
                                            if (cmd[0] == "0016")
                                            {
                                                currentLine++;
                                                break;
                                            }
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    catch
                                    {
                                        write.BaseStream.Position -= 2;
                                    }
                                }
                                else if (cmd[0] == "001C" || cmd[0] == "001D") // CompareLastResult
                                {
                                    try
                                    {
                                        if (cmd.Count() >= 3 && checkOpByte(cmd[1]) == true && cmd[2].Substring(0, 8) == "Script_#" && UInt32.TryParse(cmd[2].Substring(8), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(8)) < comboBox9.Items.Count + 1 && Convert.ToUInt32(cmd[2].Substring(8)) > 0)
                                        {
                                            write.Write(Convert.ToByte(getOpByte(cmd[1])));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToUInt32(0xFFFF + Convert.ToInt32(cmd[2].Substring(8))));
                                        }
                                        else if (cmd.Count() >= 3 && checkOpByte(cmd[1]) == true && cmd[2].Substring(0, 10) == "Function_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) < functionCount + 1 && Convert.ToUInt32(cmd[2].Substring(10)) > 0)
                                        {
                                            write.Write(Convert.ToByte(getOpByte(cmd[1])));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToUInt32(cmd[2].Substring(10)));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    catch
                                    {
                                        write.BaseStream.Position -= 2;
                                    }
                                }
                                else if (cmd[0] == "005E")
                                {
                                    try
                                    {
                                        if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && cmd[2].Substring(0, 10) == "Movement_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) <= movementCount && Convert.ToUInt32(cmd[2].Substring(10)) != 0)
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToUInt32(cmd[2].Substring(10)));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    catch
                                    {
                                        write.BaseStream.Position -= 2;
                                    }
                                }
                                else if (cmd[0] == "011D" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                    if (cmd.Count() >= 4 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "0190" && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                                {
                                    if (cmd.Count() >= 3 && Convert.ToByte(cmd[1], 16) == 0x2 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((byte)Convert.ToByte(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else if (cmd.Count() >= 2 && Convert.ToByte(cmd[1], 16) != 0x2 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8))
                                    {
                                        write.Write((byte)Convert.ToByte(cmd[1], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "01CF")
                                {
                                    if (cmd.Count() >= 3 && Convert.ToByte(cmd[1], 16) == 0x2 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((byte)Convert.ToByte(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else if (cmd.Count() >= 2 && Convert.ToByte(cmd[1], 16) != 0x2 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8))
                                    {
                                        write.Write((byte)Convert.ToByte(cmd[1], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "01D1" && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                                {
                                    if (cmd.Count() >= 3 && (Convert.ToUInt16(cmd[1], 16) == 0x4 || Convert.ToUInt16(cmd[1]) == 0x5 || Convert.ToUInt16(cmd[1]) == 0x7) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else if (cmd.Count() >= 4 && (Convert.ToUInt16(cmd[1], 16) == 0x0 || Convert.ToUInt16(cmd[1]) == 0x1 || Convert.ToUInt16(cmd[1]) == 0x2 || Convert.ToUInt16(cmd[1]) == 0x3) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                    }
                                    else if (cmd.Count() >= 2 && Convert.ToUInt16(cmd[1], 16) != 0x0 && Convert.ToUInt16(cmd[1], 16) != 0x1 && Convert.ToUInt16(cmd[1], 16) != 0x2 && Convert.ToUInt16(cmd[1], 16) != 0x3 && Convert.ToUInt16(cmd[1], 16) != 0x4 && Convert.ToUInt16(cmd[1], 16) != 0x5 && Convert.ToUInt16(cmd[1], 16) != 0x7 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "01E1" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                    if (cmd.Count() >= 4 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "01E9" && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                                {
                                    if (cmd.Count() >= 3 && (Convert.ToUInt16(cmd[1], 16) == 0x1 || Convert.ToUInt16(cmd[1], 16) == 0x2 || Convert.ToUInt16(cmd[1], 16) == 0x3 || Convert.ToUInt16(cmd[1], 16) == 0x7) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else if (cmd.Count() >= 4 && (Convert.ToUInt16(cmd[1], 16) == 0x5 || Convert.ToUInt16(cmd[1], 16) == 0x6) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                    }
                                    else if (cmd.Count() >= 2 && Convert.ToUInt16(cmd[1], 16) != 0x1 && Convert.ToUInt16(cmd[1], 16) != 0x2 && Convert.ToUInt16(cmd[1], 16) != 0x3 && Convert.ToUInt16(cmd[1], 16) != 0x5 && Convert.ToUInt16(cmd[1], 16) != 0x6 && Convert.ToUInt16(cmd[1], 16) != 0x7 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "021D")
                                {
                                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                                    {
                                        if (cmd.Count() >= 4 && Convert.ToUInt16(cmd[1], 16) != 0x5 && Convert.ToUInt16(cmd[1], 16) != 0x6 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                        }
                                        else if (cmd.Count() >= 3 && Convert.ToUInt16(cmd[1], 16) != 0x6 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        }
                                        else if (cmd.Count() >= 2 && (Convert.ToUInt16(cmd[1], 16) == 0x5 || Convert.ToUInt16(cmd[1], 16) == 0x6) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    else
                                    {
                                        if (cmd.Count() >= 4 && Convert.ToByte(cmd[1], 16) != 0x5 && Convert.ToByte(cmd[1], 16) != 0x6 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((byte)Convert.ToByte(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                        }
                                        else if (cmd.Count() >= 3 && Convert.ToByte(cmd[1], 16) != 0x6 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((byte)Convert.ToByte(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        }
                                        else if (cmd.Count() >= 2 && (Convert.ToByte(cmd[1], 16) == 0x5 || Convert.ToByte(cmd[1], 16) == 0x6) && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8))
                                        {
                                            write.Write((byte)Convert.ToByte(cmd[1], 16));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                }
                                else if (cmd[0] == "0235")
                                {
                                    if (cmd.Count() >= 3 && (Convert.ToUInt16(cmd[1], 16) == 0 || Convert.ToUInt16(cmd[1], 16) == 6) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else if (cmd.Count() >= 4 && Convert.ToUInt16(cmd[1], 16) == 4 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                    }
                                    else if (cmd.Count() >= 5 && (Convert.ToUInt16(cmd[1], 16) == 1 || Convert.ToUInt16(cmd[1], 16) == 3) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[4].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[4], 16));
                                    }
                                    else if (cmd.Count() >= 2 && Convert.ToUInt16(cmd[1], 16) != 0 && Convert.ToUInt16(cmd[1], 16) != 1 && Convert.ToUInt16(cmd[1], 16) != 3 && Convert.ToUInt16(cmd[1], 16) != 4 && Convert.ToUInt16(cmd[1], 16) != 6 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "023E")
                                {
                                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                                    {
                                        if (cmd.Count() >= 3 && (Convert.ToByte(cmd[1], 16) == 0x1 || Convert.ToByte(cmd[1], 16) == 0x3) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        }
                                        else if (cmd.Count() >= 4 && (Convert.ToByte(cmd[1], 16) == 0x5 || Convert.ToByte(cmd[1], 16) == 0x6) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                        }
                                        else if (cmd.Count() >= 2 && Convert.ToByte(cmd[1], 16) != 0x1 && Convert.ToByte(cmd[1], 16) != 0x3 && Convert.ToByte(cmd[1], 16) != 0x5 && Convert.ToByte(cmd[1], 16) != 0x6 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    else
                                    {
                                        if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                }
                                else if (cmd[0] == "02C4")
                                {
                                    if (cmd.Count() >= 3 && (Convert.ToByte(cmd[1], 16) == 0x0 || Convert.ToByte(cmd[1], 16) == 0x1) && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((byte)Convert.ToByte(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else if (cmd.Count() >= 2 && (Convert.ToByte(cmd[1], 16) != 0x0 && Convert.ToByte(cmd[1], 16) != 0x1) && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8))
                                    {
                                        write.Write((byte)Convert.ToByte(cmd[1], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "02C5" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                    if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "02C6" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                }
                                else if (cmd[0] == "02C9" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                }
                                else if (cmd[0] == "02CA" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                }
                                else if (cmd[0] == "02CD" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                }
                                else if (cmd[0] == "02CF" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                    if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else
                                {
                                    string[] data = scriptData.GetString(cmd[0]).Split(' ');
                                    int variables = Convert.ToInt32(data[0]);
                                    if (variables <= cmd.Count() - 1)
                                    {
                                        int tempcount = 2;
                                        for (int j = 0; j < variables; j++)
                                        {
                                            if (data[j + 1] == "1" && byte.TryParse(cmd[j + 1].Substring(2), hex, invar, out output8))
                                            {
                                                write.Write((byte)Convert.ToByte(cmd[j + 1], 16));
                                                tempcount++;
                                            }
                                            else if (data[j + 1] == "2" && UInt16.TryParse(cmd[j + 1].Substring(2), hex, invar, out output16))
                                            {
                                                write.Write((UInt16)Convert.ToUInt16(cmd[j + 1], 16));
                                                tempcount += 2;
                                            }
                                            else if (data[j + 1] == "4" && UInt32.TryParse(cmd[j + 1].Substring(2), hex, invar, out output32))
                                            {
                                                write.Write((UInt32)Convert.ToUInt32(cmd[j + 1], 16));
                                                tempcount += 4;
                                            }
                                            else
                                            {
                                                write.BaseStream.Position -= tempcount;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        write.BaseStream.Position -= 2;
                                    }
                                }
                            }
                            else
                            {
                                if (cmd[0] == "0002" || cmd[0] == "001B")
                                {
                                    currentLine++;
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                    if (currentLine == textBox2.Lines.Count() - 1)
                    {
                        write.Write((UInt16)0x0002);
                        break;
                    }
                    else
                    {
                        currentLine++;
                    }
                }
                if (write.BaseStream.Length % 2 == 1) write.Write((byte)0xFF);
                write.Flush();
                progressBar2.Value++;
            }
            #endregion
            #region Write Functions
            for (int i = 0; i < functionCount; i++)
            {
                functionList[comboBox9.SelectedIndex].Add(new MemoryStream());
                BinaryWriter write = new BinaryWriter(functionList[comboBox9.SelectedIndex][i]);
                currentLine = functionLines[i];
                while (true)
                {
                    try
                    {
                        string[] cmd = textBox3.Lines[currentLine].Split(' ');
                        if (scriptData.GetString(cmd[0]) == null && scriptNameW.GetString(cmd[0]) == null)
                        {
                        }
                        else
                        {
                            byte output8;
                            UInt16 output16;
                            UInt32 output32;
                            NumberStyles hex = NumberStyles.HexNumber;
                            CultureInfo invar = CultureInfo.InvariantCulture;
                            if (scriptData.GetString(cmd[0]) == null)
                            {
                                cmd[0] = scriptNameW.GetString(cmd[0]);
                            }
                            write.Write((UInt16)Convert.ToUInt16(cmd[0], 16));
                            if (scriptData.GetString(cmd[0]) != "0")
                            {
                                if (cmd[0] == "0016" || cmd[0] == "001A") // Jump GoTo
                                {
                                    try
                                    {
                                        if (cmd.Count() >= 2 && cmd[1].Substring(0, 8) == "Script_#" && UInt32.TryParse(cmd[1].Substring(8), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[1].Substring(8)) <= comboBox9.Items.Count && Convert.ToUInt32(cmd[1].Substring(8)) > 0)
                                        {
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToUInt32(0xFFFF + Convert.ToInt32(cmd[1].Substring(8))));
                                            if (cmd[0] == "0016")
                                            {
                                                currentLine++;
                                                break;
                                            }
                                        }
                                        else if (cmd.Count() >= 2 && cmd[1].Substring(0, 10) == "Function_#" && UInt32.TryParse(cmd[1].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[1].Substring(10)) <= functionCount && Convert.ToUInt32(cmd[1].Substring(10)) > 0)
                                        {
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToUInt32(cmd[1].Substring(10)));
                                            if (cmd[0] == "0016")
                                            {
                                                currentLine++;
                                                break;
                                            }
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    catch
                                    {
                                        write.BaseStream.Position -= 2;
                                    }
                                }
                                else if (cmd[0] == "001C" || cmd[0] == "001D") // CompareLastResult
                                {
                                    try
                                    {
                                        if (cmd.Count() >= 3 && checkOpByte(cmd[1]) && cmd[2].Substring(0, 8) == "Script_#" && UInt32.TryParse(cmd[2].Substring(8), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(8)) <= comboBox9.Items.Count && Convert.ToUInt32(cmd[2].Substring(8)) > 0)
                                        {
                                            write.Write(Convert.ToByte(getOpByte(cmd[1])));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToUInt32(0xFFFF + Convert.ToInt32(cmd[2].Substring(8))));
                                        }
                                        else if (cmd.Count() >= 3 && checkOpByte(cmd[1]) && cmd[2].Substring(0, 10) == "Function_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) <= functionCount && Convert.ToUInt32(cmd[2].Substring(10)) > 0)
                                        {
                                            write.Write(Convert.ToByte(getOpByte(cmd[1])));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToUInt32(cmd[2].Substring(10)));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    catch
                                    {
                                        write.BaseStream.Position -= 2;
                                    }
                                }
                                else if (cmd[0] == "005E")
                                {
                                    try
                                    {
                                        if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && cmd[2].Substring(0, 10) == "Movement_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) <= movementCount && Convert.ToUInt32(cmd[2].Substring(10)) != 0)
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToUInt32(cmd[2].Substring(10)));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    catch
                                    {
                                        write.BaseStream.Position -= 2;
                                    }
                                }
                                else if (cmd[0] == "011D" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                    if (cmd.Count() >= 4 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "0190" && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                                {
                                    if (cmd.Count() >= 3 && Convert.ToByte(cmd[1], 16) == 0x2 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((byte)Convert.ToByte(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else if (cmd.Count() >= 2 && Convert.ToByte(cmd[1], 16) != 0x2 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8))
                                    {
                                        write.Write((byte)Convert.ToByte(cmd[1], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "01CF")
                                {
                                    if (cmd.Count() >= 3 && Convert.ToByte(cmd[1], 16) == 0x2 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((byte)Convert.ToByte(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else if (cmd.Count() >= 2 && Convert.ToByte(cmd[1], 16) != 0x2 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8))
                                    {
                                        write.Write((byte)Convert.ToByte(cmd[1], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "01D1" && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                                {
                                    if (cmd.Count() >= 3 && (Convert.ToUInt16(cmd[1], 16) == 0x4 || Convert.ToUInt16(cmd[1]) == 0x5 || Convert.ToUInt16(cmd[1]) == 0x7) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else if (cmd.Count() >= 4 && (Convert.ToUInt16(cmd[1], 16) == 0x0 || Convert.ToUInt16(cmd[1]) == 0x1 || Convert.ToUInt16(cmd[1]) == 0x2 || Convert.ToUInt16(cmd[1]) == 0x3) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                    }
                                    else if (cmd.Count() >= 2 && Convert.ToUInt16(cmd[1], 16) != 0x0 && Convert.ToUInt16(cmd[1], 16) != 0x1 && Convert.ToUInt16(cmd[1], 16) != 0x2 && Convert.ToUInt16(cmd[1], 16) != 0x3 && Convert.ToUInt16(cmd[1], 16) != 0x4 && Convert.ToUInt16(cmd[1], 16) != 0x5 && Convert.ToUInt16(cmd[1], 16) != 0x7 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "01E1" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                    if (cmd.Count() >= 4 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "01E9" && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                                {
                                    if (cmd.Count() >= 3 && (Convert.ToUInt16(cmd[1], 16) == 0x1 || Convert.ToUInt16(cmd[1], 16) == 0x2 || Convert.ToUInt16(cmd[1], 16) == 0x3 || Convert.ToUInt16(cmd[1], 16) == 0x7) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else if (cmd.Count() >= 4 && (Convert.ToUInt16(cmd[1], 16) == 0x5 || Convert.ToUInt16(cmd[1], 16) == 0x6) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                    }
                                    else if (cmd.Count() >= 2 && Convert.ToUInt16(cmd[1], 16) != 0x1 && Convert.ToUInt16(cmd[1], 16) != 0x2 && Convert.ToUInt16(cmd[1], 16) != 0x3 && Convert.ToUInt16(cmd[1], 16) != 0x5 && Convert.ToUInt16(cmd[1], 16) != 0x6 && Convert.ToUInt16(cmd[1], 16) != 0x7 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "021D")
                                {
                                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                                    {
                                        if (cmd.Count() >= 4 && Convert.ToUInt16(cmd[1], 16) != 0x5 && Convert.ToUInt16(cmd[1], 16) != 0x6 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                        }
                                        else if (cmd.Count() >= 3 && Convert.ToUInt16(cmd[1], 16) != 0x6 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        }
                                        else if (cmd.Count() >= 2 && (Convert.ToUInt16(cmd[1], 16) == 0x5 || Convert.ToUInt16(cmd[1], 16) == 0x6) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    else
                                    {
                                        if (cmd.Count() >= 4 && Convert.ToByte(cmd[1], 16) != 0x5 && Convert.ToByte(cmd[1], 16) != 0x6 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((byte)Convert.ToByte(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                        }
                                        else if (cmd.Count() >= 3 && Convert.ToByte(cmd[1], 16) != 0x6 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((byte)Convert.ToByte(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        }
                                        else if (cmd.Count() >= 2 && (Convert.ToByte(cmd[1], 16) == 0x5 || Convert.ToByte(cmd[1], 16) == 0x6) && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8))
                                        {
                                            write.Write((byte)Convert.ToByte(cmd[1], 16));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                }
                                else if (cmd[0] == "0235")
                                {
                                    if (cmd.Count() >= 3 && (Convert.ToUInt16(cmd[1], 16) == 0 || Convert.ToUInt16(cmd[1], 16) == 6) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else if (cmd.Count() >= 4 && Convert.ToUInt16(cmd[1], 16) == 4 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                    }
                                    else if (cmd.Count() >= 5 && (Convert.ToUInt16(cmd[1], 16) == 1 || Convert.ToUInt16(cmd[1], 16) == 3) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[4].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[4], 16));
                                    }
                                    else if (cmd.Count() >= 2 && Convert.ToUInt16(cmd[1], 16) != 0 && Convert.ToUInt16(cmd[1], 16) != 1 && Convert.ToUInt16(cmd[1], 16) != 3 && Convert.ToUInt16(cmd[1], 16) != 4 && Convert.ToUInt16(cmd[1], 16) != 6 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "023E")
                                {
                                    if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                                    {
                                        if (cmd.Count() >= 3 && (Convert.ToByte(cmd[1], 16) == 0x1 || Convert.ToByte(cmd[1], 16) == 0x3) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        }
                                        else if (cmd.Count() >= 4 && (Convert.ToByte(cmd[1], 16) == 0x5 || Convert.ToByte(cmd[1], 16) == 0x6) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[3], 16));
                                        }
                                        else if (cmd.Count() >= 2 && Convert.ToByte(cmd[1], 16) != 0x1 && Convert.ToByte(cmd[1], 16) != 0x3 && Convert.ToByte(cmd[1], 16) != 0x5 && Convert.ToByte(cmd[1], 16) != 0x6 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    else
                                    {
                                        if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                }
                                else if (cmd[0] == "02C4")
                                {
                                    if (cmd.Count() >= 3 && (Convert.ToByte(cmd[1], 16) == 0x0 || Convert.ToByte(cmd[1], 16) == 0x1) && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((byte)Convert.ToByte(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else if (cmd.Count() >= 2 && (Convert.ToByte(cmd[1], 16) != 0x0 && Convert.ToByte(cmd[1], 16) != 0x1) && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8))
                                    {
                                        write.Write((byte)Convert.ToByte(cmd[1], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "02C5" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                    if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else if (cmd[0] == "02C6" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                }
                                else if (cmd[0] == "02C9" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                }
                                else if (cmd[0] == "02CA" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                }
                                else if (cmd[0] == "02CD" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                }
                                else if (cmd[0] == "02CF" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                                {
                                    if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                        write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                    }
                                    else write.BaseStream.Position -= 2;
                                }
                                else
                                {
                                    string[] data = scriptData.GetString(cmd[0]).Split(' ');
                                    int variables = Convert.ToInt32(data[0]);
                                    if (variables <= cmd.Count() - 1)
                                    {
                                        int tempcount = 2;
                                        for (int j = 0; j < variables; j++)
                                        {
                                            if (data[j + 1] == "1" && byte.TryParse(cmd[j + 1].Substring(2), hex, invar, out output8))
                                            {
                                                write.Write((byte)Convert.ToByte(cmd[j + 1], 16));
                                                tempcount++;
                                            }
                                            else if (data[j + 1] == "2" && UInt16.TryParse(cmd[j + 1].Substring(2), hex, invar, out output16))
                                            {
                                                write.Write((UInt16)Convert.ToUInt16(cmd[j + 1], 16));
                                                tempcount += 2;
                                            }
                                            else if (data[j + 1] == "4" && UInt32.TryParse(cmd[j + 1].Substring(2), hex, invar, out output32))
                                            {
                                                write.Write((UInt32)Convert.ToUInt32(cmd[j + 1], 16));
                                                tempcount += 4;
                                            }
                                            else
                                            {
                                                write.BaseStream.Position -= tempcount;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        write.BaseStream.Position -= 2;
                                    }
                                }
                            }
                            else
                            {
                                if (cmd[0] == "0002" || cmd[0] == "001B")
                                {
                                    currentLine++;
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                    if (currentLine == textBox3.Lines.Count() - 1)
                    {
                        write.Write((UInt16)0x0002);
                        break;
                    }
                    else
                    {
                        currentLine++;
                    }
                }
                if (write.BaseStream.Length % 2 == 1) write.Write((byte)0xFF);
                write.Flush();
                progressBar2.Value++;
            }
            #endregion
            #region Write Movements
            for (int i = 0; i < movementCount; i++)
            {
                movementList[comboBox9.SelectedIndex].Add(new MemoryStream());
                BinaryWriter write = new BinaryWriter(movementList[comboBox9.SelectedIndex][i]);
                currentLine = movementLines[i];
                while (true)
                {
                    try
                    {
                        string[] cmd = textBox4.Lines[currentLine].Split(' ');
                        int output;
                        if ((!int.TryParse(cmd[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output) || (Convert.ToUInt16(cmd[0], 16) < 0 && Convert.ToUInt16(cmd[0], 16) > 0x00FE)) && MovementNameW.GetString(cmd[0]) == null)
                        {
                        }
                        else
                        {
                            if (!int.TryParse(cmd[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output))
                            {
                                cmd[0] = MovementNameW.GetString(cmd[0]);
                            }
                            write.Write((UInt16)Convert.ToUInt16(cmd[0], 16));
                            if (cmd.Count() >= 2)
                            {
                                write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                            }
                            else
                            {
                                if (cmd[0] == "End" || cmd[0] == "00FE")
                                {
                                    currentLine++;
                                    break;
                                }
                                else
                                {
                                    write.BaseStream.Position -= 2;
                                }
                            }
                        }
                    }
                    catch { }
                    if (currentLine == textBox4.Lines.Count() - 1)
                    {
                        write.Write((UInt16)0xFE);
                        break;
                    }
                    else
                    {
                        currentLine++;
                    }
                }
                write.Flush();
                progressBar2.Value++;
            }
            #endregion
            //write.Write((UInt16)0);
            //if (write.BaseStream.Length % 2 == 1) write.Write((byte)0);
            //write.Close();
            progressBar2.Value = progressBar2.Maximum;
        }

        private int GetScriptLength(int line, bool function, int scriptCount, int functionCount, int movementCount, int currentIndex) 
        {
            int length = 0;
            currentLine = line;
            while (true)
            {
                try
                {
                    string[] cmd;
                    if (!function) cmd = textBox2.Lines[currentLine].Split(' ');
                    else cmd = textBox3.Lines[currentLine].Split(' ');
                    if (scriptData.GetString(cmd[0]) == null && scriptNameW.GetString(cmd[0]) == null)
                    {
                        uint output;
                        if (!function && length == 0 && cmd[0].Substring(0, 11) == "UseScript_#" && UInt32.TryParse(cmd[0].Substring(11), NumberStyles.Any, CultureInfo.InvariantCulture, out output) && Convert.ToUInt32(cmd[0].Substring(11)) < currentIndex + 1 && Convert.ToUInt32(cmd[0].Substring(11)) > 0)
                        {
                            useIndex.Add((int)Convert.ToUInt32(cmd[0].Substring(11)));
                            break;
                        }
                    }
                    else
                    {
                        byte output8;
                        UInt16 output16;
                        UInt32 output32;
                        NumberStyles hex = NumberStyles.HexNumber;
                        CultureInfo invar = CultureInfo.InvariantCulture;
                        if (scriptData.GetString(cmd[0]) == null)
                        {
                            cmd[0] = scriptNameW.GetString(cmd[0]);
                        }
                        length += 2;
                        if (scriptData.GetString(cmd[0]) != "0")
                        {
                            if (cmd[0] == "0016" || cmd[0] == "001A") // Jump GoTo
                            {
                                if (cmd.Count() >= 2 && cmd[1].Substring(0, 8) == "Script_#" && UInt32.TryParse(cmd[1].Substring(8), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[1].Substring(8)) < scriptCount + 1 && Convert.ToUInt32(cmd[1].Substring(8)) > 0)
                                {
                                    length += 4;
                                    if (cmd[0] == "0016")
                                    {
                                        currentLine++;
                                        break;
                                    }
                                }
                                else if (cmd.Count() >= 2 && cmd[1].Substring(0, 10) == "Function_#" && UInt32.TryParse(cmd[1].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[1].Substring(10)) < functionCount + 1 && Convert.ToUInt32(cmd[1].Substring(10)) > 0)
                                {
                                    length += 4;
                                    if (cmd[0] == "0016")
                                    {
                                        currentLine++;
                                        break;
                                    }
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "001C" || cmd[0] == "001D") // CompareLastResult
                            {
                                if (cmd.Count() >= 3 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && cmd[2].Substring(0, 8) == "Script_#" && UInt32.TryParse(cmd[2].Substring(8), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(8)) < scriptCount + 1 && Convert.ToUInt32(cmd[2].Substring(8)) > 0)
                                {
                                    length++;
                                    length += 4;
                                }
                                else if (cmd.Count() >= 3 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && cmd[2].Substring(0, 10) == "Function_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) < functionCount + 1 && Convert.ToUInt32(cmd[2].Substring(10)) > 0)
                                {
                                    length++;
                                    length += 4;
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "005E")
                            {
                                if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && cmd[2].Substring(0, 10) == "Movement_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) <= movementCount && Convert.ToUInt32(cmd[2].Substring(10)) != 0)
                                {
                                    length += 2;
                                    length += 4;
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "011D" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                                if (cmd.Count() >= 4 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                {
                                    length += 6;
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "0190" && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                            {
                                if (cmd.Count() >= 3 && Convert.ToByte(cmd[1], 16) == 0x2 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                {
                                    length += 3;
                                }
                                else if (cmd.Count() >= 2 && Convert.ToByte(cmd[1], 16) != 0x2 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8))
                                {
                                    length++;
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "01CF")
                            {
                                if (cmd.Count() >= 3 && Convert.ToByte(cmd[1], 16) == 0x2 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                {
                                    length += 3;
                                }
                                else if (cmd.Count() >= 2 && Convert.ToByte(cmd[1], 16) != 0x2 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8))
                                {
                                    length++;
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "01D1" && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                            {
                                if (cmd.Count() >= 3 && (Convert.ToUInt16(cmd[1], 16) == 0x4 || Convert.ToUInt16(cmd[1]) == 0x5 || Convert.ToUInt16(cmd[1]) == 0x7) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                {
                                    length += 4;
                                }
                                else if (cmd.Count() >= 4 && (Convert.ToUInt16(cmd[1], 16) == 0x0 || Convert.ToUInt16(cmd[1]) == 0x1 || Convert.ToUInt16(cmd[1]) == 0x2 || Convert.ToUInt16(cmd[1]) == 0x3) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                {
                                    length += 6;
                                }
                                else if (cmd.Count() >= 2 && Convert.ToUInt16(cmd[1], 16) != 0x0 && Convert.ToUInt16(cmd[1], 16) != 0x1 && Convert.ToUInt16(cmd[1], 16) != 0x2 && Convert.ToUInt16(cmd[1], 16) != 0x3 && Convert.ToUInt16(cmd[1], 16) != 0x4 && Convert.ToUInt16(cmd[1], 16) != 0x5 && Convert.ToUInt16(cmd[1], 16) != 0x7 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                {
                                    length += 2;
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "01E1" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                                if (cmd.Count() >= 4 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                {
                                    length += 6;
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "01E9" && (gameID == 0x454B5049 || gameID == 0x45475049 || gameID == 0x534B5049 || gameID == 0x53475049 || gameID == 0x464B5049 || gameID == 0x46475049 || gameID == 0x494B5049 || gameID == 0x49475049 || gameID == 0x444B5049 || gameID == 0x44475049 || gameID == 0x4A4B5049 || gameID == 0x4A475049 || gameID == 0x4B4B5049 || gameID == 0x4B475049))
                            {
                                if (cmd.Count() >= 3 && (Convert.ToUInt16(cmd[1], 16) == 0x1 || Convert.ToUInt16(cmd[1], 16) == 0x2 || Convert.ToUInt16(cmd[1], 16) == 0x3 || Convert.ToUInt16(cmd[1], 16) == 0x7) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                {
                                    length += 4;
                                }
                                else if (cmd.Count() >= 4 && (Convert.ToUInt16(cmd[1], 16) == 0x5 || Convert.ToUInt16(cmd[1], 16) == 0x6) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                {
                                    length += 6;
                                }
                                else if (cmd.Count() >= 2 && Convert.ToUInt16(cmd[1], 16) != 0x1 && Convert.ToUInt16(cmd[1], 16) != 0x2 && Convert.ToUInt16(cmd[1], 16) != 0x3 && Convert.ToUInt16(cmd[1], 16) != 0x5 && Convert.ToUInt16(cmd[1], 16) != 0x6 && Convert.ToUInt16(cmd[1], 16) != 0x7 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                {
                                    length += 2;
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "021D")
                            {
                                if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                                {
                                    if (cmd.Count() >= 4 && Convert.ToUInt16(cmd[1], 16) != 0x5 && Convert.ToUInt16(cmd[1], 16) != 0x6 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                    {
                                        length += 6;
                                    }
                                    else if (cmd.Count() >= 3 && Convert.ToUInt16(cmd[1], 16) != 0x6 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        length += 4;
                                    }
                                    else if (cmd.Count() >= 2 && (Convert.ToUInt16(cmd[1], 16) == 0x5 || Convert.ToUInt16(cmd[1], 16) == 0x6) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                    {
                                        length += 2;
                                    }
                                    else length -= 2;
                                }
                                else
                                {
                                    if (cmd.Count() >= 4 && Convert.ToByte(cmd[1], 16) != 0x5 && Convert.ToByte(cmd[1], 16) != 0x6 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                    {
                                        length += 5;
                                    }
                                    else if (cmd.Count() >= 3 && Convert.ToByte(cmd[1], 16) != 0x6 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        length += 3;
                                    }
                                    else if (cmd.Count() >= 2 && (Convert.ToByte(cmd[1], 16) == 0x5 || Convert.ToByte(cmd[1], 16) == 0x6) && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8))
                                    {
                                        length++;
                                    }
                                    else length -= 2;
                                }
                            }
                            else if (cmd[0] == "0235")
                            {
                                if (cmd.Count() >= 3 && (Convert.ToUInt16(cmd[1], 16) == 0 || Convert.ToUInt16(cmd[1], 16) == 6) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                {
                                    length += 4;
                                }
                                else if (cmd.Count() >= 4 && Convert.ToUInt16(cmd[1], 16) == 4 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                {
                                    length += 6;
                                }
                                else if (cmd.Count() >= 5 && (Convert.ToUInt16(cmd[1], 16) == 1 || Convert.ToUInt16(cmd[1], 16) == 3) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[4].Substring(2), hex, invar, out output16))
                                {
                                    length += 8;
                                }
                                else if (cmd.Count() >= 2 && Convert.ToUInt16(cmd[1], 16) != 0 && Convert.ToUInt16(cmd[1], 16) != 1 && Convert.ToUInt16(cmd[1], 16) != 3 && Convert.ToUInt16(cmd[1], 16) != 4 && Convert.ToUInt16(cmd[1], 16) != 6 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                {
                                    length += 2;
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "023E")
                            {
                                if (gameID == 0x45414441 || gameID == 0x45415041 || gameID == 0x53414441 || gameID == 0x53415041 || gameID == 0x46414441 || gameID == 0x46415041 || gameID == 0x49414441 || gameID == 0x49415041 || gameID == 0x44414441 || gameID == 0x44415041 || gameID == 0x4A414441 || gameID == 0x4A415041 || gameID == 0x4B414441 || gameID == 0x4B415041 || gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043)
                                {
                                    if (cmd.Count() >= 3 && (Convert.ToByte(cmd[1], 16) == 0x1 || Convert.ToByte(cmd[1], 16) == 0x3) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        length += 4;
                                    }
                                    else if (cmd.Count() >= 4 && (Convert.ToByte(cmd[1], 16) == 0x5 || Convert.ToByte(cmd[1], 16) == 0x6) && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[3].Substring(2), hex, invar, out output16))
                                    {
                                        length += 6;
                                    }
                                    else if (cmd.Count() >= 2 && Convert.ToByte(cmd[1], 16) != 0x1 && Convert.ToByte(cmd[1], 16) != 0x3 && Convert.ToByte(cmd[1], 16) != 0x5 && Convert.ToByte(cmd[1], 16) != 0x6 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16))
                                    {
                                        length += 2;
                                    }
                                    else length -= 2;
                                }
                                else
                                {
                                    if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                    {
                                        length += 4;
                                    }
                                    else length -= 2;
                                }
                            }
                            else if (cmd[0] == "02C4")
                            {
                                if (cmd.Count() >= 3 && (Convert.ToByte(cmd[1], 16) == 0x0 || Convert.ToByte(cmd[1], 16) == 0x1) && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                {
                                    length += 3;
                                }
                                else if (cmd.Count() >= 2 && (Convert.ToByte(cmd[1], 16) != 0x0 && Convert.ToByte(cmd[1], 16) != 0x1) && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8))
                                {
                                    length++;
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "02C5" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                                if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                {
                                    length += 4;
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "02C6" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                            }
                            else if (cmd[0] == "02C9" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                            }
                            else if (cmd[0] == "02CA" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                            }
                            else if (cmd[0] == "02CD" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                            }
                            else if (cmd[0] == "02CF" && (gameID == 0x45555043 || gameID == 0x53555043 || gameID == 0x46555043 || gameID == 0x49555043 || gameID == 0x44555043 || gameID == 0x4A555043 || gameID == 0x4B555043))
                            {
                                if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                {
                                    length += 4;
                                }
                                else length -= 2;
                            }
                            else
                            {
                                string[] data = scriptData.GetString(cmd[0]).Split(' ');
                                int variables = Convert.ToInt32(data[0]);
                                if (variables <= cmd.Count() - 1)
                                {
                                    int tempcount = 2;
                                    for (int i = 0; i < variables; i++)
                                    {
                                        if (data[i + 1] == "1" && byte.TryParse(cmd[i + 1].Substring(2), hex, invar, out output8))
                                        {
                                            length++;
                                            tempcount++;
                                        }
                                        else if (data[i + 1] == "2" && UInt16.TryParse(cmd[i + 1].Substring(2), hex, invar, out output16))
                                        {
                                            length += 2;
                                            tempcount += 2;
                                        }
                                        else if (data[i + 1] == "4" && UInt32.TryParse(cmd[i + 1].Substring(2), hex, invar, out output32))
                                        {
                                            length += 4;
                                            tempcount += 4;
                                        }
                                        else
                                        {
                                            length -= tempcount;
                                        }
                                    }
                                }
                                else
                                {
                                    length -= 2;
                                }
                            }
                        }
                        else
                        {
                            if (cmd[0] == "0002" || cmd[0] == "001B")
                            {
                                currentLine++;
                                break;
                            }
                        }
                    }
                }
                catch { }
                if (!function && currentLine == textBox2.Lines.Count() - 1)
                {
                    length += 2;
                    break;
                }
                else if (function && currentLine == textBox3.Lines.Count() - 1)
                {
                    length += 2;
                    break;
                }
                else
                {
                    currentLine++;
                }
            }
            if (length % 2 == 1) length++;
            return length;
        }

        private int GetMovementLength(int line)
        {
            int length = 0;
            currentLine = line + 1;
            while (true)
            {
                try
                {
                    string[] cmd = textBox4.Lines[currentLine].Split(' ');
                    int output;
                    if ((!int.TryParse(cmd[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output) || (Convert.ToUInt16(cmd[0], 16) < 0 && Convert.ToUInt16(cmd[0], 16) > 0x00FE)) && MovementNameW.GetString(cmd[0]) == null)
                    {
                    }
                    else
                    {
                        if (!int.TryParse(cmd[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output))
                        {
                            cmd[0] = MovementNameW.GetString(cmd[0]);
                        }
                        length += 2;
                        UInt16 output16;
                        if (cmd.Count() >= 2 && UInt16.TryParse(cmd[1].Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output16))
                        {
                            length += 2;
                        }
                        else
                        {
                            if (cmd[0] == "End" || cmd[0] == "00FE")
                            {
                                currentLine++;
                                break;
                            }
                            else
                            {
                                length -= 2;
                            }
                        }
                    }
                }
                catch { }
                if (currentLine == textBox4.Lines.Count() - 1)
                {
                    length += 2;
                    break;
                }
                else
                {
                    currentLine++;
                }
            }
            return length;
        }

        private void readScriptGenV() // Read Script File (Gen V)
        {
            #region RM
            if (isBW)
            {
                if (gameID == 0x4A425249 || gameID == 0x4A415249)
                {
                    scriptData = new ResourceManager("WindowsFormsApplication1.Resources.ScriptsV.ScriptsBWJ", Assembly.GetExecutingAssembly());
                    scriptName = new ResourceManager("WindowsFormsApplication1.Resources.ScriptsV.ScriptNamesBWJ", Assembly.GetExecutingAssembly());
                }
                else
                {
                    scriptData = new ResourceManager("WindowsFormsApplication1.Resources.ScriptsV.ScriptsBW", Assembly.GetExecutingAssembly());
                    scriptName = new ResourceManager("WindowsFormsApplication1.Resources.ScriptsV.ScriptNamesBW", Assembly.GetExecutingAssembly());
                }
            }
            else
            {
                if (gameID == 0x4A455249 || gameID == 0x4A445249)
                {
                    scriptData = new ResourceManager("WindowsFormsApplication1.Resources.ScriptsV.ScriptsB2W2J", Assembly.GetExecutingAssembly());
                    scriptName = new ResourceManager("WindowsFormsApplication1.Resources.ScriptsV.ScriptNamesB2W2J", Assembly.GetExecutingAssembly());
                }
                else
                {
                    scriptData = new ResourceManager("WindowsFormsApplication1.Resources.ScriptsV.ScriptsB2W2", Assembly.GetExecutingAssembly());
                    scriptName = new ResourceManager("WindowsFormsApplication1.Resources.ScriptsV.ScriptNamesB2W2", Assembly.GetExecutingAssembly());
                }
            }
            #endregion
            progressBar2.Value = progressBar2.Minimum;
            textBox2.Clear();
            textBox3.Clear();
            textBox4.Clear();
            scriptList.Clear();
            functionList.Clear();
            movementList.Clear();
            button33.Enabled = false;
            int count = 0;
            MovementOffset.Clear();
            FunctionOffset.Clear();
            scriptOffset.Clear();
            useScriptList.Clear();
            comboBox9.Items.Clear();
            comboBox9.Enabled = false;
            System.IO.BinaryReader readScript = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\5\scripts" + "\\" + comboBox5.SelectedIndex.ToString("D4")));
            int flag = (int)readScript.ReadUInt32();
            if (flag == 0)
            {
                textBox2.Text = "Level Scripts File";
                progressBar2.Value = progressBar2.Maximum;
                button33.Enabled = false;
                readScript.Close();
                return;
            }
            if ((flag & 0xFFFF) == 0xFD13)
            {
                textBox2.Text = "No scripts";
                progressBar2.Value = progressBar2.Maximum;
                readScript.Close();
                button33.Enabled = true;
                return;
            }
            while ((flag & 0xFFFF) != 0xFD13)
            {
                scriptOffset.Add(flag + (int)readScript.BaseStream.Position);
                FunctionOffset.Add(new List<int>());
                MovementOffset.Add(new List<int>());
                functionList.Add(new List<MemoryStream>());
                movementList.Add(new List<MemoryStream>());
                count++;
                if (scriptOffset.Contains((int)readScript.BaseStream.Position)) break;
                flag = (int)readScript.ReadUInt32();
                if ((flag & 0xFFFF) == 0)
                {
                    textBox2.Text = "Level Scripts File";
                    progressBar2.Value = progressBar2.Maximum;
                    button33.Enabled = false;
                    readScript.Close();
                    return;
                }
            }
            progressBar2.Maximum = count * 3;
            comboBox9.Enabled = true;
            try
            {
                #region Scripts
                for (int i = 0; i < count; i++)
                {
                    readScript.BaseStream.Position = scriptOffset[i];
                    comboBox9.Items.Add("Script #" + (i + 1).ToString());
                    scriptList.Add(new MemoryStream());
                    BinaryWriter load = new BinaryWriter(scriptList[i]);
                    bool exists = false;
                    if (scriptOffset.IndexOf(scriptOffset[i]) != i)
                    {
                        useScriptList.Add(scriptOffset.IndexOf(scriptOffset[i]));
                        exists = true;
                    }
                    useScriptList.Add(0xFFFF);
                    while (!exists)
                    {
                        int currentCmd = readScript.ReadUInt16();
                        load.Write((UInt16)currentCmd);
                        Debug.Print("Command read: " + currentCmd.ToString("X4"));
                        string[] cmd = scriptData.GetString(currentCmd.ToString("X4")).Split(' ');
                        if (cmd[0] != "0")
                        {
                            if (currentCmd == 0x001E || currentCmd == 0x0004) // Jump Call
                            {
                                int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                if (FunctionOffset[i].Contains(offset))
                                {
                                    load.Write((FunctionOffset[i].IndexOf(offset) + 1));
                                }
                                else if (scriptOffset.Contains(offset))
                                {
                                    load.Write((0xFFFF + (scriptOffset.IndexOf(offset) + 1)));
                                }
                                else
                                {
                                    FunctionOffset[i].Add(offset);
                                    load.Write((FunctionOffset[i].Count));
                                    ProcessFunctionV(offset, i);
                                }
                                if (currentCmd == 0x001E)
                                {
                                    break;
                                }
                            }
                            else if (currentCmd == 0x001F) // If
                            {
                                load.Write((byte)readScript.ReadByte());
                                int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                if (FunctionOffset[i].Contains(offset))
                                {
                                    load.Write((FunctionOffset[i].IndexOf(offset) + 1));
                                }
                                else if (scriptOffset.Contains(offset))
                                {
                                    load.Write((0xFFFF + (scriptOffset.IndexOf(offset) + 1)));
                                }
                                else
                                {
                                    FunctionOffset[i].Add(offset);
                                    load.Write((FunctionOffset[i].Count));
                                    ProcessFunctionV(offset, i);
                                }
                            }
                            else if (currentCmd == 0x0064 && gameID != 0x4A425249 && gameID != 0x4A415249 && gameID != 0x4A455249 && gameID != 0x4A445249)
                            {
                                load.Write((Int16)readScript.ReadInt16());
                                int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                MovementOffset[i].Add(offset);
                                load.Write((MovementOffset[i].Count));
                            }
                            else if (currentCmd == 0x0060 && (gameID == 0x4A425249 || gameID == 0x4A415249 || gameID == 0x4A455249 || gameID == 0x4A445249))
                            {
                                load.Write((Int16)readScript.ReadInt16());
                                int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                MovementOffset[i].Add(offset);
                                load.Write((MovementOffset[i].Count));
                            }
                            else
                            {
                                for (int j = 0; j < Convert.ToInt32(cmd[0]); j++)
                                {
                                    if (cmd[j + 1] == "1")
                                    {
                                        load.Write((byte)readScript.ReadByte());
                                    }
                                    if (cmd[j + 1] == "2")
                                    {
                                        load.Write((Int16)readScript.ReadInt16());
                                    }
                                    if (cmd[j + 1] == "4")
                                    {
                                        load.Write(readScript.ReadInt32());
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (currentCmd == 0x0002 || currentCmd == 0x0005 || currentCmd == 0x001D)
                            {
                                break;
                            }
                            else if (currentCmd == 0x0003)
                            {
                                load.Write((Int16)readScript.ReadInt16());
                                break;
                            }
                        }
                    }
                    load.Flush();
                    progressBar2.Value++;
                }
                #endregion
                #region Functions
                for (int j = 0; j < count; j++)
                {
                    for (int i = 0; i < FunctionOffset[j].Count; i++)
                    {
                        readScript.BaseStream.Position = FunctionOffset[j][i];
                        functionList[j].Add(new MemoryStream());
                        BinaryWriter load = new BinaryWriter(functionList[j][i]);
                        while (true)
                        {
                            int currentCmd = readScript.ReadUInt16();
                            load.Write((UInt16)currentCmd);
                            Debug.Print("Command read: " + currentCmd.ToString("X4"));
                            string[] cmd = scriptData.GetString(currentCmd.ToString("X4")).Split(' ');
                            if (cmd[0] != "0")
                            {
                                if (currentCmd == 0x001E || currentCmd == 0x0004) // Jump Call
                                {
                                    int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                    if (FunctionOffset[j].Contains(offset))
                                    {
                                        load.Write((FunctionOffset[j].IndexOf(offset) + 1));
                                    }
                                    else
                                    {
                                        load.Write((0xFFFF + (scriptOffset.IndexOf(offset) + 1)));
                                    }
                                    if (currentCmd == 0x001E)
                                    {
                                        break;
                                    }
                                }
                                else if (currentCmd == 0x001F) // If
                                {
                                    load.Write((byte)readScript.ReadByte());
                                    int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                    if (FunctionOffset[j].Contains(offset))
                                    {
                                        load.Write((FunctionOffset[j].IndexOf(offset) + 1));
                                    }
                                    else
                                    {
                                        load.Write((0xFFFF + (scriptOffset.IndexOf(offset) + 1)));
                                    }
                                }
                                else if (currentCmd == 0x0064 && gameID != 0x4A425249 && gameID != 0x4A415249 && gameID != 0x4A455249 && gameID != 0x4A445249)
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                    int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                    MovementOffset[j].Add(offset);
                                    load.Write((MovementOffset[j].Count));
                                }
                                else if (currentCmd == 0x0060 && (gameID == 0x4A425249 || gameID == 0x4A415249 || gameID == 0x4A455249 || gameID == 0x4A445249))
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                    int offset = readScript.ReadInt32() + (int)readScript.BaseStream.Position;
                                    MovementOffset[j].Add(offset);
                                    load.Write((MovementOffset[j].Count));
                                }
                                else
                                {
                                    for (int k = 0; k < Convert.ToInt32(cmd[0]); k++)
                                    {
                                        if (cmd[k + 1] == "1")
                                        {
                                            load.Write((byte)readScript.ReadByte());
                                        }
                                        if (cmd[k + 1] == "2")
                                        {
                                            load.Write((Int16)readScript.ReadInt16());
                                        }
                                        if (cmd[k + 1] == "4")
                                        {
                                            load.Write(readScript.ReadInt32());
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (currentCmd == 0x0002 || currentCmd == 0x0005 || currentCmd == 0x001D)
                                {
                                    break;
                                }
                                else if (currentCmd == 0x0003)
                                {
                                    load.Write((Int16)readScript.ReadInt16());
                                    break;
                                }
                            }
                        }
                        load.Flush();
                    }
                    progressBar2.Value++;
                }
                #endregion
                #region Movements
                for (int j = 0; j < count; j++)
                {
                    for (int i = 0; i < MovementOffset[j].Count; i++)
                    {
                        readScript.BaseStream.Position = MovementOffset[j][i];
                        movementList[j].Add(new MemoryStream());
                        BinaryWriter load = new BinaryWriter(movementList[j][i]);
                        while (true)
                        {
                            int currentMove = readScript.ReadUInt16();
                            load.Write((UInt16)currentMove);
                            if (currentMove != 0x00FE)
                            {
                                load.Write((UInt16)readScript.ReadUInt16());
                            }
                            else
                            {
                                break;
                            }
                        }
                        load.Flush();
                    }
                    progressBar2.Value++;
                }
                #endregion
            }
            catch
            {
                textBox2.Text = "Script Error";
                progressBar2.Value = progressBar2.Maximum;
                readScript.Close();
                return;
            }
            progressBar2.Value = progressBar2.Maximum;
            button33.Enabled = true;
            readScript.Close();
            comboBox9.SelectedIndex = 0;
        }

        private void ProcessFunctionV(int offset2, int script) // Read function (Gen V)
        {
            Debug.Print("Function Read Start");
            System.IO.BinaryReader readFunction = new System.IO.BinaryReader(File.OpenRead(workingFolder + @"data\a\0\5\scripts" + "\\" + comboBox5.SelectedIndex.ToString("D4")));
            readFunction.BaseStream.Position = offset2;
            while (true)
            {
                int currentCmd = readFunction.ReadUInt16();
                Debug.Print("Command read: " + currentCmd.ToString("X4"));
                string[] cmd = scriptData.GetString(currentCmd.ToString("X4")).Split(' ');
                if (cmd[0] != "0")
                {
                    if (currentCmd == 0x001E || currentCmd == 0x0004) // Jump Call
                    {
                        int offset = readFunction.ReadInt32() + (int)readFunction.BaseStream.Position;
                        if (!FunctionOffset[script].Contains(offset) && !scriptOffset.Contains(offset))
                        {
                            FunctionOffset[script].Add(offset);
                            ProcessFunctionV(offset, script);
                        }
                        if (currentCmd == 0x001E)
                        {
                            break;
                        }
                    }
                    else if (currentCmd == 0x001F) // If
                    {
                        readFunction.BaseStream.Position++;
                        int offset = readFunction.ReadInt32() + (int)readFunction.BaseStream.Position;
                        if (!FunctionOffset[script].Contains(offset) && !scriptOffset.Contains(offset))
                        {
                            FunctionOffset[script].Add(offset);
                            ProcessFunctionV(offset, script);
                        }
                    }
                    else if (currentCmd == 0x0064 && gameID != 0x4A425249 && gameID != 0x4A415249 && gameID != 0x4A455249 && gameID != 0x4A445249)
                    {
                        readFunction.BaseStream.Position += 6;
                    }
                    else if (currentCmd == 0x0060 && (gameID == 0x4A425249 || gameID == 0x4A415249 || gameID == 0x4A455249 || gameID == 0x4A445249))
                    {
                        readFunction.BaseStream.Position += 6;
                    }
                    else
                    {
                        for (int j = 0; j < Convert.ToInt32(cmd[0]); j++)
                        {
                            if (cmd[j + 1] == "1")
                            {
                                readFunction.BaseStream.Position++;
                            }
                            if (cmd[j + 1] == "2")
                            {
                                readFunction.BaseStream.Position += 2;
                            }
                            if (cmd[j + 1] == "4")
                            {
                                readFunction.BaseStream.Position += 4;
                            }
                        }
                    }
                }
                else
                {
                    if (currentCmd == 0x0002 || currentCmd == 0x0005 || currentCmd == 0x001D)
                    {
                        break;
                    }
                    else if (currentCmd == 0x0003)
                    {
                        readFunction.BaseStream.Position += 2;
                        break;
                    }
                }
            }
            readFunction.Close();
            Debug.Print("Function Read End");
            return;
        }

        private void saveScriptGenV() // Save Script File (Gen V)
        {
            #region RM
            if (isBW)
            {
                if (gameID == 0x4A425249 || gameID == 0x4A415249)
                {
                    scriptNameW = new ResourceManager("WindowsFormsApplication1.Resources.ScriptsV.ScriptNamesWBWJ", Assembly.GetExecutingAssembly());
                }
                else
                {
                    scriptNameW = new ResourceManager("WindowsFormsApplication1.Resources.ScriptsV.ScriptNamesWBW", Assembly.GetExecutingAssembly());
                }
            }
            else
            {
            }
            #endregion
            List<int> scriptLines = new List<int>();
            List<int> functionLines = new List<int>();
            List<int> movementLines = new List<int>();
            List<int> scriptOffsets = new List<int>();
            List<int> functionOffsets = new List<int>();
            List<int> movementOffsets = new List<int>();
            int scriptCount = 0;
            int functionCount = 0;
            int movementCount = 0;
            for (int i = 0; i < textBox2.Lines.Count(); i++)
            {
                try
                {
                    if (textBox2.Lines[i].Substring(0, 8) == "Script #")
                    {
                        scriptCount++;
                        scriptLines.Add(i);
                    }
                }
                catch { }
            }
            for (int i = 0; i < textBox3.Lines.Count(); i++)
            {
                try
                {
                    if (textBox3.Lines[i].Substring(0, 10) == "Function #")
                    {
                        functionCount++;
                        functionLines.Add(i);
                    }
                }
                catch { }
            }
            for (int i = 0; i < textBox4.Lines.Count(); i++)
            {
                try
                {
                    if (textBox4.Lines[i].Substring(0, 10) == "Movement #")
                    {
                        movementCount++;
                        movementLines.Add(i);
                    }
                }
                catch { }
            }
            if (scriptCount == 0)
            {
                System.IO.BinaryWriter writeZero = new System.IO.BinaryWriter(File.Create(workingFolder + @"data\a\0\5\scripts" + "\\" + comboBox5.SelectedIndex.ToString("D4")));
                writeZero.Write((int)0xFD13);
                writeZero.Close();
            }
            else
            {
                progressBar2.Maximum = (scriptCount + functionCount + movementCount) * 2;
                progressBar2.Value = 0;
                int[] scriptLength = new int[scriptCount];
                currentLine = 0;
                for (int i = 0; i < scriptCount; i++)
                {
                    scriptLength[i] = GetVScriptLength(scriptLines[i], false, scriptCount, functionCount, movementCount, i);
                    progressBar2.Value++;
                }
                int[] functionLength = new int[functionCount];
                currentLine = 0;
                for (int i = 0; i < functionCount; i++)
                {
                    functionLength[i] = GetVScriptLength(functionLines[i], true, scriptCount, functionCount, movementCount, i);
                    progressBar2.Value++;
                }
                int[] movementLength = new int[movementCount];
                currentLine = 0;
                fixMovOffset.Clear();
                for (int i = 0; i < movementCount; i++)
                {
                    movementLength[i] = GetMovementLength(movementLines[i]);
                    fixMovOffset.Add(false);
                    progressBar2.Value++;
                }
                int baseOffset = 2 + (scriptCount * 4);
                scriptOffsets.Add(0);
                functionOffsets.Add(0);
                movementOffsets.Add(0);
                for (int i = 0; i < scriptCount; i++)
                {
                    scriptOffsets.Add(baseOffset);
                    baseOffset += scriptLength[i];
                }
                for (int i = 0; i < functionCount; i++)
                {
                    functionOffsets.Add(baseOffset);
                    baseOffset += functionLength[i];
                }
                for (int i = 0; i < movementCount; i++)
                {
                    movementOffsets.Add(baseOffset);
                    baseOffset += movementLength[i];
                }
                System.IO.BinaryWriter write = new System.IO.BinaryWriter(File.Create(workingFolder + @"data\a\0\5\scripts" + "\\" + comboBox5.SelectedIndex.ToString("D4")));
                for (int i = 1; i < scriptCount + 1; i++)
                {
                    write.Write((Int32)(scriptOffsets[i] - (write.BaseStream.Position + 4)));
                }
                write.Write((UInt16)0xFD13);
                #region Write Scripts
                int currentIndex = 0;
                for (int i = 0; i < scriptCount; i++)
                {
                    currentLine = scriptLines[i];
                    bool special = false;
                    if (scriptLength[i] == 0)
                    {
                        long position = write.BaseStream.Position;
                        write.BaseStream.Position = 4 * i;
                        write.Write((Int32)(scriptOffsets[useIndex[currentIndex]] - (write.BaseStream.Position + 4)));
                        write.BaseStream.Position = position;
                        currentIndex++;
                        special = true;
                    }
                    while (!special)
                    {
                        try
                        {
                            string[] cmd = textBox2.Lines[currentLine].Split(' ');
                            if (scriptData.GetString(cmd[0]) == null && scriptNameW.GetString(cmd[0]) == null)
                            {
                            }
                            else
                            {
                                byte output8;
                                UInt16 output16;
                                UInt32 output32;
                                NumberStyles hex = NumberStyles.HexNumber;
                                CultureInfo invar = CultureInfo.InvariantCulture;
                                if (scriptData.GetString(cmd[0]) == null)
                                {
                                    cmd[0] = scriptNameW.GetString(cmd[0]);
                                }
                                write.Write((UInt16)Convert.ToUInt16(cmd[0], 16));
                                if (scriptData.GetString(cmd[0]) != "0")
                                {
                                    if (cmd[0] == "001E" || cmd[0] == "0004") // Jump Call
                                    {
                                        if (cmd.Count() >= 2 && cmd[1].Substring(0, 8) == "Script_#" && UInt32.TryParse(cmd[1].Substring(8), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[1].Substring(8)) < scriptCount + 1 && Convert.ToUInt32(cmd[1].Substring(8)) > 0)
                                        {
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToInt32(scriptOffsets[Convert.ToInt32(cmd[1].Substring(8))] - position));
                                            if (cmd[0] == "001E")
                                            {
                                                currentLine++;
                                                break;
                                            }
                                        }
                                        else if (cmd.Count() >= 2 && cmd[1].Substring(0, 10) == "Function_#" && UInt32.TryParse(cmd[1].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[1].Substring(10)) < functionCount + 1 && Convert.ToUInt32(cmd[1].Substring(10)) > 0)
                                        {
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToInt32(functionOffsets[Convert.ToInt32(cmd[1].Substring(10))] - position));
                                            if (cmd[0] == "001E")
                                            {
                                                currentLine++;
                                                break;
                                            }
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    else if (cmd[0] == "001F") // If
                                    {
                                        if (cmd.Count() >= 3 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && cmd[2].Substring(0, 8) == "Script_#" && UInt32.TryParse(cmd[2].Substring(8), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(8)) < scriptCount + 1 && Convert.ToUInt32(cmd[2].Substring(8)) > 0)
                                        {
                                            write.Write((byte)Convert.ToByte(cmd[1], 16));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToInt32(scriptOffsets[Convert.ToInt32(cmd[2].Substring(8))] - position));
                                        }
                                        else if (cmd.Count() >= 3 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && cmd[2].Substring(0, 10) == "Function_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) < functionCount + 1 && Convert.ToUInt32(cmd[2].Substring(10)) > 0)
                                        {
                                            write.Write((byte)Convert.ToByte(cmd[1], 16));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToInt32(functionOffsets[Convert.ToInt32(cmd[2].Substring(10))] - position));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    else if (cmd[0] == "0064" && gameID != 0x4A425249 && gameID != 0x4A415249 && gameID != 0x4A455249 && gameID != 0x4A445249)
                                    {
                                        if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && cmd[2].Substring(0, 10) == "Movement_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) <= movementCount && Convert.ToUInt32(cmd[2].Substring(10)) != 0)
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write((Int32)movementOffsets[Convert.ToInt32(cmd[2].Substring(10))] - position);
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    else if (cmd[0] == "0060" && (gameID == 0x4A425249 || gameID == 0x4A415249 || gameID == 0x4A455249 || gameID == 0x4A445249))
                                    {
                                        if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && cmd[2].Substring(0, 10) == "Movement_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) <= movementCount && Convert.ToUInt32(cmd[2].Substring(10)) != 0)
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write((Int32)movementOffsets[Convert.ToInt32(cmd[2].Substring(10))] - position);
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    else
                                    {
                                        string[] data = scriptData.GetString(cmd[0]).Split(' ');
                                        int variables = Convert.ToInt32(data[0]);
                                        if (variables <= cmd.Count() - 1)
                                        {
                                            int tempcount = 2;
                                            for (int j = 0; j < variables; j++)
                                            {
                                                if (data[j + 1] == "1" && byte.TryParse(cmd[j + 1].Substring(2), hex, invar, out output8))
                                                {
                                                    write.Write((byte)Convert.ToByte(cmd[j + 1], 16));
                                                    tempcount++;
                                                }
                                                else if (data[j + 1] == "2" && UInt16.TryParse(cmd[j + 1].Substring(2), hex, invar, out output16))
                                                {
                                                    write.Write((UInt16)Convert.ToUInt16(cmd[j + 1], 16));
                                                    tempcount += 2;
                                                }
                                                else if (data[j + 1] == "4" && UInt32.TryParse(cmd[j + 1].Substring(2), hex, invar, out output32))
                                                {
                                                    write.Write((UInt32)Convert.ToUInt32(cmd[j + 1], 16));
                                                    tempcount += 4;
                                                }
                                                else
                                                {
                                                    write.BaseStream.Position -= tempcount;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            write.BaseStream.Position -= 2;
                                        }
                                    }
                                }
                                else
                                {
                                    if (cmd[0] == "0002" || cmd[0] == "0005" || cmd[0] == "001D")
                                    {
                                        currentLine++;
                                        break;
                                    }
                                    else if (cmd[0] == "0003")
                                    {
                                        if (UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                            currentLine++;
                                            break;
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                }
                            }
                        }
                        catch { }
                        if (currentLine == textBox2.Lines.Count() - 1)
                        {
                            write.Write((UInt16)0x0002);
                            break;
                        }
                        else
                        {
                            currentLine++;
                        }
                    }
                    progressBar2.Value++;
                }
                #endregion
                #region Write Functions
                for (int i = 0; i < functionCount; i++)
                {
                    currentLine = functionLines[i];
                    while (true)
                    {
                        try
                        {
                            string[] cmd = textBox3.Lines[currentLine].Split(' ');
                            if (scriptData.GetString(cmd[0]) == null && scriptNameW.GetString(cmd[0]) == null)
                            {
                            }
                            else
                            {
                                byte output8;
                                UInt16 output16;
                                UInt32 output32;
                                NumberStyles hex = NumberStyles.HexNumber;
                                CultureInfo invar = CultureInfo.InvariantCulture;
                                if (scriptData.GetString(cmd[0]) == null)
                                {
                                    cmd[0] = scriptNameW.GetString(cmd[0]);
                                }
                                write.Write((UInt16)Convert.ToUInt16(cmd[0], 16));
                                if (scriptData.GetString(cmd[0]) != "0")
                                {
                                    if (cmd[0] == "001E" || cmd[0] == "0004") // Jump Call
                                    {
                                        if (cmd.Count() >= 2 && cmd[1].Substring(0, 8) == "Script_#" && UInt32.TryParse(cmd[1].Substring(8), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[1].Substring(8)) < scriptCount + 1 && Convert.ToUInt32(cmd[1].Substring(8)) > 0)
                                        {
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToInt32(scriptOffsets[Convert.ToInt32(cmd[1].Substring(8))] - position));
                                            if (cmd[0] == "001E")
                                            {
                                                currentLine++;
                                                break;
                                            }
                                        }
                                        else if (cmd.Count() >= 2 && cmd[1].Substring(0, 10) == "Function_#" && UInt32.TryParse(cmd[1].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[1].Substring(10)) < functionCount + 1 && Convert.ToUInt32(cmd[1].Substring(10)) > 0)
                                        {
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToInt32(functionOffsets[Convert.ToInt32(cmd[1].Substring(10))] - position));
                                            if (cmd[0] == "001E")
                                            {
                                                currentLine++;
                                                break;
                                            }
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    else if (cmd[0] == "001F") // If
                                    {
                                        if (cmd.Count() >= 3 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && cmd[2].Substring(0, 8) == "Script_#" && UInt32.TryParse(cmd[2].Substring(8), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(8)) < scriptCount + 1 && Convert.ToUInt32(cmd[2].Substring(8)) > 0)
                                        {
                                            write.Write((byte)Convert.ToByte(cmd[1], 16));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToInt32(scriptOffsets[Convert.ToInt32(cmd[2].Substring(8))] - position));
                                        }
                                        else if (cmd.Count() >= 3 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && cmd[2].Substring(0, 10) == "Function_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) < functionCount + 1 && Convert.ToUInt32(cmd[2].Substring(10)) > 0)
                                        {
                                            write.Write((byte)Convert.ToByte(cmd[1], 16));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write(Convert.ToInt32(functionOffsets[Convert.ToInt32(cmd[2].Substring(10))] - position));
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    else if (cmd[0] == "0064" && gameID != 0x4A425249 && gameID != 0x4A415249 && gameID != 0x4A455249 && gameID != 0x4A445249)
                                    {
                                        if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && cmd[2].Substring(0, 10) == "Movement_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) <= movementCount && Convert.ToUInt32(cmd[2].Substring(10)) != 0)
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write((Int32)movementOffsets[Convert.ToInt32(cmd[2].Substring(10))] - position);
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    else if (cmd[0] == "0060" && (gameID == 0x4A425249 || gameID == 0x4A415249 || gameID == 0x4A455249 || gameID == 0x4A445249))
                                    {
                                        if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && cmd[2].Substring(0, 10) == "Movement_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) <= movementCount && Convert.ToUInt32(cmd[2].Substring(10)) != 0)
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                            int position = (int)write.BaseStream.Position + 4;
                                            write.Write((Int32)movementOffsets[Convert.ToInt32(cmd[2].Substring(10))] - position);
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                    else
                                    {
                                        string[] data = scriptData.GetString(cmd[0]).Split(' ');
                                        int variables = Convert.ToInt32(data[0]);
                                        if (variables <= cmd.Count() - 1)
                                        {
                                            int tempcount = 2;
                                            for (int j = 0; j < variables; j++)
                                            {
                                                if (data[j + 1] == "1" && byte.TryParse(cmd[j + 1].Substring(2), hex, invar, out output8))
                                                {
                                                    write.Write((byte)Convert.ToByte(cmd[j + 1], 16));
                                                    tempcount++;
                                                }
                                                else if (data[j + 1] == "2" && UInt16.TryParse(cmd[j + 1].Substring(2), hex, invar, out output16))
                                                {
                                                    write.Write((UInt16)Convert.ToUInt16(cmd[j + 1], 16));
                                                    tempcount += 2;
                                                }
                                                else if (data[j + 1] == "4" && UInt32.TryParse(cmd[j + 1].Substring(2), hex, invar, out output32))
                                                {
                                                    write.Write((UInt32)Convert.ToUInt32(cmd[j + 1], 16));
                                                    tempcount += 4;
                                                }
                                                else
                                                {
                                                    write.BaseStream.Position -= tempcount;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            write.BaseStream.Position -= 2;
                                        }
                                    }
                                }
                                else
                                {
                                    if (cmd[0] == "0002" || cmd[0] == "0005" || cmd[0] == "001D")
                                    {
                                        currentLine++;
                                        break;
                                    }
                                    else if (cmd[0] == "0003")
                                    {
                                        if (UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                        {
                                            write.Write((UInt16)Convert.ToUInt16(cmd[2], 16));
                                            currentLine++;
                                            break;
                                        }
                                        else write.BaseStream.Position -= 2;
                                    }
                                }
                            }
                        }
                        catch { }
                        if (currentLine == textBox3.Lines.Count() - 1)
                        {
                            write.Write((UInt16)0x0002);
                            break;
                        }
                        else
                        {
                            currentLine++;
                        }
                    }
                    progressBar2.Value++;
                }
                #endregion
                #region Write Movements
                for (int i = 0; i < movementCount; i++)
                {
                    if (fixMovOffset[i] == true) write.Write((byte)0xFF);
                    currentLine = movementLines[i];
                    while (true)
                    {
                        try
                        {
                            string[] cmd = textBox4.Lines[currentLine].Split(' ');
                            int output;
                            if ((!int.TryParse(cmd[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output) || (Convert.ToUInt16(cmd[0], 16) < 0 && Convert.ToUInt16(cmd[0], 16) > 0x00FE)) && MovementNameW.GetString(cmd[0]) == null)
                            {
                            }
                            else
                            {
                                if (!int.TryParse(cmd[0], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out output))
                                {
                                    cmd[0] = MovementNameW.GetString(cmd[0]);
                                }
                                write.Write((UInt16)Convert.ToUInt16(cmd[0], 16));
                                if (cmd.Count() >= 2)
                                {
                                    write.Write((UInt16)Convert.ToUInt16(cmd[1], 16));
                                }
                                else
                                {
                                    if (cmd[0] == "End" || cmd[0] == "00FE")
                                    {
                                        currentLine++;
                                        break;
                                    }
                                    else
                                    {
                                        write.BaseStream.Position -= 2;
                                    }
                                }
                            }
                        }
                        catch { }
                        if (currentLine == textBox4.Lines.Count() - 1)
                        {
                            write.Write((UInt16)0xFE);
                            break;
                        }
                        else
                        {
                            currentLine++;
                        }
                    }
                    progressBar2.Value++;
                }
                #endregion
                write.Write((UInt16)0);
                if (write.BaseStream.Length % 2 == 1) write.Write((byte)0);
                write.Close();
                progressBar2.Value = progressBar2.Maximum;
            }
        }

        private int GetVScriptLength(int line, bool function, int scriptCount, int functionCount, int movementCount, int currentIndex)
        {
            int length = 0;
            currentLine = line + 1;
            while (true)
            {
                try
                {
                    string[] cmd;
                    if (!function) cmd = textBox2.Lines[currentLine].Split(' ');
                    else cmd = textBox3.Lines[currentLine].Split(' ');
                    if (scriptData.GetString(cmd[0]) == null && scriptNameW.GetString(cmd[0]) == null)
                    {
                        uint output;
                        if (!function && length == 0 && cmd[0].Substring(0, 11) == "UseScript_#" && UInt32.TryParse(cmd[0].Substring(11), NumberStyles.Any, CultureInfo.InvariantCulture, out output) && Convert.ToUInt32(cmd[0].Substring(11)) < currentIndex + 1 && Convert.ToUInt32(cmd[0].Substring(11)) > 0)
                        {
                            useIndex.Add((int)Convert.ToUInt32(cmd[0].Substring(11)));
                            break;
                        }
                    }
                    else
                    {
                        byte output8;
                        UInt16 output16;
                        UInt32 output32;
                        NumberStyles hex = NumberStyles.HexNumber;
                        CultureInfo invar = CultureInfo.InvariantCulture;
                        if (scriptData.GetString(cmd[0]) == null)
                        {
                            cmd[0] = scriptNameW.GetString(cmd[0]);
                        }
                        length += 2;
                        if (scriptData.GetString(cmd[0]) != "0")
                        {
                            if (cmd[0] == "001E" || cmd[0] == "0004") // Jump Call
                            {
                                if (cmd.Count() >= 2 && cmd[1].Substring(0, 8) == "Script_#" && UInt32.TryParse(cmd[1].Substring(8), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[1].Substring(8)) < scriptCount + 1 && Convert.ToUInt32(cmd[1].Substring(8)) > 0)
                                {
                                    length += 4;
                                    if (cmd[0] == "001E")
                                    {
                                        currentLine++;
                                        break;
                                    }
                                }
                                else if (cmd.Count() >= 2 && cmd[1].Substring(0, 10) == "Function_#" && UInt32.TryParse(cmd[1].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[1].Substring(10)) < functionCount + 1 && Convert.ToUInt32(cmd[1].Substring(10)) > 0)
                                {
                                    length += 4;
                                    if (cmd[0] == "001E")
                                    {
                                        currentLine++;
                                        break;
                                    }
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "001F") // If
                            {
                                if (cmd.Count() >= 3 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && cmd[2].Substring(0, 8) == "Script_#" && UInt32.TryParse(cmd[2].Substring(8), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(8)) < scriptCount + 1 && Convert.ToUInt32(cmd[2].Substring(8)) > 0)
                                {
                                    length++;
                                    length += 4;
                                }
                                else if (cmd.Count() >= 3 && byte.TryParse(cmd[1].Substring(2), hex, invar, out output8) && cmd[2].Substring(0, 10) == "Function_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) < functionCount + 1 && Convert.ToUInt32(cmd[2].Substring(10)) > 0)
                                {
                                    length++;
                                    length += 4;
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "0064" && gameID != 0x4A425249 && gameID != 0x4A415249 && gameID != 0x4A455249 && gameID != 0x4A445249)
                            {
                                if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && cmd[2].Substring(0, 10) == "Movement_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) <= movementCount && Convert.ToUInt32(cmd[2].Substring(10)) != 0)
                                {
                                    length += 2;
                                    length += 4;
                                }
                                else length -= 2;
                            }
                            else if (cmd[0] == "0060" && (gameID == 0x4A425249 || gameID == 0x4A415249 || gameID == 0x4A455249 || gameID == 0x4A445249))
                            {
                                if (cmd.Count() >= 3 && UInt16.TryParse(cmd[1].Substring(2), hex, invar, out output16) && cmd[2].Substring(0, 10) == "Movement_#" && UInt32.TryParse(cmd[2].Substring(10), NumberStyles.Any, invar, out output32) && Convert.ToUInt32(cmd[2].Substring(10)) <= movementCount && Convert.ToUInt32(cmd[2].Substring(10)) != 0)
                                {
                                    length += 2;
                                    length += 4;
                                }
                                else length -= 2;
                            }
                            else
                            {
                                string[] data = scriptData.GetString(cmd[0]).Split(' ');
                                int variables = Convert.ToInt32(data[0]);
                                if (variables <= cmd.Count() - 1)
                                {
                                    int tempcount = 2;
                                    for (int j = 0; j < variables; j++)
                                    {
                                        if (data[j + 1] == "1" && byte.TryParse(cmd[j + 1].Substring(2), hex, invar, out output8))
                                        {
                                            length++;
                                            tempcount++;
                                        }
                                        else if (data[j + 1] == "2" && UInt16.TryParse(cmd[j + 1].Substring(2), hex, invar, out output16))
                                        {
                                            length += 2;
                                            tempcount += 2;
                                        }
                                        else if (data[j + 1] == "4" && UInt32.TryParse(cmd[j + 1].Substring(2), hex, invar, out output32))
                                        {
                                            length += 4;
                                            tempcount += 4;
                                        }
                                        else
                                        {
                                            length -= tempcount;
                                        }
                                    }
                                }
                                else
                                {
                                    length -= 2;
                                }
                            }
                        }
                        else
                        {
                            if (cmd[0] == "0002" || cmd[0] == "0005" || cmd[0] == "001D")
                            {
                                currentLine++;
                                break;
                            }
                            else if (cmd[0] == "0003")
                            {
                                if (UInt16.TryParse(cmd[2].Substring(2), hex, invar, out output16))
                                {
                                    length += 2;
                                    currentLine++;
                                    break;
                                }
                                else
                                {
                                    length -= 2;
                                }
                            }
                        }                        
                    }
                }
                catch { }
                if (!function && currentLine == textBox2.Lines.Count() - 1)
                {
                    length += 2;
                    break;
                }
                else if (function && currentLine == textBox3.Lines.Count() - 1)
                {
                    length += 2;
                    break;
                }
                else
                {
                    currentLine++;
                }
            }
            return length;
        }

        #endregion

        private void btnSearchScript_Click(object sender, EventArgs e)
        {
            searchScripts();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                //
            }
        }
    }
}
