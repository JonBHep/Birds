using System;
using System.Collections.Generic;
using System.Linq;

namespace Birds;

public class Bird : IComparable<Bird>
{
    private string _bouCode;
    private string _englishNameCurrent;
    private string _french;
    private string _internationalEnglishName;

    public bool BarFromImageQuiz
    {
        get => _barFromImageQuiz;
        set => _barFromImageQuiz = value;
    }

    public bool BarFromAudioQuiz
    {
        get => _barFromAudioQuiz;
        set => _barFromAudioQuiz = value;
    }

    private string _scientificNameCurrent;
    private bool _seenEver;
    private bool _seenFrequently;
    private string _status;
    private int _taxonomicSequenceIndex;
    private List<string> _otherEnglishNames = new List<string>();
    private List<string> _formerScientificNames = new List<string>();
    private DateTime _lastImageQuizzed;
    private DateTime _lastAudioQuizzed;
    private bool _barFromImageQuiz;
    private bool _barFromAudioQuiz;
    private int _key;
    private bool _rare;

    public string BOUCode
    {
        get => _bouCode;
        set => _bouCode = value;
    }

    public string OtherEnglishNamesBloc
    {
        get { return string.Join("#", OtherEnglishNames.ToArray()); }
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                string[] nom = value.Split("#".ToCharArray());
                OtherEnglishNames = nom.ToList();
            }
        }
    }

    public string FormerScientificNamesBloc
    {
        get
        {
            string q = string.Join("#", FormerScientificNames.ToArray());
            return q;
        }
        set
        {
            if (!string.IsNullOrEmpty(value))
            {
                string[] nom = value.Split("#".ToCharArray());
                FormerScientificNames = nom.ToList();
            }
        }
    }

    public string EnglishNameCurrent
    {
        get => _englishNameCurrent;
        set => _englishNameCurrent = value;
    }

    public int FamilyKey
    {
        get => Core.Instance.FamilyKey(Genus);
    }

    public string French
    {
        get => _french;
        set => _french = value;
    }

    public string DifferentInternationalEnglishName
    {
        get => _internationalEnglishName;
        set => _internationalEnglishName = value;
    }

    public string UniversalEnglishName
    {
        get
        {
            string uen = _internationalEnglishName;
            if (string.IsNullOrWhiteSpace(uen))
            {
                uen = _englishNameCurrent;
            }

            return uen;
        }
    }

    public int Key
    {
        get => _key;
        set => _key = value;
    }

    public bool Rare
    {
        get => _rare;
        set => _rare = value;
    }

    public string ScientificNameCurrent
    {
        get => _scientificNameCurrent;
        set => _scientificNameCurrent = value;
    }

    public bool SeenEver
    {
        get => _seenEver;
        set => _seenEver = value;
    }

    public bool Recognisable { get; set; }
    public bool RecognisableVoice { get; set; }

    public bool SeenFrequently
    {
        get => _seenFrequently;
        set => _seenFrequently = value;
    }

    public string Status
    {
        get => _status;
        set => _status = value;
    }

    public int TaxonomicSequenceIndex
    {
        get => _taxonomicSequenceIndex;
        set => _taxonomicSequenceIndex = value;
    }

    public System.Windows.Media.Brush BirdBrush
    {
        get
        {
            System.Windows.Media.Brush brosse = System.Windows.Media.Brushes.SaddleBrown;
            if (string.IsNullOrWhiteSpace(_french))
            {
                brosse = System.Windows.Media.Brushes.DarkOrange;
            }

            if (Core.Instance.FamilyKey(Genus) < 0)
            {
                brosse = System.Windows.Media.Brushes.Red;
            }

            if (string.IsNullOrWhiteSpace(_bouCode))
            {
                brosse = System.Windows.Media.Brushes.Red;
            }

            if (string.IsNullOrWhiteSpace(_englishNameCurrent))
            {
                brosse = System.Windows.Media.Brushes.Red;
            }

            if (string.IsNullOrWhiteSpace(_scientificNameCurrent))
            {
                brosse = System.Windows.Media.Brushes.Red;
            }

            if (string.IsNullOrWhiteSpace(_status))
            {
                brosse = System.Windows.Media.Brushes.Red;
            }

            return brosse;
        }
    }

    public string BirdBrushMeaning
    {
        get
        {
            string tip = string.Empty;
            System.Windows.Media.Brush brosse = System.Windows.Media.Brushes.SaddleBrown;
            if (string.IsNullOrWhiteSpace(_french))
            {
                tip = "French name?";
            }

            if (Core.Instance.FamilyKey(Genus) < 0)
            {
                tip = "Genus not allocated to a family";
            }

            if (string.IsNullOrWhiteSpace(_bouCode))
            {
                tip = "BOU code missing";
            }

            if (string.IsNullOrWhiteSpace(_englishNameCurrent))
            {
                tip = "English name?";
            }

            if (string.IsNullOrWhiteSpace(_scientificNameCurrent))
            {
                tip = "Scientific name?";
            }

            if (string.IsNullOrWhiteSpace(_status))
            {
                tip = "Status?";
            }

            return tip;
        }
    }

    public Bird()
    {
        _bouCode = string.Empty;
        _englishNameCurrent = string.Empty;
        _french = string.Empty;
        _internationalEnglishName = string.Empty;
        Key = 0;
        _rare = false;
        _scientificNameCurrent = string.Empty;
        _seenEver = false;
        _seenFrequently = false;
        _status = string.Empty;
        _taxonomicSequenceIndex = 0;
        LastAudioQuizzed = DateTime.Today.AddDays(-1);
        LastImageQuizzed = DateTime.Today.AddDays(-1);
    }

    public Tuple<string, string> Binomial
    {
        get
        {
            string sci = _scientificNameCurrent;
            int s = sci.IndexOf(' ');
            string ge = sci.Substring(0, s);
            string sp = sci.Substring(s + 1);
            return new Tuple<string, string>(ge, sp);
        }
    }

    public void Mirror(Bird other)
    {
        _bouCode = other._bouCode;
        OtherEnglishNamesBloc = other.OtherEnglishNamesBloc;
        FormerScientificNamesBloc = other.FormerScientificNamesBloc;
        _englishNameCurrent = other._englishNameCurrent;
        _french = other._french;
        _internationalEnglishName = other._internationalEnglishName;
        Key = other.Key;
        Rare = other.Rare;
        _scientificNameCurrent = other._scientificNameCurrent;
        _seenEver = other._seenEver;
        _seenFrequently = other._seenFrequently;
        Recognisable = other.Recognisable;
        RecognisableVoice = other.RecognisableVoice;
        BarFromImageQuiz = other.BarFromImageQuiz;
        _status = other._status;
        _taxonomicSequenceIndex = other._taxonomicSequenceIndex;
    }

    public int CompareTo(Bird other)
    {
        return (_taxonomicSequenceIndex.CompareTo(other._taxonomicSequenceIndex));
    }

    public string Genus
    {
        get
        {
            string genus = _scientificNameCurrent;
            int spacePoint = genus.IndexOf((char) 32);
            if (spacePoint > 0)
            {
                genus = genus.Substring(0, spacePoint);
            }

            return genus;
        }
    }

    public List<string> OtherEnglishNames
    {
        get => _otherEnglishNames;
        set => _otherEnglishNames = value;
    }

    public List<string> FormerScientificNames
    {
        get => _formerScientificNames;
        set => _formerScientificNames = value;
    }

    public DateTime LastImageQuizzed
    {
        get => _lastImageQuizzed;
        set => _lastImageQuizzed = value;
    }

    public DateTime LastAudioQuizzed
    {
        get => _lastAudioQuizzed;
        set => _lastAudioQuizzed = value;
    }
}