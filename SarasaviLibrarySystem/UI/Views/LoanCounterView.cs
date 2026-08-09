using System;
using System.Drawing;
using System.Windows.Forms;
using SarasaviLibrarySystem.Services;

namespace SarasaviLibrarySystem.UI.Views
{
    public class LoanCounterView : UserControl
    {
        private TextBox txtUserNumber = null!;
        private Button btnCheckMember = null!;
        private Label lblMemberStatus = null!;
        private Label lblActiveLoansCount = null!;

        private TextBox txtCopyCode = null!;
        private Button btnIssueLoan = null!;

        private Label lblCheckLimit = null!;
        private Label lblCheckOverdue = null!;
        private Label lblCheckReference = null!;
        private Label lblDueDate = null!;

        public LoanCounterView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(24, 27, 36);

            var lblHeader = new Label
            {
                Text = "Loan Issue Counter (Checkout)",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 20),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            var pnlMember = new Panel
            {
                Location = new Point(25, 70),
                Size = new Size(570, 260),
                BackColor = Color.FromArgb(34, 39, 53)
            };

            pnlMember.Controls.Add(new Label
            {
                Text = "Step 1: Borrower Verification",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                Location = new Point(20, 15),
                AutoSize = true
            });

            pnlMember.Controls.Add(new Label
            {
                Text = "User Number (e.g. M-1001):",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(20, 50),
                AutoSize = true
            });

            txtUserNumber = new TextBox
            {
                Location = new Point(20, 75),
                Size = new Size(360, 30),
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(45, 52, 71),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "M-1001"
            };
            pnlMember.Controls.Add(txtUserNumber);

            btnCheckMember = new Button
            {
                Text = "Verify",
                Location = new Point(395, 74),
                Size = new Size(155, 33),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnCheckMember.FlatAppearance.BorderSize = 0;
            btnCheckMember.Click += BtnCheckMember_Click;
            pnlMember.Controls.Add(btnCheckMember);

            lblMemberStatus = new Label
            {
                Text = "Member Name: Kamal Perera (M-1001)",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                Location = new Point(20, 130),
                AutoSize = true
            };
            pnlMember.Controls.Add(lblMemberStatus);

            lblActiveLoansCount = new Label
            {
                Text = "Active Borrowed Loans: 1 of 5 Allowed",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.Gainsboro,
                Location = new Point(20, 165),
                AutoSize = true
            };
            pnlMember.Controls.Add(lblActiveLoansCount);

            this.Controls.Add(pnlMember);

            var pnlLoan = new Panel
            {
                Location = new Point(25, 350),
                Size = new Size(570, 300),
                BackColor = Color.FromArgb(34, 39, 53)
            };

            pnlLoan.Controls.Add(new Label
            {
                Text = "Step 2: Book Copy Selection",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                Location = new Point(20, 15),
                AutoSize = true
            });

            pnlLoan.Controls.Add(new Label
            {
                Text = "Scan / Enter Copy Accession Code (e.g. C0002-02):",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Gray,
                Location = new Point(20, 50),
                AutoSize = true
            });

            txtCopyCode = new TextBox
            {
                Location = new Point(20, 75),
                Size = new Size(530, 30),
                Font = new Font("Segoe UI", 11F),
                BackColor = Color.FromArgb(45, 52, 71),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "C0002-02"
            };
            pnlLoan.Controls.Add(txtCopyCode);

            lblDueDate = new Label
            {
                Text = $"Auto Calculated Due Date: {DateTime.Now.AddDays(14):yyyy-MM-dd} (14 Days Loan)",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(230, 126, 34),
                Location = new Point(20, 130),
                AutoSize = true
            };
            pnlLoan.Controls.Add(lblDueDate);

            btnIssueLoan = new Button
            {
                Text = "Issue Book Loan (14 Days)",
                Location = new Point(20, 180),
                Size = new Size(530, 50),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnIssueLoan.FlatAppearance.BorderSize = 0;
            btnIssueLoan.Click += BtnIssueLoan_Click;
            pnlLoan.Controls.Add(btnIssueLoan);

            this.Controls.Add(pnlLoan);

            var pnlRules = new Panel
            {
                Location = new Point(620, 70),
                Size = new Size(580, 580),
                BackColor = Color.FromArgb(34, 39, 53)
            };

            pnlRules.Controls.Add(new Label
            {
                Text = "Automated Rules Checklist Engine",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                Location = new Point(25, 20),
                AutoSize = true
            });

            lblCheckLimit = CreateCheckItem("✔ 1. Active Loans Limit Rule (Max 5 Books)", Color.FromArgb(46, 204, 113), 25, 80);
            lblCheckOverdue = CreateCheckItem("✔ 2. Overdue Books Rule (No overdue items)", Color.FromArgb(46, 204, 113), 25, 140);
            lblCheckReference = CreateCheckItem("✔ 3. Access Type Rule (Copy is Borrowable)", Color.FromArgb(46, 204, 113), 25, 200);

            pnlRules.Controls.Add(lblCheckLimit);
            pnlRules.Controls.Add(lblCheckOverdue);
            pnlRules.Controls.Add(lblCheckReference);

            var pnlRuleDesc = new Panel
            {
                Location = new Point(25, 270),
                Size = new Size(530, 280),
                BackColor = Color.FromArgb(24, 27, 36)
            };

            pnlRuleDesc.Controls.Add(new Label
            {
                Text = "Assignment Rule Specifications Enforced:\n\n" +
                       "• Rule 01: Maximum 5 books on loan per borrower.\n" +
                       "• Rule 02: Overdue unreturned books block further checkout.\n" +
                       "• Rule 03: Reference Only copies cannot be issued.\n" +
                       "• Rule 04: Standard loan period is 2 weeks (14 Days).\n" +
                       "• Rule 05: Librarian has full authorization control.",
                Font = new Font("Segoe UI", 10.5F),
                ForeColor = Color.Gainsboro,
                Location = new Point(20, 20),
                Size = new Size(490, 240)
            });

            pnlRules.Controls.Add(pnlRuleDesc);
            this.Controls.Add(pnlRules);
        }

        private Label CreateCheckItem(string text, Color color, int x, int y)
        {
            return new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = color,
                Location = new Point(x, y),
                AutoSize = true
            };
        }

