using Constellation;
using Constellation.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Simu_State_Object.MySQL.MessageCallbacks;
using Simu_State_Object.ForecastIO.MessageCallbacks;


public class Donnee
{
    public string Name;
    public int Value;
    public string Unit;
    public Donnee(string name, int value, string unit)
    {
        this.Name = name;
        this.Value = value;
        this.Unit = unit;
    }
}
public class Capteur
{
    public Donnee Température;
    public Donnee Luminosité;
    public Donnee Humidité;
    public Donnee Niveau_eau;

    public Capteur() //Creation d'une classe avec toute les données des capteurs
    {
        this.Humidité = new Donnee("Humidité", 999, "%");
        this.Température = new Donnee("Température", 999, "°C");
        this.Niveau_eau = new Donnee("Niveau d'eau", 999, "L");
        this.Luminosité = new Donnee("Luminosité", 999, "Lux");
    }
}
public class Serre
{
    public Plante[] Plante;
    public int LastId;
    public String[] tab_url;
    public Database[] data;
    public Serre(int a,int b)
    {
        Plante = new Plante[a];
        int LastId = -1;
        tab_url = new String[PackageHost.GetSettingValue<Int32>("max_x") * PackageHost.GetSettingValue<Int32>("max_y")];  //Serre de 5*5, parametrable
        data = new Database[b];
    }
}
public class Plante
{
    public string nom;
    public string url;
    public string seuil_lum;
    public string temp;
    public string humidite;
    public string besoin_eau;
    public string maturation;
    public string position_x;
    public string position_y;
    public string effectif;
}

public class Database
{
    public string Id;
    public string nom;
    public string url;
    public string seuil_lum;
    public string temp;
    public string humidite;
    public string besoin_eau;
    public string maturation;
}

namespace WebTest
{
    public class Program : PackageBase
    {
        private Random rdn = new Random();
        private Capteur Rcapteur = new Capteur();

        [StateObjectLink("MyVirtualPackage", "Humidite")] //Nom + Clef
        public StateObjectNotifier hum { get; set; }
        [StateObjectLink("MyVirtualPackage", "/lux")] //Nom + Clef
        public StateObjectNotifier lum { get; set; }
        [StateObjectLink("MyVirtualPackage", "Temperature")] //Nom + Clef
        public StateObjectNotifier temp { get; set; }

        static void Main(string[] args)
        {
            PackageHost.Start<Program>(args);
        }

        public override void OnStart()
        {
            PackageHost.WriteInfo("Package starting - IsRunning: {0} - IsConnected: {1}", PackageHost.IsRunning, PackageHost.IsConnected);
            PackageHost.WriteInfo("Je suis le package nommé {0} version {1}", PackageHost.PackageName, PackageHost.PackageVersion);


            Task.Factory.StartNew(() =>
            {
                while (PackageHost.IsRunning)
                {
                    //Mettre les valeurs des capteurs dans la classe   
                    Rcapteur.Humidité.Value = this.hum.DynamicValue;
                    Rcapteur.Luminosité.Value = this.lum.DynamicValue;
                    Rcapteur.Niveau_eau.Value = rdn.Next(10, 30);
                    Rcapteur.Température.Value = this.temp.DynamicValue;

                    //STATE OBJECT CAPTEURS
                    PackageHost.PushStateObject("Résultats Capteurs", Rcapteur, lifetime: PackageHost.GetSettingValue<int>("Interval") + 4000);

                   
                    //STATE OBJECT
                    //Un state object expire si après 4 seconde de son intervale il n'est pas mis à jour. C'est un choix arbitraire
                    Thread.Sleep(PackageHost.GetSettingValue<int>("Interval"));
                    

                    //Ajouter du code qui surveille temperature/Humidite des plantes dans la serre et envoyer un push si c'est le cas
                }
            });

        }

        [MessageCallback]
        public async Task Maj_SerreAsync()
        {
            int i,a;
            String[][] response = await PackageHost.CreateMessageProxy("MySQL").Select_DB<System.String[][]>("plante,serre WHERE plante.Id = serre.Id" as System.String,
                "nom,url,seuil_lum,temp,besoin_eau,humidite,maturation,position_x,position_y,effectif" as System.String);
            a = await PackageHost.CreateMessageProxy("MySQL").Count_DB<System.Int32>("plante" as System.String);

            Serre so = new Serre(response[0].Length,a);

            init_tab(so.tab_url, PackageHost.GetSettingValue<String>("default_picture"));
            for (i = 0; i < response[0].Length; i++) //Contient les infos sur toutes les plantes planté dans la serre
            {
                so.Plante[i] = new Plante();
                so.Plante[i].nom = response[0][i];
                so.Plante[i].url = response[1][i];
                so.Plante[i].seuil_lum = response[2][i];
                so.Plante[i].temp = response[3][i];
                so.Plante[i].besoin_eau = response[4][i];
                so.Plante[i].humidite = response[5][i];
                so.Plante[i].maturation = response[6][i];
                so.Plante[i].position_x = response[7][i];
                so.Plante[i].position_y = response[8][i];
                so.Plante[i].effectif = response[9][i];
                
                so.tab_url[( int.Parse(so.Plante[i].position_x) - 1) + (int.Parse(so.Plante[i].position_y) - 1 ) * PackageHost.GetSettingValue<Int32>("max_y")] = so.Plante[i].url;
            }
            so.LastId = a + 1; //Nouvel Id utilisable pendantl'ajout d'une plante

            response = await PackageHost.CreateMessageProxy("MySQL").Select_DB<System.String[][]>("plante" as System.String,
                "Id,nom,url,seuil_lum,temp,besoin_eau,humidite,maturation" as System.String);
            for (i = 0; i < response[0].Length; i++) //Contient toute les infos de la table plante
            {
                so.data[i] = new Database();
                so.data[i].Id = response[0][i];
                so.data[i].nom = response[1][i];
                so.data[i].url = response[2][i];
                so.data[i].seuil_lum = response[3][i];
                so.data[i].temp = response[4][i];
                so.data[i].besoin_eau = response[5][i];
                so.data[i].humidite = response[6][i];
                so.data[i].maturation = response[7][i];
            }

            PackageHost.PushStateObject("Serre", so);
        }
        public void init_tab(String[] tab, string defaut_value)
        {
            int i;
            for (i = 0; i < tab.Length; i++)
            {
                tab[i] = defaut_value;
            }
        }
    }
}


