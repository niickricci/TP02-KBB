using RaymondCharlesServicesBase;
using System.Collections.Generic;
using static RaymondCharlesClientMT.Exceptions;
using static RaymondCharlesServicesBase.Carte;

namespace RaymondCharlesClientMT
{
    /// <summary>
    ///Expose les dimensions d'une console classique une
    /// hauteur (NB_LIGNES) de 25 et une largeur(NB_COLONNES) de 80.
    /// </summary>
    public static class ConsoleClassique
    {
        public const int NB_LIGNES = 25;
        public const int NB_COLONNES = 80;
    }


    /// <summary>
    /// Les types/sortes de directeurs.
    /// </summary>
    public enum SorteDirecteur { Inconnu, Humain, Brownien } //Fait


    /// <summary>
    /// Implémente le schéma de conception Fabrique/Factory pour créer un directeur.
    /// </summary>
    public static class FabriqueDirecteur
    {
        public static IDirecteur Créer(SorteDirecteur sorte)
        {
            switch (sorte)
            {
                case SorteDirecteur.Inconnu:
                    return new DirecteurBrownien();
                case SorteDirecteur.Humain:
                    return new DirecteurHumain();
                case SorteDirecteur.Brownien:
                    return new DirecteurBrownien();
                default:
                    throw new ArgumentException("Sorte de directeur inconnue.");
            }
        }


        /// <summary>
        /// Prend en paramètre une string et retourne un SorteDirecteur soit un type de directeur..
        /// </summary>
        /// <param name="nomSorte">Type/Sorte de directeur en string.</param>
        /// <returns>Le type/sorte de directeur (SorteDirecteur) à partir d'un string</returns>
        /// <exception cref="ArgumentException"></exception>
        public static SorteDirecteur TraduireSorte(string nomSorte)
        {
            switch (nomSorte.ToLower())
            {
                case "inconnu":
                    return SorteDirecteur.Inconnu;
                case "humain":
                    return SorteDirecteur.Humain;
                case "brownien":
                    return SorteDirecteur.Brownien;
                default:
                    throw new ArgumentException("Nom de sorte inconnu.");
            }
        }
    }


    /// <summary>
    /// Un Participant est une sorte de Protagoniste. Son symbole se traduit par 'R' (RaymondCharles).
    /// </summary>
    public class Participant : Protagoniste
    {
        public static char SYMBOLE => 'R';
        public override char Symbole => SYMBOLE;

        public Participant(Point position, PanneauAffichage menu) : base(position, new DirecteurHumain(), menu) { }

        public Participant(Point position, IDirecteur directeur, PanneauAffichage menu) : base(position, directeur, menu) { }

        public new void Associer(IDirecteur directeur)
        {
            base.Associer(directeur);
        }
    }


    /// <summary>
    /// Un Obstacleur crée un certain nombre d’instances
    ///d’Obstacle. Un peu comme le fait la Centrale avec les instances de Capteur.
    /// </summary>
    public class Obstacleur
    {
        List<char> dejaAttribues = new();
        public List<Obstacle> listeObstacleur = new();
        public Carte UneCarte { get; private set; }
        public IDirecteur directeur { get; private set; }

        //Thread th;
        List<Thread> listeThreads;
         bool thTerminer = false;
        private object mutex = new object();

        public Obstacleur(Carte map, int nbObst)
        {
            listeThreads = new List<Thread>();
            UneCarte = map;
            directeur = new DirecteurBrownien();
            dejaAttribues.Add('R');
            dejaAttribues.Add('r');
            dejaAttribues.Add('X');
            dejaAttribues.Add('x');
            dejaAttribues.Add('C');
            dejaAttribues.Add('c');
            dejaAttribues.Add('M');
            dejaAttribues.Add('m');

            for (int i = 0; i < nbObst; i++)
            {
                listeObstacleur.Add(new Obstacle(TrouverCaseVide(map), SymboleRandom(dejaAttribues)));
            }

            foreach(Obstacle obst in listeObstacleur)
            {
                obst.Associer(directeur);
            }
        }

