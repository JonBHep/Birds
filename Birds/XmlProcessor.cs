using System;
using System.Collections.Generic;

namespace Birds;

public class XmlProcessor
{
    // Declare events using EventHandler<T> 
    public event EventHandler<XmlEventArgument> RaiseAlertEvent;
    public event EventHandler<XmlEventArgument> RaisePassMessageEvent;

    // Wrap event invocations inside a protected virtual method to allow derived classes to override the event invocation behavior 
    protected virtual void OnRaiseAlertEvent(XmlEventArgument e)
        // A protected member of a base class is accessible in a derived class only if the access occurs through the derived class type.
        // The implementation of a virtual member can be changed by an overriding member in a derived class.
    {
        // Make a temporary copy of the event to avoid possibility of a race condition if the last subscriber unsubscribes immediately after the null check and before the event is raised.
        EventHandler<XmlEventArgument> handler = RaiseAlertEvent;

        // Event will be null if there are no subscribers 
        if (handler != null)
        {
            // Format the string to send inside the CustomEventArgs parameter
            e.Message += String.Format(" at {0}", DateTime.Now.ToString());

            // Use the () operator to raise the event.
            handler(this, e);
        }
    }

    protected virtual void OnRaisePassMessageEvent(XmlEventArgument e)
    {
        EventHandler<XmlEventArgument> handler = RaisePassMessageEvent;
        if (handler != null)
        {
            e.Message += String.Format(" at {0}", DateTime.Now.ToString());
            handler(this, e);
        }
    }

    private int _linesWritten;

    private string _xmlFileSpec;

    public XmlProcessor(string dataFileSpec) //constructor, providing file path
    {
        _xmlFileSpec = dataFileSpec;
    }

