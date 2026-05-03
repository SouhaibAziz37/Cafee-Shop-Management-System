using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;           // For DataTable
using System.Data.SqlClient; // For SQL Server

namespace ADBMS1
{
    public partial class InventoryForm : Form
    {
        DataGridView dgvInventory = new DataGridView();
        TextBox txtItemName = new TextBox();
        NumericUpDown numQuantity = new NumericUpDown();
        Label lblAlert = new Label(); // Made global to update dynamically

        public InventoryForm()
        {
            InitializeComponent();
            this.Text = "Inventory Management - Stock Control";
            this.Size = new Size(800, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.OldLace;
        }

        private void InventoryForm_Load(object sender, EventArgs e)
        {
            Label lblHeader = new Label() { Text = "STOCK & INVENTORY", Font = new Font("Arial", 16, FontStyle.Bold), Top = 20, Left = 20, Width = 300 };

            // --- UPDATE SECTION ---
            Label lblItem = new Label() { Text = "Product ID to Update:", Top = 80, Left = 20, AutoSize = true };
            txtItemName.Location = new Point(20, 105); txtItemName.Width = 200;

            Label lblQty = new Label() { Text = "New Stock Quantity:", Top = 145, Left = 20, AutoSize = true };
            numQuantity.Location = new Point(20, 170); numQuantity.Width = 200;
            numQuantity.Maximum = 5000;

            Button btnUpdateStock = new Button()
            {
                Text = "SET STOCK LEVEL",
                Top = 220,
                Left = 20,
                Width = 200,
                Height = 45,
                BackColor = Color.DarkGoldenrod,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };

            // --- DATA DISPLAY ---
            dgvInventory.Location = new Point(250, 80);
            dgvInventory.Size = new Size(500, 280);
            dgvInventory.BackgroundColor = Color.White;
            dgvInventory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            Panel pnlAlert = new Panel() { Location = new Point(250, 370), Size = new Size(500, 50), BackColor = Color.MistyRose, BorderStyle = BorderStyle.FixedSingle };
            lblAlert.ForeColor = Color.DarkRed;
            lblAlert.Font = new Font("Arial", 9, FontStyle.Bold);
            lblAlert.Dock = DockStyle.Fill;
            lblAlert.TextAlign = ContentAlignment.MiddleCenter;
            pnlAlert.Controls.Add(lblAlert);

            Button btnBack = new Button() { Text = "Exit Inventory", Top = 400, Left = 20, Width = 120 };
            btnBack.Click += (s, ev) => this.Close();

            this.Controls.AddRange(new Control[] { lblHeader, lblItem, txtItemName, lblQty, numQuantity, btnUpdateStock, dgvInventory, pnlAlert, btnBack });

            // Initial Data Load
            LoadInventoryData();

            // --- DATABASE LOGIC: UPDATE STOCK ---
            btnUpdateStock.Click += (s, ev) => {
                if (string.IsNullOrWhiteSpace(txtItemName.Text))
                {
                    MessageBox.Show("Please enter the Product ID.");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(Connection.stringConn))
                {
                    try
                    {
                        string query = "UPDATE Products SET StockQuantity = @qty WHERE ProductID = @id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@qty", (int)numQuantity.Value);
                        cmd.Parameters.AddWithValue("@id", txtItemName.Text);

                        conn.Open();
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            MessageBox.Show("Stock updated successfully.");
                            LoadInventoryData();
                        }
                        else { MessageBox.Show("Product ID not found."); }
                    }
                    catch (Exception ex) { MessageBox.Show("Update Error: " + ex.Message); }
                }
            };
        }

        private void LoadInventoryData()
        {
            using (SqlConnection conn = new SqlConnection(Connection.stringConn))
            {
                try
                {
                    // Fetch all products
                    SqlDataAdapter da = new SqlDataAdapter("SELECT ProductID, ItemName, Category, StockQuantity FROM Products", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvInventory.DataSource = dt;

                    // Logic for the Low Stock Alert
                    string lowStockItems = "";
                    foreach (DataRow row in dt.Rows)
                    {
                        if (Convert.ToInt32(row["StockQuantity"]) < 10)
                        {
                            lowStockItems += row["ItemName"].ToString() + ", ";
                        }
                    }

                    if (lowStockItems != "")
                    {
                        lblAlert.Text = "⚠️ LOW STOCK ALERT: " + lowStockItems.TrimEnd(',', ' ');
                    }
                    else
                    {
                        lblAlert.Text = "✅ All stock levels are healthy.";
                        lblAlert.ForeColor = Color.DarkGreen;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Load Error: " + ex.Message); }
            }
        }
    }
}