        public void Populer()
        {

            lock (mutex)
            {
                foreach (Obstacle obst in listeObstacleur)
                {
                    UneCarte.Installer(obst);
                                              
                    Thread th = new Thread(() =>
                    {
                        while (!thTerminer)
                        {
                            Thread.Sleep(1000);
                            UneCarte.Appliquer(obst, directeur.Agir(obst, UneCarte, null));
                        }
                    });


                    listeThreads.Add(th);
                }
            }
            Demarrer();
        }
        public char SymboleRandom(List<char> dejaAttribues)
        {
            Random random = new Random();
            char randomChar;

            do
            {      
                int minChar = 65; // Code ASCII pour 'A'
                int maxChar = 90; // Code ASCII pour 'Z'


                int randomNum = random.Next(minChar, maxChar + 1);


                randomChar = (char)randomNum;

            } while (dejaAttribues.Contains(randomChar));

            dejaAttribues.Add(randomChar);
            return randomChar;
        }

        public Point TrouverCaseVide(Carte map)
        {
            List<Point> listeCaseVide;
            Random random = new Random();

            listeCaseVide = map.Trouver(' ');

            Point positionHazard = listeCaseVide[random.Next(listeCaseVide.Count)];

            return positionHazard;
        }

        public void Demarrer()
        {
            foreach (Thread thread in listeThreads)
            {
                if (!thread.IsAlive)
                {
                    thread.Start();
                }
            }
        }

        public void Terminer()
        {
            thTerminer = true;

            //Attendre que les threads finissent
            foreach (Thread thread in listeThreads)
            {
                if (thread.IsAlive)
                {
                    thread.Join();
                }
            }
        }
    }


    /// <summary>
    /// Une instance de la classe DirecteurHumain implémente l’interface IDirecteur de
    ///manière à ce que sa méthode Agir affiche un menu destiné à un humain, puis lise une touche au
    ///clavier et retourne un Choix
    /// </summary>
    public class DirecteurHumain : IDirecteur
    {
        public Choix Agir(Protagoniste protagoniste, Carte carte, IAffichable? menu)
        {
            if (menu is PanneauAffichage panneauAffichage)
            {
                panneauAffichage.Write($"TP2 - Nicolas Ricci | Carte de format {carte.Hauteur} x {carte.Largeur}\n                    | Entrez q pour quitter\n                    | Votre choix?\n", ConsoleColor.Yellow);
            }


            ConsoleKeyInfo keyInfo = Console.ReadKey();

            switch (keyInfo.Key)
            {
                case ConsoleKey.UpArrow:
                    return Choix.Haut;
                case ConsoleKey.DownArrow:
                    return Choix.Bas;
                case ConsoleKey.LeftArrow:
                    return Choix.Gauche;
                case ConsoleKey.RightArrow:
                    return Choix.Droite;
                case ConsoleKey.Q:
                    return Choix.Quitter;
                default:
                    return Choix.Rien;
            }
        }
    }


    /// <summary>
    /// Une instance de la classe DirecteurBrownien implémentera l’interface IDirecteur de
    ///manière à ce que sa méthode Agir prenne des décisions de manière analogue à un mouvement
    ///Brownien, soit faire un Choix qui déplacerait le Protagoniste pris en charge dans une
    ///case adjacente à la case courante de manière aléatoire, mais sans entrer en collision.
    /// </summary>
    public class DirecteurBrownien : IDirecteur
    {
        public Choix Agir(Protagoniste protagoniste, Carte carte, IAffichable? menu)
        {
            return MouvementBrownien(protagoniste, carte);
        }

