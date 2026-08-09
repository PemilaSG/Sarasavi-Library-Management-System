using System;
using System.Drawing;
using System.Windows.Forms;
using SarasaviLibrarySystem.UI.Views;

namespace SarasaviLibrarySystem.UI
{
    public class MainForm : Form
    {
        private Panel pnlHeader = null!;
        private Panel pnlSidebar = null!;
        private Panel pnlContent = null!;

        private Label lblClock = null!;
        private System.Windows.Forms.Timer timerClock = null!;

        private Button btnNavDashboard = null!;
        private Button btnNavBookReg = null!;
        private Button btnNavBorrowerReg = null!;
        private Button btnNavLoanCounter = null!;
        private Button btnNavReturnCounter = null!;
        private Button btnNavInquiry = null!;

        private Button currentActiveNavButton = null!;

        public MainForm()
        {
            InitializeComponent();
            LoadView(new DashboardView(), btnNavDashboard);
        }

        private void InitializeComponent()
        {
            this.Text = "Sarasavi Library Management System - IMBS Green Campus Final Project";
            this.Size = new Size(1440, 900);
            this.MinimumSize = new Size(1280, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(24, 27, 36);

            pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 65,
                BackColor = Color.FromArgb(30, 34, 45)
            };

            var lblLogo = new Label
            {
                Text = "📚 SARASAVI LIBRARY SYSTEM",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 204, 113),
                Location = new Point(20, 18),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblLogo);

            var lblUserBadge = new Label
            {
                Text = "👤 Active User: Senior Librarian (Admin)",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                Location = new Point(900, 22),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlHeader.Controls.Add(lblUserBadge);

            lblClock = new Label
            {
                Text = DateTime.Now.ToString("F"),
                Font = new Font("Segoe UI", 9.5F),
                ForeColor = Color.Gray,
                Location = new Point(1160, 23),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            pnlHeader.Controls.Add(lblClock);

            timerClock = new System.Windows.Forms.Timer { Interval = 1000 };
            timerClock.Tick += (s, e) => lblClock.Text = DateTime.Now.ToString("F");
            timerClock.Start();

            this.Controls.Add(pnlHeader);

            pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 230,
                BackColor = Color.FromArgb(22, 25, 34)
            };

            int top = 20;

            btnNavDashboard = CreateNavButton("📊  Dashboard", top);
            btnNavDashboard.Click += (s, e) => LoadView(new DashboardView(), btnNavDashboard);
            pnlSidebar.Controls.Add(btnNavDashboard);
            top += 55;

            btnNavBookReg = CreateNavButton("📘  Book Registration", top);
            btnNavBookReg.Click += (s, e) => LoadView(new BookRegistrationView(), btnNavBookReg);
            pnlSidebar.Controls.Add(btnNavBookReg);
            top += 55;

            btnNavBorrowerReg = CreateNavButton("👤  Borrower Entry", top);
            btnNavBorrowerReg.Click += (s, e) => LoadView(new BorrowerRegistrationView(), btnNavBorrowerReg);
            pnlSidebar.Controls.Add(btnNavBorrowerReg);
            top += 55;

            btnNavLoanCounter = CreateNavButton("🔄  Issue Loan Counter", top);
            btnNavLoanCounter.Click += (s, e) => LoadView(new LoanCounterView(), btnNavLoanCounter);
            pnlSidebar.Controls.Add(btnNavLoanCounter);
            top += 55;

            btnNavReturnCounter = CreateNavButton("↩️  Return Counter", top);
            btnNavReturnCounter.Click += (s, e) => LoadView(new ReturnCounterView(), btnNavReturnCounter);
            pnlSidebar.Controls.Add(btnNavReturnCounter);
            top += 55;

            btnNavInquiry = CreateNavButton("🔍  Catalog Search", top);
            btnNavInquiry.Click += (s, e) => LoadView(new InquiryView(), btnNavInquiry);
            pnlSidebar.Controls.Add(btnNavInquiry);

            this.Controls.Add(pnlSidebar);

            pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 27, 36)
            };
            this.Controls.Add(pnlContent);
            pnlContent.BringToFront();
        }

        private Button CreateNavButton(string text, int y)
        {
            var btn = new Button
            {
                Text = text,
                Location = new Point(10, y),
                Size = new Size(210, 48),
                BackColor = Color.FromArgb(22, 25, 34),
                ForeColor = Color.Gainsboro,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(15, 0, 0, 0),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private void LoadView(UserControl view, Button navButton)
        {
            if (currentActiveNavButton != null)
            {
                currentActiveNavButton.BackColor = Color.FromArgb(22, 25, 34);
                currentActiveNavButton.ForeColor = Color.Gainsboro;
            }

            currentActiveNavButton = navButton;
            currentActiveNavButton.BackColor = Color.FromArgb(41, 128, 185);
            currentActiveNavButton.ForeColor = Color.White;

            pnlContent.Controls.Clear();
            view.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(view);
        }
    }
}
