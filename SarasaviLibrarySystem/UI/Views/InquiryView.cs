using System;
using System.Drawing;
using System.Windows.Forms;
using SarasaviLibrarySystem.Services;

namespace SarasaviLibrarySystem.UI.Views
{
    public class InquiryView : UserControl
    {
        private Label lblHeader = null!;
        private Panel pnlSearch = null!;
        private TextBox txtSearch = null!;
        private Button btnSearch = null!;
        private DataGridView gridCatalog = null!;
        private Button btnReserve = null!;

        public InquiryView()
        {
            InitializeComponent();
            PerformSearch("");
            this.Resize += (s, e) => LayoutInquiry();
            LayoutInquiry();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(24, 27, 36);

            lblHeader = new Label
            {
                Text = "Catalog Inquiry & Multi-Keyword Search",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 20),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            pnlSearch = new Panel
            {
                Location = new Point(25, 65),
                Size = new Size(1180, 70),
                BackColor = Color.FromArgb(34, 39, 53)
            };

            pnlSearch.Controls.Add(new Label
            {
                Text = "Search Catalog (Accession Code X9999 / Title / Author):",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(15, 10),
                AutoSize = true
            });

            txtSearch = new TextBox
            {
                Location = new Point(15, 30),
                Size = new Size(700, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(45, 52, 71),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtSearch.TextChanged += (s, e) => PerformSearch(txtSearch.Text);
            pnlSearch.Controls.Add(txtSearch);

            btnSearch = new Button
            {
                Text = "Search Catalog",
                Location = new Point(725, 29),
                Size = new Size(180, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += (s, e) => PerformSearch(txtSearch.Text);
            pnlSearch.Controls.Add(btnSearch);

            btnReserve = new Button
            {
                Text = "Place Reservation for Selected Title",
                Location = new Point(915, 29),
                Size = new Size(250, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReserve.FlatAppearance.BorderSize = 0;
            btnReserve.Click += BtnReserve_Click;
            pnlSearch.Controls.Add(btnReserve);

            this.Controls.Add(pnlSearch);

            gridCatalog = new DataGridView
            {
                Location = new Point(25, 150),
                Size = new Size(1180, 500),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackgroundColor = Color.FromArgb(30, 34, 45),
                BorderStyle = BorderStyle.None,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            gridCatalog.EnableHeadersVisualStyles = false;
            gridCatalog.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(45, 52, 71);
            gridCatalog.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            gridCatalog.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            gridCatalog.ColumnHeadersHeight = 35;
            gridCatalog.DefaultCellStyle.BackColor = Color.FromArgb(30, 34, 45);
            gridCatalog.DefaultCellStyle.SelectionBackColor = Color.FromArgb(52, 73, 94);
            gridCatalog.DefaultCellStyle.SelectionForeColor = Color.White;

            gridCatalog.CellFormatting += GridCatalog_CellFormatting;

            this.Controls.Add(gridCatalog);
        }

        private void LayoutInquiry()
        {
            int margin = 25;
            int availableWidth = Math.Max(900, this.ClientSize.Width - (margin * 2));

            if (lblHeader != null)
            {
                lblHeader.Location = new Point(margin, 20);
            }

            if (pnlSearch != null)
            {
                pnlSearch.Location = new Point(margin, 65);
                pnlSearch.Size = new Size(availableWidth, 70);
            }

            if (txtSearch != null)
            {
                int searchWidth = Math.Max(420, availableWidth - 470);
                txtSearch.Size = new Size(searchWidth, 30);
            }

            if (btnSearch != null)
            {
                btnSearch.Location = new Point(pnlSearch.ClientSize.Width - 435, 29);
            }

            if (btnReserve != null)
            {
                btnReserve.Location = new Point(pnlSearch.ClientSize.Width - 250 - 15, 29);
            }

            if (gridCatalog != null)
            {
                gridCatalog.Location = new Point(margin, 150);
                gridCatalog.Size = new Size(availableWidth, Math.Max(300, this.ClientSize.Height - 175));
            }
        }

        private void PerformSearch(string query)
        {
            var service = new CatalogService();
            var list = service.SearchCatalog(query);
            gridCatalog.DataSource = list;
            FormatGridColumns();
        }

        private void FormatGridColumns()
        {
            if (gridCatalog.Columns["TitleId"] is DataGridViewColumn colTitleId)
            {
                colTitleId.HeaderText = "Title ID";
                colTitleId.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colTitleId.Width = 80;
            }
            if (gridCatalog.Columns["AccessionNumber"] is DataGridViewColumn colAcc)
            {
                colAcc.HeaderText = "Accession Code";
                colAcc.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colAcc.Width = 140;
            }
            if (gridCatalog.Columns["TitleName"] is DataGridViewColumn colTitle)
            {
                colTitle.HeaderText = "Book Title";
                colTitle.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            }
            if (gridCatalog.Columns["AuthorName"] is DataGridViewColumn colAuthor)
            {
                colAuthor.HeaderText = "Author Name";
                colAuthor.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colAuthor.Width = 260;
            }
            if (gridCatalog.Columns["CopyType"] is DataGridViewColumn colType)
            {
                colType.HeaderText = "Copy Type";
                colType.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colType.Width = 140;
            }
            if (gridCatalog.Columns["Status"] is DataGridViewColumn colStatus)
            {
                colStatus.HeaderText = "Current Status";
                colStatus.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                colStatus.Width = 130;
            }
        }

        private void GridCatalog_CellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
        {
            if (gridCatalog.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString()!;
                if (status == "Available")
                {
                    e.CellStyle!.ForeColor = Color.FromArgb(46, 204, 113);
                    e.CellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                }
                else if (status == "Borrowed")
                {
                    e.CellStyle!.ForeColor = Color.FromArgb(52, 152, 219);
                    e.CellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                }
                else if (status == "Reserved")
                {
                    e.CellStyle!.ForeColor = Color.FromArgb(230, 126, 34);
                    e.CellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
                }
            }
        }

        private void BtnReserve_Click(object? sender, EventArgs e)
        {
            if (gridCatalog.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a book row from the table first.", "Selection Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = gridCatalog.SelectedRows[0];
            int titleId = Convert.ToInt32(row.Cells["TitleId"].Value);
            string titleName = row.Cells["TitleName"].Value?.ToString() ?? string.Empty;

            using var inputDlg = new Form
            {
                Text = "Place Book Title Reservation",
                Size = new Size(420, 200),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(34, 39, 53),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblPrompt = new Label
            {
                Text = $"Reserving Title: '{titleName}'\nEnter Member User Number (e.g. M-1002):",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                Location = new Point(20, 20),
                Size = new Size(360, 45)
            };

            var txtUserNum = new TextBox
            {
                Location = new Point(20, 70),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(45, 52, 71),
                ForeColor = Color.White,
                Text = "M-1002"
            };

            var btnSubmit = new Button
            {
                Text = "Submit Reservation",
                Location = new Point(20, 115),
                Size = new Size(360, 35),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };

            inputDlg.Controls.Add(lblPrompt);
            inputDlg.Controls.Add(txtUserNum);
            inputDlg.Controls.Add(btnSubmit);

            if (inputDlg.ShowDialog() == DialogResult.OK && !string.IsNullOrWhiteSpace(txtUserNum.Text))
            {
                var service = new ReservationService();
                if (service.CreateReservation(titleId, txtUserNum.Text.Trim(), out string msg))
                {
                    MessageBox.Show(msg, "Reservation Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    PerformSearch(txtSearch.Text);
                }
                else
                {
                    MessageBox.Show(msg, "Reservation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
