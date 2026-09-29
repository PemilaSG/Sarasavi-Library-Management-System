using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SarasaviLibrarySystem.Models;
using SarasaviLibrarySystem.Services;

namespace SarasaviLibrarySystem.UI.Views
{
    public class ManagementView : UserControl
    {
        // Navigation Sub-Header Buttons
        private Button btnTabMembers = null!;
        private Button btnTabBooks = null!;

        private Panel pnlMembersTab = null!;
        private Panel pnlBooksTab = null!;

        // --- Member Management UI Elements ---
        private TextBox txtSearchMember = null!;
        private DataGridView gridMembers = null!;

        private TextBox txtMemUserNumber = null!;
        private TextBox txtMemName = null!;
        private ComboBox cmbMemSex = null!;
        private TextBox txtMemNIC = null!;
        private TextBox txtMemAddress = null!;
        private Button btnUpdateMember = null!;
        private Button btnDeleteMember = null!;
        private Button btnClearMember = null!;
        private Label lblMemStatus = null!;

        // --- Book Management UI Elements ---
        private TextBox txtSearchBook = null!;
        private DataGridView gridBooks = null!;

        private TextBox txtBookAccession = null!;
        private TextBox txtBookTitle = null!;
        private TextBox txtBookAuthor = null!;
        private TextBox txtBookPublisher = null!;
        private ComboBox cmbBookCategory = null!;
        private ComboBox cmbBookCopyType = null!;
        private ComboBox cmbBookStatus = null!;
        private Button btnUpdateBook = null!;
        private Button btnDeleteBook = null!;
        private Button btnClearBook = null!;
        private Label lblBookStatusMsg = null!;

        public ManagementView()
        {
            InitializeComponent();
            LoadMembersGrid("");
            LoadBooksGrid("");
            SwitchTab(true); // Default to Members Tab
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(18, 22, 33);

            // Title Header
            var lblHeader = new Label
            {
                Text = "System Management Hub",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 15),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            var lblSubHeader = new Label
            {
                Text = "Comprehensive View, Update & Deletion Control Center for Library Members & Book Catalog",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.Gray,
                Location = new Point(25, 48),
                AutoSize = true
            };
            this.Controls.Add(lblSubHeader);

            // Tab Selector Bar
            var pnlTabBar = new Panel
            {
                Location = new Point(25, 75),
                Size = new Size(1180, 45),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.FromArgb(26, 32, 46)
            };

            btnTabMembers = new Button
            {
                Text = "👥  Member Management (View / Update / Delete)",
                Location = new Point(5, 5),
                Size = new Size(420, 35),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTabMembers.FlatAppearance.BorderSize = 0;
            btnTabMembers.Click += (s, e) => SwitchTab(true);
            pnlTabBar.Controls.Add(btnTabMembers);

            btnTabBooks = new Button
            {
                Text = "📚  Book Management (View / Update / Delete)",
                Location = new Point(435, 5),
                Size = new Size(420, 35),
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTabBooks.FlatAppearance.BorderSize = 0;
            btnTabBooks.Click += (s, e) => SwitchTab(false);
            pnlTabBar.Controls.Add(btnTabBooks);

            this.Controls.Add(pnlTabBar);

            // --- Panel 1: Member Management Tab ---
            pnlMembersTab = new Panel
            {
                Location = new Point(25, 130),
                Size = new Size(1180, 530),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = Color.Transparent
            };
            BuildMembersTabUI();
            this.Controls.Add(pnlMembersTab);

            // --- Panel 2: Book Management Tab ---
            pnlBooksTab = new Panel
            {
                Location = new Point(25, 130),
                Size = new Size(1180, 530),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = Color.Transparent
            };
            BuildBooksTabUI();
            this.Controls.Add(pnlBooksTab);
        }

        private void SwitchTab(bool isMembersTab)
        {
            if (isMembersTab)
            {
                btnTabMembers.BackColor = Color.FromArgb(41, 128, 185);
                btnTabMembers.ForeColor = Color.White;

                btnTabBooks.BackColor = Color.FromArgb(34, 42, 60);
                btnTabBooks.ForeColor = Color.Gainsboro;

                pnlMembersTab.Visible = true;
                pnlBooksTab.Visible = false;
                pnlMembersTab.BringToFront();
            }
            else
            {
                btnTabBooks.BackColor = Color.FromArgb(41, 128, 185);
                btnTabBooks.ForeColor = Color.White;

                btnTabMembers.BackColor = Color.FromArgb(34, 42, 60);
                btnTabMembers.ForeColor = Color.Gainsboro;

                pnlBooksTab.Visible = true;
                pnlMembersTab.Visible = false;
                pnlBooksTab.BringToFront();
            }
        }

        #region Member Management Tab UI & Methods
        private void BuildMembersTabUI()
        {
            // Left Side: Grid & Search Panel
            var pnlGridHost = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(720, 530),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                BackColor = Color.FromArgb(26, 32, 46)
            };

            txtSearchMember = new TextBox
            {
                Location = new Point(15, 15),
                Size = new Size(690, 32),
                Font = new Font("Segoe UI", 10.5F),
                BackColor = Color.FromArgb(36, 44, 62),
                ForeColor = Color.Gainsboro,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "🔍 Search member by User Number (M-1001), Name, or NIC..."
            };
            txtSearchMember.GotFocus += (s, e) => { if (txtSearchMember.Text.StartsWith("🔍")) txtSearchMember.Text = ""; };
            txtSearchMember.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearchMember.Text)) txtSearchMember.Text = "🔍 Search member by User Number (M-1001), Name, or NIC..."; };
            txtSearchMember.TextChanged += (s, e) => {
                string q = txtSearchMember.Text.StartsWith("🔍") ? "" : txtSearchMember.Text.Trim();
                LoadMembersGrid(q);
            };
            pnlGridHost.Controls.Add(txtSearchMember);

