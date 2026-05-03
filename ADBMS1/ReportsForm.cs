using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;           // For DataTable
using System.Data.SqlClient; // For SQL Server

namespace ADBMS1
{
    public partial class ReportsForm : Form
    {
        DataGridView dgvSalesHistory = new DataGridView();
        Label lblTotalRevenue = new Label();
        Label lblOrders = new Label();
        Label lblTopItem = new Label();

        public ReportsForm()
        {
            InitializeComponent();
            this.Text = "Business Analytics & Sales Reports";
            this.Size = new Size(850, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.AliceBlue;
        }

        private void ReportsForm_Load(object sender, EventArgs e)
        {
            // --- MAIN HEADER ---
            Label lblHeader = new Label() { Text = "EXECUTIVE SALES REPORT", Font = new Font("Arial", 18, FontStyle.Bold), Top = 20, Left = 20, Width = 400 };

            // --- SUMMARY DASHBOARD ---
            Panel pnlStats = new Panel() { Location = new Point(20, 70), Size = new Size(790, 100), BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };

            lblOrders.Font = new Font("Arial", 10, FontStyle.Bold);
            lblOrders.Location = new Point(50, 25);
            lblOrders.TextAlign = ContentAlignment.MiddleCenter;
            lblOrders.AutoSize = true;

            lblTotalRevenue.Font = new Font("Arial", 10, FontStyle.Bold);
            lblTotalRevenue.ForeColor = Color.DarkGreen;
            lblTotalRevenue.Location = new Point(320, 25);
            lblTotalRevenue.TextAlign = ContentAlignment.MiddleCenter;
            lblTotalRevenue.AutoSize = true;

            lblTopItem.Font = new Font("Arial", 10, FontStyle.Bold);
            lblTopItem.Location = new Point(580, 25);
            lblTopItem.TextAlign = ContentAlignment.MiddleCenter;
            lblTopItem.AutoSize = true;

            pnlStats.Controls.AddRange(new Control[] { lblOrders, lblTotalRevenue, lblTopItem });

            // --- DETAILED SALES TABLE ---
            Label lblTableTitle = new Label() { Text = "Transaction History:", Top = 190, Left = 20, Font = new Font("Arial", 10, FontStyle.Bold) };
            dgvSalesHistory.Location = new Point(20, 220);
            dgvSalesHistory.Size = new Size(790, 250);
            dgvSalesHistory.BackgroundColor = Color.White;
            dgvSalesHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // --- ACTION BUTTONS ---
            Button btnExport = new Button() { Text = "GENERATE PDF REPORT", Top = 490, Left = 20, Width = 250, Height = 45, BackColor = Color.DarkSlateGray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };

            Button btnRefresh = new Button() { Text = "Refresh Data", Top = 490, Left = 280, Width = 150, Height = 45 };
            btnRefresh.Click += (s, ev) => LoadReportData();

            Button btnBack = new Button() { Text = "Back", Top = 490, Left = 710, Width = 100, Height = 45 };
            btnBack.Click += (s, ev) => this.Close();

            this.Controls.AddRange(new Control[] { lblHeader, pnlStats, lblTableTitle, dgvSalesHistory, btnExport, btnRefresh, btnBack });

            // Initial Data Load
            LoadReportData();
        }

        private void LoadReportData()
        {
            using (SqlConnection conn = new SqlConnection(Connection.stringConn))
            {
                try
                {
                    conn.Open();

                    // 1. Get Today's Order Count and Total Revenue
                    string summaryQuery = "SELECT COUNT(*) as Count, SUM(TotalAmount) as Total FROM Orders WHERE CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)";
                    SqlCommand cmdSum = new SqlCommand(summaryQuery, conn);
                    SqlDataReader reader = cmdSum.ExecuteReader();
                    if (reader.Read())
                    {
                        lblOrders.Text = $"Today's Orders\n{reader["Count"]}";
                        lblTotalRevenue.Text = $"Total Revenue\n$ {Convert.ToDouble(reader["Total"] == DBNull.Value ? 0 : reader["Total"]):F2}";
                    }
                    reader.Close();

                    // 2. Get Top Selling Item Name using a JOIN
                    string topItemQuery = @"SELECT TOP 1 p.ItemName FROM OrderDetails od 
                                           JOIN Products p ON od.ProductID = p.ProductID 
                                           GROUP BY p.ItemName ORDER BY COUNT(od.ProductID) DESC";
                    SqlCommand cmdTop = new SqlCommand(topItemQuery, conn);
                    object topItem = cmdTop.ExecuteScalar();
                    lblTopItem.Text = $"Top Selling Item\n{(topItem ?? "N/A")}";

                    // 3. Populate Grid with Transaction History
                    SqlDataAdapter da = new SqlDataAdapter("SELECT OrderID, OrderDate, StaffName, TotalAmount FROM Orders ORDER BY OrderDate DESC", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgvSalesHistory.DataSource = dt;
                }
                catch (Exception ex) { MessageBox.Show("Report Error: " + ex.Message); }
            }
        }
    }
}