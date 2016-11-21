# Heuristics added to AIM

## Bezier Curve Class

~~~
    #region AIMVariables
    public float size = 2.9f; 
    private int count;
    #endregion

    #region AIMMethods
    public bool incr()
    {
        float distance = this.length;
        int capacity = (int)(distance / size) + 1;
        if (count < capacity)
        {
            ++count;
            return true;
        }
        return false;
    }
    public bool decr(string name)
    {
        if (count <= 0)
            Debug.Log(name);
        Debug.Assert(count > 0);
        --count;
        return true;
    }

    public bool full()
    {
        float distance = this.length;
        int capacity = (int)(distance / size) + 1;
        if (count < capacity)
        {
            return true;
        }
        return false;
    }
   
    #endregion
~~~~
