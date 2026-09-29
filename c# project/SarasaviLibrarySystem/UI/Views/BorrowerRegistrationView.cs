using System;
using System.Drawing;
using System.Windows.Forms;
using SarasaviLibrarySystem.Services;

namespace SarasaviLibrarySystem.UI.Views
{
    public class BorrowerRegistrationView : UserControl
    {
        private TextBox txtName = null!;
        private RadioButton rdoMale = null!;
        private RadioButton rdoFemale = null!;
        private TextBox txtNIC = null!;
        private TextBox txtAddress = null!;
        private Button btnRegister = null!;

        private DataGridView gridMembers = null!;

        public BorrowerRegistrationView()
        {
            InitializeComponent();
            LoadMembersList();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(24, 27, 36);

            var lblHeader = new Label
            {
                Text = "Borrower Member Registration",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 20),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            var pnlForm = new Panel
            {
                Location = new Point(25, 70),
                Size = new Size(520, 580),
                BackColor = Color.FromArgb(34, 39, 53)
            };

            int top = 25;

            pnlForm.Controls.Add(CreateLabel("Full Name *", 25, top));
            txtName = CreateTextBox(25, top + 25, 460);
            pnlForm.Controls.Add(txtName);
            top += 70;

            pnlForm.Controls.Add(CreateLabel("Gender (Sex) *", 25, top));
            rdoMale = new RadioButton
            {
                Text = "Male",
                Location = new Point(25, top + 25),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 10F)
            };
            rdoFemale = new RadioButton
            {
                Text = "Female",
                Location = new Point(140, top + 25),
                AutoSize = true,
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 10F)
            };
            pnlForm.Controls.Add(rdoMale);
            pnlForm.Controls.Add(rdoFemale);
            top += 70;

            pnlForm.Controls.Add(CreateLabel("National Identity Card (NIC) Number *", 25, top));
            txtNIC = CreateTextBox(25, top + 25, 460);
            pnlForm.Controls.Add(txtNIC);
            top += 70;

            pnlForm.Controls.Add(CreateLabel("Residential Address *", 25, top));
            txtAddress = new TextBox
            {
                Location = new Point(25, top + 25),
                Size = new Size(460, 100),
                Multiline = true,
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.FromArgb(45, 52, 71),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlForm.Controls.Add(txtAddress);
            top += 140;

            btnRegister = new Button
            {
                Text = "Register Borrower Member",
                Location = new Point(25, top),
                Size = new Size(460, 45),
                BackColor = Color.FromArgb(22, 160, 133),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;
            pnlForm.Controls.Add(btnRegister);

            this.Controls.Add(pnlForm);

            var pnlGrid = new Panel
            {
                Location = new Point(570, 70),
                Size = new Size(630, 580),
                BackColor = Color.FromArgb(34, 39, 53)
            };

            var lblGridHeader = new Label
            {
                Text = "Registered Library Borrowers List",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                Location = new Point(20, 20),
                AutoSize = true
            };
            pnlGrid.Controls.Add(lblGridHeader);

            gridMembers = new DataGridView
            {
                Location = new Point(20, 60),
                Size = new Size(590, 490),
                BackgroundColor = Color.FromArgb(24, 27, 36),
                BorderStyle = BorderStyle.None,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            gridMembers.EnableHeadersVisualStyles = false;
            gridMembers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 52, 71);
            gridMembers.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridMembers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            gridMembers.ColumnHeadersHeight = 35;
            gridMembers.DefaultCellStyle.BackColor = Color.FromArgb(24, 27, 36);
            gridMembers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 73, 94);
            gridMembers.DefaultCellStyle.SelectionForeColor = Color.White;

            pnlGrid.Controls.Add(gridMembers);
            this.Controls.Add(pnlGrid);
        }

        private Label CreateLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                AutoSize = true
            };
        }

        private TextBox CreateTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 30),
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(45, 52, 71),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private void LoadMembersList()
        {
            var service = new BorrowerService();
            var list = service.GetAllBorrowers();
            gridMembers.DataSource = list;
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || string.IsNullOrWhiteSpace(txtNIC.Text) || string.IsNullOrWhiteSpace(txtAddress.Text))
            {
                MessageBox.Show("Please fill in all required borrower details.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string sex = rdoMale.Checked ? "Male" : "Female";
            var service = new BorrowerService();

            if (service.RegisterBorrower(txtName.Text.Trim(), sex, txtNIC.Text.Trim(), txtAddress.Text.Trim(), out string msg, out _))
            {
                MessageBox.Show(msg, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtName.Clear();
                txtNIC.Clear();
                txtAddress.Clear();
                LoadMembersList();
            }
            else
            {
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
