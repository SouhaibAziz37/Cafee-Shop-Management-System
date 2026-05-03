using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Data.SqlClient; // Added for SQL
using System.Data;           // Added for DataTable

namespace ADBMS1
{
    public partial class CustomerForm : Form
    {
        ListBox lstCart = new ListBox();
        Label lblTotal = new Label();
        double grandTotal = 0;

        // List to track ProductIDs for the order details
        List<int> cartProductIDs = new List<int>();

        public CustomerForm()
        {
            InitializeComponent();
            this.Text = "Cafe Sales & Billing System";
            this.Size = new Size(850, 550);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;
        }

        private void CustomerForm_Load(object sender, EventArgs e)
        {
            Label lblMenu = new Label() { Text = "LIVE MENU", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(20, 10), Width = 200 };

            FlowLayoutPanel menuPanel = new FlowLayoutPanel();
            menuPanel.Location = new Point(20, 40);
            menuPanel.Size = new Size(450, 400);
            menuPanel.AutoScroll = true;

            // --- SQL DATA LOADING: Replace Dictionary with Database Query ---
            using (SqlConnection conn = new SqlConnection(Connection.stringConn))
            {
                try
                {
                    string query = "SELECT ProductID, ItemName, Price FROM Products WHERE StockQuantity > 0";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        int id = (int)reader["ProductID"];
                        string name = reader["ItemName"].ToString();
                        double price = Convert.ToDouble(reader["Price"]);

                        Button btnItem = new Button();
                        btnItem.Text = $"{name}\n${price:F2}";
                        btnItem.Size = new Size(130, 80);
                        btnItem.BackColor = Color.Tan;
                        btnItem.FlatStyle = FlatStyle.Flat;
                        btnItem.Font = new Font("Segoe UI", 9, FontStyle.Bold);

                        btnItem.Click += (s, ev) => {
                            lstCart.Items.Add($"{name} - ${price:F2}");
                            cartProductIDs.Add(id); // Store ID for the transaction
                            grandTotal += price;
                            lblTotal.Text = $"Grand Total: $ {grandTotal:F2}";
                        };

                        menuPanel.Controls.Add(btnItem);
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error loading menu: " + ex.Message); }
            }

            // --- UI Setup (Same as before) ---
            Label lblCart = new Label() { Text = "CURRENT BILL", Font = new Font("Segoe UI", 12, FontStyle.Bold), Location = new Point(500, 10), Width = 200 };
            lstCart.Location = new Point(500, 40);
            lstCart.Size = new Size(300, 280);
            lstCart.Font = new Font("Consolas", 10);

            lblTotal.Text = "Grand Total: $ 0.00";
            lblTotal.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblTotal.ForeColor = Color.DarkOliveGreen;
            lblTotal.Location = new Point(500, 330);
            lblTotal.AutoSize = true;

            // --- DATABASE INTEGRATION: Generate Invoice ---
            Button btnConfirm = new Button();
            btnConfirm.Text = "GENERATE INVOICE";
            btnConfirm.Location = new Point(500, 380);
            btnConfirm.Size = new Size(300, 45);
            btnConfirm.BackColor = Color.ForestGreen;
            btnConfirm.ForeColor = Color.White;
            btnConfirm.FlatStyle = FlatStyle.Flat;
            btnConfirm.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            btnConfirm.Click += (s, ev) => {
                if (lstCart.Items.Count > 0)
                {
                    using (SqlConnection conn = new SqlConnection(Connection.stringConn))
                    {
                        try
                        {
                            conn.Open();
                            // 1. Call Stored Procedure to create Order
                            SqlCommand cmd = new SqlCommand("sp_PlaceOrder", conn);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@StaffName", "Counter_1");
                            cmd.Parameters.AddWithValue("@Total", grandTotal);

                            int newOrderID = Convert.ToInt32(cmd.ExecuteScalar());

                            // 2. Insert Order Details (This triggers the stock update in SQL!)
                            foreach (int pID in cartProductIDs)
                            {
                                string detailQuery = "INSERT INTO OrderDetails (OrderID, ProductID, Quantity, SubTotal) VALUES (@oid, @pid, 1, (SELECT Price FROM Products WHERE ProductID=@pid))";
                                SqlCommand detailCmd = new SqlCommand(detailQuery, conn);
                                detailCmd.Parameters.AddWithValue("@oid", newOrderID);
                                detailCmd.Parameters.AddWithValue("@pid", pID);
                                detailCmd.ExecuteNonQuery();
                            }

                            MessageBox.Show($"Order Placed Successfully!\nInvoice #{newOrderID} Generated.", "Success");

                            // Reset UI
                            lstCart.Items.Clear();
                            cartProductIDs.Clear();
                            grandTotal = 0;
                            lblTotal.Text = "Grand Total: $ 0.00";
                        }
                        catch (Exception ex) { MessageBox.Show("Transaction Failed: " + ex.Message); }
                    }
                }
            };

            Button btnClear = new Button() { Text = "CLEAR ALL", Location = new Point(500, 435), Size = new Size(300, 30), BackColor = Color.LightGray, FlatStyle = FlatStyle.Flat };
            btnClear.Click += (s, ev) => { lstCart.Items.Clear(); cartProductIDs.Clear(); grandTotal = 0; lblTotal.Text = "Grand Total: $ 0.00"; };

            this.Controls.AddRange(new Control[] { lblMenu, menuPanel, lblCart, lstCart, lblTotal, btnConfirm, btnClear });
        }
    }
}