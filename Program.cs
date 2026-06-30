using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

public interface IPersonne
{
    int Id { get; set; }
    string Nom { get; set; }
    string Prenom {  get; set; }
    DateTime DateNaissance {  get; set; }
}
public interface IPayable
{
    decimal CalculSalaire();
    string GenererFichePaie();

}
public  abstract class Personne : IPersonne
{
    public int Id {  get; set; }
    public string Nom {  set; get; }
    public string Prenom { set; get; }
   
    public DateTime DateNaissance { get; set; }
}
public class Etudiant : Personne
{
    public string Classe { get; set; }
    public List<Matiere> MatieresInscrits { get; set; }
    public Dictionary<Matiere, double> Notes { get; set; }
    public Etudiant()
    {
        MatieresInscrits = new List<Matiere>();
        Notes = new Dictionary<Matiere, double>();
    }

    public decimal CalculMoyenneGenerale()
    {
        decimal SommePondere =(decimal) Notes.Sum(p => p.Value * p.Key.Coefficient);
        decimal CoefficientPondere = Notes.Sum(p=>p.Key.Coefficient);
        return SommePondere / CoefficientPondere;


    }
}
public class Professeur : Personne, IPayable
{
    public string MatierEnseignee {  get; set; }
    public decimal SalaireBase { get; set; }
    public int HeuresSupplementaires { get; set; }
    
    public decimal CalculSalaire()
    {
        decimal Taux = 50;
        return SalaireBase + (HeuresSupplementaires* Taux);
    }
    public string GenererFichePaie()
    {
        return $"{Nom}->{Prenom} -> {CalculSalaire()}";
    }
}

public class Administration : Personne, IPayable
{
    public string Departement {  get; set; }
    public decimal SalaireFixe {  get; set; }
    public decimal PrimeAnnuelle {  get; set; }
    public decimal CalculSalaire()
    {
        return SalaireFixe + (PrimeAnnuelle/12);
    }
    public string GenererFichePaie()
    {
        return $"{Nom}->{Prenom}-> {CalculSalaire()}";
    }
}
public class Matiere
{
    public int Id {  get; set; }
    public string Nom {  get; set; }
    public int Coefficient {  get; set; }
    public int PlacesDisponibles {  get; set; }
}
public class Cours
{
    public Matiere Matiere;
    public Professeur Professeur;
    public List<Etudiant> Inscrits;
    public DateTime Date;
    public string Salle { get; set; }
    public Cours()
    {
        Inscrits = new List<Etudiant>();
    }
    public bool InscrireEtudiant(Etudiant e)
    {
        if (Inscrits.Count > Matiere.PlacesDisponibles) return false;
        else if(Inscrits.Contains(e)) return false; 
        Inscrits.Add(e);
        e.MatieresInscrits.Add(Matiere);
        return true;

    }
}
    public class Repository<T> where T : IPersonne
    {
        public Dictionary<int, T> dic = new Dictionary<int, T>();
        private int ProchaineId = 1;
        public void AjouterEntite(T Entite)
        {
            Entite.Id = ProchaineId++;
            dic.Add(Entite.Id, Entite);
        }
        public T GetId(int id)
        {
            if (!dic.ContainsKey(id)) throw new KeyNotFoundException($"{id} non trouve");
            return dic[id];
        }

       /* public void SupprimerEntite(int id)
        {
            if (!dic.ContainsKey(id)) throw new KeyNotFoundException($"{id} non trouve");
            dic.Remove(id);
        }*/
        public List<T> ListerTous()
        {
            return new List<T>(dic.Values);
        }
    }
public class GestionnaireEcole
{
    public Repository<Etudiant> Etudiants;
    public Repository<Professeur> Professeurs;
    public Repository<Administration> Administrations;
    public List<Matiere> Matieres;
    public List<Cours> CoursProgrammes;
    public Stack<string> HistoriqueActions;
    public GestionnaireEcole()
    {
        Etudiants = new Repository<Etudiant>();
        Professeurs = new Repository<Professeur>();
        Administrations = new Repository<Administration>();
        Matieres = new List<Matiere>();
        CoursProgrammes = new List<Cours>();
        HistoriqueActions = new Stack<string>();
    }

