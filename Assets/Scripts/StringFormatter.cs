using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class StringFormatter : ScriptableObject
{
    public Color Default;
    public Color Yellow;
    public Color Orange;
    public Color Red;
    public Color Magenta;
    public Color Purple;
    public Color Blue;
    public Color Cyan;
    public Color Green;
    public Color White;
    public Color Grey;
    public Color Black;
    public Color Brown;
    public Color DarkRed;
    
    private enum Attribute { Yellow, Orange, Red, Magenta, Purple, Blue, Cyan, Green, White, Grey, Black,
        Brown, DarkRed, Rotate, Shake, Shake1, Shake2, Shake3, Shake4, Remove, RemoveAll, NotFound };
    
    private readonly List<Attribute> _attributes = new List<Attribute>();
    
    private readonly char[] _delimiterChars = { ' ', ',', '.', ':', ';', '!', '?', '\n' };
    private const string Delimiters = @"([ ,:;'!?\n])|([.])(?![0-9])"; // (?<!\d).(?!\d) negative lookaround

    public string Format(string text)
    {
        var formattedText = "";
        var builder = "";
        var referenceAttribute = Attribute.NotFound;
        var words = GetWords(text);
        foreach (var word in words)
        {
            /*if (formattedText.Length != 0)
            {
                formattedText += ' ';
            }*/
            switch (word.ToLower())
            {
                case "white": case "weiß": case "weiss": case "weiße": case "weißer": case "weißes": case "weißen": case "weißem":
                    formattedText += string.Concat("<white>", word, "</>");
                    referenceAttribute = Attribute.White;
                    break;
                case "yellow": case "gelb": case "gelbe": case "gelber": case "gelbes" : case "gelben": case "gelbem":
                    formattedText += string.Concat("<yellow>", word, "</>");
                    referenceAttribute = Attribute.Yellow;
                    break;
                case "orange":
                    formattedText += string.Concat("<orange>", word, "</>");
                    referenceAttribute = Attribute.Orange;
                    break;
                case "red": case "rot": case "rote": case "roter": case "rotes": case "roten": case "rotem":
                    formattedText += string.Concat("<red>", word, "</>");
                    referenceAttribute = Attribute.Red;
                    break;
                case "magenta":
                    formattedText += string.Concat("<magenta>", word, "</>");
                    referenceAttribute = Attribute.Magenta;
                    break;
                case "purple": case "violet": case "lila": case "violett": case "violette": case "violetter": case "violettes": case "violetten": case "violettem":
                    formattedText += string.Concat("<purple>", word, "</>");
                    referenceAttribute = Attribute.Purple;
                    break;
                case "blue": case "blau": case "blaue": case "blauer": case "blaues": case "blauen": case "blauem":
                    formattedText += string.Concat("<blue>", word, "</>");
                    referenceAttribute = Attribute.Blue;
                    break;
                case "cyan":
                    formattedText += string.Concat("<cyan>", word, "</>");
                    referenceAttribute = Attribute.Cyan;
                    break;
                case "green": case "grün": case "gruen": case "grüne": case "grüner": case "grünes": case "grünen": case "grünem":
                    formattedText += string.Concat("<green>", word, "</>");
                    referenceAttribute = Attribute.Green;
                    break;
                case "grey": case "gray": case "grau": case "graue": case "grauer": case "graues": case "grauen": case "grauem":
                    formattedText += string.Concat("<grey>", word, "</>");
                    referenceAttribute = Attribute.Grey;
                    break;
                case "black": case "schwarz":
                    formattedText += string.Concat("<black>", word, "</>");
                    referenceAttribute = Attribute.Black;
                    break;
                case "brown": case "braun": case "braune": case "brauner": case "braunes": case "braunen": case "braunem":
                    formattedText += string.Concat("<brown>", word, "</>");
                    referenceAttribute = Attribute.Brown;
                    break;
                case "rainbows":
                    builder = word;
                    builder = builder.Insert(7, "<green>");
                    builder = builder.Insert(6, "<cyan>");
                    builder = builder.Insert(5, "<blue>");
                    builder = builder.Insert(4, "<purple>");
                    builder = builder.Insert(3, "<magenta>");
                    builder = builder.Insert(2, "<red>");
                    builder = builder.Insert(1, "<orange>");
                    builder = builder.Insert(0, "<yellow>");
                    formattedText += string.Concat(builder, "<*>");
                    break;
                case "cloud": case "wolke":
                    formattedText += string.Concat("<white>", word, "</>");
                    break;
                case "ground": case "earth": case "boden":
                    formattedText += string.Concat("<brown>", word, "</>");
                    break;
                case "cart": case "minecart": case "lore":
                    formattedText += string.Concat("<grey>", word, "</>");
                    break;
                case "handcar": case "train": case "draisine":
                    formattedText += string.Concat("<grey>", word, "</>");
                    break;
                case "soul": case "soulform": case "seele":
                    switch (referenceAttribute)
                    {
                        case Attribute.Red:
                            formattedText += string.Concat("<red>", word, "</>");
                            break;
                        case Attribute.Orange:
                            formattedText += string.Concat("<orange>", word, "</>");
                            break;
                        case Attribute.Yellow:
                            formattedText += string.Concat("<yellow>", word, "</>");
                            break;
                        case Attribute.Green:
                            formattedText += string.Concat("<green>", word, "</>");
                            break;
                        case Attribute.Blue:
                            formattedText += string.Concat("<blue>", word, "</>");
                            break;
                        case Attribute.Purple:
                            formattedText += string.Concat("<purple>", word, "</>");
                            break;
                        default:
                            formattedText += word;
                            break;
                    }
                    break;
                default:
                    formattedText += word;
                    break;
            }
        }
        

        return formattedText;
    }

    public List<Letter> GetLetters(string text, Alphabet.Font font)
    {
        _attributes.Clear();
        text = text.Trim();
        var letters = new List<Letter>();
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i].Equals('<'))
            {
                var attributeLength = text.Substring(i + 1).IndexOf('>');
                var attribute = text.Substring(i + 1, attributeLength);
                ParseAttribute(attribute);
                i += attributeLength + 1;
                continue;
            }

            var letter = References.Prefabs.GetLetter();
            letters.Add(letter);
            if (letter == null) continue;
            
            letter.SetActive(true);
            letter.SetCharacter(text[i], font);
            letter.SetColor(Default);
            ApplyAttributes(letter);
            
        }
        return letters;
    }

    public string[] GetWords(string text)
    {
        return Regex.Split(text, Delimiters);
        //return text.Split(_delimiterChars, StringSplitOptions.None);
    }
    
    private static int FilteredStringLength(string s)
    {
        // returns length of string without formatting tags like [red]
        var length = 0;
        var isInsideTag = false;
        foreach (var character in s)
        {
            if (character.Equals('<'))
            {
                isInsideTag = true;
            } else if (character.Equals('>'))
            {
                isInsideTag = false;
            }
            else if (!isInsideTag)
            {
                length++;
            }
        }
        return length;
    }
    
    private void ParseAttribute(string attribute)
    {
        var a = StringToAttribute(attribute);

        switch (a)
        {
            case Attribute.NotFound:
                Debug.Log("Letter attribute not found: " + attribute);
                break;
            case Attribute.Remove:
                if (_attributes.Count > 0)
                {
                    _attributes.RemoveAt(_attributes.Count - 1);
                }
                break;
            case Attribute.RemoveAll:
                _attributes.Clear();
                break;
            default:
                _attributes.Add(a);
                break;
        }
    }
    
    private void ApplyAttributes(Letter l)
    {
        foreach (var a in _attributes)
        {
            switch (a)
            {
                case Attribute.Yellow:
                    l.SetColor(Yellow);
                    break;
                case Attribute.Orange:
                    l.SetColor(Orange);
                    break;
                case Attribute.Red:
                    l.SetColor(Red);
                    break;
                case Attribute.Magenta:
                    l.SetColor(Magenta);
                    break;
                case Attribute.Purple:
                    l.SetColor(Purple);
                    break;
                case Attribute.Blue:
                    l.SetColor(Blue);
                    break;
                case Attribute.Cyan:
                    l.SetColor(Cyan);
                    break;
                case Attribute.Green:
                    l.SetColor(Green);
                    break;
                case Attribute.White:
                    l.SetColor(White);
                    break;
                case Attribute.Grey:
                    l.SetColor(Grey);
                    break;
                case Attribute.Black:
                    l.SetColor(Black);
                    break;
                case Attribute.Brown:
                    l.SetColor(Brown);
                    break;
                case Attribute.DarkRed:
                    l.SetColor(DarkRed);
                    break;
                case Attribute.Shake:
                    l.Shake();
                    break;
                case Attribute.Shake1:
                    l.Shake();
                    break;
                case Attribute.Shake2:
                    l.Shake(2);
                    break;
                case Attribute.Shake3:
                    l.Shake(3);
                    break;
                case Attribute.Shake4:
                    l.Shake(4);
                    break;
                case Attribute.Rotate:
                    l.RotateIntoPlace(0.5f);
                    break;
            }
        }
    }

    private static Attribute StringToAttribute(string s)
    {
        var attribute = s.ToLower();
        switch (attribute)
        {
            case "yellow":
                return Attribute.Yellow;
            case "orange":
                return Attribute.Orange;
            case "red":
                return Attribute.Red;
            case "magenta":
                return Attribute.Magenta;
            case "purple":
                return Attribute.Purple;
            case "blue":
                return Attribute.Blue;
            case "cyan":
                return Attribute.Cyan;
            case "green":
                return Attribute.Green;
            case "white":
                return Attribute.White;
            case "grey":
                return Attribute.Grey;
            case "black":
                return Attribute.Black;
            case "brown":
                return Attribute.Brown;
            case "darkred":
                return Attribute.DarkRed;
            case "shake":
                return Attribute.Shake;
            case "shake1":
                return Attribute.Shake1;
            case "shake2":
                return Attribute.Shake2;
            case "shake3":
                return Attribute.Shake3;
            case "shake4":
                return Attribute.Shake4;
            case "rotate":
                return Attribute.Rotate;
            case "/":
                return Attribute.Remove;
            case "*":
                return Attribute.RemoveAll;
            default:
                return Attribute.NotFound;
        }
    }
}