            gridMembers = new DataGridView
            {
                Location = new Point(15, 55),
                Size = new Size(690, 460),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackgroundColor = Color.FromArgb(18, 22, 33),
                BorderStyle = BorderStyle.None,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                RowTemplate = { Height = 34 }
            };

            gridMembers.EnableHeadersVisualStyles = false;
            gridMembers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(34, 42, 60);
            gridMembers.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gainsboro;
            gridMembers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            gridMembers.ColumnHeadersHeight = 35;
            gridMembers.DefaultCellStyle.BackColor = Color.FromArgb(18, 22, 33);
            gridMembers.DefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 56, 80);
            gridMembers.DefaultCellStyle.SelectionForeColor = Color.White;

            gridMembers.Columns.Add(new DataGridViewTextBoxColumn { Name = "UserNumber", DataPropertyName = "UserNumber", HeaderText = "User No", Width = 90 });
            gridMembers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", DataPropertyName = "Name", HeaderText = "Full Name", Width = 150 });
            gridMembers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Sex", DataPropertyName = "Sex", HeaderText = "Gender", Width = 75 });
            gridMembers.Columns.Add(new DataGridViewTextBoxColumn { Name = "NIC", DataPropertyName = "NIC", HeaderText = "NIC", Width = 110 });
            gridMembers.Columns.Add(new DataGridViewTextBoxColumn { Name = "Address", DataPropertyName = "Address", HeaderText = "Address", Width = 160 });
            gridMembers.Columns.Add(new DataGridViewTextBoxColumn { Name = "ActiveLoansCount", DataPropertyName = "ActiveLoansCount", HeaderText = "Loans", Width = 55 });
            gridMembers.Columns.Add(new DataGridViewCheckBoxColumn { Name = "HasOverdueLoans", DataPropertyName = "HasOverdueLoans", HeaderText = "Overdue", Width = 50 });

            gridMembers.SelectionChanged += GridMembers_SelectionChanged;
            pnlGridHost.Controls.Add(gridMembers);
            pnlMembersTab.Controls.Add(pnlGridHost);

            // Right Side: Edit / Delete Form Panel
            var pnlEditHost = new Panel
            {
                Location = new Point(735, 0),
                Size = new Size(445, 530),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = Color.FromArgb(26, 32, 46)
            };

            pnlEditHost.Controls.Add(new Label
            {
                Text = "Member Action & Edit Details",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            });

            int y = 50;

            pnlEditHost.Controls.Add(CreateFieldLabel("User Number (ID):", 20, y));
            txtMemUserNumber = CreateReadOnlyTextBox(20, y + 22, 405);
            pnlEditHost.Controls.Add(txtMemUserNumber);
            y += 62;

            pnlEditHost.Controls.Add(CreateFieldLabel("Full Name *", 20, y));
            txtMemName = CreateInputTextBox(20, y + 22, 405);
            pnlEditHost.Controls.Add(txtMemName);
            y += 62;

            pnlEditHost.Controls.Add(CreateFieldLabel("Gender (Sex) *", 20, y));
            cmbMemSex = new ComboBox
            {
                Location = new Point(20, y + 22),
                Size = new Size(405, 30),
                Font = new Font("Segoe UI", 10.5F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(36, 44, 62),
                ForeColor = Color.White
            };
            cmbMemSex.Items.AddRange(new object[] { "Male", "Female" });
            cmbMemSex.SelectedIndex = 0;
            pnlEditHost.Controls.Add(cmbMemSex);
            y += 62;

            pnlEditHost.Controls.Add(CreateFieldLabel("NIC Number *", 20, y));
            txtMemNIC = CreateInputTextBox(20, y + 22, 405);
            pnlEditHost.Controls.Add(txtMemNIC);
            y += 62;

            pnlEditHost.Controls.Add(CreateFieldLabel("Residential Address *", 20, y));
            txtMemAddress = new TextBox
            {
                Location = new Point(20, y + 22),
                Size = new Size(405, 65),
                Multiline = true,
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.FromArgb(36, 44, 62),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlEditHost.Controls.Add(txtMemAddress);
            y += 95;

            btnUpdateMember = new Button
            {
                Text = "💾  Update Member Details",
                Location = new Point(20, y),
                Size = new Size(195, 38),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUpdateMember.FlatAppearance.BorderSize = 0;
            btnUpdateMember.Click += BtnUpdateMember_Click;
            pnlEditHost.Controls.Add(btnUpdateMember);

            btnDeleteMember = new Button
            {
                Text = "🗑️  Delete Member",
                Location = new Point(225, y),
                Size = new Size(195, 38),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDeleteMember.FlatAppearance.BorderSize = 0;
            btnDeleteMember.Click += BtnDeleteMember_Click;
            pnlEditHost.Controls.Add(btnDeleteMember);

            y += 48;
            btnClearMember = new Button
            {
                Text = "🔄  Clear / Deselect Selection",
                Location = new Point(20, y),
                Size = new Size(405, 32),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClearMember.FlatAppearance.BorderSize = 0;
            btnClearMember.Click += (s, e) => ClearMemberForm();
            pnlEditHost.Controls.Add(btnClearMember);

            y += 40;
            lblMemStatus = new Label
            {
                Text = "Select a member from the grid to update or delete.",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(20, y),
                Size = new Size(405, 30)
            };
            pnlEditHost.Controls.Add(lblMemStatus);

            pnlMembersTab.Controls.Add(pnlEditHost);
        }

        private void LoadMembersGrid(string query)
        {
            var service = new BorrowerService();
            var all = service.GetAllBorrowers();

            if (!string.IsNullOrWhiteSpace(query))
            {
                all = all.FindAll(b => b.UserNumber.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                       b.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                       b.NIC.Contains(query, StringComparison.OrdinalIgnoreCase));
            }

            gridMembers.DataSource = all;
            ClearMemberForm();
        }

        private void GridMembers_SelectionChanged(object? sender, EventArgs e)
        {
            if (gridMembers.SelectedRows.Count > 0)
            {
                var b = gridMembers.SelectedRows[0].DataBoundItem as Borrower;
                if (b != null)
                {
                    txtMemUserNumber.Text = b.UserNumber;
                    txtMemName.Text = b.Name;
                    cmbMemSex.SelectedItem = b.Sex;
                    txtMemNIC.Text = b.NIC;
                    txtMemAddress.Text = b.Address;
                    lblMemStatus.Text = $"Selected member '{b.UserNumber}' ({b.Name}).";
                    lblMemStatus.ForeColor = Color.Gainsboro;
                }
            }
        }

        private void ClearMemberForm()
        {
            txtMemUserNumber.Clear();
            txtMemName.Clear();
            cmbMemSex.SelectedIndex = 0;
            txtMemNIC.Clear();
            txtMemAddress.Clear();
            lblMemStatus.Text = "Form cleared. Select a member from the grid.";
            lblMemStatus.ForeColor = Color.Gray;
        }

        private void BtnUpdateMember_Click(object? sender, EventArgs e)
        {
            string unum = txtMemUserNumber.Text.Trim();
            if (string.IsNullOrWhiteSpace(unum))
            {
                MessageBox.Show("Please select a member from the grid to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var service = new BorrowerService();
            if (service.UpdateBorrower(unum, txtMemName.Text.Trim(), cmbMemSex.SelectedItem?.ToString() ?? "Male", txtMemNIC.Text.Trim(), txtMemAddress.Text.Trim(), out string msg))
            {
                MessageBox.Show(msg, "Member Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                string q = txtSearchMember.Text.StartsWith("🔍") ? "" : txtSearchMember.Text.Trim();
                LoadMembersGrid(q);
            }
            else
            {
                MessageBox.Show(msg, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteMember_Click(object? sender, EventArgs e)
        {
            string unum = txtMemUserNumber.Text.Trim();
            if (string.IsNullOrWhiteSpace(unum))
            {
                MessageBox.Show("Please select a member from the grid to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Are you sure you want to permanently delete member '{unum}' ({txtMemName.Text})?",
                                          "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                var service = new BorrowerService();
                if (service.DeleteBorrower(unum, out string msg))
                {
                    MessageBox.Show(msg, "Member Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    string q = txtSearchMember.Text.StartsWith("🔍") ? "" : txtSearchMember.Text.Trim();
                    LoadMembersGrid(q);
                }
                else
                {
                    MessageBox.Show(msg, "Deletion Blocked", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Book Management Tab UI & Methods
        private void BuildBooksTabUI()
        {
            // Left Side: Grid & Search Panel
            var pnlGridHost = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(720, 530),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                BackColor = Color.FromArgb(26, 32, 46)
            };

            txtSearchBook = new TextBox
            {
                Location = new Point(15, 15),
                Size = new Size(690, 32),
                Font = new Font("Segoe UI", 10.5F),
                BackColor = Color.FromArgb(36, 44, 62),
                ForeColor = Color.Gainsboro,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "🔍 Search book by Accession Code (C0001-01), Title, Author, or Publisher..."
            };
            txtSearchBook.GotFocus += (s, e) => { if (txtSearchBook.Text.StartsWith("🔍")) txtSearchBook.Text = ""; };
            txtSearchBook.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearchBook.Text)) txtSearchBook.Text = "🔍 Search book by Accession Code (C0001-01), Title, Author, or Publisher..."; };
            txtSearchBook.TextChanged += (s, e) => {
                string q = txtSearchBook.Text.StartsWith("🔍") ? "" : txtSearchBook.Text.Trim();
                LoadBooksGrid(q);
            };
            pnlGridHost.Controls.Add(txtSearchBook);

            gridBooks = new DataGridView
            {
                Location = new Point(15, 55),
                Size = new Size(690, 460),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackgroundColor = Color.FromArgb(18, 22, 33),
                BorderStyle = BorderStyle.None,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                RowTemplate = { Height = 34 }
            };

            gridBooks.EnableHeadersVisualStyles = false;
            gridBooks.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(34, 42, 60);
            gridBooks.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gainsboro;
            gridBooks.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            gridBooks.ColumnHeadersHeight = 35;
            gridBooks.DefaultCellStyle.BackColor = Color.FromArgb(18, 22, 33);
            gridBooks.DefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 56, 80);
            gridBooks.DefaultCellStyle.SelectionForeColor = Color.White;

            gridBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "AccessionCode", DataPropertyName = "AccessionCode", HeaderText = "Accession Code", Width = 110 });
            gridBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "Title", DataPropertyName = "Title", HeaderText = "Title", Width = 180 });
            gridBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "Author", DataPropertyName = "Author", HeaderText = "Author", Width = 120 });
            gridBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "Publisher", DataPropertyName = "Publisher", HeaderText = "Publisher", Width = 100 });
            gridBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", DataPropertyName = "Category", HeaderText = "Category", Width = 80 });
            gridBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "CopyType", DataPropertyName = "CopyType", HeaderText = "Type", Width = 85 });
            gridBooks.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", DataPropertyName = "Status", HeaderText = "Status", Width = 75 });

            gridBooks.SelectionChanged += GridBooks_SelectionChanged;
            gridBooks.CellPainting += GridBooks_CellPainting;
            pnlGridHost.Controls.Add(gridBooks);
            pnlBooksTab.Controls.Add(pnlGridHost);

            // Right Side: Edit / Delete Form Panel
            var pnlEditHost = new Panel
            {
                Location = new Point(735, 0),
                Size = new Size(445, 530),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = Color.FromArgb(26, 32, 46)
            };

            pnlEditHost.Controls.Add(new Label
            {
                Text = "Book Action & Edit Details",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            });

            int y = 45;

            pnlEditHost.Controls.Add(CreateFieldLabel("Accession Code (Copy ID):", 20, y));
            txtBookAccession = CreateReadOnlyTextBox(20, y + 20, 405);
            pnlEditHost.Controls.Add(txtBookAccession);
            y += 54;

            pnlEditHost.Controls.Add(CreateFieldLabel("Book Title *", 20, y));
            txtBookTitle = CreateInputTextBox(20, y + 20, 405);
            pnlEditHost.Controls.Add(txtBookTitle);
            y += 54;

            pnlEditHost.Controls.Add(CreateFieldLabel("Author Name *", 20, y));
            txtBookAuthor = CreateInputTextBox(20, y + 20, 405);
            pnlEditHost.Controls.Add(txtBookAuthor);
            y += 54;

            pnlEditHost.Controls.Add(CreateFieldLabel("Publisher *", 20, y));
            txtBookPublisher = CreateInputTextBox(20, y + 20, 405);
            pnlEditHost.Controls.Add(txtBookPublisher);
            y += 54;

            pnlEditHost.Controls.Add(CreateFieldLabel("Category / Classification *", 20, y));
            cmbBookCategory = new ComboBox
            {
                Location = new Point(20, y + 20),
                Size = new Size(405, 30),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(36, 44, 62),
                ForeColor = Color.White
            };
            cmbBookCategory.Items.AddRange(new object[] { "Computing", "Fiction", "Science", "Management" });
            cmbBookCategory.SelectedIndex = 0;
            pnlEditHost.Controls.Add(cmbBookCategory);
            y += 54;

            pnlEditHost.Controls.Add(CreateFieldLabel("Copy Type *", 20, y));
            cmbBookCopyType = new ComboBox
            {
                Location = new Point(20, y + 20),
                Size = new Size(195, 30),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(36, 44, 62),
                ForeColor = Color.White
            };
            cmbBookCopyType.Items.AddRange(new object[] { "Borrowable", "Reference Only" });
            cmbBookCopyType.SelectedIndex = 0;
            pnlEditHost.Controls.Add(cmbBookCopyType);

            pnlEditHost.Controls.Add(CreateFieldLabel("Status *", 230, y));
            cmbBookStatus = new ComboBox
            {
                Location = new Point(230, y + 20),
                Size = new Size(195, 30),
                Font = new Font("Segoe UI", 10F),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(36, 44, 62),
                ForeColor = Color.White
            };
            cmbBookStatus.Items.AddRange(new object[] { "Available", "Borrowed", "Reserved" });
            cmbBookStatus.SelectedIndex = 0;
            pnlEditHost.Controls.Add(cmbBookStatus);
            y += 60;

            btnUpdateBook = new Button
            {
                Text = "💾  Update Book Details",
                Location = new Point(20, y),
                Size = new Size(195, 38),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnUpdateBook.FlatAppearance.BorderSize = 0;
            btnUpdateBook.Click += BtnUpdateBook_Click;
            pnlEditHost.Controls.Add(btnUpdateBook);

            btnDeleteBook = new Button
            {
                Text = "🗑️  Delete Book Copy",
                Location = new Point(225, y),
                Size = new Size(195, 38),
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnDeleteBook.FlatAppearance.BorderSize = 0;
            btnDeleteBook.Click += BtnDeleteBook_Click;
            pnlEditHost.Controls.Add(btnDeleteBook);

            y += 48;
            btnClearBook = new Button
            {
                Text = "🔄  Clear / Deselect Selection",
                Location = new Point(20, y),
                Size = new Size(405, 32),
                BackColor = Color.FromArgb(52, 73, 94),
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnClearBook.FlatAppearance.BorderSize = 0;
            btnClearBook.Click += (s, e) => ClearBookForm();
            pnlEditHost.Controls.Add(btnClearBook);

            y += 40;
            lblBookStatusMsg = new Label
            {
                Text = "Select a book copy from the grid to update or delete.",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(20, y),
                Size = new Size(405, 30)
            };
            pnlEditHost.Controls.Add(lblBookStatusMsg);

            pnlBooksTab.Controls.Add(pnlEditHost);
        }

        private void LoadBooksGrid(string query)
        {
            var service = new CatalogService();
            var list = service.GetInventoryItems(query);
            gridBooks.DataSource = list;
            ClearBookForm();
        }

        private void GridBooks_SelectionChanged(object? sender, EventArgs e)
        {
            if (gridBooks.SelectedRows.Count > 0)
            {
                var item = gridBooks.SelectedRows[0].DataBoundItem as BookInventoryItem;
                if (item != null)
                {
                    txtBookAccession.Text = item.AccessionCode;
                    txtBookTitle.Text = item.Title;
                    txtBookAuthor.Text = item.Author;
                    txtBookPublisher.Text = item.Publisher;

                    if (cmbBookCategory.Items.Contains(item.Category))
                        cmbBookCategory.SelectedItem = item.Category;
                    else
                        cmbBookCategory.SelectedIndex = 0;

                    if (cmbBookCopyType.Items.Contains(item.CopyType))
                        cmbBookCopyType.SelectedItem = item.CopyType;
                    else
                        cmbBookCopyType.SelectedIndex = 0;

                    if (cmbBookStatus.Items.Contains(item.Status))
                        cmbBookStatus.SelectedItem = item.Status;
                    else
                        cmbBookStatus.SelectedIndex = 0;

                    lblBookStatusMsg.Text = $"Selected copy '{item.AccessionCode}' ({item.Title}).";
                    lblBookStatusMsg.ForeColor = Color.Gainsboro;
                }
            }
        }

        private void ClearBookForm()
        {
            txtBookAccession.Clear();
            txtBookTitle.Clear();
            txtBookAuthor.Clear();
            txtBookPublisher.Clear();
            cmbBookCategory.SelectedIndex = 0;
            cmbBookCopyType.SelectedIndex = 0;
            cmbBookStatus.SelectedIndex = 0;
            lblBookStatusMsg.Text = "Form cleared. Select a book copy from the grid.";
            lblBookStatusMsg.ForeColor = Color.Gray;
        }

        private void BtnUpdateBook_Click(object? sender, EventArgs e)
        {
            string acc = txtBookAccession.Text.Trim();
            if (string.IsNullOrWhiteSpace(acc))
            {
                MessageBox.Show("Please select a book copy from the grid to update.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var service = new CatalogService();
            string cat = cmbBookCategory.SelectedItem?.ToString() ?? "Computing";
            string ctype = cmbBookCopyType.SelectedItem?.ToString() ?? "Borrowable";
            string stat = cmbBookStatus.SelectedItem?.ToString() ?? "Available";

            if (service.UpdateBookItem(acc, txtBookTitle.Text.Trim(), txtBookAuthor.Text.Trim(), txtBookPublisher.Text.Trim(), cat, ctype, stat, out string msg))
            {
                MessageBox.Show(msg, "Book Updated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                string q = txtSearchBook.Text.StartsWith("🔍") ? "" : txtSearchBook.Text.Trim();
                LoadBooksGrid(q);
            }
            else
            {
                MessageBox.Show(msg, "Update Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDeleteBook_Click(object? sender, EventArgs e)
        {
            string acc = txtBookAccession.Text.Trim();
            if (string.IsNullOrWhiteSpace(acc))
            {
                MessageBox.Show("Please select a book copy from the grid to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Are you sure you want to delete book copy '{acc}' ({txtBookTitle.Text})?",
                                          "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                var service = new CatalogService();
                if (service.DeleteBookCopy(acc, out string msg))
                {
                    MessageBox.Show(msg, "Book Copy Deleted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    string q = txtSearchBook.Text.StartsWith("🔍") ? "" : txtSearchBook.Text.Trim();
                    LoadBooksGrid(q);
                }
                else
                {
                    MessageBox.Show(msg, "Deletion Blocked", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void GridBooks_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && gridBooks.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

                string status = e.Value.ToString()!;
                Color bg = Color.FromArgb(39, 174, 96);
                Color text = Color.White;

                if (status == "Reference Only")
                {
                    bg = Color.FromArgb(241, 196, 15);
                    text = Color.FromArgb(20, 20, 20);
                }
                else if (status == "Borrowed")
                {
                    bg = Color.FromArgb(52, 152, 219);
                    text = Color.White;
                }
                else if (status == "Reserved")
                {
                    bg = Color.FromArgb(230, 126, 34);
                    text = Color.White;
                }

                int padX = 8;
                int padY = 5;
                Rectangle badgeRect = new Rectangle(
                    e.CellBounds.X + padX,
                    e.CellBounds.Y + padY,
                    e.CellBounds.Width - (padX * 2),
                    e.CellBounds.Height - (padY * 2)
                );

                if (e.Graphics != null)
                {
                    using var path = GetRoundedPath(badgeRect, 10);
                    using var brush = new SolidBrush(bg);
                    using var font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
                    using var textBrush = new SolidBrush(text);
                    using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };

                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawString(status, font, textBrush, badgeRect, sf);
                }

                e.Handled = true;
            }
        }
        #endregion

        #region UI Helper Factories
        private Label CreateFieldLabel(string text, int x, int y)
        {
            return new Label
            {
                Text = text,
                Location = new Point(x, y),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                AutoSize = true
            };
        }

        private TextBox CreateReadOnlyTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 28),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 52, 71),
                ForeColor = Color.FromArgb(46, 204, 113),
                ReadOnly = true,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private TextBox CreateInputTextBox(int x, int y, int width)
        {
            return new TextBox
            {
                Location = new Point(x, y),
                Size = new Size(width, 28),
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.FromArgb(36, 44, 62),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }
        #endregion
    }
}
