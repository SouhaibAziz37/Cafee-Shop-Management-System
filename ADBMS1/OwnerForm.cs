using System;
using System.Drawing;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;

namespace ADBMS1
{
    public partial class OwnerForm : Form
    {
        public OwnerForm()
        {
            InitializeComponent();
            this.Text = "Admin Dashboard - Cafe Management System";
            this.Size = new Size(850, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Load -= OwnerForm_Load;
            this.Load += new EventHandler(OwnerForm_Load);
        }

        private void OwnerForm_Load(object sender, EventArgs e)
        {
            this.Controls.Clear();
            TabControl ownerTabs = new TabControl() { Dock = DockStyle.Fill };
            ownerTabs.Font = new Font("Segoe UI", 10);

            // --- TAB 1: MENU MANAGEMENT ---
            TabPage tabMenu = new TabPage("Menu Management");
            Action openMenuEditor = () => {
                using (var f = new MenuForm())
                {
                    f.ShowDialog();
                    RefreshTabGrid(tabMenu, "Products");
                }
            };
            SetupGridTab(tabMenu, "Add Menu Item", "Update Price", "Remove Item", "Products",
                openMenuEditor, openMenuEditor, openMenuEditor);

            // --- TAB 2: STAFF MANAGEMENT ---
            TabPage tabStaff = new TabPage("Staff Management");
            SetupGridTab(tabStaff, "Hire Staff", "Edit Role", "Terminate", "Users",
                () => { new StaffManagementForm().Show(); },
                () => { new StaffManagementForm().Show(); },
                () => { new StaffManagementForm().Show(); });

            // --- TAB 3: INVENTORY MANAGEMENT ---
            TabPage tabStock = new TabPage("Inventory / Stock");
            SetupGridTab(tabStock, "Add Stock", "Update Qty", "Low Stock Alert", "Products",
                () => { new InventoryForm().Show(); },
                () => { new InventoryForm().Show(); },
                () => { new InventoryForm().Show(); });

            // --- TAB 4: SALES REPORTS ---
            TabPage tabReports = new TabPage("Sales Reports");
            DataGridView gridSales = new DataGridView() { Name = "gridSales", Size = new Size(800, 350), Location = new Point(15, 15), BackgroundColor = Color.WhiteSmoke, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            Label lblTotal = new Label() { Name = "lblTotal", Text = "Daily Total: $0.00", Font = new Font("Arial", 14, FontStyle.Bold), Location = new Point(15, 380), AutoSize = true };
            Button btnRefreshSales = new Button() { Text = "Refresh Sales", Location = new Point(550, 380), Size = new Size(120, 40), BackColor = Color.LightGray };
            btnRefreshSales.Click += (s, ev) => LoadSalesData(gridSales, lblTotal);
            tabReports.Controls.AddRange(new Control[] { gridSales, lblTotal, btnRefreshSales });

            ownerTabs.TabPages.AddRange(new TabPage[] { tabMenu, tabStaff, tabStock, tabReports });

            Button btnLogout = new Button() { Text = "Logout", Dock = DockStyle.Bottom, Height = 40, BackColor = Color.Firebrick, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
            btnLogout.Click += (s, ev) => { new Form1().Show(); this.Close(); };

            this.Controls.Add(ownerTabs);
            this.Controls.Add(btnLogout);
            LoadSalesData(gridSales, lblTotal);
        }

        private void SetupGridTab(TabPage page, string btn1Text, string btn2Text, string btn3Text, string tableName,
                                  Action onBtn1Click, Action onBtn2Click, Action onBtn3Click)
        {
            page.Controls.Clear();
            DataGridView dgv = new DataGridView() { Size = new Size(800, 300), Location = new Point(15, 15), BackgroundColor = Color.White, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };

            Button b1 = new Button() { Text = btn1Text, Location = new Point(15, 330), Size = new Size(130, 40), BackColor = Color.LightSteelBlue, FlatStyle = FlatStyle.Flat };
            Button b2 = new Button() { Text = btn2Text, Location = new Point(160, 330), Size = new Size(130, 40), BackColor = Color.LightSteelBlue, FlatStyle = FlatStyle.Flat };
            Button b3 = new Button() { Text = btn3Text, Location = new Point(305, 330), Size = new Size(130, 40), ForeColor = Color.DarkRed, FlatStyle = FlatStyle.Flat };

            b1.Click += (s, ev) => onBtn1Click();
            b2.Click += (s, ev) => onBtn2Click();
            b3.Click += (s, ev) => onBtn3Click();

            page.Controls.AddRange(new Control[] { dgv, b1, b2, b3 });
            RefreshTabGrid(page, tableName);
        }

        private void RefreshTabGrid(TabPage page, string tableName)
        {
            foreach (Control c in page.Controls)
            {
                if (c is DataGridView dgv)
                {
                    using (SqlConnection conn = new SqlConnection(Connection.stringConn))
                    {
                        try
                        {
                            SqlDataAdapter da = new SqlDataAdapter($"SELECT * FROM {tableName}", conn);
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            dgv.DataSource = dt;
                        }
                        catch { }
                    }
                }
            }
        }

        private void LoadSalesData(DataGridView dgv, Label lbl)
        {
            using (SqlConnection conn = new SqlConnection(Connection.stringConn))
            {
                try
                {
                    SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM Orders WHERE CAST(OrderDate AS DATE) = CAST(GETDATE() AS DATE)", conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dgv.DataSource = dt;
                    decimal total = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        total += Convert.ToDecimal(row["TotalAmount"] == DBNull.Value ? 0 : row["TotalAmount"]);
                    }
                    lbl.Text = $"Daily Total: ${total:F2}";
                }
                catch { }
            }
        }
    }
}