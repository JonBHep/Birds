using System;
using System.Collections.Generic;
using System.Linq;

namespace Birds;

public sealed class Core
{
    // The class is implemented as a singleton so that only a single instance can be created and this instance can be accessed globally, and because the class is sealed no other class can inherit from it

    // TODO Create a picklist window to facilitate reallocating a genus to a new family, or family to a new order

    private static readonly Core _instance = new Core();

    private Core()
    {
        // the constructor is private thus preventing instances other than the single private instance from being created
    }

    public static Core
        Instance // this static property allows global access to the single private instance of this class
    {
        get { return _instance; }

    }

    private Dictionary<int, Bird> _birdDictionary = new Dictionary<int, Bird>();
    private Dictionary<int, BirdOrder> _orderDictionary = new Dictionary<int, BirdOrder>();
    private Dictionary<int, BirdFamily> _familyDictionary = new Dictionary<int, BirdFamily>();
    private Dictionary<string, BirdGenus> _genusDictionary = new Dictionary<string, BirdGenus>();

    private List<Sighting> _sightingList = new List<Sighting>();

    public Dictionary<int, Bird> BirdDictionary
    {
        get => _birdDictionary;
    }

    public Dictionary<int, BirdOrder> OrderDictionary
    {
        get => _orderDictionary;
    }

    public Dictionary<int, BirdFamily> FamilyDictionary
    {
        get => _familyDictionary;
    }

    public Dictionary<string, BirdGenus> GenusDictionary
    {
        get => _genusDictionary;
        set => _genusDictionary = value;
    }

    public int RandomBirdWithImageKey()
    {
        List<int> OldBirds = new List<int>();
        DateTime periodAgo = DateTime.Now.AddMinutes(-30);
        foreach (int k in _birdDictionary.Keys)
        {
            if (_birdDictionary[k].LastImageQuizzed < periodAgo)
            {
                OldBirds.Add(k);
            }
        }

        if (OldBirds.Count < 5)
        {
            OldBirds = _birdDictionary.Keys.ToList();
        }

        Random r = new Random();
        int max = OldBirds.Max();
        int o = -1;
        while (o < 0)
        {
            o = r.Next(0, max);
            if (_birdDictionary.ContainsKey(o))
            {
                if (_birdDictionary[o].BarFromImageQuiz)
                {
                    o = -1;
                }

                if (!OldBirds.Contains(o))
                {
                    o = -1;
                }
            }
            else
            {
                o = -1;
            }
        }

        _birdDictionary[o].LastImageQuizzed = DateTime.Now;
        return o;
    }

    public int RandomBirdWithAudioKey()
    {
        List<int> OldBirds = new List<int>();
        Dictionary<int, int> dic = BirdAudioCounts();

        DateTime periodAgo = DateTime.Now.AddMinutes(-30);
        foreach (int k in dic.Keys)
        {
            if (_birdDictionary[k].LastAudioQuizzed < periodAgo)
            {
                OldBirds.Add(k);
            }
        }

        if (OldBirds.Count < 5)
        {
            OldBirds = dic.Keys.ToList();
        }

        Random r = new Random();
        int max = OldBirds.Max();
        int o = -1;
        while (o < 0)
        {
            o = r.Next(0, max);
            if (_birdDictionary.ContainsKey(o))
            {
                if (_birdDictionary[o].BarFromAudioQuiz)
                {
                    o = -1;
                }

                if (!OldBirds.Contains(o))
                {
                    o = -1;
                }
            }
            else
            {
                o = -1;
            }
        }

        _birdDictionary[o].LastAudioQuizzed = DateTime.Now;
        return o;
    }

    public Tuple<int, int, int, int, int, int> ImageQuizNumbers()
    {
        int inc = 0, exc = 0, incR = 0, excR = 0, incF = 0, excF = 0;
        foreach (Bird b in _birdDictionary.Values)
        {
            if (b.BarFromImageQuiz)
            {
                exc++;
                if (b.Rare)
                {
                    excR++;
                }

                if (b.Recognisable)
                {
                    excF++;
                }
            }
            else
            {
                inc++;
                if (b.Rare)
                {
                    incR++;
                }

                if (b.Recognisable)
                {
                    incF++;
                }
            }
        }

        return new Tuple<int, int, int, int, int, int>(inc, incR, incF, exc, excR, excF);
    }

