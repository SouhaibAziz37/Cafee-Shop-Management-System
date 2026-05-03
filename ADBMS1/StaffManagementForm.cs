using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;

namespace ADBMS1
{
    public partial class StaffManagementForm : Form
    {
        // Define UI components
        DataGridView dgvStaff = new DataGridView();
        TextBox txtStaffName = new TextBox();
        TextBox txtStaffPass = new TextBox();
        ComboBox cmbPosition = new ComboBox();

        public StaffManagementForm()
        {
            InitializeComponent();

            // CRITICAL: This line links the Load event so your buttons actually work
            this.Load += new EventHandler(StaffManagementForm_Load);

            this.Text = "Staff Management - Cafe System";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;
        }

        private void StaffManagementForm_Load(object sender, EventArgs e)
        {
            // --- HEADER ---
            Label lblTitle = new Label() { Text = "STAFF DIRECTORY", Font = new Font("Arial", 16, FontStyle.Bold), Top = 20, Left = 20, Width = 300 };

            // --- INPUT FIELDS ---
            Label lblName = new Label() { Text = "Username:", Top = 70, Left = 20, AutoSize = true };
            txtStaffName.Location = new Point(20, 95); txtStaffName.Width = 200;

            Label lblPass = new Label() { Text = "Temporary Password:", Top = 135, Left = 20, AutoSize = true };
            txtStaffPass.Location = new Point(20, 160); txtStaffPass.Width = 200;

            Label lblPos = new Label() { Text = "Position / Role:", Top = 200, Left = 20, AutoSize = true };
            cmbPosition.Location = new Point(20, 225); cmbPosition.Width = 200;
            cmbPosition.Items.AddRange(new string[] { "Staff", "Manager", "Owner" });
            cmbPosition.SelectedIndex = 0;

            // --- BUTTONS ---
            Button btnAddStaff = new Button()
            {
                Text = "ADD STAFF",
                Top = 275,
                Left = 20,
                Width = 200,
                Height = 40,
                BackColor = Color.SteelBlue,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            Button btnBack = new Button() { Text = "Back", Top = 400, Left = 20, Width = 150, Height = 35 };

            // --- DATA DISPLAY ---
            dgvStaff.Location = new Point(250, 70);
            dgvStaff.Size = new Size(500, 300);
            dgvStaff.BackgroundColor = Color.White;
            dgvStaff.ReadOnly = true;
            dgvStaff.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Add all controls to the Form
            this.Controls.AddRange(new Control[] { lblTitle, lblName, txtStaffName, lblPass, txtStaffPass, lblPos, cmbPosition, btnAddStaff, dgvStaff, btnBack });

            // Initial Data Load
            RefreshStaffGrid();

            // --- BUTTON EVENT: BACK ---
            btnBack.Click += (s, ev) => { this.Close(); };

            // --- BUTTON EVENT: ADD STAFF (SQL LOGIC) ---
            btnAddStaff.Click += (s, ev) => {
                if (string.IsNullOrWhiteSpace(txtStaffName.Text) || string.IsNullOrWhiteSpace(txtStaffPass.Text))
                {
                    MessageBox.Show("Please fill in all fields.");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(Connection.stringConn))
                {
                    try
                    {
                        string query = "INSERT INTO Users (Username, Password, Role) VALUES (@user, @pass, @role)";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@user", txtStaffName.Text);
                        cmd.Parameters.AddWithValue("@pass", txtStaffPass.Text);
                        cmd.Parameters.AddWithValue("@role", cmbPosition.SelectedItem.ToString());

                        conn.Open();
                        cmd.ExecuteNonQuery();

                        MessageBox.Show("Staff member added successfully!");
                        txtStaffName.Clear();
                        txtStaffPass.Clear();
                        RefreshStaffGrid();
                    }
                    catch (Exception ex) { MessageBox.Show("Database Error: " + ex.Message); }
                }
            };
        }

        private void RefreshStaffGrid()
        {
            using (SqlConnection conn = new SqlConnection(Connection.stringConn))
            {
                try
                {
                    SqlDataAdapter da = new SqlDataAdapter("SELECT UserID, Username, Role FROM Users", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvStaff.DataSource = dt;
                }
                catch (Exception ex) { MessageBox.Show("Data Load Error: " + ex.Message); }
            }
        }
    }
}