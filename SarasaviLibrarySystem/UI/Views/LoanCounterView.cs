using System;
using System.Drawing;
using System.Windows.Forms;
using SarasaviLibrarySystem.Services;

namespace SarasaviLibrarySystem.UI.Views
{
    public class LoanCounterView : UserControl
    {
        private TextBox txtUserNumber = null!;
        private Button btnVerifyMember = null!;
        private Label lblMemberName = null!;
        private Label lblMemberContact = null!;
        private ProgressBar prgActiveLoans = null!;
        private Label lblLoansText = null!;

        private TextBox txtCopyCode = null!;
        private Button btnScan = null!;
        private Panel pnlBookFound = null!;
        private Label lblBookFound = null!;

        private Label lblCheckLimit = null!;
        private Label lblCheckOverdue = null!;
        private Label lblCheckReference = null!;
        private Label lblDueDate = null!;

        private Button btnIssueLoan = null!;
        private DataGridView gridIssueQueue = null!;

        public LoanCounterView()
        {
            InitializeComponent();
            VerifyMember("M-1001");
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(18, 22, 33);

            var lblHeader = new Label
            {
                Text = "Loan Issue Counter",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 18),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            // 1. Member Information Panel (Left Side)
            var pnlMember = new Panel
            {
                Location = new Point(25, 65),
                Size = new Size(460, 600),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom,
                BackColor = Color.FromArgb(26, 32, 46)
            };

            pnlMember.Controls.Add(new Label
            {
                Text = "1. Member Information",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            });

            pnlMember.Controls.Add(new Label
            {
                Text = "Search Member (User Number / NIC):",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(20, 50),
                AutoSize = true
            });

            txtUserNumber = new TextBox
            {
                Location = new Point(20, 72),
                Size = new Size(300, 30),
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(36, 44, 62),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "M-1001"
            };
            pnlMember.Controls.Add(txtUserNumber);

            btnVerifyMember = new Button
            {
                Text = "Verify",
                Location = new Point(330, 71),
                Size = new Size(110, 32),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnVerifyMember.FlatAppearance.BorderSize = 0;
            btnVerifyMember.Click += (s, e) => VerifyMember(txtUserNumber.Text.Trim());
            pnlMember.Controls.Add(btnVerifyMember);

            var pnlProfileCard = new Panel
            {
                Location = new Point(20, 120),
                Size = new Size(420, 240),
                BackColor = Color.FromArgb(34, 42, 60)
            };

            var lblAvatar = new Label
            {
                Text = "👤",
                Font = new Font("Segoe UI", 36F),
                ForeColor = Color.FromArgb(41, 128, 185),
                Location = new Point(15, 15),
                Size = new Size(70, 70)
            };
            pnlProfileCard.Controls.Add(lblAvatar);

            lblMemberName = new Label
            {
                Text = "Borrower: Kamal Perera (M-1001)",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(90, 20),
                AutoSize = true
            };
            pnlProfileCard.Controls.Add(lblMemberName);

            lblMemberContact = new Label
            {
                Text = "NIC: 921820482V\nAddress: 123 Galle Road, Colombo 03",
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.Gainsboro,
                Location = new Point(90, 48),
                Size = new Size(310, 45)
            };
            pnlProfileCard.Controls.Add(lblMemberContact);

            pnlProfileCard.Controls.Add(new Label
            {
                Text = "Active Loans Limit Progress:",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(20, 115),
                AutoSize = true
            });

            prgActiveLoans = new ProgressBar
            {
                Location = new Point(20, 140),
                Size = new Size(380, 22),
                Maximum = 5,
                Value = 1
            };
            pnlProfileCard.Controls.Add(prgActiveLoans);

            lblLoansText = new Label
            {
                Text = "1 Book Borrowed / 5 Books Limit (4 Available)",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                Location = new Point(20, 175),
                AutoSize = true
            };
            pnlProfileCard.Controls.Add(lblLoansText);

            pnlMember.Controls.Add(pnlProfileCard);
            this.Controls.Add(pnlMember);

            // 2. Add Books to Issue Panel (Right Side)
            var pnlIssue = new Panel
            {
                Location = new Point(505, 65),
                Size = new Size(700, 600),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackColor = Color.FromArgb(26, 32, 46)
            };

            pnlIssue.Controls.Add(new Label
            {
                Text = "2. Add Books to Issue",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            });

            pnlIssue.Controls.Add(new Label
            {
                Text = "Scan / Enter Accession Code:",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(20, 50),
                AutoSize = true
            });

            txtCopyCode = new TextBox
            {
                Location = new Point(20, 72),
                Size = new Size(480, 30),
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(36, 44, 62),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "C0002-02"
            };
            pnlIssue.Controls.Add(txtCopyCode);

            btnScan = new Button
            {
                Text = "Scan / Verify",
                Location = new Point(510, 71),
                Size = new Size(165, 32),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnScan.FlatAppearance.BorderSize = 0;
            btnScan.Click += BtnScan_Click;
            pnlIssue.Controls.Add(btnScan);

            // Book Found Banner
            pnlBookFound = new Panel
            {
                Location = new Point(20, 115),
                Size = new Size(655, 40),
                BackColor = Color.FromArgb(39, 174, 96)
            };
            lblBookFound = new Label
            {
                Text = "✅ Book Found: 'C# 10 and .NET 6 Modern Cross-Platform Development' by Mark J. Price",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(10, 10),
                AutoSize = true
            };
            pnlBookFound.Controls.Add(lblBookFound);
            pnlIssue.Controls.Add(pnlBookFound);

            // Automated Rules Validation Box
            var pnlRulesVal = new Panel
            {
                Location = new Point(20, 165),
                Size = new Size(655, 150),
                BackColor = Color.FromArgb(34, 42, 60)
            };

            pnlRulesVal.Controls.Add(new Label
            {
                Text = "Automated Business Rules Validation:",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(15, 12),
                AutoSize = true
            });

            lblCheckLimit = CreateCheckLabel("✔ Within 5 book limit (1 of 5 checked out)", Color.FromArgb(46, 204, 113), 15, 40);
            lblCheckOverdue = CreateCheckLabel("✔ No overdue loans on borrower account", Color.FromArgb(46, 204, 113), 15, 70);
            lblCheckReference = CreateCheckLabel("✔ Copy is borrowable (Status: Available)", Color.FromArgb(46, 204, 113), 15, 100);

            pnlRulesVal.Controls.Add(lblCheckLimit);
            pnlRulesVal.Controls.Add(lblCheckOverdue);
            pnlRulesVal.Controls.Add(lblCheckReference);

            pnlIssue.Controls.Add(pnlRulesVal);

            lblDueDate = new Label
            {
                Text = $"Calculated Return Due Date: {DateTime.Now.AddDays(14):yyyy-MM-dd} (14 Days Loan Period)",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(230, 126, 34),
                Location = new Point(20, 325),
                AutoSize = true
            };
            pnlIssue.Controls.Add(lblDueDate);

            btnIssueLoan = new Button
            {
                Text = "Issue Loan (14 Days)",
                Location = new Point(480, 320),
                Size = new Size(195, 42),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnIssueLoan.FlatAppearance.BorderSize = 0;
            btnIssueLoan.Click += BtnIssueLoan_Click;
            pnlIssue.Controls.Add(btnIssueLoan);

            // Current Issue List Table
            pnlIssue.Controls.Add(new Label
            {
                Text = "Current Checkout Issue List",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                Location = new Point(20, 375),
                AutoSize = true
            });

            gridIssueQueue = new DataGridView
            {
                Location = new Point(20, 405),
                Size = new Size(665, 180),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
                BackgroundColor = Color.FromArgb(18, 22, 33),
                BorderStyle = BorderStyle.None,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F),
                RowHeadersVisible = false,
                AllowUserToAddRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                RowTemplate = { Height = 32 }
            };

            gridIssueQueue.EnableHeadersVisualStyles = false;
            gridIssueQueue.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(34, 42, 60);
            gridIssueQueue.ColumnHeadersDefaultCellStyle.ForeColor = Color.Gainsboro;
            gridIssueQueue.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
            gridIssueQueue.ColumnHeadersHeight = 32;
            gridIssueQueue.DefaultCellStyle.BackColor = Color.FromArgb(18, 22, 33);
            gridIssueQueue.DefaultCellStyle.SelectionBackColor = Color.FromArgb(45, 56, 80);
            gridIssueQueue.DefaultCellStyle.SelectionForeColor = Color.White;

            gridIssueQueue.Columns.Add("AccessionCode", "Accession Code");
            gridIssueQueue.Columns.Add("Title", "Title");
            gridIssueQueue.Columns.Add("Author", "Author");
            gridIssueQueue.Columns.Add("DueDate", "Due Date");

            gridIssueQueue.Columns["AccessionCode"].Width = 160;
            gridIssueQueue.Columns["Title"].Width = 245;
            gridIssueQueue.Columns["Author"].Width = 160;
            gridIssueQueue.Columns["DueDate"].Width = 120;

            pnlIssue.Controls.Add(gridIssueQueue);
            this.Controls.Add(pnlIssue);
        }

        private Label CreateCheckLabel(string text, Color color, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private void VerifyMember(string userNum)
        {
            var service = new BorrowerService();
            var b = service.GetBorrowerDetails(userNum);

            if (b == null)
            {
                lblMemberName.Text = "Borrower NOT Found!";
                lblMemberName.ForeColor = Color.FromArgb(192, 57, 43);
                lblMemberContact.Text = "Invalid User Number entered.";
                prgActiveLoans.Value = 0;
                lblLoansText.Text = "0 Books Borrowed / 5 Limit";
                lblLoansText.ForeColor = Color.FromArgb(192, 57, 43);
                lblCheckLimit.Text = "❌ Within 5 book limit (Borrower Not Found)";
                lblCheckLimit.ForeColor = Color.FromArgb(192, 57, 43);
                return;
            }

            lblMemberName.Text = $"Borrower: {b.Name} ({b.UserNumber})";
            lblMemberName.ForeColor = Color.White;
            lblMemberContact.Text = $"NIC: {b.NIC}\nAddress: {b.Address}";
            
            prgActiveLoans.Value = Math.Min(b.ActiveLoansCount, 5);
            lblLoansText.Text = $"{b.ActiveLoansCount} Books Borrowed / 5 Books Limit ({5 - b.ActiveLoansCount} Available)";

            if (b.ActiveLoansCount >= 5)
            {
                lblCheckLimit.Text = "❌ Within 5 book limit (Max 5 Books Limit Reached!)";
                lblCheckLimit.ForeColor = Color.FromArgb(192, 57, 43);
                lblLoansText.ForeColor = Color.FromArgb(192, 57, 43);
            }
            else
            {
                lblCheckLimit.Text = $"✔ Within 5 book limit ({b.ActiveLoansCount} of 5 checked out)";
                lblCheckLimit.ForeColor = Color.FromArgb(46, 204, 113);
                lblLoansText.ForeColor = Color.FromArgb(46, 204, 113);
            }

            if (b.HasOverdueLoans)
            {
                lblCheckOverdue.Text = "❌ No overdue loans (Borrower has overdue unreturned books!)";
                lblCheckOverdue.ForeColor = Color.FromArgb(192, 57, 43);
            }
            else
            {
                lblCheckOverdue.Text = "✔ No overdue loans on borrower account";
                lblCheckOverdue.ForeColor = Color.FromArgb(46, 204, 113);
            }
        }

        private void BtnScan_Click(object? sender, EventArgs e)
        {
            string copyCode = txtCopyCode.Text.Trim();
            var service = new CatalogService();
            var items = service.GetInventoryItems(copyCode);

            if (items.Count > 0)
            {
                var item = items[0];
                lblBookFound.Text = $"✅ Book Found: '{item.Title}' by {item.Author}";
                pnlBookFound.BackColor = Color.FromArgb(39, 174, 96);

                if (item.Status == "Reference Only")
                {
                    lblCheckReference.Text = "❌ Copy is borrowable (Status: REFERENCE ONLY - Cannot Borrow)";
                    lblCheckReference.ForeColor = Color.FromArgb(192, 57, 43);
                }
                else if (item.Status != "Available")
                {
                    lblCheckReference.Text = $"❌ Copy is borrowable (Status: {item.Status} - Unavailable)";
                    lblCheckReference.ForeColor = Color.FromArgb(192, 57, 43);
                }
                else
                {
                    lblCheckReference.Text = "✔ Copy is borrowable (Status: Available)";
                    lblCheckReference.ForeColor = Color.FromArgb(46, 204, 113);
                }
            }
            else
            {
                lblBookFound.Text = $"❌ Copy '{copyCode}' Not Found in Inventory!";
                pnlBookFound.BackColor = Color.FromArgb(192, 57, 43);
            }
        }

        private void BtnIssueLoan_Click(object? sender, EventArgs e)
        {
            string userNum = txtUserNumber.Text.Trim();
            string copyCode = txtCopyCode.Text.Trim();

            var service = new LoanService();
            if (service.IssueLoan(userNum, copyCode, out string msg))
            {
                MessageBox.Show(msg, "Loan Issued", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                var catalogService = new CatalogService();
                var items = catalogService.GetInventoryItems(copyCode);
                string title = items.Count > 0 ? items[0].Title : "Book";
                string author = items.Count > 0 ? items[0].Author : "Author";

                gridIssueQueue.Rows.Add(copyCode, title, author, DateTime.Now.AddDays(14).ToString("yyyy-MM-dd"));
                VerifyMember(userNum);
            }
            else
            {
                MessageBox.Show(msg, "Checkout Blocked", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
