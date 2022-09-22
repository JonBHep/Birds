using System;

namespace Birds;

public class Sighting : IComparable<Sighting>
{
    private int _birdId;
        private int _viewDateDol;
        private bool _firstSighting;

        public int BirdId { get => _birdId; set => _birdId = value; }
        public int ViewDayOfLife { get => _viewDateDol; set => _viewDateDol = value; }
        public bool FirstSighting { get => _firstSighting; set => _firstSighting = value; }

        public DateTime ViewDate
        {
            get
            {
                return DateFromDayOfLife(_viewDateDol);
            }
            set
            {
                _viewDateDol = DayOfLife(value);
            }
        }        
        public Sighting(string code)
        {
                string f = code.Substring(0, 1);
                string b = code.Substring(1, 3);
                string d = code.Substring(4);
                _firstSighting = (f == "F");
                _birdId = int.Parse(b);
                _viewDateDol = int.Parse(d);
        }

        public Sighting(DateTime when, int birdKey, bool newBird)
        {
            _birdId = birdKey;
            _viewDateDol =DayOfLife(when);
            _firstSighting = newBird;
        }

        public string Code
        {
            get
            {
                string cd;
                if (_firstSighting) { cd = "F"; } else { cd = "R"; } // first or repeat sighting
string bd= _birdId.ToString();
                bd = bd.PadLeft(3, '0');
                cd += bd;
                cd += _viewDateDol.ToString();
                return cd;
            }
        }

        public static int DayOfLife(DateTime target)
        {
            DateTime birth = new DateTime(1954, 1, 3);
            TimeSpan ts = target - birth;
            return (int)(ts.TotalDays + 1);
        }

        public static DateTime DateFromDayOfLife(int DayOfLife)
        {
            DateTime birth = new DateTime(1954, 1, 3);
            int days = DayOfLife - 1;
            return birth.AddDays(days);
        }

        public static string AgeCaption(DateTime current)
        {
            DateTime birth = new DateTime(1954, 1, 3);
            int dys = 0;
            int yrs = 0;
            while ((current.Day != birth.Day) || (current.Month != birth.Month))
            { current = current.AddDays(-1); dys++; }
            yrs = current.Year - birth.Year;
            return yrs.ToString() + " years " + dys.ToString() + " days";
        }

        public int CompareTo(Sighting other)
        {
            return _viewDateDol.CompareTo(other._viewDateDol);
        }
}