        public Choix MouvementBrownien(Protagoniste protagoniste, Carte carte)
        {
            Point positionProta = protagoniste.Position;
            char[,] map = carte.Snapshot();
            object mutex = new object();

            Random random = new Random();

            int decision = random.Next(0, 4);
            //lock (mutex)
            //{
                switch (decision)
                {
                    case 0:
                        if (!carte.EstDisponible(new(positionProta.X + 1, positionProta.Y)))
                        {
                            return Choix.Rien;
                        }

                        return Choix.Haut;



                    case 1:
                        if (!carte.EstDisponible(new(positionProta.X - 1, positionProta.Y)))
                        {
                            return Choix.Rien;
                        }

                        return Choix.Bas;




                    case 2:
                        if (!carte.EstDisponible(new(positionProta.X, positionProta.Y - 1)))
                        {
                            return Choix.Rien;
                        }
                        return Choix.Gauche;



                    case 3:
                        if (!carte.EstDisponible(new(positionProta.X, positionProta.Y + 1)))
                        {
                            return Choix.Rien;
                        }
                        return Choix.Droite;




                    default:
                        return Choix.Rien;
                }
            //}

        }
    } 

    /// <summary>
    /// 
    /// </summary>
    public class Centrale
    {
        FabriqueGénérateurs fabID = new FabriqueGénérateurs();
        public List<Capteur> listeCapteur = new();
        public List<Obstacle> listeObst = new();
        public Carte UneCarte { get; private set; }
        public Ardoise UneArdoise { get; private set; }
        public Protagoniste UnProtagoniste { get; private set; }
        public Centrale(Carte map, Protagoniste protagoniste, List<Obstacle> listeObst, PanneauAffichage pan, Ardoise ardoise)
        {
            UneCarte = map;
            UneArdoise = ardoise;
            UnProtagoniste = protagoniste;
            this.listeObst = listeObst;

            CaméraProximale camProx = new CaméraProximale(map, fabID.Créer(TypeGénérateur.Aléatoire, "CP").Prendre(), UnProtagoniste, TrouverCaseVide(map), pan);
            DétecteurMouvement detecteurMouv = new DétecteurMouvement(map, protagoniste, listeObst, fabID.Créer(TypeGénérateur.Aléatoire, "DM").Prendre(), TrouverCaseVide(map), ardoise);
            listeCapteur.Add(camProx);
            listeCapteur.Add(detecteurMouv);

            FiltreCaméra filtreCamera = new FiltreCaméra();
            DépôtFiltres.Empiler(filtreCamera);
            filtreCamera.InsérerFiltre(camProx.Symbole, ConsoleColor.Red);

        }

        public void Populer()
        {
            List<Point> listeCaseVide;
            listeCaseVide = UneCarte.Trouver(' ');
            Random random = new Random();

            for (int i = 0; i < listeCaseVide.Count; i++)
            {
                Point point = listeCaseVide[i];

                if (UneCarte.EstDans(point) || UneCarte.EstSurFrontière(point))
                {
                    listeCaseVide.RemoveAt(i);
                }
            }

            UneCarte.Installer(listeCapteur);
        }

        public Point TrouverCaseVide(Carte map)
        {
            List<Point> listeCaseVide;
            Random random = new Random();

            listeCaseVide = map.Trouver(' ');

            Point positionHazard = listeCaseVide[random.Next(listeCaseVide.Count)];

            return positionHazard;
        }
    } 


    /// <summary>
    /// 
    /// </summary>
    public class Caméra : Capteur
    {

        public override char Symbole { get { return 'C'; } }

        public Caméra(Carte carte, Identifiant identifiant, Point pos) : base(identifiant, pos)
        {
            carte.Abonner(this);
        }