    /// <summary>
    /// Overloaded AddXmlItem methods
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="content"></param>
    private void AddXmlItem(string tag, string content, System.IO.StreamWriter writer)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            content = string.Empty;
        }

        tag = tag.Trim();
        content = AcceptableText(content);
        if (string.IsNullOrWhiteSpace(content))
        {
            return;
        }

        string s = "<" + tag + ">" + content.Trim() + "</" + tag + ">";
        XmlLineWrite(s, writer);
    }

    /// <summary>
    /// Overloaded AddXmlItem methods
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="content"></param>
    private void AddXmlItem(string tag, int content, System.IO.StreamWriter writer)
    {
        if (content == 0)
        {
            return;
        }

        string contents = content.ToString();
        string s = "<" + tag.Trim() + ">" + contents + "</" + tag + ">";
        XmlLineWrite(s, writer);
    }

    /// <summary>
    /// Overloaded AddXmlItem methods
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="content"></param>
    private void AddXmlItem(string tag, bool content, System.IO.StreamWriter writer)
    {
        // Removed prevention of saving 'false' values because e.g. Family.Married defaults to true
        string s = "<" + tag.Trim() + ">" + content.ToString() + "</" + tag + ">";
        XmlLineWrite(s, writer);
    }

    /// <summary>
    /// Overloaded AddXmlItem methods
    /// </summary>
    /// <param name="tag"></param>
    /// <param name="content"></param>
    private void AddXmlItem(string tag, byte content, System.IO.StreamWriter writer)
    {
        if (content == 0)
        {
            return;
        }

        string contents = content.ToString();
        string s = "<" + tag.Trim() + ">" + contents + "</" + tag + ">";
        XmlLineWrite(s, writer);
    }

    private string AcceptableText(string original)
    {
        string o;
        o = original.Replace('&', '+'); // ampersand messes up xml code
        o = o.Replace('<', '{');
        o = o.Replace('>', '}');
        o = o.Replace('\"', '\''); // double quotes replaced with single
        o = o.Replace((char) 0, (char) 32);
        o = o.Replace((char) 10, (char) 32);
        o = o.Replace((char) 13, (char) 32);
        return o.Trim();
    }

    private enum XmlLineType
    {
        Unparseable
        , Rubric
        , OpeningTag
        , ClosingTag
        , SingleLineEmptyTag
        , TagPlusContent
    }

    /// <summary>
    /// September 2018 version
    /// </summary>
    /// <param name="xmlLine">The supplied line of xml code</param>
    /// <param name="tagCaption">The name of the tag referenced in this line</param>
    /// <param name="tagValue">The content of the data in this tag (as a string)</param>
    /// <param name="validParse">Whether the line could be parsed</param>
    /// <param name="loneTag">Whether this line represents an opening or closing tag without its content</param>
    /// <param name="openingTag">Whether the lone tag is an opening (rather than a closing) tag</param>
    /// <param name="noContentTag">Whether the line represents a tag with no content e.g. <Size/></param>
    private XmlLineType ParseXmlLine(string xmlLine, out string tagCaption, out string tagValue)
    {
        int p1; // first <
        int p2; // first >
        int p3; // start of matching closing tag, if present

        tagCaption = string.Empty;
        tagValue = string.Empty;
        XmlLineType returnType = XmlLineType.Unparseable;

        xmlLine = xmlLine.Trim();
        if (xmlLine.StartsWith("<?xml version"))
        {
            returnType = XmlLineType.Rubric;
        }
        else
        {
            p1 = xmlLine.IndexOf('<');
            p2 = xmlLine.IndexOf('>');
            if ((p1 >= 0) && (p2 > p1))
            {
                tagCaption = xmlLine.Substring(p1 + 1, p2 - (p1 + 1));
                p3 = xmlLine.IndexOf("</" + tagCaption + ">", p2 + 1);
                if (p3 >= 0) // opening and closing tags and content all contained in same line
                {
                    tagValue = xmlLine.Substring(p2 + 1, p3 - (p2 + 1));
                    returnType = XmlLineType.TagPlusContent;
                }
                else
                {
                    if (xmlLine.Substring(p2 - 1, 2) ==
                        "/>") // shorthand <tag/> signifying tag with no content (has no closing tag)
                    {
                        returnType = XmlLineType.SingleLineEmptyTag;
                        tagCaption = tagCaption.Substring(0, tagCaption.Length - 1); // strip off ending slash
                        tagValue = string.Empty;
                    }
                    else
                    {
                        returnType = XmlLineType.OpeningTag;
                        if (tagCaption.StartsWith("/"))
                        {
                            returnType = XmlLineType.ClosingTag;
                        } // closing tag
                    }
                }
            }

        }

        return returnType;
    }

    private void XmlLineWrite(string linetext, System.IO.StreamWriter sw)
    {
        sw.WriteLine(linetext);
        _linesWritten++;
    }

    /// <summary>
    /// September 2018 version
    /// </summary>
    /// <param name="VerifiedLines">Number of lines examined in the xml file</param>
    /// <param name="TagResolutionError">Whether any error was detected in the nesting of tags</param>
    /// <param name="TagSequences">List of nested tags found in the xml file</param>
    private void VerifyXmlFile(out int VerifiedLines, out bool TagResolutionError, out List<string> TagSequences)
    {
        // This Sub is not application-specific as MakeXmlFile and ReadXmlFile are: it does not depend on application-specific data characteristics
        // The TagSequences list is returned for diagnositc purposes; no use is made of it in normal use
        int currentLevel = -1;
        List<string> etage = new List<string>();
        string s;

        OnRaisePassMessageEvent(new XmlEventArgument("Verifying data file write"));

        TagSequences = new List<string>();
        VerifiedLines = 0;
        TagResolutionError = false;

        using (System.IO.StreamReader xr = new System.IO.StreamReader(_xmlFileSpec))
        {
            while (!xr.EndOfStream)
            {
                s = xr.ReadLine();
                XmlLineType LineType
                    = ParseXmlLine(xmlLine: s, tagCaption: out string tag, tagValue: out string content);
                VerifiedLines++;

                switch (LineType)
                {
                    case XmlLineType.Unparseable:
                    {
                        VerifiedLines--;
                        break;
                    }
                    case XmlLineType.Rubric:
                    {
                        break;
                    }
                    case XmlLineType.OpeningTag:
                    {
                        currentLevel++;
                        if (etage.Count <= currentLevel)
                        {
                            etage.Add(tag);
                        }
                        else
                        {
                            etage[currentLevel] = tag;
                        }

                        // add to TagSequences
                        string OpeningTagPath = string.Empty;
                        for (int z = 0; z <= currentLevel; z++)
                        {
                            OpeningTagPath += "/" + etage[z];
                        }

                        OpeningTagPath = OpeningTagPath.Substring(1); // strip off the leading / slash
                        if (!TagSequences.Contains(OpeningTagPath))
                        {
                            TagSequences.Add(OpeningTagPath);
                        }

                        break;
                    }
                    case XmlLineType.ClosingTag:
                    {
                        // Record an error if the closing tag does not match the last opening tag
                        if ("/" + etage[currentLevel] != tag)
                        {
                            TagResolutionError = true;
                        }

                        currentLevel--;
                        break;
                    }
                    case XmlLineType.SingleLineEmptyTag:
                    {
                        // is one-line tag with no content e.g. <Name/>
                        // add to TagSequences
                        string OneLineTagPath = string.Empty;
                        for (int z = 0; z <= currentLevel; z++)
                        {
                            OneLineTagPath += "/" + etage[z];
                        }

                        OneLineTagPath += "/" + tag;
                        OneLineTagPath = OneLineTagPath.Substring(1); // strip off the leading / slash
                        if (!TagSequences.Contains(OneLineTagPath))
                        {
                            TagSequences.Add(OneLineTagPath);
                        }

                        break;
                    }
                    case XmlLineType.TagPlusContent:
                    {
                        // opening and closing tags on one line with content
                        // add to TagSequences
                        string OneLineTagPath = string.Empty;
                        for (int z = 0; z <= currentLevel; z++)
                        {
                            OneLineTagPath += "/" + etage[z];
                        }

                        OneLineTagPath += "/" + tag;
                        OneLineTagPath = OneLineTagPath.Substring(1); // strip off the leading / slash
                        if (!TagSequences.Contains(OneLineTagPath))
                        {
                            TagSequences.Add(OneLineTagPath);
                        }

                        break;
                    }
                    default:
                    {
                        throw new Exception("Unexpected XmlLineType value in VerifyXmlFile method");
                    }
                }
            }

            if (currentLevel != -1)
            {
                TagResolutionError = true;
            }

            TagSequences.Sort();
        }

        OnRaisePassMessageEvent(new XmlEventArgument("Xml data saved"));
    }

    /// <summary>
    /// September 2018 version
    /// </summary>
    public void WriteXmlFile()
    {
        // This Sub is not application-specific as MakeXmlFile (private) and ReadXmlFile (public) are: i.e. it does not depend on application-specific data characteristics

        MakeXmlFile();

        VerifyXmlFile(VerifiedLines: out int linesVerified, TagResolutionError: out bool verificationTabError
            , TagSequences: out List<string> TagStructure);
        if (verificationTabError)
        {
            OnRaiseAlertEvent(new XmlEventArgument(
                "The data file that has just been written could not be verified\nThere were tag resolution errors"));
        }
        else if (linesVerified != _linesWritten)
        {
            OnRaiseAlertEvent(new XmlEventArgument(
                "The data file that has just been written could not be verified\nLines written = " +
                _linesWritten.ToString() + " lines read = " + linesVerified.ToString()));
        }
    }

    private void MakeXmlFile()
    {
        _linesWritten = 0;

        using (System.IO.StreamWriter xw = new System.IO.StreamWriter(_xmlFileSpec))
        {
            // xml document prolog
            string s = "<?xml version=\"1.0\" encoding=\"ISO-8859-1\" standalone=\"yes\"?>";
            // i.e. <?xml version="1.0" encoding="ISO-8859-1" standalone="yes"?>
            // ISO-8859-1" signifies Western European latin characters
            XmlLineWrite(s, xw);

            XmlLineWrite("<BirdData>", xw);

            XmlLineWrite("<Birds>", xw);
            foreach (Bird b in Core.Instance.BirdDictionary.Values)
            {
                XmlLineWrite("<Bird>", xw);
                AddXmlItem("BOU", b.BOUCode, xw);
                AddXmlItem("Eng1923", b.OtherEnglishNamesBloc, xw);
                AddXmlItem("Eng2007", b.EnglishNameCurrent, xw);
                AddXmlItem("Family", b.FamilyKey, xw);
                AddXmlItem("French", b.French, xw);
                AddXmlItem("IntEng", b.DifferentInternationalEnglishName, xw);
                AddXmlItem("Key", b.Key, xw);
                AddXmlItem("Rare", b.Rare, xw);
                AddXmlItem("Sci1923", b.FormerScientificNamesBloc, xw);
                AddXmlItem("Sci2007", b.ScientificNameCurrent, xw);
                AddXmlItem("Seen", b.SeenEver, xw);
                AddXmlItem("Recognised", b.Recognisable, xw);
                AddXmlItem("RecognisedVoice", b.RecognisableVoice, xw);
                AddXmlItem("Common", b.SeenFrequently, xw);
                AddXmlItem("Status", b.Status, xw);
                AddXmlItem("SortIndex", b.TaxonomicSequenceIndex, xw);
                AddXmlItem("QuizBar", b.BarFromImageQuiz, xw);
                XmlLineWrite("</Bird>", xw);
            }

            XmlLineWrite("</Birds>", xw);

            XmlLineWrite("<Sightings>", xw);
            foreach (Sighting v in Core.Instance.SightingList)
            {
                AddXmlItem("Sighting", v.Code, xw);
            }

            XmlLineWrite("</Sightings>", xw);

            XmlLineWrite("<Orders>", xw);
            foreach (BirdOrder o in Core.Instance.OrderDictionary.Values)
            {
                XmlLineWrite("<Order>", xw);
                AddXmlItem("Key", o.Key, xw);
                AddXmlItem("Name", o.Name, xw);
                XmlLineWrite("</Order>", xw);
            }

            XmlLineWrite("</Orders>", xw);

            XmlLineWrite("<Families>", xw);
            foreach (BirdFamily f in Core.Instance.FamilyDictionary.Values)
            {
                XmlLineWrite("<Family>", xw);
                AddXmlItem("Key", f.Key, xw);
                AddXmlItem("Name", f.Name, xw);
                AddXmlItem("OrderKey", f.OrderKey, xw);
                XmlLineWrite("</Family>", xw);
            }

            XmlLineWrite("</Families>", xw);

            XmlLineWrite("<Genera>", xw);
            foreach (BirdGenus g in Core.Instance.GenusDictionary.Values)
            {
                XmlLineWrite("<Genus>", xw);
                AddXmlItem("Key", g.Key, xw);
                AddXmlItem("Name", g.Name, xw);
                AddXmlItem("FamilyKey", g.FamilyKey, xw);
                XmlLineWrite("</Genus>", xw);
            }

            XmlLineWrite("</Genera>", xw);

            XmlLineWrite("</BirdData>", xw);
        }

        OnRaisePassMessageEvent(new XmlEventArgument("Data file written"));
    }

    /// <summary>
    /// September 2018 version
    /// </summary>
    public void ReadXmlFile()
    {
        Bird bd = new Bird();
        BirdOrder bo = new BirdOrder();
        BirdFamily bf = new BirdFamily();
        BirdGenus bg = new BirdGenus();

        List<string> level = new List<string>();
        string s;

        List<string> ReportedUnknownTags = new List<string>(); // todo review whether this is correctly implemented

        OnRaisePassMessageEvent(new XmlEventArgument("Starting load"));

        using (System.IO.StreamReader xmlrdr = new System.IO.StreamReader(_xmlFileSpec))
        {
            while (!xmlrdr.EndOfStream)
            {
                s = xmlrdr.ReadLine();
                XmlLineType LineType
                    = ParseXmlLine(xmlLine: s, tagCaption: out string tag, tagValue: out string content);
                switch (LineType)
                {
                    case XmlLineType.Unparseable:
                    case XmlLineType.Rubric:
                    {
                        break;
                    }
                    case XmlLineType.OpeningTag:
                    {
                        level.Add(tag);
                        switch (tag)
                        {
                            case "BirdData": // level 1
                            case "Birds": // level 2
                            case "Sightings": // level 2
                            case "Orders": // level 2
                            case "Families": // level 2
                            case "Genera": // level 2
                            {
                                break;
                            }
                            case "Bird":
                            {
                                bd = new Bird();
                                break;
                            }
                            case "Order":
                            {
                                bo = new BirdOrder();
                                break;
                            }
                            case "Family":
                            {
                                bf = new BirdFamily();
                                break;
                            }
                            case "Genus":
                            {
                                bg = new BirdGenus();
                                break;
                            }
                            default:
                            {
                                string msg = $"Unknown XML tag at level {level.Count}\"{tag}\"";
                                if (!ReportedUnknownTags.Contains(msg))
                                {
                                    OnRaiseAlertEvent(new XmlEventArgument(msg));
                                    ReportedUnknownTags.Add(msg);
                                }

                                break;
                            }
                        } // end switch

                        break;
                    }
                    case XmlLineType.ClosingTag:
                    {
                        if ("/" + level[level.Count - 1] != tag) //
                        {
                            OnRaiseAlertEvent(new XmlEventArgument(
                                $"Mismatched opening and closing XML tags \"{level[level.Count - 1]}\" and \"{tag}"));
                        }

                        level.RemoveAt(level.Count - 1);
                        switch (tag)
                        {
                            case "/Bird":
                            {
                                Core.Instance.BirdDictionary.Add(bd.Key, bd);
                                break;
                            }
                            case "/Order":
                            {
                                Core.Instance.OrderDictionary.Add(bo.Key, bo);
                                break;
                            }
                            case "/Family":
                            {
                                Core.Instance.FamilyDictionary.Add(bf.Key, bf);
                                break;
                            }
                            case "/Genus":
                            {
                                Core.Instance.GenusDictionary.Add(bg.Key, bg);
                                break;
                            }
                            default:
                            {
                                break;
                            }
                        }

                        break;
                    }
                    case XmlLineType.SingleLineEmptyTag:
                    {
                        break;
                    }
                    case XmlLineType.TagPlusContent:
                    {
                        switch (level[level.Count - 1])
                        {
                            case "Bird":
                            {
                                switch (tag)
                                {
                                    case "BOU":
                                    {
                                        bd.BOUCode = content;
                                        break;
                                    }
                                    case "Eng1923":
                                    {
                                        bd.OtherEnglishNamesBloc = content;
                                        break;
                                    }
                                    case "Eng2007":
                                    {
                                        bd.EnglishNameCurrent = content;
                                        break;
                                    }
                                    case "French":
                                    {
                                        bd.French = content;
                                        break;
                                    }
                                    case "GoodImage":
                                    {
                                        break;
                                    }
                                    case "IntEng":
                                    {
                                        bd.DifferentInternationalEnglishName = content;
                                        break;
                                    }
                                    case "Key":
                                    {
                                        bd.Key = int.Parse(content);
                                        break;
                                    }
                                    case "Rare":
                                    {
                                        bd.Rare = bool.Parse(content);
                                        break;
                                    }
                                    case "Sci1923":
                                    {
                                        bd.FormerScientificNamesBloc = content;
                                        break;
                                    }
                                    case "Sci2007":
                                    {
                                        bd.ScientificNameCurrent = content;
                                        break;
                                    }
                                    case "Seen":
                                    {
                                        bd.SeenEver = bool.Parse(content);
                                        break;
                                    }
                                    case "Recognised":
                                    {
                                        bd.Recognisable = bool.Parse(content);
                                        break;
                                    }
                                    case "RecognisedVoice":
                                    {
                                        bd.RecognisableVoice = bool.Parse(content);
                                        break;
                                    }
                                    case "Common":
                                    {
                                        bd.SeenFrequently = bool.Parse(content);
                                        break;
                                    }
                                    case "SortIndex":
                                    {
                                        bd.TaxonomicSequenceIndex = int.Parse(content);
                                        break;
                                    }
                                    case "Status":
                                    {
                                        bd.Status = content;
                                        break;
                                    }
                                    case "QuizBar":
                                    {
                                        bd.BarFromImageQuiz = bool.Parse(content);
                                        break;
                                    }
                                    case "QuizAudioBar":
                                    {
                                        bd.BarFromAudioQuiz = bool.Parse(content);
                                        break;
                                    }
                                    default:
                                    {
                                        string msg
                                            = $"Unknown XML tag at level {level.Count} (\"{level[level.Count - 1]}\") \"{tag}\"";
                                        if (!ReportedUnknownTags.Contains(msg))
                                        {
                                            OnRaiseAlertEvent(new XmlEventArgument(msg));
                                            ReportedUnknownTags.Add(msg);
                                        }

                                        break;
                                    }
                                }

                                break;
                            }
                            case "Sightings":
                            {
                                switch (tag)
                                {
                                    case "Sighting":
                                    {
                                        Sighting sg = new Sighting(content);
                                        Core.Instance.SightingList.Add(sg);
                                        break;
                                    }
                                    default:
                                    {
                                        string msg
                                            = $"Unknown XML tag at level {level.Count} (\"{level[level.Count - 1]}\") \"{tag}\"";
                                        if (!ReportedUnknownTags.Contains(msg))
                                        {
                                            OnRaiseAlertEvent(new XmlEventArgument(msg));
                                            ReportedUnknownTags.Add(msg);
                                        }

                                        break;
                                    }
                                }

                                break;
                            }
                            case "Order":
                            {
                                switch (tag)
                                {
                                    case "Key":
                                    {
                                        bo.Key = int.Parse(content);
                                        break;
                                    }
                                    case "Name":
                                    {
                                        bo.Name = content;
                                        break;
                                    }
                                    default:
                                    {
                                        string msg
                                            = $"Unknown XML tag at level {level.Count} (\"{level[level.Count - 1]}\") \"{tag}\"";
                                        if (!ReportedUnknownTags.Contains(msg))
                                        {
                                            OnRaiseAlertEvent(new XmlEventArgument(msg));
                                            ReportedUnknownTags.Add(msg);
                                        }

                                        break;
                                    }
                                }

                                break;
                            }
                            case "Family":
                            {
                                switch (tag)
                                {
                                    case "Key":
                                    {
                                        bf.Key = int.Parse(content);
                                        break;
                                    }
                                    case "Name":
                                    {
                                        bf.Name = content;
                                        break;
                                    }
                                    case "OrderKey":
                                    {
                                        bf.OrderKey = int.Parse(content);
                                        break;
                                    }
                                    default:
                                    {
                                        string msg
                                            = $"Unknown XML tag at level {level.Count} (\"{level[level.Count - 1]}\") \"{tag}\"";
                                        if (!ReportedUnknownTags.Contains(msg))
                                        {
                                            OnRaiseAlertEvent(new XmlEventArgument(msg));
                                            ReportedUnknownTags.Add(msg);
                                        }

                                        break;
                                    }
                                }

                                break;
                            }
                            case "Genus":
                            {
                                switch (tag)
                                {
                                    case "Key":
                                    {
                                        bg.Key = content;
                                        break;
                                    }
                                    case "Name":
                                    {
                                        bg.Name = content;
                                        break;
                                    }
                                    case "FamilyKey":
                                    {
                                        bg.FamilyKey = int.Parse(content);
                                        break;
                                    }
                                    default:
                                    {
                                        string msg
                                            = $"Unknown XML tag at level {level.Count} (\"{level[level.Count - 1]}\") \"{tag}\"";
                                        if (!ReportedUnknownTags.Contains(msg))
                                        {
                                            OnRaiseAlertEvent(new XmlEventArgument(msg));
                                            ReportedUnknownTags.Add(msg);
                                        }

                                        break;
                                    }
                                }
                            }
                                break;
                        }

                        break;
                    }
                }

            }

            OnRaisePassMessageEvent(new XmlEventArgument("Finished loading"));
            if (level.Count != 0)
            {
                OnRaiseAlertEvent(new XmlEventArgument("Opening and closing XML tags have not been resolved"));
            }
        }
    }
}