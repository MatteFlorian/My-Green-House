using Constellation;
using Constellation.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;


public class DBConnect
{
    /// <summary>
    /// The connection
    /// </summary>
    private MySqlConnection connection;
    /// <summary>
    /// MySQL server adress
    /// </summary>
    public string server;
    /// <summary>
    /// name of the database on the server
    /// </summary>
    public string database;
    /// <summary>
    /// User Identifiant
    /// </summary>
    public string uid;
    /// <summary>
    /// Password
    /// </summary>
    public string psw;
    /// <summary>
    /// Inform on the connection status
    /// </summary>
    public string status;

    /// <summary>
    /// Initializes a new instance of the <see cref="DBConnect"/> class.
    /// </summary>
    public DBConnect(string server_name, string database_name, string username, string password)
    {
        Initialize(server_name, database_name, username, password);
    }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    public void Initialize(string server_name, string database_name, string username, string password)
    {
        server = server_name;
        database = database_name;
        uid = username; 
        psw = password;
        string connectionString;
        connectionString = "SERVER=" + server + ";" + "DATABASE=" +
        database + ";" + "UID=" + uid + ";" + "PASSWORD=" + psw + ";"; //Create connection query
        status = "disconnected"; //Used to know if a connection is open

        connection = new MySqlConnection(connectionString); //Initialize connection
    }

    /// <summary>
    /// Opens the connection.
    /// </summary>
    public void OpenConnection()
    {
        try
        {
            connection.Open();
            PackageHost.WriteInfo("MySQL server connected");
            this.status = "connected"; //Set status to connected
        }
        catch (MySqlException ex) //Show error number + most common error advice
        {
            PackageHost.WriteInfo(ex.Number);
            this.status = "disconnected";
            switch (ex.Number)
            {
                case 0:
                    PackageHost.WriteError("Cannot connect to server.  Contact administrator");
                    break;

                case 1045:
                    PackageHost.WriteError("Invalid username/password, please try again");
                    break;

                    PackageHost.WriteError($"Connection error = {ex.Number}");
                    PackageHost.WriteError($"Connection error = {ex.Message}");

            }
        }
    }

    /// <summary>
    /// Closes the connection.
    /// </summary>
    public void CloseConnection()
    {
        try
        {
            connection.Close();
            PackageHost.WriteInfo("MySQL server disconnected");
            this.status = "disconnected"; //Set Status to disconnected
        }
        catch (MySqlException ex)
        {
            PackageHost.WriteError($"Connection error = {ex.Number}");
            PackageHost.WriteError(ex.Message);
        }
    }

    /// <summary>
    /// Inserts a statement into a table.
    /// </summary>
    public void Insert(string table, string values)
    {
        string query = "INSERT INTO " + table + " VALUES " + values;
        //open connection
        this.OpenConnection();
        try
        {
            if (this.status == "connected")
            {
                //create command and assign the query and connection from the constructor
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //Execute command
                cmd.ExecuteNonQuery();
                PackageHost.WriteInfo($"Query succesfull ", query);
                PackageHost.WriteInfo(query);

                //close connection
                this.CloseConnection();
            }
        }
        catch (MySqlException ex)
        {
            this.CloseConnection();
            PackageHost.WriteError($"Error number {ex.Number}");
            PackageHost.WriteError(ex.Message);
            switch (ex.Number)
            {
                case 1146:
                    PackageHost.WriteError("Error, check table format and compatibility");
                    break;

                case 1062:
                    PackageHost.WriteError("Error check values format and compatibility");
                    break;
                               
            }
            this.status = "disconnected";
        }
    } //table = "tableinfo (name, age)"  values = "('John Smith', '33')"