    public Tuple<int, int, int, int, int, int> AudioQuizNumbers()
    {
        Dictionary<int, int> dic = BirdAudioCounts();
        int inc = 0, exc = 0, incR = 0, excR = 0, incF = 0, excF = 0;
        foreach (int b in dic.Keys)
        {
            Bird waz = _birdDictionary[b];
            if (waz.BarFromAudioQuiz)
            {
                exc++;
                if (waz.Rare)
                {
                    excR++;
                }

                if (waz.RecognisableVoice)
                {
                    excF++;
                }
            }
            else
            {
                inc++;
                if (waz.Rare)
                {
                    incR++;
                }

                if (waz.RecognisableVoice)
                {
                    incF++;
                }
            }
        }

        return new Tuple<int, int, int, int, int, int>(inc, incR, incF, exc, excR, excF);
    }

    public void ResetImageQuizExclusions()
    {
        foreach (Bird b in _birdDictionary.Values)
        {
            if (b.BarFromImageQuiz)
            {
                b.BarFromImageQuiz = false;
            }
        }
    }

    public void ResetAudioQuizExclusions()
    {
        foreach (Bird b in _birdDictionary.Values)
        {
            if (b.BarFromAudioQuiz)
            {
                b.BarFromAudioQuiz = false;
            }
        }
    }

    public List<Sighting> SightingList
    {
        get => _sightingList;
    }

    public void LoadData()
    {
        string path = System.IO.Path.Combine(Jbh.AppManager.DataPath, "Aves.xml");
        XmlProcessor pump = new XmlProcessor(path);
        pump.ReadXmlFile();
        ReconcileImages();
        ReconcileNotes();
    }

    public void ReconcileImages()
    {
        string imagePath = PictureFolder;
        string[] foundImages = System.IO.Directory.GetFiles(imagePath, "*.jpg");
        foreach (string trouve in foundImages)
        {
            int oiseau = Core.BirdKeyFromPicture(trouve);
            if (!Core.Instance.BirdDictionary.ContainsKey(oiseau))
            {
                System.Windows.MessageBox.Show($"Deleting picture for unknown bird key {oiseau}");
                System.IO.File.Delete(trouve);
            }
        }
    }

