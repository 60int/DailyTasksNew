using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace BetegsegManager
{
    enum BetegsegTipus
    {
        Bakteriális,
        Vírusos
    }

    enum BetegsegLefolyas
    {
        Enyhe,
        Heveny,
        Veszélyes
    }

    class Betegseg
    {
        string megnevezes;
        BetegsegTipus tipus;
        BetegsegLefolyas lefolyas;

        public string Megnevezes
        {
            get => megnevezes;
            private set
            {
                if (value.Length >= 3)
                {
                    megnevezes = value;
                }
                else
                {
                    //Hiba dobása!
                    megnevezes = "HIBÁS!";
                }
            }
        }
        internal BetegsegTipus Tipus { get => tipus; /*set => tipus = value;*/ }
        internal BetegsegLefolyas Lefolyas { get => lefolyas; set => lefolyas = value; }

        public Betegseg(string megnevezes, BetegsegTipus tipus, BetegsegLefolyas lefolyas)
        {
            Megnevezes = megnevezes;
            this.tipus = tipus;
            Lefolyas = lefolyas;
        }

        public override string ToString()
        {
            return megnevezes;
        }

        public string CSVFormatum()
        {
            return megnevezes + ";" + (int)tipus + ";" + (int)lefolyas;
        }

        public static Betegseg[] Deszerializacio(string fajlnev)
        {
            string[] sorok = File.ReadAllLines(fajlnev);
            Betegseg[] betegsegek = new Betegseg[sorok.Length];
            for (int i = 0; i < sorok.Length; i++)
            {
                string[] sor = sorok[i].Split(';');
                betegsegek[i] = new Betegseg(sor[0], (BetegsegTipus)Convert.ToInt32(sor[1]), (BetegsegLefolyas)int.Parse(sor[2]));
            }
            return betegsegek;
        }

        public static void Szerializacio(string fajlnev, Betegseg[] betegsegek)
        {
            StreamWriter kiir = new StreamWriter(fajlnev, false, Encoding.UTF8);
            foreach (Betegseg item in betegsegek)
            {
                kiir.WriteLine(item.CSVFormatum());
            }
            kiir.Close();
        }
    }
}