    /// <summary>
    /// Updates a statement in a table
    /// </summary>
    public void Update(string table, string values, string condition) //table="nomtable (id1,id2,id3)" values="name='Joe', age='22'"  condition="name='John Smith'"
    {
        string query = "UPDATE " + table + " SET " + values + " WHERE " + condition;

        //open connection
        this.OpenConnection();
        try
        {
            if (this.status == "connected")
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;
                PackageHost.WriteInfo($"Query succesfull ", query);
                PackageHost.WriteInfo(query);
                //Execute query
                cmd.ExecuteNonQuery();

                //close connection
                this.CloseConnection();
            }
        }
        catch (MySqlException ex)
        {
            PackageHost.WriteError($"Error number {ex.Number}");
            PackageHost.WriteError(ex.Message);
        }
    }

    /// <summary>
    /// To enter an SQL request
    /// </summary>
    public void Request(string query)
    {
        //open connection
        this.OpenConnection();
        try
        {
            if (this.status == "connected")
            {
                //create mysql command
                MySqlCommand cmd = new MySqlCommand();
                //Assign the query using CommandText
                cmd.CommandText = query;
                //Assign the connection using Connection
                cmd.Connection = connection;

                //Execute query
                cmd.ExecuteNonQuery();
                PackageHost.WriteInfo($"Query succesfull ", query);
                PackageHost.WriteInfo(query);
                //close connection
                this.CloseConnection();
            }
        }
        catch (MySqlException ex)
        {
            PackageHost.WriteError($"Error number {ex.Number}");
            PackageHost.WriteError(ex.Message);
        }
    }

    /// <summary>
    /// Deletes the specified table.
    /// </summary>
    public void Delete(string table, string condition)
    {
        //Create query
        string query = "DELETE FROM " + table + " WHERE " + condition;
        this.OpenConnection();
        try
        {
            if (this.status == "connected")
            {
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.ExecuteNonQuery();
                PackageHost.WriteInfo($"Query succesfull ", query);
                PackageHost.WriteInfo(query);
                this.CloseConnection();
            }
        }
        catch (MySqlException ex)
        {
            this.CloseConnection();
            PackageHost.WriteError($"Error number {ex.Number}");
            PackageHost.WriteError(ex.Message);
        }
    }

    /// <summary>
    /// Counts the specified table.
    /// </summary>
    public int Count(string table)
    {
        string query = "SELECT Count(*) FROM " + table;
        int Count = -1;

        this.OpenConnection();
        try
        {
            if (this.status == "connected")
            {
                //Create Mysql Command
                MySqlCommand cmd = new MySqlCommand(query, connection);

                //ExecuteScalar will return one value (count query)
                Count = int.Parse(cmd.ExecuteScalar() + "");
                PackageHost.WriteInfo("Query succesfull ");
                PackageHost.WriteInfo(query);
                //close Connection
                this.CloseConnection();
                return Count;
            }
            else
            {
                this.CloseConnection();
                return Count;
            }
        }
        catch (MySqlException ex)
        {
            this.CloseConnection();
            PackageHost.WriteError($"Error number {ex.Number}");
            PackageHost.WriteError(ex.Message);
            return -1;

        }
    }

    /// <summary>
    /// Transform a string input into an array of string, used in Select method
    /// </summary>
    /// <param name="s">string wich will be cnovert into a sting array</param>
    /// <returns></returns>
    public string[] StringToTab(string s)
    {
        int i, c;
        c = 0;
        for (i = 0; i < s.Length; i++)
        {
            if (s[i] == ',')
            {
                c++;
            }
        }
        string[] tab = new string[c + 1];
        c = 0;
        for (i = 0; i < s.Length; i++)
        {
            if (s[i] == ',')
            {
                c++;
            }
            else
            {
                tab[c] += s[i];
            }
        }
        return tab;
    }

    /// <summary>
    /// Selects and return a table of your choice. * symbol is not supported
    /// </summary>
    public string[][] Select(string table, string selection)
    {
        if (selection == "*")
            //Can't use * in query from constellation
        {
            PackageHost.WriteError("* not supported");
            string[][] taberror = new string[0][];
            return taberror;
        }

        string[] tab = StringToTab(selection); //
        string[][] tabfinal = new string[tab.Length][];
        int i, j;
        int count = this.Count(table);

        List<string>[] list = new List<string>[tab.Length];
        for (i = 0; i < tab.Length; i++)
        {
            list[i] = new List<string>();
            tabfinal[i] = new string[count];
        }

        string query = "SELECT " + selection + " FROM " + table;

        this.OpenConnection();
        try
        {
            if (this.status == "connected")
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();

                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    for (i = 0; i < tab.Length; i++)
                    {
                        list[i].Add(dataReader[tab[i]] + "");
                    }

                }

                //close Data Reader
                dataReader.Close();
                for (i = 0; i < tab.Length; i++)
                {
                    for (j = 0; j < count; j++)
                    {                
                        tabfinal[i][j] = list[i][j];
                    }
                }
                PackageHost.WriteInfo($"Query succesfull ");
                PackageHost.WriteInfo(query);
                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return tabfinal;
            }
            else
            {
                this.CloseConnection();
                return tabfinal;
            }
        }
        catch (MySqlException ex)
        {
            this.CloseConnection();
            PackageHost.WriteError($"Error number {ex.Number}");
            PackageHost.WriteError(ex.Message);
            return tabfinal;
        }
    }

}

