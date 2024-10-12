using System.ComponentModel;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic.FileIO;
using SearchOption = System.IO.SearchOption;

namespace ModManager;

public class GUI : Form
{
    private readonly IContainer components = null;

    private Label label1;

    private Label label2;

    private CheckedListBox modList;

    private Panel panel1;

    private Panel panel2;

    private ListBox fileList;

    private Button RenameBtn;

    private Panel panel3;

    private Button InstallBtn;

    private Button HelpBtn;

    private Label label3;

    private Button RestoreBtn;

    private Button DeleteBtn;

    private Panel panel4;

    private TextBox DebugBox;

    public GUI()
    {
        InitializeComponent();
        Init();
    }

    private void Init()
    {
        Directory.CreateDirectory("mods");
        Settings.Load();
        Debug("Ready.");
        UpdateModList();
    }

    private void ModList_SelectedIndexChanged(object sender, EventArgs e)
    {
        fileList.Items.Clear();
        if (modList.SelectedIndex > -1)
        {
            string modName = (string)modList.SelectedItem;
            string[] files = Directory.GetFiles($"mods\\{modName}", "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string item = file.Replace($"mods\\{modName}\\", "");
                fileList.Items.Add(item);
            }
        }
    }

    private void AddNewMod(string[] path)
    {
        string name = "New Mod";
        DialogResult dialogResult = InputBox("New Mod Entry", "Enter the name of the new mod:", ref name);
        if (dialogResult != DialogResult.OK)
        {
            return;
        }
        if (Regex.IsMatch(name, "[/\\?%*:|\"<>.]"))
        {
            MessageBox.Show("Please keep special characters out of mod names!");
            AddNewMod(path);
            return;
        }
        List<string> list = [.. Directory.GetDirectories("mods")];
        if (list.Exists((string d) => d.EndsWith($"mods\\{name}")))
        {
            MessageBox.Show("ERROR: Mod already exists with that name.");
            AddNewMod(path);
        }
        else
        {
            ImportMod(path, name.Trim());
        }
    }

    private void ImportMod(string[] path, string modName)
    {
        Directory.CreateDirectory($"mods\\{modName}");
        foreach (string source in path)
        {
            FileAttributes attributes = File.GetAttributes(source);
            if (attributes.HasFlag(FileAttributes.Directory))
            {
                string target = $"mods\\{modName}\\{source[(source.LastIndexOf('\\') + 1)..]}";
                FileSystem.CopyDirectory(source, target);
            }
            else
            {
                File.Copy(source, $"mods\\{modName}\\{Path.GetFileName(source)}");
            }
        }
        UpdateModList();
    }

    private void UpdateModList()
    {
        modList.Items.Clear();
        string[] mods = (from e in Directory.GetDirectories("mods") select Path.GetFileName(e)).ToArray();
        Array.Sort(mods);
        foreach (string modName in mods)
        {
            modList.Items.Add(modName, isChecked: Settings.InstalledMods.Contains(modName));
        }
    }

    public static DialogResult InputBox(string title, string promptText, ref string value)
    {
        Form form = new();
        Label label = new();
        TextBox textBox = new();
        Button button = new();
        Button button2 = new();
        form.Text = title;
        label.Text = promptText;
        textBox.Text = value;
        button.Text = "OK";
        button2.Text = "Cancel";
        button.DialogResult = DialogResult.OK;
        button2.DialogResult = DialogResult.Cancel;
        label.SetBounds(9, 20, 372, 13);
        textBox.SetBounds(12, 36, 372, 20);
        button.SetBounds(228, 72, 75, 23);
        button2.SetBounds(309, 72, 75, 23);
        label.AutoSize = true;
        textBox.Anchor |= AnchorStyles.Right;
        button.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        button2.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
        form.ClientSize = new Size(396, 107);
        form.Controls.AddRange([label, textBox, button, button2]);
        form.ClientSize = new Size(Math.Max(300, label.Right + 10), form.ClientSize.Height);
        form.FormBorderStyle = FormBorderStyle.FixedDialog;
        form.StartPosition = FormStartPosition.CenterScreen;
        form.MinimizeBox = false;
        form.MaximizeBox = false;
        form.AcceptButton = button;
        form.CancelButton = button2;
        DialogResult result = form.ShowDialog();
        value = textBox.Text;
        return result;
    }

