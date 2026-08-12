using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SarasaviLibrarySystem.Services;

namespace SarasaviLibrarySystem.UI.Views
{
    public class InquiryView : UserControl
    {
        private TextBox txtSearch = null!;
        private Button btnSearch = null!;
        private DataGridView gridCatalog = null!;
        private Button btnReserve = null!;

        public InquiryView()
        {
            InitializeComponent();
            LoadCatalog("");
            this.Resize += (s, e) => LayoutInquiry();
            LayoutInquiry();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(18, 22, 33);

            var lblHeader = new Label
            {
                Text = "Catalog Inquiry & Search Engine",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 20),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            txtSearch = new TextBox
            {
                Location = new Point(25, 70),
                Size = new Size(500, 34),
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(30, 36, 52),
                ForeColor = Color.Gainsboro,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Search by Accession Code (e.g., C0001), Title, or Author..."
            };
            txtSearch.GotFocus += (s, e) => { if (txtSearch.Text.StartsWith("Search by")) txtSearch.Text = ""; };
            txtSearch.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtSearch.Text)) txtSearch.Text = "Search by Accession Code (e.g., C0001), Title, or Author..."; };
            txtSearch.TextChanged += (s, e) => {
                string q = txtSearch.Text.StartsWith("Search by") ? "" : txtSearch.Text;
                LoadCatalog(q);
            };
            this.Controls.Add(txtSearch);

            btnSearch = new Button
            {
                Text = "Search Catalog",
                Location = new Point(535, 69),
                Size = new Size(150, 36),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnSearch.FlatAppearance.BorderSize = 0;
            btnSearch.Click += (s, e) => {
                string q = txtSearch.Text.StartsWith("Search by") ? "" : txtSearch.Text;
                LoadCatalog(q);
            };
            this.Controls.Add(btnSearch);

            btnReserve = new Button
            {
                Text = "Place Reservation for Selected Book",
                Location = new Point(700, 69),
                Size = new Size(270, 36),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReserve.FlatAppearance.BorderSize = 0;
            btnReserve.Click += BtnReserve_Click;
            this.Controls.Add(btnReserve);

            gridCatalog = new DataGridView
            {
                Location = new Point(25, 120),
                Size = new Size(1180, 530),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackgroundColor = Color.FromArgb(26, 32, 46),
                BorderStyle = BorderStyle.None,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoGenerateColumns = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowTemplate = { Height = 42 }
            };

            gridCatalog.EnableHeadersVisualStyles = false;
            gridCatalog.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(34, 42, 60);
            gridCatalog.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gainsboro;
            gridCatalog.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            gridCatalog.ColumnHeadersHeight = 40;
            gridCatalog.DefaultCellStyle.BackColor = Color.FromArgb(26, 32, 46);
            gridCatalog.DefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 56, 80);
            gridCatalog.DefaultCellStyle.SelectionForeColor = Color.White;

            gridCatalog.Columns.Clear();
            gridCatalog.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AccessionCode",
                DataPropertyName = "AccessionCode",
                HeaderText = "Accession Code 🔹",
                FillWeight = 15F
            });
            gridCatalog.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Title",
                DataPropertyName = "Title",
                HeaderText = "Title",
                FillWeight = 38F
            });
            gridCatalog.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Author",
                DataPropertyName = "Author",
                HeaderText = "Author",
                FillWeight = 25F
            });
            gridCatalog.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Category",
                DataPropertyName = "Category",
                HeaderText = "Category",
                FillWeight = 11F
            });
            gridCatalog.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                DataPropertyName = "Status",
                HeaderText = "Status",
                FillWeight = 11F
            });

            gridCatalog.CellPainting += GridCatalog_CellPainting;
            this.Controls.Add(gridCatalog);
        }

        private void LayoutInquiry()
        {
            int margin = 25;
            int topRowY = 69;
            int gridTop = 120;

            if (txtSearch != null)
            {
                int searchWidth = Math.Min(500, Math.Max(320, this.ClientSize.Width - 725));
                txtSearch.Location = new Point(margin, 70);
                txtSearch.Size = new Size(searchWidth, 34);
            }

            if (btnSearch != null)
            {
                btnSearch.Location = new Point((txtSearch?.Right ?? (margin + 500)) + 10, topRowY - 1);
            }

            if (btnReserve != null)
            {
                btnReserve.Location = new Point((btnSearch?.Right ?? (margin + 650)) + 15, topRowY - 1);
                btnReserve.Size = new Size(Math.Max(220, this.ClientSize.Width - btnReserve.Left - margin), 36);
            }

            if (gridCatalog != null)
            {
                int gridWidth = Math.Max(300, this.ClientSize.Width - (margin * 2));
                int gridHeight = Math.Max(200, this.ClientSize.Height - gridTop - 25);
                gridCatalog.Location = new Point(margin, gridTop);
                gridCatalog.Size = new Size(gridWidth, gridHeight);
            }
        }

        private void LoadCatalog(string query)
        {
            var service = new CatalogService();
            var list = service.GetInventoryItems(query);
            gridCatalog.DataSource = list;
        }

        private void GridCatalog_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0 && gridCatalog.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
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

                int padX = 14;
                int padY = 7;
                Rectangle badgeRect = new Rectangle(
                    e.CellBounds.X + padX,
                    e.CellBounds.Y + padY,
                    e.CellBounds.Width - (padX * 2),
                    e.CellBounds.Height - (padY * 2)
                );

                using (var path = GetRoundedPath(badgeRect, 12))
                using (var brush = new SolidBrush(bg))
                using (var font = new Font("Segoe UI", 9F, FontStyle.Bold))
                using (var textBrush = new SolidBrush(text))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.FillPath(brush, path);
                    e.Graphics.DrawString(status, font, textBrush, badgeRect, sf);
                }

                e.Handled = true;
            }
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

        private void BtnReserve_Click(object? sender, EventArgs e)
        {
            if (gridCatalog.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a book from the list to reserve.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var row = gridCatalog.SelectedRows[0];
            string code = row.Cells["AccessionCode"].Value?.ToString() ?? "";
            string title = row.Cells["Title"].Value?.ToString() ?? "";

            using var dlg = new Form
            {
                Text = $"Place Reservation - {title}",
                Size = new Size(420, 220),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(26, 32, 46)
            };

            dlg.Controls.Add(new Label
            {
                Text = $"Reserving: '{title}' ({code})",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            });

            dlg.Controls.Add(new Label
            {
                Text = "Enter Borrower User Number (e.g. M-1002):",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.Gainsboro,
                Location = new Point(20, 55),
                AutoSize = true
            });

            var txtUserNum = new TextBox
            {
                Location = new Point(20, 80),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 10.5F),
                BackColor = Color.FromArgb(36, 44, 62),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "M-1002"
            };
            dlg.Controls.Add(txtUserNum);

            var btnSubmit = new Button
            {
                Text = "Confirm Reservation",
                Location = new Point(20, 125),
                Size = new Size(360, 36),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnSubmit.FlatAppearance.BorderSize = 0;
            btnSubmit.Click += (s, ev) => {
                string uNum = txtUserNum.Text.Trim();
                var resService = new ReservationService();
                if (resService.ReserveTitle(1, uNum, out string msg))
                {
                    MessageBox.Show(msg, "Reservation Placed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    dlg.Close();
                }
                else
                {
                    MessageBox.Show(msg, "Reservation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            dlg.Controls.Add(btnSubmit);

            dlg.ShowDialog();
        }
    }
}
