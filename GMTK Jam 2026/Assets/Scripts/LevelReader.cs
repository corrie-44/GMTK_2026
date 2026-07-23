using UnityEngine;
using System.Collections.Generic;
using System.Linq; 
public class Level
{
    public string LevelName;
    public int Width, Height;
    public string[] Elements;

    public Level(string name, int width, int height, string[] elements)
    {
        LevelName = name;
        Width = width;
        Height = height;
        Elements = elements; 
    }
}

public static class LevelReader
{
    const int HEADER_SPAN = 3; 
    public static List<Level> GetLevels(TextAsset levelCSV)
    {
        List<Level> levels = new();

        string[] levelStrings = levelCSV.text.Split(new string[] { "LEVEL,"}, System.StringSplitOptions.RemoveEmptyEntries);
        //Debug.Log(levelStrings.Length);
        foreach(string lvlString in levelStrings)
        {
            
            string[] data = lvlString.Split(new string[] { ",", "\n"}, System.StringSplitOptions.RemoveEmptyEntries);
            List<string> dataList = data.ToList<string>();
            dataList.RemoveAll(s => string.IsNullOrWhiteSpace(s));
            //foreach (string s in dataList) Debug.Log(s);
            if( 
                int.TryParse(dataList[1], out int width) 
                && int.TryParse(dataList[2], out int height))
            {
                string[] levelElements = new string[width * height];
                for (int i = 0; i < levelElements.Length; i++)
                {
                    int index = HEADER_SPAN + i;
                    levelElements[i] = dataList[index];
                }

                Level level = new Level(dataList[0], width, height, levelElements);
                levels.Add(level);
            }
        }
        
        return levels; 
    }
}