    public void AjouterEtudiant(Etudiant Etudiant)
    {
        Etudiants.AjouterEntite(Etudiant);
        HistoriqueActions.Push($"{Etudiant.Nom} ajoutes");
    }
    public void AjouterProfesseur(string nom, string prenom, string matiere, decimal salaire)
    {
        var professeur = new Professeur();
        professeur.Nom = nom;
        professeur.Prenom = prenom;
        professeur.MatierEnseignee = matiere;
        professeur.SalaireBase = salaire;
        Professeurs.AjouterEntite(professeur);
        HistoriqueActions.Push($"{DateTime.Now} le professeur {professeur.Nom} a ete ajouter");


    }
    public void AjouterAdministratif(string nom, string prenom, string departement, decimal salaire)
    {
        var adm = new Administration();
        adm.Nom = nom;
        adm.Prenom = prenom;
        adm.Departement = departement;
        adm.SalaireFixe = salaire;
        Administrations.AjouterEntite(adm);
        HistoriqueActions.Push($"{DateTime.Now} l'administrateur {adm.Nom} a ete ajoute ");

    }
    public Personne RechercherPersonne(int id)
    {
        try { return Etudiants.GetId(id); }

        catch
        {
        }
        try { return Professeurs.GetId(id); }
        catch { }
        try { return Administrations.GetId(id); }
        catch { }
        throw new Exception($"{id} Non trouvable");
    }
    public void ListerParCategories()
    {
        Console.WriteLine("== Listes de personnes ==");
        Console.WriteLine("Listes des etudiants ");
        foreach (var e in Etudiants.ListerTous()) Console.WriteLine($"{e.Id}->{e.Nom}->{e.Prenom}");
        Console.WriteLine("Listes des Professeurs ");
        foreach (var p in Professeurs.ListerTous()) Console.WriteLine($"{p.Id}->{p.Nom}->{p.Prenom}");
        Console.WriteLine("Listes des administrateurs ");
        foreach (var a in Administrations.ListerTous()) Console.WriteLine($"{a.Id}->{a.Nom}->{a.Prenom}");
    }

    public void CreerMatiere(string nom, int coefficient, int plcesdisponibles)
    {
        var m = new Matiere();
        m.Nom = nom;
        m.Coefficient = coefficient;
        m.PlacesDisponibles = plcesdisponibles;
        Matieres.Add(m);
        HistoriqueActions.Push($"{DateTime.Now} la matiere {m.Nom} a ete creer");
    }
    public void ProgrammerCours(Matiere matiere, Professeur professeur, DateTime date, string salle)
    {
        var cours = new Cours();
        cours.Matiere = matiere;
        cours.Professeur = professeur;
        cours.Date = date;
        cours.Salle = salle;
        CoursProgrammes.Add(cours);
    }
    public void InscrireEtudiant(Etudiant e, Cours c)
    {

        if (c.InscrireEtudiant(e))
        {
            Console.WriteLine($"{e.Nom} inscrits avec succes ");
            HistoriqueActions.Push($"{DateTime.Now} l'etudiant {e.Nom} a ete inscrit dans un cours");
        }

    }
    public void AttribuerNote(Etudiant e, Matiere m, double note)
    {
        if (e.MatieresInscrits.Contains(m))
        {
            e.Notes[m] = note;
            HistoriqueActions.Push($"{DateTime.Now} la note {m} attribue a l'etudiant  {e.Nom}");
        }
        else Console.WriteLine($"{e.Nom} n'a pas ete attribue la note {m}");
    }
    public decimal MoyenneGeneralEtudiant(Etudiant e)
    {
        return e.CalculMoyenneGenerale();
    }
    public decimal MasseSalarialeEcole()
    {
        decimal total = 0;
        foreach (var p in Professeurs.ListerTous()) total += p.CalculSalaire();
        foreach (var ad in Administrations.ListerTous()) total += ad.CalculSalaire();

        return total;
    }
    public void AnnulerDernierAction()
    {
        if (HistoriqueActions.Count > 0)
        {

            var action = HistoriqueActions.Pop();
            Console.WriteLine($"{action} a ete annulee");
        }
        else Console.WriteLine("Historique Vide!!! ");

    }

