using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager instance;
    public RectTransform characterPanel;

    public List<Character> characters = new List<Character>();
    public Dictionary<string, int> characterDictionary = new Dictionary<string, int>();

    void Awake()
    {
        instance = this;
    }

    public Character GetCharacter(string characterName, bool CreateCharacterIfNotExist = true ,bool enableCreateCharacterOnStart = true  )
    {
        int index = -1;
        if (characterDictionary.TryGetValue (characterName , out index))
        {
            return characters[index];
        }
        else if(CreateCharacterIfNotExist )
        {
            return createCharacter(characterName, enableCreateCharacterOnStart);
        }
        return null;
    }

    public Character createCharacter(string characterName, bool enableOnStart = true )
    {
        Character newCharacter = new Character(characterName, enableOnStart );
        characterDictionary.Add(characterName, characters.Count);
        characters.Add(newCharacter );

        return newCharacter;
    }

    public class CharacterPosition
    {
        public Vector2 TopLeft = new Vector2(0f, 1f);
        public Vector2 BottomLeft = new Vector2(0f, 0f);
        public Vector2 Center = new Vector2(0.5f, 0.5f);
        public Vector2 TopRight = new Vector2(1f, 1f);
        public Vector2 BottomRight = new Vector2(1f, 0f);
    }
    public CharacterPosition characterPosition = new CharacterPosition();

}