        public override void MouvementObservé(Carte carte)
        {
            carte.Snapshot();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class CaméraProximale : Capteur
    {
        public override char Symbole { get { return 'x'; } }
        public Protagoniste UnProtagoniste { get; private set; }
        public PanneauAffichage Pan { get; private set; }

        private double distanceLimite = 3.5;

        public CaméraProximale(Carte carte, Identifiant identifiant, Protagoniste protagoniste, Point pos, PanneauAffichage pan) : base(identifiant, pos)
        {
            UnProtagoniste = protagoniste;
            Pan = pan;
            carte.Abonner(this);
        }

        public override void MouvementObservé(Carte carte)
        {
            char[,] map = carte.Snapshot();
            List<(char ch, ConsoleColor c)> messages = new List<(char ch, ConsoleColor c)>();

            for (int y = 0; y < carte.Hauteur; y++)
            {
                for (int x = 0; x < carte.Largeur; x++)
                {
                    Point caseCourante = new Point(x, y);
                    double distanceCaseProta = UnProtagoniste.Position.Distance(caseCourante);

                    ConsoleColor couleurFiltre;

                    if (map[caseCourante.Y, caseCourante.X] == Symbole)
                    {
                        couleurFiltre = (distanceCaseProta > distanceLimite) ? DépôtFiltres.Courant.ObtenirFiltre(Symbole) : ConsoleColor.Cyan;
                    }
                    else
                    {
                        couleurFiltre = (distanceCaseProta <= distanceLimite) ? ConsoleColor.Cyan : ConsoleColor.White;
                    }

                    messages.Add((map[caseCourante.Y, caseCourante.X], couleurFiltre));
                }

                messages.Add(('\n', ConsoleColor.White));
            }

            Pan.Write(messages); 
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class DétecteurMouvement : Capteur
    {
        private Dictionary<char, Point> positionsPrec = new();
        private const int Portée = 5;
        public override char Symbole { get { return 'm'; } }
        public Ardoise Ardoise { get; private set; }
        public Protagoniste p { get; private set; }
        private double distanceProche = 3.5;
        public List<Obstacle> listObst = new();


        public DétecteurMouvement(Carte carte, Protagoniste prota, List<Obstacle> listeObst, Identifiant identifiant, Point pos, Ardoise ardoise) : base(identifiant, pos)
        {
            Ardoise = ardoise;
            carte.Abonner(this);
            p = prota;
            listObst = listeObst;
            positionsPrec.Add(p.Symbole, p.Position);

            foreach(Obstacle obst in listeObst)
            {
                positionsPrec.Add(obst.Symbole, obst.Position);
            }

        }

        public override void MouvementObservé(Carte carte)
        {
            char[,] map = carte.Snapshot();

            string qui;
            string quoi;


            ConsoleColor couleurFiltre;
 
            foreach (Obstacle obst in listObst)
            {
                double distanceO = obst.Position.Distance(this.Position);

                if (distanceO <= Portée)
                {
                    if (!positionsPrec.ContainsKey(obst.Symbole) || positionsPrec[obst.Symbole] != obst.Position)
                    {
                        couleurFiltre = (distanceO <= distanceProche) ? ConsoleColor.Red : ConsoleColor.Yellow;
                        qui = obst.Symbole.ToString();
                        quoi = $"Distance de {Math.Round(distanceO, 2).ToString("F2")}";
                        Ardoise.Ajouter(qui, quoi, couleurFiltre);

                        positionsPrec[obst.Symbole] = obst.Position;
                    }
                }
            }

            double distanceP = p.Position.Distance(this.Position);
            if (distanceP <= Portée)
            {
                if (!positionsPrec.ContainsKey(p.Symbole) || positionsPrec[p.Symbole] != p.Position)
                {
                    couleurFiltre = (distanceP <= distanceProche) ? ConsoleColor.Red : ConsoleColor.Yellow;
                    qui = p.Symbole.ToString();
                    quoi = $"Distance de {Math.Round(distanceP, 2).ToString("F2")}";
                    Ardoise.Ajouter(qui, quoi, couleurFiltre);

                    positionsPrec[p.Symbole] = p.Position;
                }
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class Diagnosticien : IObservateurComportements
    {

        private int compteurCollision = 0;
        private const int nbCollisionMax = 5;
        private raisonArrêt raison = raisonArrêt.poursuivre;

        //Prédicat Poursuivre 
        public bool Poursuivre(Carte carte, Participant participant)
        {
            Point position = participant.Position;

            if (compteurCollision >= 5)
            {
                raison = raisonArrêt.collisionMax;
                return false;
            }
            else if (carte.EstSurFrontière(position))
            {
                raison = raisonArrêt.sortie;
                return false;
            }
            else if (raison == raisonArrêt.departV)
            {
                return false;
            }

            return true;
        }


        public void CollisionObservée(Protagoniste protagoniste)
        {

            //signaler une collision 
            Console.Beep();

            compteurCollision++;

            if (compteurCollision >= nbCollisionMax) //danger pour protagoniste
            {
                raison = raisonArrêt.collisionMax;
            }
        }


        public string ExpliquerArrêt()
        {

            if (raison == raisonArrêt.departV)
            {
                return "Départ volontaire.";
            }
            if (raison == raisonArrêt.collisionMax)
            {
                return "Risque pour la santé du Protagoniste.";
            }
            else if (raison == raisonArrêt.sortie)
            {
                return "Le Protagoniste est sorti de la pièce.";
            }
            else
            {
                return "Raison d'arrêt inconnue.";
            }
        }

        //Méthode Analyser
        public Choix Analyser(Choix choix, Protagoniste protagoniste, Carte carte)
        {
            if (choix == Choix.Quitter)
            {
                raison = raisonArrêt.departV;
            }
            return choix;
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public interface IObservateurAffichage
    {
        void MiseÀJour(PanneauAffichage panneau);
    }


    /// <summary>
    /// 
    /// </summary>
    public class PanneauAffichage : IAffichable
    {
        public bool Modifié { get; protected set; } = false;
        public ConsoleColor Fond { get; init; } = ConsoleColor.Black;
        public Point Position { get; private init; }
        public int Hauteur { get; private init; }
        public int Largeur { get; private init; }
        (char car, ConsoleColor coul)[,] Grille { get; init; }
        private List<IObservateurAffichage> Observateurs { get; } = new List<IObservateurAffichage>();

        public PanneauAffichage(int hau, int lar, Point pos, ConsoleColor fond)
        {
            Hauteur = hau;
            Largeur = lar;
            Position = pos;
            Fond = fond;
            Grille = new (char, ConsoleColor)[Hauteur, Largeur];
        }
        public void Write(string s, ConsoleColor coul)
        {
            Clear();
            var texte = s.Split(new[] { '\n' });
            for (int i = 0; i != texte.Length && i < Hauteur; ++i)
                for (int j = 0; j != texte[i].Length && j < Largeur; ++j)
                    Grille[i, j] = (texte[i][j], coul);
            Modifié = true;
            NotifierObs();
        }
        public void Write(List<(char ch, ConsoleColor c)> messages)
        {
            //Clear();

            //for (int i = 0; i < messages.Count && i < Hauteur; i++)
            //{
            //    char character = messages[i].ch;
            //    ConsoleColor couleur = messages[i].c;

            //    for (int j = 0; j < Largeur; j++)
            //    {
            //        Grille[i, j] = (character, couleur);
            //    }
            //}

            //Modifié = true;
            //NotifierObs();

            Clear();
            int x = 0, y = 0; // Position initiale dans la grille

            foreach (var message in messages)
            {
                if (message.ch == '\n') // Passer à la ligne suivante sur un retour à la ligne
                {
                    y++;
                    x = 0;
                    continue;
                }

                if (x < Largeur && y < Hauteur) // Vérifier si les indices sont dans la plage valide
                {
                    Grille[y, x] = message;
                    x++; // Passer au caractère suivant dans la ligne
                }
            }

            Modifié = true;
            NotifierObs();
        }

        public void Clear()
        {
            for (int i = 0; i != Hauteur; ++i)
                for (int j = 0; j != Largeur; ++j)
                    Grille[i, j] = (' ', Fond);
            Modifié = true;
            NotifierObs();
        }
        void ProjeterÀ(int x, int y, char c, ConsoleColor coul)
        {

            Console.SetCursorPosition(x, y);
            var préFront = Console.ForegroundColor;
            var préBack = Console.BackgroundColor;
            Console.ForegroundColor = coul;
            Console.BackgroundColor = Fond;
            Console.Write(c);
            Console.ForegroundColor = préFront;
            Console.BackgroundColor = préBack;
        }
        public void Projeter()
        {
           
            for (int i = 0; i != Hauteur; ++i)
                for (int j = 0; j != Largeur; ++j)
                {
                    var (car, coul) = Grille[i, j];
                    ProjeterÀ(j + Position.X, i + Position.Y, car, coul);
                }
            Modifié = false;
        }
        public void Ajouter(IObservateurAffichage obs)
        {
            Observateurs.Add(obs);
        }
        public void NotifierObs()
        {
            foreach(var obs in Observateurs)
            {
                obs.MiseÀJour(this);
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class Écran : IObservateurMouvement, IObservateurAffichage
    {
        List<PanneauAffichage> listPan { get; } = new List<PanneauAffichage>();
        public int Largeur { get; }
        public int Hauteur { get; }
        public char[,] Grille { get; }
        private bool enMarche;
        private Thread thread;
        private object mutex = new object();

        public Écran(List<PanneauAffichage> listP)
        {
            listPan = listP;

            foreach (var panneaux in listP)
            {
                Largeur += panneaux.Largeur;
                Hauteur += panneaux.Hauteur;
                panneaux.Ajouter(this);

            }
            Grille = new char[Largeur, Hauteur];
            thread = new Thread(RafraichirPanneaux);
        }

        public void Démarrer()
        {
            if (!enMarche)
            {
                enMarche = true;
                thread.Start();
            }
        }

        public void Arrêter()
        {
            if (enMarche)
            {
                enMarche = false;
                lock (mutex)
                {
                    thread.Join();
                }
            }

        }

        private void RafraichirPanneaux()
        {
            while (enMarche)
            {
                Console.CursorVisible = false; 

                foreach (var panneau in listPan)
                {
                    lock (mutex)
                    {
                        panneau.Projeter();
                    }
                }
                Thread.Sleep(1000); 
            }
        }

        public void MiseÀJour(PanneauAffichage panneau)
        {
            Démarrer();
        }

        public void MouvementObservé(Carte c)
        {
            Démarrer();
        }
    }


    /// <summary>
    /// 
    /// </summary>
    public class Ardoise : PanneauAffichage
    {
        Dictionary<string, List<(string message, ConsoleColor color)>> InfoArdoise;
        List<(string, ConsoleColor)> colonneMsg;
        object mutex = new object();
        private int largeurColonne;


        public Ardoise(int hauteur, int largeur, Point pos, ConsoleColor fond) : base(hauteur, largeur, pos, fond)
        {
            InfoArdoise = new Dictionary<string, List<(string message, ConsoleColor color)>>();
            colonneMsg = new List<(string, ConsoleColor)>();
            largeurColonne = largeur / 4;
        }

        public void Ajouter(string qui, string quoi, ConsoleColor couleur)
        {
            lock (mutex)
            {
                // Check if 'qui' already exists in 'InfoArdoise'
                if (!InfoArdoise.ContainsKey(qui))
            {
                // If not, create a new list for this 'qui'
                InfoArdoise[qui] = new List<(string, ConsoleColor)>();
            }

            // Now, add the message to the specific list of 'qui'
            InfoArdoise[qui].Add((quoi, couleur));

            // Call 'Publier' to update the display
            Publier();}
        }


        public void Publier()
        {
            lock (mutex)
            {
                List<(char, ConsoleColor)> contenu = new List<(char, ConsoleColor)>();

                // Pour chaque ligne de l'ardoise
                for (int l = Hauteur; l >= 0; l--)  // Assurez-vous de définir 'nombreDeLignes' selon votre ardoise
                {
                    // Calculate the total number of messages in all columns
                    foreach (var key in InfoArdoise.Keys.ToList())
                    {
                        while (InfoArdoise[key].Count >= Hauteur + 2)
                        {
                            InfoArdoise[key].RemoveAt(0); // Remove the oldest message from the column
                        }
                    }

                    // Pour chaque colonne dans InfoArdoise
                    // Pour chaque colonne dans InfoArdoise
                    foreach (var colonneMsg in InfoArdoise)
                    {
                        string msg;
                        ConsoleColor couleur;

                        // Vérifiez si la ligne actuelle 'l' est dans la plage de ce message
                        if (l < colonneMsg.Value.Count)
                        {
                            var messageEtCouleur = colonneMsg.Value[l];
                            msg = $"| {colonneMsg.Key} | {messageEtCouleur.Item1}";
                            couleur = messageEtCouleur.Item2;

                            // Ajoutez des espaces pour atteindre la 'largeurColonne'
                            msg = msg.PadRight(this.largeurColonne, ' ');
                        }
                        else
                        {
                            // Ligne sans message: remplir avec des espaces
                            msg = new string(' ', this.largeurColonne);
                            couleur = ConsoleColor.White;
                        }

                        // Ajoutez chaque caractère du message à 'contenu'
                        foreach (char ch in msg)
                        {
                            contenu.Add((ch, couleur));
                        }
                    }

                    // Ajoutez un retour à la ligne après chaque ligne complétée
                    contenu.Add(('\n', ConsoleColor.White));
                }

                // Écrivez le contenu
                Write(contenu);
            }
        }
    } 


    /// <summary>
    /// 
    /// </summary>
    public static  class Organisateur
    {
        public static (Diagnosticien, Écran, Obstacleur)
        Organiser(Carte carte, Protagoniste protagoniste, SorteDirecteur sorte, PanneauAffichage menu)
        {
            List<PanneauAffichage> listPan = new();


            PanneauAffichage panneauCarte = new PanneauAffichage(carte.Hauteur, carte.Largeur, new Point(0, 0), ConsoleColor.Black);
            Ardoise ardoise = new Ardoise(carte.Hauteur, 94, new Point(panneauCarte.Largeur + 1, 0), ConsoleColor.Black);  

            listPan.Add(panneauCarte);
            listPan.Add(menu);
            listPan.Add(ardoise);
            Obstacleur obstacleur = new Obstacleur(carte, 3);
            obstacleur.Populer(); // Méthode à implémenter pour ajouter des obstacles
            Centrale centrale = new Centrale(carte, protagoniste, obstacleur.listeObstacleur, panneauCarte, ardoise);
            centrale.Populer();

            Diagnosticien diagnosticien = new Diagnosticien();
            protagoniste.Abonner(diagnosticien);


            // Créer l'écran et y placer les panneaux
            Écran ecran = new Écran(listPan);
            
            //écran.AjouterPanneau(ardoise);

            // Abonner l'écran à la carte
            carte.Abonner(ecran);

            // Associer un IDirecteur au protagoniste
            IDirecteur directeur = FabriqueDirecteur.Créer(sorte); // Méthode à implémenter pour créer un IDirecteur
            protagoniste.Associer(directeur);

            // Créer un Obstacleur et le populer avec des obstacles

            // Enfin, démarrer l'affichage de l'écran
            ecran.Démarrer();

            // Retourner les instances créées
            return (diagnosticien, ecran, obstacleur);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum raisonArrêt { poursuivre, departV, collisionMax, sortie }


    /// <summary>
    /// 
    /// </summary>
    public static class FabriquePanneau
    {
        public static PanneauAffichage Créer(Carte carte, int hauteur, int largeur, ConsoleColor fond)
        {
            int temp = largeur;
            largeur = hauteur;
            hauteur = temp;

            Point position = new Point(0, carte.Hauteur);

            return new PanneauAffichage(largeur, hauteur, position, fond);
        }
    }


    //Fabrique d'identifiant
    class DomaineIdentifiants
    {
        public static ushort Min = 0;
        public static ushort Max = 65535;

        //Une méthode de classe Formater
        internal static string Formater(ushort identifiant)
        {
            int longMax = Max.ToString().Length;
            int longId = identifiant.ToString().Length;

            int nbZeroMax = longMax - longId; //Ex: Max=100; Formater(12) => 012

            if (longId > longMax)
            {
                throw new IdentifiantInvalide("ERREUR: Le ID de l'identifiant depasse la limite d'identifiant maximale.");
            }

            return identifiant.ToString().PadLeft(longMax, '0');
        }
    }
    public interface IGénérateurId
    {
        //Un service (une méthode d’instance) nommé Prendre
        Identifiant Prendre();


        //Un autre service nommé Rendre
        void Rendre(Identifiant identifiant);

    }
    public class GénérateurAléatoire : IGénérateurId
    {
        private ushort minId;
        private ushort maxId;
        private Random aleatoire;
        private List<Identifiant> identifiantsGénérés;

        public string Prefixe { get; private set; }

        public GénérateurAléatoire(string prefixID, int germe)
        {
            Prefixe = prefixID;
            minId = DomaineIdentifiants.Min;
            maxId = DomaineIdentifiants.Max;
            identifiantsGénérés = new List<Identifiant>();

            if (germe > 0)
            {
                aleatoire = new Random(germe);
            }
            else
            {
                aleatoire = new Random();
            }
        }

        public Identifiant Prendre()
        {
            if (identifiantsGénérés.Count == maxId)
            {
                throw new BanqueVideException("ERREUR: Le nombre maximum d'identifiants a été atteint.");
            }

            string valeurIdentifiant;
            bool identifiantExisteDansLaListe;

            do
            {
                valeurIdentifiant = aleatoire.Next(minId, maxId).ToString();
                identifiantExisteDansLaListe = false;

                foreach (Identifiant identifiantListe in identifiantsGénérés)
                {
                    if (identifiantListe.Valeur == valeurIdentifiant)
                    {
                        identifiantExisteDansLaListe = true;
                        break;
                    }
                }
            }
            while (identifiantExisteDansLaListe);

            Identifiant identifiant = new Identifiant(valeurIdentifiant);
            identifiantsGénérés.Add(identifiant);
            return identifiant;
        }

        public void Rendre(Identifiant identifiant)
        {
            if (identifiantsGénérés.Contains(identifiant))
            {
                identifiantsGénérés.Remove(identifiant);
            }
        }
    }
    public enum TypeGénérateur {Aléatoire}
    public class FabriqueGénérateurs
    {
        private Dictionary<TypeGénérateur, int> statistiques;

        public FabriqueGénérateurs()
        {
            statistiques = new Dictionary<TypeGénérateur, int>();
            foreach (TypeGénérateur typeGenerateur in Enum.GetValues(typeof(TypeGénérateur)))
            {
                statistiques[typeGenerateur] = 0;
            }
        }

        public IGénérateurId Créer(TypeGénérateur type)
        {
            return Créer(type, string.Empty, 3);
        }

        public IGénérateurId Créer(TypeGénérateur type, string préfixe)
        {
            return Créer(type, préfixe, 3);
        }

        public IGénérateurId Créer(TypeGénérateur type, string préfixe, int germe = -1)
        {
            IGénérateurId générateur;
            switch (type)
            {
                case TypeGénérateur.Aléatoire:
                    générateur = new GénérateurAléatoire(préfixe, germe);
                    break;
                default:
                    throw new ArgumentException("ERREUR: Le type de générateur est non-existant.");
            }

            statistiques[type]++;
            return générateur;
        }

        public (TypeGénérateur, int)[] ObtenirStatistiques()
        {
            var tabStats = new (TypeGénérateur, int)[statistiques.Count];
            int i = 0;
            foreach (var pairesTab in statistiques)
            {
                tabStats[i] = (pairesTab.Key, pairesTab.Value);
                i++;
            }
            return tabStats;
        }
    }
    //************************


    //Exception
    internal class Exceptions
    {
        public class PositionIllégaleException : Exception
        {
            public PositionIllégaleException(string message) : base(message) { }
        }

        public class IdentifiantInvalide : Exception
        {
            public IdentifiantInvalide(string? message) : base(message) { }
        }

        public class BanqueVideException : Exception
        {
            public BanqueVideException(string? message) : base(message) { }
        }

        public class JamaisPrisException : Exception
        {
            public JamaisPrisException(string? message) : base(message) { }
        }

        public class DéjàRenduException : Exception
        {
            public DéjàRenduException(string? message) : base(message) { }
        }
    }
}
