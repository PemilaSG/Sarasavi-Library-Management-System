using System;
using System.Drawing;
using System.Windows.Forms;
using SarasaviLibrarySystem.Services;

namespace SarasaviLibrarySystem.UI.Views
{
    public class ReturnCounterView : UserControl
    {
        private TextBox txtCopyCode = null!;
        private Button btnProcessReturn = null!;
        private Button btnTestAlert = null!;
        private Label lblStatusMsg = null!;

        public ReturnCounterView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(18, 22, 33);

            var lblHeader = new Label
            {
                Text = "Book Return Counter",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(25, 20),
                AutoSize = true
            };
            this.Controls.Add(lblHeader);

            var pnlReturnCard = new Panel
            {
                Location = new Point(25, 75),
                Size = new Size(600, 320),
                BackColor = Color.FromArgb(26, 32, 46)
            };

            pnlReturnCard.Controls.Add(new Label
            {
                Text = "Scan or Enter Returned Copy Accession Code:",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                Location = new Point(20, 25),
                AutoSize = true
            });

            txtCopyCode = new TextBox
            {
                Location = new Point(20, 60),
                Size = new Size(560, 34),
                Font = new Font("Segoe UI", 12F),
                BackColor = Color.FromArgb(36, 44, 62),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "C0002-01"
            };
            pnlReturnCard.Controls.Add(txtCopyCode);

            btnProcessReturn = new Button
            {
                Text = "Process Return",
                Location = new Point(20, 110),
                Size = new Size(270, 45),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnProcessReturn.FlatAppearance.BorderSize = 0;
            btnProcessReturn.Click += BtnProcessReturn_Click;
            pnlReturnCard.Controls.Add(btnProcessReturn);

            btnTestAlert = new Button
            {
                Text = "Test Reservation Alert Modal Popup",
                Location = new Point(310, 110),
                Size = new Size(270, 45),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTestAlert.FlatAppearance.BorderSize = 0;
            btnTestAlert.Click += (s, e) => ShowReservationAlertModal("C0002-01", "C# 10 and .NET 6 Modern Cross-Platform Development", "M-1002 (Nimali Fernando)");
            pnlReturnCard.Controls.Add(btnTestAlert);

            lblStatusMsg = new Label
            {
                Text = "Ready to accept returned book copies.",
                Font = new Font("Segoe UI", 10.5F),
                ForeColor = Color.Gray,
                Location = new Point(20, 175),
                Size = new Size(560, 120)
            };
            pnlReturnCard.Controls.Add(lblStatusMsg);

            this.Controls.Add(pnlReturnCard);

            var pnlInstructions = new Panel
            {
                Location = new Point(650, 75),
                Size = new Size(500, 320),
                BackColor = Color.FromArgb(26, 32, 46)
            };

            pnlInstructions.Controls.Add(new Label
            {
                Text = "Return Counter Rules & Guidance",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 20),
                AutoSize = true
            });

            pnlInstructions.Controls.Add(new Label
            {
                Text = "• Scanning a copy marks its active loan as RETURNED.\n\n" +
                       "• The FIFO Reservation Queue is automatically scanned.\n\n" +
                       "• If a reservation exists, a High-Visibility Alert Modal pops up instructing the librarian to place the copy in the Set-Aside shelf.\n\n" +
                       "• The reservation status is updated to 'Fulfilled'.",
                Font = new Font("Segoe UI", 10F),
                ForeColor = Color.Gainsboro,
                Location = new Point(20, 60),
                Size = new Size(460, 230)
            });

            this.Controls.Add(pnlInstructions);
        }

        private void BtnProcessReturn_Click(object? sender, EventArgs e)
        {
            string copyCode = txtCopyCode.Text.Trim();
            var service = new ReturnService();

            if (service.ProcessReturn(copyCode, out string msg, out bool isReserved, out string resUserNum))
            {
                lblStatusMsg.Text = msg;
                lblStatusMsg.ForeColor = Color.FromArgb(46, 204, 113);

                if (isReserved)
                {
                    var catalogService = new CatalogService();
                    var items = catalogService.GetInventoryItems(copyCode);
                    string title = items.Count > 0 ? items[0].Title : "Book Title";

                    ShowReservationAlertModal(copyCode, title, resUserNum);
                }
            }
            else
            {
                lblStatusMsg.Text = msg;
                lblStatusMsg.ForeColor = Color.FromArgb(231, 76, 60);
            }
        }

        private void ShowReservationAlertModal(string copyCode, string title, string memberInfo)
        {
            using var dlg = new Form
            {
                Text = "⚠️ RESERVATION SET-ASIDE NOTIFICATION ALERT",
                Size = new Size(550, 320),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(26, 32, 46)
            };

            var pnlTop = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(230, 126, 34)
            };
            pnlTop.Controls.Add(new Label
            {
                Text = "🔔 RESERVED BOOK - DO NOT PUT ON PUBLIC SHELF",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 18),
                AutoSize = true
            });
            dlg.Controls.Add(pnlTop);

            var lblBody = new Label
            {
                Text = $"Returned Copy Accession Code: {copyCode}\n" +
                       $"Book Title: {title}\n\n" +
                       $"RESERVED FOR MEMBER: {memberInfo}\n\n" +
                       $"Action Required: Please place this physical copy in the Reserved Set-Aside Shelf immediately.",
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                Location = new Point(25, 80),
                Size = new Size(480, 140)
            };
            dlg.Controls.Add(lblBody);

            var btnOk = new Button
            {
                Text = "Acknowledge & Set Aside",
                DialogResult = DialogResult.OK,
                Location = new Point(175, 230),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(41, 128, 185),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnOk.FlatAppearance.BorderSize = 0;
            dlg.Controls.Add(btnOk);

            dlg.ShowDialog();
        }
    }
}
