using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SarasaviLibrarySystem.Services;

namespace SarasaviLibrarySystem.UI.Views
{
    public class DashboardView : UserControl
    {
        private Label lblHeader = null!;
        private Panel pnlTableContainer = null!;
        private TableLayoutPanel pnlCardsContainer = null!;
        private Panel cardTitles = null!;
        private Panel cardAvailable = null!;
        private Panel cardLoans = null!;
        private Panel cardOverdue = null!;

        private Label lblTitlesVal = null!;
        private Label lblAvailableVal = null!;
        private Label lblLoansVal = null!;
        private Label lblOverdueVal = null!;

        private TextBox txtTopSearch = null!;
        private TextBox txtTableFilter = null!;
        private DataGridView gridInventory = null!;

        public DashboardView()
        {
            InitializeComponent();
            LoadData();
            this.Resize += (s, e) => LayoutDashboard();
            LayoutDashboard();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(18, 22, 33);

            lblHeader = new Label
            {
                Text = "Library Dashboard",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 20),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            txtTopSearch = new TextBox
            {
                Location = new Point(0, 22),
                Size = new Size(360, 32),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10.5F),
                BackColor = Color.FromArgb(30, 36, 52),
                ForeColor = Color.Gainsboro,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Search by Accession Code (e.g., X9999), Title, or Author..."
            };
            txtTopSearch.GotFocus += (s, e) => { if (txtTopSearch.Text.StartsWith("Search by")) txtTopSearch.Text = ""; };
            txtTopSearch.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtTopSearch.Text)) txtTopSearch.Text = "Search by Accession Code (e.g., X9999), Title, or Author..."; };
            txtTopSearch.TextChanged += (s, e) => {
                string q = txtTopSearch.Text.StartsWith("Search by") ? "" : txtTopSearch.Text;
                LoadInventory(q);
            };
            this.Controls.Add(txtTopSearch);

            // Responsive 4-Column TableLayoutPanel for Top Cards
            pnlCardsContainer = new TableLayoutPanel
            {
                Location = new Point(25, 70),
                Size = new Size(1175, 100),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right,
                ColumnCount = 4,
                RowCount = 1,
                BackColor = Color.Transparent
            };
            pnlCardsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            pnlCardsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            pnlCardsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            pnlCardsContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            pnlCardsContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            cardTitles = CreateCard("Total Books", "500", Color.White);
            lblTitlesVal = (Label)cardTitles.Controls[1];

            cardAvailable = CreateCard("Available", "412", Color.FromArgb(46, 204, 113));
            lblAvailableVal = (Label)cardAvailable.Controls[1];

            cardLoans = CreateCard("Active Loans", "88", Color.FromArgb(52, 152, 219));
            lblLoansVal = (Label)cardLoans.Controls[1];

            cardOverdue = CreateCard("Overdue", "3", Color.FromArgb(230, 126, 34));
            lblOverdueVal = (Label)cardOverdue.Controls[1];

            pnlCardsContainer.Controls.Add(cardTitles, 0, 0);
            pnlCardsContainer.Controls.Add(cardAvailable, 1, 0);
            pnlCardsContainer.Controls.Add(cardLoans, 2, 0);
            pnlCardsContainer.Controls.Add(cardOverdue, 3, 0);

            this.Controls.Add(pnlCardsContainer);

            // Table Container Panel
            pnlTableContainer = new Panel
            {
                Location = new Point(25, 185),
                Size = new Size(1175, 490),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = Color.FromArgb(26, 32, 46)
            };

            var lblTableTitle = new Label
            {
                Text = "Current Book Inventory",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };
            pnlTableContainer.Controls.Add(lblTableTitle);

            txtTableFilter = new TextBox
            {
                Location = new Point(870, 15),
                Size = new Size(280, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.FromArgb(36, 44, 62),
                ForeColor = Color.Gainsboro,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "🔍 Quick Filter..."
            };
            txtTableFilter.GotFocus += (s, e) => { if (txtTableFilter.Text.StartsWith("🔍")) txtTableFilter.Text = ""; };
            txtTableFilter.LostFocus += (s, e) => { if (string.IsNullOrWhiteSpace(txtTableFilter.Text)) txtTableFilter.Text = "🔍 Quick Filter..."; };
            txtTableFilter.TextChanged += (s, e) => {
                string q = txtTableFilter.Text.StartsWith("🔍") ? "" : txtTableFilter.Text;
                LoadInventory(q);
            };
            pnlTableContainer.Controls.Add(txtTableFilter);

            gridInventory = new DataGridView
            {
                Location = new Point(20, 55),
                Size = new Size(1135, 415),
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

            gridInventory.EnableHeadersVisualStyles = false;
            gridInventory.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(34, 42, 60);
            gridInventory.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gainsboro;
            gridInventory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
            gridInventory.ColumnHeadersHeight = 40;
            gridInventory.DefaultCellStyle.BackColor = Color.FromArgb(26, 32, 46);
            gridInventory.DefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 56, 80);
            gridInventory.DefaultCellStyle.SelectionForeColor = Color.White;

            gridInventory.Columns.Clear();
            
            // 5 Responsive Proportional Fill Columns
            gridInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AccessionCode",
                DataPropertyName = "AccessionCode",
                HeaderText = "Accession Code 🔹",
                FillWeight = 15F
            });
            gridInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Title",
                DataPropertyName = "Title",
                HeaderText = "Title",
                FillWeight = 38F
            });
            gridInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Author",
                DataPropertyName = "Author",
                HeaderText = "Author",
                FillWeight = 25F
            });
            gridInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Category",
                DataPropertyName = "Category",
                HeaderText = "Category",
                FillWeight = 11F
            });
            gridInventory.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                DataPropertyName = "Status",
                HeaderText = "Status",
                FillWeight = 11F
            });

            gridInventory.CellPainting += GridInventory_CellPainting;

            pnlTableContainer.Controls.Add(gridInventory);
            this.Controls.Add(pnlTableContainer);
        }

        private Panel CreateCard(string title, string value, Color accentColor)
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 15, 0),
                BackColor = Color.FromArgb(26, 32, 46)
            };

            var lblT = new Label
            {
                Text = title,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(18, 15),
                AutoSize = true
            };

            var lblV = new Label
            {
                Text = value,
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = accentColor,
                Location = new Point(15, 38),
                AutoSize = true
            };

            pnl.Controls.Add(lblT);
            pnl.Controls.Add(lblV);
            return pnl;
        }

        public void LoadData()
        {
            var service = new CatalogService();
            var stats = service.GetDashboardStats();

            lblTitlesVal.Text = stats.totalTitles.ToString();
            lblAvailableVal.Text = stats.availableCopies.ToString();
            lblLoansVal.Text = stats.activeLoans.ToString();
            lblOverdueVal.Text = stats.overdueCount.ToString();

            LoadInventory("");
        }

        private void LayoutDashboard()
        {
            int margin = 25;
            int availableWidth = Math.Max(900, this.ClientSize.Width - (margin * 2));

            if (lblHeader != null)
            {
                lblHeader.Location = new Point(margin, 20);
            }

            if (txtTopSearch != null)
            {
                int searchWidth = Math.Min(360, Math.Max(280, availableWidth / 3));
                txtTopSearch.Size = new Size(searchWidth, 32);
                txtTopSearch.Location = new Point(this.ClientSize.Width - margin - searchWidth, 22);
            }

            if (pnlCardsContainer != null)
            {
                pnlCardsContainer.Location = new Point(margin, 70);
                pnlCardsContainer.Size = new Size(availableWidth, 100);
            }

            var tableContainer = pnlTableContainer;
            if (tableContainer != null)
            {
                tableContainer.Location = new Point(margin, 185);
                tableContainer.Size = new Size(availableWidth, Math.Max(300, this.ClientSize.Height - 210));
            }

            if (gridInventory != null && tableContainer != null)
            {
                gridInventory.Location = new Point(20, 55);
                gridInventory.Size = new Size(Math.Max(300, tableContainer.ClientSize.Width - 40), Math.Max(200, tableContainer.ClientSize.Height - 70));
            }
        }

        private void LoadInventory(string query)
        {
            var service = new CatalogService();
            var list = service.GetInventoryItems(query);
            gridInventory.DataSource = list;
        }

        private void GridInventory_CellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
        {
            var grid = sender as DataGridView;
            if (grid != null && e.RowIndex >= 0 && e.ColumnIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                e.Paint(e.CellBounds, DataGridViewPaintParts.All & ~DataGridViewPaintParts.ContentForeground);

                string status = e.Value.ToString()!;
                Color bg = Color.FromArgb(39, 174, 96); // Green
                Color text = Color.White;

                if (status == "Reference Only")
                {
                    bg = Color.FromArgb(241, 196, 15); // Yellow
                    text = Color.FromArgb(20, 20, 20);
                }
                else if (status == "Borrowed")
                {
                    bg = Color.FromArgb(52, 152, 219); // Blue
                    text = Color.White;
                }
                else if (status == "Reserved")
                {
                    bg = Color.FromArgb(230, 126, 34); // Orange
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
                    var graphics = e.Graphics!;
                    graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    graphics.FillPath(brush, path);
                    graphics.DrawString(status, font, textBrush, badgeRect, sf);
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
    }
}
