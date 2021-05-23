using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using System.Diagnostics;
using System.Net.Http.Headers;

namespace http
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        private static string secret = "B(>7(njbJeyM*gT#QVVJVn*s7t>K=m&c%,x5qTyCg-22#CVb";
        private bool isLogged = false;

        public MainWindow()
        {
            
            InitializeComponent();
            updateData();

            form.Visibility = Visibility.Collapsed;
            login.Visibility = Visibility.Collapsed;

            if (isLogged)
            {
                form.Visibility = Visibility.Visible;
            } else
            {
                login.Visibility = Visibility.Visible;
            }
        }

        void updateData()
        {

            var task = Task.Run(() => Auth());
            task.Wait();


            var json = JsonConvert.DeserializeObject<List<JsonMsg>>(task.Result);

            fillingData(json);
        }


        void fillingData(List<JsonMsg> data)
        {
            DataTable dt = new DataTable();

            DataColumn name = new DataColumn("Jméno");
            DataColumn surname = new DataColumn("Příjmení");
            DataColumn city = new DataColumn("Město");
            DataColumn phone = new DataColumn("Mobil");
            

            dt.Columns.Add(name);
            dt.Columns.Add(surname);
            dt.Columns.Add(city);
            dt.Columns.Add(phone);

            foreach (var item in data)
            {
                DataRow row = dt.NewRow();
                row[0] = item.name;
                row[1] = item.surname;
                row[2] = item.city;
                row[3] = item.phone;

                dt.Rows.Add(row);
            }

            myDataGrid.ItemsSource = dt.DefaultView;
        }

        public static async Task<string> Auth()
        {
            using (HttpClient http = new HttpClient())
            {

                

                var data = new List<KeyValuePair<string, string>>();
               // data.Add(new KeyValuePair<string, string>("secret", secret));


                // odeslani pozadavku na url pres post
                HttpResponseMessage resp = await http.PostAsync(new Uri("https://api.jakubstellner.cz/subdom/api/c/get.php"), new FormUrlEncodedContent(data));
                

                string retstr = await resp.Content.ReadAsStringAsync();

                return retstr;
            }
        }

        public static async Task<string> Insert(string name, string surname, string city, string phone)
        {
            using (HttpClient http = new HttpClient())
            {

                var data = new List<KeyValuePair<string, string>>();
                data.Add(new KeyValuePair<string, string>("name", name));
                data.Add(new KeyValuePair<string, string>("surname", surname));
                data.Add(new KeyValuePair<string, string>("city", city));
                data.Add(new KeyValuePair<string, string>("phone", phone));
                //data.Add(new KeyValuePair<string, string>("secret", secret));


                // odeslani pozadavku na url pres post
                HttpResponseMessage resp = await http.PostAsync(new Uri("https://api.jakubstellner.cz/subdom/api/c/insert.php"), new FormUrlEncodedContent(data));

                string retstr = await resp.Content.ReadAsStringAsync();

                return retstr;
            }
        }




        private void addUser(object sender, RoutedEventArgs e)
        {
            string name_db = name.Text;
            string surname_db = surname.Text;
            string city_db = city.Text;
            string phone_db = phone.Text;

            name.Text = "";
            surname.Text = "";
            city.Text = "";
            phone.Text = "";

            var task = Task.Run(() => Insert(name_db, surname_db, city_db, phone_db));
            task.Wait();

            updateData();

        }

        private void Auth(object sender, RoutedEventArgs e)
        {
            string user = email.Text;
            string pass = password.Text;

            // task pro asynchorni operace - pozadavek na http
            var task = Task.Run(() => Login(user, pass));
            task.Wait();

            // task.Result vraci navratovou hodnotu (return retsrt v Login) a v tomto pripade nam web vraci json
            var json = JsonConvert.DeserializeObject<JsonAuthMsg>(task.Result);

            Console.WriteLine(json.status);
            if (json.status)
            {
                login.Visibility = Visibility.Collapsed;
                form.Visibility = Visibility.Visible;
            }
   

        }

        public static async Task<string> Login(string user, string pass)
        {
            using (HttpClient http = new HttpClient())
            {
                // vytvoreni listu pro POST
                var data = new List<KeyValuePair<string, string>>();
                data.Add(new KeyValuePair<string, string>("user", user));
                data.Add(new KeyValuePair<string, string>("pass", pass));

                // odeslani pozadavku na url pres post
                HttpResponseMessage resp = await http.PostAsync(new Uri("https://api.jakubstellner.cz/subdom/api/c/index.php"), new FormUrlEncodedContent(data));
                string retstr = await resp.Content.ReadAsStringAsync();

                return retstr;

                // druha moznost deserializace, nemusime mit tridu pro zpravy
                /*dynamic json = JsonConvert.DeserializeObject(retstr);
                foreach (var item in json) {
                    .... item.status;
                    .... item.msg;
                }*/
            }
        }

        // trida pro zpravy, nas web vzdy vraci status a msg 
        public class JsonAuthMsg
        {
            public bool status { get; set; }
            public string msg { get; set; }
        }

        // trida pro zpravy, nas web vzdy vraci status a msg 
        public class JsonMsg
        {
            public int id { get; set; }
            public string name { get; set; }
            public string surname { get; set; }
            public string city { get; set; }
            public string phone { get; set; }

        }
    }
}