    public void Sauvegarder(string fichier = "ecole.json")
    {
        var data = new ContainerData { 
        
        Etudiants = Etudiants.ListerTous(),
        Profsseurs = Professeurs.ListerTous(),
        Administrations = Administrations.ListerTous(),
        Matieres = Matieres,
        CoursProg = CoursProgrammes,
        Historique = HistoriqueActions.ToList()
        
        };
        string json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        });

        File.WriteAllText(fichier, json);
        Console.WriteLine($"Les donnees sont sauvegardees dans le fichier {fichier}");
    }

    public void Charger(string fichier = "ecole.json")
    {
        if (!File.Exists(fichier))
        {
            Console.WriteLine($"le fichier {fichier} est introuvable ");
            return;
        }
        string json = File.ReadAllText(fichier);
        var data = JsonSerializer.Deserialize<ContainerData>(json);
        if (data == null) return;
        foreach (var item in data.Etudiants) Etudiants.AjouterEntite(item);
        foreach(var p in data.Profsseurs) Professeurs.AjouterEntite(p);
        foreach(var ad in data.Administrations) Administrations.AjouterEntite(ad);
        Matieres.AddRange(data.Matieres);
        CoursProgrammes.AddRange(data.CoursProg);
        foreach(var h in data.Historique) HistoriqueActions.Push(h);
        Console.WriteLine($" les donnnes sont chargees depuis le fichier {fichier} avec succes ");
    }
}
public class ContainerData
{
    public List<Etudiant> Etudiants { get; set; } = new List<Etudiant>();
    public List<Professeur> Profsseurs { get; set; } = new List<Professeur>();
    public List<Administration> Administrations { get; set; } = new List<Administration>();
    public List<Matiere> Matieres { get; set; } = new List<Matiere>();
    public List<Cours> CoursProg { get; set; } = new List<Cours>();
    public List<string> Historique { get; set; } = new List<string>();
}

   