    public void ReconcileNotes()
    {
        string notePath = NoteFolder;
        string[] foundNotes = System.IO.Directory.GetFiles(notePath, "*.txt");
        List<string> claimedNotes = new List<string>();

        foreach (Bird bd in Core.Instance.BirdDictionary.Values)
        {
            string np = Core.BirdTextSpecification(bd.Key);
            claimedNotes.Add(np);
        }

        // delete any notes not corresponding to a bird
        foreach (string nt in foundNotes)
        {
            if (!claimedNotes.Contains(nt))
            {
                try
                {
                    System.IO.File.Delete(nt);
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(ex.Message + "\n\n" + nt, "Delete unclaimed note"
                        , System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                }
            }
        }
    }

    public void SaveData()
    {
        Jbh.AppManager.PurgeOldBackups("xml", 20, 20);
        string path = System.IO.Path.Combine(Jbh.AppManager.DataPath, "Aves.xml");
        Jbh.AppManager.CreateBackupDataFile(path);
        XmlProcessor pump = new XmlProcessor(path);
        pump.WriteXmlFile();
    }

    public List<string> Genera()
    {
        List<string> gList = new List<string>();
        foreach (Bird b in _birdDictionary.Values)
        {
            string genus = b.Binomial.Item1;
            if (!gList.Contains(genus))
            {
                gList.Add(genus);
            }
        }

        gList.Sort();
        return gList;
    }

    public List<string> GeneraUnallocated()
    {
        List<string> gList = new List<string>();
        foreach (Bird b in _birdDictionary.Values)
        {
            string genus = b.Binomial.Item1;
            if (!_genusDictionary.ContainsKey(genus))
            {
                if (!gList.Contains(genus))
                {
                    gList.Add(genus);
                }
            }
        }

        gList.Sort();
        return gList;
    }

    public List<Tuple<string, int>> Species(string genus)
    {
        List<string> sList = new List<string>();
        foreach (Bird b in _birdDictionary.Values)
        {
            Tuple<string, string> binom = b.Binomial;
            if (binom.Item1.Equals(genus))
            {
                sList.Add(binom.Item2);
            }
        }

        sList.Sort();
        List<Tuple<string, int>> ReturnList = new List<Tuple<string, int>>();
        foreach (string sp in sList)
        {
            foreach (Bird b in _birdDictionary.Values)
            {
                Tuple<string, string> binom = b.Binomial;
                if (binom.Item1.Equals(genus))
                {
                    if (binom.Item2.Equals(sp))
                    {
                        Tuple<string, int> brd = new Tuple<string, int>(sp, b.Key);
                        ReturnList.Add(brd);
                    }
                }
            }
        }

        return ReturnList;
    }

    public List<int> BirdKeysInTaxonomicSequence()
    {
        List<Bird> oiseaux = new List<Bird>();
        foreach (Bird b in _birdDictionary.Values)
        {
            oiseaux.Add(b);
        }

        oiseaux.Sort(); // sorts by taxonomic sequence index
        List<int> tsKeys = new List<int>();
        // while producing sorted list, re-index the Birds to exclude any gaps or duplications in the sequence indices
        int idx = 0;
        foreach (Bird b in oiseaux)
        {
            idx++;
            b.TaxonomicSequenceIndex = idx;
            tsKeys.Add(b.Key);
        }

        return tsKeys;
    }

    public List<Tuple<string, int>> BirdNamesInTaxonomicSequence()
    {
        List<int> sorted = BirdKeysInTaxonomicSequence();
        List<Tuple<string, int>> names = new List<Tuple<string, int>>();
        foreach (int k in sorted)
        {
            names.Add(new Tuple<string, int>(_birdDictionary[k].EnglishNameCurrent, k));
        }

        return names;
    }

    public List<Tuple<string, int>> BirdNamesInEnglishSequence()
    {
        List<Tuple<string, int>> names = new List<Tuple<string, int>>();
        foreach (Bird b in _birdDictionary.Values)
        {
            names.Add(new Tuple<string, int>(b.EnglishNameCurrent, b.Key));
        }

        names.Sort();
        return names;
    }

    public List<int> BirdKeysInEnglishSequence()
    {
        List<Tuple<string, int>> names = BirdNamesInEnglishSequence();
        List<int> keys = new List<int>();
        foreach (Tuple<string, int> b in names)
        {
            keys.Add(b.Item2);
        }

        return keys;
    }

    public List<Tuple<string, int>> BirdNamesInFrenchSequence()
    {
        List<Tuple<string, int>> names = new List<Tuple<string, int>>();
        foreach (Bird b in _birdDictionary.Values)
        {
            names.Add(new Tuple<string, int>(b.French, b.Key));
        }

        names.Sort();
        return names;
    }

    public List<Tuple<string, int>> BirdNamesInInternationalSequence()
    {
        List<Tuple<string, int>> names = new List<Tuple<string, int>>();
        foreach (Bird b in _birdDictionary.Values)
        {
            names.Add(new Tuple<string, int>(b.UniversalEnglishName, b.Key));
        }

        names.Sort();
        return names;
    }

    public List<Tuple<string, int>> BirdNamesInEnglishTotalSequence()
    {
        List<Tuple<string, int>> names = new List<Tuple<string, int>>();
        foreach (Bird b in _birdDictionary.Values)
        {
            names.Add(new Tuple<string, int>(b.EnglishNameCurrent, b.Key));
            string alt = b.DifferentInternationalEnglishName;
            if (!string.IsNullOrEmpty(alt))
            {
                names.Add(new Tuple<string, int>(alt, b.Key));
            }

            foreach (string altuk in b.OtherEnglishNames)
            {
                names.Add(new Tuple<string, int>(altuk, b.Key));
            }
        }

        names.Sort();
        return names;
    }

    public List<Tuple<string, int>> BirdNamesInScientificSequence()
    {
        List<Tuple<string, int>> names = new List<Tuple<string, int>>();
        foreach (Bird b in _birdDictionary.Values)
        {
            names.Add(new Tuple<string, int>(b.ScientificNameCurrent, b.Key));
        }

        names.Sort();
        return names;
    }

    public List<Tuple<string, int>> BirdNamesInScientificTotalSequence()
    {
        List<Tuple<string, int>> names = new List<Tuple<string, int>>();
        foreach (Bird b in _birdDictionary.Values)
        {
            names.Add(new Tuple<string, int>(b.ScientificNameCurrent, b.Key));
            foreach (string alt in b.FormerScientificNames)
            {
                names.Add(new Tuple<string, int>(alt, b.Key));
            }
        }

        names.Sort();
        return names;
    }

    public List<int> OrderKeysInAlphabeticalOrder()
    {
        List<BirdOrder> orders = new List<BirdOrder>();
        foreach (int k in _orderDictionary.Keys)
        {
            orders.Add(_orderDictionary[k]);
        }

        orders.Sort();
        List<int> keys = new List<int>();
        foreach (BirdOrder odr in orders)
        {
            keys.Add(odr.Key);
        }

        return keys;
    }



    public static string OccurrenceRubric(string CategoryCodeString)
    {
        // British Ornithologists' Union codes
        string Cat = string.Empty;
        string SubCat = string.Empty;
        string Rb = $"STATUS: {CategoryCodeString}";
        for (int x = 0; x < CategoryCodeString.Length; x++)
        {
            string Ch = CategoryCodeString.Substring(x, 1);
            switch (Ch)
            {
                case "A":
                case "B":
                case "C":
                case "D":
                case "E":
                case "F":
                case "X":
                {
                    Rb = OccurrenceInterpretation(Rb, Cat, SubCat);
                    Cat = Ch;
                    SubCat = string.Empty;
                    break;
                }
                default:
                {
                    SubCat += Ch;
                    break;
                }
            }
        }

        Rb = OccurrenceInterpretation(Rb, Cat, SubCat);
        return Rb;
    }

    private static string OccurrenceInterpretation(string R, string Cat, string SubCat)
    {
        // British Ornithologists' Union codes
        // British List = categories A,B,C
        switch (Cat)
        {
            case "A":
            {
                R += "\n\n[A] Has been recorded in an apparently natural state at least once since 1 January 1950.";
                break;
            }
            case "B":
            {
                R += "\n\n[B] Has been recorded in an apparently natural state at least once between 1 January 1800 and 31 December 1949, but has not been recorded subsequently.";
                break;
            }
            case "C":
            {
                R += "\n\n[C] Although introduced, now derives from the resulting self-sustaining populations.";
                switch (SubCat)
                {
                    case "1":
                    {
                        R += "\n[1] Naturalized introduced species – species that has occurred only as a result of introduction.";
                        break;
                    }
                    case "2":
                    {
                        R += "\n[2] Naturalized established species - species with established populations resulting from introduction by Man, but which also occur in an apparently natural state.";
                        break;
                    }
                    case "3":
                    {
                        R += "\n[3] Naturalized re-established species - species with populations successfully re-established by Man in areas of former occurrence.";
                        break;
                    }
                    case "4":
                    {
                        R += "\n[4] Naturalized feral species - domesticated species with populations established in the wild.";
                        break;
                    }
                    case "5":
                    {
                        R += "\n[5] Vagrant naturalized species - species from established naturalized populations abroad.";
                        // currently there are no examples
                        break;
                    }
                    case "6":
                    {
                        R += "\n[6] Former naturalized species – formerly naturalized introduced species whose naturalized populations are either no longer self-sustaining or are considered extinct.";
                        break;
                    }
                }

                break;
            }
            case "D":
            {
                R += "\n\n[D] Has been recorded at least once since 1 January 1950, but there is reasonable doubt that they have ever occurred in a natural state.";
                // Category D species form no part of the British List
                break;
            }
            case "E":
            {
                R += "\n\n[E] Has been recorded as introductions, human-assisted transportees or escapees from captivity resulting in breeding populations which (if any) are thought not to be self-sustaining.";
                // Category E species form no part of the British List (unless already included within Categories A, B or C)
                if (SubCat == "*")
                {
                    R += "\n[*] Have bred in the wild in Britain.";
                }

                break;
            }
            case "F":
            {
                R += "\n\n[F] Records of bird species recorded before 1800.";
                break;
            }
            case "X":
            {
                R += "\n\n[X] Bird species not on the British List.";
                break;
            }
            case "":
            {
                break;
            }
            default:
            {
                R = $"Unknown occurrence code {Cat}";
                break;
            }
        }

        return R;
    }

    public void CreateNewOrder(string OrderName)
    {
        int k = 1;
        while (_orderDictionary.ContainsKey(k))
        {
            k++;
        }

        BirdOrder bo = new BirdOrder() {Key = k, Name = OrderName};
        _orderDictionary.Add(k, bo);
    }

    public void CreateNewFamily(string FamilyName, int OrdKey)
    {
        int k = 1;
        while (_familyDictionary.ContainsKey(k))
        {
            k++;
        }

        BirdFamily bf = new BirdFamily() {Key = k, Name = FamilyName, OrderKey = OrdKey};
        _familyDictionary.Add(k, bf);
    }

    public void CreateNewBird(Bird Details)
    {
        int k = 1;
        while (_birdDictionary.ContainsKey(k))
        {
            k++;
        }

        Bird bd = new Bird();
        bd.Mirror(Details);
        bd.Key = k;
        _birdDictionary.Add(k, bd);
    }

    public void DeleteBird(int key)
    {
        if (_birdDictionary.ContainsKey(key))
        {
            _birdDictionary.Remove(key);
        }
    }

    public bool OrderHasFamilies(int orderKey)
    {
        bool has = false;
        foreach (BirdFamily bf in _familyDictionary.Values)
        {
            if (bf.OrderKey.Equals(orderKey))
            {
                has = true;
                break;
            }
        }

        return has;
    }

    public bool FamilyHasSpecies(int familyKey)
    {
        bool has = false;
        foreach (Bird bd in _birdDictionary.Values)
        {
            if (bd.FamilyKey.Equals(familyKey))
            {
                has = true;
                break;
            }
        }

        return has;
    }

    public List<string> StatusPickList
    {
        get
        {
            List<string> stList = new List<string>();
            foreach (Bird bd in _birdDictionary.Values)
            {
                if (!string.IsNullOrWhiteSpace(bd.Status))
                {
                    if (!stList.Contains(bd.Status))
                    {
                        stList.Add(bd.Status);
                    }
                }
            }

            stList.Sort();
            return stList;
        }
    }

    private static string AudioFolder
    {
        get { return System.IO.Path.Combine(Jbh.AppManager.DataPath, "Audio"); }
    }

    private static string PictureFolder
    {
        get { return System.IO.Path.Combine(Jbh.AppManager.DataPath, "Pictures"); }
    }

    private static string NoteFolder
    {
        get { return System.IO.Path.Combine(Jbh.AppManager.DataPath, "Texts"); }
    }

    public int BirdPictureUnusedIndex(int clef)
    {
        int x = -1;
        while (true)
        {
            x++;
            bool there = System.IO.File.Exists(BirdPictureSpecification(clef, x));
            if (!there)
            {
                break;
            }
        }

        return x;
    }

    public int BirdAudioUnusedIndex(int clef)
    {
        int x = -1;
        while (true)
        {
            x++;
            bool there = System.IO.File.Exists(BirdAudioSpecification(clef, x));
            if (!there)
            {
                break;
            }
        }

        return x;
    }

    public static string BirdPictureSpecification(int clef, int index)
    {
        return System.IO.Path.Combine(PictureFolder, $"{EightHexString(clef)}-{ThreeHexString(index)}.jpg");
    }

    public static string BirdAudioSpecification(int clef, int index)
    {
        return System.IO.Path.Combine(AudioFolder, $"{EightHexString(clef)}-{ThreeHexString(index)}.mp3");
    }

    public string RandomBirdPicture(int clef)
    {
        string pattern = $"{EightHexString(clef)}-???.jpg";
        string[] examples = System.IO.Directory.GetFiles(PictureFolder, pattern);
        string returner = string.Empty;
        switch (examples.Length)
        {
            case 0:
            {
                break;
            }
            case 1:
            {
                returner = examples[0];
                break;
            }
            default:
            {
                Random r = new Random();
                int choisi = r.Next(0, examples.Length);
                returner = examples[choisi];
                break;
            }
        }

        return returner;
    }

    public string RandomBirdAudioClip(int clef)
    {
        string pattern = $"{EightHexString(clef)}-???.mp3";
        string[] examples = System.IO.Directory.GetFiles(AudioFolder, pattern);
        string returner = string.Empty;
        switch (examples.Length)
        {
            case 0:
            {
                break;
            }
            case 1:
            {
                returner = examples[0];
                break;
            }
            default:
            {
                Random r = new Random();
                int choisi = r.Next(0, examples.Length);
                returner = examples[choisi];
                break;
            }
        }

        return returner;
    }

    public Dictionary<int, int> BirdPictureCounts()
    {
        Dictionary<int, int> dic = new Dictionary<int, int>();
        string pattern = "????????-???.jpg";
        string[] examples = System.IO.Directory.GetFiles(PictureFolder, pattern);
        foreach (string spec in examples)
        {
            int bk = BirdKeyFromPicture(spec);
            if (dic.ContainsKey(bk))
            {
                dic[bk]++;
            }
            else
            {
                dic.Add(bk, 1);
            }
        }

        return dic;
    }

    public Dictionary<int, int> BirdAudioCounts()
    {
        Dictionary<int, int> dic = new Dictionary<int, int>();
        string pattern = "????????-???.mp3";
        string[] examples = System.IO.Directory.GetFiles(AudioFolder, pattern);
        foreach (string spec in examples)
        {
            int bk = BirdKeyFromAudio(spec);
            if (dic.ContainsKey(bk))
            {
                dic[bk]++;
            }
            else
            {
                dic.Add(bk, 1);
            }
        }

        return dic;
    }

    public static List<string> BirdPictureFilePaths(int avian)
    {
        string pattern = EightHexString(avian) + "-???.jpg";
        return System.IO.Directory.GetFiles(PictureFolder, pattern).ToList();
    }

    public static int BirdKeyFromPicture(string pic)
    {
        string file = System.IO.Path.GetFileName(pic);
        string eight = file.Substring(0, 8);
        return int.Parse(eight, System.Globalization.NumberStyles.HexNumber
            , System.Globalization.CultureInfo.InvariantCulture);
    }

    public static Dictionary<int, int> ImageCounts()
    {
        Dictionary<int, int> dic = new Dictionary<int, int>();
        string pattern = "????????-???.jpg";
        string[] files = System.IO.Directory.GetFiles(PictureFolder, pattern);
        foreach (string f in files)
        {
            int b = BirdKeyFromPicture(f);
            if (dic.ContainsKey(b))
            {
                dic[b]++;
            }
            else
            {
                dic.Add(b, 1);
            }
        }

        return dic;
    }

    public static Dictionary<int, int> SoundCounts()
    {
        Dictionary<int, int> dic = new Dictionary<int, int>();
        string pattern = "????????-???.mp3";
        string[] files = System.IO.Directory.GetFiles(AudioFolder, pattern);
        foreach (string f in files)
        {
            int b = BirdKeyFromAudio(f);
            if (dic.ContainsKey(b))
            {
                dic[b]++;
            }
            else
            {
                dic.Add(b, 1);
            }
        }

        return dic;
    }

    public static List<string> BirdAudioFilePaths(int avian)
    {
        string pattern = EightHexString(avian) + "-???.mp3";
        return System.IO.Directory.GetFiles(AudioFolder, pattern).ToList();
    }

    public static int BirdKeyFromAudio(string pic)
    {
        string file = System.IO.Path.GetFileName(pic);
        string eight = file.Substring(0, 8);
        return int.Parse(eight, System.Globalization.NumberStyles.HexNumber
            , System.Globalization.CultureInfo.InvariantCulture);
    }

    public static string BirdTextSpecification(int bKey)
    {
        string notepath = NoteFolder;
        string notename = EightHexString(bKey) + ".txt";
        return System.IO.Path.Combine(notepath, notename);
    }

    private static string EightHexString(int z)
    {
        string partial = z.ToString("X"); // hexadecimal
        string padded = partial.PadLeft(8, '0');
        return padded;
    }

    private static string ThreeHexString(int z)
    {
        string partial = z.ToString("X"); // hexadecimal
        string padded = partial.PadLeft(3, '0');
        return padded;
    }

    public int FamilyKey(string genus)
    {
        if (!GenusDictionary.ContainsKey(genus))
        {
            return -1;
        }

        BirdGenus gnus = GenusDictionary[genus];
        return gnus.FamilyKey;
    }
}