        private void BtnCheckMember_Click(object? sender, EventArgs e)
        {
            string userNum = txtUserNumber.Text.Trim();
            var service = new BorrowerService();
            var b = service.GetBorrowerDetails(userNum);

            if (b == null)
            {
                lblMemberStatus.Text = "Member Status: Borrower NOT Found!";
                lblMemberStatus.ForeColor = Color.FromArgb(192, 57, 43);
                lblActiveLoansCount.Text = "Active Borrowed Loans: -";
                lblCheckLimit.Text = "❌ 1. Active Loans Limit Rule (Borrower Not Found)";
                lblCheckLimit.ForeColor = Color.FromArgb(192, 57, 43);
                return;
            }

            lblMemberStatus.Text = $"Member Name: {b.Name} ({b.UserNumber})";
            lblMemberStatus.ForeColor = Color.FromArgb(46, 204, 113);
            lblActiveLoansCount.Text = $"Active Borrowed Loans: {b.ActiveLoansCount} of 5 Allowed";

            if (b.ActiveLoansCount >= 5)
            {
                lblCheckLimit.Text = "❌ 1. Active Loans Limit Rule (5 Books Max Reached!)";
                lblCheckLimit.ForeColor = Color.FromArgb(192, 57, 43);
            }
            else
            {
                lblCheckLimit.Text = $"✔ 1. Active Loans Limit Rule ({b.ActiveLoansCount}/5 Books)";
                lblCheckLimit.ForeColor = Color.FromArgb(46, 204, 113);
            }

            if (b.HasOverdueLoans)
            {
                lblCheckOverdue.Text = "❌ 2. Overdue Books Rule (Borrower Has Overdue Books!)";
                lblCheckOverdue.ForeColor = Color.FromArgb(192, 57, 43);
            }
            else
            {
                lblCheckOverdue.Text = "✔ 2. Overdue Books Rule (No Overdue Items)";
                lblCheckOverdue.ForeColor = Color.FromArgb(46, 204, 113);
            }
        }

        private void BtnIssueLoan_Click(object? sender, EventArgs e)
        {
            string userNum = txtUserNumber.Text.Trim();
            string copyCode = txtCopyCode.Text.Trim();

            if (string.IsNullOrWhiteSpace(userNum) || string.IsNullOrWhiteSpace(copyCode))
            {
                MessageBox.Show("Please enter User Number and Copy Accession Code.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var service = new LoanService();
            if (service.IssueLoan(userNum, copyCode, out string msg))
            {
                MessageBox.Show(msg, "Checkout Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                BtnCheckMember_Click(sender, e);
            }
            else
            {
                MessageBox.Show(msg, "Checkout Blocked", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