namespace GestionEcole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

         
            var ecole = new GestionnaireEcole();

            
            ecole.Charger("ecole.json");

            MenuPrincipal(ecole);

            
            ecole.Sauvegarder("ecole.json");
        }

      
        static void MenuPrincipal(GestionnaireEcole ecole)
        {
            int choix;
            do
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine("║        GESTION DE L'ÉCOLE             ║");
                Console.WriteLine("╠════════════════════════════════════════╣");
                Console.WriteLine("║  1. Gestion des personnes              ║");
                Console.WriteLine("║  2. Gestion académique                 ║");
                Console.WriteLine("║  3. Gestion financière                 ║");
                Console.WriteLine("║  4. Administration                     ║");
                Console.WriteLine("║  0. Quitter                            ║");
                Console.WriteLine("╚════════════════════════════════════════╝");
                Console.Write("\nVotre choix : ");

                choix = int.Parse(Console.ReadLine());

                switch (choix)
                {
                    case 1: MenuPersonnes(ecole); break;
                    case 2: MenuAcademique(ecole); break;
                    case 3: MenuFinancier(ecole); break;
                    case 4: MenuAdministration(ecole); break;
                    case 0:
                        Console.WriteLine("\nAu revoir !");
                        break;
                    default:
                        Console.WriteLine("Choix invalide !");
                        break;
                }
            } while (choix != 0);
        }

       
        static void MenuPersonnes(GestionnaireEcole ecole)
        {
            int choix;
            do
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine("║        GESTION DES PERSONNES          ║");
                Console.WriteLine("╠════════════════════════════════════════╣");
                Console.WriteLine("║  1. Ajouter un étudiant               ║");
                Console.WriteLine("║  2. Ajouter un professeur             ║");
                Console.WriteLine("║  3. Ajouter un administratif          ║");
                Console.WriteLine("║  4. Rechercher une personne           ║");
                Console.WriteLine("║  5. Lister par catégorie              ║");
                Console.WriteLine("║  0. Retour                            ║");
                Console.WriteLine("╚════════════════════════════════════════╝");
                Console.Write("\nVotre choix : ");

                choix = int.Parse(Console.ReadLine());

                switch (choix)
                {
                    case 1: AjouterEtudiant(ecole); break;
                    case 2: AjouterProfesseur(ecole); break;
                    case 3: AjouterAdministratif(ecole); break;
                    case 4: RechercherPersonne(ecole); break;
                    case 5: ecole.ListerParCategories(); break;
                    case 0: break;
                    default:
                        Console.WriteLine("Choix invalide !");
                        break;
                }

                if (choix != 0)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }
            } while (choix != 0);
        }

      
       
        static void AjouterEtudiant(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== AJOUT D'UN ÉTUDIANT ===\n");

            try
            {
                Console.Write("Nom : ");
                string nom = Console.ReadLine();

                Console.Write("Prénom : ");
                string prenom = Console.ReadLine();

                Console.Write("Classe : ");
                string classe = Console.ReadLine();

                var etudiant = new Etudiant
                {
                    Nom = nom,
                    Prenom = prenom,
                    Classe = classe
                };

                ecole.AjouterEtudiant(etudiant);
                Console.WriteLine($"\n Étudiant {prenom} {nom} ajouté avec succès !");
                Console.WriteLine($"   ID : {etudiant.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Erreur : {ex.Message}");
            }
        }

        
       
        static void AjouterProfesseur(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== AJOUT D'UN PROFESSEUR ===\n");

            try
            {
                Console.Write("Nom : ");
                string nom = Console.ReadLine();

                Console.Write("Prénom : ");
                string prenom = Console.ReadLine();

                Console.Write("Matière enseignée : ");
                string matiere = Console.ReadLine();

                Console.Write("Salaire de base : ");
                decimal salaire = decimal.Parse(Console.ReadLine());

                ecole.AjouterProfesseur(nom, prenom, matiere, salaire);
                Console.WriteLine($"\n Professeur {prenom} {nom} ajouté avec succès !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Erreur : {ex.Message}");
            }
        }

        static void AjouterAdministratif(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== AJOUT D'UN ADMINISTRATIF ===\n");

            try
            {
                Console.Write("Nom : ");
                string nom = Console.ReadLine();

                Console.Write("Prénom : ");
                string prenom = Console.ReadLine();

                Console.Write("Département : ");
                string departement = Console.ReadLine();

                Console.Write("Salaire fixe : ");
                decimal salaire = decimal.Parse(Console.ReadLine());

                ecole.AjouterAdministratif(nom, prenom, departement, salaire);
                Console.WriteLine($"\n Administratif {prenom} {nom} ajouté avec succès !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Erreur : {ex.Message}");
            }
        }

        
        static void RechercherPersonne(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== RECHERCHE D'UNE PERSONNE ===\n");

            try
            {
                Console.Write("1. Rechercher par ID\n");
                //Console.Write("2. Rechercher par Nom\n");
               // Console.Write("Votre choix : ");
               // int choix = int.Parse(Console.ReadLine());

                
                
                    Console.Write("ID : ");
                    int id = int.Parse(Console.ReadLine());
                    var personne = ecole.RechercherPersonne(id);
                    AfficherPersonne(personne);
                
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n {ex.Message}");
            }
        }

        static void AfficherPersonne(Personne personne)
        {
            Console.WriteLine($"\n Personne trouvée :");
            Console.WriteLine($"   ID : {personne.Id}");
            Console.WriteLine($"   Nom : {personne.Nom}");
            Console.WriteLine($"   Prénom : {personne.Prenom}");
            Console.WriteLine($"   Type : {personne.GetType().Name}");
        }

       
        static void MenuAcademique(GestionnaireEcole ecole)
        {
            int choix;
            do
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine("║        GESTION ACADÉMIQUE             ║");
                Console.WriteLine("╠════════════════════════════════════════╣");
                Console.WriteLine("║  1. Créer une matière                 ║");
                Console.WriteLine("║  2. Programmer un cours               ║");
                Console.WriteLine("║  3. Inscrire à un cours               ║");
                Console.WriteLine("║  4. Attribuer une note                ║");
                Console.WriteLine("║  5. Voir moyenne d'un étudiant        ║");
                Console.WriteLine("║  0. Retour                            ║");
                Console.WriteLine("╚════════════════════════════════════════╝");
                Console.Write("\nVotre choix : ");

                choix = int.Parse(Console.ReadLine());

                switch (choix)
                {
                    case 1: CreerMatiere(ecole); break;
                    case 2: ProgrammerCours(ecole); break;
                    case 3: InscrireCours(ecole); break;
                    case 4: AttribuerNote(ecole); break;
                    case 5: VoirMoyenne(ecole); break;
                    case 0: break;
                    default:
                        Console.WriteLine("Choix invalide !");
                        break;
                }

                if (choix != 0)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }
            } while (choix != 0);
        }

        
        static void CreerMatiere(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== CRÉER UNE MATIÈRE ===\n");

            try
            {
                Console.Write("Nom de la matière : ");
                string nom = Console.ReadLine();

                Console.Write("Coefficient : ");
                int coefficient = int.Parse(Console.ReadLine());

                Console.Write("Places disponibles : ");
                int places = int.Parse(Console.ReadLine());

                ecole.CreerMatiere(nom, coefficient, places);
                Console.WriteLine($"\n Matière {nom} créée avec succès !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Erreur : {ex.Message}");
            }
        }

        
        static void ProgrammerCours(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== PROGRAMMER UN COURS ===\n");

            try
            {
                
                Console.WriteLine("Matières disponibles :");
                for (int i = 0; i < ecole.Matieres.Count; i++)
                {
                    Console.WriteLine($"   {i + 1}. {ecole.Matieres[i].Nom}");
                }
                Console.Write("Choisissez une matière : ");
                int matiereIndex = int.Parse(Console.ReadLine()) - 1;
                var matiere = ecole.Matieres[matiereIndex];

                
                Console.WriteLine("\nProfesseurs disponibles :");
                var profs = ecole.Professeurs.ListerTous();
                for (int i = 0; i < profs.Count; i++)
                {
                    Console.WriteLine($"   {i + 1}. {profs[i].Nom} {profs[i].Prenom}");
                }
                Console.Write("Choisissez un professeur : ");
                int profIndex = int.Parse(Console.ReadLine()) - 1;
                var prof = profs[profIndex];

                Console.Write("Date (ex: 2024-12-25) : ");
                DateTime date = DateTime.Parse(Console.ReadLine());

                Console.Write("Salle : ");
                string salle = Console.ReadLine();

                ecole.ProgrammerCours(matiere, prof, date, salle);
                Console.WriteLine($"\n Cours programmé avec succès !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Erreur : {ex.Message}");
            }
        }

        
        static void InscrireCours(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== INSCRIRE À UN COURS ===\n");

            try
            {
               
                Console.WriteLine("Étudiants disponibles :");
                var etudiants = ecole.Etudiants.ListerTous();
                for (int i = 0; i < etudiants.Count; i++)
                {
                    Console.WriteLine($"   {i + 1}. {etudiants[i].Nom} {etudiants[i].Prenom}");
                }
                Console.Write("Choisissez un étudiant : ");
                int etudiantIndex = int.Parse(Console.ReadLine()) - 1;
                var etudiant = etudiants[etudiantIndex];

               
                Console.WriteLine("\nCours disponibles :");
                for (int i = 0; i < ecole.CoursProgrammes.Count; i++)
                {
                    var c = ecole.CoursProgrammes[i];
                    Console.WriteLine($"   {i + 1}. {c.Matiere.Nom} - {c.Professeur.Nom} - {c.Date:dd/MM/yyyy}");
                }
                Console.Write("Choisissez un cours : ");
                int coursIndex = int.Parse(Console.ReadLine()) - 1;
                var cours = ecole.CoursProgrammes[coursIndex];

                ecole.InscrireEtudiant(etudiant, cours);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Erreur : {ex.Message}");
            }
        }

        
        static void AttribuerNote(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== ATTRIBUER UNE NOTE ===\n");

            try
            {
                
                Console.WriteLine("Étudiants disponibles :");
                var etudiants = ecole.Etudiants.ListerTous();
                for (int i = 0; i < etudiants.Count; i++)
                {
                    Console.WriteLine($"   {i + 1}. {etudiants[i].Nom} {etudiants[i].Prenom}");
                }
                Console.Write("Choisissez un étudiant : ");
                int etudiantIndex = int.Parse(Console.ReadLine()) - 1;
                var etudiant = etudiants[etudiantIndex];

                
                Console.WriteLine($"\nMatières de {etudiant.Nom} :");
                for (int i = 0; i < etudiant.MatieresInscrits.Count; i++)
                {
                    Console.WriteLine($"   {i + 1}. {etudiant.MatieresInscrits[i].Nom}");
                }
                Console.Write("Choisissez une matière : ");
                int matiereIndex = int.Parse(Console.ReadLine()) - 1;
                var matiere = etudiant.MatieresInscrits[matiereIndex];

                Console.Write("Note : ");
                double note = double.Parse(Console.ReadLine());

                ecole.AttribuerNote(etudiant, matiere, note);
                Console.WriteLine($"\n Note attribuée avec succès !");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Erreur : {ex.Message}");
            }
        }

       
        static void VoirMoyenne(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== MOYENNE D'UN ÉTUDIANT ===\n");

            try
            {
                var etudiants = ecole.Etudiants.ListerTous();
                for (int i = 0; i < etudiants.Count; i++)
                {
                    Console.WriteLine($"   {i + 1}. {etudiants[i].Nom} {etudiants[i].Prenom}");
                }
                Console.Write("Choisissez un étudiant : ");
                int index = int.Parse(Console.ReadLine()) - 1;
                var etudiant = etudiants[index];

                decimal moyenne = ecole.MoyenneGeneralEtudiant(etudiant);
                Console.WriteLine($"\n Moyenne générale de {etudiant.Nom} {etudiant.Prenom} : {moyenne:F2}/20");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n Erreur : {ex.Message}");
            }
        }

        
        static void MenuFinancier(GestionnaireEcole ecole)
        {
            int choix;
            do
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine("║        GESTION FINANCIÈRE             ║");
                Console.WriteLine("╠════════════════════════════════════════╣");
                Console.WriteLine("║  1. Calculer salaire professeur        ║");
                Console.WriteLine("║  2. Calculer salaire administratif     ║");
                Console.WriteLine("║  3. Générer une fiche de paie          ║");
                Console.WriteLine("║  4. Masse salariale totale             ║");
                Console.WriteLine("║  0. Retour                            ║");
                Console.WriteLine("╚════════════════════════════════════════╝");
                Console.Write("\nVotre choix : ");

                choix = int.Parse(Console.ReadLine());

                switch (choix)
                {
                    case 1: SalaireProfesseur(ecole); break;
                    case 2: SalaireAdministratif(ecole); break;
                    case 3: GenererFichePaie(ecole); break;
                    case 4: MasseSalariale(ecole); break;
                    case 0: break;
                    default:
                        Console.WriteLine("Choix invalide !");
                        break;
                }

                if (choix != 0)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }
            } while (choix != 0);
        }

        
        static void SalaireProfesseur(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== SALAIRE PROFESSEUR ===\n");

            var profs = ecole.Professeurs.ListerTous();
            for (int i = 0; i < profs.Count; i++)
            {
                Console.WriteLine($"   {i + 1}. {profs[i].Nom} {profs[i].Prenom}");
            }
            Console.Write("Choisissez un professeur : ");
            int index = int.Parse(Console.ReadLine()) - 1;
            var prof = profs[index];

            Console.WriteLine($"\n Salaire de {prof.Nom} {prof.Prenom} : {prof.CalculSalaire():F2} €");
        }

        
        static void SalaireAdministratif(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== SALAIRE ADMINISTRATIF ===\n");

            var admins = ecole.Administrations.ListerTous();
            for (int i = 0; i < admins.Count; i++)
            {
                Console.WriteLine($"   {i + 1}. {admins[i].Nom} {admins[i].Prenom}");
            }
            Console.Write("Choisissez un administratif : ");
            int index = int.Parse(Console.ReadLine()) - 1;
            var admin = admins[index];

            Console.WriteLine($"\n Salaire de {admin.Nom} {admin.Prenom} : {admin.CalculSalaire():F2} €");
        }

        
        static void GenererFichePaie(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== GÉNÉRER FICHE DE PAIE ===\n");

            Console.WriteLine("1. Professeur");
            Console.WriteLine("2. Administratif");
            Console.Write("Votre choix : ");
            int choix = int.Parse(Console.ReadLine());

            if (choix == 1)
            {
                var profs = ecole.Professeurs.ListerTous();
                for (int i = 0; i < profs.Count; i++)
                {
                    Console.WriteLine($"   {i + 1}. {profs[i].Nom} {profs[i].Prenom}");
                }
                Console.Write("Choisissez un professeur : ");
                int index = int.Parse(Console.ReadLine()) - 1;
                Console.WriteLine($"\n{profs[index].GenererFichePaie()}");
            }
            else if (choix == 2)
            {
                var admins = ecole.Administrations.ListerTous();
                for (int i = 0; i < admins.Count; i++)
                {
                    Console.WriteLine($"   {i + 1}. {admins[i].Nom} {admins[i].Prenom}");
                }
                Console.Write("Choisissez un administratif : ");
                int index = int.Parse(Console.ReadLine()) - 1;
                Console.WriteLine($"\n{admins[index].GenererFichePaie()}");
            }
        }

        static void MasseSalariale(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== MASSE SALARIALE TOTALE ===\n");

            decimal total = ecole.MasseSalarialeEcole();
            Console.WriteLine($" Masse salariale totale : {total:F2} €");
        }

        
        static void MenuAdministration(GestionnaireEcole ecole)
        {
            int choix;
            do
            {
                Console.Clear();
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine("║        ADMINISTRATION                 ║");
                Console.WriteLine("╠════════════════════════════════════════╣");
                Console.WriteLine("║  1. Voir historique                   ║");
                Console.WriteLine("║  2. Annuler dernière action           ║");
                Console.WriteLine("║  3. Sauvegarder                       ║");
                Console.WriteLine("║  4. Charger                           ║");
                Console.WriteLine("║  0. Retour                            ║");
                Console.WriteLine("╚════════════════════════════════════════╝");
                Console.Write("\nVotre choix : ");

                choix = int.Parse(Console.ReadLine());

                switch (choix)
                {
                    case 1: VoirHistorique(ecole); break;
                    case 2: AnnulerAction(ecole); break;
                    case 3: Sauvegarder(ecole); break;
                    case 4: Charger(ecole); break;
                    case 0: break;
                    default:
                        Console.WriteLine("Choix invalide !");
                        break;
                }

                if (choix != 0)
                {
                    Console.WriteLine("\nAppuyez sur une touche pour continuer...");
                    Console.ReadKey();
                }
            } while (choix != 0);
        }

        static void VoirHistorique(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== HISTORIQUE ===\n");

            var historique = ecole.HistoriqueActions.ToList();
            if (historique.Count == 0)
            {
                Console.WriteLine("Aucune action dans l'historique.");
                return;
            }

            for (int i = historique.Count - 1; i >= 0; i--)
            {
                Console.WriteLine($"   {historique[i]}");
            }
        }

        static void AnnulerAction(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== ANNULER DERNIÈRE ACTION ===\n");
            ecole.AnnulerDernierAction();
        }

    
        static void Sauvegarder(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== SAUVEGARDER ===\n");
            ecole.Sauvegarder("ecole.json");
        }

       
        static void Charger(GestionnaireEcole ecole)
        {
            Console.Clear();
            Console.WriteLine("=== CHARGER ===\n");
            ecole.Charger("ecole.json");
        }
    }
}