using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data.SqlClient; // Added for SQL connectivity

namespace ADBMS1
{
    public partial class Form1 : Form
    {
        TextBox txtUser = new TextBox();
        TextBox txtPass = new TextBox();
        ComboBox cmbRole = new ComboBox();
        Button btnLogin = new Button();

        public Form1()
        {
            InitializeComponent();
            this.Text = "Cafe Management - Login";
            this.Size = new Size(320, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.SeaShell;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Label lblTitle = new Label() { Text = "CAFE SYSTEM", Font = new Font("Arial", 16, FontStyle.Bold), Top = 20, Left = 50, Width = 200, TextAlign = ContentAlignment.MiddleCenter };

            Label lblUser = new Label() { Text = "Username:", Top = 75, Left = 40, AutoSize = true };
            txtUser.Location = new Point(40, 100); txtUser.Width = 220;

            Label lblPass = new Label() { Text = "Password:", Top = 140, Left = 40, AutoSize = true };
            txtPass.Location = new Point(40, 165); txtPass.Width = 220;
            txtPass.PasswordChar = '●';

            Label lblRole = new Label() { Text = "Login Role:", Top = 205, Left = 40, AutoSize = true };
            cmbRole.Location = new Point(40, 230); cmbRole.Width = 220;
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRole.Items.AddRange(new string[] { "Admin/Owner", "Staff/Cashier" });
            cmbRole.SelectedIndex = 0;

            btnLogin.Text = "LOGIN";
            btnLogin.Location = new Point(40, 290);
            btnLogin.Size = new Size(220, 45);
            btnLogin.BackColor = Color.Sienna;
            btnLogin.ForeColor = Color.White;
            btnLogin.FlatStyle = FlatStyle.Flat;
            btnLogin.Font = new Font("Arial", 10, FontStyle.Bold);

            // --- DATABASE LOGIC ADDED BELOW ---
            btnLogin.Click += (s, ev) => {
                if (cmbRole.SelectedItem == null) return;

                string selectedRole = cmbRole.SelectedItem.ToString() ?? "";

                // Map the ComboBox text to the Role names in your Database
                string dbRole = (selectedRole == "Admin/Owner") ? "Owner" : "Staff";

                if (string.IsNullOrEmpty(txtUser.Text) || string.IsNullOrEmpty(txtPass.Text))
                {
                    MessageBox.Show("Please enter credentials.");
                    return;
                }

                // Database connectivity block
                using (SqlConnection conn = new SqlConnection(Connection.stringConn))
                {
                    try
                    {
                        // Check if user exists with matching username, password, and role
                        string query = "SELECT COUNT(*) FROM Users WHERE Username=@user AND Password=@pass AND Role=@role";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@user", txtUser.Text);
                        cmd.Parameters.AddWithValue("@pass", txtPass.Text);
                        cmd.Parameters.AddWithValue("@role", dbRole);

                        conn.Open();
                        int userCount = (int)cmd.ExecuteScalar();

                        if (userCount > 0)
                        {
                            MessageBox.Show("Login Successful!");

                            if (dbRole == "Owner")
                            {
                                OwnerForm owner = new OwnerForm();
                                owner.Show();
                            }
                            else
                            {
                                CustomerForm customer = new CustomerForm();
                                customer.Show();
                            }
                            this.Hide();
                        }
                        else
                        {
                            MessageBox.Show("Invalid Username, Password, or Role.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Database Error: " + ex.Message);
                    }
                }
            };

            this.Controls.AddRange(new Control[] { lblTitle, lblUser, txtUser, lblPass, txtPass, lblRole, cmbRole, btnLogin });
        }
    }
}