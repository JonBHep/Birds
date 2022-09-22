namespace Birds;

public class XmlEventArgument
{
    // Support for event handling (for use with xmlProcessor) - this needs to be visible to both Publisher (xmlProcessor) and Subscriber (a window)
    // Defining and using events see https://msdn.microsoft.com/en-us/library/awbftdfh%28v=vs.140%29.aspx

    // Class to hold custom event info 

    public XmlEventArgument(string s) // constructor
    {
        _message = s;
    }

    private string _message;

    public string Message
    {
        get { return _message; }
        set { _message = value; }
    }
}