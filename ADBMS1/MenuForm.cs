using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ADBMS1
{
    public partial class MenuForm : Form
    {
        private SqlDataAdapter da;
        private DataTable dt;
        private DataGridView dgvMenu;

        public MenuForm()
        {
            InitializeComponent();
            SetupManualGrid();
            this.Load += (s, e) => LoadMenuData();
        }

        private void SetupManualGrid()
        {
            dgvMenu = new DataGridView();
            dgvMenu.Name = "dgvMenu";
            dgvMenu.Dock = DockStyle.Top;
            dgvMenu.Height = 300; // Adjusted height to fit more buttons
            dgvMenu.BackgroundColor = System.Drawing.Color.White;
            dgvMenu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMenu.SelectionMode = DataGridViewSelectionMode.FullRowSelect; // Makes it easier to select items to remove
            dgvMenu.ReadOnly = false;

            // Panel to hold buttons side-by-side
            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Fill;

            Button btnSave = new Button()
            {
                Text = "Save All Changes",
                Size = new System.Drawing.Size(150, 50),
                Location = new System.Drawing.Point(10, 10),
                BackColor = System.Drawing.Color.LightGreen
            };
            btnSave.Click += (s, e) => SaveData();

            Button btnRemove = new Button()
            {
                Text = "Remove Selected Item",
                Size = new System.Drawing.Size(150, 50),
                Location = new System.Drawing.Point(170, 10),
                BackColor = System.Drawing.Color.LightCoral
            };
            btnRemove.Click += (s, e) => RemoveItem();

            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnRemove);

            this.Controls.Add(buttonPanel);
            this.Controls.Add(dgvMenu);
        }

        private void LoadMenuData()
        {
            SqlConnection conn = new SqlConnection(Connection.stringConn);
            try
            {
                da = new SqlDataAdapter("SELECT * FROM Products", conn);
                dt = new DataTable();
                da.Fill(dt);
                dgvMenu.DataSource = dt;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        private void SaveData()
        {
            try
            {
                SqlCommandBuilder cb = new SqlCommandBuilder(da);
                da.Update(dt);
                MessageBox.Show("Database updated successfully!");
            }
            catch (Exception ex) { MessageBox.Show("Save failed: " + ex.Message); }
        }

        private void RemoveItem()
        {
            if (dgvMenu.SelectedRows.Count > 0)
            {
                var result = MessageBox.Show("Delete selected item permanently?", "Confirm Delete", MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    // Deletes the row from the local DataTable
                    foreach (DataGridViewRow row in dgvMenu.SelectedRows)
                    {
                        dgvMenu.Rows.RemoveAt(row.Index);
                    }

                    // Sync with database
                    SaveData();
                }
            }
            else
            {
                MessageBox.Show("Please select a full row to remove.");
            }
        }
    }
}