namespace MySQL
{

    /// <seealso cref="Constellation.Package.PackageBase" />
    public class Program : PackageBase
    {
        /// <summary>
        /// database associated to the sentinel
        /// </summary>
        private DBConnect database = new DBConnect("", "", "", "");

        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            PackageHost.Start<Program>(args);
        }

        /// <summary>
        /// Called when the package is started.
        /// </summary>
        public override void OnStart()
        {
            //Initialize class database
            database.Initialize(PackageHost.GetSettingValue("server_name"), PackageHost.GetSettingValue("database_name"), PackageHost.GetSettingValue("username"), PackageHost.GetSettingValue("password"));

            database.OpenConnection(); //Test if the connection is working
            database.CloseConnection();

        }

        /// <summary>
        /// Inserts a statement in a selected table. You have to mention the primary key of the table and the other parameters you want to add.
        /// </summary>
        /// <param name="table_to_insert">Name of the table ex: plante (no arguments necessary here).To avoid adding all entry of the table you can call plante(Id,name) for exemple</param>
        /// <param name="values_to_insert">Values to add ex : (1,'Salad')</param>
        [MessageCallback]
        public void Insert_DB(string table_to_insert, string values_to_insert)
        {
            database.Insert(table_to_insert, values_to_insert);
        }
        /// <summary>
        /// Updates a statement in a selected line of a table
        /// </summary>
        /// <param name="table_noarg">Name of the table ex: nomtable (no arguments necessary here).</param>
        /// <param name="values_change">Values to change separated by a coma ex : name='Joe', age='22'</param>
        /// <param name="condition">Condition to select the statement ex : name='John Smith'</param>
        [MessageCallback]
        public void Update_DB(string table_noarg, string values_change, string condition)
        {
            database.Update(table_noarg, values_change, condition);
        }

        /// <summary>
        /// Operate a script directly in MySQL server
        /// </summary>
        /// <param name="query">SQL request wich don't expect a return</param>
        [MessageCallback]
        public void Request_DB(string query)
        {
            database.Request(query);
        }

        /// <summary>
        /// Deletes the specified table.
        /// </summary>il
        /// <param name="table_noarg">Name of the table ex: nomtable (no arguments necessary here).</param>
        /// <param name="condition_del">Condition to select the ligne wich wl be deleted ex : name='John Smith'</param>
        [MessageCallback]
        public void Delete_DB(string table_noarg, string condition_del)
        {
            database.Delete(table_noarg, condition_del);
        }

        /// <summary>
        /// Counts statement of selected table
        /// </summary>
        /// <param name="table_noarg">Name of the table ex: nomtable (no arguments necessary here).</param>
        /// <returns></returns>
        [MessageCallback]
        public int Count_DB(string table_noarg)
        {
            return database.Count(table_noarg);
        }

        /// <summary>
        /// Selects and return different collumns of your choice in a double array
        /// </summary>
        /// <param name="table_noarg">Name of the table ex: nomtable (no arguments necessary here).</param>
        /// <param name="selection">column needed in a table sepparated by a coma ex : "id1,id2" </param>
        /// <returns></returns>
        [MessageCallback]
        public string[][] Select_DB(string table_noarg, string selection)
        {
            return database.Select(table_noarg, selection);
        }

    }
}

