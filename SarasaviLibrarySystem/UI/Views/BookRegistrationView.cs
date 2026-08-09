using System;
using System.Drawing;
using System.Windows.Forms;
using SarasaviLibrarySystem.Services;

namespace SarasaviLibrarySystem.UI.Views
{
    public class BookRegistrationView : UserControl
    {
        private TextBox txtTitle = null!;
        private TextBox txtAuthor = null!;
        private TextBox txtPublisher = null!;
        private ComboBox cmbClassification = null!;
        private NumericUpDown numCopyCount = null!;
        private RadioButton rdoBorrowable = null!;
        private RadioButton rdoReferenceOnly = null!;

        private Label lblPreviewAccession = null!;
        private ListBox lstCopyPreview = null!;
        private Button btnRegister = null!;

        public BookRegistrationView()
        {
            InitializeComponent();
            UpdateCodePreview();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(24, 27, 36);

            var lblHeader = new Label
            {
                Text = "Book & Copy Registration",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 20),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            var pnlForm = new Panel
            {
                Location = new Point(25, 70),
                Size = new Size(600, 580),
                BackColor = Color.FromArgb(34, 39, 53)
            };

            int top = 25;

            pnlForm.Controls.Add(CreateLabel("Book Title *", 25, top));
            txtTitle = CreateTextBox(25, top + 25, 540);
            pnlForm.Controls.Add(txtTitle);
            top += 70;

            pnlForm.Controls.Add(CreateLabel("Author Name *", 25, top));
            txtAuthor = CreateTextBox(25, top + 25, 540);
            pnlForm.Controls.Add(txtAuthor);
            top += 70;

            pnlForm.Controls.Add(CreateLabel("Publisher *", 25, top));
            txtPublisher = CreateTextBox(25, top + 25, 540);
            pnlForm.Controls.Add(txtPublisher);
            top += 70;

            pnlForm.Controls.Add(CreateLabel("Classification Category *", 25, top));
            cmbClassification = new ComboBox
            {
                Location = new Point(25, top + 25),
                Size = new Size(540, 30),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.FromArgb(45, 52, 71),
                ForeColor = Color.White
            };
            cmbClassification.Items.AddRange(new object[] {
                "C - Computing & Technology",
                "F - Fiction & Literature",
                "S - Science & Mathematics",
                "M - Management & Business"
            });
            cmbClassification.SelectedIndex = 0;
            cmbClassification.SelectedIndexChanged += (s, e) => UpdateCodePreview();
            pnlForm.Controls.Add(cmbClassification);
            top += 70;

            pnlForm.Controls.Add(CreateLabel("Number of Physical Copies (Max 10) *", 25, top));
            numCopyCount = new NumericUpDown
            {
                Location = new Point(25, top + 25),
                Size = new Size(200, 30),
                Minimum = 1,
                Maximum = 10,
                Value = 3,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                BackColor = Color.FromArgb(45, 52, 71),
                ForeColor = Color.White
            };
            numCopyCount.ValueChanged += (s, e) => UpdateCodePreview();
            pnlForm.Controls.Add(numCopyCount);
            top += 75;

            pnlForm.Controls.Add(CreateLabel("First Copy Access Type *", 25, top));
            rdoBorrowable = new RadioButton
            {
                Text = "Borrowable (All copies borrowable)",
                Location = new Point(25, top + 25),
                AutoSize = true,
                Checked = true,
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 10F)
            };
            rdoReferenceOnly = new RadioButton
            {
                Text = "Reference Only (Copy 01 marked Reference)",
                Location = new Point(270, top + 25),
                AutoSize = true,
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 10F)
            };
            pnlForm.Controls.Add(rdoBorrowable);
            pnlForm.Controls.Add(rdoReferenceOnly);
            top += 75;

            btnRegister = new Button
            {
                Text = "Register Book & Copies",
                Location = new Point(25, top),
                Size = new Size(540, 45),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnRegister.FlatAppearance.BorderSize = 0;
            btnRegister.Click += BtnRegister_Click;
            pnlForm.Controls.Add(btnRegister);

            this.Controls.Add(pnlForm);

            var pnlPreview = new Panel
            {
                Location = new Point(650, 70),
                Size = new Size(550, 580),
                BackColor = Color.FromArgb(34, 39, 53)
            };

            var lblPreviewHeader = new Label
            {
                Text = "Auto Accession Code Generator Preview",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                Location = new Point(20, 20),
                AutoSize = true
            };
            pnlPreview.Controls.Add(lblPreviewHeader);

            lblPreviewAccession = new Label
            {
                Text = "Base Accession Code: C0004",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                Location = new Point(20, 55),
                AutoSize = true
            };
            pnlPreview.Controls.Add(lblPreviewAccession);

            var lblCopyListHeader = new Label
            {
                Text = "Generated Physical Copies List:",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(20, 100),
                AutoSize = true
            };
            pnlPreview.Controls.Add(lblCopyListHeader);

            lstCopyPreview = new ListBox
            {
                Location = new Point(20, 130),
                Size = new Size(505, 420),
                BackColor = Color.FromArgb(24, 27, 36),
                ForeColor = Color.White,
                Font = new Font("Consolas", 11F),
                BorderStyle = BorderStyle.None
            };
            pnlPreview.Controls.Add(lstCopyPreview);

            this.Controls.Add(pnlPreview);
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

        private void UpdateCodePreview()
        {
            if (cmbClassification.SelectedItem == null) return;

            string selectedText = cmbClassification.SelectedItem.ToString()!;
            char clsCode = selectedText[0];

            string baseCode = AccessionGenerator.GenerateNextAccessionCode(clsCode);
            lblPreviewAccession.Text = $"Base Accession Code: {baseCode}";

            lstCopyPreview.Items.Clear();
            int copyCount = (int)numCopyCount.Value;

            for (int i = 1; i <= copyCount; i++)
            {
                string copyAcc = AccessionGenerator.GenerateCopyAccessionNumber(baseCode, i);
                string copyType = (i == 1 && rdoReferenceOnly.Checked) ? "Reference Only" : "Borrowable";
                lstCopyPreview.Items.Add($"  [Copy {i:D2}]  Code: {copyAcc,-12}  Type: {copyType}");
            }
        }

        private void BtnRegister_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) || string.IsNullOrWhiteSpace(txtAuthor.Text) || string.IsNullOrWhiteSpace(txtPublisher.Text))
            {
                MessageBox.Show("Please fill in all required fields (Title, Author, Publisher).", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            char clsCode = cmbClassification.SelectedItem!.ToString()![0];
            int copyCount = (int)numCopyCount.Value;
            bool refOnlyFirst = rdoReferenceOnly.Checked;

            var service = new CatalogService();
            if (service.AddBookTitleWithCopies(txtTitle.Text.Trim(), txtAuthor.Text.Trim(), txtPublisher.Text.Trim(), clsCode, copyCount, refOnlyFirst, out string msg))
            {
                MessageBox.Show(msg, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtTitle.Clear();
                txtAuthor.Clear();
                txtPublisher.Clear();
                UpdateCodePreview();
            }
            else
            {
                MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
