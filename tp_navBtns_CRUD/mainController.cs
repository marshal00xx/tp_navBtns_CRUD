using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Diagnostics;
using Guna.UI2.WinForms;
using Bunifu.Framework.UI;

namespace tp_navBtns_CRUD {
    public partial class mainController : Form {
        /*
         * Thos is a basic CRUD winformes with more features into it 
         * stil very slow 
         * looking forward to improve the speed and the UX 
         * Marshal 
         * **/
        // we build the connection string
        SqlConnectionStringBuilder connectionString = new SqlConnectionStringBuilder() {
            DataSource = ".",
            InitialCatalog = "School",
            IntegratedSecurity = true
        };

        // we declare the variables we gonna need
        String query = "SELECT stagaireId, nom, prenom, address, cin, age FROM stagaire";
        int tracker = 0;
        public mainController() {
            InitializeComponent();
        }


        private void mainController_Load(object sender, EventArgs e) {
            query = "SELECT stagaireId, nom, prenom, address, cin, age FROM stagaire";
            loadData(query);
        }

        private void navBtn_Click(object sender, EventArgs e) {
            Guna2CircleButton button = (Guna2CircleButton)sender;
            String buttonText = button.Text;
            int back = tracker;
            try {
                /**
                 * the next few lines are the nav btns logic 
                 * if << is 'pressed we simply go the 0
                 * else if >> is 'pressed we simply go to the rowCounter() - 1
                 * else we perform a simple algorithm to stay inside thw boundaries the we go either to 
                 *              tracker-- or tracker++
                 * **/
                switch (buttonText) {
                    case "<<":
                        tracker = 0;
                        navigateTo(tracker);
                        break;
                    case "<":
                        tracker = tracker == 0 ? 0 : tracker - 1;
                        navigateTo(tracker);                   
                        break;
                    case ">":
                        tracker = tracker == rowsCounter() - 1 ? rowsCounter() - 1 : tracker + 1;
                        navigateTo(tracker);                 
                        break;
                    case ">>":
                        tracker = rowsCounter() - 1;
                        navigateTo(tracker);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex){
                tracker = back ;
            }
        }

        private void searchByTB_KeyDown(object sender, KeyEventArgs e) {
            try {
                // if Enter is pressed it will create the new query and pass it to the data loader 
                if (e.KeyCode == Keys.Enter) {
                    String cin = searchByTB.Text;
                    query = "SELECT stagaireId, nom, prenom, address, cin, age FROM stagaire WHERE cin = " + cin;
                    loadData(query);
                }
            }
            catch(Exception ex) {
                Debug.WriteLine(ex.Message);    
            }
        }

        private void resetBtn_Click(object sender, EventArgs e) {
            // we load all the data to the dgv
            try {
                query = "SELECT stagaireId, nom, prenom, address, cin, age FROM stagaire";
                loadData(query);
            }
            catch(Exception ex) {
                Debug.WriteLine(ex.Message);   
            }
        }

        private void resetBtn_MouseLeave(object sender, EventArgs e) {
            resetBtn.ActiveImage = null;
            resetBtn.Image.Dispose();
        }

        // methods used in this app
        
        // method to load data to the dgv
        private void loadData(String query) {
            try {
                // we intialize the connection
                using (SqlConnection connection = new SqlConnection(connectionString.ToString())) {
                    // we intialize the adapter

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection)) {
                        // we intialize a dataset
                        using (DataSet ds = new DataSet()) {
                            //we fill the the dataset using the adapter 
                            adapter.Fill(ds);

                            // we assign the datasource of the dgv to the 0 table in the ds
                            dgv.DataSource = ds.Tables[0];
                        }
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }// end load method

        // method to get the number of rows in the table
        private int rowsCounter() {
            try {
                using (SqlConnection connection = new SqlConnection(connectionString.ToString())) {
                    using (SqlCommand command = new SqlCommand()) {
                        connection.Open();
                        query = "SELECT COUNT(*) FROM stagaire";
                        command.CommandText = query;
                        command.Connection = connection;
                        return (int)command.ExecuteScalar();
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
            return 0;
        }
        // method to fill in the TBs using data from dataset according to a given index
        private void navigateTo(int index) {
            try {
                // we instantiate the connection
                using (SqlConnection connection = new SqlConnection(connectionString.ToString())) {
                    // we instantiate a new command
                    using (SqlCommand command = new SqlCommand()) {
                        connection.Open();
                        // we build the query and it's parameters
                        query = "SELECT * FROM stagaire WHERE stagaireId = @stagaireId";
                        command.Parameters.AddWithValue("@stagaireId", index);
                        // we pass the query and the connection and then execute it
                        command.CommandText = query;
                        command.Connection = connection;
                        command.ExecuteNonQuery();

                        //we instantiate the adapter
                        using (SqlDataAdapter adapter = new SqlDataAdapter(command)) {
                            // we file a table using the adapter
                            DataTable table = new DataTable();
                            adapter.Fill(table);
                            // we display the data 
                            itemNumberLbl.Text = index.ToString() + "/" + (rowsCounter() - 1).ToString();
                            idTB.Text = table.Rows[0]["stagaireId"].ToString();
                            nomTB.Text = table.Rows[0]["nom"].ToString();
                            prenomTB.Text = table.Rows[0]["prenom"].ToString();
                            addressTB.Text = table.Rows[0]["address"].ToString();
                            cinTB.Text = table.Rows[0]["cin"].ToString();
                            ageTB.Text = table.Rows[0]["age"].ToString();

                        }
                    }
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }// end of the navigateTo() method

        // method to deal with the transactions <CRUD ops>
        private void transactions(String operation) {
            int selectedRow = (int)dgv.CurrentRow.Cells[0].Value;
            try {
                // we instantiate the connection
                using (SqlConnection connection = new SqlConnection(connectionString.ToString())) {
                    connection.Open();
                    if (operation == "Insert") {
                        // we instantiate a new adapter
                        query = "SELECT * FROM stagaire";
                        using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection)) {
                            // we instantiate a new command builder
                            using (SqlCommandBuilder builder = new SqlCommandBuilder(adapter)) {
                                // we instantiate a new insert command using the builder
                                using (SqlCommand command = builder.GetInsertCommand(true)) {
                                    command.Connection = connection;
                                    // we clear the parameters
                                    command.Parameters.Clear();
                                    // we add the parameters to the command then we execute it
                                    addParameters(command);

                                    command.ExecuteNonQuery();                                
                                }
                            }
                        }
                    }
                    else if(operation == "Update") {
                        query = "UPDATE stagaire SET nom = @nom, prenom = @prenom, address = @address, " +
                            "cin = @cin, age = @age WHERE stagaireId = " + idTB.Text;
                        using(SqlCommand command = new SqlCommand(query, connection)) {
                            // we clear the command's parameteres
                            command.Parameters.Clear();
                            // we add the new parameters
                            addParameters(command);
                            // we execute
                            command.ExecuteNonQuery();
                        }
                    }
                    else if(operation == "Delete") {
                        // we create the query 
                        query = "DELETE FROM stagaire WHERE stagaireId = " + selectedRow.ToString();
                        // we instantiate a new command 
                        using (SqlCommand command = new SqlCommand(query, connection)) {
                            // we verify if we are really gonna remove the row...
                            if(MessageBox.Show("You really wanna remove this student??", "Remove", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes) {
                                command.ExecuteNonQuery();
                            }

                        }
                    }
                }
            }
            catch(Exception ex) {
                Debug.WriteLine(ex.Message);
            }
            finally {
                // finally we reload the data in the db 
                query = "SELECT stagaireId, nom, prenom, address, cin, age FROM stagaire";
                loadData(query);
            }
        }
        // this mwthods adds the parameters to a SqlCommand using values from the text boxes
        private void addParameters(SqlCommand command) {
            //command.Parameters.AddWithValue("@stagaireId", Convert.ToInt64(idTB.Text));
            command.Parameters.AddWithValue("@nom", nomTB.Text);
            command.Parameters.AddWithValue("@prenom", prenomTB.Text);
            command.Parameters.AddWithValue("@address", addressTB.Text);
            command.Parameters.AddWithValue("@cin", Convert.ToInt64(cinTB.Text));
            command.Parameters.AddWithValue("@age", Convert.ToInt16(ageTB.Text));
        }

        private void CRUD_Btn_Click(object sender, EventArgs e) {
        /**
        * Here we get the text of the button and we switch case on that +_+
        * **/
            BunifuThinButton2 button = (BunifuThinButton2)sender;
            String buttonText = button.ButtonText;
            try {
                switch (buttonText) {
                    case "Insert":
                        // there somthing very weird with this part of code I think it's possessed +_+
                        transactions("Insert");
                        break;
                    case "Update":
                        // works fine but very fkn slow
                        transactions("Update");
                        break;
                    case "Delete":
                        // working fine
                        transactions("Delete");
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        private void exitIcon_Click(object sender, EventArgs e) {
            this.Dispose();
        }

        private void clearIcon_Click(object sender, EventArgs e) {

        }

        private void dgv_SelectionChanged(object sender, EventArgs e) {
            try {
                int index = 0;
                // we get id of the selected row 
                try {
                    index = (int)dgv.CurrentRow.Cells[0].Value;
                }
                catch { }
                
                // we navigate to it <display the data in the text boxes
                navigateTo(index);
                // we set the new value of the tracker
                tracker = index;
            }
            catch(Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }

        private void clearIcon_Click_1(object sender, EventArgs e) {
            // clears every TB
            idTB.Clear();
            addressTB.Clear();
            ageTB.Clear();
            cinTB.Clear();
            nomTB.Clear();
            prenomTB.Clear();
            // set the tracker back to initial state and disables the label 
            tracker = 0;
            itemNumberLbl.Visible = false;            
        }

        private void saveChangesIcon_Click(object sender, EventArgs e) {
            try {
                transactions("Update");
            }
            catch(Exception ex) {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}