    private void Debug(string s)
    {
        if (InvokeRequired)
        {
            Invoke(new MethodInvoker(updateGUI));
        }
        else
        {
            updateGUI();
        }
        void updateGUI()
        {
            TextBox debugBox = DebugBox;
            debugBox.Text = debugBox.Text + s + Environment.NewLine;
        }
    }

    private void InstallBtn_Click(object sender, EventArgs e)
    {
        Debug("Setting up mods...");
        RemoveAllMods();
        InstallAllMods();
        Settings.Save();
        Debug("Mods successfully installed.");
        MessageBox.Show("You can launch the modded game!");
        UpdateModList();
    }

    private void InstallAllMods()
    {
        List<string> modFolders = [];
        List<(string, string)> modFiles = [];
        bool conflict = false;
        for (int i = 0; i < modList.Items.Count; i++)
        {
            if (!modList.GetItemChecked(i))
            {
                continue;
            }
            string modName = (string)modList.Items[i];
            string[] sourceFolders = Directory.GetDirectories($"mods\\{modName}", "*", SearchOption.AllDirectories);
            foreach (string source in sourceFolders)
            {
                string target = source.Replace($"mods\\{modName}\\", "");
                modFolders.Add(target);
            }
            string[] sourceFiles = Directory.GetFiles($"mods\\{modName}", "*", SearchOption.AllDirectories);
            foreach (string source in sourceFiles)
            {
                string target = source.Replace($"mods\\{modName}\\", "");
                if (modFiles.Any(file => file.Item2 == target))
                {
                    conflict = true;
                }
                else
                {
                    modFiles.Add((source, target));
                }
            }
            Settings.InstalledMods.Add((string)modList.Items[i]);
        }
        if (conflict)
        {
            DialogResult dialogResult = MessageBox.Show("Multiple mods are set to modify the same files, so some of them won't work as intended.\n\nContinue anyway?", "Warning", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes || dialogResult == DialogResult.OK)
            {
                conflict = false;
            }
        }
        if (conflict)
        {
            Settings.InstalledMods.Clear();
            return;
        }
        foreach (var folder in modFolders)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
                Settings.AdditionalFolders.Add(folder);
            }
        }
        Settings.AdditionalFolders.Reverse();
        foreach (var (source, target) in modFiles)
        {
            string backup = target + ".dsmmbak";
            if (File.Exists(target))
            {
                File.Move(target, backup);
            }
            else
            {
                Settings.AdditionalFiles.Add(target);
            }
            File.Copy(source, target, overwrite: true);
        }
    }

    private void RestoreBtn_Click(object sender, EventArgs e)
    {
        Debug("Restoring original files...");
        RemoveAllMods();
        Settings.Save();
        Debug("Backups restored.");
        MessageBox.Show("You can launch the original game!");
        UpdateModList();
    }

    private void RemoveAllMods()
    {
        var targets = Directory.GetFiles(".\\", "*", SearchOption.AllDirectories);
        foreach (string target in targets)
        {
            string backup = target + ".dsmmbak";
            if (File.Exists(backup))
            {
                File.SetAttributes(target, FileAttributes.Normal);
                File.Delete(target);
                File.Move(backup, target);
            }
        }
        Settings.InstalledMods.Clear();

        foreach (string file in Settings.AdditionalFiles)
        {
            if (File.Exists(file))
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }
        }
        Settings.AdditionalFiles.Clear();

        foreach (string folder in Settings.AdditionalFolders)
        {
            if (Directory.Exists(folder) && !Directory.EnumerateFileSystemEntries(folder).Any())
            {
                Directory.Delete(folder);
            }
        }
        Settings.AdditionalFolders.Clear();
    }

    private void GUI_DragEnter(object sender, DragEventArgs e)
    {
        DragDropEffects effect = DragDropEffects.None;
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string path = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            if (Directory.Exists(path) || File.Exists(path))
            {
                effect = DragDropEffects.Copy;
            }
        }
        e.Effect = effect;
    }

    private void GUI_DragDrop(object sender, DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            return;
        }
        string[] data = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (data.Length > 1)
        {
            AddNewMod(data);
        }
        else if (data.Length == 1)
        {
            FileAttributes attributes = File.GetAttributes(data[0]);
            if (attributes.HasFlag(FileAttributes.Directory))
            {
                string[] directories = Directory.GetDirectories(data[0], "*", SearchOption.TopDirectoryOnly);
                string[] files = Directory.GetFiles(data[0], "*", SearchOption.TopDirectoryOnly);
                AddNewMod([.. directories, .. files]);
            }
            else
            {
                AddNewMod(data);
            }
        }
    }

    private void RenameBtn_Click(object sender, EventArgs e)
    {
        int selectedIndex = modList.SelectedIndex;
        if (selectedIndex <= -1)
        {
            return;
        }
        string oldName = (string)modList.SelectedItem;
        string newName = oldName;
        DialogResult dialogResult = InputBox("Rename Mod", "Enter a new name for mod \"" + newName + "\":", ref newName);
        if (dialogResult == DialogResult.OK || dialogResult == DialogResult.Yes)
        {
            try
            {
                Directory.Move("mods\\" + oldName, "mods\\" + newName);
                if (Settings.InstalledMods.Contains(oldName))
                {
                    Settings.InstalledMods.Remove(oldName);
                    Settings.InstalledMods.Add(newName);
                    Settings.Save();
                }
                UpdateModList();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error renaming mod.\n--\n" + ex.ToString());
            }
        }
    }

    private void HelpBtn_Click(object sender, EventArgs e)
    {
        List<string> list =
        [
            "To add a mod, drag a folder or group of folders into this window. Make sure the directory structure mirrors that of the DSR directory.",
            "Check the boxes next to any mods you want, then click INSTALL. This will install the selected mods into the game.",
            "To restore the game to its original version, click the RESTORE button.",
        ];
        MessageBox.Show(string.Join("\n\n", [.. list]));
    }

    private void DeleteBtn_Click(object sender, EventArgs e)
    {
        int selectedIndex = modList.SelectedIndex;
        if (selectedIndex > -1)
        {
            DialogResult dialogResult = MessageBox.Show("Really delete this mod? You can't undo this.", "Confirm", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes || dialogResult == DialogResult.OK)
            {
                string modName = (string)modList.SelectedItem;
                Directory.Delete($"mods\\{modName}", recursive: true);
                UpdateModList();
                if (Settings.InstalledMods.Contains(modName))
                {
                    Debug("Resetting mods...");
                    RemoveAllMods();
                    InstallAllMods();
                    Settings.Save();
                    Debug("Mods successfully reset.");
                }
                ModList_SelectedIndexChanged(sender, e);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        label1 = new Label();
        label2 = new Label();
        modList = new CheckedListBox();
        panel1 = new Panel();
        panel2 = new Panel();
        fileList = new ListBox();
        RenameBtn = new Button();
        panel3 = new Panel();
        DeleteBtn = new Button();
        RestoreBtn = new Button();
        InstallBtn = new Button();
        HelpBtn = new Button();
        label3 = new Label();
        panel4 = new Panel();
        DebugBox = new TextBox();
        panel1.SuspendLayout();
        panel2.SuspendLayout();
        panel3.SuspendLayout();
        panel4.SuspendLayout();
        SuspendLayout();
        label1.Location = new Point(19, 57);
        label1.Name = "label1";
        label1.Size = new Size(238, 23);
        label1.TabIndex = 5;
        label1.Text = "Available Mods";
        label1.TextAlign = ContentAlignment.MiddleCenter;
        label2.Location = new Point(263, 57);
        label2.Name = "label2";
        label2.Size = new Size(359, 23);
        label2.TabIndex = 8;
        label2.Text = "Mod Files";
        label2.TextAlign = ContentAlignment.MiddleCenter;
        modList.Dock = DockStyle.Fill;
        modList.FormattingEnabled = true;
        modList.Location = new Point(0, 0);
        modList.Margin = new Padding(0);
        modList.Name = "modList";
        modList.Size = new Size(238, 499);
        modList.TabIndex = 10;
        modList.SelectedIndexChanged += new EventHandler(ModList_SelectedIndexChanged);
        panel1.Controls.Add(modList);
        panel1.Location = new Point(19, 84);
        panel1.Name = "panel1";
        panel1.Size = new Size(238, 499);
        panel1.TabIndex = 11;
        panel2.Controls.Add(fileList);
        panel2.Location = new Point(266, 84);
        panel2.Margin = new Padding(0);
        panel2.Name = "panel2";
        panel2.Size = new Size(356, 327);
        panel2.TabIndex = 12;
        fileList.Dock = DockStyle.Fill;
        fileList.FormattingEnabled = true;
        fileList.ItemHeight = 16;
        fileList.Location = new Point(0, 0);
        fileList.Name = "fileList";
        fileList.Size = new Size(356, 327);
        fileList.TabIndex = 8;
        RenameBtn.Location = new Point(3, 3);
        RenameBtn.Name = "RenameBtn";
        RenameBtn.Size = new Size(123, 27);
        RenameBtn.TabIndex = 13;
        RenameBtn.Text = "Rename";
        RenameBtn.UseVisualStyleBackColor = true;
        RenameBtn.Click += new EventHandler(RenameBtn_Click);
        panel3.Controls.Add(DeleteBtn);
        panel3.Controls.Add(RestoreBtn);
        panel3.Controls.Add(InstallBtn);
        panel3.Controls.Add(RenameBtn);
        panel3.Location = new Point(19, 589);
        panel3.Name = "panel3";
        panel3.Size = new Size(1008, 33);
        panel3.TabIndex = 14;
        DeleteBtn.Location = new Point(132, 3);
        DeleteBtn.Name = "DeleteBtn";
        DeleteBtn.Size = new Size(106, 27);
        DeleteBtn.TabIndex = 17;
        DeleteBtn.Text = "Delete";
        DeleteBtn.UseVisualStyleBackColor = true;
        DeleteBtn.Click += new EventHandler(DeleteBtn_Click);
        RestoreBtn.Location = new Point(382, 3);
        RestoreBtn.Name = "RestoreBtn";
        RestoreBtn.Size = new Size(106, 27);
        RestoreBtn.TabIndex = 16;
        RestoreBtn.Text = "RESTORE";
        RestoreBtn.UseVisualStyleBackColor = true;
        RestoreBtn.Click += new EventHandler(RestoreBtn_Click);
        InstallBtn.Location = new Point(494, 3);
        InstallBtn.Name = "InstallBtn";
        InstallBtn.Size = new Size(106, 27);
        InstallBtn.TabIndex = 15;
        InstallBtn.Text = "INSTALL";
        InstallBtn.UseVisualStyleBackColor = true;
        InstallBtn.Click += new EventHandler(InstallBtn_Click);
        HelpBtn.Location = new Point(547, 12);
        HelpBtn.Name = "HelpBtn";
        HelpBtn.Size = new Size(75, 30);
        HelpBtn.TabIndex = 15;
        HelpBtn.Text = "Help";
        HelpBtn.UseVisualStyleBackColor = true;
        HelpBtn.Click += new EventHandler(HelpBtn_Click);
        label3.AutoSize = true;
        label3.Font = new Font("Microsoft Sans Serif", 7.8f, FontStyle.Bold, GraphicsUnit.Point, 0);
        label3.Location = new Point(19, 19);
        label3.Name = "label3";
        label3.Size = new Size(306, 17);
        label3.TabIndex = 16;
        label3.Text = "Drag your mod folder(s) into this window.";
        panel4.Controls.Add(DebugBox);
        panel4.Location = new Point(266, 417);
        panel4.Name = "panel4";
        panel4.Size = new Size(356, 151);
        panel4.TabIndex = 18;
        DebugBox.BackColor = SystemColors.Desktop;
        DebugBox.Dock = DockStyle.Fill;
        DebugBox.ForeColor = SystemColors.Window;
        DebugBox.Location = new Point(0, 0);
        DebugBox.Multiline = true;
        DebugBox.Name = "DebugBox";
        DebugBox.ReadOnly = true;
        DebugBox.Size = new Size(356, 151);
        DebugBox.TabIndex = 17;
        AllowDrop = true;
        AutoScaleDimensions = new SizeF(8f, 16f);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(635, 634);
        Controls.Add(panel4);
        Controls.Add(label3);
        Controls.Add(HelpBtn);
        Controls.Add(panel3);
        Controls.Add(panel2);
        Controls.Add(panel1);
        Controls.Add(label2);
        Controls.Add(label1);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        Icon = new Icon(typeof(GUI), "Icon.ico");
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "GUI";
        Text = "Mod Manager";
        DragDrop += new DragEventHandler(GUI_DragDrop);
        DragEnter += new DragEventHandler(GUI_DragEnter);
        panel1.ResumeLayout(false);
        panel2.ResumeLayout(false);
        panel3.ResumeLayout(false);
        panel4.ResumeLayout(false);
        panel4.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
