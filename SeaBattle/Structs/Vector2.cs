namespace SeaBattle.Structs;

public struct Vector2
{
    public string StringRepresentation = "";
    public int X;
    public int Y;

    public Vector2(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public Vector2(int x, int y, string coordsInput) : this(x, y)
    {
        StringRepresentation = coordsInput;
    }

    public Vector2(string coordsInput)
    {
        SetCoords(coordsInput);
    }
    
    public void SetCoords(string coordsInput)
    {
        try
        {
            int y = coordsInput[0] - 'a';
            int x = Convert.ToInt32(coordsInput.Substring(1)) - 1;
            this = new Vector2(x, y, coordsInput);
        }
        catch (Exception)
        {
            this = new Vector2(-1, -1, "0-1");
        }
    }

    public string GetCoordsToString()
    {
        if (StringRepresentation == "")
        {
            string result = "";
            result += (char)(Y + 'a');
            result += X + 1;
            StringRepresentation = result;
        }
        return StringRepresentation;
    }
}