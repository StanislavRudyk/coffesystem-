namespace Cafe.Launcher
{
    partial class LauncherForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            txtUsername = new Cafe.Launcher.UI.VelvetInput();
            txtPassword = new Cafe.Launcher.UI.VelvetInput();
            btnLogin = new Cafe.Launcher.UI.EmberActionButton();
            lblUsername = new Label();
            lblPassword = new Label();
            btnSettings = new Label();
            nightControlBox = new ReaLTaiizor.Controls.NightControlBox();
            SuspendLayout();
            // 
            // txtUsername
            // 
            txtUsername.BackColor = Color.Transparent;
            txtUsername.Location = new Point(170, 253);
            txtUsername.Name = "txtUsername";
            txtUsername.PlaceholderText = "Введіть логін...";
            txtUsername.Size = new Size(460, 60);
            txtUsername.TabIndex = 0;
            txtUsername.UseSystemPasswordChar = false;
            txtUsername.Click += txtUsername_Click;
            // 
            // txtPassword
            // 
            txtPassword.BackColor = Color.Transparent;
            txtPassword.Location = new Point(170, 335);
            txtPassword.Name = "txtPassword";
            txtPassword.PlaceholderText = "Введіть пароль...";
            txtPassword.Size = new Size(460, 60);
            txtPassword.TabIndex = 1;
            txtPassword.UseSystemPasswordChar = true;
            // 
            // btnLogin
            // 
            btnLogin.BackColor = Color.Transparent;
            btnLogin.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold);
            btnLogin.ForeColor = Color.White;
            btnLogin.Location = new Point(170, 415);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(460, 60);
            btnLogin.TabIndex = 2;
            btnLogin.Text = "УВІЙТИ";
            btnLogin.Click += btnLogin_Click;
            // 
            // lblUsername
            // 
            lblUsername.BackColor = Color.Transparent;
            lblUsername.Font = new Font("Microsoft Sans Serif", 8.5F, FontStyle.Bold);
            lblUsername.ForeColor = Color.FromArgb(145, 220, 160, 120);
            lblUsername.Location = new Point(170, 222);
            lblUsername.Name = "lblUsername";
            lblUsername.Size = new Size(200, 20);
            lblUsername.TabIndex = 10;
            lblUsername.Text = "ЛОГІН";
            // 
            // lblPassword
            // 
            lblPassword.BackColor = Color.Transparent;
            lblPassword.Font = new Font("Microsoft Sans Serif", 8.5F, FontStyle.Bold);
            lblPassword.ForeColor = Color.FromArgb(145, 220, 160, 120);
            lblPassword.Location = new Point(170, 312);
            lblPassword.Name = "lblPassword";
            lblPassword.Size = new Size(200, 20);
            lblPassword.TabIndex = 11;
            lblPassword.Text = "ПАРОЛЬ";
            // 
            // btnSettings
            // 
            btnSettings.AutoSize = true;
            btnSettings.BackColor = Color.Transparent;
            btnSettings.Cursor = Cursors.Hand;
            btnSettings.Font = new Font("Segoe UI", 12F);
            btnSettings.ForeColor = Color.FromArgb(120, 211, 155, 120);
            btnSettings.Location = new Point(12, 12);
            btnSettings.Name = "btnSettings";
            btnSettings.Size = new Size(32, 21);
            btnSettings.TabIndex = 6;
            btnSettings.Text = "⚙";
            btnSettings.Click += btnSettings_Click;
            // 
            // nightControlBox
            // 
            nightControlBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nightControlBox.BackColor = Color.Transparent;
            nightControlBox.CloseHoverColor = Color.FromArgb(199, 80, 80);
            nightControlBox.CloseHoverForeColor = Color.White;
            nightControlBox.DefaultLocation = true;
            nightControlBox.DisableMaximizeColor = Color.FromArgb(105, 105, 105);
            nightControlBox.DisableMinimizeColor = Color.FromArgb(105, 105, 105);
            nightControlBox.EnableCloseColor = Color.FromArgb(160, 160, 160);
            nightControlBox.EnableMaximizeButton = true;
            nightControlBox.EnableMaximizeColor = Color.FromArgb(160, 160, 160);
            nightControlBox.EnableMinimizeButton = true;
            nightControlBox.EnableMinimizeColor = Color.FromArgb(160, 160, 160);
            nightControlBox.Location = new Point(677, 0);
            nightControlBox.MaximizeHoverColor = Color.FromArgb(15, 255, 255, 255);
            nightControlBox.MaximizeHoverForeColor = Color.White;
            nightControlBox.MinimizeHoverColor = Color.FromArgb(15, 255, 255, 255);
            nightControlBox.MinimizeHoverForeColor = Color.White;
            nightControlBox.Name = "nightControlBox";
            nightControlBox.Size = new Size(139, 31);
            nightControlBox.TabIndex = 24;
            // 
            // LauncherForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 520);
            Controls.Add(btnSettings);
            Controls.Add(btnLogin);
            Controls.Add(txtPassword);
            Controls.Add(lblPassword);
            Controls.Add(txtUsername);
            Controls.Add(lblUsername);
            Controls.Add(nightControlBox);
            FormBorderStyle = FormBorderStyle.None;
            Name = "LauncherForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Cafe System Velvet Edition";
            ResumeLayout(false);
            PerformLayout();

        }

        private Cafe.Launcher.UI.VelvetInput txtUsername;
        private Cafe.Launcher.UI.VelvetInput txtPassword;
        private Cafe.Launcher.UI.EmberActionButton btnLogin;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.Label btnSettings;
        private ReaLTaiizor.Controls.NightControlBox nightControlBox;
    }
}
