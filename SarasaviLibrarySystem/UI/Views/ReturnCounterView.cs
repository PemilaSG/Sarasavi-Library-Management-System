using System;
using System.Drawing;
using System.Windows.Forms;
using SarasaviLibrarySystem.Services;

namespace SarasaviLibrarySystem.UI.Views
{
    public class ReturnCounterView : UserControl
    {
        private TextBox txtCopyCode = null!;
        private Button btnReturn = null!;
        private Label lblLastReturnStatus = null!;

        public ReturnCounterView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(24, 27, 36);

            var lblHeader = new Label
            {
                Text = "Book Return Counter & Reservation Queue Alert",
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

            pnlForm.Controls.Add(new Label
            {
                Text = "Scan / Enter Copy Accession Number:",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                Location = new Point(25, 25),
                AutoSize = true
            });

            txtCopyCode = new TextBox
            {
                Location = new Point(25, 60),
                Size = new Size(550, 35),
                Font = new Font("Segoe UI", 12F),
                BackColor = Color.FromArgb(45, 52, 71),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Text = "C0002-01"
            };
            pnlForm.Controls.Add(txtCopyCode);

            btnReturn = new Button
            {
                Text = "Process Return Book",
                Location = new Point(25, 115),
                Size = new Size(550, 50),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnReturn.FlatAppearance.BorderSize = 0;
            btnReturn.Click += BtnReturn_Click;
            pnlForm.Controls.Add(btnReturn);

            lblLastReturnStatus = new Label
            {
                Text = "Status: Ready to accept book returns.",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                Location = new Point(25, 185),
                Size = new Size(550, 80)
            };
            pnlForm.Controls.Add(lblLastReturnStatus);

            var pnlInfo = new Panel
            {
                Location = new Point(25, 280),
                Size = new Size(550, 270),
                BackColor = Color.FromArgb(24, 27, 36)
            };

            pnlInfo.Controls.Add(new Label
            {
                Text = "Automated Reservation Queue Handling:\n\n" +
                       "1. Librarian enters returned copy Accession Number.\n" +
                       "2. Active loan record is closed and marked Returned.\n" +
                       "3. System checks for pending title reservations (FIFO queue).\n" +
                       "4. IF RESERVED: Copy status set to 'Reserved' and a POPUP ALERT notifies librarian to SET ASIDE book for reserving member.",
                Font = new Font("Segoe UI", 10.5F),
                ForeColor = Color.Gainsboro,
                Location = new Point(20, 20),
                Size = new Size(510, 230)
            });

            pnlForm.Controls.Add(pnlInfo);
            this.Controls.Add(pnlForm);

            var pnlSample = new Panel
            {
                Location = new Point(650, 70),
                Size = new Size(550, 580),
                BackColor = Color.FromArgb(34, 39, 53)
            };

            pnlSample.Controls.Add(new Label
            {
                Text = "Reservation Alert Popup Preview Demo",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.Gainsboro,
                Location = new Point(20, 20),
                AutoSize = true
            });

            var btnTestAlert = new Button
            {
                Text = "Test Reservation Popup Alert",
                Location = new Point(20, 60),
                Size = new Size(510, 45),
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnTestAlert.FlatAppearance.BorderSize = 0;
            btnTestAlert.Click += (s, e) => ShowReservationPopup("🔔 [RESERVATION SET-ASIDE ALERT]\nBook Title: 'C# 10 and .NET 6 Modern Cross-Platform Development'\nCopy Returned: C0002-01\n\n⚠️ THIS TITLE HAS AN ACTIVE RESERVATION!\nPlease SET ASIDE this book for Member: Nimali Fernando (M-1002)\nReservation Request Date: 2026-08-07");
            pnlSample.Controls.Add(btnTestAlert);

            this.Controls.Add(pnlSample);
        }

        private void BtnReturn_Click(object? sender, EventArgs e)
        {
            string copyCode = txtCopyCode.Text.Trim();
            if (string.IsNullOrWhiteSpace(copyCode))
            {
                MessageBox.Show("Please enter a valid Copy Accession Number.", "Validation Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var service = new ReturnService();
            if (service.ProcessReturn(copyCode, out string msg, out string? resAlert))
            {
                lblLastReturnStatus.Text = msg;
                lblLastReturnStatus.ForeColor = Color.FromArgb(46, 204, 113);

                if (resAlert != null)
                {
                    ShowReservationPopup(resAlert);
                }
                else
                {
                    MessageBox.Show(msg, "Return Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                lblLastReturnStatus.Text = msg;
                lblLastReturnStatus.ForeColor = Color.FromArgb(192, 57, 43);
                MessageBox.Show(msg, "Return Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ShowReservationPopup(string alertMessage)
        {
            using var alertDlg = new Form
            {
                Text = "⚠️ RESERVATION SET-ASIDE ALERT",
                Size = new Size(550, 360),
                StartPosition = FormStartPosition.CenterParent,
                BackColor = Color.FromArgb(34, 39, 53),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var lblMsg = new Label
            {
                Text = alertMessage,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 196, 15),
                Location = new Point(25, 25),
                Size = new Size(480, 220)
            };

            var btnAck = new Button
            {
                Text = "Confirm & Set Aside Book",
                Location = new Point(160, 260),
                Size = new Size(220, 40),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.OK
            };

            alertDlg.Controls.Add(lblMsg);
            alertDlg.Controls.Add(btnAck);
            alertDlg.ShowDialog();
        }
    }
}
