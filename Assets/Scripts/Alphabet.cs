using UnityEngine;

[System.Serializable]
public class Alphabet : ScriptableObject
{
    public enum Font
    {
        Dos
    }

    public enum Alignment
    {
        Left,
        Right,
        Center
    }

    public enum VerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    [Header("Dos Font")]
    public Sprite[] lowercaseLetters = new Sprite[26];
    public Sprite[] capitalLetters = new Sprite[26];
    public Sprite[] specialCharacters = new Sprite[10];
    public Sprite[] numbers = new Sprite[10];
    public Sprite[] punctuations = new Sprite[28];

    [System.Serializable]
    public class LetterDictionary : SerializableDictionary<char, Sprite> { }
    public LetterDictionary letters = new LetterDictionary();
    public bool hasBeenInitialized;
    private LetterDictionary _currentDictionary;

    readonly char[] _lowercaseChars = {   'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
                                'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'};

    readonly char[] _specialChars = {     'ä', 'ö', 'ü', 'Ä', 'Ö', 'Ü', 'ß', ' ', '♥', '∂'};
    readonly char[] _numberChars = {      '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
    readonly char[] _punctuationChars = { '.', ':', ',', ';', '!', '?', '-', '_', '\'', '@', '+', '#', '/', '%', '(', ')', '$', '&', '=', '"', '|', '<', '>', '*', '[', ']', '{', '}', '\\', '„', '“', '\t', '∆'};
    // '√', '“', '”','^', '´'
    
    [SerializeField]
    private int characterCount;

    public Sprite GetSprite (char character, Font font)
    {
        switch (font)
        {
            case Font.Dos:
                _currentDictionary = letters;
                break;
            default:
                _currentDictionary = letters;
                break;
        }
        if (_currentDictionary.ContainsKey(character))
        {
            return _currentDictionary[character];
        }
        return _currentDictionary['∆'];
    }

    public Vector2 GetSizeOf(char character, Font font)
    {
        return GetSprite(character, font).rect.size;
    }

    public void InitializeDictionary ()
    {
        letters.Clear();
        
        for (var i = 0; i < lowercaseLetters.Length; i++)
        {
            letters.Add(_lowercaseChars[i], lowercaseLetters[i]);
            letters.Add(char.ToUpper(_lowercaseChars[i]), capitalLetters[i]);
        }
        for (var i = 0; i < specialCharacters.Length; i++)
        {
            letters.Add(_specialChars[i], specialCharacters[i]);
        }
        for (var i = 0; i < numbers.Length; i++)
        {
            letters.Add(_numberChars[i], numbers[i]);
        }
        for (var i = 0; i < punctuations.Length; i++)
        {
            letters.Add(_punctuationChars[i], punctuations[i]);
        }
        characterCount = letters.Count;
        hasBeenInitialized = true;
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}
